namespace Talifun.Web.StaticFile
{
    /// <summary>
    /// The method to use when handling url etag
    /// </summary>
    public enum UrlEtagHandlingMethodType
    {
        /// <summary>
        /// Do not process url etag
        /// </summary>
        None,

        /// <summary>
        /// If url etag does not match etag then respond with the new content and specify the new url with the updated etag in the content location header.
        /// </summary>
        ContentLocation,

        /// <summary>
        /// If url etag does not match etag then redirect the current request to the new url with the updated etag.
        /// </summary>
        MovedPermanently,

        /// <summary>
        /// If url etag does not match etag then redirect the current request to the new url with the updated etag.
        /// </summary>
        TemporaryRedirect
    }
}
