// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http;

namespace System.Net.Http
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class OwinHttpRequestMessageExtensions
    {
        private const string OwinEnvironmentKey = "MS_OwinEnvironment";

        /// <summary>
        /// Gets the OWIN environment for the specified request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The OWIN environment for the specified request.</returns>
        public static IDictionary<string, object> GetOwinEnvironment(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            IDictionary<string, object> environment;
            request.Properties.TryGetValue<IDictionary<string, object>>(OwinEnvironmentKey, out environment);
            return environment;
        }

        /// <summary>
        /// Sets the OWIN environment for the specified request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="environment">The OWIN environment to set.</param>
        public static void SetOwinEnvironment(this HttpRequestMessage request, IDictionary<string, object> environment)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }
            if (environment == null)
            {
                throw Error.ArgumentNull("environment");
            }

            request.Properties[OwinEnvironmentKey] = environment;
        }
    }
}