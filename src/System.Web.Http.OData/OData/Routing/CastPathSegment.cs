// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing a cast.
    /// </summary>
    public class CastPathSegment : ODataPathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CastPathSegment" /> class.
        /// </summary>
        /// <param name="previous">The previous segment in the path.</param>
        /// <param name="castType">The type of the cast.</param>
        public CastPathSegment(ODataPathSegment previous, IEdmEntityType castType)
            : base(previous)
        {
            if (castType == null)
            {
                throw Error.ArgumentNull("cast");
            }

            IEdmType previousEdmType = previous.EdmType;

            if (previousEdmType == null)
            {
                throw Error.InvalidOperation(SRResources.PreviousSegmentEdmTypeCannotBeNull);
            }

            if (previousEdmType.TypeKind == EdmTypeKind.Collection)
            {
                EdmType = castType.GetCollection();
            }
            else
            {
                EdmType = castType;
            }
            EntitySet = previous.EntitySet;
            CastType = castType;
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
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return CastType.FullName();
        }
    }
}
