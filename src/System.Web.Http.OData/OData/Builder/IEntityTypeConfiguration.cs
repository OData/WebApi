// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="IEdmEntityType"/> that can be built using <see cref="ODataModelBuilder"/>.
    /// </summary>
    public interface IEntityTypeConfiguration : IStructuralTypeConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether this type is abstract.
        /// If the property is not set the entity is assumed to be non-abstract.
        /// </summary>
        bool? IsAbstract { get; set; }

        /// <summary>
        /// Gets the base type of this entity type.
        /// </summary>
        IEntityTypeConfiguration BaseType { get; }

        /// <summary>
        /// Gets if the base type for this entity type was configured explicitly by the user.
        /// </summary>
        bool BaseTypeConfigured { get; }

        /// <summary>
        /// Gets the key properties of this entity type.
        /// </summary>
        IEnumerable<PrimitivePropertyConfiguration> Keys { get; }

        /// <summary>
        /// Gets the navigation properties of this entity type.
        /// </summary>
        IEnumerable<NavigationPropertyConfiguration> NavigationProperties { get; }

        /// <summary>
        /// Configures the key property(s) for this entity type.
        /// </summary>
        /// <param name="keyProperty">The property to be added to the key properties of this entity type.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        IEntityTypeConfiguration HasKey(PropertyInfo keyProperty);

        /// <summary>
        /// Adds a new navigation property to this entity type.
        /// </summary>
        /// <param name="navigationProperty">The property to be added as a navigation property.</param>
        /// <param name="multiplicity">The <see cref="EdmMultiplicity"/> of the navigation property.</param>
        /// <returns>The <see cref="NavigationPropertyConfiguration"/> of the added property.</returns>
        NavigationPropertyConfiguration AddNavigationProperty(PropertyInfo navigationProperty, EdmMultiplicity multiplicity);

        /// <summary>
        /// Sets the base type of this entity type.
        /// </summary>
        /// <param name="baseType">The base entity type</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        IEntityTypeConfiguration DerivesFrom(IEntityTypeConfiguration baseType);

        /// <summary>
        /// Sets the base type of this entity type to <c>null</c> meaning that this entity type 
        /// does not derive from anything.
        /// </summary>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        IEntityTypeConfiguration DerivesFromNothing();
    }
}
