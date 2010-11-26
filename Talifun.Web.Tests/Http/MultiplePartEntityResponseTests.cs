using System;
using System.IO;
using System.IO.Compression;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;

namespace Talifun.Web.Tests.Http
{
    [TestFixture]
    public class MultiPartEntityResponseTests
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
            var entityType = "image/gif";
            var entityCompressionType = ResponseCompressionType.None;
            
            var firstStartRange = 0l;
            var firstEndRange = 499l;
            var firstRangeItem = new RangeItem
            {
                StartRange = firstStartRange,
                EndRange = firstEndRange
            };

            var secondStartRange = 500l;
            var secondEndRange = 999l;
            var secondRangeItem = new RangeItem
            {
                StartRange = secondStartRange,
                EndRange = secondEndRange
            };

            var rangeItems = new[] { firstRangeItem, secondRangeItem };

            var firstHeader = "\r\n--" + MultiPartEntityResponse.MultipartBoundary + "\r\n"
                + MultiPartEntityResponse.HttpHeaderContentType + ": " + entityType + "\r\n"
                + MultiPartEntityResponse.HttpHeaderContentRange + ": " + MultiPartEntityResponse.Bytes + " " + firstStartRange + "-" + firstEndRange + "/" + entityLength + "\r\n"
                + "\r\n";

            var secondHeader = "\r\n--" + MultiPartEntityResponse.MultipartBoundary + "\r\n"
                + MultiPartEntityResponse.HttpHeaderContentType + ": " + entityType + "\r\n"
                + MultiPartEntityResponse.HttpHeaderContentRange + ": " + MultiPartEntityResponse.Bytes + " " + secondStartRange + "-" + secondEndRange + "/" + entityLength + "\r\n"
                + "\r\n";

            var footer = "\r\n--" + MultiPartEntityResponse.MultipartBoundary + "--\r\n";

            var contentLength = firstHeader.Length + firstEndRange - firstStartRange + 1
                + secondHeader.Length + secondEndRange - secondStartRange + 1
                + footer.Length;

            entity.Stub(x => x.ContentType).Return(entityType);
            entity.Stub(x => x.ContentLength).Return(entityLength);
            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            httpResponseHeaderHelper.Expect(x => x.SetContentEncoding(httpResponse, responseCompressionType));
            httpResponseHeaderHelper.Expect(x => x.AppendHeader(httpResponse, MultiPartEntityResponse.HttpHeaderContentLength, contentLength.ToString()));
            httpResponse.Expect(x => x.ContentType = MultiPartEntityResponse.MultipartContenttype);

            //Act
            var multiPartEntityResponse = new MultiPartEntityResponse(httpResponseHeaderHelper, rangeItems);
            multiPartEntityResponse.SendHeaders(httpResponse, responseCompressionType, entity);

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

            var firstStartRange = 0l;
            var firstEndRange = 499l;
            var firstRangeItem = new RangeItem
            {
                StartRange = firstStartRange,
                EndRange = firstEndRange
            };

            var secondStartRange = 500l;
            var secondEndRange = 999l;
            var secondRangeItem = new RangeItem
            {
                StartRange = secondStartRange,
                EndRange = secondEndRange
            };

            var rangeItems = new[] { firstRangeItem, secondRangeItem };

            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            httpResponse.Stub(x => x.Filter).PropertyBehavior();
            httpResponse.Filter = new MemoryStream();

            httpResponseHeaderHelper.Expect(x => x.SetContentEncoding(httpResponse, responseCompressionType));
            httpResponse.Expect(x => x.ContentType = MultiPartEntityResponse.MultipartContenttype);

            //Act
            var multiPartEntityResponse = new MultiPartEntityResponse(httpResponseHeaderHelper, rangeItems);
            multiPartEntityResponse.SendHeaders(httpResponse, responseCompressionType, entity);

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

            var firstStartRange = 0l;
            var firstEndRange = 499l;
            var firstRangeItem = new RangeItem
            {
                StartRange = firstStartRange,
                EndRange = firstEndRange
            };

