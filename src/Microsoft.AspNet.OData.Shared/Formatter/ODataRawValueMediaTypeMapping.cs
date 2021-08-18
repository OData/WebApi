//-----------------------------------------------------------------------------
// <copyright file="ODataRawValueMediaTypeMapping.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using System.Net.Http;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Media type mapping that associates requests for the raw value of properties.
    /// </summary>
    public abstract partial class ODataRawValueMediaTypeMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRawValueMediaTypeMapping"/> class.
        /// </summary>
        protected ODataRawValueMediaTypeMapping(string mediaType)
            : base(mediaType)
        {
        }

        /// <summary>
        /// This method determines if the <see cref="HttpRequestMessage"/> is an OData Raw value request.
        /// </summary>
        /// <param name="propertySegment">The <see cref="PropertySegment"/> of the path.</param>
        /// <returns>True if the request is an OData raw value request.</returns>
        protected abstract bool IsMatch(PropertySegment propertySegment);

        internal static bool IsRawValueRequest(ODataPath path)
        {
            return path != null && path.Segments.LastOrDefault() is ValueSegment;
        }

        private static PropertySegment GetProperty(ODataPath odataPath)
        {
            if (odataPath == null || odataPath.Segments.Count < 2)
            {
                return null;
            }
            return odataPath.Segments[odataPath.Segments.Count - 2] as PropertySegment;
        }
    }
}
