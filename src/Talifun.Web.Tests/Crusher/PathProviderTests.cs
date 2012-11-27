using System;
using NUnit.Framework;
using Talifun.Web.Crusher;

namespace Talifun.Web.Tests.Crusher
{
    [TestFixture]
    public class PathProviderTests
    {
        [Test]
        public void ToRelativeReturnsRelativePathWhenUsedOnRoot()
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
        public void ToRelativeReturnsRelativePathWhenUsedOnVirtualDirectory()
        {
            var applicationPath = @"/TestApp/";
            var physicalApplicationPath = @"c:\inetpub\wwwroot\";
            var pathProvider = new PathProvider(applicationPath, physicalApplicationPath);
            var filePath = @"c:\inetpub\wwwroot\TestApp\js\jquery.js";
            var expectedResult = new Uri("~/TestApp/js/jquery.js", UriKind.Relative);

            var relativePath = pathProvider.ToRelative(filePath);

            Assert.AreEqual(relativePath, expectedResult);
        }

        [Test]
        public void GetRelativeRootUriReturnsRelativePathWhenUsedOnRoot()
        {
            var applicationPath = @"/";
            var physicalApplicationPath = @"c:\inetpub\wwwroot\";
            var pathProvider = new PathProvider(applicationPath, physicalApplicationPath);
            var uri = @"~/TestApp/js/jquery.js";
            var expectedResult = new Uri("~/js/jquery.js", UriKind.Relative);

            var relativePath = pathProvider.GetRelativeRootUri(uri);

            Assert.AreEqual(relativePath, expectedResult);
        }

        [Test]
        public void GetRelativeRootUriReturnsRelativePathWhenUsedOnVirtualDirectory()
        {
            var applicationPath = @"/TestApp/";
            var physicalApplicationPath = @"c:\inetpub\wwwroot\";
            var pathProvider = new PathProvider(applicationPath, physicalApplicationPath);
            //var uri = @"~/TestApp/js/jquery.js";
            var uri = @"~/TestApp/js/jquery.js";
            var expectedResult = new Uri("~/TestApp/js/jquery.js", UriKind.Relative);

            var relativePath = pathProvider.GetRelativeRootUri(uri);

            Assert.AreEqual(relativePath, expectedResult);
        }
    }
}
