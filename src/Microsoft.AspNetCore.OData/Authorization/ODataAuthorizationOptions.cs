using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNet.OData.Authorization
{
    /// <summary>
    /// Provides configuration for the OData authorization layer
    /// </summary>
    public class ODataAuthorizationOptions
    {
        /// <summary>
        /// Gets or sets the delegate used to find the scopes granted to the authenticated user
        /// from the authorization context.
        /// By default the library tries to get scopes from the principal's claims that have "Scope" as the key.
        /// </summary>
        public Func<AuthorizationHandlerContext, Task<IEnumerable<string>>> ScopesFinder { get; set; }
    }
}
