// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Allows configuration to be performed for a singleton in a model.
    /// A <see cref="SingletonConfiguration"/> can be obtained by using the method <see cref="ODataModelBuilder.Singletons"/>.
    /// </summary>
    public class SingletonConfiguration : NavigationSourceConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonConfiguration"/> class.
        /// The default constructor is intended for use by unit testing only.
        /// </summary>
        public SingletonConfiguration()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonConfiguration"/> class.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ODataModelBuilder"/>.</param>
        /// <param name="entityClrType">The <see cref="Type"/> of the entity type contained in this singleton.</param>
        /// <param name="name">The name of the singleton.</param>
        public SingletonConfiguration(ODataModelBuilder modelBuilder, Type entityClrType, string name)
            : base(modelBuilder, entityClrType, name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonConfiguration"/> class.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ODataModelBuilder"/>.</param>
        /// <param name="entityType">The entity type <see cref="EntityTypeConfiguration"/> contained in this singleton.</param>
        /// <param name="name">The name of the singleton.</param>
        public SingletonConfiguration(ODataModelBuilder modelBuilder, EntityTypeConfiguration entityType, string name)
            : base(modelBuilder, entityType, name)
        {
        }

        /// <summary>
        /// Adds subtypes to the list of derived type constraints.
        /// </summary>
        /// <param name="subtypes">The subtypes for which the constraint needs to be added.</param>
        /// <returns>Updated configuration object.</returns>
        public SingletonConfiguration HasDerivedTypeConstraints(params Type[] subtypes)
        {
            DerivedTypeConstraints.AddConstraints(subtypes);
            return this;
        }

        /// <summary>
        /// Adds TDerived to the list of derived type constraints.
        /// </summary>
        /// <returns>Updated configuration object.</returns>
        public SingletonConfiguration HasDerivedTypeConstraint<TDerivedType>()
        {
            DerivedTypeConstraints.AddConstraint<TDerivedType>();
            return this;
        }
    }
}
