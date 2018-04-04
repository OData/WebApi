// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;
using Microsoft.Test.AspNet.OData.Common;

namespace Microsoft.Test.AspNet.OData
{
    /// <summary>
    /// A class to create IRouteBuilder/HttpConfiguration.
    /// </summary>
    public class RoutingConfigurationFactory
    {
        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static HttpConfiguration Create()
        {
            return new HttpConfiguration();
        }

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static HttpConfiguration CreateWithRoute(string route)
        {
            return new HttpConfiguration(new HttpRouteCollection(route));
        }

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        internal static HttpConfiguration CreateWithRootContainer(string routeName, Action<IContainerBuilder> configureAction = null)
        {
            HttpConfiguration configuration = Create();
            if (!string.IsNullOrEmpty(routeName))
            {
                configuration.CreateODataRootContainer(routeName, configureAction);
            }
            else
            {
                configuration.EnableDependencyInjection(configureAction);
            }

            return configuration;
        }

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        internal static HttpConfiguration CreateWithTypes(params Type[] types)
        {
            HttpConfiguration configuration = Create();

            TestAssemblyResolver resolver = new TestAssemblyResolver(new MockAssembly(types));
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);
            configuration.Count().OrderBy().Filter().Expand().MaxTop(null);

            return configuration;
        }

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        internal static HttpConfiguration CreateWithRootContainerAndTypes(string routeName = null, Action<IContainerBuilder> configureAction = null, params Type[] types)
        {
            HttpConfiguration configuration = CreateWithRootContainer(routeName, configureAction);

            TestAssemblyResolver resolver = new TestAssemblyResolver(new MockAssembly(types));
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            return configuration;
        }
    }
}