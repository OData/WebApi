// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Text;
using System.Web.Http;
using Microsoft.OData.Edm;
using Semantic = Microsoft.OData.Core.UriParser.Semantic;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// Provides an object representation for an OData path with additional information about the EDM type and entity set for the path.
    /// </summary>
    [ODataPathParameterBinding]
    public class ODataPath
    {
        private ReadOnlyCollection<ODataPathSegment> _segments;
        private IEdmType _edmType;
        private IEdmNavigationSource _navigationSource;
        private string _pathTemplate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPath" /> class.
        /// </summary>
        /// <param name="segments">The path segments for the path.</param>
        public ODataPath(params ODataPathSegment[] segments)
            : this(segments as IList<ODataPathSegment>)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPath" /> class.
        /// </summary>
        /// <param name="segments">The path segments for the path.</param>
        public ODataPath(IList<ODataPathSegment> segments)
        {
            if (segments == null)
            {
                throw Error.ArgumentNull("segments");
            }

            foreach (ODataPathSegment segment in segments)
            {
                _edmType = segment.GetEdmType(_edmType);
                _navigationSource = segment.GetNavigationSource(_navigationSource);
            }

            _segments = new ReadOnlyCollection<ODataPathSegment>(segments);
        }

        /// <summary>
        /// Gets or sets the EDM type of the path.
        /// </summary>
        public IEdmType EdmType
        {
            get
            {
                return _edmType;
            }
        }

        /// <summary>
        /// Gets or sets the navigation source of the path.
        /// </summary>
        public IEdmNavigationSource NavigationSource
        {
            get
            {
                return _navigationSource;
            }
        }

        /// <summary>
        /// Gets the path template describing the types of segments in the path.
        /// </summary>
        public string PathTemplate
        {
            get
            {
                if (_pathTemplate == null)
                {
                    StringBuilder templateBuilder = new StringBuilder("~");
                    foreach (ODataPathSegment segment in Segments)
                    {
                        templateBuilder.Append("/");
                        templateBuilder.Append(segment.SegmentKind);
                    }
                    _pathTemplate = templateBuilder.ToString();
                }
                return _pathTemplate;
            }
        }

        /// <summary>
        /// Gets the path segments for the OData path.
        /// </summary>
        public ReadOnlyCollection<ODataPathSegment> Segments
        {
            get
            {
                return _segments;
            }
        }

        internal Semantic.ODataPath ODLPath { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder pathBuilder = new StringBuilder();
            Contract.Assert(_segments != null);

            bool firstSegment = true;

            foreach (ODataPathSegment segment in _segments)
            {
                if (segment == null)
                {
                    continue;
                }

                if (segment is KeyValuePathSegment)
                {
                    pathBuilder.Append('(');
                    pathBuilder.Append(segment.ToString());
                    pathBuilder.Append(')');
                }
                else
                {
                    if (!firstSegment)
                    {
                        pathBuilder.Append('/');
                    }

                    pathBuilder.Append(segment.ToString());
                }

                firstSegment = false;
            }

            return pathBuilder.ToString();
        }
    }
}
