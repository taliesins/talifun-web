using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;

namespace Talifun.Web.Tests.Http
{
    [TestFixture]
    public class HttpRequestHeaderHelperTests
    {
        #region GetHttpHeaderValue
        [Test]
        public void GetHttpHeaderValue_HttpRequestHeaderFound_HeaderValue()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerDefaultValue = "Unknown User Agent";
            var headerValue = "Talifun Browser";
            var headerType = HttpRequestHeader.UserAgent;
            var headerName =  (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var userAgentValue = httpRequestHeaderHelper.GetHttpHeaderValue(httpRequest, headerType, headerDefaultValue);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(headerValue, userAgentValue);
        }

        [Test]
        public void GetHttpHeaderValue_HttpRequestHeaderNotFound_DefaultValue()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerDefaultValue = "Unknown User Agent";
            var headerValue = string.Empty; //means that header was not set
            var headerType = HttpRequestHeader.UserAgent;
            var headerName =  (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var userAgentValue = httpRequestHeaderHelper.GetHttpHeaderValue(httpRequest, headerType, headerDefaultValue);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(headerDefaultValue, userAgentValue);
        }

        [Test]
        public void GetHttpHeaderValue_HttpRequestHeaderStringFound_HeaderValue()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerDefaultValue = "Unknown User Agent";
            var headerValue = "Talifun Browser";
            var headerType = HttpRequestHeader.UserAgent;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);
   
            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var userAgentValue = httpRequestHeaderHelper.GetHttpHeaderValue(httpRequest, headerName, headerDefaultValue);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(headerValue, userAgentValue);
        }

        [Test]
        public void GetHttpHeaderValue_HttpRequestHeaderStringNotFound_DefaultValue()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerDefaultValue = "Unknown User Agent";
            var headerValue = string.Empty;
            var headerType = HttpRequestHeader.UserAgent;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var userAgentValue = httpRequestHeaderHelper.GetHttpHeaderValue(httpRequest, headerName, headerDefaultValue);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(headerDefaultValue, userAgentValue);
        }

        [Test]
        public void GetHttpHeaderValue_HttpRequestHeaderQuotedStringFound_HeaderValue()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerDefaultValue = "Unknown User Agent";
            var userAgent = "Talifun Browser";
            var headerValue = "\"" + userAgent + "\"";
            var headerType = HttpRequestHeader.UserAgent;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var userAgentValue = httpRequestHeaderHelper.GetHttpHeaderValue(httpRequest, headerName, headerDefaultValue);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(userAgent, userAgentValue);
        }

        [Test]
        public void GetHttpHeaderValue_IsCaseSensitive_HeaderValue()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerDefaultValue = "abcdef";
            var headerValue = "abcdeF";
            var headerType = HttpRequestHeader.UserAgent;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var userAgentValue = httpRequestHeaderHelper.GetHttpHeaderValue(httpRequest, headerName, headerDefaultValue);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(headerValue, userAgentValue);
        }
        #endregion

        #region GetHttpHeaderValues
        [Test]
        public void GetHttpHeaderValues_NoHttpHeaderQValues_EmptyList()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = string.Empty;
            var headerType = HttpRequestHeader.IfMatch;
            var headerName = (string)headerType;
            
            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var headerValues = httpRequestHeaderHelper.GetHttpHeaderValues(httpRequest, headerType);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsEmpty(headerValues);
        }

        [Test]
        public void GetHttpHeaderValues_HttpHeaderWithOneValueSet_List()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = "1234567";
            var headerType = HttpRequestHeader.IfMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var headerValues = httpRequestHeaderHelper.GetHttpHeaderValues(httpRequest, headerType);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(1, headerValues.Count);
            Assert.AreEqual(headerValue.ToLowerInvariant(), headerValues[0]);
        }

        [Test]
        public void GetHttpHeaderValues_MultipleHttpHeaderValuesSet_List()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var identity1 = "gzip";
            var identity2 = "deflate";
            var headerValue = identity1 + "," + identity2;
            var headerType = HttpRequestHeader.IfMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var headerValues = httpRequestHeaderHelper.GetHttpHeaderValues(httpRequest, headerType);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(2, headerValues.Count);
            Assert.AreEqual(identity1, headerValues[0]);
            Assert.AreEqual(identity2, headerValues[1]);
        }

        [Test]
        public void GetHttpHeaderValues_MultipleHttpHeaderWithValuesSetWithSpacing_List()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var identity1 = "gzip";
            var identity2 = "deflate";
            var headerValue = identity1 + " , " + identity2;
            var headerType = HttpRequestHeader.IfMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var headerValues = httpRequestHeaderHelper.GetHttpHeaderValues(httpRequest, headerType);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(2, headerValues.Count);
            Assert.AreEqual(identity1, headerValues[0]);
            Assert.AreEqual(identity2, headerValues[1]);
        }

        [Test]
        public void GetHttpHeaderValues_MultipleHttpHeaderWithQuotedValuesSet_List()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var identity1 = "deflate";
            var identity2 = "gzip, hello";
            var headerValue = identity1 + " , \"" + identity2 + "\"";
            var headerType = HttpRequestHeader.IfMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var headerValues = httpRequestHeaderHelper.GetHttpHeaderValues(httpRequest, headerType);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(2, headerValues.Count);
            Assert.AreEqual(identity1, headerValues[0]);
            Assert.AreEqual(identity2, headerValues[1]);
        }

        [Test]
        public void GetHttpHeaderValues_IsCaseSensitive_List()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = "abcdeF";
            var headerType = HttpRequestHeader.IfMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);
           
            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var headerValues = httpRequestHeaderHelper.GetHttpHeaderValues(httpRequest, headerType);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(1, headerValues.Count);
            Assert.AreEqual(headerValue, headerValues[0]);
        }
        #endregion

        #region GetHttpHeaderWithQValues
        [Test]
        public void GetHttpHeaderWithQValues_NoHttpHeaderQValues_EmptyList()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = string.Empty;
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);
            
            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var headerValueWithQValues = httpRequestHeaderHelper.GetHttpHeaderWithQValues(httpRequest, headerType);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsEmpty(headerValueWithQValues);
        }

        [Test]
        public void GetHttpHeaderWithQValues_HttpHeaderWithNoQValueSet_List()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = "gzip";
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var headerValueWithQValues = httpRequestHeaderHelper.GetHttpHeaderWithQValues(httpRequest, headerType);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(1, headerValueWithQValues.Count);
            Assert.AreEqual(headerValue.ToLowerInvariant(), headerValueWithQValues[0].Identity);
            Assert.IsNull(headerValueWithQValues[0].QValue);
        }

        [Test]
        public void GetHttpHeaderWithQValues_HttpHeaderWithQValueSet_List()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var identity = "gzip";
            var qValue = 0.5f;
            var headerValue = identity + ";q=" + qValue.ToString("N1");
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var headerValueWithQValues = httpRequestHeaderHelper.GetHttpHeaderWithQValues(httpRequest, headerType);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(1, headerValueWithQValues.Count);
            Assert.AreEqual(identity, headerValueWithQValues[0].Identity);
            Assert.IsNotNull(headerValueWithQValues[0].QValue);
            Assert.AreEqual(qValue, headerValueWithQValues[0].QValue.Value);
        }

        [Test]
        public void GetHttpHeaderWithQValues_MultipleHttpHeaderWithQValueSet_List()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var identity1 = "gzip";
            var qValue1 = 0.5f;

            var identity2 = "deflate";
            var qValue2 = 0.8f;

            var headerValue = identity1 + ";q=" + qValue1.ToString("N1") + "," + identity2 + ";q=" + qValue2.ToString("N1");
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var headerValueWithQValues = httpRequestHeaderHelper.GetHttpHeaderWithQValues(httpRequest, headerType);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(2, headerValueWithQValues.Count);

            Assert.AreEqual(identity1, headerValueWithQValues[0].Identity);
            Assert.IsNotNull(headerValueWithQValues[0].QValue);
            Assert.AreEqual(qValue1, headerValueWithQValues[0].QValue.Value);

            Assert.AreEqual(identity2, headerValueWithQValues[1].Identity);
            Assert.IsNotNull(headerValueWithQValues[1].QValue);
            Assert.AreEqual(qValue2, headerValueWithQValues[1].QValue.Value);
        }

        [Test]
        public void GetHttpHeaderWithQValues_MultipleHttpHeaderWithQValueSetWithSpacing_List()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var identity1 = "gzip";
            var qValue1 = 0.5f;

            var identity2 = "deflate";
            var qValue2 = 0.8f;

            var headerValue = identity1 + " ; q = " + qValue1.ToString("N1") + " , " + identity2 + "; q=" + qValue2.ToString("N1");
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var headerValueWithQValues = httpRequestHeaderHelper.GetHttpHeaderWithQValues(httpRequest, headerType);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(2, headerValueWithQValues.Count);

            Assert.AreEqual(identity1, headerValueWithQValues[0].Identity);
            Assert.IsNotNull(headerValueWithQValues[0].QValue);
            Assert.AreEqual(qValue1, headerValueWithQValues[0].QValue.Value);

            Assert.AreEqual(identity2, headerValueWithQValues[1].Identity);
            Assert.IsNotNull(headerValueWithQValues[1].QValue);
            Assert.AreEqual(qValue2, headerValueWithQValues[1].QValue.Value);
        }

        [Test]
        public void GetHttpHeaderWithQValues_MultipleHttpHeaderWithQuotedQValueSet_List()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var identity1 = "gzip";
            var qValue1 = 0.5f;

            var identity2 = "deflate, test";
            var qValue2 = 0.8f;

            var headerValue = identity1 + ";q=" + qValue1.ToString("N1") + ",\"" + identity2 + "\";q=" + qValue2.ToString("N1");
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var headerValueWithQValues = httpRequestHeaderHelper.GetHttpHeaderWithQValues(httpRequest, headerType);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(2, headerValueWithQValues.Count);

            Assert.AreEqual(identity1, headerValueWithQValues[0].Identity);
            Assert.IsNotNull(headerValueWithQValues[0].QValue);
            Assert.AreEqual(qValue1, headerValueWithQValues[0].QValue.Value);

            Assert.AreEqual(identity2, headerValueWithQValues[1].Identity);
            Assert.IsNotNull(headerValueWithQValues[1].QValue);
            Assert.AreEqual(qValue2, headerValueWithQValues[1].QValue.Value);
        }

        [Test]
        public void GetHttpHeaderWithQValues_IsCaseSensitive_List()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = "abcdeF";
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var headerValueWithQValues = httpRequestHeaderHelper.GetHttpHeaderWithQValues(httpRequest, headerType);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(1, headerValueWithQValues.Count);
            Assert.AreEqual(headerValue, headerValueWithQValues[0].Identity);
            Assert.IsNull(headerValueWithQValues[0].QValue);
        }

        [Test]
        public void GetHttpHeaderWithQValues_InvalidQValueDoubleEquals_List()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = "gzip, deflate, x-gzip, identity; q==0.5";
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var headerValueWithQValues = httpRequestHeaderHelper.GetHttpHeaderWithQValues(httpRequest, headerType);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(5, headerValueWithQValues.Count);
            Assert.AreEqual("identity", headerValueWithQValues[3].Identity);
            Assert.IsNull(headerValueWithQValues[3].QValue);
            Assert.AreEqual("q==0.5", headerValueWithQValues[4].Identity);
            Assert.IsNull(headerValueWithQValues[4].QValue);
        }

        [Test]
        public void GetHttpHeaderWithQValues_InvalidQValueNotANumber_List()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = "gzip, deflate, x-gzip, identity; q=moo";
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var headerValueWithQValues = httpRequestHeaderHelper.GetHttpHeaderWithQValues(httpRequest, headerType);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(5, headerValueWithQValues.Count);
            Assert.AreEqual("identity", headerValueWithQValues[3].Identity);
            Assert.IsNull(headerValueWithQValues[3].QValue);
            Assert.AreEqual("q=moo", headerValueWithQValues[4].Identity);
            Assert.IsNull(headerValueWithQValues[4].QValue);
        }
        #endregion

        #region GetCompressionMode
        [Test]
        public void GetCompressionMode_EmptyAcceptEncoding_None()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = string.Empty;
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);
            
            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var compressionMode = httpRequestHeaderHelper.GetCompressionMode(httpRequest);
            
            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(ResponseCompressionType.None, compressionMode);
        }

        [Test]
        public void GetCompressionMode_InvalidAcceptEncoding_None()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = "invalid;q=0.5";
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var compressionMode = httpRequestHeaderHelper.GetCompressionMode(httpRequest);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(ResponseCompressionType.None, compressionMode);
        }

        [Test]
        public void GetCompressionMode_CompressionTermASubstringNotSpecifiedAcceptEncoding_None()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = "compression, test, testdeflater";
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var compressionMode = httpRequestHeaderHelper.GetCompressionMode(httpRequest);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(ResponseCompressionType.None, compressionMode);
        }

        [Test]
        public void GetCompressionMode_DeflateAcceptEncoding_Deflate()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = "deflate";
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var compressionMode = httpRequestHeaderHelper.GetCompressionMode(httpRequest);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(ResponseCompressionType.Deflate, compressionMode);
        }

        [Test]
        public void GetCompressionMode_GzipAcceptEncoding_Gzip()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = "gzip";
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var compressionMode = httpRequestHeaderHelper.GetCompressionMode(httpRequest);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(ResponseCompressionType.GZip, compressionMode);
        }

        [Test]
        public void GetCompressionMode_CompressionPreferenceWithoutQValuesAcceptEncoding_Deflate()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = "gzip, deflate";
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var compressionMode = httpRequestHeaderHelper.GetCompressionMode(httpRequest);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(ResponseCompressionType.GZip, compressionMode);
        }

        [Test]
        public void GetCompressionMode_CompressionPreferenceWithQValuesAcceptEncoding_Deflate()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = "deflate;q=0.5, gzip;q=0.9, compress";
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var compressionMode = httpRequestHeaderHelper.GetCompressionMode(httpRequest);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(ResponseCompressionType.GZip, compressionMode);
        }

        [Test]
        public void GetCompressionMode_WildcardAcceptEncoding_Deflate()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = "*";
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var compressionMode = httpRequestHeaderHelper.GetCompressionMode(httpRequest);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(ResponseCompressionType.Deflate, compressionMode);
        }

        [Test]
        public void GetCompressionMode_WildcardWithGzipAcceptEncoding_Deflate()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = "*, gzip";
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var compressionMode = httpRequestHeaderHelper.GetCompressionMode(httpRequest);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(ResponseCompressionType.Deflate, compressionMode);
        }

        [Test]
        public void GetCompressionMode_WildcardWithDeflateAcceptEncoding_Gzip()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = "*, deflate";
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var compressionMode = httpRequestHeaderHelper.GetCompressionMode(httpRequest);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(ResponseCompressionType.GZip, compressionMode);
        }

        [Test]
        public void GetCompressionMode_WildcardWithGzipAndDeflateAcceptEncoding_Deflate()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = " * , gzip ; q = 0.5, deflate; q=0.9";
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var compressionMode = httpRequestHeaderHelper.GetCompressionMode(httpRequest);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(ResponseCompressionType.Deflate, compressionMode);
        }

        [Test]
        public void GetCompressionMode_WildcardWithPreferredInvalidAcceptEncoding_Deflate()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = " * , gzip ; q = 0.5, deflate; q=0.7, invalid;q=0.9";
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var compressionMode = httpRequestHeaderHelper.GetCompressionMode(httpRequest);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(ResponseCompressionType.Deflate, compressionMode);
        }

        [Test]
        public void GetCompressionMode_IsCaseSensitive_Gzip()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = "Gzip";
            var headerType = HttpRequestHeader.AcceptEncoding;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var compressionMode = httpRequestHeaderHelper.GetCompressionMode(httpRequest);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(ResponseCompressionType.GZip, compressionMode);
        }

        #endregion

        #region GetHttpMethod
        [Test]
        public void GetHttpMethod_GetRequest_Get()
        {
            //Arrange
            var httpMethodType = HttpMethod.Get;
            var httpMethodValue = (string)httpMethodType;

            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            httpRequest.Expect(x => x.HttpMethod).Return(httpMethodValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var result = httpRequestHeaderHelper.GetHttpMethod(httpRequest);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.AreEqual(httpMethodType, result);
        }
        #endregion

        #region IsRangeRequest
        [Test]
        public void IsRangeRequest_HasRangeHeader_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = "bytes=500-600,601-999";
            var headerType = HttpRequestHeader.Range;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var isRangeRequest = httpRequestHeaderHelper.IsRangeRequest(httpRequest);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.True(isRangeRequest);
        }

        [Test]
        public void IsRangeRequest_DoesNotHaveRangeHeader_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headerValue = string.Empty; 
            var headerType = HttpRequestHeader.Range;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var isRangeRequest = httpRequestHeaderHelper.IsRangeRequest(httpRequest);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.False(isRangeRequest);
        }
        #endregion

        #region CheckIfModifiedSince
        [Test]
        public void CheckIfModifiedSince_HasNoIfModifiedSinceHeader_Null()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01);
            var headerValue = string.Empty;
            var headerType = HttpRequestHeader.IfModifiedSince;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var hasBeenModifiedSince = httpRequestHeaderHelper.CheckIfModifiedSince(httpRequest, lastModified);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNull(hasBeenModifiedSince);
        }

        [Test]
        public void CheckIfModifiedSince_HasBeenModifiedSince_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01);
            var headerValue = lastModified.AddSeconds(-1).ToString("r");
            var headerType = HttpRequestHeader.IfModifiedSince;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var hasBeenModifiedSince = httpRequestHeaderHelper.CheckIfModifiedSince(httpRequest, lastModified);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(hasBeenModifiedSince);
            Assert.True(hasBeenModifiedSince.Value);
        }

        [Test]
        public void CheckIfModifiedSince_HasNotBeenModifiedSince_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01, DateTimeKind.Utc);
            var headerValue = lastModified.AddSeconds(1).ToString("r");
            var headerType = HttpRequestHeader.IfModifiedSince;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var hasBeenModifiedSince = httpRequestHeaderHelper.CheckIfModifiedSince(httpRequest, lastModified);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(hasBeenModifiedSince);
            Assert.False(hasBeenModifiedSince.Value);
        }

        [Test]
        public void CheckIfModifiedSince_DateModifiedSinceInvalid_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01, DateTimeKind.Utc);
            var headerValue = "invalid";
            var headerType = HttpRequestHeader.IfModifiedSince;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var hasBeenModifiedSince = httpRequestHeaderHelper.CheckIfModifiedSince(httpRequest, lastModified);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNull(hasBeenModifiedSince);
        }

        #endregion

        #region CheckIfUnmodifiedSince
        [Test]
        public void CheckIfUnmodifiedSince_HasNoIfModifiedSinceHeader_Null()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01);
            var headerValue = string.Empty;
            var headerType = HttpRequestHeader.IfUnmodifiedSince;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var hasNotBeenModifiedSince = httpRequestHeaderHelper.CheckIfUnmodifiedSince(httpRequest, lastModified);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNull(hasNotBeenModifiedSince);
        }

        [Test]
        public void CheckIfUnmodifiedSince_HasNotBeenModifiedSince_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01, DateTimeKind.Utc);
            var headerValue = lastModified.AddSeconds(1).ToString("r");
            var headerType = HttpRequestHeader.IfUnmodifiedSince;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var hasNotBeenModifiedSince = httpRequestHeaderHelper.CheckIfUnmodifiedSince(httpRequest, lastModified);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(hasNotBeenModifiedSince);
            Assert.True(hasNotBeenModifiedSince.Value);
        }

        [Test]
        public void CheckIfUnmodifiedSince_HasBeenModifiedSince_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01);
            var headerValue = lastModified.AddSeconds(-1).ToString("r");
            var headerType = HttpRequestHeader.IfUnmodifiedSince;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var hasNotBeenModifiedSince = httpRequestHeaderHelper.CheckIfUnmodifiedSince(httpRequest, lastModified);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(hasNotBeenModifiedSince);
            Assert.False(hasNotBeenModifiedSince.Value);
        }

        [Test]
        public void CheckIfUnmodifiedSince_DateUnmodifiedSinceInvalid_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01);
            var headerValue = "Invalid";
            var headerType = HttpRequestHeader.IfUnmodifiedSince;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var hasNotBeenModifiedSince = httpRequestHeaderHelper.CheckIfUnmodifiedSince(httpRequest, lastModified);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNull(hasNotBeenModifiedSince);
        }
        #endregion

        #region CheckUnlessModifiedSince
        [Test]
        public void CheckUnlessModifiedSince_HasNoIfModifiedSinceHeader_Null()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01);
            var headerValue = string.Empty;
            var headerType = HttpRequestHeader.UnlessModifiedSince;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var hasNotBeenModifiedSince = httpRequestHeaderHelper.CheckUnlessModifiedSince(httpRequest, lastModified);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNull(hasNotBeenModifiedSince);
        }

        [Test]
        public void CheckUnlessModifiedSince_HasNotBeenModifiedSince_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01);
            var headerValue = lastModified.AddSeconds(1).ToString("r");
            var headerType = HttpRequestHeader.UnlessModifiedSince;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);
            
            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var hasNotBeenModifiedSince = httpRequestHeaderHelper.CheckUnlessModifiedSince(httpRequest, lastModified);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(hasNotBeenModifiedSince);
            Assert.True(hasNotBeenModifiedSince.Value);
        }

        [Test]
        public void CheckUnlessModifiedSince_HasBeenModifiedSince_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01);
            var headerValue = lastModified.AddSeconds(-1).ToString("r");
            var headerType = HttpRequestHeader.UnlessModifiedSince;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var hasNotBeenModifiedSince = httpRequestHeaderHelper.CheckUnlessModifiedSince(httpRequest, lastModified);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(hasNotBeenModifiedSince);
            Assert.False(hasNotBeenModifiedSince.Value);
        }

        [Test]
        public void CheckUnlessModifiedSince_DateUnlessModifiedSinceInvalid_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01);
            var headerValue = "Invalid";
            var headerType = HttpRequestHeader.UnlessModifiedSince;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var hasNotBeenModifiedSince = httpRequestHeaderHelper.CheckUnlessModifiedSince(httpRequest, lastModified);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNull(hasNotBeenModifiedSince);
        }
        #endregion

        #region CheckIfRange
        [Test]
        public void CheckIfRange_NoRangeHeader_Null()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headers = MockRepository.GenerateMock<NameValueCollection>();
            httpRequest.Stub(x => x.Headers).Return(headers);
            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01);
            var etag = "1234567";
            var headerValue = string.Empty;
            var headerType = HttpRequestHeader.Range;
            var headerName = (string)headerType;

            httpRequest.Headers.Expect(x => x[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var satisfiesRangeCheck = httpRequestHeaderHelper.CheckIfRange(httpRequest, etag, lastModified);

            //Assert
            headers.VerifyAllExpectations();
            httpRequest.VerifyAllExpectations();
            Assert.IsNull(satisfiesRangeCheck);
        }

        [Test]
        public void CheckIfRange_NoCheckIfRangeHeader_Null()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headers = MockRepository.GenerateMock<NameValueCollection>();
            httpRequest.Stub(x => x.Headers).Return(headers);

            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01);
            var etag = "1234567";

            var rangeRequestHeaderValue = "bytes=500-600,601-999";
            var rangeRequestHeaderType = HttpRequestHeader.Range;
            var rangeRequestHeaderName = (string)rangeRequestHeaderType;

            var ifRangeHeaderValue = string.Empty;
            var ifRangeHeaderType = HttpRequestHeader.IfRange;
            var ifRangeHeaderName = (string)ifRangeHeaderType;

            httpRequest.Headers.Expect(x => x[rangeRequestHeaderName]).Return(rangeRequestHeaderValue);
            httpRequest.Headers.Expect(x => x[ifRangeHeaderName]).Return(ifRangeHeaderValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var satisfiesRangeCheck = httpRequestHeaderHelper.CheckIfRange(httpRequest, etag, lastModified);

            //Assert
            headers.VerifyAllExpectations();
            httpRequest.VerifyAllExpectations();
            Assert.IsNull(satisfiesRangeCheck);
        }

        [Test]
        public void CheckIfRange_CheckOnEtagIfRangeHeader_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headers = MockRepository.GenerateMock<NameValueCollection>();
            httpRequest.Stub(x => x.Headers).Return(headers);

            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01);
            var etag = "1234567";

            var rangeRequestHeaderValue = "bytes=500-600,601-999";
            var rangeRequestHeaderType = HttpRequestHeader.Range;
            var rangeRequestHeaderName = (string)rangeRequestHeaderType;

            var ifRangeHeaderValue = etag;
            var ifRangeHeaderType = HttpRequestHeader.IfRange;
            var ifRangeHeaderName = (string)ifRangeHeaderType;

            httpRequest.Headers.Expect(x => x[rangeRequestHeaderName]).Return(rangeRequestHeaderValue);
            httpRequest.Headers.Expect(x => x[ifRangeHeaderName]).Return(ifRangeHeaderValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var satisfiesRangeCheck = httpRequestHeaderHelper.CheckIfRange(httpRequest, etag, lastModified);

            //Assert
            headers.VerifyAllExpectations();
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(satisfiesRangeCheck);
            Assert.True(satisfiesRangeCheck.Value);
        }

        [Test]
        public void CheckIfRange_CheckOnEtagIfRangeHeader_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headers = MockRepository.GenerateMock<NameValueCollection>();
            httpRequest.Stub(x => x.Headers).Return(headers);

            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01);
            var etag = "1234567";

            var rangeRequestHeaderValue = "bytes=500-600,601-999";
            var rangeRequestHeaderType = HttpRequestHeader.Range;
            var rangeRequestHeaderName = (string)rangeRequestHeaderType;

            var ifRangeHeaderValue = etag + "8";
            var ifRangeHeaderType = HttpRequestHeader.IfRange;
            var ifRangeHeaderName = (string)ifRangeHeaderType;

            httpRequest.Headers.Expect(x => x[rangeRequestHeaderName]).Return(rangeRequestHeaderValue);
            httpRequest.Headers.Expect(x => x[ifRangeHeaderName]).Return(ifRangeHeaderValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var satisfiesRangeCheck = httpRequestHeaderHelper.CheckIfRange(httpRequest, etag, lastModified);

            //Assert
            headers.VerifyAllExpectations();
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(satisfiesRangeCheck);
            Assert.False(satisfiesRangeCheck.Value);
        }

        [Test]
        public void CheckIfRange_CheckOnLastModifiedIfRangeHeader_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headers = MockRepository.GenerateMock<NameValueCollection>();
            httpRequest.Stub(x => x.Headers).Return(headers);

            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01);
            var etag = "1234567";

            var rangeRequestHeaderValue = "bytes=500-600,601-999";
            var rangeRequestHeaderType = HttpRequestHeader.Range;
            var rangeRequestHeaderName = (string)rangeRequestHeaderType;

            var ifRangeHeaderValue = lastModified.ToString("r");
            var ifRangeHeaderType = HttpRequestHeader.IfRange;
            var ifRangeHeaderName = (string)ifRangeHeaderType;

            httpRequest.Headers.Expect(x => x[rangeRequestHeaderName]).Return(rangeRequestHeaderValue);
            httpRequest.Headers.Expect(x => x[ifRangeHeaderName]).Return(ifRangeHeaderValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var satisfiesRangeCheck = httpRequestHeaderHelper.CheckIfRange(httpRequest, etag, lastModified);

            //Assert
            headers.VerifyAllExpectations();
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(satisfiesRangeCheck);
            Assert.True(satisfiesRangeCheck.Value);
        }

        [Test]
        public void CheckIfRange_CheckOnLastModifiedIfRangeHeader_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var headers = MockRepository.GenerateMock<NameValueCollection>();
            httpRequest.Stub(x => x.Headers).Return(headers);

            var lastModified = new DateTime(2010, 01, 01, 01, 01, 01);
            var etag = "1234567";

            var rangeRequestHeaderValue = "bytes=500-600,601-999";
            var rangeRequestHeaderType = HttpRequestHeader.Range;
            var rangeRequestHeaderName = (string)rangeRequestHeaderType;

            var ifRangeHeaderValue = lastModified.AddSeconds(-1).ToString("r");
            var ifRangeHeaderType = HttpRequestHeader.IfRange;
            var ifRangeHeaderName = (string)ifRangeHeaderType;

            httpRequest.Headers.Expect(x => x[rangeRequestHeaderName]).Return(rangeRequestHeaderValue);
            httpRequest.Headers.Expect(x => x[ifRangeHeaderName]).Return(ifRangeHeaderValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var satisfiesRangeCheck = httpRequestHeaderHelper.CheckIfRange(httpRequest, etag, lastModified);

            //Assert
            headers.VerifyAllExpectations();
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(satisfiesRangeCheck);
            Assert.False(satisfiesRangeCheck.Value);
        }


        #endregion

        #region CheckIfMatch
        [Test]
        public void CheckIfMatch_HasNoIfMatchHeader_Null()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var entityTag = "1234567";
            var doesEntityExists = true;
            var headerValue = string.Empty;
            var headerType = HttpRequestHeader.IfMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var ifMatchSatisfied = httpRequestHeaderHelper.CheckIfMatch(httpRequest, entityTag, doesEntityExists);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNull(ifMatchSatisfied);
        }

        [Test]
        public void CheckIfMatch_HasMatchingEtag_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var entityTag = "1234567";
            var doesEntityExists = true;
            var headerValue = entityTag;
            var headerType = HttpRequestHeader.IfMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var ifMatchSatisfied = httpRequestHeaderHelper.CheckIfMatch(httpRequest, entityTag, doesEntityExists);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(ifMatchSatisfied);
            Assert.IsTrue(ifMatchSatisfied.Value);
        }

        [Test]
        public void CheckIfMatch_HasMatchingEtags_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var entityTag = "1234567";
            var doesEntityExists = true;
            var headerValue = "7654321," + entityTag;
            var headerType = HttpRequestHeader.IfMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var ifMatchSatisfied = httpRequestHeaderHelper.CheckIfMatch(httpRequest, entityTag, doesEntityExists);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(ifMatchSatisfied);
            Assert.IsTrue(ifMatchSatisfied.Value);
        }

        [Test]
        public void CheckIfMatch_HasNoMatchingEtag_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var entityTag = "1234567";
            var doesEntityExists = true;
            var headerValue = entityTag + "8";
            var headerType = HttpRequestHeader.IfMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var ifMatchSatisfied = httpRequestHeaderHelper.CheckIfMatch(httpRequest, entityTag, doesEntityExists);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(ifMatchSatisfied);
            Assert.IsFalse(ifMatchSatisfied.Value);
        }

        [Test]
        public void CheckIfMatch_HasNoMatchingEtags_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var entityTag = "1234567";
            var doesEntityExists = true;
            var headerValue = "7654321," + entityTag + "8";
            var headerType = HttpRequestHeader.IfMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var ifMatchSatisfied = httpRequestHeaderHelper.CheckIfMatch(httpRequest, entityTag, doesEntityExists);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(ifMatchSatisfied);
            Assert.IsFalse(ifMatchSatisfied.Value);
        }

        [Test]
        public void CheckIfMatch_IsCaseSensitive_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var entityTag = "abcdef";
            var doesEntityExists = true;
            var headerValue = "abcdeF";
            var headerType = HttpRequestHeader.IfMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var ifMatchSatisfied = httpRequestHeaderHelper.CheckIfMatch(httpRequest, entityTag, doesEntityExists);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(ifMatchSatisfied);
            Assert.IsFalse(ifMatchSatisfied.Value);
        }

        [Test]
        public void CheckIfMatch_MatchingWildCardAndEntityDoesExist_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var entityTag = "abcdef";
            var doesEntityExists = true;
            var headerValue = "*";
            var headerType = HttpRequestHeader.IfMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var ifMatchSatisfied = httpRequestHeaderHelper.CheckIfMatch(httpRequest, entityTag, doesEntityExists);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(ifMatchSatisfied);
            Assert.IsTrue(ifMatchSatisfied.Value);
        }

        [Test]
        public void CheckIfMatch_MatchingWildCardAndEntityDoesNotExist_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var entityTag = "abcdef";
            var doesEntityExists = false;
            var headerValue = "*";
            var headerType = HttpRequestHeader.IfMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var ifMatchSatisfied = httpRequestHeaderHelper.CheckIfMatch(httpRequest, entityTag, doesEntityExists);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(ifMatchSatisfied);
            Assert.IsFalse(ifMatchSatisfied.Value);
        }
        #endregion

        #region CheckIfNoneMatch
        [Test]
        public void CheckIfNoneMatch_HasNoIfNoneMatchHeader_Null()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var entityTag = "1234567";
            var doesEntityExists = true;
            var headerValue = string.Empty;
            var headerType = HttpRequestHeader.IfNoneMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var ifMatchSatisfied = httpRequestHeaderHelper.CheckIfNoneMatch(httpRequest, entityTag, doesEntityExists);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNull(ifMatchSatisfied);
        }

        [Test]
        public void CheckIfNoneMatch_HasMatchingEtag_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var entityTag = "1234567";
            var doesEntityExists = true;
            var headerValue = entityTag;
            var headerType = HttpRequestHeader.IfNoneMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var ifMatchSatisfied = httpRequestHeaderHelper.CheckIfNoneMatch(httpRequest, entityTag, doesEntityExists);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(ifMatchSatisfied);
            Assert.IsFalse(ifMatchSatisfied.Value);
        }

        [Test]
        public void CheckIfNoneMatch_HasMatchingEtags_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var entityTag = "1234567";
            var doesEntityExists = true;
            var headerValue = "7654321," + entityTag;
            var headerType = HttpRequestHeader.IfNoneMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);
            
            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var ifMatchSatisfied = httpRequestHeaderHelper.CheckIfNoneMatch(httpRequest, entityTag, doesEntityExists);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(ifMatchSatisfied);
            Assert.IsFalse(ifMatchSatisfied.Value);
        }

        [Test]
        public void CheckIfNoneMatch_HasNoMatchingEtag_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var entityTag = "1234567";
            var doesEntityExists = true;
            var headerValue = entityTag + "8";
            var headerType = HttpRequestHeader.IfNoneMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var ifMatchSatisfied = httpRequestHeaderHelper.CheckIfNoneMatch(httpRequest, entityTag, doesEntityExists);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(ifMatchSatisfied);
            Assert.IsTrue(ifMatchSatisfied.Value);
        }

        [Test]
        public void CheckIfNoneMatch_HasNoMatchingEtags_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var entityTag = "1234567";
            var doesEntityExists = true;
            var headerValue = "7654321," + entityTag + "8";
            var headerType = HttpRequestHeader.IfNoneMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var ifMatchSatisfied = httpRequestHeaderHelper.CheckIfNoneMatch(httpRequest, entityTag, doesEntityExists);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(ifMatchSatisfied);
            Assert.IsTrue(ifMatchSatisfied.Value);
        }

        [Test]
        public void CheckIfNoneMatch_IsCaseSensitive_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var entityTag = "abcdef";
            var doesEntityExists = true;
            var headerValue = "abcdeF";
            var headerType = HttpRequestHeader.IfNoneMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var ifMatchSatisfied = httpRequestHeaderHelper.CheckIfNoneMatch(httpRequest, entityTag, doesEntityExists);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(ifMatchSatisfied);
            Assert.IsTrue(ifMatchSatisfied.Value);
        }

        [Test]
        public void CheckIfNoneMatch_MatchingWildCardAndEntityDoesExist_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var entityTag = "abcdef";
            var doesEntityExists = true;
            var headerValue = "*";
            var headerType = HttpRequestHeader.IfNoneMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var ifMatchSatisfied = httpRequestHeaderHelper.CheckIfNoneMatch(httpRequest, entityTag, doesEntityExists);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(ifMatchSatisfied);
            Assert.IsFalse(ifMatchSatisfied.Value);
        }

        [Test]
        public void CheckIfNoneMatch_MatchingWildCardAndEntityDoesNotExist_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var entityTag = "abcdef";
            var doesEntityExists = false;
            var headerValue = "*";
            var headerType = HttpRequestHeader.IfNoneMatch;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            var ifMatchSatisfied = httpRequestHeaderHelper.CheckIfNoneMatch(httpRequest, entityTag, doesEntityExists);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNotNull(ifMatchSatisfied);
            Assert.IsTrue(ifMatchSatisfied.Value);
        }
        #endregion

        #region GetRanges
        [Test]
        public void GetRanges_NoHeaderSent_Null()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var contentLength = 10000;
            var headerValue = string.Empty;
            var headerType = HttpRequestHeader.Range;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            IEnumerable<RangeItem> ranges;
            var rangeResult = httpRequestHeaderHelper.GetRanges(httpRequest, contentLength, out ranges);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.IsNull(rangeResult);
            Assert.IsNotNull(ranges);
            Assert.IsFalse(ranges.Any());
        }

        [Test]
        public void GetRanges_InvalidStartRange_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var contentLength = 10000;
            var headerValue = "bytes=unknown-9999";
            var headerType = HttpRequestHeader.Range;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            IEnumerable<RangeItem> ranges;
            var rangeResult = httpRequestHeaderHelper.GetRanges(httpRequest, contentLength, out ranges);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.NotNull(rangeResult);
            Assert.IsFalse(rangeResult.Value);
            Assert.IsNull(ranges);
        }

        [Test]
        public void GetRanges_InvalidEndRange_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var contentLength = 10000;
            var headerValue = "bytes=0-unknown";
            var headerType = HttpRequestHeader.Range;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);
            
            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            IEnumerable<RangeItem> ranges;
            var rangeResult = httpRequestHeaderHelper.GetRanges(httpRequest, contentLength, out ranges);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.NotNull(rangeResult);
            Assert.IsFalse(rangeResult.Value);
            Assert.IsNull(ranges);
        }

        [Test]
        public void GetRanges_MalformedRange_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var contentLength = 10000;
            var headerValue = "bytes=0-499,5=100";
            var headerType = HttpRequestHeader.Range;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            IEnumerable<RangeItem> ranges;
            var rangeResult = httpRequestHeaderHelper.GetRanges(httpRequest, contentLength, out ranges);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.NotNull(rangeResult);
            Assert.IsFalse(rangeResult.Value);
            Assert.IsNull(ranges);
        }

        [Test]
        public void GetRanges_StartRangeLargerThenEndRange_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var contentLength = 10000;
            var headerValue = "bytes=499-0";
            var headerType = HttpRequestHeader.Range;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);
            
            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            IEnumerable<RangeItem> ranges;
            var rangeResult = httpRequestHeaderHelper.GetRanges(httpRequest, contentLength, out ranges);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.NotNull(rangeResult);
            Assert.IsFalse(rangeResult.Value);
            Assert.IsNull(ranges);
        }

        [Test]
        public void GetRanges_EndRangeLargerThenEntityLength_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var contentLength = 10000;
            var headerValue = "bytes=0-20000";
            var headerType = HttpRequestHeader.Range;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            IEnumerable<RangeItem> ranges;
            var rangeResult = httpRequestHeaderHelper.GetRanges(httpRequest, contentLength, out ranges);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.NotNull(rangeResult);
            Assert.IsFalse(rangeResult.Value);
            Assert.IsNull(ranges);
        }

        [Test]
        public void GetRanges_NegativeStartRange_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var contentLength = 10000;
            var headerValue = "bytes=-20000";
            var headerType = HttpRequestHeader.Range;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            IEnumerable<RangeItem> ranges;
            var rangeResult = httpRequestHeaderHelper.GetRanges(httpRequest, contentLength, out ranges);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.NotNull(rangeResult);
            Assert.IsFalse(rangeResult.Value);
            Assert.IsNull(ranges);
        }

        [Test]
        public void GetRanges_StartRangeLargerThenEntityLength_False()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var contentLength = 10000;
            var headerValue = "bytes=20000-";
            var headerType = HttpRequestHeader.Range;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            IEnumerable<RangeItem> ranges;
            var rangeResult = httpRequestHeaderHelper.GetRanges(httpRequest, contentLength, out ranges);

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.NotNull(rangeResult);
            Assert.IsFalse(rangeResult.Value);
            Assert.IsNull(ranges);
        }

        [Test]
        public void GetRanges_First500Bytes_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var contentLength = 10000;
            var headerValue = "bytes=0-499";
            var headerType = HttpRequestHeader.Range;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            IEnumerable<RangeItem> ranges;
            var rangeResult = httpRequestHeaderHelper.GetRanges(httpRequest, contentLength, out ranges);
            var rangeItem = ranges.First();

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.NotNull(rangeResult);
            Assert.IsTrue(rangeResult.Value);
            
            Assert.IsNotNull(ranges);
            Assert.IsTrue(ranges.Any());
            Assert.AreEqual(0, rangeItem.StartRange);
            Assert.AreEqual(499, rangeItem.EndRange);
        }

        [Test]
        public void GetRanges_Second500Bytes_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var contentLength = 10000;
            var headerValue = "bytes=500-999";
            var headerType = HttpRequestHeader.Range;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);
            
            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            IEnumerable<RangeItem> ranges;
            var rangeResult = httpRequestHeaderHelper.GetRanges(httpRequest, contentLength, out ranges);
            var rangeItem = ranges.First();

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.NotNull(rangeResult);
            Assert.IsTrue(rangeResult.Value);

            Assert.IsNotNull(ranges);
            Assert.IsTrue(ranges.Any());
            Assert.AreEqual(500, rangeItem.StartRange);
            Assert.AreEqual(999, rangeItem.EndRange);
        }

        [Test]
        public void GetRanges_MultipleByteRanges_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var contentLength = 10000;
            var headerValue = "bytes=0-499,500-999";
            var headerType = HttpRequestHeader.Range;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            IEnumerable<RangeItem> ranges;
            var rangeResult = httpRequestHeaderHelper.GetRanges(httpRequest, contentLength, out ranges);
            var firstRangeItem = ranges.First();
            var secondRangeItem = ranges.Skip(1).First();

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.NotNull(rangeResult);
            Assert.IsTrue(rangeResult.Value);

            Assert.IsNotNull(ranges);
            Assert.IsTrue(ranges.Any());
            Assert.AreEqual(0, firstRangeItem.StartRange);
            Assert.AreEqual(499, firstRangeItem.EndRange);
            Assert.AreEqual(500, secondRangeItem.StartRange);
            Assert.AreEqual(999, secondRangeItem.EndRange);
        }

        [Test]
        public void GetRanges_500BytesFromEnd_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var contentLength = 10000;
            var headerValue = "bytes=-500";
            var headerType = HttpRequestHeader.Range;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            IEnumerable<RangeItem> ranges;
            var rangeResult = httpRequestHeaderHelper.GetRanges(httpRequest, contentLength, out ranges);
            var rangeItem = ranges.First();

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.NotNull(rangeResult);
            Assert.IsTrue(rangeResult.Value);

            Assert.IsNotNull(ranges);
            Assert.IsTrue(ranges.Any());
            Assert.AreEqual(9500, rangeItem.StartRange);
            Assert.AreEqual(9999, rangeItem.EndRange);
       }

        [Test]
        public void GetRanges_9500BytesToEnd_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var contentLength = 10000;
            var headerValue = "bytes=9500-";
            var headerType = HttpRequestHeader.Range;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            IEnumerable<RangeItem> ranges;
            var rangeResult = httpRequestHeaderHelper.GetRanges(httpRequest, contentLength, out ranges);
            var rangeItem = ranges.First();

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.NotNull(rangeResult);
            Assert.IsTrue(rangeResult.Value);

            Assert.IsNotNull(ranges);
            Assert.IsTrue(ranges.Any());
            Assert.AreEqual(9500, rangeItem.StartRange);
            Assert.AreEqual(9999, rangeItem.EndRange);
        }

        [Test]
        public void GetRanges_FirstByte_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var contentLength = 10000;
            var headerValue = "bytes=0-0";
            var headerType = HttpRequestHeader.Range;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            IEnumerable<RangeItem> ranges;
            var rangeResult = httpRequestHeaderHelper.GetRanges(httpRequest, contentLength, out ranges);
            var rangeItem = ranges.First();

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.NotNull(rangeResult);
            Assert.IsTrue(rangeResult.Value);

            Assert.IsNotNull(ranges);
            Assert.IsTrue(ranges.Any());
            Assert.AreEqual(0, rangeItem.StartRange);
            Assert.AreEqual(0, rangeItem.EndRange);
        }

        [Test]
        public void GetRanges_LastByte_True()
        {
            //Arrange
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var contentLength = 10000;
            var headerValue = "bytes=-1";
            var headerType = HttpRequestHeader.Range;
            var headerName = (string)headerType;

            httpRequest.Expect(x => x.Headers[headerName]).Return(headerValue);

            //Act
            var httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            IEnumerable<RangeItem> ranges;
            var rangeResult = httpRequestHeaderHelper.GetRanges(httpRequest, contentLength, out ranges);
            var rangeItem = ranges.First();

            //Assert
            httpRequest.VerifyAllExpectations();
            Assert.NotNull(rangeResult);
            Assert.IsTrue(rangeResult.Value);

            Assert.IsNotNull(ranges);
            Assert.IsTrue(ranges.Any());
            Assert.AreEqual(9999, rangeItem.StartRange);
            Assert.AreEqual(9999, rangeItem.EndRange);
        }
        #endregion
    }
}
