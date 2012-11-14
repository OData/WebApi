// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// Provides an object representation for an OData path with additional information about the EDM type and entity set for the path.
    /// </summary>
    public class ODataPath
    {
        private LinkedList<ODataPathSegment> _segments = new LinkedList<ODataPathSegment>();

        /// <summary>
        /// Gets or sets the EDM type of the path.
        /// </summary>
        public IEdmType EdmType
        {
            get
            {
                return Segments.Last.Value.EdmType;
            }
        }

        /// <summary>
        /// Gets or sets the entity set of the path.
        /// </summary>
        public IEdmEntitySet EntitySet
        {
            get
            {
                return Segments.Last.Value.EntitySet;
            }
        }

        /// <summary>
        /// Gets the path template describing the types of segments in the path.
        /// </summary>
        public string PathTemplate
        {
            get
            {
                StringBuilder sb = new StringBuilder("~");
                foreach (ODataPathSegment segment in Segments)
                {
                    sb.Append("/");
                    sb.Append(segment.SegmentKind);
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the path segments for the OData path.
        /// </summary>
        public LinkedList<ODataPathSegment> Segments
        {
            get
            {
                return _segments;
            }
        }
    }
}
