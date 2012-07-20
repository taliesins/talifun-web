namespace Talifun.Web.Crusher
{
    public class CssDirectory
    {
        /// <summary>
        /// Filter file names
        /// </summary>
        public virtual string IncludeFilter { get; set; }

        /// <summary>
        /// Exclude filter file names
        /// </summary>
        public virtual string ExcludeFilter { get; set; }

        /// <summary>
        /// The amount of time to wait without file changes before considering file changed
        /// </summary>
        public virtual int PollTime { get; set; }

        /// <summary>
		/// Should sub directories be scanned for css files as well. 
		/// </summary>
		public virtual bool IncludeSubDirectories { get; set; }

        /// <summary>
        /// The file path where the css file will be created.
        /// </summary>
        public virtual string DirectoryPath { get; set; }

        /// <summary>
        /// Compression type to use on css file
        /// </summary>
        public virtual CssCompressionType CompressionType { get; set; }
    }
}