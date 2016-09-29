// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="IEdmStructuredType"/> that can be built using <see cref="ODataModelBuilder"/>.
    /// </summary>
    public abstract class StructuralTypeConfiguration<TStructuralType> where TStructuralType : class
    {
        private StructuralTypeConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuralTypeConfiguration{TStructuralType}"/> class.
        /// </summary>
        /// <param name="configuration">The inner configuration of the structural type.</param>
        protected StructuralTypeConfiguration(StructuralTypeConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }
            _configuration = configuration;
        }

        /// <summary>
        /// Gets the collection of EDM structural properties that belong to this type.
        /// </summary>
        public IEnumerable<PropertyConfiguration> Properties
        {
            get { return _configuration.Properties; }
        }

        /// <summary>
        /// Gets the full name of this EDM type.
        /// </summary>
        public string FullName
        {
            get
            {
                return _configuration.FullName;
            }
        }

        /// <summary>
        /// Gets and sets the namespace of this EDM type.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Namespace", Justification = "Follow StructuralTypeConfiguration's naming")]
        public string Namespace
        {
            get
            {
                return _configuration.Namespace;
            }
            set
            {
                _configuration.Namespace = value;
            }
        }

        /// <summary>
        /// Gets and sets the name of this EDM type.
        /// </summary>
        public string Name
        {
            get
            {
                return _configuration.Name;
            }
            set
            {
                _configuration.Name = value;
            }
        }

        /// <summary>
        /// Gets an indicator whether this EDM type is an open type or not.
        /// Returns <c>true</c> if this is an open type; <c>false</c> otherwise.
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return _configuration.IsOpen;
            }
        }

        internal StructuralTypeConfiguration Configuration
        {
            get { return _configuration; }
        }

        /// <summary>
        /// Excludes a property from the type.
        /// </summary>
        /// <typeparam name="TProperty">The property type.</typeparam>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <remarks>This method is used to exclude properties from the type that would have been added by convention during model discovery.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        public virtual void Ignore<TProperty>(Expression<Func<TStructuralType, TProperty>> propertyExpression)
        {
            PropertyInfo ignoredProperty = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);
            _configuration.RemoveProperty(ignoredProperty);
        }

        /// <summary>
        /// Adds a string property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrimitivePropertyConfiguration Property(Expression<Func<TStructuralType, string>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, optional: true);
        }

        /// <summary>
        /// Adds a binary property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrimitivePropertyConfiguration Property(Expression<Func<TStructuralType, byte[]>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, optional: true);
        }

        /// <summary>
        /// Adds a stream property the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrimitivePropertyConfiguration Property(Expression<Func<TStructuralType, Stream>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, optional: true);
        }

        /// <summary>
        /// Adds an optional primitive property to the EDM type.
        /// </summary>
        /// <typeparam name="T">The primitive property type.</typeparam>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrimitivePropertyConfiguration Property<T>(Expression<Func<TStructuralType, T?>> propertyExpression) where T : struct
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, optional: true);
        }

        /// <summary>
        /// Adds a required primitive property to the EDM type.
        /// </summary>
        /// <typeparam name="T">The primitive property type.</typeparam>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrimitivePropertyConfiguration Property<T>(Expression<Func<TStructuralType, T>> propertyExpression) where T : struct
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, optional: false);
        }

        /// <summary>
        /// Adds an optional enum property to the EDM type.
        /// </summary>
        /// <typeparam name="T">The enum property type.</typeparam>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public EnumPropertyConfiguration EnumProperty<T>(Expression<Func<TStructuralType, T?>> propertyExpression) where T : struct
        {
            return GetEnumPropertyConfiguration(propertyExpression, optional: true);
        }

        /// <summary>
        /// Adds a required enum property to the EDM type.
        /// </summary>
        /// <typeparam name="T">The enum property type.</typeparam>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public EnumPropertyConfiguration EnumProperty<T>(Expression<Func<TStructuralType, T>> propertyExpression) where T : struct
        {
            return GetEnumPropertyConfiguration(propertyExpression, optional: false);
        }

        /// <summary>
        /// Adds a complex property to the EDM type.
        /// </summary>
        /// <typeparam name="TComplexType">The complex type.</typeparam>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public ComplexPropertyConfiguration ComplexProperty<TComplexType>(Expression<Func<TStructuralType, TComplexType>> propertyExpression)
        {
            return GetComplexPropertyConfiguration(propertyExpression);
        }

        /// <summary>
        /// Adds a collection property to the EDM type.
        /// </summary>
        /// <typeparam name="TElementType">The element type of the collection.</typeparam>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public CollectionPropertyConfiguration CollectionProperty<TElementType>(Expression<Func<TStructuralType, IEnumerable<TElementType>>> propertyExpression)
        {
            return GetCollectionPropertyConfiguration(propertyExpression);
        }

        /// <summary>
        /// Adds a dynamic property dictionary property.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the dynamic property dictionary for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "More specific expression type is clearer")]
        public void HasDynamicProperties(Expression<Func<TStructuralType, IDictionary<string, object>>> propertyExpression)
        {
            PropertyInfo propertyInfo = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);

            _configuration.AddDynamicPropertyDictionary(propertyInfo);
        }

        /// <summary>
        /// Configures a many relationship from this structural type.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for the relationship. For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasMany<TTargetEntity>(Expression<Func<TStructuralType, IEnumerable<TTargetEntity>>> navigationPropertyExpression) where TTargetEntity : class
        {
            return GetOrCreateNavigationProperty(navigationPropertyExpression, EdmMultiplicity.Many);
        }

        /// <summary>
        /// Configures an optional relationship from this structural type.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for the relationship. For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasOptional<TTargetEntity>(Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression) where TTargetEntity : class
        {
            return GetOrCreateNavigationProperty(navigationPropertyExpression, EdmMultiplicity.ZeroOrOne);
        }

        /// <summary>
        /// Configures an optional relationship with referential constraint from this structural type.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t =&gt; t.Customer</c> and in Visual Basic .NET <c>Function(t) t.Customer</c>.</param>
        /// <param name="referentialConstraintExpression">A lambda expression representing the referential constraint. For example,
        ///  in C# <c>(o, c) =&gt; o.CustomerId == c.Id</c> and in Visual Basic .NET <c>Function(o, c) c.CustomerId == c.Id</c>.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasOptional<TTargetEntity>(
            Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression,
            Expression<Func<TStructuralType, TTargetEntity, bool>> referentialConstraintExpression) where TTargetEntity : class
        {
            NavigationPropertyConfiguration navigation =
                GetOrCreateNavigationProperty(navigationPropertyExpression, EdmMultiplicity.ZeroOrOne);

            IDictionary<PropertyInfo, PropertyInfo> referentialConstraints =
                PropertyPairSelectorVisitor.GetSelectedProperty(referentialConstraintExpression);

            foreach (KeyValuePair<PropertyInfo, PropertyInfo> constraint in referentialConstraints)
            {
                navigation.HasConstraint(constraint);
            }

            return navigation;
        }

        /// <summary>
        /// Configures a required relationship from this structural type.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for the relationship. For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasRequired<TTargetEntity>(Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression) where TTargetEntity : class
        {
            return GetOrCreateNavigationProperty(navigationPropertyExpression, EdmMultiplicity.One);
        }

        /// <summary>
        /// Configures a required relationship with referential constraint from this structural type.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t =&gt; t.Customer</c> and in Visual Basic .NET <c>Function(t) t.Customer</c>.</param>
        /// <param name="referentialConstraintExpression">A lambda expression representing the referential constraint. For example,
        ///  in C# <c>(o, c) =&gt; o.CustomerId == c.Id</c> and in Visual Basic .NET <c>Function(o, c) c.CustomerId == c.Id</c>.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasRequired<TTargetEntity>(
            Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression,
            Expression<Func<TStructuralType, TTargetEntity, bool>> referentialConstraintExpression) where TTargetEntity : class
        {
            NavigationPropertyConfiguration navigation =
                GetOrCreateNavigationProperty(navigationPropertyExpression, EdmMultiplicity.One);

            IDictionary<PropertyInfo, PropertyInfo> referentialConstraints =
                PropertyPairSelectorVisitor.GetSelectedProperty(referentialConstraintExpression);

            foreach (KeyValuePair<PropertyInfo, PropertyInfo> constraint in referentialConstraints)
            {
                navigation.HasConstraint(constraint);
            }

            return navigation;
        }

        /// <summary>
        /// Configures a relationship from this structural type to a contained collection navigation property.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for
        ///  the relationship. For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET
        ///  <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration ContainsMany<TTargetEntity>(
            Expression<Func<TStructuralType, IEnumerable<TTargetEntity>>> navigationPropertyExpression)
            where TTargetEntity : class
        {
            return GetOrCreateContainedNavigationProperty(navigationPropertyExpression, EdmMultiplicity.Many);
        }

        /// <summary>
        /// Configures an optional relationship from this structural type to a single contained navigation property.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for
        ///  the relationship. For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET
        ///  <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration ContainsOptional<TTargetEntity>(
            Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression) where TTargetEntity : class
        {
            return GetOrCreateContainedNavigationProperty(navigationPropertyExpression, EdmMultiplicity.ZeroOrOne);
        }

        /// <summary>
        /// Configures a required relationship from this structural type to a single contained navigation property.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for
        ///  the relationship. For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET
        ///  <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration ContainsRequired<TTargetEntity>(
            Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression) where TTargetEntity : class
        {
            return GetOrCreateContainedNavigationProperty(navigationPropertyExpression, EdmMultiplicity.One);
        }
        internal NavigationPropertyConfiguration GetOrCreateNavigationProperty(Expression navigationPropertyExpression, EdmMultiplicity multiplicity)
        {
            PropertyInfo navigationProperty = PropertySelectorVisitor.GetSelectedProperty(navigationPropertyExpression);
            return _configuration.AddNavigationProperty(navigationProperty, multiplicity);
        }

        internal NavigationPropertyConfiguration GetOrCreateContainedNavigationProperty(Expression navigationPropertyExpression, EdmMultiplicity multiplicity)
        {
            PropertyInfo navigationProperty = PropertySelectorVisitor.GetSelectedProperty(navigationPropertyExpression);
            return _configuration.AddContainedNavigationProperty(navigationProperty, multiplicity);
        }

        private PrimitivePropertyConfiguration GetPrimitivePropertyConfiguration(Expression propertyExpression, bool optional)
        {
            PropertyInfo propertyInfo = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);
            PrimitivePropertyConfiguration property = _configuration.AddProperty(propertyInfo);
            if (optional)
            {
                property.IsOptional();
            }

            return property;
        }

        private EnumPropertyConfiguration GetEnumPropertyConfiguration(Expression propertyExpression, bool optional)
        {
            PropertyInfo propertyInfo = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);

            EnumPropertyConfiguration property = _configuration.AddEnumProperty(propertyInfo);
            if (optional)
            {
                property.IsOptional();
            }

            return property;
        }

        private ComplexPropertyConfiguration GetComplexPropertyConfiguration(Expression propertyExpression, bool optional = false)
        {
            PropertyInfo propertyInfo = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);
            ComplexPropertyConfiguration property = _configuration.AddComplexProperty(propertyInfo);
            if (optional)
            {
                property.IsOptional();
            }
            else
            {
                property.IsRequired();
            }

            return property;
        }

        private CollectionPropertyConfiguration GetCollectionPropertyConfiguration(Expression propertyExpression, bool optional = false)
        {
            PropertyInfo propertyInfo = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);
            CollectionPropertyConfiguration property;

            property = _configuration.AddCollectionProperty(propertyInfo);

            if (optional)
            {
                property.IsOptional();
            }
            else
            {
                property.IsRequired();
            }

            return property;
        }
    }
}
