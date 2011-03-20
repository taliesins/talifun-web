using System;
using NUnit.Framework;

namespace Talifun.Web.Tests
{
    public abstract class BehaviourTest<TSystemUnderTest>
    {
        private ExceptionMode _exceptionMode = ExceptionMode.Throw;
        private MockManager _mockManager;
        private AutoMocker<TSystemUnderTest> _autoMocker;

        protected Exception ThrownException { get; private set; }

        protected TSystemUnderTest SystemUnderTest { get; private set; }

        [TestFixtureSetUp]
        public void SetUp()
        {
            _mockManager = new MockManager();
            _autoMocker = new AutoMocker<TSystemUnderTest>(_mockManager);

            Given();

            SystemUnderTest = CreateSystemUnderTest();

            try
            {
                When();
            }
            catch (Exception ex)
            {
                if (_exceptionMode == ExceptionMode.Record)
                {
                    ThrownException = ex;
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                Teardown();
            }
        }

        protected abstract void Given();

        protected abstract void When();

        protected virtual void Teardown()
        {
        }

        /// <summary>
        /// Override this if TSystemUnderTest doesn't have a parameterless constructor.
        /// </summary>
        protected virtual TSystemUnderTest CreateSystemUnderTest()
        {
            return _autoMocker.ConstructedObject();
        }

        protected void RecordAnyExceptionsThrown()
        {
            _exceptionMode = ExceptionMode.Record;
        }

        protected TMock Mock<TMock>() where TMock : class
        {
            return _mockManager.Mock<TMock>();
        }

        #region Nested type: ExceptionMode

        private enum ExceptionMode
        {
            Throw,
            Record
        }

        #endregion
    }
}