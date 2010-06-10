using System;
using System.Web.Mvc;

namespace Talifun.Web.Mvc
{
    public class ContentDispositionActionResult : ContentResult
    {
        public string Filename { get; set; }

        public override void ExecuteResult(ControllerContext ctx)
        {
            if (!string.IsNullOrEmpty(Filename))
            {
                ctx.HttpContext.Response.AppendHeader("content-disposition", String.Format("attachment; filename={0}", Filename));
            }
            base.ExecuteResult(ctx);
        }
    }
}
