// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="IEdmStructuredType"/> that can be built using <see cref="ODataModelBuilder"/>.
    /// </summary>
    public abstract class StructuralTypeConfiguration : IEdmTypeConfiguration
    {
        private string _namespace;
        private string _name;
        private PropertyInfo _dynamicPropertyDictionary;
        private PropertyInfo _instanceAnnotationDictionary;
        private StructuralTypeConfiguration _baseType;
        private bool _baseTypeConfigured;

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuralTypeConfiguration"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        protected StructuralTypeConfiguration()
        {
            ExplicitProperties = new Dictionary<PropertyInfo, PropertyConfiguration>();
            RemovedProperties = new List<PropertyInfo>();
            QueryConfiguration = new QueryConfiguration();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuralTypeConfiguration"/> class.
        /// </summary>
        /// <param name="clrType">The backing CLR type for this EDM structural type.</param>
        /// <param name="modelBuilder">The associated <see cref="ODataModelBuilder"/>.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The virtual property setters are only to support mocking frameworks, in which case this constructor shouldn't be called anyway.")]
        protected StructuralTypeConfiguration(ODataModelBuilder modelBuilder, Type clrType)
            : this()
        {
            if (modelBuilder == null)
            {
                throw Error.ArgumentNull("modelBuilder");
            }

            if (clrType == null)
            {
                throw Error.ArgumentNull("clrType");
            }

            ClrType = clrType;
            ModelBuilder = modelBuilder;
            _name = clrType.EdmName();

            // Use the namespace if one was provided in builder by the user, otherwise fallback to CLR Namespace.
            // If CLR Namespace is null we fallback to "Default"
            // This can still be overriden by using DataContract attribute.
            _namespace = modelBuilder.HasAssignedNamespace ? modelBuilder.Namespace : clrType.Namespace ?? modelBuilder.Namespace;
        }

        /// <summary>
        /// Gets the <see cref="EdmTypeKind"/> of this edm type.
        /// </summary>
        public abstract EdmTypeKind Kind { get; }

        /// <summary>
        /// Gets the backing CLR <see cref="Type"/>.
        /// </summary>
        public virtual Type ClrType { get; private set; }

        /// <summary>
        /// Gets the full name of this edm type.
        /// </summary>
        public virtual string FullName
        {
            get
            {
                return Namespace + "." + Name;
            }
        }

        /// <summary>
        /// Gets or sets the namespace of this EDM type.
        /// </summary>
        public virtual string Namespace
        {
            get
            {
                return _namespace;
            }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _namespace = value;
                AddedExplicitly = true;
            }
        }

        /// <summary>
        /// Gets or sets the name of this EDM type.
        /// </summary>
        public virtual string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _name = value;
                AddedExplicitly = true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this type is open or not.
        /// </summary>
        public bool IsOpen
        {
            get { return _dynamicPropertyDictionary != null; }
        }

        /// <summary>
        /// Gets the CLR property info of the dynamic property dictionary on this structural type.
        /// </summary>
        public PropertyInfo DynamicPropertyDictionary
        {
            get { return _dynamicPropertyDictionary; }
        }

        /// <summary>
        /// Gets a value indicating whether this type has instance annotations or not.
        /// </summary>
        public bool IsWithInstanceAnnotations
        {
            get { return _instanceAnnotationDictionary != null; }
        }

        /// <summary>
        /// Gets the CLR property info of the instance annotations dictionary on this structural type.
        /// </summary>
        public PropertyInfo InstanceAnnotationsDictionary
        {
            get { return _instanceAnnotationDictionary; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this type is abstract.
        /// </summary>
        public virtual bool? IsAbstract { get; set; }

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
        /// Gets the declared properties on this edm type.
        /// </summary>
        public IEnumerable<PropertyConfiguration> Properties
        {
            get
            {
                return ExplicitProperties.Values;
            }
        }

        /// <summary>
        /// Gets the properties from the backing CLR type that are to be ignored on this edm type.
        /// </summary>
        public ReadOnlyCollection<PropertyInfo> IgnoredProperties
        {
            get
            {
                return new ReadOnlyCollection<PropertyInfo>(RemovedProperties);
            }
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
        /// Gets or sets the <see cref="QueryConfiguration"/>.
        /// </summary>
        public QueryConfiguration QueryConfiguration { get; set; }

        /// <summary>
        /// Gets or sets a value that is <c>true</c> if the type's name or namespace was set by the user; <c>false</c> if it was inferred through conventions.
        /// </summary>
        /// <remarks>The default value is <c>false</c>.</remarks>
        public bool AddedExplicitly { get; set; }

        /// <summary>
        /// The <see cref="ODataModelBuilder"/>.
        /// </summary>
        public virtual ODataModelBuilder ModelBuilder { get; private set; }

        /// <summary>
        /// Gets the collection of explicitly removed properties.
        /// </summary>
        protected internal IList<PropertyInfo> RemovedProperties { get; private set; }

        /// <summary>
        /// Gets the collection of explicitly added properties.
        /// </summary>
        protected internal IDictionary<PropertyInfo, PropertyConfiguration> ExplicitProperties { get; private set; }

        /// <summary>
        /// Gets the base type of this structural type.
        /// </summary>
        protected internal virtual StructuralTypeConfiguration BaseTypeInternal
        {
            get
            {
                return _baseType;
            }
        }

        internal virtual void AbstractImpl()
        {
            IsAbstract = true;
        }

        internal virtual void DerivesFromNothingImpl()
        {
            _baseType = null;
            _baseTypeConfigured = true;
        }

        internal virtual void DerivesFromImpl(StructuralTypeConfiguration baseType)
        {
            if (baseType == null)
            {
                throw Error.ArgumentNull("baseType");
            }

            _baseType = baseType;
            _baseTypeConfigured = true;

            if (!baseType.ClrType.IsAssignableFrom(ClrType) || baseType.ClrType == ClrType)
            {
                throw Error.Argument("baseType", SRResources.TypeDoesNotInheritFromBaseType,
                    ClrType.FullName, baseType.ClrType.FullName);
            }

            foreach (PropertyConfiguration property in Properties)
            {
                ValidatePropertyNotAlreadyDefinedInBaseTypes(property.PropertyInfo);
            }

            foreach (PropertyConfiguration property in this.DerivedProperties())
            {
                ValidatePropertyNotAlreadyDefinedInDerivedTypes(property.PropertyInfo);
            }
        }

        /// <summary>
        /// Adds a primitive property to this edm type.
        /// </summary>
        /// <param name="propertyInfo">The property being added.</param>
        /// <returns>The <see cref="PrimitivePropertyConfiguration"/> so that the property can be configured further.</returns>
        public virtual PrimitivePropertyConfiguration AddProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            if (!TypeHelper.GetReflectedType(propertyInfo).IsAssignableFrom(ClrType))
            {
                throw Error.Argument("propertyInfo", SRResources.PropertyDoesNotBelongToType, propertyInfo.Name, ClrType.FullName);
            }

            ValidatePropertyNotAlreadyDefinedInBaseTypes(propertyInfo);
            ValidatePropertyNotAlreadyDefinedInDerivedTypes(propertyInfo);

            // Remove from the ignored properties
            if (RemovedProperties.Any(prop => prop.Name.Equals(propertyInfo.Name)))
            {
                RemovedProperties.Remove(RemovedProperties.First(prop => prop.Name.Equals(propertyInfo.Name)));
            }

            PrimitivePropertyConfiguration propertyConfiguration =
                ValidatePropertyNotAlreadyDefinedOtherTypes<PrimitivePropertyConfiguration>(propertyInfo,
                    SRResources.MustBePrimitiveProperty);
            if (propertyConfiguration == null)
            {
                propertyConfiguration = new PrimitivePropertyConfiguration(propertyInfo, this);
                var primitiveType = EdmLibHelpers.GetEdmPrimitiveTypeOrNull(propertyInfo.PropertyType);
                if (primitiveType != null)
                {
                    if (primitiveType.PrimitiveKind == EdmPrimitiveTypeKind.Decimal)
                    {
                        propertyConfiguration = new DecimalPropertyConfiguration(propertyInfo, this);
                    }
                    else if (EdmLibHelpers.HasLength(primitiveType.PrimitiveKind))
                    {
                        propertyConfiguration = new LengthPropertyConfiguration(propertyInfo, this);
                    }
                    else if (EdmLibHelpers.HasPrecision(primitiveType.PrimitiveKind))
                    {
                        propertyConfiguration = new PrecisionPropertyConfiguration(propertyInfo, this);
                    }
                }
                ExplicitProperties[propertyInfo] = propertyConfiguration;
            }

            return propertyConfiguration;
        }

        /// <summary>
        /// Adds an enum property to this edm type.
        /// </summary>
        /// <param name="propertyInfo">The property being added.</param>
        /// <returns>The <see cref="EnumPropertyConfiguration"/> so that the property can be configured further.</returns>
        public virtual EnumPropertyConfiguration AddEnumProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            if (!TypeHelper.GetReflectedType(propertyInfo).IsAssignableFrom(ClrType))
            {
                throw Error.Argument("propertyInfo", SRResources.PropertyDoesNotBelongToType, propertyInfo.Name, ClrType.FullName);
            }

            if (!TypeHelper.IsEnum(propertyInfo.PropertyType))
            {
                throw Error.Argument("propertyInfo", SRResources.MustBeEnumProperty, propertyInfo.Name, ClrType.FullName);
            }

            ValidatePropertyNotAlreadyDefinedInBaseTypes(propertyInfo);
            ValidatePropertyNotAlreadyDefinedInDerivedTypes(propertyInfo);

            // Remove from the ignored properties
            if (RemovedProperties.Any(prop => prop.Name.Equals(propertyInfo.Name)))
            {
                RemovedProperties.Remove(RemovedProperties.First(prop => prop.Name.Equals(propertyInfo.Name)));
            }

            EnumPropertyConfiguration propertyConfiguration =
                ValidatePropertyNotAlreadyDefinedOtherTypes<EnumPropertyConfiguration>(propertyInfo,
                    SRResources.MustBeEnumProperty);
            if (propertyConfiguration == null)
            {
                propertyConfiguration = new EnumPropertyConfiguration(propertyInfo, this);
                ExplicitProperties[propertyInfo] = propertyConfiguration;
            }

            return propertyConfiguration;
        }

        /// <summary>
        /// Adds a complex property to this edm type.
        /// </summary>
        /// <param name="propertyInfo">The property being added.</param>
        /// <returns>The <see cref="ComplexPropertyConfiguration"/> so that the property can be configured further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Helper validates non null propertyInfo")]
        public virtual ComplexPropertyConfiguration AddComplexProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            if (!TypeHelper.GetReflectedType(propertyInfo).IsAssignableFrom(ClrType))
            {
                throw Error.Argument("propertyInfo", SRResources.PropertyDoesNotBelongToType, propertyInfo.Name, ClrType.FullName);
            }

            ValidatePropertyNotAlreadyDefinedInBaseTypes(propertyInfo);
            ValidatePropertyNotAlreadyDefinedInDerivedTypes(propertyInfo);

            // Remove from the ignored properties
            if (RemovedProperties.Any(prop => prop.Name.Equals(propertyInfo.Name)))
            {
                RemovedProperties.Remove(RemovedProperties.First(prop => prop.Name.Equals(propertyInfo.Name)));
            }

            ComplexPropertyConfiguration propertyConfiguration =
                ValidatePropertyNotAlreadyDefinedOtherTypes<ComplexPropertyConfiguration>(propertyInfo,
                    SRResources.MustBeComplexProperty);
            if (propertyConfiguration == null)
            {
                propertyConfiguration = new ComplexPropertyConfiguration(propertyInfo, this);
                ExplicitProperties[propertyInfo] = propertyConfiguration;
                // Make sure the complex type is in the model.

                ModelBuilder.AddComplexType(propertyInfo.PropertyType);
            }

            return propertyConfiguration;
        }

        /// <summary>
        /// Adds a collection property to this edm type.
        /// </summary>
        /// <param name="propertyInfo">The property being added.</param>
        /// <returns>The <see cref="CollectionPropertyConfiguration"/> so that the property can be configured further.</returns>
        public virtual CollectionPropertyConfiguration AddCollectionProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            if (!propertyInfo.DeclaringType.IsAssignableFrom(ClrType))
            {
                throw Error.Argument("propertyInfo", SRResources.PropertyDoesNotBelongToType);
            }

            ValidatePropertyNotAlreadyDefinedInBaseTypes(propertyInfo);
            ValidatePropertyNotAlreadyDefinedInDerivedTypes(propertyInfo);

            // Remove from the ignored properties
            if (RemovedProperties.Any(prop => prop.Name.Equals(propertyInfo.Name)))
            {
                RemovedProperties.Remove(RemovedProperties.First(prop => prop.Name.Equals(propertyInfo.Name)));
            }

            CollectionPropertyConfiguration propertyConfiguration =
                ValidatePropertyNotAlreadyDefinedOtherTypes<CollectionPropertyConfiguration>(propertyInfo,
                    SRResources.MustBeCollectionProperty);
            if (propertyConfiguration == null)
            {
                propertyConfiguration = new CollectionPropertyConfiguration(propertyInfo, this);
                ExplicitProperties[propertyInfo] = propertyConfiguration;

                // If the ElementType is not primitive or enum treat as a ComplexType and Add to the model.
                IEdmPrimitiveTypeReference edmType =
                    EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(propertyConfiguration.ElementType);
                if (edmType == null)
                {
                    if (!TypeHelper.IsEnum(propertyConfiguration.ElementType))
                    {
                        ModelBuilder.AddComplexType(propertyConfiguration.ElementType);
                    }
                }
            }

            return propertyConfiguration;
        }

        /// <summary>
        /// Adds the property info of the dynamic properties to this structural type.
        /// </summary>
        /// <param name="propertyInfo">The property being added.</param>
        public virtual void AddDynamicPropertyDictionary(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            if (!typeof(IDictionary<string, object>).IsAssignableFrom(propertyInfo.PropertyType))
            {
                throw Error.Argument("propertyInfo", SRResources.ArgumentMustBeOfType,
                    "IDictionary<string, object>");
            }

            if (!propertyInfo.DeclaringType.IsAssignableFrom(ClrType))
            {
                throw Error.Argument("propertyInfo", SRResources.PropertyDoesNotBelongToType);
            }

            // Remove from the ignored properties
            if (IgnoredProperties.Contains(propertyInfo))
            {
                RemovedProperties.Remove(propertyInfo);
            }

            if (_dynamicPropertyDictionary != null)
            {
                throw Error.Argument("propertyInfo", SRResources.MoreThanOneDynamicPropertyContainerFound, ClrType.Name);
            }

            _dynamicPropertyDictionary = propertyInfo;
        }


        /// <summary>
        /// Adds the property info of the instanceannotation to this structural type.
        /// </summary>
        /// <param name="propertyInfo">The property being added.</param>
        public virtual void AddInstanceAnnotationDictionary(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            if (!typeof(IDictionary<string, IDictionary<string,object>>).IsAssignableFrom(propertyInfo.PropertyType))
            {
                throw Error.Argument("propertyInfo", SRResources.ArgumentMustBeOfType,
                    "IDictionary<string, IDictionary<string, object>>");
            }

            if (!propertyInfo.DeclaringType.IsAssignableFrom(ClrType))
            {
                throw Error.Argument("propertyInfo", SRResources.PropertyDoesNotBelongToType);
            }

            // Remove from the ignored properties
            if (IgnoredProperties.Contains(propertyInfo))
            {
                RemovedProperties.Remove(propertyInfo);
            }

            if (_instanceAnnotationDictionary != null)
            {
                throw Error.Argument("propertyInfo", SRResources.MoreThanOneAnnotationPropertyContainerFound, ClrType.Name);
            }

            _instanceAnnotationDictionary = propertyInfo;
        }

        /// <summary>
        /// Removes the given property.
        /// </summary>
        /// <param name="propertyInfo">The property being removed.</param>
        public virtual void RemoveProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            if (!TypeHelper.GetReflectedType(propertyInfo).IsAssignableFrom(ClrType))
            {
                throw Error.Argument("propertyInfo", SRResources.PropertyDoesNotBelongToType, propertyInfo.Name, ClrType.FullName);
            }

            if (ExplicitProperties.Keys.Any(key => key.Name.Equals(propertyInfo.Name)))
            {
                ExplicitProperties.Remove(ExplicitProperties.Keys.First(key => key.Name.Equals(propertyInfo.Name)));
            }

            if (!RemovedProperties.Any(prop => prop.Name.Equals(propertyInfo.Name)))
            {
                RemovedProperties.Add(propertyInfo);
            }

            if (_dynamicPropertyDictionary == propertyInfo)
            {
                _dynamicPropertyDictionary = null;
            }
        }

        /// <summary>
        /// Adds a non-contained EDM navigation property to this entity type.
        /// </summary>
        /// <param name="navigationProperty">The backing CLR property.</param>
        /// <param name="multiplicity">The <see cref="EdmMultiplicity"/> of the navigation property.</param>
        /// <returns>Returns the <see cref="NavigationPropertyConfiguration"/> of the added property.</returns>
        public virtual NavigationPropertyConfiguration AddNavigationProperty(PropertyInfo navigationProperty, EdmMultiplicity multiplicity)
        {
            return AddNavigationProperty(navigationProperty, multiplicity, containsTarget: false);
        }

        /// <summary>
        /// Adds a contained EDM navigation property to this entity type.
        /// </summary>
        /// <param name="navigationProperty">The backing CLR property.</param>
        /// <param name="multiplicity">The <see cref="EdmMultiplicity"/> of the navigation property.</param>
        /// <returns>Returns the <see cref="NavigationPropertyConfiguration"/> of the added property.</returns>
        public virtual NavigationPropertyConfiguration AddContainedNavigationProperty(PropertyInfo navigationProperty, EdmMultiplicity multiplicity)
        {
            return AddNavigationProperty(navigationProperty, multiplicity, containsTarget: true);
        }

        private NavigationPropertyConfiguration AddNavigationProperty(PropertyInfo navigationProperty, EdmMultiplicity multiplicity, bool containsTarget)
        {
            if (navigationProperty == null)
            {
                throw Error.ArgumentNull("navigationProperty");
            }

            if (!TypeHelper.GetReflectedType(navigationProperty).IsAssignableFrom(ClrType))
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
                navigationPropertyConfig = new NavigationPropertyConfiguration(
                    navigationProperty,
                    multiplicity,
                    this);
                if (containsTarget)
                {
                    navigationPropertyConfig = navigationPropertyConfig.Contained();
                }

                ExplicitProperties[navigationProperty] = navigationPropertyConfig;
                // make sure the related type is configured
                ModelBuilder.AddEntityType(navigationPropertyConfig.RelatedClrType);
            }
            return navigationPropertyConfig;
        }

        internal T ValidatePropertyNotAlreadyDefinedOtherTypes<T>(PropertyInfo propertyInfo, string typeErrorMessage) where T : class
        {
            T propertyConfiguration = default(T);
            var explicitPropertyInfo = ExplicitProperties.Keys.FirstOrDefault(key => key.Name.Equals(propertyInfo.Name));
            if (explicitPropertyInfo != null)
            {
                propertyConfiguration = ExplicitProperties[explicitPropertyInfo] as T;
                if (propertyConfiguration == default(T))
                {
                    throw Error.Argument("propertyInfo", typeErrorMessage, propertyInfo.Name, ClrType.FullName);
                }
            }

            return propertyConfiguration;
        }

        internal void ValidatePropertyNotAlreadyDefinedInBaseTypes(PropertyInfo propertyInfo)
        {
            PropertyConfiguration baseProperty =
                this.DerivedProperties().FirstOrDefault(p => p.Name == propertyInfo.Name);
            if (baseProperty != null)
            {
                throw Error.Argument("propertyInfo", SRResources.CannotRedefineBaseTypeProperty,
                    propertyInfo.Name, TypeHelper.GetReflectedType(baseProperty.PropertyInfo).FullName);
            }
        }

        internal void ValidatePropertyNotAlreadyDefinedInDerivedTypes(PropertyInfo propertyInfo)
        {
            foreach (StructuralTypeConfiguration derivedType in ModelBuilder.DerivedTypes(this))
            {
                PropertyConfiguration propertyInDerivedType =
                    derivedType.Properties.FirstOrDefault(p => p.Name == propertyInfo.Name);
                if (propertyInDerivedType != null)
                {
                    throw Error.Argument("propertyInfo", SRResources.PropertyAlreadyDefinedInDerivedType,
                        propertyInfo.Name, FullName, derivedType.FullName);
                }
            }
        }
    }
}
