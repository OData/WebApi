// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.Http;

namespace System.Web.OData.Routing
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
        /// <returns><see langword="true"/> in case of a match; otherwise, <see langword="false"/>.</returns>
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
