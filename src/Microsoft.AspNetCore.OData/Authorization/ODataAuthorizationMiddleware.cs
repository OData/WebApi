using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace Microsoft.AspNet.OData.Authorization
{
    /// <summary>
    /// 
    /// </summary>
    public class ODataAuthorizationMiddleware
    {
        private RequestDelegate next;

        /// <summary>
        /// Instantiates a new instance of <see cref="ODataAuthorizationMiddleware"/>.
        /// </summary>
        public ODataAuthorizationMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// Invoke the middleware.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <returns>A task that can be awaited.</returns>
        public async Task Invoke(HttpContext context)
        {
            var odataFeature = context.ODataFeature();
            if (odataFeature == null || odataFeature.Path == null)
            {
                await this.next(context);
                return;
            }
            IEdmModel model = context.Request.GetModel();
            if (model == null)
            {
                await this.next(context);
                return;
            }

            var permissions = model.ExtractPermissionRestrictions(context);
            foreach (var perm in permissions)
            {
                ApplyRestrictions(perm, context);
            }

            await this.next(context);
        }

        private void ApplyRestrictions(PermissionData permissionData, HttpContext context)
        {
#if !NETSTANDARD2_0
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var auth = new ODataAuthorizeAttribute(permissionData.Scopes.Select(s => s.Scope).ToArray()) { Scheme = permissionData.SchemeName };
                var authFilter = new AuthorizeFilter(auth.Policy);
                context.ODataFeature().ActionDescriptor.FilterDescriptors.Add(new FilterDescriptor(authFilter, 0));
            }
#endif
        }

    }
}
