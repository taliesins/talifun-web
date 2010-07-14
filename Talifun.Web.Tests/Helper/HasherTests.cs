using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;

namespace Talifun.Web.Tests.Helper
{
    [TestFixture]
    public class HasherTests
    {
        [Test]
        public void CalculateMd5Etag_Stream_HashedString()
        {
            //Arrange
            var retryableFileOpener = MockRepository.GenerateMock<IRetryableFileOpener>();
            var text = "This is a test";
            var md5HashOfText = "zhFORQHS9OLc6j4XtUbzOQ=="; //Hash value of bytes
            var byteArray = Encoding.ASCII.GetBytes(text);
            var stream = new MemoryStream(byteArray); 

            //Act
            var hasher = new Hasher(retryableFileOpener);
            var md5Hash = hasher.CalculateMd5Etag(stream);

            //Assert
            Assert.AreEqual(md5HashOfText, md5Hash);
        }

        [Test]
        public void CalculateMd5Etag_PartOfStream_HashedString()
        {
            //Arrange
            var retryableFileOpener = MockRepository.GenerateMock<IRetryableFileOpener>();
            var text = "This is a test";
            var startPosition = 0;
            var endPosition = text.Length;
            var md5HashOfText = "zhFORQHS9OLc6j4XtUbzOQ=="; //Hash value of bytes
            var byteArray = Encoding.ASCII.GetBytes(text);
            var stream = new MemoryStream(byteArray);

            //Act
            var hasher = new Hasher(retryableFileOpener);
            var md5Hash = hasher.CalculateMd5Etag(stream, startPosition, endPosition);

            //Assert
            Assert.AreEqual(md5HashOfText, md5Hash);
        }

        [Test]
        public void CalculateMd5Etag_FileInfo_HashedString()
        {
            //Arrange
            var retryableFileOpener = MockRepository.GenerateMock<IRetryableFileOpener>();
            var text = "This is a test";
            
            var md5HashOfText = "zhFORQHS9OLc6j4XtUbzOQ=="; //Hash value of bytes
            var byteArray = Encoding.ASCII.GetBytes(text);
            var stream = new MemoryStream(byteArray);
            var fileName = "test.txt";
            var fileInfo = new FileInfo(fileName);

            var fileStream = MockRepository.GenerateMock<FileStream>();

            retryableFileOpener.Expect(x => x.OpenFileStream(fileInfo, 5, FileMode.Open, FileAccess.Read, FileShare.Read)).Return(fileStream);
            Func<byte[], int, int, int> read = stream.Read;
                                                
            fileStream.Stub(x => x.Read(null, 0, 0)).IgnoreArguments().Do(read);

            //Act
            var hasher = new Hasher(retryableFileOpener);
            var md5Hash = hasher.CalculateMd5Etag(fileInfo);

            //Assert
            Assert.AreEqual(md5HashOfText, md5Hash);
        }
    }
}
