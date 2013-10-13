using System;

namespace Talifun.Crusher.Crusher
{
    public interface ICssAssetsFileHasher
    {
        Uri AppendFileHash(Uri cssRootPath, Uri url);
    }
}