namespace Talifun.Web.Crusher
{
    /// <summary>
    /// The type of compress to use when crushing js
    /// </summary>
    public enum JsCompressionType
    {
        /// <summary>
        /// Do not use any compression at all on js file
        /// </summary>
        None,

        /// <summary>
		/// Compress js using Yahoo Yui Compressor method.
        /// </summary>
        YahooYui,

		/// <summary>
		/// Compress js using Microsoft Ajax Min Compressor method.
		/// </summary>
		MicrosoftAjaxMin,
    }
}