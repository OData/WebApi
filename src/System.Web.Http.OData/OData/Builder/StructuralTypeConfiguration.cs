// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="IEdmStructuredType"/> that can be built using <see cref="ODataModelBuilder"/>.
    /// </summary>
    public abstract class StructuralTypeConfiguration : IEdmTypeConfiguration
    {
        private const string DefaultNamespace = "Default";

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuralTypeConfiguration"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        protected StructuralTypeConfiguration()
        {
            ExplicitProperties = new Dictionary<PropertyInfo, PropertyConfiguration>();
            RemovedProperties = new List<PropertyInfo>();
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
            Name = ClrType.EdmName();
            Namespace = ClrType.Namespace ?? DefaultNamespace;
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
        /// Gets the namespace of this edm type.
        /// </summary>
        public virtual string Namespace { get; protected set; }

        /// <summary>
        /// Gets the name of this edm type.
        /// </summary>
        public virtual string Name { get; protected set; }

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

            if (!propertyInfo.ReflectedType.IsAssignableFrom(ClrType))
            {
                throw Error.Argument("propertyInfo", SRResources.PropertyDoesNotBelongToType, propertyInfo.Name, ClrType.FullName);
            }

            // Remove from the ignored properties
            if (RemovedProperties.Contains(propertyInfo))
            {
                RemovedProperties.Remove(propertyInfo);
            }

            PrimitivePropertyConfiguration propertyConfiguration = null;
            if (ExplicitProperties.ContainsKey(propertyInfo))
            {
                propertyConfiguration = ExplicitProperties[propertyInfo] as PrimitivePropertyConfiguration;
                if (propertyConfiguration == null)
                {
                    throw Error.Argument("propertyInfo", SRResources.MustBePrimitiveProperty, propertyInfo.Name, ClrType.FullName);
                }
            }
            else
            {
                propertyConfiguration = new PrimitivePropertyConfiguration(propertyInfo, this);
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

            if (!propertyInfo.ReflectedType.IsAssignableFrom(ClrType))
            {
                throw Error.Argument("propertyInfo", SRResources.PropertyDoesNotBelongToType, propertyInfo.Name, ClrType.FullName);
            }

            if (propertyInfo.PropertyType == ClrType)
            {
                throw Error.Argument("propertyInfo", SRResources.RecursiveComplexTypesNotAllowed, ClrType.FullName, propertyInfo.Name);
            }

            // Remove from the ignored properties
            if (RemovedProperties.Contains(propertyInfo))
            {
                RemovedProperties.Remove(propertyInfo);
            }

            ComplexPropertyConfiguration propertyConfiguration = null;
            if (ExplicitProperties.ContainsKey(propertyInfo))
            {
                propertyConfiguration = ExplicitProperties[propertyInfo] as ComplexPropertyConfiguration;
                if (propertyConfiguration == null)
                {
                    throw Error.Argument("propertyInfo", SRResources.MustBeComplexProperty, propertyInfo.Name, ClrType.FullName);
                }
            }
            else
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

            // Remove from the ignored properties
            if (IgnoredProperties.Contains(propertyInfo))
            {
                RemovedProperties.Remove(propertyInfo);
            }

            CollectionPropertyConfiguration propertyConfiguration = null;
            if (ExplicitProperties.ContainsKey(propertyInfo))
            {
                propertyConfiguration = ExplicitProperties[propertyInfo] as CollectionPropertyConfiguration;
                if (propertyConfiguration == null)
                {
                    throw Error.Argument("propertyInfo", SRResources.MustBeCollectionProperty, propertyInfo.Name, propertyInfo.DeclaringType.FullName);
                }
            }
            else
            {
                propertyConfiguration = new CollectionPropertyConfiguration(propertyInfo, this);
                ExplicitProperties[propertyInfo] = propertyConfiguration;

                // If the ElementType is the same as this type this is recursive complex type nesting
                if (propertyConfiguration.ElementType == ClrType)
                {
                    throw Error.Argument("propertyInfo",
                                         SRResources.RecursiveComplexTypesNotAllowed,
                                         ClrType.Name,
                                         propertyConfiguration.Name);
                }

                // If the ElementType is not primitive treat as a ComplexType and Add to the model.
                IEdmPrimitiveTypeReference edmType = EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(propertyConfiguration.ElementType);
                if (edmType == null)
                {
                    ModelBuilder.AddComplexType(propertyConfiguration.ElementType);
                }
            }

            return propertyConfiguration;
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

            if (!propertyInfo.ReflectedType.IsAssignableFrom(ClrType))
            {
                throw Error.Argument("propertyInfo", SRResources.PropertyDoesNotBelongToType, propertyInfo.Name, ClrType.FullName);
            }

            if (ExplicitProperties.ContainsKey(propertyInfo))
            {
                ExplicitProperties.Remove(propertyInfo);
            }

            if (!RemovedProperties.Contains(propertyInfo))
            {
                RemovedProperties.Add(propertyInfo);
            }
        }
    }
}
