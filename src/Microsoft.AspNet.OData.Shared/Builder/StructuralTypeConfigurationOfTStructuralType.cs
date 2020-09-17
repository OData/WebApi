// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Builder
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
        public LengthPropertyConfiguration Property(Expression<Func<TStructuralType, string>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, optional: true) as LengthPropertyConfiguration;
        }

        /// <summary>
        /// Adds a binary property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public LengthPropertyConfiguration Property(Expression<Func<TStructuralType, byte[]>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, optional: true) as LengthPropertyConfiguration;
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
        /// Adds an deciaml primitive property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public DecimalPropertyConfiguration Property(Expression<Func<TStructuralType, decimal?>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, optional: true) as DecimalPropertyConfiguration;
        }

        /// <summary>
        /// Adds an deciaml primitive property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public DecimalPropertyConfiguration Property(Expression<Func<TStructuralType, decimal>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, optional: false) as DecimalPropertyConfiguration;
        }

        /// <summary>
        /// Adds an time-of-day primitive property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrecisionPropertyConfiguration Property(Expression<Func<TStructuralType, TimeOfDay?>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, optional: true) as PrecisionPropertyConfiguration;
        }

        /// <summary>
        /// Adds an time-of-day primitive property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrecisionPropertyConfiguration Property(Expression<Func<TStructuralType, TimeOfDay>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, optional: false) as PrecisionPropertyConfiguration;
        }

        /// <summary>
        /// Adds an duration primitive property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrecisionPropertyConfiguration Property(Expression<Func<TStructuralType, TimeSpan?>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, optional: true) as PrecisionPropertyConfiguration;
        }

        /// <summary>
        /// Adds an duration primitive property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrecisionPropertyConfiguration Property(Expression<Func<TStructuralType, TimeSpan>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, optional: false) as PrecisionPropertyConfiguration;
        }

        /// <summary>
        /// Adds an datetime-with-offset primitive property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrecisionPropertyConfiguration Property(Expression<Func<TStructuralType, DateTimeOffset?>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, optional: true) as PrecisionPropertyConfiguration;
        }

        /// <summary>
        /// Adds an datetime-with-offset primitive property to the EDM type.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object that can be used to further configure the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrecisionPropertyConfiguration Property(Expression<Func<TStructuralType, DateTimeOffset>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, optional: false) as PrecisionPropertyConfiguration;
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
        /// Adds an InstanceAnnotation container property.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression representing the instance annotation container property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "More specific expression type is clearer")]
        public void HasInstanceAnnotations(Expression<Func<TStructuralType, IODataInstanceAnnotationContainer>> propertyExpression)
        {
            PropertyInfo propertyInfo = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);

            _configuration.AddInstanceAnnotationContainer(propertyInfo);
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
            return HasNavigationProperty(
                navigationPropertyExpression,
                referentialConstraintExpression,
                EdmMultiplicity.ZeroOrOne,
                null);
        }

        /// <summary>
        /// Configures an optional relationship with referential constraint from this structural type.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t =&gt; t.Customer</c> and in Visual Basic .NET <c>Function(t) t.Customer</c>.</param>
        /// <param name="referentialConstraintExpression">A lambda expression representing the referential constraint. For example,
        ///  in C# <c>(o, c) =&gt; o.CustomerId == c.Id</c> and in Visual Basic .NET <c>Function(o, c) c.CustomerId == c.Id</c>.</param>
        /// <param name="partnerExpression">The partner expression for this relationship.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasOptional<TTargetEntity>(
            Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression,
            Expression<Func<TStructuralType, TTargetEntity, bool>> referentialConstraintExpression,
            Expression<Func<TTargetEntity, IEnumerable<TStructuralType>>> partnerExpression) where TTargetEntity : class
        {
            return HasNavigationProperty(
                navigationPropertyExpression,
                referentialConstraintExpression,
                EdmMultiplicity.ZeroOrOne,
                partnerExpression);
        }

        /// <summary>
        /// Configures an optional relationship with referential constraint from this structural type.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t =&gt; t.Customer</c> and in Visual Basic .NET <c>Function(t) t.Customer</c>.</param>
        /// <param name="referentialConstraintExpression">A lambda expression representing the referential constraint. For example,
        ///  in C# <c>(o, c) =&gt; o.CustomerId == c.Id</c> and in Visual Basic .NET <c>Function(o, c) c.CustomerId == c.Id</c>.</param>
        /// <param name="partnerExpression">The partner expression for this relationship.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasOptional<TTargetEntity>(
            Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression,
            Expression<Func<TStructuralType, TTargetEntity, bool>> referentialConstraintExpression,
            Expression<Func<TTargetEntity, TStructuralType>> partnerExpression) where TTargetEntity : class
        {
            return HasNavigationProperty(
                navigationPropertyExpression,
                referentialConstraintExpression,
                EdmMultiplicity.ZeroOrOne,
                partnerExpression);
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
        /// <param name="partnerExpression">The partner expression for this relationship.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasRequired<TTargetEntity>(
            Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression,
            Expression<Func<TStructuralType, TTargetEntity, bool>> referentialConstraintExpression,
            Expression<Func<TTargetEntity, IEnumerable<TStructuralType>>> partnerExpression) where TTargetEntity : class
        {
            return HasNavigationProperty(navigationPropertyExpression, referentialConstraintExpression, EdmMultiplicity.One, partnerExpression);
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
            return HasNavigationProperty(navigationPropertyExpression, referentialConstraintExpression, EdmMultiplicity.One, null);
        }

        /// <summary>
        /// Configures a required relationship with referential constraint from this structural type.
        /// </summary>
        /// <typeparam name="TTargetEntity">The type of the entity at the other end of the relationship.</typeparam>
        /// <param name="navigationPropertyExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t =&gt; t.Customer</c> and in Visual Basic .NET <c>Function(t) t.Customer</c>.</param>
        /// <param name="referentialConstraintExpression">A lambda expression representing the referential constraint. For example,
        ///  in C# <c>(o, c) =&gt; o.CustomerId == c.Id</c> and in Visual Basic .NET <c>Function(o, c) c.CustomerId == c.Id</c>.</param>
        /// <param name="partnerExpression"></param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasRequired<TTargetEntity>(
            Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression,
            Expression<Func<TStructuralType, TTargetEntity, bool>> referentialConstraintExpression,
            Expression<Func<TTargetEntity, TStructuralType>> partnerExpression) where TTargetEntity : class
        {
            return HasNavigationProperty(navigationPropertyExpression, referentialConstraintExpression, EdmMultiplicity.One, partnerExpression);
        }

        private NavigationPropertyConfiguration HasNavigationProperty<TTargetEntity>(Expression<Func<TStructuralType, TTargetEntity>> navigationPropertyExpression,
            Expression<Func<TStructuralType, TTargetEntity, bool>> referentialConstraintExpression, EdmMultiplicity multiplicity, Expression partnerProperty)
            where TTargetEntity : class
        {
            NavigationPropertyConfiguration navigation =
                GetOrCreateNavigationProperty(navigationPropertyExpression, multiplicity);

            IDictionary<PropertyInfo, PropertyInfo> referentialConstraints =
                PropertyPairSelectorVisitor.GetSelectedProperty(referentialConstraintExpression);

            foreach (KeyValuePair<PropertyInfo, PropertyInfo> constraint in referentialConstraints)
            {
                navigation.HasConstraint(constraint);
            }

            if (partnerProperty != null)
            {
                var partnerPropertyInfo = PropertySelectorVisitor.GetSelectedProperty(partnerProperty);
                if (typeof(IEnumerable).IsAssignableFrom(partnerPropertyInfo.PropertyType))
                {
                    _configuration.ModelBuilder
                        .EntityType<TTargetEntity>().HasMany((Expression<Func<TTargetEntity, IEnumerable<TStructuralType>>>)partnerProperty);
                }
                else
                {
                    _configuration.ModelBuilder
                        .EntityType<TTargetEntity>().HasRequired((Expression<Func<TTargetEntity, TStructuralType>>)partnerProperty);
                }
                var prop = _configuration.ModelBuilder
                        .EntityType<TTargetEntity>()
                        .Properties
                        .First(p => p.Name == partnerPropertyInfo.Name)
                    as NavigationPropertyConfiguration;

                navigation.Partner = prop;
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

        /// <summary>
        /// Sets this property is countable of this structural type.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Count()
        {
            _configuration.QueryConfiguration.SetCount(true);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets whether this property is countable of this structural type.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Count(QueryOptionSetting setting)
        {
            _configuration.QueryConfiguration.SetCount(setting == QueryOptionSetting.Allowed);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets sortable properties depends on <see cref="QueryOptionSetting"/> of this structural type.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> OrderBy(QueryOptionSetting setting, params string[] properties)
        {
            _configuration.QueryConfiguration.SetOrderBy(properties, setting == QueryOptionSetting.Allowed);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets sortable properties of this structural type.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> OrderBy(params string[] properties)
        {
            _configuration.QueryConfiguration.SetOrderBy(properties, true);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets whether all properties of this structural type is sortable.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> OrderBy(QueryOptionSetting setting)
        {
            _configuration.QueryConfiguration.SetOrderBy(null, setting == QueryOptionSetting.Allowed);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets all properties of this structural type is sortable.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> OrderBy()
        {
            _configuration.QueryConfiguration.SetOrderBy(null, true);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets filterable properties depends on <see cref="QueryOptionSetting"/> of this structural type.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Filter(QueryOptionSetting setting, params string[] properties)
        {
            _configuration.QueryConfiguration.SetFilter(properties, setting == QueryOptionSetting.Allowed);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets filterable properties of this structural type.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Filter(params string[] properties)
        {
            _configuration.QueryConfiguration.SetFilter(properties, true);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets whether all properties of this structural type is filterable.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Filter(QueryOptionSetting setting)
        {
            _configuration.QueryConfiguration.SetFilter(null, setting == QueryOptionSetting.Allowed);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets all properties of this structural type is filterable.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Filter()
        {
            _configuration.QueryConfiguration.SetFilter(null, true);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets selectable properties depends on <see cref="SelectExpandType"/> of this structural type.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Select(SelectExpandType selectType,
            params string[] properties)
        {
            _configuration.QueryConfiguration.SetSelect(properties, selectType);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets selectable properties of this structural type.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Select(params string[] properties)
        {
            _configuration.QueryConfiguration.SetSelect(properties, SelectExpandType.Allowed);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets <see cref="SelectExpandType"/> of all properties of this structural type is selectable.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Select(SelectExpandType selectType)
        {
            _configuration.QueryConfiguration.SetSelect(null, selectType);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets all properties of this structural type is selectable.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Select()
        {
            _configuration.QueryConfiguration.SetSelect(null, SelectExpandType.Allowed);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets the max value of $top of this structural type that a client can request
        /// and the maximum number of query results of this entity type to return.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Page(int? maxTopValue, int? pageSizeValue)
        {
            _configuration.QueryConfiguration.SetMaxTop(maxTopValue);
            _configuration.QueryConfiguration.SetPageSize(pageSizeValue);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets the properties of this structural type enable paging.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Page()
        {
            _configuration.QueryConfiguration.SetMaxTop(null);
            _configuration.QueryConfiguration.SetPageSize(null);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets the maximum depth of expand result,
        /// expandable properties and their <see cref="SelectExpandType"/> of this structural type.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Expand(int maxDepth, SelectExpandType expandType, params string[] properties)
        {
            _configuration.QueryConfiguration.SetExpand(properties, maxDepth, expandType);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets the expandable properties of this structural type.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Expand(params string[] properties)
        {
            _configuration.QueryConfiguration.SetExpand(properties, null, SelectExpandType.Allowed);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets the maximum depth of expand result,
        /// expandable properties of this structural type.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Expand(int maxDepth, params string[] properties)
        {
            _configuration.QueryConfiguration.SetExpand(properties, maxDepth, SelectExpandType.Allowed);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets the expandable properties and their <see cref="SelectExpandType"/> of this structural type.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Expand(SelectExpandType expandType, params string[] properties)
        {
            _configuration.QueryConfiguration.SetExpand(properties, null, expandType);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets <see cref="SelectExpandType"/> of all properties with maximum depth of expand result of this structural type.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Expand(SelectExpandType expandType, int maxDepth)
        {
            _configuration.QueryConfiguration.SetExpand(null, maxDepth, expandType);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets all properties expandable with maximum depth of expand result of this structural type.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Expand(int maxDepth)
        {
            _configuration.QueryConfiguration.SetExpand(null, maxDepth, SelectExpandType.Allowed);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets <see cref="SelectExpandType"/> of all properties of this structural type.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Expand(SelectExpandType expandType)
        {
            _configuration.QueryConfiguration.SetExpand(null, null, expandType);
            _configuration.AddedExplicitly = true;
            return this;
        }

        /// <summary>
        /// Sets all properties expandable of this structural type.
        /// </summary>
        public StructuralTypeConfiguration<TStructuralType> Expand()
        {
            _configuration.QueryConfiguration.SetExpand(null, null, SelectExpandType.Allowed);
            _configuration.AddedExplicitly = true;
            return this;
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
