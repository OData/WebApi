// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Routing;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template for an <see cref="ODataPath"/> that can be matched to an actual <see cref="ODataPath"/>.
    /// </summary>
    public class ODataPathTemplate
    {
        private ReadOnlyCollection<ODataPathSegmentTemplate> _segments;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathTemplate" /> class.
        /// </summary>
        /// <param name="segments">The path segment templates for the path.</param>
        public ODataPathTemplate(params ODataPathSegmentTemplate[] segments)
            : this((IList<ODataPathSegmentTemplate>)segments)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathTemplate" /> class.
        /// </summary>
        /// <param name="segments">The path segment templates for the path.</param>
        public ODataPathTemplate(IEnumerable<ODataPathSegmentTemplate> segments)
            : this(segments.ToList())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPath" /> class.
        /// </summary>
        /// <param name="segments">The path segments for the path.</param>
        public ODataPathTemplate(IList<ODataPathSegmentTemplate> segments)
        {
            if (segments == null)
            {
                throw Error.ArgumentNull("segments");
            }

            _segments = new ReadOnlyCollection<ODataPathSegmentTemplate>(segments);
        }

        /// <summary>
        /// Gets the path segments for the OData path.
        /// </summary>
        public ReadOnlyCollection<ODataPathSegmentTemplate> Segments
        {
            get
            {
                return _segments;
            }
        }

        /// <summary>
        /// Matches the current template with an OData path.
        /// </summary>
        /// <param name="path">The OData path to be matched against.</param>
        /// <param name="values">The dictionary of matches to be updated in case of a match.</param>
        /// <returns><c>true</c> in case of a match; otherwise, <c>false</c>.</returns>
        public bool TryMatch(ODataPath path, IDictionary<string, object> values)
        {
            if (path.Segments.Count != Segments.Count)
            {
                return false;
            }

            for (int index = 0; index < Segments.Count; index++)
            {
                if (!Segments[index].TryMatch(path.Segments[index], values))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
