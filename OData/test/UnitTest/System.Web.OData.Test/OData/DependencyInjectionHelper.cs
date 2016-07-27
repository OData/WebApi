// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Extensions;
using Microsoft.OData;
using HttpRouteCollectionExtensions = System.Web.OData.Formatter.HttpRouteCollectionExtensions;

namespace System.Web.OData
{
    public static class DependencyInjectionHelper
    {
        public static IServiceProvider BuildContainer(Action<IContainerBuilder> action)
        {
            return CreateConfigurationWithRootContainer().GetODataRootContainer(HttpRouteCollectionExtensions.RouteName);
        }

        public static HttpConfiguration CreateConfigurationWithRootContainer()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.EnableDependencyInjectionSupport();
            return configuration;
        }

        public static void EnableDependencyInjectionSupport(this HttpConfiguration configuration)
        {
            configuration.EnableDependencyInjectionSupport(HttpRouteCollectionExtensions.RouteName);
        }

        public static void EnableDependencyInjectionSupport(this HttpConfiguration configuration, string routeName)
        {
            configuration.CreateODataRootContainer(routeName, null);
        }

        public static void EnableDependencyInjectionSupport(this HttpRequestMessage request)
        {
            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration == null)
            {
                configuration = new HttpConfiguration();
                request.SetConfiguration(configuration);
            }

            configuration.EnableDependencyInjection();
        }
    }
}
