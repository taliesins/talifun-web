using System;

namespace Talifun.Web.Crusher
{
    public interface ICssAssetsFileHasher
    {
        Uri AppendFileHash(Uri cssRootPath, Uri url);
    }
}