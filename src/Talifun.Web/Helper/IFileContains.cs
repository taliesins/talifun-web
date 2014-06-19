using System.IO;
using System.Text.RegularExpressions;

namespace Talifun.Web.Helper
{
    public interface IFileContains
    {
        /// <summary>
        /// Does file specified by <paramref name="fileInfo" /> contain substring specified by <paramref name="substring" />
        /// </summary>
        /// <param name="fileInfo">A <see cref="FileInfo" /> object specifying the file containing the content for which we're finding the substring in.</param>
        /// <param name="substring">The string to search for</param>
        /// <returns>True if contains string else false.</returns>
        bool Contains(FileInfo fileInfo, string substring);

        /// <summary>
        /// Does file specified by <paramref name="fileInfo" /> match regex specified by <paramref name="match" />
        /// </summary>
        /// <param name="fileInfo">A <see cref="FileInfo" /> object specifying the file containing the content for which we're finding the regex match in.</param>
        /// <param name="match">The regex to match on</param>
        /// <returns>True if matches regex else false.</returns>
        bool Contains(FileInfo fileInfo, Regex match);
    }
}
