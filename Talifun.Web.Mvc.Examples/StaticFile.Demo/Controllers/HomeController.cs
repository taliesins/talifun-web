using System.IO;
using System.Web.Mvc;
using Talifun.Web.Mvc;

namespace StaticFile.Demo.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public StaticFileActionResult GetTestZip()
        {
            var filePath = Server.MapPath("~/Static/test.zip");
            var fileInfo = new FileInfo(filePath);
            return new StaticFileActionResult(fileInfo);
        }

        public StaticFileActionResult GetImage()
        {
            var filePath = Server.MapPath("~/Static/Images/Main.jpg");
            var fileInfo = new FileInfo(filePath);
            return new StaticFileActionResult(fileInfo);
        }

        public StaticFileActionResult GetCss()
        {
            var filePath = Server.MapPath("~/Static/Css/CssAdapter.css");
            var fileInfo = new FileInfo(filePath);
            return new StaticFileActionResult(fileInfo);
        }
    }
}
