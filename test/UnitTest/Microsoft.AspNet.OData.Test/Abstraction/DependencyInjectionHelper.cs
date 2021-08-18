//-----------------------------------------------------------------------------
// <copyright file="DependencyInjectionHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    public static class DependencyInjectionHelper
    {
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

        public static void EnableODataDependencyInjectionSupport(this HttpConfiguration configuration, string routeName,
            IEdmModel model)
        {
            configuration.CreateODataRootContainer(routeName, builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => model));
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
