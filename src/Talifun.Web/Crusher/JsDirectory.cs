namespace Talifun.Web.Crusher
{
    public class JsDirectory
    {
        /// <summary>
        /// Filter file names
        /// </summary>
        public virtual string Filter { get; set; }

        /// <summary>
        /// The amount of time to wait without file changes before considering file changed
        /// </summary>
        public virtual int PollTime { get; set; }

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