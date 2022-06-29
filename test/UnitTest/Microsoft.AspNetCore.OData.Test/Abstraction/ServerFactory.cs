//-----------------------------------------------------------------------------
// <copyright file="ServerFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
#if !NETCOREAPP2_0
    using Microsoft.AspNetCore.Http.Features;
#endif
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    /// <summary>
    /// Factory for creating a test servers.
    /// </summary>
    public class TestServerFactory
    {
        /// <summary>
        /// Create an TestServer.
        /// </summary>
        /// <param name="controllers">The controllers to use.</param>
        /// <param name="configureAction">The route configuration action.</param>
        /// <returns>An TestServer.</returns>
        public static TestServer Create(Type[] controllers, Action<IRouteBuilder> configureAction, Action<IServiceCollection> configureService = null)
        {
            IWebHostBuilder builder = WebHost.CreateDefaultBuilder();
            builder.ConfigureServices(services =>
            {
#if NETCOREAPP2_0
                services.AddMvc();
#else
                services.AddMvc(options => options.EnableEndpointRouting = false)
                    .AddNewtonsoftJson();  
#endif

                services.AddOData();
                configureService?.Invoke(services);
            });

            builder.Configure(app =>
            {
#if !NETCOREAPP2_0
                app.Use(next => context =>
                {
                    var body = context.Features.Get<IHttpBodyControlFeature>();
                    if (body != null)
                    {
                        body.AllowSynchronousIO = true;
                    }

                    return next(context);
                });
#endif

                app.UseODataBatching();
                app.UseMvc((routeBuilder) =>
                {
                    configureAction(routeBuilder);

                    ApplicationPartManager applicationPartManager = routeBuilder.ApplicationBuilder.ApplicationServices.GetRequiredService<ApplicationPartManager>();
                    applicationPartManager.ApplicationParts.Clear();

                    if (controllers != null)
                    {
                        AssemblyPart part = new AssemblyPart(new MockAssembly(controllers));
                        applicationPartManager.ApplicationParts.Add(part);
                    }

                    // Insert a custom ControllerFeatureProvider to bypass the IsPublic restriction of controllers
                    // to allow for nested controllers which are excluded by the built-in ControllerFeatureProvider.
                    applicationPartManager.FeatureProviders.Clear();
                    applicationPartManager.FeatureProviders.Add(new TestControllerFeatureProvider());
                });
            });

            return new TestServer(builder);
        }

        /// <summary>
        /// Create an TestServer with formatters.
        /// </summary>
        /// <param name="controllers">The controllers to use.</param>
        /// <param name="formatters">A list of formatters to use.</param>
        /// <param name="configureAction">The route configuration action.</param>
        /// <returns>An TestServer.</returns>
        public static TestServer CreateWithFormatters(
            Type[] controllers,
            IEnumerable<ODataOutputFormatter> formatters,
            Action<IRouteBuilder> configureAction)
        {
            // AspNetCore's create adds the formatters by default.
            TestServer server = Create(controllers, configureAction);
            return server;
        }

        /// <summary>
        /// Create an HttpClient from a server.
        /// </summary>
        /// <param name="server">The TestServer.</param>
        /// <returns>An HttpClient.</returns>
        public static HttpClient CreateClient(TestServer server)
        {
            return server.CreateClient();
        }

        private class TestControllerFeatureProvider : ControllerFeatureProvider
        {
            /// <inheritdoc />
            /// <remarks>
            /// Identical to ControllerFeatureProvider.IsController except for the typeInfo.IsPublic check.
            /// </remarks>
            protected override bool IsController(TypeInfo typeInfo)
            {
                if (!typeInfo.IsClass)
                {
                    return false;
                }

                if (typeInfo.IsAbstract)
                {
                    return false;
                }

                if (typeInfo.ContainsGenericParameters)
                {
                    return false;
                }

                if (typeInfo.IsDefined(typeof(NonControllerAttribute)))
                {
                    return false;
                }

                if (!typeInfo.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase) &&
                    !typeInfo.IsDefined(typeof(ControllerAttribute)))
                {
                    return false;
                }

                return true;
            }
        }
    }
}
