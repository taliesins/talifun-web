namespace Talifun.Web.StaticFile
{
    /// <summary>
    /// The method to use when calculating the etag (hash) of the file
    /// </summary>
    public enum EtagMethodType
    {
        /// <summary>
        /// The MD5 hash of the file's contents
        /// </summary>
        MD5,
        /// <summary>
        /// The date time the file was last accessed
        /// </summary>
        LastModified
    }
}