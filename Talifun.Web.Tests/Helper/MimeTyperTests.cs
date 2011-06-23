using Rhino.Mocks;

namespace Talifun.Web.Tests.Helper
{
    public abstract class MimeTyperTests : BehaviourTest<MimeTyper>
    {
        protected ICacheManager CacheManager = MockRepository.GenerateMock<ICacheManager>();

        protected override MimeTyper CreateSystemUnderTest()
        {
            return new MimeTyper(CacheManager);
        }
    }

    public abstract class WhenGettingMimeType : MimeTyperTests
    {
        protected string Extension;

        protected string MimeType;
        protected override void When()
        {
            MimeType = SystemUnderTest.GetMimeType(Extension);
        }
    }

    public class WhenGettingMimeTypeForUnknownMimeType : WhenGettingMimeType
    {
        protected override void Given()
        {
            Extension = ".unknown";
        }

        [Then]
        public void ApplicationOctetStreamShouldBeReturned()
        {
            MimeType.ShouldEqual("application/octetstream");
        }
    }

    public class WhenGettingMimeTypeForKnownMimeType : WhenGettingMimeType
    {
        protected override void Given()
        {
            Extension = ".htm";
        }

        [Then]
        public void ApplicationOctetStreamShouldBeReturned()
        {
            MimeType.ShouldEqual("text/html");
        }
    }

    public abstract class WhenGettingCachedMimeType : MimeTyperTests
    {
        protected string Extension;

        protected string MimeType;
        protected override void When()
        {
            MimeType = SystemUnderTest.GetMimeType(Extension);
            MimeType = SystemUnderTest.GetMimeType(Extension); //Calling it twice will get cached value
        }
    }

    public class WhenGettingCachedMimeTypeForUnknownMimeType : WhenGettingMimeType
    {
        protected override void Given()
        {
            Extension = ".unknown";
        }

        [Then]
        public void ApplicationOctetStreamShouldBeReturned()
        {
            MimeType.ShouldEqual("application/octetstream");
        }
    }

    public class WhenGettingCachedMimeTypeForKnownMimeType : WhenGettingMimeType
    {
        protected override void Given()
        {
            Extension = ".htm";
        }

        [Then]
        public void ApplicationOctetStreamShouldBeReturned()
        {
            MimeType.ShouldEqual("text/html");
        }
    }
}
