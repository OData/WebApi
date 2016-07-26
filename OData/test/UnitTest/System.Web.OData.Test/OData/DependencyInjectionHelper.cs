// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using Microsoft.OData;
using HttpRouteCollectionExtensions = System.Web.OData.Formatter.HttpRouteCollectionExtensions;

namespace System.Web.OData
{
    public static class DependencyInjectionHelper
    {
        public static IServiceProvider BuildContainer(Action<IContainerBuilder> action)
        {
            return CreateConfigurationWithRootContainer().GetRootContainer(HttpRouteCollectionExtensions.RouteName);
        }

        public static HttpConfiguration CreateConfigurationWithRootContainer()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.EnableDependencyInjection(HttpRouteCollectionExtensions.RouteName);
            return configuration;
        }

        public static void EnableDependencyInjection(this HttpConfiguration configuration)
        {
            configuration.EnableDependencyInjection(HttpRouteCollectionExtensions.RouteName);
        }

        public static void EnableDependencyInjection(this HttpRequestMessage request)
        {
            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration == null)
            {
                configuration = CreateConfigurationWithRootContainer();
                request.SetConfiguration(configuration);
            }

            request.SetFakeODataRouteName();
        }
    }
}
