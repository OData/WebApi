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
            IPrincipal originalPrincipal = _principalService.GetCurrentPrincipal(_request);
            HttpAuthenticationContext authenticationContext = new HttpAuthenticationContext(_context,
                originalPrincipal);

            for (int i = 0; i < _filters.Length; i++)
            {
                IAuthenticationFilter filter = _filters[i];
                await filter.AuthenticateAsync(authenticationContext, cancellationToken);

                IHttpActionResult error = authenticationContext.ErrorResult;

                // Short-circuit on the first authentication filter to provide an error result.
                if (error != null)
                {
                    result = error;
                    break;
                }
            }

            IPrincipal newPrincipal = authenticationContext.Principal;

            if (newPrincipal != originalPrincipal)
            {
                _principalService.SetCurrentPrincipal(authenticationContext.Principal, _request);
            }

            // Run challenge on all filters (passing the result of each into the next). If a filter failed, the
            // challenges run on the failure result. If no filter failed, the challenges run on the original inner
            // result.
            HttpAuthenticationChallengeContext challengeContext = new HttpAuthenticationChallengeContext(_context,
                result);

            for (int i = 0; i < _filters.Length; i++)
            {
                IAuthenticationFilter filter = _filters[i];
                await filter.ChallengeAsync(challengeContext, cancellationToken);
            }

            Contract.Assert(challengeContext.Result != null);
            result = challengeContext.Result;

            return await result.ExecuteAsync(cancellationToken);
        }
    }
}
