// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter
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
        protected override bool IsMatch(PropertyAccessPathSegment propertySegment)
        {
            return propertySegment != null && propertySegment.Property.Type.IsBinary();
        }
    }
}
