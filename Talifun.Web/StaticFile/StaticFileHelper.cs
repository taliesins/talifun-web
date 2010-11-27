using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace Talifun.Web.StaticFile
{
    public static class StaticFileHelper
    {
        private const int BufferSize = 32768;
        private const long MaxFileSizeToServe = int.MaxValue;

        internal const uint ErrorTheRemoteHostClosedTheConnection = 0x80072746; //WSAECONNRESET (10054)

        private static readonly IRetryableFileOpener RetryableFileOpener = new RetryableFileOpener();
        private static readonly IMimeTyper MimeTyper = new MimeTyper();
        private static readonly IHasher Hasher = new Hasher(RetryableFileOpener);
        private static readonly IHttpRequestHeaderHelper HttpRequestHeaderHelper = new HttpRequestHeaderHelper();
        private static readonly FileEntitySettingProvider FileEntitySettingProvider = new FileEntitySettingProvider();
        private static IHttpResponseHeaderHelper _httpResponseHeaderHelper;
        private static IHttpRequestResponder _httpRequestResponder;
        private static WebServerType _webServerType;

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

            var fileSettingEntity = FileEntitySettingProvider.GetSetting(file);

            var fileEntity = new FileEntity(RetryableFileOpener, MimeTyper, Hasher, MaxFileSizeToServe, BufferSize, file, fileSettingEntity);

            if (_webServerType == WebServerType.NotSet)
            {
                _webServerType = WebServerDetector.DetectWebServerType(context);
            }

            if (_httpResponseHeaderHelper == null)
            {
                _httpResponseHeaderHelper = new HttpResponseHeaderHelper(_webServerType);
                _httpRequestResponder = new HttpRequestResponder(HttpRequestHeaderHelper, _httpResponseHeaderHelper);
            }

            //We don't want to use up all the servers memory keeping a copy of the file, we just want to stream file to client
            response.BufferOutput = false;
      
            try
            {
                if (!_httpRequestResponder.IsHttpMethodAllowed(request))
                {
                    //If we are unable to parse url send 405 Method not allowed
                    _httpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatusCode.MethodNotAllowed);
                    return;
                }

                if (!fileEntity.IsAllowedToServeRequestedEntity)
                {
                    //If we are unable to parse url send 403 Path Forbidden
                    _httpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatusCode.Forbidden);
                    return;
                }

                var requestHttpMethod = HttpRequestHeaderHelper.GetHttpMethod(request);

                var compressionType = HttpRequestHeaderHelper.GetCompressionMode(request);

                // If this is a binary file like image, then we won't compress it.
                if (!fileEntity.IsCompressable)
                {
                    compressionType = ResponseCompressionType.None;
                }

                // If it is a partial request we need to get bytes of orginal entity data, we will compress the byte ranges returned
                var entityStoredWithCompressionType = compressionType;
                var isRangeRequest = HttpRequestHeaderHelper.IsRangeRequest(request);
                if (isRangeRequest)
                {
                    entityStoredWithCompressionType = ResponseCompressionType.None;
                }

                FileEntityCacheItem fileEntityCacheItem = null;

                if (!fileEntity.TryGetFileHandlerCacheItem(entityStoredWithCompressionType, out fileEntityCacheItem))
                {
                    //File does not exist
                    if (!fileEntity.DoesEntityExists)
                    {
                        _httpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatusCode.NotFound);
                        return;
                    }

                    //File too large to send
                    if (fileEntity.IsEntityLargerThanMaxFileSize)
                    {
                        _httpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatusCode.RequestEntityTooLarge);
                        return;
                    }
                }

                if (fileEntityCacheItem.EntityData == null && !fileEntity.DoesEntityExists)
                {
                    //If we have cached the properties of the file but its to large to serve from memory then we must check that the file exists each time.
                    _httpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatusCode.NotFound);
                    return;
                }

                //Unable to parse request range header
                IEnumerable<RangeItem> ranges = null;
                var requestRange = HttpRequestHeaderHelper.GetRanges(request, fileEntityCacheItem.ContentLength, out ranges);
                if (requestRange.HasValue && !requestRange.Value)
                {
                    _httpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatusCode.RequestedRangeNotSatisfiable);
                    return;
                }

                //Check if cached response is valid and if it is send appropriate response headers
                var httpStatus = _httpRequestResponder.GetResponseHttpStatus(request, fileEntityCacheItem.LastModified,
                                                             fileEntityCacheItem.Etag);

                _httpResponseHeaderHelper.SendHttpStatusHeaders(response, httpStatus);

                if (httpStatus == HttpStatusCode.NotModified  || httpStatus == HttpStatusCode.PreconditionFailed)
                {
                    return;
                }

                //Tell the client it supports resumable requests
                _httpResponseHeaderHelper.SetResponseResumable(response);

                //How the entity should be cached on the client
                _httpResponseHeaderHelper.SetResponseCachable(response, DateTime.Now, fileEntityCacheItem.LastModified, fileEntityCacheItem.Etag, fileEntity.Expires);

                var entityResponseForEntity = _httpRequestResponder.GetEntityResponse(response, ranges);
                entityResponseForEntity.SendHeaders(response, compressionType, fileEntityCacheItem);

                var transmitEntity = fileEntity.GetTransmitEntityStrategy(fileEntityCacheItem);
                entityResponseForEntity.SendBody(requestHttpMethod, response, transmitEntity);
            }
            catch (HttpException httpException)
            {
                //Client disconnected half way through us sending data
                if (httpException.ErrorCode != ErrorTheRemoteHostClosedTheConnection)
                    return;

                throw;
            }
        }
    }
}