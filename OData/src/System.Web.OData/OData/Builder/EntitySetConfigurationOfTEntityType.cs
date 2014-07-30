// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="IEdmEntitySet"/> that can be built using <see cref="ODataModelBuilder"/>.
    /// <typeparam name="TEntityType">The element type of the entity set.</typeparam>
    /// </summary>
    public class EntitySetConfiguration<TEntityType> : NavigationSourceConfiguration<TEntityType>
        where TEntityType : class
    {
        internal EntitySetConfiguration(ODataModelBuilder modelBuilder, string name)
            : base(modelBuilder, new EntitySetConfiguration(modelBuilder, typeof(TEntityType), name))
        {
        }

        internal EntitySetConfiguration(ODataModelBuilder modelBuilder, EntitySetConfiguration configuration)
            : base(modelBuilder, configuration)
        {
        }

        internal EntitySetConfiguration EntitySet
        {
            get { return (EntitySetConfiguration)Configuration; }
        }

        /// <summary>
        /// Adds a self link to the feed.
        /// </summary>
        /// <param name="feedSelfLinkFactory">The builder used to generate the link URL.</param>
        public virtual void HasFeedSelfLink(Func<FeedContext, string> feedSelfLinkFactory)
        {
            if (feedSelfLinkFactory == null)
            {
                throw Error.ArgumentNull("feedSelfLinkFactory");
            }

            EntitySet.HasFeedSelfLink(feedContext => new Uri(feedSelfLinkFactory(feedContext)));
        }

        /// <summary>
        /// Adds a self link to the feed.
        /// </summary>
        /// <param name="feedSelfLinkFactory">The builder used to generate the link URL.</param>
        public virtual void HasFeedSelfLink(Func<FeedContext, Uri> feedSelfLinkFactory)
        {
            if (feedSelfLinkFactory == null)
            {
                throw Error.ArgumentNull("feedSelfLinkFactory");
            }

            EntitySet.HasFeedSelfLink(feedSelfLinkFactory);
        }
    }
}
