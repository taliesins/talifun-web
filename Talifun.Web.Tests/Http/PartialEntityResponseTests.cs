using System;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using System.Collections.Generic;

namespace Talifun.Web.Tests.Http
{
    //[TestFixture]
    //public class PartialEntityResponseTests
    //{
    //    #region SendHeaders
    //    [Test]
    //    [ExpectedException(typeof(Exception))]
    //    public void SendHeaders_EntityIsCompressedWithGZip_Void()
    //    {
    //        //Arrange
    //        var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
    //        var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
    //        var entity = MockRepository.GenerateMock<IEntity>();
    //        var responseCompressionType = ResponseCompressionType.None;

    //        var startRange = 0;
    //        var endRange = 499;

    //        var firstRangeItem = new RangeItem
    //        {
    //            StartRange = startRange,
    //            EndRange = endRange
    //        };
    //        var ranges = new[] { firstRangeItem };

    //        entity.Stub(x => x.ContentLength).Return(0);
    //        entity.Stub(x => x.CompressionType).Return(ResponseCompressionType.GZip);

    //        //Act
    //        var responseForPartialEntityStrategy = new PartialEntityResponse(httpResponseHeaderHelper, ranges);
    //        responseForPartialEntityStrategy.SendHeaders(httpResponse, responseCompressionType, entity);

    //        //Assert
    //    }

    //    [Test]
    //    [ExpectedException(typeof(Exception))]
    //    public void SendHeaders_EntityIsCompressedWithDeflate_Void()
    //    {
    //        //Arrange
    //        var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
    //        var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
    //        var entity = MockRepository.GenerateMock<IEntity>();
    //        var responseCompressionType = ResponseCompressionType.None;

    //        var startRange = 0;
    //        var endRange = 499;

    //        var firstRangeItem = new RangeItem
    //        {
    //            StartRange = startRange,
    //            EndRange = endRange
    //        };
    //        var ranges = new[] { firstRangeItem };

    //        entity.Stub(x => x.ContentLength).Return(0);
    //        entity.Stub(x => x.CompressionType).Return(ResponseCompressionType.Deflate);

    //        //Act
    //        var responseForPartialEntityStrategy = new PartialEntityResponse(httpResponseHeaderHelper, ranges);
    //        responseForPartialEntityStrategy.SendHeaders(httpResponse, responseCompressionType, entity);

    //        //Assert
    //    }

    //    [Test]
    //    public void SendHeaders_OneRequestRangeWithNoCompression_Void()
    //    {
    //        //Arrange
    //        var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
    //        var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
    //        var entity = MockRepository.GenerateMock<IEntity>();
    //        var responseCompressionType = ResponseCompressionType.None;

    //        var startRange = 0;
    //        var endRange = 499;

    //        var firstRangeItem = new RangeItem
    //        {
    //            StartRange = startRange,
    //            EndRange = endRange
    //        };

    //        var bytesToRead = endRange - startRange + 1;
    //        var ranges = new[] { firstRangeItem };

    //        entity.Stub(x => x.ContentLength).Return(0);
    //        entity.Stub(x => x.CompressionType).Return(ResponseCompressionType.None);

    //        httpResponseHeaderHelper.Expect(x => x.SetContentEncoding(httpResponse, responseCompressionType));
    //        httpResponseHeaderHelper.Expect(x => x.AppendHeader(httpResponse, PartialEntityResponse.HTTP_HEADER_CONTENT_LENGTH, bytesToRead.ToString()));
    //        httpResponse.Expect(x => x.ContentType = entity.ContentType);
    //        httpResponseHeaderHelper.Expect(x => x.AppendHeader(httpResponse, PartialEntityResponse.HTTP_HEADER_CONTENT_RANGE, PartialEntityResponse.BYTES + " " + startRange + "-" + endRange + "/" + entity.ContentLength));

    //        //Act
    //        var responseForPartialEntityStrategy = new PartialEntityResponse(httpResponseHeaderHelper, ranges);
    //        responseForPartialEntityStrategy.SendHeaders(httpResponse, responseCompressionType, entity);

    //        //Assert
    //        httpResponseHeaderHelper.VerifyAllExpectations();
    //        httpResponse.VerifyAllExpectations();
    //    }

    //    [Test]
    //    public void SendHeaders_MultipleRequestRangeWithNoCompression_Void()
    //    {
    //        //Arrange
    //        var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
    //        var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
    //        var entity = MockRepository.GenerateMock<IEntity>();
    //        var responseCompressionType = ResponseCompressionType.None;

    //        var startRangeFirst = 0;
    //        var endRangeFirst = 499;

    //        var rangeItemfirst = new RangeItem
    //        {
    //            StartRange = startRangeFirst,
    //            EndRange = endRangeFirst
    //        };

    //        var startRangeSecond = 500;
    //        var endRangeSecond = 999;

    //        var rangeItemSecond = new RangeItem
    //        {
    //            StartRange = startRangeSecond,
    //            EndRange = endRangeSecond
    //        };

