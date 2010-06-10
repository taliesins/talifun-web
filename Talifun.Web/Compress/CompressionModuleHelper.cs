using System;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web;

namespace Talifun.Web.Compress
{
    public static class CompressionModuleHelper
    {
        public const string GZIP = "gzip";
        public const string DEFLATE = "deflate";
        public const string InternetExplorer = "IE";

        private static readonly StringDictionary compressibleTypes = new StringDictionary
                                                                         {
                                                                             {"text/css", null},
                                                                             {"application/x-javascript", null},
                                                                             {"text/javascript", null},
                                                                             {"text/html", null},
                                                                             {"text/plain", null}
                                                                         };

        /// <summary>
        /// Check if the current request is an AsyncCall
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static bool IsAjaxPostBackRequest(HttpContext httpContext)
        {
            return httpContext.Request.Headers["X-MicrosoftAjax"] != null || httpContext.Request.ContentType.ToLower().Contains("application/json");
        }

        /// <summary>
        /// Check if the browser support compression
        /// </summary>
        /// <param name="context"></param>
        /// <param name="isPage"
        /// <returns></returns>
        public static bool IsCompressionSupported(HttpContext context, bool isPage)
        {
            if (context == null || context.Request == null || context.Request.Browser == null)
                return false;

            if (context.Request.Headers["Accept-encoding"] == null || !(context.Request.Headers["Accept-encoding"].Contains(GZIP) || context.Request.Headers["Accept-encoding"].Contains(DEFLATE)))
                return false;

            if (!context.Request.Browser.IsBrowser(InternetExplorer))
                return true;

            if (context.Request.Params["SERVER_PROTOCOL"] != null && context.Request.Params["SERVER_PROTOCOL"].Contains("1.1"))
                return true;

            return false;
        }

        /// <summary>
        /// Adds the specified encoding to the response header.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="encoding"></param>
        public static void SetEncodingType(HttpResponse response, string encoding)
        {
            if (response != null)
                response.AppendHeader("Content-encoding", encoding);
        }

        /// <summary>
        /// Check if specific encoding is supported
        /// </summary>
        /// <param name="context"></param>
        /// <param name="encodingType"></param>
        /// <returns></returns>
        public static bool IsSpecificEncodingSupported(HttpContext context, string encodingType)
        {
            return context.Request.Headers["Accept-encoding"] != null
                   && context.Request.Headers["Accept-encoding"].Contains(encodingType);
        }

        /// <summary>
        /// Compress a given string using a given algorithm
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encodingType"></param>
        /// <returns></returns>
        public static byte[] Compressor(string input, string encodingType)
        {
            return input == null ? null : Compressor(Encoding.ASCII.GetBytes(input), encodingType);
        }

        /// <summary>
        /// Compress a given byte[] using a given algorithm
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="encodingType"></param>
        /// <returns></returns>
        public static byte[] Compressor(byte[] buffer, string encodingType)
        {
            if (buffer == null) return null;

            using (var memStream = new MemoryStream())
            {
                Stream compress = null;

                // Choose the compression type and make the compression
                if (String.Equals(encodingType, GZIP, StringComparison.Ordinal))
                {
                    compress = new GZipStream(memStream, CompressionMode.Compress);
                }
                else if (String.Equals(encodingType, DEFLATE, StringComparison.Ordinal))
                {
                    compress = new DeflateStream(memStream, CompressionMode.Compress);
                }
                else
                {
                    // No compression
                    return buffer;
                }

                compress.Write(buffer, 0, buffer.Length);
                compress.Dispose();

                return memStream.ToArray();
            }
        }

        /// <summary>
        /// Check if a specific content type is compressible
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static bool IsContentTypeCompressible(string contentType)
        {
            return compressibleTypes.ContainsKey(contentType);
        }
    }
}