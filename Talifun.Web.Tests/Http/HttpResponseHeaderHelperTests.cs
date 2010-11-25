using System;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using System.Net;

namespace Talifun.Web.Tests.Http
{
    [TestFixture]
    public class HttpResponseHeaderHelperTests
    {
        #region SendHttpStatusCodeHeaders
        [Test]
        public void SendHttpStatusCodeHeaders_HeadersSet_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpStatus = HttpStatus.Ok;
            var statusCode = (int)StringifyHttpHeaders.HttpStatusCodeFromHttpStatus(httpStatus);
            var statusDescription = StringifyHttpHeaders.StringFromHttpStatus(httpStatus);
            var webServerType = WebServerType.Unknown;

            httpResponse.Expect(x => x.StatusCode = statusCode);
            httpResponse.Expect(x => x.StatusDescription = statusDescription);

            //Act
            var httpResponseHeaderHelper = new HttpResponseHeaderHelper(webServerType);
            httpResponseHeaderHelper.SendHttpStatusHeaders(httpResponse, httpStatus);

            //Assert
            httpResponse.VerifyAllExpectations();
        }

        #endregion

        #region AppendHeader
        [Test]
        public void AppendHeader_AllButII7_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeader = HttpResponseHeader.ETag;
            var headerName = StringifyHttpHeaders.StringFromResponseHeader(httpResponseHeader);
            var headerValue = "Test";
            var webServerType = WebServerType.Unknown;

            httpResponse.Expect(x => x.AddHeader(headerName, headerValue));

            //Act
            var httpResponseHeaderHelper = new HttpResponseHeaderHelper(webServerType);
            httpResponseHeaderHelper.AppendHeader(httpResponse, httpResponseHeader, headerValue);

