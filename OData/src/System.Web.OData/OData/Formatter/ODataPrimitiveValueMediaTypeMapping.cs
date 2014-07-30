// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.OData.Routing;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter
{
    /// <summary>
    /// Media type mapping that associates requests for the raw value of non-binary primitive properties to
    /// the text/plain content type.
    /// </summary>
    public class ODataPrimitiveValueMediaTypeMapping : ODataRawValueMediaTypeMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPrimitiveValueMediaTypeMapping"/> class.
        /// </summary>
        public ODataPrimitiveValueMediaTypeMapping()
            : base("text/plain")
        {
        }

        /// <inheritdoc/>
        protected override bool IsMatch(PropertyAccessPathSegment propertySegment)
        {
            return propertySegment != null &&
                   propertySegment.Property.Type.IsPrimitive() &&
                   !propertySegment.Property.Type.IsBinary();
        }
    }
}
