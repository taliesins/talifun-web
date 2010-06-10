using System;
using System.Web;

namespace Talifun.Web.LogUrl
{
    public class BeforeLogUrlEventArgs : EventArgs
    {
        public BeforeLogUrlEventArgs(HttpApplication httpApplication, string expression)
        {
            Cancel = false;
            HttpApplication = httpApplication;
            Expression = expression;
        }

        public bool Cancel { get; set; }
        public HttpApplication HttpApplication { get; private set; }
        public string Expression { get; private set; }
    }
}
