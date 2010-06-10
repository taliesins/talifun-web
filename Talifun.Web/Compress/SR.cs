using System.Globalization;

namespace Talifun.Web.Compress
{
    internal static class SR
    {
        internal static string GetString(string strString)
        {
            return strString;
        }
        internal static string GetString(string strString, string param1)
        {
            return string.Format(CultureInfo.InvariantCulture, strString, param1);
        }
        internal static string GetString(string strString, string param1, string param2)
        {
            return string.Format(CultureInfo.InvariantCulture, strString, param1, param2);
        }

        internal const string WebResourceCompressionModule_InvalidRequest = "This is an invalid webresource request.";
        internal const string WebResourceCompressionModule_AssemblyNotFound = "Assembly {0} not found.";
        internal const string WebResourceCompressionModule_ResourceNotFound = "Resource {0} not found in assembly.";
        internal const string WebResourceCompressionModule_ReflectionNotAllowd = "Your server does not allow using reflection from your code. (Method: System.Reflection.MethodBase.Invoke(Object obj, Object[] parameters not allowed) Add the attribute 'reflectionAlloweded=\"false\"' to your web.config in the CompressorSettings section.";
        internal const string File_FileNotFound = "/* The requested file: '{0}' (phisical path: {1}) was not found */";
    }
}