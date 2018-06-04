// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Media type mapping that associates requests for the raw value of properties.
    /// </summary>
    /// <remarks>This class derives from a platform-specific class.</remarks>
    public abstract partial class ODataRawValueMediaTypeMapping : MediaTypeMapping
    {
        /// <inheritdoc/>
        public override double TryMatchMediaType(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            ODataPath odataPath = request.ODataFeature().Path;
            return (IsRawValueRequest(odataPath) && IsMatch(GetProperty(odataPath))) ? 1 : 0;
        }
    }
}
