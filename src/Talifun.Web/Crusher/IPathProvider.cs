using System;

namespace Talifun.Web.Crusher
{
    public interface IPathProvider
    {
        string MapPath(string uri);
        string MapPath(Uri uri);
        string MapPath(Uri rootPath, string url);
        string MapPath(Uri rootPath, Uri url);
        string ToAbsolute(string virtualPath);
        string ToAbsolute(string virtualPath, string applicationPath);
        
        Uri GetUriDirectory(Uri uri);
        //Uri GetRootPathUri(Uri rootUri);
        Uri GetAbsoluteUriDirectory(Uri uri);
        //Uri GetRelativeRootUri(string uri);
        Uri GetAbsoluteUriDirectory(string uri);
        Uri ToRelative(string uri);
    }
}
