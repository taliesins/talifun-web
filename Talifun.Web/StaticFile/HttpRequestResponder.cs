using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talifun.Web.StaticFile
{
    public class HttpRequestResponder : IHttpRequestResponder
    {
        protected readonly IHttpRequestHeaderHelper HttpRequestHeaderHelper;
        protected readonly IHttpResponseHeaderHelper HttpResponseHeaderHelper;

        public HttpRequestResponder(IHttpRequestHeaderHelper httpRequestHeaderHelper, IHttpResponseHeaderHelper httpResponseHeaderHelper)
        {
            HttpRequestHeaderHelper = httpRequestHeaderHelper;
            HttpResponseHeaderHelper = httpResponseHeaderHelper;
        }

        public void ServeRequest(HttpRequestBase request, HttpResponseBase response, FileEntity fileEntity)
        {
            if (!IsHttpMethodAllowed(request))
            {
                //If we are unable to parse url send 405 Method not allowed
                HttpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatusCode.MethodNotAllowed);
                return;
            }

            if (!fileEntity.IsAllowedToServeRequestedEntity)
            {
                //If we are unable to parse url send 403 Path Forbidden
                HttpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatusCode.Forbidden);
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
                    HttpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatusCode.NotFound);
                    return;
                }

                //File too large to send
                if (fileEntity.IsEntityLargerThanMaxFileSize)
                {
                    HttpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatusCode.RequestEntityTooLarge);
                    return;
                }
            }

            if (fileEntityCacheItem.EntityData == null && !fileEntity.DoesEntityExists)
            {
                //If we have cached the properties of the file but its to large to serve from memory then we must check that the file exists each time.
                HttpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatusCode.NotFound);
                return;
            }

            //Unable to parse request range header
            IEnumerable<RangeItem> ranges = null;
            var requestRange = HttpRequestHeaderHelper.GetRanges(request, fileEntityCacheItem.ContentLength, out ranges);
            if (requestRange.HasValue && !requestRange.Value)
            {
                HttpResponseHeaderHelper.SendHttpStatusHeaders(response, HttpStatusCode.RequestedRangeNotSatisfiable);
                return;
            }

            //Check if cached response is valid and if it is send appropriate response headers
            var httpStatus = GetResponseHttpStatus(request, fileEntityCacheItem.LastModified,
                                                         fileEntityCacheItem.Etag);

            HttpResponseHeaderHelper.SendHttpStatusHeaders(response, httpStatus);

            if (httpStatus == HttpStatusCode.NotModified || httpStatus == HttpStatusCode.PreconditionFailed)
            {
                return;
            }

            //Tell the client it supports resumable requests
            HttpResponseHeaderHelper.SetResponseResumable(response);

            //How the entity should be cached on the client
            HttpResponseHeaderHelper.SetResponseCachable(response, DateTime.Now, fileEntityCacheItem.LastModified, fileEntityCacheItem.Etag, fileEntity.Expires);

            var entityResponseForEntity = GetEntityResponse(response, ranges);
            entityResponseForEntity.SendHeaders(response, compressionType, fileEntityCacheItem);

            var transmitEntity = fileEntity.GetTransmitEntityStrategy(fileEntityCacheItem);
            entityResponseForEntity.SendBody(requestHttpMethod, response, transmitEntity);
        }

        /// <summary>
        /// The entity respose type to use.
        /// </summary>
        /// <param name="response">An HTTP response.</param>
        /// <param name="ranges">The byte ranges to serve.</param>
        /// <returns>Entity respose type to use</returns>
        protected IEntityResponse GetEntityResponse(HttpResponseBase response, IEnumerable<RangeItem> ranges)
        {
            if (response.StatusCode != (int)HttpStatusCode.PartialContent)
            {
                //Send a full response
                return new FullEntityResponse(HttpResponseHeaderHelper);
            }

            if (ranges.Count() == 1)
            {
                //Single byte range request, send a partial response
                return new SinglePartEntityResponse(HttpResponseHeaderHelper, ranges.First());
            }

            //Multi byte range request, send a partial response
            return new MultiPartEntityResponse(HttpResponseHeaderHelper, ranges);
        }

        /// <summary>
        /// Determine whether the http method is supported. Currently we only support get and head methods.
        /// </summary>
        /// <param name="request">An HTTP request.</param>
        /// <returns>True if http method is supported; false if it is not</returns>
        protected bool IsHttpMethodAllowed(HttpRequestBase request)
        {
            var httpMethod = HttpRequestHeaderHelper.GetHttpMethod(request);
            return (httpMethod == HttpMethod.Get || httpMethod == HttpMethod.Head);
        }

        /// <summary>
        /// Process the http request to calculate its http response code.
        /// </summary>
        /// <param name="request">An HTTP request.</param>
        /// <param name="lastModified">The last modified date of the entity.</param>
        /// <param name="etag">The etag of the entity.</param>
        /// <returns>
        /// Returns httpStatusCode for Http request.
        /// </returns>
        /// <remarks>
        /// When the browser has a satisfiable cached response, the appropriate header is also set
        /// so there is no need to continue the processing of the entity.
        /// </remarks>
        protected HttpStatusCode GetResponseHttpStatus(HttpRequestBase request, DateTime lastModified, string etag)
        {
            lastModified = lastModified.ToUniversalTime();

            //Always assume we going to send whole entity
            var responseCode = HttpStatusCode.Ok;

            if (HttpRequestHeaderHelper.IsRangeRequest(request))
            {
                //It is a partial request
                responseCode = HttpStatusCode.PartialContent;
            }

            bool? ifNoneMatch = null;
            bool? ifMatch = null;

            if ((((int)responseCode >= 200 && (int)responseCode <= 299 || (int)responseCode == 304)))
            {
                //If there no matches then we do not want a cached response
                ifNoneMatch = HttpRequestHeaderHelper.CheckIfNoneMatch(request, etag, true);
                if (ifNoneMatch.HasValue)
                {
                    if (ifNoneMatch.Value && responseCode == HttpStatusCode.NotModified)
                    {
                        responseCode = HttpStatusCode.Ok;
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
                ifMatch = HttpRequestHeaderHelper.CheckIfMatch(request, etag, true);
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

            if (!(ifNoneMatch.HasValue && ifNoneMatch.Value) || !(ifMatch.HasValue && !ifMatch.Value))
            {
                //Only use weakly typed etags headers if strong ones are valid

                bool? unlessModifiedSince = null;
                bool? ifUnmodifiedSince = null;
                bool? ifModifiedSince = null;

                if ((((int)responseCode >= 200 && (int)responseCode <= 299 || (int)responseCode == 304)))
                {
                    unlessModifiedSince = HttpRequestHeaderHelper.CheckUnlessModifiedSince(request, lastModified);
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
                    ifUnmodifiedSince = HttpRequestHeaderHelper.CheckIfUnmodifiedSince(request, lastModified);
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
                    ifModifiedSince = HttpRequestHeaderHelper.CheckIfModifiedSince(request, lastModified);
                    if (ifModifiedSince.HasValue)
                    {
                        if (ifModifiedSince.Value && responseCode == HttpStatusCode.NotModified)
                        {
                            //ifNoneMatch must be ignored if ifModifiedSince does not match so return entire entity
                            responseCode = HttpStatusCode.Ok;
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
                var ifRange = HttpRequestHeaderHelper.CheckIfRange(request, etag, lastModified);
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
                        responseCode = HttpStatusCode.Ok;
                    }
                }
            }

            return responseCode;
        }
    }
}
