using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Permissions;
using System.Web;
using System.Web.Caching;
using Talifun.Web.StaticFile.Config;

namespace Talifun.Web.StaticFile
{
    public static class StaticFileHelper
    {
        private const int BufferSize = 32768;
        private const long MAX_FILE_SIZE_TO_SERVE = int.MaxValue;

        internal const string HTTP_METHOD_GET = "GET";
        internal const string HTTP_METHOD_HEAD = "HEAD";

        internal const uint ERROR_THE_REMOTE_HOST_CLOSED_THE_CONNECTION = 0x80072746; //WSAECONNRESET (10054)

        private static IRetryableFileOpener _retryableFileOpener = new RetryableFileOpener();
        private static IMimeTyper _mimeTyper = new MimeTyper();
        private static IHasher _hasher = new Hasher(_retryableFileOpener);
        private static IHttpRequestHeaderHelper _httpRequestHeaderHelper = new HttpRequestHeaderHelper();
        private static IHttpResponseHeaderHelper _httpResponseHeaderHelper = null;
        
        internal static WebServerType WebServerType { get; set; }
        internal static Dictionary<string, FileExtensionMatch> fileExtensionMatches { get; private set; }
        internal static FileExtensionMatch fileExtensionMatchDefault { get; private set; }

        internal static string staticFileHandlerType = typeof(StaticFileHelper).ToString();

        static StaticFileHelper()
        {
            WebServerType = CurrentStaticFileHandlerConfiguration.Current.WebServerType;
            fileExtensionMatches = GetFileExtensionsForMatches();
            fileExtensionMatchDefault = GetDefaultFileExtensionForNoMatches();
        }

        private static Dictionary<string, FileExtensionMatch> GetFileExtensionsForMatches()
        {
            var fileExtensionMatches = new Dictionary<string, FileExtensionMatch>();

            var fileExtensionElements = CurrentStaticFileHandlerConfiguration.Current.FileExtensions;
            foreach (FileExtensionElement fileExtension in fileExtensionElements)
            {
                var extensions = fileExtension.Extension.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var extension in extensions)
                {
                    var key = extension.Trim().ToLower();
                    if (!key.StartsWith("."))
                    {
                        key = "." + key;
                    }

                    var fileExtensionElement = new FileExtensionMatch
                    {
                        Compress = fileExtension.Compress,
                        Extension = fileExtension.Extension,
                        MaxMemorySize = fileExtension.MaxMemorySize,
                        ServeFromMemory = fileExtension.ServeFromMemory,
                        EtagMethod = fileExtension.EtagMethod,
                        Expires = fileExtension.Expires,
                        MemorySlidingExpiration = fileExtension.MemorySlidingExpiration
                    };

                    fileExtensionMatches.Add(key, fileExtensionElement);
                }
            }

            return fileExtensionMatches;
        }

        static FileExtensionMatch GetDefaultFileExtensionForNoMatches()
        {
            var fileExtensionElementDefault = CurrentStaticFileHandlerConfiguration.Current.FileExtensionDefault;

            return new FileExtensionMatch
            {
                Compress = fileExtensionElementDefault.Compress,
                Extension = string.Empty,
                MaxMemorySize = fileExtensionElementDefault.MaxMemorySize,
                ServeFromMemory = fileExtensionElementDefault.ServeFromMemory,
                EtagMethod = fileExtensionElementDefault.EtagMethod,
                Expires = fileExtensionElementDefault.Expires,
                MemorySlidingExpiration = fileExtensionElementDefault.MemorySlidingExpiration
            };
        }

        /// <summary>
        /// Return the http worker request for the current request.
        /// </summary>
        /// <remarks>
        /// This is needed for when we manually create an HttpContext.
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [ReflectionPermission(SecurityAction.Assert, RestrictedMemberAccess = true)]
        public static HttpWorkerRequest GetWorkerRequestViaReflection(HttpRequestBase request)
        {
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

            // In Mono, the field has a different name.
            var wrField = request.GetType().GetField("_wr", bindingFlags) ?? request.GetType().GetField("worker_request", bindingFlags);

            if (wrField == null) return null;

            return (HttpWorkerRequest)wrField.GetValue(request);
        }

