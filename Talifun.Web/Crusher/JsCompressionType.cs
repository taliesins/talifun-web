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
        /// Use jsmin to compress the js file
        /// </summary>
        Min
    }
}