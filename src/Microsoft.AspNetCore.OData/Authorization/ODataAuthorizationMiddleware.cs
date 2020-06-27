using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.Authorization;

namespace Microsoft.AspNet.OData.Authorization
{
    /// <summary>
    /// The OData authorization middleware
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

            var permissions = model.ExtractPermissionsForRequest(context);
            foreach (var perm in permissions)
            {
                ApplyRestrictions(perm, context);
            }

            await this.next(context);
        }

        private void ApplyRestrictions(PermissionData permissionData, HttpContext context)
        {

            var requirement = new ODataAuthorizationScopesRequirement(permissionData.Scopes.Select(s => s.Scope).ToArray());
            var policy = new AuthorizationPolicyBuilder(permissionData.SchemeName).AddRequirements(requirement).Build();

            // We use the AuthorizeFilter instead of relying on the built-in authorization middleware
            // because we cannot add new metadata to the endpoint in the middle of a request
            // and OData's current implementation of endpoint routing does not allow for
            // adding metadata to individual routes ahead of time
            var authFilter = new AuthorizeFilter(policy);
            context.ODataFeature().ActionDescriptor?.FilterDescriptors?.Add(new FilterDescriptor(authFilter, 0));
        }

    }
}
