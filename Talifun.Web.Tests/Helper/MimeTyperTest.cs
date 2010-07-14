using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Talifun.Web.Tests.Helper
{
    [TestFixture]
    public class MimeTyperTest
    {
        [Test]
        public void GetMimeType_UnknownMimeType_ApplicationOctetstream()
        {
            //Arrange
            var extension = ".unknown";

            //Act
            var mimeTyper = new MimeTyper();
            var mimeType = mimeTyper.GetMimeType(extension);

            //Assert
            Assert.AreEqual("application/octetstream", mimeType);
        }

        [Test]
        public void GetMimeType_ValidMimeType_MimeType()
        {
            //Arrange
            var extension = ".htm";

            //Act
            var mimeTyper = new MimeTyper();
            var mimeType = mimeTyper.GetMimeType(extension);

            //Assert
            Assert.AreEqual("text/html", mimeType);
        }

        [Test]
        public void GetMimeType_CachedValidMimeType_MimeType()
        {
            //Arrange
            var extension = ".html"; //Make sure that no other unit tests lookup this extension

            //Act
            var mimeTyper = new MimeTyper();
            mimeTyper.GetMimeType(extension); //Call it twice to insure that it has been cached
            var mimeType = mimeTyper.GetMimeType(extension);

            //Assert
            Assert.AreEqual("text/html", mimeType);
        }
    }
}
