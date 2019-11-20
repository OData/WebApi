// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="IEdmSingleton"/> that can be built using <see cref="ODataModelBuilder"/>.
    /// </summary>
    public class SingletonConfiguration<TEntityType> : NavigationSourceConfiguration<TEntityType> where TEntityType : class
    {
        internal SingletonConfiguration(ODataModelBuilder modelBuilder, string name)
            : base(modelBuilder, new SingletonConfiguration(modelBuilder, typeof(TEntityType), name))
        {
        }

        internal SingletonConfiguration(ODataModelBuilder modelBuilder, SingletonConfiguration configuration)
            : base(modelBuilder, configuration)
        {
        }

        internal SingletonConfiguration Singleton
        {
            get { return (SingletonConfiguration)Configuration; }
        }

        /// <summary>
        /// Adds subtypes to the list of derived type constraints.
        /// </summary>
        /// <param name="subtypes">The subtypes for which the constraint needs to be added.</param>
        /// <returns>Updated configuration object.</returns>
        public SingletonConfiguration<TEntityType> AddDerivedTypeConstraint(params Type[] subtypes)
        {
            Singleton.AddDerivedTypeConstraint(subtypes);
            return this;
        }
    }
}
