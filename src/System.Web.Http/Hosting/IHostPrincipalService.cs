// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Principal;

namespace System.Web.Http.Hosting
{
    /// <summary>Defines a service for accessing the principal at the host layer.</summary>
    public interface IHostPrincipalService
    {
        /// <summary>Get the principal at the host layer.</summary>
        /// <param name="request">The request associated with the host context.</param>
        /// <returns>The principal at the host layer.</returns>
        IPrincipal GetCurrentPrincipal(HttpRequestMessage request);

        /// <summary>
        /// Sets the principal at the host layer.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <param name="request">The request associated with the host context.</param>
        void SetCurrentPrincipal(IPrincipal principal, HttpRequestMessage request);
    }
}
