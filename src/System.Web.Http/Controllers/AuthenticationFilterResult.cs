// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;

namespace System.Web.Http.Controllers
{
    internal class AuthenticationFilterResult : IHttpActionResult
    {
        private readonly HttpActionContext _context;
        private readonly IAuthenticationFilter[] _filters;
        private readonly IHostPrincipalService _principalService;
        private readonly HttpRequestMessage _request;
        private readonly IHttpActionResult _innerResult;

        public AuthenticationFilterResult(HttpActionContext context, IAuthenticationFilter[] filters,
            IHostPrincipalService principalService, HttpRequestMessage request, IHttpActionResult innerResult)
        {
            Contract.Assert(context != null);
            Contract.Assert(filters != null);
            Contract.Assert(principalService != null);
            Contract.Assert(request != null);
            Contract.Assert(innerResult != null);

            _context = context;
            _filters = filters;
            _principalService = principalService;
            _request = request;
            _innerResult = innerResult;
        }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            IHttpActionResult result = _innerResult;
            HttpAuthenticationContext authenticationContext = new HttpAuthenticationContext(_context);
            authenticationContext.Principal = _principalService.GetCurrentPrincipal(_request);

            for (int i = 0; i < _filters.Length; i++)
            {
                IAuthenticationFilter filter = _filters[i];
                IAuthenticationResult authenticationResult = await filter.AuthenticateAsync(authenticationContext,
                    cancellationToken);

                if (authenticationResult != null)
                {
                    IHttpActionResult error = authenticationResult.ErrorResult;

                    // Short-circuit on the first authentication filter to provide an error result.
                    if (error != null)
                    {
                        result = error;
                        break;
                    }

                    IPrincipal principal = authenticationResult.Principal;

                    if (principal != null)
                    {
                        authenticationContext.Principal = principal;
                        _principalService.SetCurrentPrincipal(principal, _request);
                    }
                }
            }

            for (int i = 0; i < _filters.Length; i++)
            {
                IAuthenticationFilter filter = _filters[i];
                result = await filter.ChallengeAsync(_context, result, cancellationToken) ?? result;
            }

            return await result.ExecuteAsync(cancellationToken);
        }
    }
}
