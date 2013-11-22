// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Threading;

namespace System.Web.Http.WebHost
{
    internal static class HttpResponseBaseExtensions
    {
        private static bool _isSystemWebVersion451OrGreater = IsSystemWebVersion451OrGreater();

        public static CancellationToken GetClientDisconnectedTokenWhenFixed(this HttpResponseBase response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            // On .NET 4.5 and earlier, using response.ClientDisconnectedToken can cause application crashes.
            // Gracefully degrade to CancellationToken.None unless running .NET 4.5.1 or later.
            if (!_isSystemWebVersion451OrGreater)
            {
                return CancellationToken.None;
            }

            return response.ClientDisconnectedToken;
        }

        private static bool IsSystemWebVersion451OrGreater()
        {
            Assembly systemWeb = typeof(HttpContextBase).Assembly;
            // System.Web.AspNetEventSource only exists in .NET 4.5.1 and will not be back-ported to .NET 4.5.
            return systemWeb.GetType("System.Web.AspNetEventSource") != null;
        }
    }
}
