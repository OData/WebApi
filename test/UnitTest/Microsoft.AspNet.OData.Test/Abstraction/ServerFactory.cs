//-----------------------------------------------------------------------------
// <copyright file="ServerFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Common;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    /// <summary>
    /// Factory for creating a test servers.
    /// </summary>
    public class TestServerFactory
    {
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
    }
}
