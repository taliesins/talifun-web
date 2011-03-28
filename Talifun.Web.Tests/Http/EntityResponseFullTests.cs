using System;
using System.IO;
using System.IO.Compression;
using System.Web;
using Rhino.Mocks;

namespace Talifun.Web.Tests.Http
{
    public abstract class EntityResponseFullTests : BehaviourTest<EntityResponseFull>
    {
        protected IHttpResponseHeaderHelper HttpResponseHeaderHelper = MockRepository.GenerateMock<IHttpResponseHeaderHelper>();

        protected override EntityResponseFull CreateSystemUnderTest()
        {
            return new EntityResponseFull(HttpResponseHeaderHelper);
        }
    }

    public abstract class WhenSendingHeaders : EntityResponseFullTests
    {
        protected HttpResponseBase HttpResponse = MockRepository.GenerateMock<HttpResponseBase>();
        protected IEntity Entity = MockRepository.GenerateMock<IEntity>();

        protected ResponseCompressionType EntityCompressionType;
        protected long EntityContentLength;
        protected string EntityContentType;
        protected ResponseCompressionType ResponseCompressionType;
        protected ResponseCompressionType ResponseCompressionTypeSet;
        protected string ContentLengthSet;

        protected override void When()
        {
            Entity.Stub(x => x.ContentLength).Return(EntityContentLength);
            Entity.Stub(x => x.CompressionType).Return(EntityCompressionType);
            Entity.Stub(x => x.ContentType).Return(EntityContentType);

            HttpResponseHeaderHelper
                .Stub(x => x.SetContentEncoding(HttpResponse, ResponseCompressionType))
                .Callback((HttpResponseBase httpResponse, ResponseCompressionType responseCompressionType) =>
                              {
                                  ResponseCompressionTypeSet = responseCompressionType;
                                  return true;
                              });

            HttpResponseHeaderHelper
                .Stub(x => x.AppendHeader(HttpResponse, EntityResponseFull.HttpHeaderContentLength, EntityContentLength.ToString()))
                .Callback((HttpResponseBase httpResponse, string httpResponseHeader, string headerValue)=>
                              {
                                  ContentLengthSet = headerValue;
                                  return true;
                              });
            
            HttpResponse.Stub(x => x.ContentType).PropertyBehavior();
            HttpResponse.Stub(x => x.Filter).PropertyBehavior();
            HttpResponse.Filter = new MemoryStream();

            SystemUnderTest.SendHeaders(HttpResponse, ResponseCompressionType, Entity);
        }
    }

    public class WhenSendingHeadersAndEntityCompressionNoneRequestedCompressionNone : WhenSendingHeaders
    {
        protected override void Given()
        {
            EntityContentType = "";
            EntityContentLength = 1000l;
            EntityCompressionType = ResponseCompressionType.None;
            ResponseCompressionType = ResponseCompressionType.None;
        }

        [Then]
        public void ResponseCompressionTypeShouldBeSet()
        {
            ResponseCompressionTypeSet.ShouldEqual(ResponseCompressionType);
        }

        [Then]
        public void ContentLengthShouldBeSet()
        {
            ContentLengthSet.ShouldEqual(EntityContentLength.ToString());
        }

        [Then]
        public void ContentTypeShouldBeSet()
        {
            HttpResponse.ContentType.ShouldEqual(EntityContentType);
        }

        [Then]
        public void OutputIsNotCompressed()
        {
            HttpResponse.Filter.ShouldBeOfType<MemoryStream>();
        }
    }

    public class WhenSendingHeadersAndEntityCompressionNoneRequestedCompressionGzip : WhenSendingHeaders
    {
        protected override void Given()
        {
            EntityContentType = "";
            EntityContentLength = 1000l;
            EntityCompressionType = ResponseCompressionType.None;
            ResponseCompressionType = ResponseCompressionType.GZip;
        }

        [Then]
        public void ResponseCompressionTypeShouldBeSet()
        {
            ResponseCompressionTypeSet.ShouldEqual(ResponseCompressionType);
        }

        [Then]
        public void ContentLengthShouldBeSet()
        {
            ContentLengthSet.ShouldBeNull();
        }

        [Then]
        public void ContentTypeShouldBeSet()
        {
            HttpResponse.ContentType.ShouldEqual(EntityContentType);
        }

        [Then]
        public void OutputShouldBeCompressed()
        {
            HttpResponse.Filter.ShouldBeOfType<GZipStream>();
        }
    }

    public class WhenSendingHeadersAndEntityCompressionNoneRequestedCompressionDeflate : WhenSendingHeaders
    {
        protected override void Given()
        {
            EntityContentType = "";
            EntityContentLength = 1000l;
            EntityCompressionType = ResponseCompressionType.None;
            ResponseCompressionType = ResponseCompressionType.Deflate;
        }

        [Then]
        public void ResponseCompressionTypeShouldBeSet()
        {
            ResponseCompressionTypeSet.ShouldEqual(ResponseCompressionType);
        }

        [Then]
        public void ContentLengthShouldNotBeSet()
        {
            ContentLengthSet.ShouldBeNull();
        }

        [Then]
        public void ContentTypeShouldBeSet()
        {
            HttpResponse.ContentType.ShouldEqual(EntityContentType);
        }

        [Then]
        public void OutputShouldBeCompressed()
        {
            HttpResponse.Filter.ShouldBeOfType<DeflateStream>();
        }
    }

    public class WhenSendingHeadersAndEntityCompressionGzipRequestedCompressionNone : WhenSendingHeaders
    {
        protected override void Given()
        {
            RecordAnyExceptionsThrown();
            EntityContentType = "";
            EntityContentLength = 1000l;
            EntityCompressionType = ResponseCompressionType.GZip;
            ResponseCompressionType = ResponseCompressionType.None;
        }

