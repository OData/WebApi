// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.Security;

namespace System.Web.Helpers.AntiXsrf
{
    // Interfaces with the System.Web.MachineKey static class using the 4.5 Protect / Unprotect methods.
    internal sealed class MachineKey45CryptoSystem : ICryptoSystem
    {
        private static readonly string[] _purposes = new string[] { "System.Web.Helpers.AntiXsrf.AntiForgeryToken.v1" };
        private static readonly MachineKey45CryptoSystem _singletonInstance = GetSingletonInstance();

        private readonly Func<byte[], string[], byte[]> _protectThunk;
        private readonly Func<byte[], string[], byte[]> _unprotectThunk;

        // to get an instance of this type, use the static 'Instance' property rather than calling this ctor
        internal MachineKey45CryptoSystem(Func<byte[], string[], byte[]> protectThunk, Func<byte[], string[], byte[]> unprotectThunk)
        {
            _protectThunk = protectThunk;
            _unprotectThunk = unprotectThunk;
        }

        public static MachineKey45CryptoSystem Instance
        {
            get
            {
                return _singletonInstance;
            }
        }

        private static MachineKey45CryptoSystem GetSingletonInstance()
        {
            // Late bind to the MachineKey.Protect / Unprotect methods only if <httpRuntime targetFramework="4.5" />.
            // Though technically unsupported, this prevents the anti-XSRF system from breaking if a farm is running
            // in a mixed 4.0 / 4.5 environment.
            PropertyInfo targetFrameworkProperty = typeof(HttpRuntime).GetProperty("TargetFramework", typeof(Version));
            Version targetFramework = (targetFrameworkProperty != null) ? targetFrameworkProperty.GetValue(null, null) as Version : null;
            if (targetFramework != null && targetFramework >= new Version(4, 5))
            {
                Func<byte[], string[], byte[]> protectThunk = (Func<byte[], string[], byte[]>)Delegate.CreateDelegate(typeof(Func<byte[], string[], byte[]>), typeof(MachineKey), "Protect", ignoreCase: false, throwOnBindFailure: false);
                Func<byte[], string[], byte[]> unprotectThunk = (Func<byte[], string[], byte[]>)Delegate.CreateDelegate(typeof(Func<byte[], string[], byte[]>), typeof(MachineKey), "Unprotect", ignoreCase: false, throwOnBindFailure: false);
                if (protectThunk != null && unprotectThunk != null)
                {
                    return new MachineKey45CryptoSystem(protectThunk, unprotectThunk);
                }
            }

            // we can't call Protect / Unprotect
            return null;
        }

        public string Protect(byte[] data)
        {
            byte[] rawProtectedBytes = _protectThunk(data, _purposes);
            return HttpServerUtility.UrlTokenEncode(rawProtectedBytes);
        }

        public byte[] Unprotect(string protectedData)
        {
            byte[] rawProtectedBytes = HttpServerUtility.UrlTokenDecode(protectedData);
            return _unprotectThunk(rawProtectedBytes, _purposes);
        }
    }
}
