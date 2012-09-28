// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// EntityCollectionConfiguration represents a Collection of Entities.
    /// This class can be used to configure things that get bound to entities, like Actions bound to a collection.
    /// </summary>
    /// <typeparam name="TEntityType">The EntityType that is the ElementType of the EntityCollection</typeparam>
    public class EntityCollectionConfiguration<TEntityType> : CollectionTypeConfiguration
    {
        internal EntityCollectionConfiguration(IEntityTypeConfiguration elementType)
            : base(elementType, typeof(IEnumerable<TEntityType>))
        {
        }

        /// <summary>
        /// Creates a new Action that binds to Collection(EntityType).
        /// </summary>
        /// <param name="name">The name of the Action</param>
        /// <returns>An ActionConfiguration to allow further configuration of the Action.</returns>
        public ActionConfiguration Action(string name)
        {
            ActionConfiguration configuration = new ActionConfiguration(ElementType.ModelBuilder, name);
            configuration.SetBindingParameter(BindingParameterConfiguration.DefaultBindingParameterName, this, alwaysBindable: true);
            return configuration;
        }

        /// <summary>
        /// Creates a new Action that sometimes binds to Collection(EntityType).
        /// </summary>
        /// <param name="name">The name of the Action</param>
        /// <returns>An ActionConfiguration to allow further configuration of the Action.</returns>
        public ActionConfiguration TransientAction(string name)
        {
            ActionConfiguration configuration = new ActionConfiguration(ElementType.ModelBuilder, name);
            configuration.SetBindingParameter(BindingParameterConfiguration.DefaultBindingParameterName, this, alwaysBindable: false);
            return configuration;
        }
    }
}