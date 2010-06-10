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
            var responseCompressionType = ResponseCompressionType.None;
            var contentLength = 1000l;
            var entityCompressionType = ResponseCompressionType.None;

            entity.Stub(x => x.ContentLength).Return(contentLength);
            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            httpResponseHeaderHelper.Expect(x => x.SetContentEncoding(httpResponse, responseCompressionType));
            httpResponseHeaderHelper.Expect(x => x.AppendHeader(httpResponse, FullEntityResponse.HTTP_HEADER_CONTENT_LENGTH, contentLength.ToString()));
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
            var responseCompressionType = ResponseCompressionType.GZip;
            var entityCompressionType = ResponseCompressionType.None;

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
            var responseCompressionType = ResponseCompressionType.Deflate;
            var entityCompressionType = ResponseCompressionType.None;

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
        #endregion
    }
}
