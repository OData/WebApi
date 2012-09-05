// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
        /// Initializes a new instance of the <see cref="FeedContext" /> class.
        /// </summary>
        /// <param name="entitySet">The entity set.</param>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="feedInstance">The feed instance.</param>
        public FeedContext(IEdmEntitySet entitySet, UrlHelper urlHelper, object feedInstance)
        {
            if (entitySet == null)
            {
                throw Error.ArgumentNull("entitySet");
            }

            if (feedInstance == null)
            {
                throw Error.ArgumentNull("feedInstance");
            }

            EntitySet = entitySet;
            UrlHelper = urlHelper;
            FeedInstance = feedInstance;
        }

        /// <summary>
        /// Gets the <see cref="IEdmEntitySet"/> this instance belongs to.
        /// 
        /// The setter is not intended to be used other than for unit testing purpose. 
        /// </summary>
        public IEdmEntitySet EntitySet { get; set; }

        /// <summary>
        /// Gets the <see cref="UrlHelper"/> to be used for generating navigation and self links while
        /// serializing this feed instance.
        /// 
        /// The setter is not intended to be used other than for unit testing purpose. 
        /// </summary>
        public UrlHelper UrlHelper { get; set; }

        /// <summary>
        /// Gets the value of this feed instance.
        /// 
        /// The setter is not intended to be used other than for unit testing purpose. 
        /// </summary>
        public object FeedInstance { get; set; }
    }
}
