
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace Microsoft.AspNet.OData.Authorization
{
    /// <summary>
    /// Authorizarion requirement specifying the scopes required
    /// to authorize an OData request.
    /// </summary>
    public class ODataAuthorizationScopesRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Creates an instance of <see cref="ODataAuthorizationScopesRequirement"/>.
        /// </summary>
        /// <param name="allowedScopes">The scopes required to authorize a request where this requirement is applied.</param>
        public ODataAuthorizationScopesRequirement(params string[] allowedScopes)
        {
            AllowedScopes = allowedScopes;
        }

        /// <summary>
        /// The scopes specified by this authorization requirement.
        /// </summary>
        public IEnumerable<string> AllowedScopes { get; private set; }
    }
}
