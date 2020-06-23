using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNet.OData.Authorization
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ODataAuthorizeAttribute : AuthorizeAttribute, IODataAuthorizeData, IFilterMetadata
    {
        string[] allowedScopes;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowedScopes"></param>
        public ODataAuthorizeAttribute(params string[] allowedScopes) : base()
        {
            this.allowedScopes = allowedScopes;
            AllowedScopes = allowedScopes;
            Policy = GeneratePolicyFromScopes();
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> AllowedScopes { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Scheme { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FieldRestrictions { get; set; }

        private string GeneratePolicyFromScopes()
        {
            var policy = string.Join(",", allowedScopes.OrderBy(s => s));
            return $"ODataScopes_{policy}";
        }
    }
}
