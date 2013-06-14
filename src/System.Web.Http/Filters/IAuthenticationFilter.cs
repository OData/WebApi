// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace System.Web.Http.Filters
{
    /// <summary>Defines a filter that performs authentication.</summary>
    public interface IAuthenticationFilter : IFilter
    {
        /// <summary>Authenticates the request.</summary>
        /// <param name="context">The authentication context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The authentication result.</returns>
        Task<IAuthenticationResult> AuthenticateAsync(HttpAuthenticationContext context,
            CancellationToken cancellationToken);

        /// <summary>Adds an authentication challenge to the inner <see cref="IHttpActionResult"/>.</summary>
        /// <param name="context">The action context.</param>
        /// <param name="innerResult">The current action result.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The new action result.</returns>
        Task<IHttpActionResult> ChallengeAsync(HttpActionContext context, IHttpActionResult innerResult,
            CancellationToken cancellationToken);
    }
}
