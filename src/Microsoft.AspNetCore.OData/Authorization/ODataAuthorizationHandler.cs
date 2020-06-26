using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Microsoft.AspNet.OData.Authorization
{
    /// <summary>
    /// Decides whether an OData request should be authorized or denied.
    /// </summary>
    public class ODataAuthorizationHandler : AuthorizationHandler<ODataAuthorizationScopesRequirement>
    {
        /// <summary>
        /// Makes decision whether authorization should be allowed based on the provided scopes.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The <see cref="ODataAuthorizationScopesRequirement"/> defining the scopes required
        /// for authorization to succeed.</param>
        /// <returns></returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ODataAuthorizationScopesRequirement requirement)
        {
            var claim = context.User?.FindFirst("Scope");
            if (claim != null)
            {
                if (requirement.AllowedScopes.Contains(claim.Value))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
