using System;
using System.IO;

namespace Talifun.Web.Crusher
{
    public class CssAssetsFileHasher : ICssAssetsFileHasher
    {
        protected readonly string HashQueryStringKeyName;
        protected readonly IHasher Hasher;
        protected readonly IPathProvider PathProvider;

        public CssAssetsFileHasher(string hashQueryStringKeyName, IHasher hasher, IPathProvider pathProvider)
        {
            HashQueryStringKeyName = hashQueryStringKeyName;
            Hasher = hasher;
            PathProvider = pathProvider;
        }

        public virtual Uri AppendFileHash(Uri cssRootPath, Uri url)
        {
            if (url.IsAbsoluteUri) return url;

            var fileInfo = new FileInfo(PathProvider.MapPath(cssRootPath, url));

            if (!fileInfo.Exists)
            {
                return url;
            }

            var hash = Hasher.CalculateMd5Etag(fileInfo);

            var uriBuilder = new UriBuilder(url);
            uriBuilder.AddQueryArgument(HashQueryStringKeyName, hash);
            return uriBuilder.Uri;
        }
    }
}