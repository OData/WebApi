// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    // TODO: add support for FK properties
    // CUT: support for bi-directional properties

    /// <summary>
    /// Represents an <see cref="IEdmEntityType"/> that can be built using <see cref="ODataModelBuilder"/>.
    /// </summary>
    public class EntityTypeConfiguration : StructuralTypeConfiguration
    {
        private List<PrimitivePropertyConfiguration> _keys = new List<PrimitivePropertyConfiguration>();
        private EntityTypeConfiguration _baseType;
        private bool _baseTypeConfigured;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTypeConfiguration"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public EntityTypeConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTypeConfiguration"/> class.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ODataModelBuilder"/> being used.</param>
        /// <param name="clrType">The backing CLR type for this entity type.</param>
        public EntityTypeConfiguration(ODataModelBuilder modelBuilder, Type clrType)
            : base(modelBuilder, clrType)
        {
        }

        /// <summary>
        /// Gets the <see cref="EdmTypeKind"/> of this <see cref="IEdmTypeConfiguration"/>
        /// </summary>
        public override EdmTypeKind Kind
        {
            get { return EdmTypeKind.Entity; }
        }

        /// <summary>
        /// Gets the collection of <see cref="NavigationPropertyConfiguration"/> of this entity type.
        /// </summary>
        public virtual IEnumerable<NavigationPropertyConfiguration> NavigationProperties
        {
            get
            {
                return ExplicitProperties.Values.OfType<NavigationPropertyConfiguration>();
            }
        }

        /// <summary>
        /// Gets the collection of keys for this entity type.
        /// </summary>
        public virtual IEnumerable<PrimitivePropertyConfiguration> Keys
        {
            get
            {
                return _keys;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this type is abstract.
        /// </summary>
        public virtual bool? IsAbstract { get; set; }

        /// <summary>
        /// Gets or sets the base type of this entity type.
        /// </summary>
        public virtual EntityTypeConfiguration BaseType
        {
            get
            {
                return _baseType;
            }

            set
            {
                DerivesFrom(value);
            }
        }

        /// <summary>
        /// Gets a value that represents whether the base type is explicitly configured or inferred.
        /// </summary>
        public virtual bool BaseTypeConfigured
        {
            get
            {
                return _baseTypeConfigured;
            }
        }

        /// <summary>
        /// Marks this entity type as abstract.
        /// </summary>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public virtual EntityTypeConfiguration Abstract()
        {
            IsAbstract = true;
            return this;
        }

        /// <summary>
        /// Configures the key property(s) for this entity type.
        /// </summary>
        /// <param name="keyProperty">The property to be added to the key properties of this entity type.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public virtual EntityTypeConfiguration HasKey(PropertyInfo keyProperty)
        {
            if (BaseType != null)
            {
                throw Error.InvalidOperation(SRResources.CannotDefineKeysOnDerivedTypes, FullName, BaseType.FullName);
            }

            PrimitivePropertyConfiguration propertyConfig = AddProperty(keyProperty);

            // keys are always required
            propertyConfig.IsRequired();

            if (!_keys.Contains(propertyConfig))
            {
                _keys.Add(propertyConfig);
            }

            return this;
        }

        /// <summary>
        /// Removes the property from the entity keys collection.
        /// </summary>
        /// <param name="keyProperty">The key to be removed.</param>
        /// <remarks>This method just disable the property to be not a key anymore. It does not remove the property all together.
        /// To remove the property completely, use the method <see cref="RemoveProperty"/></remarks>
        public virtual void RemoveKey(PrimitivePropertyConfiguration keyProperty)
        {
            if (keyProperty == null)
            {
                throw Error.ArgumentNull("keyProperty");
            }

            _keys.Remove(keyProperty);
        }

        /// <summary>
        /// Sets the base type of this entity type to <c>null</c> meaning that this entity type 
        /// does not derive from anything.
        /// </summary>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public virtual EntityTypeConfiguration DerivesFromNothing()
        {
            _baseType = null;
            _baseTypeConfigured = true;
            return this;
        }

        /// <summary>
        /// Sets the base type of this entity type.
        /// </summary>
        /// <param name="baseType">The base entity type.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public virtual EntityTypeConfiguration DerivesFrom(EntityTypeConfiguration baseType)
        {
            if (baseType == null)
            {
                throw Error.ArgumentNull("baseType");
            }

            _baseType = baseType;
            _baseTypeConfigured = true;

            if (!baseType.ClrType.IsAssignableFrom(ClrType) || baseType.ClrType == ClrType)
            {
                throw Error.Argument("baseType", SRResources.TypeDoesNotInheritFromBaseType, ClrType.FullName, baseType.ClrType.FullName);
            }

            if (Keys.Any())
            {
                throw Error.InvalidOperation(SRResources.CannotDefineKeysOnDerivedTypes, FullName, baseType.FullName);
            }

            foreach (PropertyConfiguration property in Properties)
            {
                ValidatePropertyNotAlreadyDefinedInBaseTypes(property.PropertyInfo);
            }

            foreach (PropertyConfiguration property in this.DerivedProperties())
            {
                ValidatePropertyNotAlreadyDefinedInDerivedTypes(property.PropertyInfo);
            }

            return this;
        }

        /// <summary>
        /// Adds a new EDM primitive property to this entity type.
        /// </summary>
        /// <param name="propertyInfo">The backing CLR property.</param>
        /// <returns>Returns the <see cref="PrimitivePropertyConfiguration"/> of the added property.</returns>
        public override PrimitivePropertyConfiguration AddProperty(PropertyInfo propertyInfo)
        {
            ValidatePropertyNotAlreadyDefinedInBaseTypes(propertyInfo);
            ValidatePropertyNotAlreadyDefinedInDerivedTypes(propertyInfo);

            return base.AddProperty(propertyInfo);
        }

        /// <summary>
        /// Adds a new EDM complex property to this entity type.
        /// </summary>
        /// <param name="propertyInfo">The backing CLR property.</param>
        /// <returns>Returns the <see cref="ComplexPropertyConfiguration"/> of the added property.</returns>
        public override ComplexPropertyConfiguration AddComplexProperty(PropertyInfo propertyInfo)
        {
            ValidatePropertyNotAlreadyDefinedInBaseTypes(propertyInfo);
            ValidatePropertyNotAlreadyDefinedInDerivedTypes(propertyInfo);

            return base.AddComplexProperty(propertyInfo);
        }

        /// <summary>
        /// Adds a new EDM collection property to this entity type.
        /// </summary>
        /// <param name="propertyInfo">The backing CLR property.</param>
        /// <returns>Returns the <see cref="CollectionPropertyConfiguration"/> of the added property.</returns>
        public override CollectionPropertyConfiguration AddCollectionProperty(PropertyInfo propertyInfo)
        {
            ValidatePropertyNotAlreadyDefinedInBaseTypes(propertyInfo);
            ValidatePropertyNotAlreadyDefinedInDerivedTypes(propertyInfo);

            return base.AddCollectionProperty(propertyInfo);
        }

        /// <summary>
        /// Adds a new EDM navigation property to this entity type.
        /// </summary>
        /// <param name="navigationProperty">The backing CLR property.</param>
        /// <param name="multiplicity">The <see cref="EdmMultiplicity"/> of the navigation property.</param>
        /// <returns>Returns the <see cref="NavigationPropertyConfiguration"/> of the added property.</returns>
        public virtual NavigationPropertyConfiguration AddNavigationProperty(PropertyInfo navigationProperty, EdmMultiplicity multiplicity)
        {
            if (navigationProperty == null)
            {
                throw Error.ArgumentNull("navigationProperty");
            }

            if (!navigationProperty.ReflectedType.IsAssignableFrom(ClrType))
            {
                throw Error.Argument("navigationProperty", SRResources.PropertyDoesNotBelongToType, navigationProperty.Name, ClrType.FullName);
            }

            ValidatePropertyNotAlreadyDefinedInBaseTypes(navigationProperty);
            ValidatePropertyNotAlreadyDefinedInDerivedTypes(navigationProperty);

            PropertyConfiguration propertyConfig;
            NavigationPropertyConfiguration navigationPropertyConfig;

            if (ExplicitProperties.ContainsKey(navigationProperty))
            {
                propertyConfig = ExplicitProperties[navigationProperty];
                if (propertyConfig.Kind != PropertyKind.Navigation)
                {
                    throw Error.Argument("navigationProperty", SRResources.MustBeNavigationProperty, navigationProperty.Name, ClrType.FullName);
                }

                navigationPropertyConfig = propertyConfig as NavigationPropertyConfiguration;
                if (navigationPropertyConfig.Multiplicity != multiplicity)
                {
                    throw Error.Argument("navigationProperty", SRResources.MustHaveMatchingMultiplicity, navigationProperty.Name, multiplicity);
                }
            }
            else
            {
                navigationPropertyConfig = new NavigationPropertyConfiguration(navigationProperty, multiplicity, this);
                ExplicitProperties[navigationProperty] = navigationPropertyConfig;
                // make sure the related type is configured
                ModelBuilder.AddEntity(navigationPropertyConfig.RelatedClrType);
            }
            return navigationPropertyConfig;
        }

        /// <summary>
        /// Removes the property from the entity.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> of the property to be removed.</param>
        public override void RemoveProperty(PropertyInfo propertyInfo)
        {
            base.RemoveProperty(propertyInfo);
            _keys.RemoveAll(p => p.PropertyInfo == propertyInfo);
        }

        private void ValidatePropertyNotAlreadyDefinedInBaseTypes(PropertyInfo propertyInfo)
        {
            PropertyConfiguration baseProperty = this.DerivedProperties().Where(p => p.Name == propertyInfo.Name).FirstOrDefault();
            if (baseProperty != null)
            {
                throw Error.Argument("propertyInfo", SRResources.CannotRedefineBaseTypeProperty, propertyInfo.Name, baseProperty.PropertyInfo.ReflectedType.FullName);
            }
        }

        private void ValidatePropertyNotAlreadyDefinedInDerivedTypes(PropertyInfo propertyInfo)
        {
            foreach (EntityTypeConfiguration derivedEntity in ModelBuilder.DerivedTypes(this))
            {
                PropertyConfiguration propertyInDerivedType = derivedEntity.Properties.Where(p => p.Name == propertyInfo.Name).FirstOrDefault();
                if (propertyInDerivedType != null)
                {
                    throw Error.Argument("propertyInfo", SRResources.PropertyAlreadyDefinedInDerivedType, propertyInfo.Name, FullName, derivedEntity.FullName);
                }
            }
        }
    }
}
