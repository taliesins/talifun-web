using System.IO;
using System.Text;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;

namespace Talifun.Web.Tests.Http
{
    [TestFixture]
    public class TransmitEntityStrategyForByteArrayTests
    {
        #region Transmit
        [Test]
        public void Transmit_TransmitFullResponse_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var entity = MockRepository.GenerateMock<IEntity>();
            var text = "123456789012345678901234567890";
            var entityData = Encoding.ASCII.GetBytes(text);

            var outputStream = new MemoryStream();
            httpResponse.Stub(x => x.OutputStream).Return(outputStream);

            //Act
            var transmitEntityStrategyForByteArray = new TransmitEntityStrategyForByteArray(entity, entityData);
            transmitEntityStrategyForByteArray.Transmit(httpResponse);

            //Assert
            Assert.AreEqual(outputStream.ToArray(), entityData);
        }

        [Test]
        public void Transmit_TransmitPartialResponse_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var entity = MockRepository.GenerateMock<IEntity>();
            var text = "123456789012345678901234567890";
            var entityData = Encoding.ASCII.GetBytes(text);

            var offset = 5l;
            var length = 10l;

            var expectedByteArray = Encoding.ASCII.GetBytes(text.Substring((int)offset, (int)length));

            var outputStream = new MemoryStream();
            httpResponse.Stub(x => x.OutputStream).Return(outputStream);

            //Act
            var transmitEntityStrategyForByteArray = new TransmitEntityStrategyForByteArray(entity, entityData);
            transmitEntityStrategyForByteArray.Transmit(httpResponse, offset, length);

            //Assert
            Assert.AreEqual(outputStream.ToArray(), expectedByteArray);
        }

        [Test]
        public void Transmit_TransmitComplete_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var entity = MockRepository.GenerateMock<IEntity>();
            var text = "hello";
            var entityData = Encoding.ASCII.GetBytes(text);

            //Act
            var transmitEntityStrategyForByteArray = new TransmitEntityStrategyForByteArray(entity, entityData);
            transmitEntityStrategyForByteArray.TransmitComplete(httpResponse);

            //Assert
        }

        #endregion
    }
}