        [Then]
        public void ThrowsNotImplementedException()
        {
            ThrownException.ShouldBeOfType<NotImplementedException>();
        }
    }

    public class WhenSendingHeadersAndEntityCompressionGzipRequestedCompressionGzip : WhenSendingHeaders
    {
        protected override void Given()
        {
            EntityContentType = "";
            EntityContentLength = 1000l;
            EntityCompressionType = ResponseCompressionType.GZip;
            ResponseCompressionType = ResponseCompressionType.GZip;
        }

        [Then]
        public void ResponseCompressionTypeShouldBeSet()
        {
            ResponseCompressionTypeSet.ShouldEqual(ResponseCompressionType);
        }

        [Then]
        public void ContentLengthShouldBeSet()
        {
            ContentLengthSet.ShouldEqual(EntityContentLength.ToString());
        }

        [Then]
        public void ContentTypeShouldBeSet()
        {
            HttpResponse.ContentType.ShouldEqual(EntityContentType);
        }

        [Then]
        public void OutputIsNotCompressed()
        {
            HttpResponse.Filter.ShouldBeOfType<MemoryStream>();
        }
    }

    public class WhenSendingHeadersAndEntityCompressionGzipRequestedCompressionDeflate : WhenSendingHeaders
    {
        protected override void Given()
        {
            RecordAnyExceptionsThrown();
            EntityContentType = "";
            EntityContentLength = 1000l;
            EntityCompressionType = ResponseCompressionType.GZip;
            ResponseCompressionType = ResponseCompressionType.Deflate;
        }

        [Then]
        public void ThrowsNotImplementedException()
        {
            ThrownException.ShouldBeOfType<NotImplementedException>();
        }
    }

    public class WhenSendingHeadersAndEntityCompressionDeflateRequestedCompressionNone : WhenSendingHeaders
    {
        protected override void Given()
        {
            RecordAnyExceptionsThrown();
            EntityContentType = "";
            EntityContentLength = 1000l;
            EntityCompressionType = ResponseCompressionType.Deflate;
            ResponseCompressionType = ResponseCompressionType.None;
        }

        [Then]
        public void ThrowsNotImplementedException()
        {
            ThrownException.ShouldBeOfType<NotImplementedException>();
        }
    }

    public class WhenSendingHeadersAndEntityCompressionDeflateRequestedCompressionGzip : WhenSendingHeaders
    {
        protected override void Given()
        {
            RecordAnyExceptionsThrown();
            EntityContentType = "";
            EntityContentLength = 1000l;
            EntityCompressionType = ResponseCompressionType.Deflate;
            ResponseCompressionType = ResponseCompressionType.GZip;
        }

        [Then]
        public void ThrowsNotImplementedException()
        {
            ThrownException.ShouldBeOfType<NotImplementedException>();
        }
    }

    public class WhenSendingHeadersAndEntityCompressionDeflateRequestedCompressionDeflate : WhenSendingHeaders
    {
        protected override void Given()
        {
            EntityContentType = "";
            EntityContentLength = 1000l;
            EntityCompressionType = ResponseCompressionType.Deflate;
            ResponseCompressionType = ResponseCompressionType.Deflate;
        }

        [Then]
        public void ResponseCompressionTypeShouldBeSet()
        {
            ResponseCompressionTypeSet.ShouldEqual(ResponseCompressionType);
        }

        [Then]
        public void ContentLengthShouldBeSet()
        {
            ContentLengthSet.ShouldEqual(EntityContentLength.ToString());
        }

        [Then]
        public void ContentTypeShouldBeSet()
        {
            HttpResponse.ContentType.ShouldEqual(EntityContentType);
        }

        [Then]
        public void OutputIsNotCompressed()
        {
            HttpResponse.Filter.ShouldBeOfType<MemoryStream>();
        }
    }

    public abstract class WhenSendingBody : EntityResponseFullTests
    {
        protected HttpMethod RequestHttpMethod;
        protected HttpResponseBase HttpResponse = MockRepository.GenerateMock<HttpResponseBase>();
        protected ITransmitEntityStrategy TransmitEntityStrategy = MockRepository.GenerateMock<ITransmitEntityStrategy>();
        protected bool IsBodySent;
        protected override void When()
        {
            TransmitEntityStrategy
                .Stub(x => x.Transmit(HttpResponse))
                .Callback((HttpResponseBase httpResponse) =>
                              {
                                  IsBodySent = true;
                                    return true;
                                });

            SystemUnderTest.SendBody(RequestHttpMethod, HttpResponse, TransmitEntityStrategy);
        }
    }

    public class WhenSendingBodyForGetRequest : WhenSendingBody
    {
        protected override void Given()
        {
            RequestHttpMethod = HttpMethod.Get;
        }

        [Then]
        public void BodyIsSent()
        {
            IsBodySent.ShouldEqual(true);
        }
    }

    public class WhenSendingBodyForHeadRequest : WhenSendingBody
    {
        protected override void Given()
        {
            RequestHttpMethod = HttpMethod.Head;
        }

        [Then]
        public void BodyIsNotSent()
        {
            IsBodySent.ShouldEqual(false);
        }
    }

    public class WhenSendingBodyForNotHeadOrGetRequest : WhenSendingBody
    {
        protected override void Given()
        {
            RecordAnyExceptionsThrown();
            RequestHttpMethod = HttpMethod.Options;
        }

        [Then]
        public void ThrowsException()
        {
            ThrownException.ShouldBeOfType<Exception>();
        }
    }
}
