// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization
{
    /// <summary>
    /// The default <see cref="ODataDeserializerProvider"/>.
    /// </summary>
    public class DefaultODataDeserializerProvider : IODataDeserializerProvider
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultODataDeserializerProvider"/> class.
        /// </summary>
        /// <param name="serviceProvider">The root container.</param>
        public DefaultODataDeserializerProvider(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull("serviceProvider");
            }

            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public ODataEdmTypeDeserializer GetEdmTypeDeserializer(IEdmTypeReference edmType)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            switch (edmType.TypeKind())
            {
                case EdmTypeKind.Entity:
                case EdmTypeKind.Complex:
                    return _serviceProvider.GetRequiredService<ODataResourceDeserializer>();

                case EdmTypeKind.Enum:
                    return _serviceProvider.GetRequiredService<ODataEnumDeserializer>();

                case EdmTypeKind.Primitive:
                    return _serviceProvider.GetRequiredService<ODataPrimitiveDeserializer>();

                case EdmTypeKind.Collection:
                    IEdmCollectionTypeReference collectionType = edmType.AsCollection();
                    if (collectionType.ElementType().IsEntity() || collectionType.ElementType().IsComplex())
                    {
                        return _serviceProvider.GetRequiredService<ODataResourceSetDeserializer>();
                    }
                    else
                    {
                        return _serviceProvider.GetRequiredService<ODataCollectionDeserializer>();
                    }

                default:
                    return null;
            }
        }

        /// <inheritdoc />
        public ODataDeserializer GetODataDeserializer(IEdmModel model, Type type, HttpRequest request)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            /*
            if (type == typeof(Uri))
            {
                return _serviceProvider.GetRequiredService<ODataEntityReferenceLinkDeserializer>();
            }

            if (type == typeof(ODataActionParameters) || type == typeof(ODataUntypedActionParameters))
            {
                return _serviceProvider.GetRequiredService<ODataActionPayloadDeserializer>();
            }*/

            //IEdmModel model = request.GetModel();
            ClrTypeCache typeMappingCache = model.GetTypeMappingCache();
            IEdmTypeReference edmType = typeMappingCache.GetEdmType(type, model);

            if (edmType == null)
            {
                return null;
            }
            else
            {
                return GetEdmTypeDeserializer(edmType);
            }
        }
    }
}
