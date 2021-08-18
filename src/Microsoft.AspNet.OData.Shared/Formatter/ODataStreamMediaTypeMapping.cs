//-----------------------------------------------------------------------------
// <copyright file="ODataStreamMediaTypeMapping.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
