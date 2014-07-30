// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using System.Web.OData.Formatter;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing a cast.
    /// </summary>
    public class CastPathSegment : ODataPathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CastPathSegment" /> class.
        /// </summary>
        /// <param name="castType">The type of the cast.</param>
        public CastPathSegment(IEdmEntityType castType)
        {
            if (castType == null)
            {
                throw Error.ArgumentNull("castType");
            }

            CastType = castType;
            CastTypeName = castType.FullName();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CastPathSegment" /> class.
        /// </summary>
        /// <param name="castTypeName">Name of the cast type.</param>
        public CastPathSegment(string castTypeName)
        {
            if (castTypeName == null)
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
                return ODataSegmentKinds.Cast;
            }
        }

        /// <summary>
        /// Gets the type of the cast.
        /// </summary>
        public IEdmEntityType CastType
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
                    return CastType.GetCollection();
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
            if (pathSegment.SegmentKind == ODataSegmentKinds.Cast)
            {
                CastPathSegment castSegment = (CastPathSegment)pathSegment;
                return castSegment.CastType == CastType
                    && castSegment.CastTypeName == CastTypeName;
            }

            return false;
        }
    }
}
