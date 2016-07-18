// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Extensions;
using Microsoft.OData;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;

namespace System.Web.OData
{
    public static class DependencyInjectionHelper
    {
        public static IServiceProvider BuildContainer(Action<IContainerBuilder> action)
        {
            IContainerBuilder builder = new DefaultContainerBuilder();

            builder.AddDefaultODataServices();
            builder.AddDefaultWebApiServices();

            if (action != null)
            {
                action(builder);
            }

            return builder.BuildContainer();
        }

        public static HttpConfiguration CreateConfigurationWithRootContainer()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.SetFakeRootContainer();
            return configuration;
        }

        public static void SetFakeRootContainer(this HttpConfiguration configuration)
        {
            configuration.SetRootContainer(BuildContainer(builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => configuration)
                       .AddService(ServiceLifetime.Singleton, sp => configuration.GetDefaultQuerySettings())));
        }

        public static void SetFakeRootContainer(this HttpRequestMessage request)
        {
            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration == null)
            {
                configuration = CreateConfigurationWithRootContainer();
                request.SetConfiguration(configuration);
            }
        }
    }
}
