//-----------------------------------------------------------------------------
// <copyright file="EntitySetConfigurationOfTEntityType.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Builder
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
        public virtual void HasFeedSelfLink(Func<ResourceSetContext, string> feedSelfLinkFactory)
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
        public virtual void HasFeedSelfLink(Func<ResourceSetContext, Uri> feedSelfLinkFactory)
        {
            if (feedSelfLinkFactory == null)
            {
                throw Error.ArgumentNull("feedSelfLinkFactory");
            }

            EntitySet.HasFeedSelfLink(feedSelfLinkFactory);
        }

        /// <summary>
        /// Adds subtypes to the list of derived type constraints.
        /// </summary>
        /// <param name="subtypes">The subtypes for which the constraint needs to be added.</param>
        /// <returns>Updated configuration object.</returns>
        public EntitySetConfiguration<TEntityType> HasDerivedTypeConstraints(params Type[] subtypes)
        {
            EntitySet.HasDerivedTypeConstraints(subtypes);
            return this;
        }

        /// <summary>
        /// Adds TDerivedType to the list of derived type constraints.
        /// </summary>
        /// <returns>Updated configuration object.</returns>
        public EntitySetConfiguration<TEntityType> HasDerivedTypeConstraint<TDerivedType>()
        {
            EntitySet.HasDerivedTypeConstraints(typeof(TDerivedType));
            return this;
        }

    }
}
