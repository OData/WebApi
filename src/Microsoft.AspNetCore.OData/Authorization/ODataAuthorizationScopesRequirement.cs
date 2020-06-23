using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNet.OData.Authorization
{
    /// <summary>
    /// 
    /// </summary>
    public class ODataAuthorizationScopesRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowedScopes"></param>
        public ODataAuthorizationScopesRequirement(params string[] allowedScopes)
        {
            AllowedScopes = allowedScopes;
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> AllowedScopes { get; private set; }
    }
}
