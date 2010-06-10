namespace Talifun.Web.Crusher
{
    /// <summary>
    /// The type of compress to use when crushing css
    /// </summary>
    public enum CssCompressionType
    {
        /// <summary>
        /// Do not use any compression at all on css file
        /// </summary>
        None,
        StockYuiCompressor,
        MichaelAshRegexEnhancements,
        Hybrid
    }
}