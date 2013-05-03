// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Net.Http
{
    internal static class HttpRequestMessageExtensions
    {
        private const string OwinEnvironmentKey = "MS_OwinEnvironment";

        public static void SetOwinEnvironment(this HttpRequestMessage request, IDictionary<string, object> environment)
        {
            Contract.Assert(request != null);
            Contract.Assert(environment != null);

            request.Properties[OwinEnvironmentKey] = environment;
        }
    }
}