namespace Talifun.Web.Crusher
{
    public interface ICssAssetsFileHasher
    {
        string AppendFileHash(string cssFilePath, string url);
    }
}