            var secondStartRange = 500l;
            var secondEndRange = 999l;
            var secondRangeItem = new RangeItem
            {
                StartRange = secondStartRange,
                EndRange = secondEndRange
            };

            var rangeItems = new[] { firstRangeItem, secondRangeItem };

            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            httpResponse.Stub(x => x.Filter).PropertyBehavior();
            httpResponse.Filter = new MemoryStream();

            httpResponseHeaderHelper.Expect(x => x.SetContentEncoding(httpResponse, responseCompressionType));
            httpResponse.Expect(x => x.ContentType = MultiPartEntityResponse.MultipartContenttype);

            //Act
            var multiPartEntityResponse = new MultiPartEntityResponse(httpResponseHeaderHelper, rangeItems);
            multiPartEntityResponse.SendHeaders(httpResponse, responseCompressionType, entity);

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
            var responseCompressionType = ResponseCompressionType.None;
            var entityCompressionType = ResponseCompressionType.GZip;

            var firstStartRange = 0l;
            var firstEndRange = 499l;
            var firstRangeItem = new RangeItem
            {
                StartRange = firstStartRange,
                EndRange = firstEndRange
            };

            var secondStartRange = 500l;
            var secondEndRange = 999l;
            var secondRangeItem = new RangeItem
            {
                StartRange = secondStartRange,
                EndRange = secondEndRange
            };

