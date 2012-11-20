// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData
{
    /// <summary>
    /// Contains context information about the feed currently being serialized.
    /// </summary>
    public class FeedContext
    {
        /// <summary>
        /// Gets the <see cref="IEdmEntitySet"/> this instance belongs to.
        /// </summary>
        public IEdmEntitySet EntitySet { get; set; }

        /// <summary>
        /// Gets the <see cref="UrlHelper"/> to be used for generating navigation and self links while
        /// serializing this feed instance.
        /// </summary>
        public UrlHelper UrlHelper { get; set; }

        /// <summary>
        /// Gets the value of this feed instance.
        /// </summary>
        public object FeedInstance { get; set; }

        /// <summary>
        /// Gets the <see cref="IODataPathHandler"/> to be used for generating OData paths while serializing this feed.
        /// </summary>
        public IODataPathHandler PathHandler { get; set; }
    }
}
