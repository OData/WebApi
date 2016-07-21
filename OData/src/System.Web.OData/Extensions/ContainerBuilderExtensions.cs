// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.OData.Query.Validators;
using System.Web.OData.Routing;
using Microsoft.OData;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;

namespace System.Web.OData.Extensions
{
    internal static class ContainerBuilderExtensions
    {
        public static IContainerBuilder AddDefaultWebApiServices(this IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            builder.AddService<IODataPathHandler, DefaultODataPathHandler>(ServiceLifetime.Singleton);
            builder.AddServicePrototype(new ODataMessageReaderSettings
            {
                EnableMessageStreamDisposal = false,
                MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue },
            });
            builder.AddServicePrototype(new ODataMessageWriterSettings
            {
                EnableMessageStreamDisposal = false,
                MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue },
                AutoComputePayloadMetadata = true,
            });
            builder.AddService<CountQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<FilterQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<ODataQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<OrderByQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<SelectExpandQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<SkipQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<TopQueryValidator>(ServiceLifetime.Singleton);

            return builder;
        }
    }
}
