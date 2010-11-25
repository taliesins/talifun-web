using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Handlers;
using System.Web.UI;
using Talifun.Web.Module;

namespace Talifun.Web.Compress
{
    /// <summary>
    /// A Module that replace the System.Web.Handlers.AssemblyResourceLoader handler for better performance and 
    /// supporting compression (gzip & deflate).
    /// This module will noly handle WebResource files (with any content)
    /// </summary>
    public class WebResourceCompressionModule : HttpModuleBase
    {
        private static readonly TimeSpan Expires = new TimeSpan(7,0,0,0);
        private static readonly IDictionary WebResourceCache = Hashtable.Synchronized(new Hashtable());

        protected override void OnInit(HttpApplication httpApplication)
        {
            httpApplication.PreRequestHandlerExecute += OnPreRequestHandlerExecute;
        }

        protected static void OnPreRequestHandlerExecute(object sender, EventArgs e)
        {
            var httpApplication = (HttpApplication)sender;
            var httpContext = httpApplication.Context;

            if (!(httpContext.CurrentHandler is AssemblyResourceLoader)) return;

            var response = httpContext.Response;
            var request = httpContext.Request;

            var etag = (request.QueryString.ToString()).GetHashCode().ToString();

            // Check if the ETag is match. If so, we don't send nothing to the client, and stop here.
            CheckETag(httpApplication, etag);

            try
            {
                // Parse the QueryString into parts
                var urlInfo = GetDataFromQuery(httpApplication.Context.Request.QueryString);

                // Load the assembly
                var assembly = GetAssembly(urlInfo.First, urlInfo.Second);

                if (assembly == null) ThrowHttpException(404, SR.WebResourceCompressionModuleAssemblyNotFound, urlInfo.Forth);

                //var lastModified = File.GetLastWriteTimeUtc(assembly.Location);

                // Get the resource info from assembly.
                var resourceInfo = GetResourceInfo(assembly, urlInfo.Third);

                if (!resourceInfo.First) ThrowHttpException(404, SR.WebResourceCompressionModuleAssemblyNotFound, urlInfo.Forth);

                // If the WebResource needs to perform substitution (WebResource inside WebResource), we leave it to the original AssemblyResourceLoader handler ;-)
                if (resourceInfo.Second) return;

                response.Clear();

                // Set the response cache headers
                SetCachingHeadersForWebResource(response.Cache, etag, Expires);

                // Set the response content type
                response.ContentType = resourceInfo.Third;

                // Write content with compression
                if (resourceInfo.Forth && CompressionModuleHelper.IsCompressionSupported(httpContext, false))
                {
                    using (var resourceStream = new StreamReader(assembly.GetManifestResourceStream(urlInfo.Third), true))
                    {
                        CompressAndWriteToStream(resourceStream, httpApplication.Context);
                    }
                }
                    // Write content without compression
                else
                {
                    using (var resourceStream = assembly.GetManifestResourceStream(urlInfo.Third))
                    {
                        WriteToStream(resourceStream, response.OutputStream);
                    }
                }
                response.OutputStream.Flush();
                httpApplication.CompleteRequest();
            }
            catch (ArgumentNullException)
            {
                return;
            }
            catch (TargetInvocationException)
            {
                return;
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                return;
            }
        }

        /// <summary>
        /// Write a StreamReader to an outpit stream
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="context"></param>
        protected static void CompressAndWriteToStream(StreamReader reader, HttpContext context)
        {
            var compressionType = CompressionModuleHelper.IsSpecificEncodingSupported(context, CompressionModuleHelper.GZIP) ? CompressionModuleHelper.GZIP : CompressionModuleHelper.DEFLATE;
            CompressionModuleHelper.SetEncodingType(context.Response, compressionType);

            // All the supported content for compression can be read as string, so we use reader.ReadToEnd()
            var compressed = CompressionModuleHelper.Compressor(reader.ReadToEnd(), compressionType);
            context.Response.OutputStream.Write(compressed, 0, compressed.Length);
            reader.Dispose();
        }

        /// <summary>
        /// Write a StreamReader to an outpit stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="outputStream"></param>
        protected static void WriteToStream(Stream stream, Stream outputStream)
        {
            StreamCopy(stream, outputStream);
            stream.Dispose();
        }

        /// <summary>
        /// copy one stream to another
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        protected static void StreamCopy(Stream input, Stream output)
        {
            var buffer = new byte[4096];
            int read;
            do
            {
                read = input.Read(buffer, 0, buffer.Length);
                output.Write(buffer, 0, read);
            } while (read > 0);
        }


        /// <summary>
        /// Get the info about the resource that in the assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        protected static Quadruplet<bool, bool, string, bool> GetResourceInfo(Assembly assembly, string resourceName)
        {
            // Create a unique cache key
            var cacheKey = CombineHashCodes(assembly.GetHashCode(), resourceName.GetHashCode());

            var resourceInfo = WebResourceCache[cacheKey] as Quadruplet<bool, bool, string, bool>;

            // Assembly info was not in the cache
            if (resourceInfo == null)
            {
                var first = false;
                var second = false;
                var third = string.Empty;
                var forth = false;

                var customAttributes = assembly.GetCustomAttributes(false);
                for (var j = 0; j < customAttributes.Length; j++)
                {
                    var attribute = customAttributes[j] as WebResourceAttribute;
                    if ((attribute == null) || !string.Equals(attribute.WebResource, resourceName, StringComparison.Ordinal)) continue;

                    first = true;
                    second = attribute.PerformSubstitution;
                    third = attribute.ContentType;
                    forth = CompressionModuleHelper.IsContentTypeCompressible(attribute.ContentType);
                    break;
                }
                resourceInfo = new Quadruplet<bool, bool, string, bool>(first, second, third, forth);
                WebResourceCache[cacheKey] = resourceInfo;
            }
            return resourceInfo;
        }



