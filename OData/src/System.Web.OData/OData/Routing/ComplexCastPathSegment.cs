// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing a complex type cast.
    /// </summary>
    public class ComplexCastPathSegment : ODataPathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexCastPathSegment" /> class.
        /// </summary>
        /// <param name="castType">The type of the cast.</param>
        public ComplexCastPathSegment(IEdmComplexType castType)
        {
            if (castType == null)
            {
                throw Error.ArgumentNull("castType");
            }

            CastType = castType;
            CastTypeName = castType.FullTypeName();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexCastPathSegment" /> class.
        /// </summary>
        /// <param name="castTypeName">Name of the cast type.</param>
        public ComplexCastPathSegment(string castTypeName)
        {
            if (String.IsNullOrEmpty(castTypeName))
            {
                throw Error.ArgumentNull("castTypeName");
            }

            CastTypeName = castTypeName;
        }

        /// <summary>
        /// Gets the segment kind for the current segment.
        /// </summary>
        public override string SegmentKind
        {
            get
            {
                return ODataSegmentKinds.ComplexCast;
            }
        }

        /// <summary>
        /// Gets the type of the cast.
        /// </summary>
        public IEdmComplexType CastType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the cast type.
        /// </summary>
        public string CastTypeName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the EDM type for this segment.
        /// </summary>
        /// <param name="previousEdmType">The EDM type of the previous path segment.</param>
        /// <returns>
        /// The EDM type for this segment.
        /// </returns>
        public override IEdmType GetEdmType(IEdmType previousEdmType)
        {
            if (CastType != null && previousEdmType != null)
            {
                if (previousEdmType.TypeKind == EdmTypeKind.Collection)
                {
                    return new EdmCollectionType(
                        new EdmComplexTypeReference(CastType, isNullable: false));
                }
                else
                {
                    return CastType;
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public override IEdmNavigationSource GetNavigationSource(IEdmNavigationSource previousNavigationSource)
        {
            return previousNavigationSource;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return CastTypeName;
        }

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            if (pathSegment.SegmentKind == ODataSegmentKinds.ComplexCast)
            {
                ComplexCastPathSegment castSegment = (ComplexCastPathSegment)pathSegment;
                return castSegment.CastType == CastType && castSegment.CastTypeName == CastTypeName;
            }

            return false;
        }
    }
}
