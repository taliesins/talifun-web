namespace Talifun.Web.Crusher
{
    public class JsFile
    {
        /// <summary>
        /// The file path where the js file will be created.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Compression type to use on js file
        /// </summary>
        public JsCompressionType CompressionType { get; set; }
    }
}