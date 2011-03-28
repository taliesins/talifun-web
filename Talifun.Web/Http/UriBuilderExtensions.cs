using System;
using System.Linq;
using System.Web;

namespace Talifun.Web
{
    public static class UriBuilderExtensions
    {
        public static void AddQueryArgument(this UriBuilder b, string key, string value)
        {
            var x = HttpUtility.ParseQueryString(b.Query);
            if (x.AllKeys.Contains(key)) throw new ArgumentOutOfRangeException(key, string.Format("Key '{0}' already exists!", key));
            x.Add(key, value);
            b.Query = x.ToString();
        }

        public static void EditQueryArgument(this UriBuilder b, string key, string value)
        {
            var x = HttpUtility.ParseQueryString(b.Query);
            if (x.AllKeys.Contains(key))
                x[key] = value;
            else throw new ArgumentNullException(key, string.Format("Key '{0}' does not exists!", key));
            b.Query = x.ToString();
        }

        public static void AddOrEditQueryArgument(this UriBuilder b, string key, string value)
        {
            var x = HttpUtility.ParseQueryString(b.Query);
            if (x.AllKeys.Contains(key))
                x[key] = value;
            else
                x.Add(key, value);
            b.Query = x.ToString();
        }

        public static void DeleteQueryArgument(this UriBuilder b, string key)
        {
            var x = HttpUtility.ParseQueryString(b.Query);
            if (x.AllKeys.Contains(key))
                x.Remove(key);
            b.Query = x.ToString();
        }
    }
}
