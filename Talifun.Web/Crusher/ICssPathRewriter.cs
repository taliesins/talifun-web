namespace Talifun.Web.Crusher
{
    public interface ICssPathRewriter
    {
        string RewriteCssPaths(string outputPath, string sourcePath, string css);
    }
}
