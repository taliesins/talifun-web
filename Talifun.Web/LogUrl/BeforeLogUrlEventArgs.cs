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

        public virtual bool Cancel { get; set; }
        public virtual HttpApplication HttpApplication { get; private set; }
        public virtual string Expression { get; private set; }
    }
}
