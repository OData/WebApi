//-----------------------------------------------------------------------------
// <copyright file="BindingPathConfigurationOfTStructuralType.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Represents the configuration for the binding path that can be built using <see cref="ODataModelBuilder"/>.
    /// <typeparam name="TStructuralType">The structural type of the current binding path property.</typeparam>
    /// </summary>
    public class BindingPathConfiguration<TStructuralType> where TStructuralType : class
    {
        private readonly NavigationSourceConfiguration _navigationSource;
        private readonly StructuralTypeConfiguration<TStructuralType> _structuralType;
        private readonly ODataModelBuilder _modelBuilder;
        private readonly IList<MemberInfo> _bindingPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingPathConfiguration{TStructuralType}"/> class.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        /// <param name="structuralType">The type configuration.</param>
        /// <param name="navigationSource">The navigation source configuration.</param>
        public BindingPathConfiguration(ODataModelBuilder modelBuilder,
            StructuralTypeConfiguration<TStructuralType> structuralType,
            NavigationSourceConfiguration navigationSource)
            : this(modelBuilder, structuralType, navigationSource, new List<MemberInfo>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingPathConfiguration{TStructuralType}"/> class.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        /// <param name="structuralType">The type configuration.</param>
        /// <param name="navigationSource">The navigation source configuration.</param>
        /// <param name="bindingPath">The binding path.</param>
        public BindingPathConfiguration(ODataModelBuilder modelBuilder,
            StructuralTypeConfiguration<TStructuralType> structuralType,
            NavigationSourceConfiguration navigationSource,
            IList<MemberInfo> bindingPath)
        {
            if (modelBuilder == null)
            {
                throw Error.ArgumentNull("modelBuilder");
            }

            if (structuralType == null)
            {
                throw Error.ArgumentNull("structuralType");
            }

            if (navigationSource == null)
            {
                throw Error.ArgumentNull("navigationSource");
            }

            if (bindingPath == null)
            {
                throw Error.ArgumentNull("bindingPath");
            }

            _modelBuilder = modelBuilder;
            _navigationSource = navigationSource;
            _structuralType = structuralType;
            _bindingPath = bindingPath;
        }

        /// <summary>
        /// Gets the list of binding path information.
        /// </summary>
        public IList<MemberInfo> Path
        {
            get { return _bindingPath; }
        }

        /// <summary>
        /// Gets the string of binding path information. like "A.B/C/D.E".
        /// </summary>
        public string BindingPath
        {
            get { return _bindingPath.ConvertBindingPath(); }
        }

        /// <summary>
        /// Configures an one-to-many path for this binding path.
        /// </summary>
        /// <typeparam name="TTargetType">The target property type.</typeparam>
        /// <param name="pathExpression">A lambda expression representing the binding path property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object <see cref="BindingPathConfiguration{TStructuralType}"/>
        /// that can be used to further configure the binding path or end the binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public BindingPathConfiguration<TTargetType> HasManyPath<TTargetType>(
            Expression<Func<TStructuralType, IEnumerable<TTargetType>>> pathExpression)
            where TTargetType : class
        {
            return HasManyPath<TTargetType>(pathExpression, contained: false);
        }

        /// <summary>
        /// Configures an one-to-many path for this binding path.
        /// </summary>
        /// <typeparam name="TTargetType">The target property type.</typeparam>
        /// <param name="pathExpression">A lambda expression representing the binding path property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="contained">A flag representing the target property as containment.</param>
        /// <returns>A configuration object <see cref="BindingPathConfiguration{TStructuralType}"/>
        /// that can be used to further configure the binding path or end the binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public BindingPathConfiguration<TTargetType> HasManyPath<TTargetType>(
            Expression<Func<TStructuralType, IEnumerable<TTargetType>>> pathExpression,
            bool contained)
            where TTargetType : class
        {
            if (pathExpression == null)
            {
                throw Error.ArgumentNull("pathExpression");
            }

            PropertyInfo pathProperty = PropertySelectorVisitor.GetSelectedProperty(pathExpression);

            IList<MemberInfo> bindingPath = new List<MemberInfo>(_bindingPath);
            bindingPath.Add(pathProperty);

            StructuralTypeConfiguration<TTargetType> target;
            if (contained)
            {
                target = _modelBuilder.EntityType<TTargetType>();
                _structuralType.ContainsMany(pathExpression); // add a containment navigation property
            }
            else
            {
                target = _modelBuilder.ComplexType<TTargetType>();
                _structuralType.CollectionProperty(pathExpression); // add a collection complex property
            }

            return new BindingPathConfiguration<TTargetType>(_modelBuilder, target, _navigationSource, bindingPath);
        }

        /// <summary>
        /// Configures an one-to-many path of the derived type for this binding path.
        /// </summary>
        /// <typeparam name="TTargetType">The target property type.</typeparam>
        /// <typeparam name="TDerivedType">The derived structural type.</typeparam>
        /// <param name="pathExpression">A lambda expression representing the binding path property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object <see cref="BindingPathConfiguration{TStructuralType}"/> 
        /// that can be used to further configure the binding path or end the binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public BindingPathConfiguration<TTargetType> HasManyPath<TTargetType, TDerivedType>(
            Expression<Func<TDerivedType, IEnumerable<TTargetType>>> pathExpression)
            where TTargetType : class
            where TDerivedType : class, TStructuralType
        {
            return HasManyPath(pathExpression, contained: false);
        }

        /// <summary>
        /// Configures an one-to-many path of the derived type for this binding path.
        /// </summary>
        /// <typeparam name="TTargetType">The target property type.</typeparam>
        /// <typeparam name="TDerivedType">The derived structural type.</typeparam>
        /// <param name="pathExpression">A lambda expression representing the binding path property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="contained">A flag representing the target property as containment.</param>
        /// <returns>A configuration object <see cref="BindingPathConfiguration{TStructuralType}"/> 
        /// that can be used to further configure the binding path or end the binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public BindingPathConfiguration<TTargetType> HasManyPath<TTargetType, TDerivedType>(
            Expression<Func<TDerivedType, IEnumerable<TTargetType>>> pathExpression,
            bool contained)
            where TTargetType : class
            where TDerivedType : class, TStructuralType
        {
            if (pathExpression == null)
            {
                throw Error.ArgumentNull("pathExpression");
            }

            PropertyInfo pathProperty = PropertySelectorVisitor.GetSelectedProperty(pathExpression);

            IList<MemberInfo> bindingPath = new List<MemberInfo>(_bindingPath);
            bindingPath.Add(TypeHelper.AsMemberInfo(typeof(TDerivedType)));
            bindingPath.Add(pathProperty);

            // make sure the derived type has the same type kind with the resource type.
            StructuralTypeConfiguration<TDerivedType> derivedConfiguration;
            if (_structuralType.Configuration.Kind == EdmTypeKind.Entity)
            {
                derivedConfiguration = _modelBuilder.EntityType<TDerivedType>().DerivesFrom<TStructuralType>();
            }
            else
            {
                derivedConfiguration = _modelBuilder.ComplexType<TDerivedType>().DerivesFrom<TStructuralType>();
            }

            StructuralTypeConfiguration<TTargetType> target;
            if (contained)
            {
                target = _modelBuilder.EntityType<TTargetType>();
                derivedConfiguration.ContainsMany(pathExpression); // add a containment navigation property
            }
            else
            {
                target = _modelBuilder.ComplexType<TTargetType>();
                derivedConfiguration.CollectionProperty(pathExpression); // add a collection complex property
            }

            return new BindingPathConfiguration<TTargetType>(_modelBuilder, target, _navigationSource, bindingPath);
        }

        /// <summary>
        /// Configures an one-to-one path for this binding path.
        /// </summary>
        /// <typeparam name="TTargetType">The target property type.</typeparam>
        /// <param name="pathExpression">A lambda expression representing the binding path property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object <see cref="BindingPathConfiguration{TStructuralType}"/>
        /// that can be used to further configure the binding path or end the binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public BindingPathConfiguration<TTargetType> HasSinglePath<TTargetType>(
            Expression<Func<TStructuralType, TTargetType>> pathExpression)
            where TTargetType : class
        {
            return HasSinglePath<TTargetType>(pathExpression, required: false, contained: false);
        }

        /// <summary>
        /// Configures an one-to-one path for this binding path.
        /// </summary>
        /// <typeparam name="TTargetType">The target property type.</typeparam>
        /// <param name="pathExpression">A lambda expression representing the binding path property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="required">A flag representing the target property required or optional.</param>
        /// <param name="contained">A flag representing the target property as containment.</param>
        /// <returns>A configuration object <see cref="BindingPathConfiguration{TStructuralType}"/>
        /// that can be used to further configure the binding path or end the binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public BindingPathConfiguration<TTargetType> HasSinglePath<TTargetType>(
            Expression<Func<TStructuralType, TTargetType>> pathExpression,
            bool required,
            bool contained)
            where TTargetType : class
        {
            if (pathExpression == null)
            {
                throw Error.ArgumentNull("pathExpression");
            }

            PropertyInfo pathProperty = PropertySelectorVisitor.GetSelectedProperty(pathExpression);

            IList<MemberInfo> bindingPath = new List<MemberInfo>(_bindingPath);
            bindingPath.Add(pathProperty);

            StructuralTypeConfiguration<TTargetType> target;
            if (contained)
            {
                target = _modelBuilder.EntityType<TTargetType>();

                if (required)
                {
                    _structuralType.ContainsRequired(pathExpression);
                }
                else
                {
                    _structuralType.ContainsOptional(pathExpression);
                }
            }
            else
            {
                target = _modelBuilder.ComplexType<TTargetType>();
                _structuralType.ComplexProperty(pathExpression).OptionalProperty = !required;
            }

            return new BindingPathConfiguration<TTargetType>(_modelBuilder, target, _navigationSource, bindingPath);
        }

        /// <summary>
        /// Configures a required one-to-one path of the derived type for this binding path.
        /// </summary>
        /// <typeparam name="TTargetType">The target property type.</typeparam>
        /// <typeparam name="TDerivedType">The derived structural type.</typeparam>
        /// <param name="pathExpression">A lambda expression representing the binding path property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <returns>A configuration object <see cref="BindingPathConfiguration{TStructuralType}"/> 
        /// that can be used to further configure the binding path or end the binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public BindingPathConfiguration<TTargetType> HasSinglePath<TTargetType, TDerivedType>(
            Expression<Func<TDerivedType, TTargetType>> pathExpression)
            where TTargetType : class
            where TDerivedType : class, TStructuralType
        {
            return HasSinglePath(pathExpression, required: false, contained: false);
        }

        /// <summary>
        /// Configures a required one-to-one path of the derived type for this binding path.
        /// </summary>
        /// <typeparam name="TTargetType">The target property type.</typeparam>
        /// <typeparam name="TDerivedType">The derived structural type.</typeparam>
        /// <param name="pathExpression">A lambda expression representing the binding path property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="required">A flag representing the target property required or optional.</param>
        /// <param name="contained">A flag representing the target property as containment.</param>
        /// <returns>A configuration object <see cref="BindingPathConfiguration{TStructuralType}"/> 
        /// that can be used to further configure the binding path or end the binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public BindingPathConfiguration<TTargetType> HasSinglePath<TTargetType, TDerivedType>(
            Expression<Func<TDerivedType, TTargetType>> pathExpression,
            bool required,
            bool contained)
            where TTargetType : class
            where TDerivedType : class, TStructuralType
        {
            if (pathExpression == null)
            {
                throw Error.ArgumentNull("pathExpression");
            }

            PropertyInfo pathProperty = PropertySelectorVisitor.GetSelectedProperty(pathExpression);

            IList<MemberInfo> bindingPath = new List<MemberInfo>(_bindingPath);
            bindingPath.Add(TypeHelper.AsMemberInfo(typeof(TDerivedType)));
            bindingPath.Add(pathProperty);

            // make sure the derived type has the same type kind with the resource type.
            StructuralTypeConfiguration<TDerivedType> derivedConfiguration;
            if (_structuralType.Configuration.Kind == EdmTypeKind.Entity)
            {
                derivedConfiguration = _modelBuilder.EntityType<TDerivedType>().DerivesFrom<TStructuralType>();
            }
            else
            {
                derivedConfiguration = _modelBuilder.ComplexType<TDerivedType>().DerivesFrom<TStructuralType>();
            }

            StructuralTypeConfiguration<TTargetType> target;
            if (contained)
            {
                target = _modelBuilder.EntityType<TTargetType>();

                if (required)
                {
                    derivedConfiguration.ContainsRequired(pathExpression);
                }
                else
                {
                    derivedConfiguration.ContainsOptional(pathExpression);
                }
            }
            else
            {
                target = _modelBuilder.ComplexType<TTargetType>();
                derivedConfiguration.ComplexProperty(pathExpression).OptionalProperty = !required;
            }

            return new BindingPathConfiguration<TTargetType>(_modelBuilder, target, _navigationSource, bindingPath);
        }

        /// <summary>
        /// Configures an one-to-many path for this binding path and binds the corresponding navigation property to
        /// the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target property type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the binding path property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetEntitySet">The target navigation source (entity set) for the binding.</param>
        /// <returns>A configuration object <see cref="NavigationPropertyBindingConfiguration"/>
        /// that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasManyBinding<TTargetType>(
            Expression<Func<TStructuralType, IEnumerable<TTargetType>>> navigationExpression,
            string targetEntitySet)
            where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (String.IsNullOrEmpty(targetEntitySet))
            {
                throw Error.ArgumentNullOrEmpty("targetEntitySet");
            }

            NavigationPropertyConfiguration navigation = _structuralType.HasMany(navigationExpression);

            IList<MemberInfo> bindingPath = new List<MemberInfo>(_bindingPath);
            bindingPath.Add(navigation.PropertyInfo);

            NavigationSourceConfiguration entitySet = _modelBuilder.EntitySet<TTargetType>(targetEntitySet).Configuration;
            return this._navigationSource.AddBinding(navigation, entitySet, bindingPath);
        }

        /// <summary>
        /// Configures an one-to-many path of the derived type for this binding path and binds the corresponding
        /// navigation property to the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target property type.</typeparam>
        /// <typeparam name="TDerivedType">The derived structural type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the binding path property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetEntitySet">The target navigation source (entity set) for the binding.</param>
        /// <returns>A configuration object <see cref="NavigationPropertyBindingConfiguration"/>
        /// that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasManyBinding<TTargetType, TDerivedType>(
            Expression<Func<TDerivedType, IEnumerable<TTargetType>>> navigationExpression,
            string targetEntitySet)
            where TTargetType : class
            where TDerivedType : class, TStructuralType
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (String.IsNullOrEmpty(targetEntitySet))
            {
                throw Error.ArgumentNullOrEmpty("targetEntitySet");
            }

            StructuralTypeConfiguration<TDerivedType> derivedConfiguration;
            if (this._structuralType.Configuration.Kind == EdmTypeKind.Entity)
            {
                derivedConfiguration = _modelBuilder.EntityType<TDerivedType>().DerivesFrom<TStructuralType>();
            }
            else
            {
                derivedConfiguration = _modelBuilder.ComplexType<TDerivedType>().DerivesFrom<TStructuralType>();
            }

            NavigationPropertyConfiguration navigation = derivedConfiguration.HasMany(navigationExpression);

            IList<MemberInfo> bindingPath = new List<MemberInfo>(_bindingPath);
            bindingPath.Add(TypeHelper.AsMemberInfo(typeof(TDerivedType)));
            bindingPath.Add(navigation.PropertyInfo);

            NavigationSourceConfiguration entitySet = _modelBuilder.EntitySet<TTargetType>(targetEntitySet).Configuration;

            return _navigationSource.AddBinding(navigation, entitySet, bindingPath);
        }

        /// <summary>
        /// Configures a required one-to-one path for this binding path and binds the corresponding navigation property to
        /// the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetEntitySet">The target navigation source (entity set) name for the binding.</param>
        /// <returns>A configuration object <see cref="NavigationPropertyBindingConfiguration"/>
        /// that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasRequiredBinding<TTargetType>(
            Expression<Func<TStructuralType, TTargetType>> navigationExpression,
            string targetEntitySet)
            where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (String.IsNullOrEmpty(targetEntitySet))
            {
                throw Error.ArgumentNullOrEmpty("targetEntitySet");
            }

            NavigationPropertyConfiguration navigation = this._structuralType.HasRequired(navigationExpression);

            IList<MemberInfo> bindingPath = new List<MemberInfo>(_bindingPath);
            bindingPath.Add(navigation.PropertyInfo);

            NavigationSourceConfiguration entitySet = _modelBuilder.EntitySet<TTargetType>(targetEntitySet).Configuration;
            return this._navigationSource.AddBinding(navigation, entitySet, bindingPath);
        }

        /// <summary>
        /// Configures a required one-to-one path of the derived type for this binding path and binds the corresponding
        /// navigation property to the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <typeparam name="TDerivedType">The derived structural type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetEntitySet">The target navigation source (entity set) name for the binding.</param>
        /// <returns>A configuration object <see cref="NavigationPropertyBindingConfiguration"/>
        /// that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasRequiredBinding<TTargetType, TDerivedType>(
            Expression<Func<TDerivedType, TTargetType>> navigationExpression,
            string targetEntitySet)
            where TTargetType : class
            where TDerivedType : class, TStructuralType
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (String.IsNullOrEmpty(targetEntitySet))
            {
                throw Error.ArgumentNullOrEmpty("targetEntitySet");
            }

            StructuralTypeConfiguration<TDerivedType> derivedConfiguration;
            if (this._structuralType.Configuration.Kind == EdmTypeKind.Entity)
            {
                derivedConfiguration = _modelBuilder.EntityType<TDerivedType>().DerivesFrom<TStructuralType>();
            }
            else
            {
                derivedConfiguration = _modelBuilder.ComplexType<TDerivedType>().DerivesFrom<TStructuralType>();
            }

            NavigationPropertyConfiguration navigation = derivedConfiguration.HasRequired(navigationExpression);

            IList<MemberInfo> bindingPath = new List<MemberInfo>(_bindingPath);
            bindingPath.Add(TypeHelper.AsMemberInfo(typeof(TDerivedType)));
            bindingPath.Add(navigation.PropertyInfo);

            NavigationSourceConfiguration entitySet = _modelBuilder.EntitySet<TTargetType>(targetEntitySet).Configuration;
            return this._navigationSource.AddBinding(navigation, entitySet, bindingPath);
        }

        /// <summary>
        /// Configures an optional one-to-one path for this binding path and binds the corresponding navigation property to
        /// the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetEntitySet">The target navigation source (entity set) name for the binding.</param>
        /// <returns>A configuration object <see cref="NavigationPropertyBindingConfiguration"/>
        /// that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasOptionalBinding<TTargetType>(
            Expression<Func<TStructuralType, TTargetType>> navigationExpression,
            string targetEntitySet)
            where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (String.IsNullOrEmpty(targetEntitySet))
            {
                throw Error.ArgumentNullOrEmpty("targetEntitySet");
            }

            NavigationPropertyConfiguration navigation = this._structuralType.HasOptional(navigationExpression);

            IList<MemberInfo> bindingPath = new List<MemberInfo>(_bindingPath);
            bindingPath.Add(navigation.PropertyInfo);

            NavigationSourceConfiguration entitySet = _modelBuilder.EntitySet<TTargetType>(targetEntitySet).Configuration;
            return this._navigationSource.AddBinding(navigation, entitySet, bindingPath);
        }

        /// <summary>
        /// Configures an one-to-one path of the derived type for this binding path and binds the corresponding
        /// navigation property to the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <typeparam name="TDerivedType">The derived structural type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetEntitySet">The target navigation source (entity set) name for the binding.</param>
        /// <returns>A configuration object <see cref="NavigationPropertyBindingConfiguration"/>
        /// that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasOptionalBinding<TTargetType, TDerivedType>(
            Expression<Func<TDerivedType, TTargetType>> navigationExpression,
            string targetEntitySet)
            where TTargetType : class
            where TDerivedType : class, TStructuralType
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (String.IsNullOrEmpty(targetEntitySet))
            {
                throw Error.ArgumentNullOrEmpty("targetEntitySet");
            }

            StructuralTypeConfiguration<TDerivedType> derivedConfiguration;
            if (this._structuralType.Configuration.Kind == EdmTypeKind.Entity)
            {
                derivedConfiguration = _modelBuilder.EntityType<TDerivedType>().DerivesFrom<TStructuralType>();
            }
            else
            {
                derivedConfiguration = _modelBuilder.ComplexType<TDerivedType>().DerivesFrom<TStructuralType>();
            }

            NavigationPropertyConfiguration navigation = derivedConfiguration.HasOptional(navigationExpression);

            IList<MemberInfo> bindingPath = new List<MemberInfo>(_bindingPath);
            bindingPath.Add(TypeHelper.AsMemberInfo(typeof(TDerivedType)));
            bindingPath.Add(navigation.PropertyInfo);

            NavigationSourceConfiguration entitySet = _modelBuilder.EntitySet<TTargetType>(targetEntitySet).Configuration;
            return this._navigationSource.AddBinding(navigation, entitySet, bindingPath);
        }
    }
}
