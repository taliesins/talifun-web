namespace Talifun.Web.Crusher
{
    public class JsDirectory
    {
		/// <summary>
		/// Should sub directories be scanned for js files as well. 
		/// </summary>
    	public virtual bool IncludeSubDirectories { get; set; }

        /// <summary>
        /// The file path where the js file will be created.
        /// </summary>
        public virtual string FilePath { get; set; }

        /// <summary>
        /// Compression type to use on js file
        /// </summary>
        public virtual JsCompressionType CompressionType { get; set; }
    }
}