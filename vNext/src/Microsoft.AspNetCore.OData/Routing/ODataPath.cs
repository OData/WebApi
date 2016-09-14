// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Common;
using Semantic = Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Provides an object representation for an OData path with additional information about the EDM type and entity set for the path.
    /// </summary>
    [ODataPathParameterBinding]
    public class ODataPath : IEnumerable<Semantic.ODataPathSegment>
    {
        private readonly ReadOnlyCollection<Semantic.ODataPathSegment> _segments;
        private readonly IEdmType _edmType;
        private readonly IEdmNavigationSource _navigationSource;
        private readonly string _pathLiteral;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPath" /> class.
        /// </summary>
        /// <param name="segments">The path segments for the path.</param>
        public ODataPath(params Semantic.ODataPathSegment[] segments)
            : this(segments as IEnumerable<Semantic.ODataPathSegment>)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPath" /> class.
        /// </summary>
        /// <param name="segments">The path segments for the path.</param>
        public ODataPath(IEnumerable<Semantic.ODataPathSegment> segments)
        {
            if (segments == null)
            {
                throw Error.ArgumentNull("segments");
            }

            var oDataPathSegments = segments as IList<Semantic.ODataPathSegment> ?? segments.ToList();

            _edmType = oDataPathSegments.Any() ? oDataPathSegments.Last().EdmType : null;

            _segments = new ReadOnlyCollection<Semantic.ODataPathSegment>(oDataPathSegments);

            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();
            foreach (var segment in oDataPathSegments)
            {
                UnresolvedPathSegment pathSegment = segment as UnresolvedPathSegment;
                if (pathSegment != null)
                {
                    handler.Handle(pathSegment);
                }
                else
                {
                    segment.HandleWith(handler);
                }
            }

            _navigationSource = handler.NavigationSource;
            PathTemplate = handler.PathTemplate;
            _pathLiteral = handler.PathLiteral;
        }

        /// <summary>
        /// Gets the EDM type of the path.
        /// </summary>
        public IEdmType EdmType
        {
            get { return _edmType; }
        }

        /// <summary>
        /// Gets the navigation source of the path.
        /// </summary>
        public IEdmNavigationSource NavigationSource
        {
            get { return _navigationSource; }
        }

        /// <summary>
        /// Gets the path segments for the OData path.
        /// </summary>
        public ReadOnlyCollection<Semantic.ODataPathSegment> Segments
        {
            get { return _segments; }
        }

        /// <summary>
        /// Gets the first segment in the path. Returns null if the path is empty.
        /// </summary> 
        public Semantic.ODataPathSegment FirstSegment
        {
            get { return this._segments.Count == 0 ? null : this._segments[0]; }
        }

        /// <summary>
        /// Get the last segment in the path. Returns null if the path is empty.
        /// </summary> 
        public Semantic.ODataPathSegment LastSegment
        {
            get { return this._segments.Count == 0 ? null : this._segments[this._segments.Count - 1]; }
        }

        /// <summary>
        /// Get the number of segments in this path.
        /// </summary>
        public int Count
        {
            get { return this._segments.Count; }
        }

        /// <summary>
        /// Gets the path template describing the types of segments in the path.
        /// </summary>
        public virtual string PathTemplate { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return _pathLiteral;
        }

        /// <summary>
        /// Get the segments enumerator.
        /// </summary>
        /// <returns>The segments enumerator.</returns>
        public IEnumerator<Semantic.ODataPathSegment> GetEnumerator()
        {
            return this._segments.GetEnumerator();
        }

        /// <summary>
        /// get the segments enumerator.
        /// </summary>
        /// <returns>The segments enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal Semantic.ODataPath ODLPath { get; set; }
    }
}
