//-----------------------------------------------------------------------------
// <copyright file="DefaultODataSerializerProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// The default <see cref="ODataSerializerProvider"/>.
    /// </summary>
    public partial class DefaultODataSerializerProvider : ODataSerializerProvider
    {
        private readonly IServiceProvider _rootContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultODataSerializerProvider"/> class.
        /// </summary>
        /// <param name="rootContainer">The root container.</param>
        public DefaultODataSerializerProvider(IServiceProvider rootContainer)
        {
            if (rootContainer == null)
            {
                throw Error.ArgumentNull("rootContainer");
            }

            _rootContainer = rootContainer;
        }

        /// <inheritdoc />
        public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            switch (edmType.TypeKind())
            {
                case EdmTypeKind.Enum:
                    return _rootContainer.GetRequiredService<ODataEnumSerializer>();

                case EdmTypeKind.Primitive:
                    return _rootContainer.GetRequiredService<ODataPrimitiveSerializer>();

                case EdmTypeKind.Collection:
                    IEdmCollectionTypeReference collectionType = edmType.AsCollection();
                    if (collectionType.Definition.IsDeltaFeed())
                    {
                        return _rootContainer.GetRequiredService<ODataDeltaFeedSerializer>();
                    }
                    else if (collectionType.ElementType().IsEntity() || collectionType.ElementType().IsComplex())
                    {
                        return _rootContainer.GetRequiredService<ODataResourceSetSerializer>();
                    }
                    else
                    {
                        return _rootContainer.GetRequiredService<ODataCollectionSerializer>();
                    }

                case EdmTypeKind.Complex:
                case EdmTypeKind.Entity:
                    return _rootContainer.GetRequiredService<ODataResourceSerializer>();

                default:
                    return null;
            }
        }

        /// <inheritdoc />
        internal ODataSerializer GetODataPayloadSerializerImpl(Type type, Func<IEdmModel> modelFunction, ODataPath path, Type errorType)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (modelFunction == null)
            {
                throw Error.ArgumentNull("modelFunction");
            }

            // handle the special types.
            if (type == typeof(ODataServiceDocument))
            {
                return _rootContainer.GetRequiredService<ODataServiceDocumentSerializer>();
            }
            else if (type == typeof(Uri) || type == typeof(ODataEntityReferenceLink))
            {
                return _rootContainer.GetRequiredService<ODataEntityReferenceLinkSerializer>();
            }
            else if (TypeHelper.IsTypeAssignableFrom(typeof(IEnumerable<Uri>), type) || type == typeof(ODataEntityReferenceLinks))
            {
                return _rootContainer.GetRequiredService<ODataEntityReferenceLinksSerializer>();
            }
            else if (type == typeof(ODataError) || type == errorType)
            {
                return _rootContainer.GetRequiredService<ODataErrorSerializer>();
            }
            else if (TypeHelper.IsTypeAssignableFrom(typeof(IEdmModel), type))
            {
                return _rootContainer.GetRequiredService<ODataMetadataSerializer>();
            }

            // Get the model. Using a Func<IEdmModel> to delay evaluation of the model
            // until after the above checks have passed.
            IEdmModel model = modelFunction();

            // if it is not a special type, assume it has a corresponding EdmType.
            ClrTypeCache typeMappingCache = model.GetTypeMappingCache();
            IEdmTypeReference edmType = typeMappingCache.GetEdmType(type, model);

            if (edmType != null)
            {
                bool isCountRequest = path != null && path.Segments.LastOrDefault() is CountSegment;
                bool isRawValueRequest = path != null && path.Segments.LastOrDefault() is ValueSegment;
                bool isStreamRequest = path.IsStreamPropertyPath();

                if (((edmType.IsPrimitive() || edmType.IsEnum()) && isRawValueRequest) || isCountRequest || isStreamRequest)
                {
                    return _rootContainer.GetRequiredService<ODataRawValueSerializer>();
                }
                else
                {
                    return GetEdmTypeSerializer(edmType);
                }
            }
            else
            {
                return null;
            }
        }
    }
}
