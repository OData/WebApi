// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.OData.Formatter.Deserialization;
using System.Web.OData.Formatter.Serialization;
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
            builder.AddService<ODataSerializerProvider, DefaultODataSerializerProvider>(ServiceLifetime.Singleton);
            builder.AddService<ODataDeserializerProvider, DefaultODataDeserializerProvider>(ServiceLifetime.Singleton);
            builder.AddService<ODataResourceDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataEnumDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataPrimitiveDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataResourceSetDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataCollectionDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataEntityReferenceLinkDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataActionPayloadDeserializer>(ServiceLifetime.Singleton);

            return builder;
        }
    }
}
