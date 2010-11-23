using System;
using System.Web;

namespace Talifun.Web.LogUrl
{
    public class LogUrlEventArgs : EventArgs
    {
        public LogUrlEventArgs(HttpApplication httpApplication, string expression)
        {
            HttpApplication = httpApplication;
            Expression = expression;
        }

        public virtual HttpApplication HttpApplication { get; private set; }
        public virtual string Expression { get; private set; }
    }
}