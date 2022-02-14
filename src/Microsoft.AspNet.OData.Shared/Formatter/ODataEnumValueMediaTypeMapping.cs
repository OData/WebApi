//-----------------------------------------------------------------------------
// <copyright file="ODataEnumValueMediaTypeMapping.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Formatter
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
        protected override bool IsMatch(PropertySegment propertySegment)
        {
            return propertySegment != null && propertySegment.Property.Type.IsEnum();
        }
    }
}
