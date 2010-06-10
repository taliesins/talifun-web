using System.Collections.Generic;
using System.Collections.Specialized;
using Talifun.Web.UrlRewriter;

namespace UrlReWriter.Demo.CustomUrlRewriter
{
    public class StaticUrlTransform : StaticUrlTransformBase
    {
        protected override string RewriteUrl(List<string> filePath, string fileName, string fileExtension, NameValueCollection queryString, string bookMark)
        {
            return UrlToString(filePath, fileName, fileExtension, queryString, bookMark);
        }

        public override string Name
        {
            get { return "StaticUrlTransform"; }
        }
    }
}