        /// <summary>
        /// Detect the web server being used to serve requests
        /// </summary>
        /// <param name="context">Http context</param>
        [SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        public static void DetectWebServerType(HttpContextBase context)
        {
            var provider = (IServiceProvider)context;
            var worker = (HttpWorkerRequest)provider.GetService(typeof(HttpWorkerRequest)) ?? GetWorkerRequestViaReflection(context.Request);

            if (worker != null)
            {
                var workerType = worker.GetType();
                if (workerType != null)
                {
                    switch (workerType.FullName)
                    {
                        case "System.Web.Hosting.ISAPIWorkerRequest":
                            //IIS 7 in Classic mode gets lumped in here too
                            WebServerType = WebServerType.IIS6orIIS7ClassicMode;
                            break;
                        case "Microsoft.VisualStudio.WebHost.Request":
                        case "Cassini.Request":
                            WebServerType = WebServerType.Cassini;
                            break;
                        case "System.Web.Hosting.IIS7WorkerRequest":
                            WebServerType = WebServerType.IIS7;
                            break;
                        default:
                            WebServerType = WebServerType.Unknown;
                            break;
                    }

                    return;
                }
            }

            WebServerType = WebServerType.Unknown;
        }

        public static void ProcessRequest(HttpContextBase context)
        {
            var physicalFilePath = context.Request.PhysicalPath;
            var file = new FileInfo(physicalFilePath);

            ProcessRequest(context, file);
        }

        public static void ProcessRequest(HttpContextBase context, FileInfo file)
        {
            var request = context.Request;
            var response = context.Response;

            if (WebServerType == WebServerType.NotSet)
            {
                DetectWebServerType(context);
            }

            if (_httpResponseHeaderHelper == null)
            {
                _httpResponseHeaderHelper = new HttpResponseHeaderHelper(WebServerType);
            }

            //We don't want to use up all the servers memory keeping a copy of the file, we just want to stream file to client
            response.BufferOutput = false;
      
            try
            {
                if (!ValidateHttpMethod(request))
                {
                    //If we are unable to parse url send 405 Method not allowed
                    _httpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatus.MethodNotAllowed);
                    return;
                }

                if (file.FullName.EndsWith(".asp", StringComparison.InvariantCultureIgnoreCase) ||
                    file.FullName.EndsWith(".aspx", StringComparison.InvariantCultureIgnoreCase))
                {
                    //If we are unable to parse url send 403 Path Forbidden
                    _httpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatus.Forbidden);
                    return;
                }

                var requestHttpMethod = _httpRequestHeaderHelper.GetHttpMethod(request);

                var compressionType = _httpRequestHeaderHelper.GetCompressionMode(request);

                FileExtensionMatch fileExtensionMatch = null;
                if (!fileExtensionMatches.TryGetValue(file.Extension.ToLower(), out fileExtensionMatch))
                {
                    fileExtensionMatch = fileExtensionMatchDefault;
                }

                // If this is a binary file like image, then we won't compress it.
                if (!fileExtensionMatch.Compress)
                    compressionType = ResponseCompressionType.None;

                // If it is a partial request we need to get bytes of orginal entity data, we will compress the byte ranges returned
                var entityStoredWithCompressionType = compressionType;
                var isRangeRequest = _httpRequestHeaderHelper.IsRangeRequest(request);
                if (isRangeRequest)
                {
                    entityStoredWithCompressionType = ResponseCompressionType.None;
                }

                FileHandlerCacheItem fileHandlerCacheItem = null;

                if (!TryGetFileHandlerCacheItem(fileExtensionMatch, file, entityStoredWithCompressionType, out fileHandlerCacheItem))
                {
                    //File does not exist
                    if (!file.Exists)
                    {
                        _httpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatus.NotFound);
                        return;
                    }

                    //File too large to send
                    if (file.Length > MAX_FILE_SIZE_TO_SERVE)
                    {
                        _httpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatus.RequestEntityTooLarge);
                        return;
                    }
                }

                if (fileHandlerCacheItem.EntityData == null && !file.Exists)
                {
                    //If we have cached the properties of the file but its to large to serve from memory then we must check that the file exists each time.
                    _httpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatus.NotFound);
                    return;
                }

                //Unable to parse request range header
                IEnumerable<RangeItem> ranges = null;
                var requestRange = _httpRequestHeaderHelper.GetRanges(request, fileHandlerCacheItem.ContentLength, out ranges);
                if (requestRange.HasValue && !requestRange.Value)
                {
                    _httpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatus.RequestedRangeNotSatisfiable);
                    return;
                }

                //Check if cached response is valid and if it is send appropriate response headers
                var httpStatusCode = GetResponseHttpStatusCode(request, response, fileHandlerCacheItem.LastModified,
                                                             fileHandlerCacheItem.Etag);

                switch (httpStatusCode)
                {
                    case HttpStatusCode.NotModified:
                        //Browser cache is ok so, just load from cache
                        _httpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatus.NotModified);
                        return;
                    case HttpStatusCode.PreconditionFailed:
                        _httpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatus.PreconditionFailed);
                        return;
                    case HttpStatusCode.PartialContent:
                        _httpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatus.PartialContent);
                        break;
                    case HttpStatusCode.OK:
                        _httpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatus.OK);
                        break;
                    default:
                        //Unhandled status code
                        break;
                }

                //Tell the client it supports resumable requests
                _httpResponseHeaderHelper.SetResponseResumable(response);

                //How the entity should be cached on the client
                _httpResponseHeaderHelper.SetResponseCachable(response, DateTime.Now, fileHandlerCacheItem.LastModified, fileHandlerCacheItem.Etag, fileExtensionMatch.Expires);

                ServeContent(response, file, fileHandlerCacheItem, requestHttpMethod, compressionType, ranges);
            }
            catch (HttpException httpException)
            {
                //Client disconnected half way through us sending data
                if (httpException.ErrorCode != ERROR_THE_REMOTE_HOST_CLOSED_THE_CONNECTION)
                    return;

                throw;
            }
        }

        /// <summary>
        /// Serve content for request.
        /// </summary>
        /// <param name="response">An HTTP response.</param>
        /// <param name="file">File to send.</param>
        /// <param name="fileHandlerCacheItem">An item </param>
        /// <param name="requestHttpMethod">The http method for the HTTP request.</param>
        /// <param name="compressionType">The compression type to use when sending content.</param>
        /// <param name="ranges">The byte ranges to serve.</param>
        public static void ServeContent(HttpResponseBase response, FileInfo file, FileHandlerCacheItem fileHandlerCacheItem, HttpMethod requestHttpMethod, ResponseCompressionType compressionType, IEnumerable<RangeItem> ranges)
        {
            IEntityResponse entityResponseForEntity;
            if (response.StatusCode == (int) HttpStatusCode.PartialContent)
            {            
                if (ranges.Count() == 1)
                {
                    //Single byte range request, send a partial response
                    entityResponseForEntity = new SinglePartEntityResponse(_httpResponseHeaderHelper, ranges.First());
                }
                else
                {
                    //Multi byte range request, send a partial response
                    entityResponseForEntity = new MultiPartEntityResponse(_httpResponseHeaderHelper, ranges);
                }
            }
            else
            {
                //Send a full response
                entityResponseForEntity = new FullEntityResponse(_httpResponseHeaderHelper);
            }
            entityResponseForEntity.SendHeaders(response, compressionType, fileHandlerCacheItem);

            ITransmitEntityStrategy transmitEntity;   
            if (fileHandlerCacheItem.EntityData == null)
            {
                //Let IIS send file content with TransmitFile
                transmitEntity = new TransmitEntityStrategyForIIS(fileHandlerCacheItem, file.FullName);
            }
            else
            {
                //We will serve the in memory file
                transmitEntity = new TransmitEntityStrategyForByteArray(fileHandlerCacheItem, fileHandlerCacheItem.EntityData);
            }
            entityResponseForEntity.SendBody(requestHttpMethod, response, transmitEntity);
        }

        /// <summary>
        /// Get a fileHanderCacheItem for the requested file.
        /// </summary>
        /// <param name="fileExtensionMatch">The configuration to use for this extension.</param>
        /// <param name="file">The file to serve.</param>
        /// <param name="entityStoredWithCompressionType">The compression type to use for the file.</param>
        /// <param name="fileHandlerCacheItem">The fileHandlerCacheItem </param>
        /// <returns>Returns true if a fileHandlerCacheItem can be created; otherwise false.</returns>
        internal static bool TryGetFileHandlerCacheItem(FileExtensionMatch fileExtensionMatch, FileInfo file, ResponseCompressionType entityStoredWithCompressionType, out FileHandlerCacheItem fileHandlerCacheItem)
        {
            fileHandlerCacheItem = null;

            // If the response bytes are already cached, then deliver the bytes directly from cache
            var cacheKey = staticFileHandlerType + ":" + entityStoredWithCompressionType + ":" + file.FullName;

            var cachedValue = HttpRuntime.Cache.Get(cacheKey);
            if (cachedValue != null)
            {
                fileHandlerCacheItem = (FileHandlerCacheItem)cachedValue;
            }
            else
            {
                //File does not exist
                if (!file.Exists)
                {
                    return false;
                }

                //File too large to send
                if (file.Length > MAX_FILE_SIZE_TO_SERVE)
                {
                    return false;
                }

                var etag = string.Empty;
                var lastModifiedFileTime = file.LastWriteTime.ToUniversalTime();
                //When a browser sets the If-Modified-Since field to 13-1-2010 10:30:58, another DateTime instance is created, but this one has a Ticks value of 633989754580000000
                //But the time from the file system is accurate to a tick. So it might be 633989754586086250.
                var lastModified = new DateTime(lastModifiedFileTime.Year, lastModifiedFileTime.Month,
                                                lastModifiedFileTime.Day, lastModifiedFileTime.Hour,
                                                lastModifiedFileTime.Minute, lastModifiedFileTime.Second);
                var contentType = _mimeTyper.GetMimeType(file.Extension);
                var contentLength = file.Length;

                //ETAG is always calculated from uncompressed entity data
                switch (fileExtensionMatch.EtagMethod)
                {
                    case EtagMethodType.MD5:
                        etag = _hasher.CalculateMd5Etag(file);
                        break;
                    case EtagMethodType.LastModified:
                        etag = lastModified.ToString();
                        break;
                    default:
                        throw new Exception("Unknown etag method generation");
                }

                fileHandlerCacheItem = new FileHandlerCacheItem
                {
                    Etag = etag,
                    LastModified = lastModified,
                    ContentLength = contentLength,
                    ContentType = contentType,
                    CompressionType = entityStoredWithCompressionType
                };

                if (fileExtensionMatch.ServeFromMemory
                    && (contentLength <= fileExtensionMatch.MaxMemorySize))
                {
                    // When not compressed, buffer is the size of the file but when compressed, 
                    // initial buffer size is one third of the file size. Assuming, compression 
                    // will give us less than 1/3rd of the size
                    using (var memoryStream = new MemoryStream(
                        entityStoredWithCompressionType == ResponseCompressionType.None
                            ?
                                Convert.ToInt32(file.Length)
                            :
                                Convert.ToInt32((double)file.Length / 3)))
                    {
                        ReadEntityData(entityStoredWithCompressionType, file, memoryStream);
                        var entityData = memoryStream.ToArray();
                        var entityDataLength = entityData.LongLength;

                        fileHandlerCacheItem.EntityData = entityData;
                        fileHandlerCacheItem.ContentLength = entityDataLength;
                    }
                }

                //Put fileHandlerCacheItem into cache with 30 min sliding expiration, also if file changes then remove fileHandlerCacheItem from cache
                HttpRuntime.Cache.Insert(
                    cacheKey,
                    fileHandlerCacheItem,
                    new CacheDependency(file.FullName),
                    Cache.NoAbsoluteExpiration,
                    fileExtensionMatch.MemorySlidingExpiration,
                    CacheItemPriority.BelowNormal,
                    null);
            }

            return true;
        }

        /// <summary>
        /// Read a files contents into a stream
        /// </summary>
        /// <param name="compressionType">The compression type to use.</param>
        /// <param name="file">The file to read from.</param>
        /// <param name="stream">The stream to write to.</param>
        private static void ReadEntityData(ResponseCompressionType compressionType, FileInfo file, Stream stream)
        {
            using (var outputStream = (compressionType == ResponseCompressionType.None ? stream : (compressionType == ResponseCompressionType.GZip ? (Stream)new GZipStream(stream, CompressionMode.Compress, true) : (Stream)new DeflateStream(stream, CompressionMode.Compress))))
            {
                // We can compress and cache this file
                using (var fs = _retryableFileOpener.OpenFileStream(file, 5, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var bufferSize = Convert.ToInt32(Math.Min(file.Length, BufferSize));
                    var buffer = new byte[bufferSize];

                    int bytesRead;
                    while ((bytesRead = fs.Read(buffer, 0, bufferSize)) > 0)
                    {
                        outputStream.Write(buffer, 0, bytesRead);
                    }
                }

                outputStream.Flush();
            }
        }

        /// <summary>
        /// Determine whether the http method is supported. Currently we only support get and head methods.
        /// </summary>
        /// <param name="request">An HTTP request.</param>
        /// <returns>True if http method is supported; false if it is not</returns>
        internal static bool ValidateHttpMethod(HttpRequestBase request)
        {
            return (request.HttpMethod == HTTP_METHOD_GET || request.HttpMethod == HTTP_METHOD_HEAD);
        }

        /// <summary>
        /// Process the http request to calculate its http response code.
        /// </summary>
        /// <param name="request">An HTTP request.</param>
        /// <param name="response">An HTTP response.</param>
        /// <param name="lastModified">The last modified date of the entity.</param>
        /// <param name="etag">The etag of the entity.</param>
        /// <returns>
        /// Returns HttpStatusCode for Http request.
        /// </returns>
        /// <remarks>
        /// When the browser has a satisfiable cached response, the appropriate header is also set
        /// so there is no need to continue the processing of the entity.
        /// </remarks>
        internal static HttpStatusCode GetResponseHttpStatusCode(HttpRequestBase request, HttpResponseBase response, DateTime lastModified, string etag)
        {
            lastModified = lastModified.ToUniversalTime();

            //Always assume we going to send whole entity
            var responseCode = HttpStatusCode.OK;

            if (_httpRequestHeaderHelper.IsRangeRequest(request))
            {
                //It is a partial request
                responseCode = HttpStatusCode.PartialContent;
            }

            bool? ifNoneMatch = null;
            bool? ifMatch = null;

            if ((((int)responseCode >= 200 && (int)responseCode <= 299 || (int)responseCode == 304)))
            {
                //If there no matches then we do not want a cached response
                ifNoneMatch = _httpRequestHeaderHelper.CheckIfNoneMatch(request, etag, true);
                if (ifNoneMatch.HasValue)
                {
                    if (ifNoneMatch.Value && responseCode == HttpStatusCode.NotModified)
                    {
                        responseCode = HttpStatusCode.OK;
                    }
                    else
                    {
                        //If the request would, without the If-None-Match header field, result in 
                        //anything other than a 2xx or 304 status, then the If-None-Match header MUST be ignored.
                        responseCode = HttpStatusCode.NotModified;
                    }
                }
            }

            if ((((int)responseCode >= 200 && (int)responseCode <= 299 || (int)responseCode == 304)))
            {
                ifMatch = _httpRequestHeaderHelper.CheckIfMatch(request, etag, true);
                if (ifMatch.HasValue && !ifMatch.Value)
                {
                    //If none of the entity tags match, or if "*" is given and no current 
                    //entity exists, the server MUST NOT perform the requested method, and 
                    //MUST return a 412 (Precondition Failed) response

                    //If the request would, without the If-Match header field, result in 
                    //anything other than a 2xx or 412 status, then the If-Match header MUST be ignored.
                    responseCode = HttpStatusCode.PreconditionFailed;
                }
            }

            if (
                !(ifNoneMatch.HasValue && ifNoneMatch.Value) ||
                !(ifMatch.HasValue && !ifMatch.Value))
            {
                //Only use weakly typed etags headers if strong ones are valid

                bool? unlessModifiedSince = null;
                bool? ifUnmodifiedSince = null;
                bool? ifModifiedSince = null;

                if ((((int)responseCode >= 200 && (int)responseCode <= 299 || (int)responseCode == 304)))
                {
                    unlessModifiedSince = _httpRequestHeaderHelper.CheckUnlessModifiedSince(request, lastModified);
                    if (unlessModifiedSince.HasValue && !unlessModifiedSince.Value)
                    {
                        //If the requested variant has been modified since the specified time, 
                        //the server MUST NOT perform the requested operation, and MUST return 
                        //a 412 (Precondition Failed). Otherwise header is ignored.

                        //If the request normally (i.e., without the If-Unmodified-Since header) 
                        //would result in anything other than a 2xx or 412 status, 
                        //the If-Unmodified-Since header SHOULD be ignored.
                        responseCode = HttpStatusCode.PreconditionFailed;
                    }
                }

                if ((((int)responseCode >= 200 && (int)responseCode <= 299 || (int)responseCode == 304)))
                {
                    ifUnmodifiedSince = _httpRequestHeaderHelper.CheckIfUnmodifiedSince(request, lastModified);
                    if (ifUnmodifiedSince.HasValue && !ifUnmodifiedSince.Value)
                    {
                        //If the requested variant has been modified since the specified time, 
                        //the server MUST NOT perform the requested operation, and MUST return 
                        //a 412 (Precondition Failed). Otherwise header is ignored. 

                        //If the request normally (i.e., without the If-Unmodified-Since header) 
                        //would result in anything other than a 2xx or 412 status, 
                        //the If-Unmodified-Since header SHOULD be ignored.
                        responseCode = HttpStatusCode.PreconditionFailed;
                    }
                }

                if ((((int)responseCode >= 200 && (int)responseCode <= 299 || (int)responseCode == 304)))
                {
                    ifModifiedSince = _httpRequestHeaderHelper.CheckIfModifiedSince(request, lastModified);
                    if (ifModifiedSince.HasValue)
                    {
                        if (ifModifiedSince.Value && responseCode == HttpStatusCode.NotModified)
                        {
                            //ifNoneMatch must be ignored if ifModifiedSince does not match so return entire entity
                            responseCode = HttpStatusCode.OK;
                        }
                        else
                        {
                            responseCode = HttpStatusCode.NotModified;
                        }
                    }
                }
            }

            //If its not modified there is no need to send it
            if ((((int)responseCode >= 200 && (int)responseCode <= 299)))
            {    
                var ifRange = _httpRequestHeaderHelper.CheckIfRange(request, etag, lastModified);
                if (ifRange.HasValue)
                {
                    //GET /foo HTTP/1.1
                    //Range: 500-1000
                    //If-Match: "abc", "xyz"
                    //If-Range: "xyz"

                    //This clearly says: if the entity is "abc", send me the whole thing, if
                    //it's "xyz", send me the second 500 bytes, otherwise, send me a 412.

                    //if the entity is unchanged, send me the part(s) that I am missing; otherwise, send me the entire new entity
                    if (ifRange.Value)
                    {
                        responseCode = HttpStatusCode.PartialContent;
                    }
                    else
                    {
                        responseCode = HttpStatusCode.OK;
                    }
                }
            }

            return responseCode;
        }        
    }
}