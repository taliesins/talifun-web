using System;
using System.Web;

namespace Talifun.Web.LogUrl
{
    public class AfterLogUrlEventArgs : EventArgs
    {
        public AfterLogUrlEventArgs(HttpApplication httpApplication, string expression)
        {
            HttpApplication = httpApplication;
            Expression = expression;
        }

        public HttpApplication HttpApplication { get; private set; }
        public string Expression { get; private set; }
    }
}