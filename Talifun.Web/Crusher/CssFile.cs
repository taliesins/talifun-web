namespace Talifun.Web.Crusher
{
    public class CssFile
    {
        /// <summary>
        /// The file path where the css file will be created.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Compression type to use on css file
        /// </summary>
        public CssCompressionType CompressionType { get; set; }
    }
}