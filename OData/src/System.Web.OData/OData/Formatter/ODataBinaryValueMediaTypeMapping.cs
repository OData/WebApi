// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter
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
