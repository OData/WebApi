﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// EntityCollectionConfiguration represents a Collection of Entities.
    /// This class can be used to configure things that get bound to entities, like Actions bound to a collection.
    /// </summary>
    /// <typeparam name="TEntityType">The EntityType that is the ElementType of the EntityCollection</typeparam>
    public class EntityCollectionConfiguration<TEntityType> : CollectionTypeConfiguration
    {
        internal EntityCollectionConfiguration(EntityTypeConfiguration elementType)
            : base(elementType, typeof(IEnumerable<TEntityType>))
        {
        }

        /// <summary>
        /// Creates a new Action that binds to Collection(EntityType).
        /// </summary>
        /// <param name="name">The name of the Action</param>
        /// <returns>An <see cref="ActionConfiguration"/> to allow further configuration of the Action.</returns>
        public ActionConfiguration Action(string name)
        {
            Contract.Assert(ModelBuilder != null);
            ActionConfiguration configuration = ModelBuilder.Action(name);
            configuration.SetBindingParameter(BindingParameterConfiguration.DefaultBindingParameterName, this);
            return configuration;
        }

        /// <summary>
        /// Creates a new Function that binds to Collection(EntityType).
        /// </summary>
        /// <param name="name">The name of the Function</param>
        /// <returns>A <see cref="FunctionConfiguration"/> to allow further configuration of the Function.</returns>
        public FunctionConfiguration Function(string name)
        {
            Contract.Assert(ModelBuilder != null);
            FunctionConfiguration configuration = ModelBuilder.Function(name);
            configuration.SetBindingParameter(BindingParameterConfiguration.DefaultBindingParameterName, this);
            return configuration;
        }
    }
}