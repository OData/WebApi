// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.OData.Routing;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter
{
    /// <summary>
    /// Media type mapping that associates requests for the raw value of enum properties with
    /// the text/plain content type.
    /// </summary>
    public class ODataEnumValueMediaTypeMapping : ODataRawValueMediaTypeMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEnumValueMediaTypeMapping"/> class.
        /// </summary>
        public ODataEnumValueMediaTypeMapping()
            : base("text/plain")
        {
        }

        /// <inheritdoc/>
        protected override bool IsMatch(PropertyAccessPathSegment propertySegment)
        {
            return propertySegment != null && propertySegment.Property.Type.IsEnum();
        }
    }
}
