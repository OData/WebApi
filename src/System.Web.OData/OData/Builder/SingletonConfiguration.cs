// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using System.Web.OData.Properties;

namespace System.Web.OData.Builder
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
    }
}
