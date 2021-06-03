//-----------------------------------------------------------------------------
// <copyright file="DefaultODataDeserializerProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// The default <see cref="ODataDeserializerProvider"/>.
    /// </summary>
    public partial class DefaultODataDeserializerProvider : ODataDeserializerProvider
    {
        private readonly IServiceProvider _rootContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultODataDeserializerProvider"/> class.
        /// </summary>
        /// <param name="rootContainer">The root container.</param>
        public DefaultODataDeserializerProvider(IServiceProvider rootContainer)
        {
            if (rootContainer == null)
            {
                throw Error.ArgumentNull("rootContainer");
            }

            _rootContainer = rootContainer;
        }

        /// <inheritdoc />
        public override ODataEdmTypeDeserializer GetEdmTypeDeserializer(IEdmTypeReference edmType)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            switch (edmType.TypeKind())
            {
                case EdmTypeKind.Entity:
                case EdmTypeKind.Complex:
                    return _rootContainer.GetRequiredService<ODataResourceDeserializer>();

                case EdmTypeKind.Enum:
                    return _rootContainer.GetRequiredService<ODataEnumDeserializer>();

                case EdmTypeKind.Primitive:
                    return _rootContainer.GetRequiredService<ODataPrimitiveDeserializer>();

                case EdmTypeKind.Collection:
                    IEdmCollectionTypeReference collectionType = edmType.AsCollection();
                    if (collectionType.ElementType().IsEntity() || collectionType.ElementType().IsComplex())
                    {
                        return _rootContainer.GetRequiredService<ODataResourceSetDeserializer>();
                    }
                    else
                    {
                        return _rootContainer.GetRequiredService<ODataCollectionDeserializer>();
                    }

                default:
                    return null;
            }
        }

        /// <inheritdoc />
        internal ODataDeserializer GetODataDeserializerImpl(Type type, Func<IEdmModel> modelFunction)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (modelFunction == null)
            {
                throw Error.ArgumentNull("modelFunction");
            }

            if (type == typeof(Uri))
            {
                return _rootContainer.GetRequiredService<ODataEntityReferenceLinkDeserializer>();
            }

            if (type == typeof(ODataActionParameters) || type == typeof(ODataUntypedActionParameters))
            {
                return _rootContainer.GetRequiredService<ODataActionPayloadDeserializer>();
            }

            IEdmModel model = modelFunction();
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
