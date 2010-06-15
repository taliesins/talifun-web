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

            var firstHeader = "\r\n--" + MultiPartEntityResponse.MULTIPART_BOUNDARY + "\r\n"
                + MultiPartEntityResponse.HTTP_HEADER_CONTENT_TYPE + ": " + entityType + "\r\n"
                + MultiPartEntityResponse.HTTP_HEADER_CONTENT_RANGE + ": " + MultiPartEntityResponse.BYTES + " " + firstStartRange + "-" + firstEndRange + "/" + entityLength + "\r\n"
                + "\r\n";

            var secondHeader = "\r\n--" + MultiPartEntityResponse.MULTIPART_BOUNDARY + "\r\n"
                + MultiPartEntityResponse.HTTP_HEADER_CONTENT_TYPE + ": " + entityType + "\r\n"
                + MultiPartEntityResponse.HTTP_HEADER_CONTENT_RANGE + ": " + MultiPartEntityResponse.BYTES + " " + secondStartRange + "-" + secondEndRange + "/" + entityLength + "\r\n"
                + "\r\n";

            var footer = "\r\n--" + MultiPartEntityResponse.MULTIPART_BOUNDARY + "--\r\n";

            var contentLength = firstHeader.Length + firstEndRange - firstStartRange + 1
                + secondHeader.Length + secondEndRange - secondStartRange + 1
                + footer.Length;

            entity.Stub(x => x.ContentType).Return(entityType);
            entity.Stub(x => x.ContentLength).Return(entityLength);
            entity.Stub(x => x.CompressionType).Return(entityCompressionType);

            httpResponseHeaderHelper.Expect(x => x.SetContentEncoding(httpResponse, responseCompressionType));
            httpResponseHeaderHelper.Expect(x => x.AppendHeader(httpResponse, MultiPartEntityResponse.HTTP_HEADER_CONTENT_LENGTH, contentLength.ToString()));
            httpResponse.Expect(x => x.ContentType = MultiPartEntityResponse.MULTIPART_CONTENTTYPE);

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
            httpResponse.Expect(x => x.ContentType = MultiPartEntityResponse.MULTIPART_CONTENTTYPE);

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
            httpResponse.Expect(x => x.ContentType = MultiPartEntityResponse.MULTIPART_CONTENTTYPE);

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
        //#region SetContentLength
        //[Test]
        //public void SetContentLength_ContentLengthSet_Void()
        //{
        //    //Arrange
        //    var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
        //    var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
        //    var entity = MockRepository.GenerateMock<IEntity>();

        //    var contentType = "image/gif";
        //    var contentLength = 1000l;

        //    var firstStartRange = 0l;
        //    var firstEndRange = 499l;
        //    var firstRangeItem = new RangeItem
        //    {
        //        StartRange = firstStartRange,
        //        EndRange = firstEndRange
        //    };

        //    var secondStartRange = 500l;
        //    var secondEndRange = 999l;
        //    var secondRangeItem = new RangeItem
        //    {
        //        StartRange = secondStartRange,
        //        EndRange = secondEndRange
        //    };

        //    var rangeItems = new[] { firstRangeItem, secondRangeItem };

        //    var firstHeader = "\r\n--" + MultiPartEntityResponse.MULTIPART_BOUNDARY + "\r\n"
        //        + MultiPartEntityResponse.HTTP_HEADER_CONTENT_TYPE + ": " + contentType + "\r\n"
        //        + MultiPartEntityResponse.HTTP_HEADER_CONTENT_RANGE + ": " + MultiPartEntityResponse.BYTES + " " + firstStartRange + "-" + firstEndRange + "/" + contentLength + "\r\n"
        //        + "\r\n";

        //    var secondHeader = "\r\n--" + MultiPartEntityResponse.MULTIPART_BOUNDARY + "\r\n"
        //        + MultiPartEntityResponse.HTTP_HEADER_CONTENT_TYPE + ": " + contentType + "\r\n"
        //        + MultiPartEntityResponse.HTTP_HEADER_CONTENT_RANGE + ": " + MultiPartEntityResponse.BYTES + " " + secondStartRange + "-" + secondEndRange + "/" + contentLength + "\r\n"
        //        + "\r\n";

        //    var footer = "\r\n--" + MultiPartEntityResponse.MULTIPART_BOUNDARY + "--\r\n";

        //    var responseContentLength = firstHeader.Length + firstEndRange - firstStartRange + 1
        //        + secondHeader.Length + secondEndRange - secondStartRange + 1
        //        + footer.Length;

        //    entity.Stub(x => x.ContentType).Return(contentType);
        //    entity.Stub(x => x.ContentLength).Return(contentLength);

        //    httpResponseHeaderHelper.Expect(x => x.AppendHeader(httpResponse, SingleByteRangeResponse.HTTP_HEADER_CONTENT_LENGTH, responseContentLength.ToString()));

        //    //Act
        //    var MultiPartEntityResponse = new MultiPartEntityResponse(httpResponseHeaderHelper, rangeItems);
        //    MultiPartEntityResponse.SetContentLength(httpResponse, entity);

        //    //Assert
        //    httpResponseHeaderHelper.VerifyAllExpectations();
        //    httpResponse.VerifyAllExpectations();
        //}
        //#endregion

        //#region SetOtherHeaders
        //[Test]
        //public void SetOtherHeaders_OtherHeadersSet_Void()
        //{
        //    //Arrange
        //    var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
        //    var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
        //    var entity = MockRepository.GenerateMock<IEntity>();

        //    var firstStartRange = 0l;
        //    var firstEndRange = 499l;
        //    var firstRangeItem = new RangeItem
        //    {
        //        StartRange = firstStartRange,
        //        EndRange = firstEndRange
        //    };

        //    var secondStartRange = 500l;
        //    var secondEndRange = 999l;
        //    var secondRangeItem = new RangeItem
        //    {
        //        StartRange = secondStartRange,
        //        EndRange = secondEndRange
        //    };

        //    var rangeItems = new[] { firstRangeItem, secondRangeItem };

        //    httpResponse.Expect(x => x.ContentType = MultiPartEntityResponse.MULTIPART_CONTENTTYPE);

        //    //Act
        //    var MultiPartEntityResponse = new MultiPartEntityResponse(httpResponseHeaderHelper, rangeItems);
        //    MultiPartEntityResponse.SetOtherHeaders(httpResponse, entity);

        //    //Assert
        //    httpResponseHeaderHelper.VerifyAllExpectations();
        //    httpResponse.VerifyAllExpectations();
        //}
        //#endregion

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

            var firstHeader = "\r\n--" + MultiPartEntityResponse.MULTIPART_BOUNDARY + "\r\n"
                + MultiPartEntityResponse.HTTP_HEADER_CONTENT_TYPE + ": " + contentType + "\r\n"
                + MultiPartEntityResponse.HTTP_HEADER_CONTENT_RANGE + ": " + MultiPartEntityResponse.BYTES + " " + firstStartRange + "-" + firstEndRange + "/" + contentLength + "\r\n"
                + "\r\n";

            var secondHeader = "\r\n--" + MultiPartEntityResponse.MULTIPART_BOUNDARY + "\r\n"
                + MultiPartEntityResponse.HTTP_HEADER_CONTENT_TYPE + ": " + contentType + "\r\n"
                + MultiPartEntityResponse.HTTP_HEADER_CONTENT_RANGE + ": " + MultiPartEntityResponse.BYTES + " " + secondStartRange + "-" + secondEndRange + "/" + contentLength + "\r\n"
                + "\r\n";

            var footer = "\r\n--" + MultiPartEntityResponse.MULTIPART_BOUNDARY + "--\r\n";

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

            var firstHeader = "\r\n--" + MultiPartEntityResponse.MULTIPART_BOUNDARY + "\r\n"
                + MultiPartEntityResponse.HTTP_HEADER_CONTENT_TYPE + ": " + contentType + "\r\n"
                + MultiPartEntityResponse.HTTP_HEADER_CONTENT_RANGE + ": " + MultiPartEntityResponse.BYTES + " " + firstStartRange + "-" + firstEndRange + "/" + contentLength + "\r\n"
                + "\r\n";

            var secondHeader = "\r\n--" + MultiPartEntityResponse.MULTIPART_BOUNDARY + "\r\n"
                + MultiPartEntityResponse.HTTP_HEADER_CONTENT_TYPE + ": " + contentType + "\r\n"
                + MultiPartEntityResponse.HTTP_HEADER_CONTENT_RANGE + ": " + MultiPartEntityResponse.BYTES + " " + secondStartRange + "-" + secondEndRange + "/" + contentLength + "\r\n"
                + "\r\n";

            var footer = "\r\n--" + MultiPartEntityResponse.MULTIPART_BOUNDARY + "--\r\n";

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
