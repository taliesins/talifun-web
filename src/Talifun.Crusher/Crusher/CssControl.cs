using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Talifun.Crusher.Configuration;

namespace Talifun.Crusher.Crusher
{
    /// <summary>
    /// Generates the include references to css for a web page according to the provided configuration.
    /// </summary>
    public class CssControl : WebControl
    {
        /// <summary>
        /// The name of css group to generate the include headers for.
        /// </summary>
        public virtual string GroupName
        {
            get
            {
                var o = ViewState["GroupName"];
                return ((o == null) ? String.Empty : (string)o);
            }
            set
            {
                ViewState["GroupName"] = value;
            }
        }

        /// <summary>
        /// Generate the url for the crushed css file.
        /// </summary>
        /// <param name="writer"></param>
        /// <remarks>
        /// The generated url will also have a querystring with the hash of the file appended to it.
        /// </remarks>
        protected override void Render(HtmlTextWriter writer)
        {
            var scriptLinks = CrusherHelper.Css(GroupName);
            writer.Write(scriptLinks);
        }
    }
}