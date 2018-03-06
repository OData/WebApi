// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.Common;
#else
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.Test.AspNet.OData.Common;
#endif

namespace Microsoft.Test.AspNet.OData.Factories
{
    /// <summary>
    /// Factory for creating a test servers.
    /// </summary>
    public class TestServerFactory
    {
#if NETCORE
        /// <summary>
        /// Create an TestServer.
        /// </summary>
        /// <param name="controllers">The controllers to use.</param>
        /// <param name="configureAction">The route configuration action.</param>
        /// <returns>An TestServer.</returns>
        public static TestServer Create(
            Type[] controllers,
            Action<IRouteBuilder> configureAction)
        {
            IWebHostBuilder builder = WebHost.CreateDefaultBuilder();
            builder.ConfigureServices(services =>
            {
                services.AddMvc();
                services.AddOData();
            });

            builder.Configure(app =>
            {
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
#else
            /// <summary>
            /// Create an HttpServer.
            /// </summary>
            /// <param name="controllers">The controllers to use.</param>
            /// <param name="configureAction">The route configuration action.</param>
            /// <returns>An HttpServer.</returns>
            public static HttpServer Create(
            Type[] controllers,
            Action<HttpConfiguration> configureAction)
        {
            HttpConfiguration configuration = new HttpConfiguration();
            return Create(configuration, controllers, configureAction);
        }

        /// <summary>
        /// Create an TestServer with formatters.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="controllers">The controllers to use.</param>
        /// <param name="getModelFunction">A function to get the model.</param>
        /// <returns>An HttpServer.</returns>
        public static HttpServer CreateWithFormatters(
            Type[] controllers,
            IEnumerable<ODataMediaTypeFormatter> formatters,
            Action<HttpConfiguration> configureAction)
        {
            HttpConfiguration configuration = new HttpConfiguration();
            //configuration.Formatters.Clear();
            configuration.Formatters.InsertRange(0, formatters == null ? ODataMediaTypeFormatters.Create() : formatters);
            return Create(configuration, controllers, configureAction);
        }

        /// <summary>
        /// Create an TestServer.
        /// </summary>
        /// <param name="controllers">The controllers to use.</param>
        /// <param name="configureAction">The route configuration.</param>
        /// <param name="configureAction">The route configuration action.</param>
        /// <returns>An TestServer.</returns>
        private static HttpServer Create(
            HttpConfiguration configuration,
            Type[] controllers,
            Action<HttpConfiguration> configureAction)
        {
            if (controllers != null)
            {
                TestAssemblyResolver resolver = new TestAssemblyResolver(new MockAssembly(controllers));
                configuration.Services.Replace(typeof(IAssembliesResolver), resolver);
            }

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configureAction(configuration);
            configuration.EnsureInitialized();

            return new HttpServer(configuration);
        }

        /// <summary>
        /// Create an HttpClient from a server.
        /// </summary>
        /// <param name="server">The HttpServer.</param>
        /// <returns>An HttpClient.</returns>
        public static HttpClient CreateClient(HttpServer server)
        {
            return new HttpClient(server);
        }
#endif
    }
}