    //        var bytesToRead = (endRangeFirst - startRangeFirst) + (endRangeSecond - startRangeSecond) + 1;

    //        var ranges = new[] { rangeItemfirst, rangeItemSecond };

    //        var responseForPartialEntityStrategy = MockRepository.GeneratePartialMock<PartialEntityResponse>(httpResponseHeaderHelper, ranges));
    //        //var responseForPartialEntityStrategy = new PartialEntityResponse(httpResponseHeaderHelper, ranges);

    //        entity.Stub(x => x.ContentLength).Return(0);
    //        entity.Stub(x => x.CompressionType).Return(ResponseCompressionType.None);

    //        httpResponseHeaderHelper.Expect(x => x.SetContentEncoding(httpResponse, responseCompressionType));
    //        //httpResponseHeaderHelper.Expect(x => x.AppendHeader(httpResponse, PartialEntityResponse.HTTP_HEADER_CONTENT_LENGTH, bytesToRead.ToString()));
    //        httpResponse.Expect(x => x.ContentType = PartialEntityResponse.MULTIPART_CONTENTTYPE);

    //        //Act
            
    //        responseForPartialEntityStrategy.SendHeaders(httpResponse, responseCompressionType, entity);

    //        //Assert
    //        httpResponseHeaderHelper.VerifyAllExpectations();
    //        httpResponse.VerifyAllExpectations();
    //    }

    //    #endregion

    //    #region SendBody
    //    [Test]
    //    public void SendBody_GetRequestForOneRange_Void()
    //    {
    //        //Arrange
    //        var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
    //        var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
    //        var transmitEntityStrategy = MockRepository.GenerateMock<ITransmitEntityStrategy>();
    //        var requestHttpMethod = HttpMethod.Get;

    //        var startRange = 0;
    //        var endRange = 499;
            
    //        var bytesToRead = endRange - startRange + 1;
    //        var firstRangeItem = new RangeItem
    //                                 {
    //                                     StartRange = startRange,
    //                                     EndRange = endRange
    //                                 };
    //        var ranges = new []{firstRangeItem};

    //        //Act
    //        var responseForPartialEntityStrategy = new PartialEntityResponse(httpResponseHeaderHelper, ranges);
    //        responseForPartialEntityStrategy.SendBody(requestHttpMethod, httpResponse, transmitEntityStrategy);

    //        //Assert
    //        transmitEntityStrategy.AssertWasCalled(x => x.Transmit(httpResponse, startRange, bytesToRead));
    //    }

    //    [Test]
    //    public void SendBody_GetRequestForMoreThanOneRange_Void()
    //    {
    //        //Arrange
    //        var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
    //        var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
    //        var transmitEntityStrategy = MockRepository.GenerateMock<ITransmitEntityStrategy>();
    //        var entity = MockRepository.GenerateMock<IEntity>();
    //        entity.Expect(x => x.ContentType).Return("");
    //        entity.Expect(x => x.ContentLength).Return(0);

    //        transmitEntityStrategy.Stub(x => x.Entity).Return(entity);

    //        var requestHttpMethod = HttpMethod.Get;
    //        var firstRangeItem = new RangeItem
    //        {
    //            StartRange = 0,
    //            EndRange = 499
    //        };
    //        var secondRangeItem = new RangeItem
    //        {
    //            StartRange = 500,
    //            EndRange = 999
    //        };
    //        var ranges = new[] { firstRangeItem, secondRangeItem };

    //        var responseForPartialEntityStrategy = MockRepository.GeneratePartialMock<PartialEntityResponse>(httpResponseHeaderHelper, ranges);
    //        responseForPartialEntityStrategy.Expect(x => x.TransmitMultiPartFile(httpResponse, transmitEntityStrategy.Entity.ContentType, transmitEntityStrategy.Entity.ContentLength, ranges, transmitEntityStrategy));

    //        //Act
    //        responseForPartialEntityStrategy.SendBody(requestHttpMethod, httpResponse, transmitEntityStrategy);

    //        //Assert
    //        responseForPartialEntityStrategy.AssertWasCalled(x => x.TransmitMultiPartFile(httpResponse, transmitEntityStrategy.Entity.ContentType, transmitEntityStrategy.Entity.ContentLength, ranges, transmitEntityStrategy));
    //    }

    //    [Test]
    //    public void SendBody_HeadRequest_Void()
    //    {
    //        //Arrange
    //        var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
    //        var httpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();
    //        var transmitEntityStrategy = MockRepository.GenerateMock<ITransmitEntityStrategy>();
    //        var requestHttpMethod = HttpMethod.Head;
    //        var ranges = new List<RangeItem>();

    //        //Act
    //        var responseForPartialEntityStrategy = new PartialEntityResponse(httpResponseHeaderHelper, ranges);
    //        responseForPartialEntityStrategy.SendBody(requestHttpMethod, httpResponse, transmitEntityStrategy);

    //        //Assert
    //        transmitEntityStrategy.AssertWasNotCalled(x => x.Transmit(httpResponse));
    //    }
    //    #endregion
    //}
}
