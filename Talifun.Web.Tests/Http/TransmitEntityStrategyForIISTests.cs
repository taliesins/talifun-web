using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;

namespace Talifun.Web.Tests.Http
{
    [TestFixture]
    public class TransmitEntityStrategyForIISTests
    {
        #region Transmit
        [Test]
        public void Transmit_TransmitFullResponse_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var entity = MockRepository.GenerateMock<IEntity>();
            var filename = "test.zip";

            httpResponse.Stub(x => x.TransmitFile(filename));

            //Act
            var transmitEntityStrategyForIis = new TransmitEntityStrategyForIIS(entity, filename);
            transmitEntityStrategyForIis.Transmit(httpResponse);

            //Assert
            httpResponse.VerifyAllExpectations();
        }

        [Test]
        public void Transmit_TransmitPartialResponse_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var entity = MockRepository.GenerateMock<IEntity>();
            var filename = "test.zip";

            var offset = 2l;
            var length = 2l;

            httpResponse.Stub(x => x.TransmitFile(filename, offset, length));

            //Act
            var transmitEntityStrategyForIis = new TransmitEntityStrategyForIIS(entity, filename);
            transmitEntityStrategyForIis.Transmit(httpResponse, offset, length);

            //Assert
            httpResponse.VerifyAllExpectations();
        }

        #endregion

        [Test]
        public void Transmit_TransmitComplete_Void()
        {
            //Arrange
            var httpResponse = MockRepository.GenerateMock<HttpResponseBase>();
            var entity = MockRepository.GenerateMock<IEntity>();
            var filename = "test.zip";

            httpResponse.Expect(x => x.Flush());

            //Act
            var transmitEntityStrategyForIis = new TransmitEntityStrategyForIIS(entity, filename);
            transmitEntityStrategyForIis.TransmitComplete(httpResponse);

            //Assert
            httpResponse.VerifyAllExpectations();
        }
    }
}
