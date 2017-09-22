// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;

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
    }
}
