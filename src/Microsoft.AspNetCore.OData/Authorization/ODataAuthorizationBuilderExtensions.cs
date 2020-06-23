using Microsoft.AspNet.OData.Authorization;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNet.OData.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class ODataAuthorizationBuilderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseODataAuthorization(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ODataAuthorizationMiddleware>();
        }
    }
}
