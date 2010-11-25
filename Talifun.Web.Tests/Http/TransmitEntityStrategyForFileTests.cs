using System;
using System.IO;
using System.Text;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;

namespace Talifun.Web.Tests.Http
{
    [TestFixture]
    public class TransmitEntityStrategyForFileTests
    {
        [Test]
        public void Transmit_TransmitFullResponseWithBufferSizeLargerThanOrTheSameAsFileSize_Void()
        {
            //Arrange
            var retryableFileOpener = MockRepository.GenerateMock<IRetryableFileOpener>();
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var entity = MockRepository.GenerateMock<IEntity>();
            var fileInfo = MockRepository.GenerateMock<FileInfo>();
            var bufferSize = 50;

            var text = "123456789012345678901234567890"; //Hash value of bytes
            var expectedByteArray = Encoding.ASCII.GetBytes(text);
            var stream = new MemoryStream(expectedByteArray);

            var fileStream = MockRepository.GenerateMock<FileStream>();

            retryableFileOpener.Expect(x => x.OpenFileStream(fileInfo, 5, FileMode.Open, FileAccess.Read, FileShare.Read)).Return(fileStream);

            Func<byte[], int, int, int> read = stream.Read;
            fileStream.Stub(x => x.Read(null, 0, 0)).IgnoreArguments().Do(read);

            var outputStream = new MemoryStream();
            httpResponse.Stub(x => x.OutputStream).Return(outputStream);

            //Act
            var transmitEntityStrategyForByteArray = new TransmitEntityStrategyForFile(retryableFileOpener, entity, fileInfo, bufferSize);
            transmitEntityStrategyForByteArray.Transmit(httpResponse);

            //Assert
            Assert.AreEqual(outputStream.ToArray(), expectedByteArray);
        }

        [Test]
        public void Transmit_TransmitFullResponseWithBufferSizeSmallerThanFileSize_Void()
        {
            //Arrange
            var retryableFileOpener = MockRepository.GenerateMock<IRetryableFileOpener>();
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var entity = MockRepository.GenerateMock<IEntity>();
            var fileInfo = MockRepository.GenerateMock<FileInfo>();
            var bufferSize = 2;

            var text = "123456789012345678901234567890"; //Hash value of bytes
            var expectedByteArray = Encoding.ASCII.GetBytes(text);
            var stream = new MemoryStream(expectedByteArray);

            var fileStream = MockRepository.GenerateMock<FileStream>();

            retryableFileOpener.Expect(x => x.OpenFileStream(fileInfo, 5, FileMode.Open, FileAccess.Read, FileShare.Read)).Return(fileStream);

            Func<byte[], int, int, int> read = stream.Read;
            fileStream.Stub(x => x.Read(null, 0, 0)).IgnoreArguments().Do(read);

            var outputStream = new MemoryStream();
            httpResponse.Stub(x => x.OutputStream).Return(outputStream);

            //Act
            var transmitEntityStrategyForByteArray = new TransmitEntityStrategyForFile(retryableFileOpener, entity, fileInfo, bufferSize);
            transmitEntityStrategyForByteArray.Transmit(httpResponse);

            //Assert
            Assert.AreEqual(outputStream.ToArray(), expectedByteArray);
        }

        [Test]
        public void Transmit_TransmitPartialResponseWithBufferSizeLargerThanOrTheSameAsFileSize_Void()
        {
            //Arrange
            var retryableFileOpener = MockRepository.GenerateMock<IRetryableFileOpener>();
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var entity = MockRepository.GenerateMock<IEntity>();
            var fileInfo = MockRepository.GenerateMock<FileInfo>();
            var bufferSize = 50;
            var offset = 5l;
            var length = 10l;

            var text = "123456789012345678901234567890";
            var byteArray = Encoding.ASCII.GetBytes(text);
            var stream = new MemoryStream(byteArray);

            var expectedByteArray = Encoding.ASCII.GetBytes(text.Substring((int)offset, (int)length));

            var fileStream = MockRepository.GenerateMock<FileStream>();

            retryableFileOpener.Expect(x => x.OpenFileStream(fileInfo, 5, FileMode.Open, FileAccess.Read, FileShare.Read)).Return(fileStream);

            Func<byte[], int, int, int> read = stream.Read;
            fileStream.Stub(x => x.Read(null, 0, 0)).IgnoreArguments().Do(read);

            Func<long, SeekOrigin, long> seek = stream.Seek;
            fileStream.Stub(x => x.Seek(0, SeekOrigin.Begin)).IgnoreArguments().Do(seek);

            var outputStream = new MemoryStream();
            httpResponse.Stub(x => x.OutputStream).Return(outputStream);

            //Act
            var transmitEntityStrategyForByteArray = new TransmitEntityStrategyForFile(retryableFileOpener, entity, fileInfo, bufferSize);
            transmitEntityStrategyForByteArray.Transmit(httpResponse, offset, length);

            var outputByteArray = outputStream.ToArray();

            //Assert
            Assert.AreEqual(outputByteArray, expectedByteArray);
        }

        [Test]
        public void Transmit_TransmitPartialResponseWithBufferSizeSmallerThanFileSize_Void()
        {
            //Arrange
            var retryableFileOpener = MockRepository.GenerateMock<IRetryableFileOpener>();
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var entity = MockRepository.GenerateMock<IEntity>();
            var fileInfo = MockRepository.GenerateMock<FileInfo>();
            var bufferSize = 2;
            var offset = 5l;
            var length = 10l;

            var text = "123456789012345678901234567890";
            var byteArray = Encoding.ASCII.GetBytes(text);
            var stream = new MemoryStream(byteArray);

            var expectedByteArray = Encoding.ASCII.GetBytes(text.Substring((int)offset, (int)length));

            var fileStream = MockRepository.GenerateMock<FileStream>();

            retryableFileOpener.Expect(x => x.OpenFileStream(fileInfo, 5, FileMode.Open, FileAccess.Read, FileShare.Read)).Return(fileStream);

            Func<byte[], int, int, int> read = stream.Read;
            fileStream.Stub(x => x.Read(null, 0, 0)).IgnoreArguments().Do(read);

            Func<long, SeekOrigin, long> seek = stream.Seek;
            fileStream.Stub(x => x.Seek(0, SeekOrigin.Begin)).IgnoreArguments().Do(seek);

            var outputStream = new MemoryStream();
            httpResponse.Stub(x => x.OutputStream).Return(outputStream);

            //Act
            var transmitEntityStrategyForByteArray = new TransmitEntityStrategyForFile(retryableFileOpener, entity, fileInfo, bufferSize);
            transmitEntityStrategyForByteArray.Transmit(httpResponse, offset, length);

            //Assert
            Assert.AreEqual(outputStream.ToArray(), expectedByteArray);
        }

        [Test]
        public void Transmit_TransmitComplete_Void()
        {
            //Arrange
            var retryableFileOpener = MockRepository.GenerateMock<IRetryableFileOpener>();
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var entity = MockRepository.GenerateMock<IEntity>();
            var fileInfo = MockRepository.GenerateMock<FileInfo>();
            var bufferSize = 30;

            //Act
            var transmitEntityStrategyForByteArray = new TransmitEntityStrategyForFile(retryableFileOpener, entity, fileInfo, bufferSize);
            transmitEntityStrategyForByteArray.TransmitComplete(httpResponse);

            //Assert
        }
    }
}
