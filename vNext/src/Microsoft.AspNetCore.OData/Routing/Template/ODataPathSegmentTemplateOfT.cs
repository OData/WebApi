// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that can match a general path segment.
    /// </summary>
    public class ODataPathSegmentTemplate<T> : ODataPathSegmentTemplate where T : ODataPathSegment
    {
        /// <summary>
        /// Matches the template with an <see cref="ODataPathSegment"/>.
        /// </summary>
        /// <param name="pathSegment">The path segment to match this template with.</param>
        /// <param name="values">The dictionary of matches to be updated if the segment matches the template.</param>
        /// <returns><c>true</c> if the segment matches the template; otherwise, <c>false</c>.</returns>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            return pathSegment is T;
        }
    }
}
