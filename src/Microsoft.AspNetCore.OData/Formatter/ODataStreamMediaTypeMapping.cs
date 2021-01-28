// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Media type mapping that associates requests with stream property.
    /// </summary>
    /// <remarks>This class derives from a platform-specific class.</remarks>
    public partial class ODataStreamMediaTypeMapping : MediaTypeMapping
    {
        /// <inheritdoc/>
        public override double TryMatchMediaType(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.ODataFeature().Path.IsStreamPropertyPath() ? 1 : 0;
        }
    }
}
