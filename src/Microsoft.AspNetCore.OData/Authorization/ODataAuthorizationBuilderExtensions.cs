using Microsoft.AspNetCore.Builder;
using Microsoft.AspNet.OData.Authorization;

namespace Microsoft.AspNet.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IApplicationBuilder"/> to add OData authorization
    /// </summary>
    public static class ODataAuthorizationBuilderExtensions
    {
        /// <summary>
        /// Use OData authorization to handle endpoint permissions based on capability restrictions
        /// defined in the model.
        /// This only works with endpoint routing.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to use.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseODataAuthorization(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ODataAuthorizationMiddleware>();
        }
    }
}
