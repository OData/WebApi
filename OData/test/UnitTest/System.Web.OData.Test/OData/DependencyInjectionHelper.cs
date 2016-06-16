// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

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
            configuration.SetRootContainer(BuildContainer(null));
        }

        public static void SetFakeRequestContainer(this HttpRequestMessage request)
        {
            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration == null)
            {
                configuration = CreateConfigurationWithRootContainer();
                request.SetConfiguration(configuration);
            }

            IServiceScope requestScope =
                configuration.GetRootContainer().GetRequiredService<IServiceScopeFactory>().CreateScope();
            request.BindRequestScope(requestScope);
        }
    }
}
