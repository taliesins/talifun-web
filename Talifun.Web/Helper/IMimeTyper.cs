namespace Talifun.Web
{
    public interface IMimeTyper
    {
        /// <summary>
        /// Get the mime type for a file based on its extension
        /// </summary>
        /// <param name="extension">The extension of the file</param>
        /// <returns>Mime type of a file</returns>
        string GetMimeType(string extension);
    }
}