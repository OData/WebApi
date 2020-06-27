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
        /// <param name="configureOptions">Action to configure the authorization options</param>
        /// <returns></returns>
        public static IServiceCollection AddODataAuthorization(this IServiceCollection services, Action<ODataAuthorizationOptions> configureOptions = null)
        {
            var options = new ODataAuthorizationOptions();
            configureOptions?.Invoke(options);
            services.AddSingleton<IAuthorizationHandler, ODataAuthorizationHandler>(_ => new ODataAuthorizationHandler(options.ScopesFinder));
            return services;
        }
    }
}
