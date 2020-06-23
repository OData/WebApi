using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.OData.Authorization
{
    /// <summary>
    /// 
    /// </summary>
    public class ODataAuthorizationPolicyProvider : IAuthorizationPolicyProvider
    {
        const string POLICY_PREFIX = "ODataScopes_";
        IAuthorizationPolicyProvider fallbackProvider;

        /// <summary>
        /// 
        /// </summary>
        public ODataAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            fallbackProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return fallbackProvider.GetDefaultPolicyAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<AuthorizationPolicy> GetFallbackPolicyAsync()
        {
            return fallbackProvider.GetDefaultPolicyAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="policyName"></param>
        /// <returns></returns>
        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(POLICY_PREFIX))
            {

                var scopes = policyName.Substring(POLICY_PREFIX.Length).Split(',');
                var requirement = new ODataAuthorizationScopesRequirement(scopes);
                var policy = new AuthorizationPolicyBuilder("AuthScheme").AddRequirements(requirement).Build();
                return Task.FromResult(policy);
            }

            return Task.FromResult(default(AuthorizationPolicy));
        }
    }
}
