// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Properties;

namespace System.Web.Http.Hosting
{
    public class SuppressHostPrincipalMessageHandler : DelegatingHandler
    {
        private static readonly Lazy<IPrincipal> _anonymousPrincipal = new Lazy<IPrincipal>(
            () => new ClaimsPrincipal(new ClaimsIdentity()), isThreadSafe: true);

        private readonly IHostPrincipalService _principalService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SuppressHostPrincipalMessageHandler"/> class.
        /// </summary>
        /// <param name="configuration">The configuration from which to use services.</param>
        public SuppressHostPrincipalMessageHandler(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            Contract.Assert(configuration.Services != null);
            IHostPrincipalService principalService = configuration.Services.GetHostPrincipalService();

            if (principalService == null)
            {
                throw new InvalidOperationException(SRResources.ServicesContainerIHostPrincipalServiceRequired);
            }

            _principalService = principalService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuppressHostPrincipalMessageHandler"/> class.
        /// </summary>
        /// <param name="principalService">The host principal service to use to access the current principal.</param>
        public SuppressHostPrincipalMessageHandler(IHostPrincipalService principalService)
        {
            if (principalService == null)
            {
                throw new ArgumentNullException("principalService");
            }

            _principalService = principalService;
        }

        /// <summary>
        /// Gets the host principal service to use to access the current principal.
        /// </summary>
        public IHostPrincipalService HostPrincipalService
        {
            get
            {
                return _principalService;
            }
        }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            SetCurrentPrincipalToAnonymous(request);

            return base.SendAsync(request, cancellationToken);
        }

        private void SetCurrentPrincipalToAnonymous(HttpRequestMessage request)
        {
            Contract.Assert(request != null);

            Contract.Assert(_principalService != null);
            _principalService.SetCurrentPrincipal(_anonymousPrincipal.Value, request);
        }
    }
}
