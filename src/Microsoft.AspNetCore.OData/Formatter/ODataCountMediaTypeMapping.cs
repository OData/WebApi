//-----------------------------------------------------------------------------
// <copyright file="ODataCountMediaTypeMapping.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Media type mapping that associates requests with $count.
    /// </summary>
    /// <remarks>This class derives from a platform-specific class.</remarks>
    public partial class ODataCountMediaTypeMapping : MediaTypeMapping
    {
        /// <inheritdoc/>
        public override double TryMatchMediaType(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return IsCountRequest(request.ODataFeature().Path) ? 1 : 0;
        }
    }
}