            var rangeItems = new[] { firstRangeItem, secondRangeItem };

            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            //Act
            var multiPartEntityResponse = new MultiPartEntityResponse(httpResponseHeaderHelper, rangeItems);
            var ex = Assert.Throws<Exception>(() => multiPartEntityResponse.SendHeaders(httpResponse, responseCompressionType, entity));
        }

        [Test]
        public void SendHeaders_EntityCompressionGzipRequestedCompressionDeflate_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
            var entity = MockRepository.GenerateMock<IEntity>();
            var responseCompressionType = ResponseCompressionType.Deflate;
            var entityCompressionType = ResponseCompressionType.GZip;

            var firstStartRange = 0l;
            var firstEndRange = 499l;
            var firstRangeItem = new RangeItem
            {
                StartRange = firstStartRange,
                EndRange = firstEndRange
            };

            var secondStartRange = 500l;
            var secondEndRange = 999l;
            var secondRangeItem = new RangeItem
            {
                StartRange = secondStartRange,
                EndRange = secondEndRange
            };

            var rangeItems = new[] { firstRangeItem, secondRangeItem };

            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            //Act
            var multiPartEntityResponse = new MultiPartEntityResponse(httpResponseHeaderHelper, rangeItems);
            var ex = Assert.Throws<Exception>(() => multiPartEntityResponse.SendHeaders(httpResponse, responseCompressionType, entity));
        }

        [Test]
        public void SendHeaders_EntityCompressionDeflateRequestedCompressionNone_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
            var entity = MockRepository.GenerateMock<IEntity>();
            var responseCompressionType = ResponseCompressionType.None;
            var entityCompressionType = ResponseCompressionType.Deflate;

            var firstStartRange = 0l;
            var firstEndRange = 499l;
            var firstRangeItem = new RangeItem
            {
                StartRange = firstStartRange,
                EndRange = firstEndRange
            };

            var secondStartRange = 500l;
            var secondEndRange = 999l;
            var secondRangeItem = new RangeItem
            {
                StartRange = secondStartRange,
                EndRange = secondEndRange
            };

            var rangeItems = new[] { firstRangeItem, secondRangeItem };

            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            //Act
            var multiPartEntityResponse = new MultiPartEntityResponse(httpResponseHeaderHelper, rangeItems);
            var ex = Assert.Throws<Exception>(() => multiPartEntityResponse.SendHeaders(httpResponse, responseCompressionType, entity));
        }

        [Test]
        public void SendHeaders_EntityCompressionDeflateRequestedCompressionGzip_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
            var entity = MockRepository.GenerateMock<IEntity>();
            var responseCompressionType = ResponseCompressionType.GZip;
            var entityCompressionType = ResponseCompressionType.Deflate;

            var firstStartRange = 0l;
            var firstEndRange = 499l;
            var firstRangeItem = new RangeItem
            {
                StartRange = firstStartRange,
                EndRange = firstEndRange
            };

            var secondStartRange = 500l;
            var secondEndRange = 999l;
            var secondRangeItem = new RangeItem
            {
                StartRange = secondStartRange,
                EndRange = secondEndRange
            };

            var rangeItems = new[] { firstRangeItem, secondRangeItem };

            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            //Act
            var multiPartEntityResponse = new MultiPartEntityResponse(httpResponseHeaderHelper, rangeItems);
            var ex = Assert.Throws<Exception>(() => multiPartEntityResponse.SendHeaders(httpResponse, responseCompressionType, entity));
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
            var entity = MockRepository.GenerateMock<IEntity>();
            var output = MockRepository.GenerateMock<TextWriter>();
            var requestHttpMethod = HttpMethod.Get;

            var contentType = "image/gif";
            var contentLength = 1000l;

            var firstStartRange = 0l;
            var firstEndRange = 499l;
            var firstBytesToRead = firstEndRange - firstStartRange + 1;
            var firstRangeItem = new RangeItem
            {
                StartRange = firstStartRange,
                EndRange = firstEndRange
            };

            var secondStartRange = 500l;
            var secondEndRange = 999l;
            var secondBytesToRead = secondEndRange - secondStartRange + 1;
            var secondRangeItem = new RangeItem
            {
                StartRange = secondStartRange,
                EndRange = secondEndRange
            };

            var rangeItems = new[] { firstRangeItem, secondRangeItem };

            var firstHeader = "\r\n--" + MultiPartEntityResponse.MultipartBoundary + "\r\n"
                + MultiPartEntityResponse.HttpHeaderContentType + ": " + contentType + "\r\n"
                + MultiPartEntityResponse.HttpHeaderContentRange + ": " + MultiPartEntityResponse.Bytes + " " + firstStartRange + "-" + firstEndRange + "/" + contentLength + "\r\n"
                + "\r\n";

            var secondHeader = "\r\n--" + MultiPartEntityResponse.MultipartBoundary + "\r\n"
                + MultiPartEntityResponse.HttpHeaderContentType + ": " + contentType + "\r\n"
                + MultiPartEntityResponse.HttpHeaderContentRange + ": " + MultiPartEntityResponse.Bytes + " " + secondStartRange + "-" + secondEndRange + "/" + contentLength + "\r\n"
                + "\r\n";

            var footer = "\r\n--" + MultiPartEntityResponse.MultipartBoundary + "--\r\n";

            transmitEntityStrategy.Stub(x => x.Entity).Return(entity);
            entity.Stub(x => x.ContentType).Return(contentType);
            entity.Stub(x => x.ContentLength).Return(contentLength);
            httpResponse.Stub(x => x.Output).Return(output);

            //Act
            var multiPartEntityResponse = new MultiPartEntityResponse(httpResponseHeaderHelper, rangeItems);
            multiPartEntityResponse.SendBody(requestHttpMethod, httpResponse, transmitEntityStrategy);

            //Assert
            output.AssertWasCalled(x => x.Write(firstHeader));
            transmitEntityStrategy.AssertWasCalled(x => x.Transmit(httpResponse, firstStartRange, firstBytesToRead));

            output.AssertWasCalled(x => x.Write(secondHeader));
            transmitEntityStrategy.AssertWasCalled(x => x.Transmit(httpResponse, secondStartRange, secondBytesToRead));

            output.AssertWasCalled(x => x.Write(footer));
        }

        [Test]
        public void SendBody_HeadRequest_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
            var transmitEntityStrategy = MockRepository.GenerateMock<ITransmitEntityStrategy>();
            var entity = MockRepository.GenerateMock<IEntity>();
            var output = MockRepository.GenerateMock<TextWriter>();
            var requestHttpMethod = HttpMethod.Head;

            var contentType = "image/gif";
            var contentLength = 1000l;

            var firstStartRange = 0l;
            var firstEndRange = 499l;
            var firstBytesToRead = firstEndRange - firstStartRange + 1;
            var firstRangeItem = new RangeItem
            {
                StartRange = firstStartRange,
                EndRange = firstEndRange
            };

            var secondStartRange = 500l;
            var secondEndRange = 999l;
            var secondBytesToRead = secondEndRange - secondStartRange + 1;
            var secondRangeItem = new RangeItem
            {
                StartRange = secondStartRange,
                EndRange = secondEndRange
            };

            var rangeItems = new[] { firstRangeItem, secondRangeItem };

            var firstHeader = "\r\n--" + MultiPartEntityResponse.MultipartBoundary + "\r\n"
                + MultiPartEntityResponse.HttpHeaderContentType + ": " + contentType + "\r\n"
                + MultiPartEntityResponse.HttpHeaderContentRange + ": " + MultiPartEntityResponse.Bytes + " " + firstStartRange + "-" + firstEndRange + "/" + contentLength + "\r\n"
                + "\r\n";

            var secondHeader = "\r\n--" + MultiPartEntityResponse.MultipartBoundary + "\r\n"
                + MultiPartEntityResponse.HttpHeaderContentType + ": " + contentType + "\r\n"
                + MultiPartEntityResponse.HttpHeaderContentRange + ": " + MultiPartEntityResponse.Bytes + " " + secondStartRange + "-" + secondEndRange + "/" + contentLength + "\r\n"
                + "\r\n";

            var footer = "\r\n--" + MultiPartEntityResponse.MultipartBoundary + "--\r\n";

            transmitEntityStrategy.Stub(x => x.Entity).Return(entity);
            entity.Stub(x => x.ContentType).Return(contentType);
            entity.Stub(x => x.ContentLength).Return(contentLength);
            httpResponse.Stub(x => x.Output).Return(output);

            //Act
            var multiPartEntityResponse = new MultiPartEntityResponse(httpResponseHeaderHelper, rangeItems);
            multiPartEntityResponse.SendBody(requestHttpMethod, httpResponse, transmitEntityStrategy);

            //Assert
            output.AssertWasNotCalled(x => x.Write(firstHeader));
            transmitEntityStrategy.AssertWasNotCalled(x => x.Transmit(httpResponse, firstStartRange, firstBytesToRead));

            output.AssertWasNotCalled(x => x.Write(secondHeader));
            transmitEntityStrategy.AssertWasNotCalled(x => x.Transmit(httpResponse, secondStartRange, secondBytesToRead));

            output.AssertWasNotCalled(x => x.Write(footer));
        }

        [Test]
        public void SendBody_NotAHeadOrGetRequest_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
            var transmitEntityStrategy = MockRepository.GenerateMock<ITransmitEntityStrategy>();
            var requestHttpMethod = HttpMethod.Options;

            var firstStartRange = 0l;
            var firstEndRange = 499l;

            var firstRangeItem = new RangeItem
            {
                StartRange = firstStartRange,
                EndRange = firstEndRange
            };

            var secondStartRange = 500l;
            var secondEndRange = 999l;

            var secondRangeItem = new RangeItem
            {
                StartRange = secondStartRange,
                EndRange = secondEndRange
            };

            var rangeItems = new[] { firstRangeItem, secondRangeItem };

            //Act
            var multiPartEntityResponse = new MultiPartEntityResponse(httpResponseHeaderHelper, rangeItems);
            var ex = Assert.Throws<Exception>(() => multiPartEntityResponse.SendBody(requestHttpMethod, httpResponse, transmitEntityStrategy));
        }

        #endregion
    }
}
