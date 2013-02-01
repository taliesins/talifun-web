using System;
using NUnit.Framework;
using Talifun.Web.Crusher;

namespace Talifun.Web.Tests.Crusher
{
    [TestFixture]
    public class PathProviderTests
    {
        [Test]
        public void ToRelativeReturnsRelativePathWhenApplicationPathIsRoot()
        {
            var applicationPath = @"/";
            var physicalApplicationPath = @"c:\inetpub\wwwroot\";
            var pathProvider = new PathProvider(applicationPath, physicalApplicationPath);
            var filePath = @"c:\inetpub\wwwroot\js\jquery.js";
            var expectedResult = new Uri("~/js/jquery.js", UriKind.Relative);

            var relativePath = pathProvider.ToRelative(filePath);

            Assert.AreEqual(relativePath, expectedResult);
        }

        [Test]
        public void ToRelativeReturnsRelativePathWhenApplicationPathIsNotRoot()
        {
            var applicationPath = @"/TestApp/";
            var physicalApplicationPath = @"c:\inetpub\wwwroot\";
            var pathProvider = new PathProvider(applicationPath, physicalApplicationPath);
            var filePath = @"c:\inetpub\wwwroot\TestApp\js\jquery.js";
            var expectedResult = new Uri("~/js/jquery.js", UriKind.Relative);

            var relativePath = pathProvider.ToRelative(filePath);

            Assert.AreEqual(relativePath, expectedResult);
        }

        [Test]
        public void GetAbsoluteUriDirectoryReturnsRelativePathWhenApplicationPathIsRoot()
        {
            var applicationPath = @"/";
            var physicalApplicationPath = @"c:\inetpub\wwwroot\";
            var pathProvider = new PathProvider(applicationPath, physicalApplicationPath);
            var uri = @"~/js/jquery.js";
            var expectedResult = new Uri("c:/inetpub/wwwroot/js/", UriKind.Absolute);

            var relativePath = pathProvider.GetAbsoluteUriDirectory(uri);

            Assert.AreEqual(relativePath, expectedResult);
        }

        [Test]
        public void GetAbsoluteUriDirectoryReturnsRelativePathWhenApplicationPathIsNotRoot()
        {
            var applicationPath = @"/TestApp/";
            var physicalApplicationPath = @"c:\inetpub\wwwroot\";
            var pathProvider = new PathProvider(applicationPath, physicalApplicationPath);
            var uri = @"~/js/jquery.js";
            var expectedResult = new Uri("c:/inetpub/wwwroot/TestApp/js/", UriKind.Absolute);

            var relativePath = pathProvider.GetAbsoluteUriDirectory(uri);

            Assert.AreEqual(relativePath, expectedResult);
        }
    }
}
