using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;

namespace Talifun.Web.Tests.Http
{
    [TestFixture]
    public class MultipleByteRangeResponseTests
    {
        #region SetContentLength
        [Test]
        public void SetContentLength_ContentLengthSet_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
            var entity = MockRepository.GenerateMock<IEntity>();

            var contentType = "image/gif";
            var contentLength = 1000l;
            
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

            var rangeItems = new[] {firstRangeItem, secondRangeItem};

            var firstHeader = "\r\n--" + MultipleByteRangeResponse.MULTIPART_BOUNDARY + "\r\n"
                + MultipleByteRangeResponse.HTTP_HEADER_CONTENT_TYPE + ": " + contentType + "\r\n"
                + MultipleByteRangeResponse.HTTP_HEADER_CONTENT_RANGE + ": " + MultipleByteRangeResponse.BYTES + " " + firstStartRange + "-" + firstEndRange + "/" + contentLength + "\r\n"
                + "\r\n";

            var secondHeader = "\r\n--" + MultipleByteRangeResponse.MULTIPART_BOUNDARY + "\r\n"
                + MultipleByteRangeResponse.HTTP_HEADER_CONTENT_TYPE + ": " + contentType + "\r\n"
                + MultipleByteRangeResponse.HTTP_HEADER_CONTENT_RANGE + ": " + MultipleByteRangeResponse.BYTES + " " + secondStartRange + "-" + secondEndRange + "/" + contentLength + "\r\n"
                + "\r\n";

            var footer = "\r\n--" + MultipleByteRangeResponse.MULTIPART_BOUNDARY + "--\r\n";

            var responseContentLength = firstHeader.Length + firstEndRange - firstStartRange + 1
                + secondHeader.Length + secondEndRange - secondStartRange + 1
                + footer.Length;

            entity.Stub(x => x.ContentType).Return(contentType);
            entity.Stub(x => x.ContentLength).Return(contentLength);

            httpResponseHeaderHelper.Expect(x => x.AppendHeader(httpResponse, SingleByteRangeResponse.HTTP_HEADER_CONTENT_LENGTH, responseContentLength.ToString()));

            //Act
            var multipleByteRangeResponse = new MultipleByteRangeResponse(httpResponseHeaderHelper, rangeItems);
            multipleByteRangeResponse.SetContentLength(httpResponse, entity);

            //Assert
            httpResponseHeaderHelper.VerifyAllExpectations();
            httpResponse.VerifyAllExpectations();
        }
        #endregion

        #region SetOtherHeaders
        [Test]
        public void SetOtherHeaders_OtherHeadersSet_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
            var entity = MockRepository.GenerateMock<IEntity>();

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

            var rangeItems = new[] {firstRangeItem, secondRangeItem};

            httpResponse.Expect(x => x.ContentType = MultipleByteRangeResponse.MULTIPART_CONTENTTYPE);

            //Act
            var multipleByteRangeResponse = new MultipleByteRangeResponse(httpResponseHeaderHelper, rangeItems);
            multipleByteRangeResponse.SetOtherHeaders(httpResponse, entity);

            //Assert
            httpResponseHeaderHelper.VerifyAllExpectations();
            httpResponse.VerifyAllExpectations();
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

            var firstHeader = "\r\n--" + MultipleByteRangeResponse.MULTIPART_BOUNDARY + "\r\n"
                + MultipleByteRangeResponse.HTTP_HEADER_CONTENT_TYPE + ": " + contentType + "\r\n"
                + MultipleByteRangeResponse.HTTP_HEADER_CONTENT_RANGE + ": " + MultipleByteRangeResponse.BYTES + " " + firstStartRange + "-" + firstEndRange + "/" + contentLength + "\r\n"
                + "\r\n";

            var secondHeader = "\r\n--" + MultipleByteRangeResponse.MULTIPART_BOUNDARY + "\r\n"
                + MultipleByteRangeResponse.HTTP_HEADER_CONTENT_TYPE + ": " + contentType + "\r\n"
                + MultipleByteRangeResponse.HTTP_HEADER_CONTENT_RANGE + ": " + MultipleByteRangeResponse.BYTES + " " + secondStartRange + "-" + secondEndRange + "/" + contentLength + "\r\n"
                + "\r\n";

            var footer = "\r\n--" + MultipleByteRangeResponse.MULTIPART_BOUNDARY + "--\r\n";

            transmitEntityStrategy.Stub(x => x.Entity).Return(entity);
            entity.Stub(x => x.ContentType).Return(contentType);
            entity.Stub(x => x.ContentLength).Return(contentLength);
            httpResponse.Stub(x => x.Output).Return(output);

            //Act
            var multipleByteRangeResponse = new MultipleByteRangeResponse(httpResponseHeaderHelper, rangeItems);
            multipleByteRangeResponse.SendBody(requestHttpMethod, httpResponse, transmitEntityStrategy);

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

            var firstHeader = "\r\n--" + MultipleByteRangeResponse.MULTIPART_BOUNDARY + "\r\n"
                + MultipleByteRangeResponse.HTTP_HEADER_CONTENT_TYPE + ": " + contentType + "\r\n"
                + MultipleByteRangeResponse.HTTP_HEADER_CONTENT_RANGE + ": " + MultipleByteRangeResponse.BYTES + " " + firstStartRange + "-" + firstEndRange + "/" + contentLength + "\r\n"
                + "\r\n";

            var secondHeader = "\r\n--" + MultipleByteRangeResponse.MULTIPART_BOUNDARY + "\r\n"
                + MultipleByteRangeResponse.HTTP_HEADER_CONTENT_TYPE + ": " + contentType + "\r\n"
                + MultipleByteRangeResponse.HTTP_HEADER_CONTENT_RANGE + ": " + MultipleByteRangeResponse.BYTES + " " + secondStartRange + "-" + secondEndRange + "/" + contentLength + "\r\n"
                + "\r\n";

            var footer = "\r\n--" + MultipleByteRangeResponse.MULTIPART_BOUNDARY + "--\r\n";

            transmitEntityStrategy.Stub(x => x.Entity).Return(entity);
            entity.Stub(x => x.ContentType).Return(contentType);
            entity.Stub(x => x.ContentLength).Return(contentLength);
            httpResponse.Stub(x => x.Output).Return(output);

            //Act
            var multipleByteRangeResponse = new MultipleByteRangeResponse(httpResponseHeaderHelper, rangeItems);
            multipleByteRangeResponse.SendBody(requestHttpMethod, httpResponse, transmitEntityStrategy);

            //Assert
            output.AssertWasNotCalled(x => x.Write(firstHeader));
            transmitEntityStrategy.AssertWasNotCalled(x => x.Transmit(httpResponse, firstStartRange, firstBytesToRead));

            output.AssertWasNotCalled(x => x.Write(secondHeader));
            transmitEntityStrategy.AssertWasNotCalled(x => x.Transmit(httpResponse, secondStartRange, secondBytesToRead));

            output.AssertWasNotCalled(x => x.Write(footer));
        }

        [Test]
        [ExpectedException]
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
            var multipleByteRangeResponse = new MultipleByteRangeResponse(httpResponseHeaderHelper, rangeItems);
            multipleByteRangeResponse.SendBody(requestHttpMethod, httpResponse, transmitEntityStrategy);
        }

        #endregion
    }
}
