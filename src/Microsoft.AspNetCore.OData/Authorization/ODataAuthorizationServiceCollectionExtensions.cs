using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData.Extensions
{

    /// <summary>
    /// Provides authorization extensions for <see cref="IServiceCollection"/>
    /// </summary>
    public static class ODataAuthorizationServiceCollectionExtensions
    {
        /// <summary>
        /// Enables OData model-based authorization
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="getScopes">Function to retrieve the authenticated user's scopes from the authorization context</param>
        /// <returns></returns>
        public static IServiceCollection AddODataAuthorization(this IServiceCollection services, Func<AuthorizationHandlerContext, Task<IEnumerable<string>>> getScopes = null)
        {
            services.AddSingleton<IAuthorizationHandler, ODataAuthorizationHandler>(_ => new ODataAuthorizationHandler(getScopes));
            return services;
        }
    }
}
