using System;
using System.IO;
using System.IO.Compression;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;

namespace Talifun.Web.Tests.Http
{
    [TestFixture]
    public class SinglePartEntityResponseTests
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
            var entityLength = 1000l;
            var entityCompressionType = ResponseCompressionType.None;

            var startRange = 0l;
            var endRange = 499l;
            var rangeItem = new RangeItem
            {
                StartRange = startRange,
                EndRange = endRange
            };
            var contentLength = endRange - startRange + 1;

            entity.Stub(x => x.ContentLength).Return(entityLength);
            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            httpResponseHeaderHelper.Expect(x => x.SetContentEncoding(httpResponse, responseCompressionType));
            httpResponseHeaderHelper.Expect(x => x.AppendHeader(httpResponse, SinglePartEntityResponse.HttpHeaderContentLength, contentLength.ToString()));
            httpResponse.Expect(x => x.ContentType = entity.ContentType);

            //Act
            var singlePartEntityResponse = new SinglePartEntityResponse(httpResponseHeaderHelper, rangeItem);
            singlePartEntityResponse.SendHeaders(httpResponse, responseCompressionType, entity);

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

            var startRange = 0l;
            var endRange = 499l;
            var rangeItem = new RangeItem
            {
                StartRange = startRange,
                EndRange = endRange
            };

            httpResponse.Stub(x => x.Filter).PropertyBehavior();
            httpResponse.Filter = new MemoryStream();

            httpResponseHeaderHelper.Expect(x => x.SetContentEncoding(httpResponse, responseCompressionType));
            httpResponse.Expect(x => x.ContentType = entity.ContentType);

            //Act
            var singlePartEntityResponse = new SinglePartEntityResponse(httpResponseHeaderHelper, rangeItem);
            singlePartEntityResponse.SendHeaders(httpResponse, responseCompressionType, entity);

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

            var startRange = 0l;
            var endRange = 499l;
            var rangeItem = new RangeItem
            {
                StartRange = startRange,
                EndRange = endRange
            };

            httpResponse.Stub(x => x.Filter).PropertyBehavior();
            httpResponse.Filter = new MemoryStream();

            httpResponseHeaderHelper.Expect(x => x.SetContentEncoding(httpResponse, responseCompressionType));
            httpResponse.Expect(x => x.ContentType = entity.ContentType);

            //Act
            var singlePartEntityResponse = new SinglePartEntityResponse(httpResponseHeaderHelper, rangeItem);
            singlePartEntityResponse.SendHeaders(httpResponse, responseCompressionType, entity);

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

            var startRange = 0l;
            var endRange = 499l;
            var rangeItem = new RangeItem
            {
                StartRange = startRange,
                EndRange = endRange
            };

            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            //Act
            var singlePartEntityResponse = new SinglePartEntityResponse(httpResponseHeaderHelper, rangeItem);
            var ex = Assert.Throws<Exception>(() => singlePartEntityResponse.SendHeaders(httpResponse, responseCompressionType, entity));
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

            var startRange = 0l;
            var endRange = 499l;
            var rangeItem = new RangeItem
            {
                StartRange = startRange,
                EndRange = endRange
            };

            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            //Act
            var singlePartEntityResponse = new SinglePartEntityResponse(httpResponseHeaderHelper, rangeItem);
            var ex = Assert.Throws<Exception>(() => singlePartEntityResponse.SendHeaders(httpResponse, responseCompressionType, entity));
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

            var startRange = 0l;
            var endRange = 499l;
            var rangeItem = new RangeItem
            {
                StartRange = startRange,
                EndRange = endRange
            };

            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            //Act
            var singlePartEntityResponse = new SinglePartEntityResponse(httpResponseHeaderHelper, rangeItem);
            var ex = Assert.Throws<Exception>(() => singlePartEntityResponse.SendHeaders(httpResponse, responseCompressionType, entity));
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

            var startRange = 0l;
            var endRange = 499l;
            var rangeItem = new RangeItem
            {
                StartRange = startRange,
                EndRange = endRange
            };

            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            //Act
            var singlePartEntityResponse = new SinglePartEntityResponse(httpResponseHeaderHelper, rangeItem);
            var ex = Assert.Throws<Exception>(() => singlePartEntityResponse.SendHeaders(httpResponse, responseCompressionType, entity));
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

            var startRange = 0l;
            var endRange = 499l;
            var rangeItem = new RangeItem
            {
                StartRange = startRange,
                EndRange = endRange
            };
            var bytesToRead = endRange - startRange + 1;

            //Act
            var singleByteRangeResponse = new SinglePartEntityResponse(httpResponseHeaderHelper, rangeItem);
            singleByteRangeResponse.SendBody(requestHttpMethod, httpResponse, transmitEntityStrategy);

            //Assert
            transmitEntityStrategy.AssertWasCalled(x => x.Transmit(httpResponse, startRange, bytesToRead));
        }

        [Test]
        public void SendBody_HeadRequest_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
            var transmitEntityStrategy = MockRepository.GenerateMock<ITransmitEntityStrategy>();
            var requestHttpMethod = HttpMethod.Head;

            var startRange = 0l;
            var endRange = 499l;
            var rangeItem = new RangeItem
            {
                StartRange = startRange,
                EndRange = endRange
            };

            var bytesToRead = endRange - startRange + 1;

            //Act
            var singleByteRangeResponse = new SinglePartEntityResponse(httpResponseHeaderHelper, rangeItem);
            singleByteRangeResponse.SendBody(requestHttpMethod, httpResponse, transmitEntityStrategy);

            //Assert
            transmitEntityStrategy.AssertWasNotCalled(x => x.Transmit(httpResponse, startRange, bytesToRead));
        }

        [Test]
        public void SendBody_NotAHeadOrGetRequest_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
            var transmitEntityStrategy = MockRepository.GenerateMock<ITransmitEntityStrategy>();
            var requestHttpMethod = HttpMethod.Options;

            var startRange = 0l;
            var endRange = 499l;
            var rangeItem = new RangeItem
            {
                StartRange = startRange,
                EndRange = endRange
            };

            var bytesToRead = endRange - startRange + 1;

            //Act
            var singleByteRangeResponse = new SinglePartEntityResponse(httpResponseHeaderHelper, rangeItem);
            var ex = Assert.Throws<Exception>(() => singleByteRangeResponse.SendBody(requestHttpMethod, httpResponse, transmitEntityStrategy));
        }

        #endregion
    }
}
