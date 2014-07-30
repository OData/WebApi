// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Allows configuration to be performed for an entity set in a model.
    /// A <see cref="EntitySetConfiguration"/> can be obtained by using the method <see cref="ODataModelBuilder.EntitySet"/>.
    /// </summary>
    public class EntitySetConfiguration : NavigationSourceConfiguration
    {
        private Func<FeedContext, Uri> _feedSelfLinkFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetConfiguration"/> class.
        /// The default constructor is intended for use by unit testing only.
        /// </summary>
        public EntitySetConfiguration()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetConfiguration"/> class.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ODataModelBuilder"/>.</param>
        /// <param name="entityClrType">The <see cref="Type"/> of the entity type contained in this entity set.</param>
        /// <param name="name">The name of the entity set.</param>
        public EntitySetConfiguration(ODataModelBuilder modelBuilder, Type entityClrType, string name)
            : base(modelBuilder, entityClrType, name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetConfiguration"/> class.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ODataModelBuilder"/>.</param>
        /// <param name="entityType">The entity type <see cref="EntityTypeConfiguration"/> contained in this entity set.</param>
        /// <param name="name">The name of the entity set.</param>
        public EntitySetConfiguration(ODataModelBuilder modelBuilder, EntityTypeConfiguration entityType, string name)
            : base(modelBuilder, entityType, name)
        {
        }

        /// <summary>
        /// Adds a self link to the feed.
        /// </summary>
        /// <param name="feedSelfLinkFactory">The builder used to generate the link URL.</param>
        /// <returns>The navigation source configuration currently being configured.</returns>
        public virtual INavigationSourceConfiguration HasFeedSelfLink(Func<FeedContext, Uri> feedSelfLinkFactory)
        {
            _feedSelfLinkFactory = feedSelfLinkFactory;
            return this;
        }

        /// <summary>
        /// Gets the builder used to generate self links for feeds for this navigation source.
        /// </summary>
        /// <returns>The link builder.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Consistent with EF Has/Get pattern")]
        public virtual Func<FeedContext, Uri> GetFeedSelfLink()
        {
            return _feedSelfLinkFactory;
        }
    }
}
