// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.OData.Extensions;
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

        public static HttpConfiguration CreateConfigurationWithContainer()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.SetRootContainer(BuildContainer(null));
            return configuration;
        }

        public static void SetRootContainer(this HttpConfiguration configuration)
        {
            configuration.SetRootContainer(BuildContainer(null));
        }
    }
}
