using System.Web;

namespace Talifun.Web.StaticFile
{
    public interface IHttpRequestResponder
    {
        void ServeRequest(HttpRequestBase request, HttpResponseBase response, FileEntity fileEntity);
    }
}
