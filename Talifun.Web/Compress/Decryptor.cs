using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Web.UI;

namespace Talifun.Web.Compress
{
    public class Decryptor
    {
        protected const bool ReflectionAlloweded = true;
        protected static MethodInfo decryptString;
        protected static readonly Object getMethodLock = new Object();

        /// <summary>
        /// Decrypt a string using MachineKey
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [
            ReflectionPermission(SecurityAction.Assert, Unrestricted = true),
            SecurityCritical,
            SecurityTreatAsSafe
        ]
        internal static string DecryptString(string input)
        {
            if (!ReflectionAlloweded)
            {
                return EmptyMembershipProvider.Instance.DecryptString(input);
            }

            if (decryptString == null)
            {
                lock (getMethodLock)
                {
                    if (decryptString == null)
                    {
                        decryptString = typeof(Page).GetMethod("DecryptString", BindingFlags.Static | BindingFlags.NonPublic);
                    }
                }
            }

            return (string)decryptString.Invoke(null, new object[] { input });
        }
    }
}