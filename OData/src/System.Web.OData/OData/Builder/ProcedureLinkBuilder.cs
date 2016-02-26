// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// <see cref="ProcedureLinkBuilder"/> can be used to annotate an action or a function.
    /// This is how formatters create links to invoke bound actions or functions.
    /// </summary>
    public class ProcedureLinkBuilder
    {
        private Func<EntityInstanceContext, Uri> _linkFactory; // bound to entity
        private readonly Func<FeedContext, Uri> _feedLinkFactory; // bound to collection of entity

        /// <summary>
        /// Create a new <see cref="ProcedureLinkBuilder"/> based on an entity link factory.
        /// </summary>
        /// <param name="linkFactory">The link factory this <see cref="ProcedureLinkBuilder"/> should use when building links.</param>
        /// <param name="followsConventions">
        /// A value indicating whether the link factory generates links that follow OData conventions.
        /// </param>
        public ProcedureLinkBuilder(Func<EntityInstanceContext, Uri> linkFactory, bool followsConventions)
        {
            if (linkFactory == null)
            {
                throw Error.ArgumentNull("linkFactory");
            }

            _linkFactory = linkFactory;
            FollowsConventions = followsConventions;
        }

        /// <summary>
        /// Create a new <see cref="ProcedureLinkBuilder"/> based on a feed link factory.
        /// </summary>
        /// <param name="linkFactory">The link factory this <see cref="ProcedureLinkBuilder"/> should use when building links.</param>
        /// <param name="followsConventions">
        /// A value indicating whether the action link factory generates links that follow OData conventions.
        /// </param>
        public ProcedureLinkBuilder(Func<FeedContext, Uri> linkFactory, bool followsConventions)
        {
            if (linkFactory == null)
            {
                throw Error.ArgumentNull("linkFactory");
            }

            _feedLinkFactory = linkFactory;
            FollowsConventions = followsConventions;
        }

        /// <summary>
        /// Gets the entity link factory.
        /// </summary>
        public Func<EntityInstanceContext, Uri> LinkFactory
        {
            get { return _linkFactory; }
            internal set { _linkFactory = value; }
        }

        /// <summary>
        /// Gets the feed link factory.
        /// </summary>
        public Func<FeedContext, Uri> FeedLinkFactory
        {
            get { return _feedLinkFactory; }
        }

        /// <summary>
        /// Gets a Boolean indicating whether the link factory follows OData conventions or not.
        /// </summary>
        public bool FollowsConventions { get; private set; }

        /// <summary>
        /// Builds the procedure link for the given entity.
        /// </summary>
        /// <param name="context">An instance context wrapping the entity instance.</param>
        /// <returns>The generated link.</returns>
        public virtual Uri BuildLink(EntityInstanceContext context)
        {
            if (_linkFactory == null)
            {
                return null;
            }

            return _linkFactory(context);
        }

        /// <summary>
        /// Builds the procedure link for the given feed.
        /// </summary>
        /// <param name="context">An feed context wrapping the feed instance.</param>
        /// <returns>The generated link.</returns>
        public virtual Uri BuildLink(FeedContext context)
        {
            if (_feedLinkFactory == null)
            {
                return null;
            }

            return _feedLinkFactory(context);
        }
    }
}
