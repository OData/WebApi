// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter.Deserialization;
using System.Web.OData.Formatter.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using HttpRouteCollectionExtensions = System.Web.OData.Formatter.HttpRouteCollectionExtensions;

namespace System.Web.OData
{
    public static class DependencyInjectionHelper
    {
        public static ODataSerializerProvider GetDefaultODataSerializerProvider()
        {
            return new MockContainer().GetRequiredService<ODataSerializerProvider>();
        }

        public static ODataDeserializerProvider GetDefaultODataDeserializerProvider()
        {
            return new MockContainer().GetRequiredService<ODataDeserializerProvider>();
        }

        public static HttpConfiguration CreateConfigurationWithRootContainer()
        {
            return new MockContainer().Configuration;
        }

        public static IServiceProvider GetODataRootContainer(this HttpConfiguration configuration)
        {
            return configuration.GetODataRootContainer(HttpRouteCollectionExtensions.RouteName);
        }

        public static void EnableODataDependencyInjectionSupport(this HttpConfiguration configuration)
        {
            configuration.EnableODataDependencyInjectionSupport(HttpRouteCollectionExtensions.RouteName);
        }

        public static void EnableODataDependencyInjectionSupport(this HttpConfiguration configuration, string routeName)
        {
            configuration.CreateODataRootContainer(routeName, null);
        }

        public static void EnableODataDependencyInjectionSupport(this HttpConfiguration configuration,
            Action<IContainerBuilder> action)
        {
            configuration.CreateODataRootContainer(HttpRouteCollectionExtensions.RouteName, action);
        }

        public static void EnableHttpDependencyInjectionSupport(this HttpRequestMessage request)
        {
            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration == null)
            {
                configuration = new HttpConfiguration();
                request.SetConfiguration(configuration);
            }

            configuration.EnableDependencyInjection();
        }

        public static void EnableODataDependencyInjectionSupport(this HttpRequestMessage request)
        {
            request.EnableODataDependencyInjectionSupport(HttpRouteCollectionExtensions.RouteName);
        }

        public static void EnableODataDependencyInjectionSupport(this HttpRequestMessage request, string routeName)
        {
            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration == null)
            {
                configuration = new HttpConfiguration();
                configuration.EnableODataDependencyInjectionSupport(routeName);
                request.SetConfiguration(configuration);
            }

            request.ODataProperties().RouteName = routeName;
            request.CreateRequestContainer(routeName);
        }
    }
}