        /// <summary>
        /// Load the specifid assembly curresponding to the given signal.
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        protected static Assembly GetAssembly(char signal, string assemblyName)
        {
            Assembly assembly = null;
            switch (signal)
            {
                case 's':
                    assembly = typeof(AssemblyResourceLoader).Assembly;
                    break;
                case 'p':
                    assembly = Assembly.Load(assemblyName);
                    break;
                case 'f':
                    {
                        var strArray = assemblyName.Split(new char[] { ',' });
                        if (strArray.Length != 4)
                        {
                            ThrowHttpException(400, SR.WebResourceCompressionModuleInvalidRequest);
                        }
                        var assemblyRef = new AssemblyName
                                              {
                                                  Name = strArray[0],
                                                  Version = new Version(strArray[1])
                                              };
                        var name = strArray[2];
                        assemblyRef.CultureInfo = name.Length > 0 ? new CultureInfo(name) : CultureInfo.InvariantCulture;
                        var tokens = strArray[3];
                        var publicKeyToken = new byte[tokens.Length / 2];
                        for (var i = 0; i < publicKeyToken.Length; i++)
                        {
                            publicKeyToken[i] = byte.Parse(tokens.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        }
                        assemblyRef.SetPublicKeyToken(publicKeyToken);
                        assembly = Assembly.Load(assemblyRef);
                        break;
                    }
                default:
                    ThrowHttpException(400, SR.WebResourceCompressionModuleInvalidRequest);
                    break;
            }
            return assembly;
        }

        /// <summary>
        /// Collect the necessary data from the query string
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        protected static Quadruplet<Char, String, String, String> GetDataFromQuery(NameValueCollection queryString)
        {
            var queryParam = queryString["d"];
            if (string.IsNullOrEmpty(queryParam))
            {
                ThrowHttpException(400, SR.WebResourceCompressionModuleInvalidRequest);
            }
            var decryptedParam = string.Empty; ;
            try
            {
                decryptedParam = Decryptor.DecryptString(queryParam);
            }
            catch (MethodAccessException mae)
            {
                ThrowHttpException(403, SR.WebResourceCompressionModuleReflectionNotAllowd, mae);
            }
            catch (Exception ex)
            {
                ThrowHttpException(400, SR.WebResourceCompressionModuleInvalidRequest, ex);
            }

            var pipeIndex = decryptedParam.IndexOf('|');

            if (pipeIndex < 1 || pipeIndex > (decryptedParam.Length - 2))
            {
                ThrowHttpException(404, SR.WebResourceCompressionModuleAssemblyNotFound, decryptedParam);
            }
            if (pipeIndex > (decryptedParam.Length - 2))
            {
                ThrowHttpException(404, SR.WebResourceCompressionModuleResourceNotFound, decryptedParam);
            }
            var assemblyName = decryptedParam.Substring(1, pipeIndex - 1);
            var resourceName = decryptedParam.Substring(pipeIndex + 1);
            return new Quadruplet<Char, String, String, String>(decryptedParam[0], assemblyName, resourceName, decryptedParam);
        }

        /// <summary>
        /// Check if the ETag that sent from the client is match to the current ETag.
        /// If so, set the status code to 'Not Modified' and stop the response.
        /// </summary>
        protected static void CheckETag(HttpApplication app, string etagCode)
        {
            var etag = "\"" + etagCode + "\"";
            var incomingEtag = app.Request.Headers["If-None-Match"];

            if (!String.Equals(incomingEtag, etag, StringComparison.OrdinalIgnoreCase)) return;

            app.Response.Cache.SetETag(etag);
            app.Response.AppendHeader("Content-Length", "0");
            app.Response.StatusCode = (int)HttpStatusCode.NotModified;
            app.Response.End();
        }

        /// <summary>
        /// Set the response cache headers for WebResource
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="etag"></param>
        /// <param name="maxAge"></param>
        protected static void SetCachingHeadersForWebResource(HttpCachePolicy cache, string etag, TimeSpan maxAge)
        {
            cache.SetCacheability(HttpCacheability.Public);
            cache.VaryByParams["d"] = true;
            cache.SetOmitVaryStar(true);
            cache.SetExpires(DateTime.Now.Add(maxAge));
            cache.SetValidUntilExpires(true);
            cache.VaryByHeaders["Accept-Encoding"] = true;
            cache.SetETag(string.Concat("\"", etag, "\""));
        }

        /// <summary>
        /// Combine two hash codes (From class: 'HashCodeCombiner' in the assembly: 'System.Web.Util')
        /// </summary>
        /// <param name="hash1"></param>
        /// <param name="hash2"></param>
        /// <returns></returns>
        protected static int CombineHashCodes(int hash1, int hash2)
        {
            return (((hash1 << 5) + hash1) ^ hash2);
        }

        protected static void ThrowHttpException(int num, string SRName)
        {
            throw new HttpException(num, SR.GetString(SRName));
        }

        protected static void ThrowHttpException(int num, string SRName, string param1)
        {
            throw new HttpException(num, SR.GetString(SRName, param1));
        }

        protected static void ThrowHttpException(int num, string SRName, Exception innerException)
        {
            throw new HttpException(num, SR.GetString(SRName), innerException);
        }
    }
}