namespace Talifun.Web.Crusher
{
    public class CssDirectory
    {
        /// <summary>
        /// The file path where the css file will be created.
        /// </summary>
        public virtual string FilePath { get; set; }

        /// <summary>
        /// Compression type to use on css file
        /// </summary>
        public virtual CssCompressionType CompressionType { get; set; }
    }
}