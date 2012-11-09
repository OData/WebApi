// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing the root of the service.
    /// </summary>
    public class ServiceBasePathSegment : ODataPathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBasePathSegment" /> class.
        /// </summary>
        /// <param name="baseUri">The base URI of the service.</param>
        public ServiceBasePathSegment(Uri baseUri)
        {
            BaseUri = baseUri;
        }

        /// <summary>
        /// Gets the base URI of the service.
        /// </summary>
        public Uri BaseUri
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the segment kind for the current segment.
        /// </summary>
        public override string SegmentKind
        {
            get
            {
                return ODataSegmentKinds.ServiceBase;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return BaseUri.ToString();
        }
    }
}
