using System;
using System.Collections.Generic;
using System.Web;

namespace Talifun.Web.StaticFile
{
    public interface IHttpRequestResponder
    {
        IEntityResponse GetEntityResponse(HttpResponseBase response, IEnumerable<RangeItem> ranges);
        bool IsHttpMethodAllowed(HttpRequestBase request);
        HttpStatusCode GetResponseHttpStatus(HttpRequestBase request, DateTime lastModified, string etag);
    }
}
