namespace Talifun.Web
{
    /// <summary>
    /// The compression type to use when outputting file in compressed mode.
    /// </summary>
    public enum ResponseCompressionType
    {
        /// <summary>
        /// Do not compress the output of the file
        /// </summary>
        None,
        GZip,
        Deflate
    }
}