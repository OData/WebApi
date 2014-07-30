// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.OData.Routing
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
        /// <returns><see langword="true"/> if the segment matches the template; otherwise, <see langword="false"/>.</returns>
        public virtual bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            return false;
        }
    }
}
