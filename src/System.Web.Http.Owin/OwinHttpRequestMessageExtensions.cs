// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http.Owin;
using System.Web.Http.Owin.Properties;
using Microsoft.Owin;

namespace System.Net.Http
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class OwinHttpRequestMessageExtensions
    {
        private const string OwinEnvironmentKey = "MS_OwinEnvironment";

        /// <summary>Gets the OWIN environment for the specified request.</summary>
        /// <param name="request">The HTTP request message.</param>
        /// <returns>
        /// The OWIN environment for the specified request, if available; otherwise <see langword="null"/>.
        /// </returns>
        public static IDictionary<string, object> GetOwinEnvironment(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            IDictionary<string, object> environment;
            request.Properties.TryGetValue<IDictionary<string, object>>(OwinEnvironmentKey, out environment);
            return environment;
        }

        /// <summary>Sets the OWIN environment for the specified request.</summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="environment">The OWIN environment to set.</param>
        public static void SetOwinEnvironment(this HttpRequestMessage request, IDictionary<string, object> environment)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            request.Properties[OwinEnvironmentKey] = environment;
        }

        /// <summary>Gets the OWIN request for the specified request.</summary>
        /// <param name="request">The HTTP request message.</param>
        /// <returns>The OWIN request for the specified request.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no OWIN environment is available for the request.
        /// </exception>
        public static OwinRequest GetOwinRequest(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            IDictionary<string, object> environment = GetOwinEnvironment(request);

            if (environment == null)
            {
                throw new InvalidOperationException(OwinResources.OwinEnvironmentNotAvailable);
            }

            return new OwinRequest(environment);
        }

        /// <summary>Gets the OWIN response for with the specified request.</summary>
        /// <param name="request">The HTTP request message.</param>
        /// <returns>The OWIN response for with the specified request.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no OWIN environment is available for the request.
        /// </exception>
        public static OwinResponse GetOwinResponse(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            IDictionary<string, object> environment = GetOwinEnvironment(request);

            if (environment == null)
            {
                throw new InvalidOperationException(OwinResources.OwinEnvironmentNotAvailable);
            }

            return new OwinResponse(environment);
        }
    }
}
