// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using HttpRouteCollectionExtensions = Microsoft.Test.AspNet.OData.Formatter.HttpRouteCollectionExtensions;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;

namespace Microsoft.Test.AspNet.OData
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

        public static HttpConfiguration CreateConfigurationWithRootContainer(IEdmModel model)
        {
            return new MockContainer(model).Configuration;
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
            configuration.EnableODataDependencyInjectionSupport(routeName, (Action<IContainerBuilder>)null);
        }

        public static void EnableODataDependencyInjectionSupport(this HttpConfiguration configuration, string routeName,
            Action<IContainerBuilder> action)
        {
            configuration.CreateODataRootContainer(routeName, action);
        }

        public static void EnableODataDependencyInjectionSupport(this HttpConfiguration configuration, IEdmModel model)
        {
            configuration.CreateODataRootContainer(HttpRouteCollectionExtensions.RouteName, builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => model));
        }

        public static void EnableODataDependencyInjectionSupport(this HttpConfiguration configuration, string routeName,
            IEdmModel model)
        {
            configuration.CreateODataRootContainer(routeName, builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => model));
        }

        public static void EnableODataDependencyInjectionSupport(this HttpConfiguration configuration, string routeName,
            IODataPathHandler pathHandler)
        {
            configuration.CreateODataRootContainer(routeName, builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => pathHandler));
        }

        public static void EnableODataDependencyInjectionSupport(this HttpConfiguration configuration, string routeName,
            IEdmModel model, IODataPathHandler pathHandler)
        {
            configuration.CreateODataRootContainer(routeName, builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => model)
                       .AddService(ServiceLifetime.Singleton, sp => pathHandler));
        }

        public static void EnableODataDependencyInjectionSupport(this HttpConfiguration configuration,
            Action<IContainerBuilder> action)
        {
            configuration.EnableODataDependencyInjectionSupport(HttpRouteCollectionExtensions.RouteName, action);
        }

        public static void EnableHttpDependencyInjectionSupport(this HttpRequestMessage request)
        {
            request.EnableHttpDependencyInjectionSupport((Action<IContainerBuilder>)null);
        }

        public static void EnableHttpDependencyInjectionSupport(this HttpRequestMessage request,
            IEdmModel model)
        {
            request.EnableHttpDependencyInjectionSupport(builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => model));
        }

        public static void EnableHttpDependencyInjectionSupport(this HttpRequestMessage request,
            Action<IContainerBuilder> action)
        {
            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration == null)
            {
                configuration = new HttpConfiguration();
                request.SetConfiguration(configuration);
            }

            configuration.EnableDependencyInjection(action);
        }

        public static void EnableODataDependencyInjectionSupport(this HttpRequestMessage request)
        {
            request.EnableODataDependencyInjectionSupport(HttpRouteCollectionExtensions.RouteName);
        }

        public static void EnableODataDependencyInjectionSupport(this HttpRequestMessage request, string routeName)
        {
            request.EnableODataDependencyInjectionSupport(routeName, null);
        }

        public static void EnableODataDependencyInjectionSupport(this HttpRequestMessage request, IEdmModel model)
        {
            request.EnableODataDependencyInjectionSupport(HttpRouteCollectionExtensions.RouteName, builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => model));
        }

        public static void EnableODataDependencyInjectionSupport(this HttpRequestMessage request, string routeName,
            Action<IContainerBuilder> action)
        {
            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration == null)
            {
                configuration = new HttpConfiguration();
                configuration.EnableODataDependencyInjectionSupport(routeName, action);
                request.SetConfiguration(configuration);
            }

            request.ODataProperties().RouteName = routeName;
            request.CreateRequestContainer(routeName);
        }
    }
}
