//-----------------------------------------------------------------------------
// <copyright file="ODataPath.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;
using ODataPathSegment = Microsoft.OData.UriParser.ODataPathSegment;
using Semantic = Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// Provides an object representation for an OData path with additional information about the EDM type and entity set for the path.
    /// </summary>
    [ODataPathParameterBinding]
    public class ODataPath
    {
        private readonly ReadOnlyCollection<ODataPathSegment> _segments;
        private readonly IEdmType _edmType;
        private readonly IEdmNavigationSource _navigationSource;
        private readonly string _pathTemplate;
        private readonly string _pathLiteral;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPath" /> class.
        /// </summary>
        /// <param name="segments">The path segments for the path.</param>
        public ODataPath(params ODataPathSegment[] segments)
            : this(segments as IEnumerable<ODataPathSegment>)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPath" /> class.
        /// </summary>
        /// <param name="segments">The path segments for the path.</param>
        public ODataPath(IEnumerable<ODataPathSegment> segments)
        {
            if (segments == null)
            {
                throw Error.ArgumentNull("segments");
            }

            var oDataPathSegments = segments as IList<ODataPathSegment> ?? segments.ToList();

            _edmType = oDataPathSegments.Any() ? oDataPathSegments.Last().EdmType : null;

            _segments = new ReadOnlyCollection<ODataPathSegment>(oDataPathSegments);

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
            _pathTemplate = handler.PathTemplate;
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
        public ReadOnlyCollection<ODataPathSegment> Segments
        {
            get { return _segments; }
        }

        /// <summary>
        /// Gets the path template describing the types of segments in the path.
        /// </summary>
        public virtual string PathTemplate
        {
            get { return _pathTemplate; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _pathLiteral;
        }

        /// <summary>
        /// Gets the ODL path.
        /// </summary>
        public Semantic.ODataPath Path { get; internal set; }

        internal IList<ODataPathSegment> SegmentList
        {
            get { return _segments.ToList(); }
        }
    }
}
