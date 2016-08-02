// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
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
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "DI services")]
        public static IContainerBuilder AddDefaultWebApiServices(this IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            builder.AddService<IODataPathHandler, DefaultODataPathHandler>(ServiceLifetime.Singleton);

            // ReaderSettings and WriterSettings are registered as prototype services.
            // There will be a copy (if it is accessed) of each prototype for each request.
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

            // QueryValidators.
            builder.AddService<CountQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<FilterQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<ODataQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<OrderByQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<SelectExpandQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<SkipQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<TopQueryValidator>(ServiceLifetime.Singleton);

            // SerializerProvider and DeserializerProvider.
            builder.AddService<ODataSerializerProvider, DefaultODataSerializerProvider>(ServiceLifetime.Singleton);
            builder.AddService<ODataDeserializerProvider, DefaultODataDeserializerProvider>(ServiceLifetime.Singleton);

            // Deserializers.
            builder.AddService<ODataResourceDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataEnumDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataPrimitiveDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataResourceSetDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataCollectionDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataEntityReferenceLinkDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataActionPayloadDeserializer>(ServiceLifetime.Singleton);

            // Serializers.
            builder.AddService<ODataEnumSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataPrimitiveSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataDeltaFeedSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataResourceSetSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataCollectionSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataResourceSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataServiceDocumentSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataEntityReferenceLinkSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataEntityReferenceLinksSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataErrorSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataMetadataSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataRawValueSerializer>(ServiceLifetime.Singleton);

            return builder;
        }
    }
}
