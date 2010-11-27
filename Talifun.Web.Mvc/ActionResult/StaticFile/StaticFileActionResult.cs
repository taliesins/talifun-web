using System;
using System.IO;
using System.Web.Mvc;
using Talifun.Web.StaticFile;

namespace Talifun.Web.Mvc
{
    public class StaticFileActionResult : ActionResult
    {
        public StaticFileActionResult(FileInfo file)
        {
            File = file;
            if (file == null)
            {
                throw new ArgumentException("file to download must not be null");
            }
        }

        public FileInfo File { get; private set; }

        public override void ExecuteResult(ControllerContext context)
        {
            StaticFileManager.Instance.ProcessRequest(context.HttpContext, File);
        }
    }
}
