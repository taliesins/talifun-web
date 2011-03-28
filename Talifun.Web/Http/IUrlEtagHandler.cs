using System;
using System.Web;

namespace Talifun.Web
{
    public interface IUrlEtagHandler
    {
        bool UpdateEtag(HttpResponseBase response, Uri uri, string etag);
    }
}
