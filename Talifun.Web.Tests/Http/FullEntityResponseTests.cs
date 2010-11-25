using System;
using System.IO;
using System.IO.Compression;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;

namespace Talifun.Web.Tests.Http
{
    [TestFixture]
    public class FullEntityResponseTests
    {
        #region SendHeaders
        [Test]
        public void SendHeaders_EntityCompressionMatchesRequestedCompression_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
            var entity = MockRepository.GenerateMock<IEntity>();
            
            var contentLength = 1000l;
            var entityCompressionType = ResponseCompressionType.None;
            var responseCompressionType = ResponseCompressionType.None;

            entity.Stub(x => x.ContentLength).Return(contentLength);
            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            httpResponseHeaderHelper.Expect(x => x.SetContentEncoding(httpResponse, responseCompressionType));
            httpResponseHeaderHelper.Expect(x => x.AppendHeader(httpResponse, FullEntityResponse.HttpHeaderContentLength, contentLength.ToString()));
            httpResponse.Expect(x => x.ContentType = entity.ContentType);

            //Act
            var responseForFullEntityStrategy = new FullEntityResponse(httpResponseHeaderHelper);
            responseForFullEntityStrategy.SendHeaders(httpResponse,responseCompressionType, entity);

            //Assert
            httpResponseHeaderHelper.VerifyAllExpectations();
            httpResponse.VerifyAllExpectations();
        }

        [Test]
        public void SendHeaders_EntityCompressionNoneRequestedCompressionGzip_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
            var entity = MockRepository.GenerateMock<IEntity>();

            var entityCompressionType = ResponseCompressionType.None;
            var responseCompressionType = ResponseCompressionType.GZip;

            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            httpResponseHeaderHelper.Expect(x => x.SetContentEncoding(httpResponse, responseCompressionType));
            httpResponse.Expect(x => x.ContentType = entity.ContentType);

            httpResponse.Stub(x => x.Filter).PropertyBehavior();
            httpResponse.Filter = new MemoryStream();

            //Act
            var responseForFullEntityStrategy = new FullEntityResponse(httpResponseHeaderHelper);
            responseForFullEntityStrategy.SendHeaders(httpResponse, responseCompressionType, entity);

            //Assert
            httpResponseHeaderHelper.VerifyAllExpectations();
            httpResponse.VerifyAllExpectations();

            Assert.IsInstanceOf(typeof(GZipStream), httpResponse.Filter);
        }

        [Test]
        public void SendHeaders_EntityCompressionNoneRequestedCompressionDeflate_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
            var entity = MockRepository.GenerateMock<IEntity>();

            var entityCompressionType = ResponseCompressionType.None;
            var responseCompressionType = ResponseCompressionType.Deflate;
            
            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            httpResponseHeaderHelper.Expect(x => x.SetContentEncoding(httpResponse, responseCompressionType));
            httpResponse.Expect(x => x.ContentType = entity.ContentType);

            httpResponse.Stub(x => x.Filter).PropertyBehavior();
            httpResponse.Filter = new MemoryStream();

            //Act
            var responseForFullEntityStrategy = new FullEntityResponse(httpResponseHeaderHelper);
            responseForFullEntityStrategy.SendHeaders(httpResponse, responseCompressionType, entity);

            //Assert
            httpResponseHeaderHelper.VerifyAllExpectations();
            httpResponse.VerifyAllExpectations();

            Assert.IsInstanceOf(typeof(DeflateStream), httpResponse.Filter);
        }

        [Test]
        public void SendHeaders_EntityCompressionGzipRequestedCompressionNone_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
            var entity = MockRepository.GenerateMock<IEntity>();
            
            var entityCompressionType = ResponseCompressionType.GZip;
            var responseCompressionType = ResponseCompressionType.None;

            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            //Act
            var responseForFullEntityStrategy = new FullEntityResponse(httpResponseHeaderHelper);
            var ex = Assert.Throws<NotImplementedException>(() => responseForFullEntityStrategy.SendHeaders(httpResponse, responseCompressionType, entity));
        }

        [Test]
        public void SendHeaders_EntityCompressionGzipRequestedCompressionDeflate_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
            var entity = MockRepository.GenerateMock<IEntity>();

            var entityCompressionType = ResponseCompressionType.GZip;
            var responseCompressionType = ResponseCompressionType.Deflate;

            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            //Act
            var responseForFullEntityStrategy = new FullEntityResponse(httpResponseHeaderHelper);
            var ex = Assert.Throws<NotImplementedException>(() => responseForFullEntityStrategy.SendHeaders(httpResponse, responseCompressionType, entity));
        }

        [Test]
        public void SendHeaders_EntityCompressionDeflateRequestedCompressionNone_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
            var entity = MockRepository.GenerateMock<IEntity>();

            var entityCompressionType = ResponseCompressionType.Deflate;
            var responseCompressionType = ResponseCompressionType.None;

            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            //Act
            var responseForFullEntityStrategy = new FullEntityResponse(httpResponseHeaderHelper);
            var ex = Assert.Throws<NotImplementedException>(() => responseForFullEntityStrategy.SendHeaders(httpResponse, responseCompressionType, entity));
        }

        [Test]
        public void SendHeaders_EntityCompressionDeflateRequestedCompressionGzip_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
            var entity = MockRepository.GenerateMock<IEntity>();

            var entityCompressionType = ResponseCompressionType.Deflate;
            var responseCompressionType = ResponseCompressionType.GZip;

            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            //Act
            var responseForFullEntityStrategy = new FullEntityResponse(httpResponseHeaderHelper);
            var ex = Assert.Throws<NotImplementedException>(() => responseForFullEntityStrategy.SendHeaders(httpResponse, responseCompressionType, entity));
        }

        #endregion

        #region SendBody
        [Test]
        public void SendBody_GetRequest_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
            var transmitEntityStrategy = MockRepository.GenerateMock<ITransmitEntityStrategy>();
            var requestHttpMethod = HttpMethod.Get;

            //Act
            var responseForFullEntityStrategy = new FullEntityResponse(httpResponseHeaderHelper);
            responseForFullEntityStrategy.SendBody(requestHttpMethod, httpResponse, transmitEntityStrategy);

            //Assert
            transmitEntityStrategy.AssertWasCalled(x => x.Transmit(httpResponse));
        }

        [Test]
        public void SendBody_HeadRequest_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
            var transmitEntityStrategy = MockRepository.GenerateMock<ITransmitEntityStrategy>();
            var requestHttpMethod = HttpMethod.Head;

            //Act
            var responseForFullEntityStrategy = new FullEntityResponse(httpResponseHeaderHelper);
            responseForFullEntityStrategy.SendBody(requestHttpMethod, httpResponse, transmitEntityStrategy);

            //Assert
            transmitEntityStrategy.AssertWasNotCalled(x => x.Transmit(httpResponse));
        }

        [Test]
        public void SendBody_NotAHeadOrGetRequest_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
            var transmitEntityStrategy = MockRepository.GenerateMock<ITransmitEntityStrategy>();
            var requestHttpMethod = HttpMethod.Options;

            //Act
            var responseForFullEntityStrategy = new FullEntityResponse(httpResponseHeaderHelper);
        
            var ex = Assert.Throws<Exception>(() => responseForFullEntityStrategy.SendBody(requestHttpMethod, httpResponse, transmitEntityStrategy));
        }

        #endregion
    }
}
