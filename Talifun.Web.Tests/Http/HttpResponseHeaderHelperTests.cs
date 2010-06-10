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
            var httpStatus = HttpStatus.OK;
            var statusCode = StringifyHttpHeaders.HttpStatusCodeFromHttpStatus(httpStatus);
            var statusDescription = StringifyHttpHeaders.StringFromHttpStatus(httpStatus);

            httpResponse.Expect(x => x.StatusCode = (int)statusCode);
            httpResponse.Expect(x => x.StatusDescription = statusDescription);

            //Act
            var httpResponseHeaderHelper = new HttpResponseHeaderHelper(WebServerType.Unknown);
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

            httpResponse.Expect(x => x.AddHeader(headerName, headerValue));

            //Act
            var httpResponseHeaderHelper = new HttpResponseHeaderHelper(WebServerType.Unknown);
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

            httpResponse.Expect(x => x.AddHeader(headerName, headerValue));

            //Act
            var httpResponseHeaderHelper = new HttpResponseHeaderHelper(WebServerType.Unknown);
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

            httpResponse.Expect(x => x.AppendHeader(headerName, headerValue));

            //Act
            var httpResponseHeaderHelperIis7 = new HttpResponseHeaderHelper(WebServerType.IIS7);
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

            httpResponse.Expect(x => x.AppendHeader(headerName, headerValue));

            //Act
            var httpResponseHeaderHelperIis7 = new HttpResponseHeaderHelper(WebServerType.IIS7);
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

            //Act
            var httpResponseHeaderHelper = new HttpResponseHeaderHelper(WebServerType.Unknown);
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

            httpResponse.Expect(x => x.AddHeader(headerName, headerValue));

            //Act
            var httpResponseHeaderHelper = new HttpResponseHeaderHelper(WebServerType.Unknown);
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

            httpResponse.Expect(x => x.AddHeader(headerName, headerValue));

            //Act
            var httpResponseHeaderHelper = new HttpResponseHeaderHelper(WebServerType.Unknown);
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
            var headerValue = HttpResponseHeaderHelper.HTTP_HEADER_ACCEPT_RANGES_BYTES;

            httpResponse.Expect(x => x.AddHeader(headerName, headerValue));

            //Act
            var httpResponseHeaderHelper = new HttpResponseHeaderHelper(WebServerType.Unknown);
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
            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01);
            var etag = "1234567";
            var maxAge = new TimeSpan(1, 0, 0, 0);

            httpResponse.Cache.Expect(x => x.SetExpires(now.Add(maxAge)));
            httpResponse.Cache.Expect(x => x.SetCacheability(HttpCacheability.Public));
            httpResponse.Cache.Expect(x => x.AppendCacheExtension("must-revalidate, proxy-revalidate"));
            httpResponse.Cache.Expect(x => x.SetLastModified(lastModified));
            httpResponse.Cache.Expect(x => x.SetETag(etag));
            httpResponse.Cache.Expect(x => x.SetMaxAge(maxAge));

            //Act
            var httpResponseHeaderHelper = new HttpResponseHeaderHelper(WebServerType.Unknown);
            httpResponseHeaderHelper.SetResponseCachable(httpResponse, now, lastModified, etag, maxAge);

            //Assert
            cache.VerifyAllExpectations();
            httpResponse.VerifyAllExpectations();
        }
        #endregion
    }
}