            //Assert
            httpResponse.AssertWasNotCalled(x => x.AppendHeader(headerName, headerValue));
            httpResponse.VerifyAllExpectations();
        }

        [Test]
        public void AppendHeader_StringAllButII7_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeader = HttpResponseHeader.ETag;
            var headerName = StringifyHttpHeaders.StringFromResponseHeader(httpResponseHeader);
            var headerValue = "Test";
            var webServerType = WebServerType.Unknown;

            httpResponse.Expect(x => x.AddHeader(headerName, headerValue));

            //Act
            var httpResponseHeaderHelper = new HttpResponseHeaderHelper(webServerType);
            httpResponseHeaderHelper.AppendHeader(httpResponse, headerName, headerValue);

            //Assert
            httpResponse.AssertWasNotCalled(x => x.AppendHeader(headerName, headerValue));
            httpResponse.VerifyAllExpectations();
        }

        [Test]
        public void AppendHeader_OnlyIIS7_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeader = HttpResponseHeader.ETag;
            var headerName = StringifyHttpHeaders.StringFromResponseHeader(httpResponseHeader);
            var headerValue = "Test";
            var webServerType = WebServerType.IIS7;

            httpResponse.Expect(x => x.AppendHeader(headerName, headerValue));

            //Act
            var httpResponseHeaderHelperIis7 = new HttpResponseHeaderHelper(webServerType);
            httpResponseHeaderHelperIis7.AppendHeader(httpResponse, httpResponseHeader, headerValue);

            //Assert
            httpResponse.AssertWasNotCalled(x => x.AddHeader(headerName, headerValue));
            httpResponse.VerifyAllExpectations();
        }

        [Test]
        public void AppendHeader_StringOnlyIIS7_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeader = HttpResponseHeader.ETag;
            var headerName = StringifyHttpHeaders.StringFromResponseHeader(httpResponseHeader);
            var headerValue = "Test";
            var webServerType = WebServerType.IIS7;

            httpResponse.Expect(x => x.AppendHeader(headerName, headerValue));

            //Act
            var httpResponseHeaderHelperIis7 = new HttpResponseHeaderHelper(webServerType);
            httpResponseHeaderHelperIis7.AppendHeader(httpResponse, headerName, headerValue);

            //Assert
            httpResponse.AssertWasNotCalled(x => x.AddHeader(headerName, headerValue));
            httpResponse.VerifyAllExpectations();
        }
        #endregion

        #region SetContentEncoding
        [Test]
        public void SetContentEncoding_None_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var responseCompressionType = ResponseCompressionType.None;

            var httpResponseHeader = HttpResponseHeader.ContentEncoding;
            var headerName = StringifyHttpHeaders.StringFromResponseHeader(httpResponseHeader);
            var headerValue = responseCompressionType.ToString().ToLower();
            var webServerType = WebServerType.Unknown;

            //Act
            var httpResponseHeaderHelper = new HttpResponseHeaderHelper(webServerType);
            httpResponseHeaderHelper.SetContentEncoding(httpResponse, responseCompressionType);

            //Assert
            httpResponse.AssertWasNotCalled(x => x.AddHeader(headerName, headerValue));
            httpResponse.VerifyAllExpectations();
        }

        [Test]
        public void SetContentEncoding_Deflate_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var responseCompressionType = ResponseCompressionType.Deflate;

            var httpResponseHeader = HttpResponseHeader.ContentEncoding;
            var headerName = StringifyHttpHeaders.StringFromResponseHeader(httpResponseHeader);
            var headerValue = responseCompressionType.ToString().ToLower();
            var webServerType = WebServerType.Unknown;

            httpResponse.Expect(x => x.AddHeader(headerName, headerValue));

            //Act
            var httpResponseHeaderHelper = new HttpResponseHeaderHelper(webServerType);
            httpResponseHeaderHelper.SetContentEncoding(httpResponse, responseCompressionType);

            //Assert
            httpResponse.VerifyAllExpectations();
        }

        [Test]
        public void SetContentEncoding_GZip_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var responseCompressionType = ResponseCompressionType.GZip;

            var httpResponseHeader = HttpResponseHeader.ContentEncoding;
            var headerName = StringifyHttpHeaders.StringFromResponseHeader(httpResponseHeader);
            var headerValue = responseCompressionType.ToString().ToLower();
            var webServerType = WebServerType.Unknown;

            httpResponse.Expect(x => x.AddHeader(headerName, headerValue));

            //Act
            var httpResponseHeaderHelper = new HttpResponseHeaderHelper(webServerType);
            httpResponseHeaderHelper.SetContentEncoding(httpResponse, responseCompressionType);

            //Assert
            httpResponse.VerifyAllExpectations();
        }
        #endregion

        #region SetResponseResumable
        [Test]
        public void SetResponseResumable_HeadersSet_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();

            var headerName = StringifyHttpHeaders.StringFromResponseHeader(HttpResponseHeader.AcceptRanges);
            var headerValue = HttpResponseHeaderHelper.HttpHeaderAcceptRangesBytes;
            var webServerType = WebServerType.Unknown;

            httpResponse.Expect(x => x.AddHeader(headerName, headerValue));

            //Act
            var httpResponseHeaderHelper = new HttpResponseHeaderHelper(webServerType);
            httpResponseHeaderHelper.SetResponseResumable(httpResponse);

            //Assert
            httpResponse.VerifyAllExpectations();
        }

        #endregion

        #region SetResponseCachable
        [Test]
        public void SetResponseCachable_HeadersSet_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var cache = MockRepository.GenerateMock<HttpCachePolicyBase>();
            httpResponse.Stub(x => x.Cache).Return(cache);

            var now = DateTime.Now;
            var etag = "1234567";
            var maxAge = new TimeSpan(1, 0, 0, 0);
            
            var expiryDate = now.Add(maxAge);
            var cachability = HttpCacheability.Public;
            var cacheExtensions = "must-revalidate, proxy-revalidate";
            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01);

            var webServerType = WebServerType.Unknown;
            
            httpResponse.Cache.Expect(x => x.SetExpires(expiryDate));
            httpResponse.Cache.Expect(x => x.SetCacheability(cachability));
            httpResponse.Cache.Expect(x => x.AppendCacheExtension(cacheExtensions));
            httpResponse.Cache.Expect(x => x.SetLastModified(lastModified));
            httpResponse.Cache.Expect(x => x.SetETag(etag));
            httpResponse.Cache.Expect(x => x.SetMaxAge(maxAge));

            //Act
            var httpResponseHeaderHelper = new HttpResponseHeaderHelper(webServerType);
            httpResponseHeaderHelper.SetResponseCachable(httpResponse, now, lastModified, etag, maxAge);

            //Assert
            cache.VerifyAllExpectations();
            httpResponse.VerifyAllExpectations();
        }
        #endregion

        #region SetContentType
        [Test]
        public void SetContentType()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var contentType = "text/plain";
            var webServerType = WebServerType.Unknown;

            httpResponse.Expect(x => x.ContentType = contentType);

            //Act
            var httpResponseHeaderHelper = new HttpResponseHeaderHelper(webServerType);
            httpResponseHeaderHelper.SetContentType(httpResponse, contentType);

            //Assert
            httpResponse.VerifyAllExpectations();
        }
        #endregion
    }
}
