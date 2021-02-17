// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Media type mapping that associates requests with stream property.
    /// </summary>
    public partial class ODataStreamMediaTypeMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataStreamMediaTypeMapping"/> class.
        /// </summary>
        public ODataStreamMediaTypeMapping()
            : base("application/octet-stream")
        {
        }
    }
}
