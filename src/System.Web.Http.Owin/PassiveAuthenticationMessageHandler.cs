// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using System.Web.Http.Owin.Properties;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace System.Web.Http.Owin
{
    /// <summary>Represents a message handler that treats all OWIN authentication middleware as passive.</summary>
    /// <remarks>
    /// This message handler sets the current principal to anonymous upon entry and disables the default OWIN
    /// authentication middleware challenges. As a result, any default authentication performed by the host is ignored.
    /// The subsequent pipeline, including <see cref="IAuthenticationFilter"/>s, is then the exclusive authority for
    /// authentication.
    /// </remarks>
    public class PassiveAuthenticationMessageHandler : DelegatingHandler
    {
        private static readonly Lazy<IPrincipal> _anonymousPrincipal = new Lazy<IPrincipal>(
            () => new ClaimsPrincipal(new ClaimsIdentity()), isThreadSafe: true);

        private readonly IHostPrincipalService _principalService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PassiveAuthenticationMessageHandler"/> class.
        /// </summary>
        /// <param name="configuration">The configuration from which to use services.</param>
        public PassiveAuthenticationMessageHandler(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            Contract.Assert(configuration.Services != null);
            IHostPrincipalService principalService = configuration.Services.GetHostPrincipalService();

            if (principalService == null)
            {
                throw new InvalidOperationException(OwinResources.ServicesContainerIHostPrincipalServiceRequired);
            }

            _principalService = principalService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PassiveAuthenticationMessageHandler"/> class.
        /// </summary>
        /// <param name="principalService">The host principal service to use to access the current principal.</param>
        public PassiveAuthenticationMessageHandler(IHostPrincipalService principalService)
        {
            if (principalService == null)
            {
                throw new ArgumentNullException("principalService");
            }

            _principalService = principalService;
        }

        /// <summary>Gets the host principal service to use to access the current principal.</summary>
        public IHostPrincipalService HostPrincipalService
        {
            get
            {
                return _principalService;
            }
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            SetCurrentPrincipalToAnonymous(request);

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            SuppressDefaultAuthenticationChallenges(request);

            return response;
        }

        private void SetCurrentPrincipalToAnonymous(HttpRequestMessage request)
        {
            Contract.Assert(request != null);

            Contract.Assert(_principalService != null);
            _principalService.SetCurrentPrincipal(_anonymousPrincipal.Value, request);
        }

        private static void SuppressDefaultAuthenticationChallenges(HttpRequestMessage request)
        {
            Contract.Assert(request != null);

            IAuthenticationManager authenticationManager = request.GetAuthenticationManager();

            if (authenticationManager == null)
            {
                throw new InvalidOperationException(OwinResources.IAuthenticationManagerNotAvailable);
            }

            AuthenticationResponseChallenge currentChallenge = authenticationManager.AuthenticationResponseChallenge;

            // A null challenge or challenge.AuthenticationTypes == null or empty represents the the default behavior
            // of running all active authentication middleware challenges.
            // Provide an array with a single null item to suppress this default behavior.
            string[] suppressAuthenticationTypes = new string[] { null };

            if (currentChallenge == null)
            {
                authenticationManager.AuthenticationResponseChallenge = new AuthenticationResponseChallenge(
                    suppressAuthenticationTypes, new AuthenticationProperties());
            }
            else if (currentChallenge.AuthenticationTypes == null || currentChallenge.AuthenticationTypes.Length == 0)
            {
                authenticationManager.AuthenticationResponseChallenge = new AuthenticationResponseChallenge(
                    suppressAuthenticationTypes, currentChallenge.Properties);
            }
        }
    }
}
