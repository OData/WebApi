// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match an <see cref="ODataPathSegment"/>.
    /// </summary>
    public abstract class ODataPathSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathSegmentTemplate"/> class.
        /// </summary>
        protected ODataPathSegmentTemplate()
        {
        }

        /// <summary>
        /// Matches the template with an <see cref="ODataPathSegment"/>.
        /// </summary>
        /// <param name="pathSegment">The path segment to match this template with.</param>
        /// <param name="values">The dictionary of matches to be updated if the segment matches the template.</param>
        /// <returns><c>true</c> if the segment matches the template; otherwise, <c>false</c>.</returns>
        public virtual bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            return false;
        }
    }
}
