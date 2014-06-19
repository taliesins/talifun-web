using System.IO;

namespace Talifun.Web.Helper
{
    public interface IAmdModule
    {
        /// <summary>
        /// Does file specified by <paramref name="fileInfo" /> match regex specified by <paramref name="match" />
        /// </summary>
        /// <param name="fileInfo">A <see cref="FileInfo" /> object specifying the file containing the content for which we're finding the regex match in.</param>
        /// <returns>True if matches regex else false.</returns>
        bool IsAnonymousAmdModule(FileInfo fileInfo);

        bool IsAnonymousAmdModule(string content);

        string GetModuleName(string moduleName);

        string GetModuleHeader(string moduleName);

        string GetModuleFooter();
    }
}
