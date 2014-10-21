using NUnit.Framework;
using Talifun.Web;
using Talifun.Web.Helper;

namespace Talifun.Crusher.Tests.Crusher
{
    [TestFixture]
    public class AmdModuleTests
    {
        [Test]
        public void GetModuleNameFromFileNameWithVersionMin()
        {
            var fileName = @"jquery-2.1.1.min.js";
            var expectedModuleName = @"jquery";

            var amdModule = new AmdModule(new RetryableFileOpener());
            var moduleName = amdModule.GetModuleName(fileName);

            Assert.AreEqual(expectedModuleName, moduleName);
        }

        [Test]
        public void GetModuleNameFromFileNameWithVersionMinAndDebug()
        {
            var fileName = @"jquery-2.1.1.min.debug.js";
            var expectedModuleName = @"jquery";

            var amdModule = new AmdModule(new RetryableFileOpener());
            var moduleName = amdModule.GetModuleName(fileName);

            Assert.AreEqual(expectedModuleName, moduleName);
        }

        [Test]
        public void GetModuleNameFromFileNameWithNothing()
        {
            var fileName = @"jquery.js";
            var expectedModuleName = @"jquery";

            var amdModule = new AmdModule(new RetryableFileOpener());
            var moduleName = amdModule.GetModuleName(fileName);

            Assert.AreEqual(expectedModuleName, moduleName);
        }

        [Test]
        public void GetModuleNameFromFileNameWithMinAndVersion()
        {
            var fileName = @"jquery-ui.min-1.11.1.js";
            var expectedModuleName = @"jquery-ui";

            var amdModule = new AmdModule(new RetryableFileOpener());
            var moduleName = amdModule.GetModuleName(fileName);

            Assert.AreEqual(expectedModuleName, moduleName);
        }
    }
}
