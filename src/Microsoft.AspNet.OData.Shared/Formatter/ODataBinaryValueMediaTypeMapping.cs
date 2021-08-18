//-----------------------------------------------------------------------------
// <copyright file="ODataBinaryValueMediaTypeMapping.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Media type mapping that associates requests for the raw value of binary properties to
    /// the application/octet-stream content type.
    /// </summary>
    public class ODataBinaryValueMediaTypeMapping : ODataRawValueMediaTypeMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBinaryValueMediaTypeMapping"/> class.
        /// </summary>
        public ODataBinaryValueMediaTypeMapping()
            : base("application/octet-stream")
        {
        }

        /// <inheritdoc/>
        protected override bool IsMatch(PropertySegment propertySegment)
        {
            return propertySegment != null && propertySegment.Property.Type.IsBinary();
        }
    }
}
