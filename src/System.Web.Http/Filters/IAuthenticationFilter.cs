// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace System.Web.Http.Filters
{
    public interface IAuthenticationFilter : IFilter
    {
        Task<IAuthenticationResult> AuthenticateAsync(HttpAuthenticationContext context,
            CancellationToken cancellationToken);

        Task<IHttpActionResult> ChallengeAsync(HttpActionContext context, IHttpActionResult result,
            CancellationToken cancellationToken);
    }
}
