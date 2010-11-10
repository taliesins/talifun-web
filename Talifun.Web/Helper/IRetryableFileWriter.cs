using System.IO;
using System.Text;

namespace Talifun.Web.Helper
{
    public interface IRetryableFileWriter
    {
        void SaveContentsToFile(StringBuilder output, string outputPath);
        void SaveContentsToFile(Stream outputStream, string outputPath);
    }
}
