using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;

namespace Talifun.Web.Tests.Http
{
    [TestFixture]
    public class TransmitEntityStrategyForStreamTests
    {
        #region Transmit
        [Test]
        public void Transmit_TransmitFile_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var entity = MockRepository.GenerateMock<IEntity>();
            var bufferSize = 30;
            var text = "1234567890123456789012345678901234567890";
            var entityData = Encoding.ASCII.GetBytes(text);
            var stream = new MemoryStream(entityData);

            var outputStream = new MemoryStream();
            httpResponse.Stub(x => x.OutputStream).Return(outputStream);

            //Act
            var transmitEntityStrategyForStream = new TransmitEntityStrategyForStream(entity, stream, bufferSize);
            transmitEntityStrategyForStream.Transmit(httpResponse);

            //Assert
            Assert.AreEqual(outputStream.ToArray(), entityData);
        }

        [Test]
        public void Transmit_TransmitFilePartial_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var entity = MockRepository.GenerateMock<IEntity>();
            var bufferSize = 30;
            var text = "1234567890123456789012345678901234567890";
            var entityData = Encoding.ASCII.GetBytes(text);
            var stream = new MemoryStream(entityData);

            var offset = 2l;
            var length = 2l;

            var outputStream = new MemoryStream();
            httpResponse.Stub(x => x.OutputStream).Return(outputStream);

            //Act
            var transmitEntityStrategyForStream = new TransmitEntityStrategyForStream(entity, stream, bufferSize);
            transmitEntityStrategyForStream.Transmit(httpResponse, offset, length);

            //Assert
            Assert.AreEqual(outputStream.ToArray(), entityData.Skip((int)offset).Take((int)length).ToArray());
        }

        [Test]
        public void Transmit_TransmitComplete_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var entity = MockRepository.GenerateMock<IEntity>();
            var stream = MockRepository.GenerateMock<Stream>();
            var bufferSize = 30;

            //Act
            var transmitEntityStrategyForStream = new TransmitEntityStrategyForStream(entity, stream, bufferSize);
            transmitEntityStrategyForStream.TransmitComplete(httpResponse);

            //Assert
        }
        #endregion
    }
}
