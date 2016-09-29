// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="IEdmNavigationSource"/> that can be built using <see cref="ODataModelBuilder"/>.
    /// <typeparam name="TEntityType">The entity type of the navigation source.</typeparam>
    /// </summary>
    public abstract class NavigationSourceConfiguration<TEntityType> where TEntityType : class
    {
        private readonly NavigationSourceConfiguration _configuration;
        private readonly EntityTypeConfiguration<TEntityType> _entityType;
        private readonly ODataModelBuilder _modelBuilder;
        private readonly BindingPathConfiguration<TEntityType> _binding;

        internal NavigationSourceConfiguration(ODataModelBuilder modelBuilder, NavigationSourceConfiguration configuration)
        {
            if (modelBuilder == null)
            {
                throw Error.ArgumentNull("modelBuilder");
            }

            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            _configuration = configuration;
            _modelBuilder = modelBuilder;
            _entityType = new EntityTypeConfiguration<TEntityType>(modelBuilder, _configuration.EntityType);
            _binding = new BindingPathConfiguration<TEntityType>(modelBuilder, _entityType, _configuration);
        }

        /// <summary>
        /// Gets the entity type contained in this navigation source configuration.
        /// </summary>
        public EntityTypeConfiguration<TEntityType> EntityType
        {
            get
            {
                return _entityType;
            }
        }

        internal NavigationSourceConfiguration Configuration
        {
            get { return _configuration; }
        }

        /// <summary>
        /// Gets a binding path configuration through which you can configure
        /// binding paths for the navigation property of this navigation source.
        /// </summary>
        public BindingPathConfiguration<TEntityType> Binding
        {
            get
            {
                return _binding;
            }
        }

        /// <summary>
        /// Configures an one-to-many relationship from this entity type and binds the corresponding navigation property to
        /// the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <typeparam name="TDerivedEntityType">The target entity type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="entitySetName">The target navigation source (entity set) name for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasManyBinding<TTargetType, TDerivedEntityType>(
            Expression<Func<TDerivedEntityType, IEnumerable<TTargetType>>> navigationExpression, string entitySetName)
            where TTargetType : class
            where TDerivedEntityType : class, TEntityType
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (String.IsNullOrEmpty(entitySetName))
            {
                throw Error.ArgumentNullOrEmpty("entitySetName");
            }

            EntityTypeConfiguration<TDerivedEntityType> derivedEntityType =
                _modelBuilder.EntityType<TDerivedEntityType>().DerivesFrom<TEntityType>();

            NavigationPropertyConfiguration navigation = derivedEntityType.HasMany(navigationExpression);

            IList<object> bindingPath = new List<object>
            {
                typeof(TDerivedEntityType),
                navigation.PropertyInfo
            };

            return this.Configuration.AddBinding(navigation,
                _modelBuilder.EntitySet<TTargetType>(entitySetName)._configuration, bindingPath);
        }

        /// <summary>
        /// Configures an one-to-many relationship from this entity type and binds the corresponding navigation property to
        /// the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="entitySetName">The target navigation source (entity set) name for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasManyBinding<TTargetType>(
            Expression<Func<TEntityType, IEnumerable<TTargetType>>> navigationExpression, string entitySetName)
            where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (String.IsNullOrEmpty(entitySetName))
            {
                throw Error.ArgumentNullOrEmpty("entitySetName");
            }

            return this.Configuration.AddBinding(EntityType.HasMany(navigationExpression),
                _modelBuilder.EntitySet<TTargetType>(entitySetName)._configuration);
        }

        /// <summary>
        /// Configures an one-to-many relationship from this entity type and binds the corresponding navigation property to
        /// the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetEntitySet">The target navigation source (entity set) for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasManyBinding<TTargetType>(
            Expression<Func<TEntityType, IEnumerable<TTargetType>>> navigationExpression,
            NavigationSourceConfiguration<TTargetType> targetEntitySet) where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (targetEntitySet == null)
            {
                throw Error.ArgumentNull("targetEntitySet");
            }

            return this.Configuration.AddBinding(EntityType.HasMany(navigationExpression), targetEntitySet.Configuration);
        }

        /// <summary>
        /// Configures an one-to-many relationship from this entity type and binds the corresponding navigation property to
        /// the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <typeparam name="TDerivedEntityType">The target entity type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetEntitySet">The target navigation source (entity set) for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasManyBinding<TTargetType, TDerivedEntityType>(
            Expression<Func<TDerivedEntityType, IEnumerable<TTargetType>>> navigationExpression,
            NavigationSourceConfiguration<TTargetType> targetEntitySet)
            where TTargetType : class
            where TDerivedEntityType : class, TEntityType
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (targetEntitySet == null)
            {
                throw Error.ArgumentNull("targetEntitySet");
            }

            EntityTypeConfiguration<TDerivedEntityType> derivedEntityType =
                _modelBuilder.EntityType<TDerivedEntityType>().DerivesFrom<TEntityType>();

            NavigationPropertyConfiguration navigation = derivedEntityType.HasMany(navigationExpression);
            IList<object> bindingPath = new List<object>
            {
                typeof(TDerivedEntityType),
                navigation.PropertyInfo
            };

            return this.Configuration.AddBinding(navigation, targetEntitySet.Configuration, bindingPath);
        }

        /// <summary>
        /// Configures a required relationship from this entity type and binds the corresponding navigation property to
        /// the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="entitySetName">The target navigation source (entity set) name for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasRequiredBinding<TTargetType>(
            Expression<Func<TEntityType, TTargetType>> navigationExpression, string entitySetName)
            where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (String.IsNullOrEmpty(entitySetName))
            {
                throw Error.ArgumentNullOrEmpty("entitySetName");
            }

            return this.Configuration.AddBinding(EntityType.HasRequired(navigationExpression),
                _modelBuilder.EntitySet<TTargetType>(entitySetName).Configuration);
        }

        /// <summary>
        /// Configures a required relationship from this entity type and binds the corresponding navigation property to
        /// the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <typeparam name="TDerivedEntityType">The target entity type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="entitySetName">The target navigation source (entity set) name for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasRequiredBinding<TTargetType, TDerivedEntityType>(
            Expression<Func<TDerivedEntityType, TTargetType>> navigationExpression, string entitySetName)
            where TTargetType : class
            where TDerivedEntityType : class, TEntityType
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (String.IsNullOrEmpty(entitySetName))
            {
                throw Error.ArgumentNullOrEmpty("entitySetName");
            }

            EntityTypeConfiguration<TDerivedEntityType> derivedEntityType =
                _modelBuilder.EntityType<TDerivedEntityType>().DerivesFrom<TEntityType>();

            NavigationPropertyConfiguration navigation = derivedEntityType.HasRequired(navigationExpression);
            IList<object> bindingPath = new List<object>
            {
                typeof(TDerivedEntityType),
                navigation.PropertyInfo
            };

            return this.Configuration.AddBinding(navigation,
                _modelBuilder.EntitySet<TTargetType>(entitySetName).Configuration, bindingPath);
        }

        /// <summary>
        /// Configures a required relationship from this entity type and binds the corresponding navigation property to
        /// the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetEntitySet">The target navigation source (entity set) for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasRequiredBinding<TTargetType>(
            Expression<Func<TEntityType, TTargetType>> navigationExpression,
            NavigationSourceConfiguration<TTargetType> targetEntitySet) where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (targetEntitySet == null)
            {
                throw Error.ArgumentNull("targetEntitySet");
            }

            return this.Configuration.AddBinding(EntityType.HasRequired(navigationExpression), targetEntitySet.Configuration);
        }

        /// <summary>
        /// Configures a required relationship from this entity type and binds the corresponding navigation property to
        /// the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <typeparam name="TDerivedEntityType">The target entity type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetEntitySet">The target navigation source (entity set) for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasRequiredBinding<TTargetType, TDerivedEntityType>(
            Expression<Func<TDerivedEntityType, TTargetType>> navigationExpression,
            NavigationSourceConfiguration<TTargetType> targetEntitySet)
            where TTargetType : class
            where TDerivedEntityType : class, TEntityType
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (targetEntitySet == null)
            {
                throw Error.ArgumentNull("targetEntitySet");
            }

            EntityTypeConfiguration<TDerivedEntityType> derivedEntityType =
                _modelBuilder.EntityType<TDerivedEntityType>().DerivesFrom<TEntityType>();

            NavigationPropertyConfiguration navigation = derivedEntityType.HasRequired(navigationExpression);
            IList<object> bindingPath = new List<object>
            {
                typeof(TDerivedEntityType),
                navigation.PropertyInfo
            };

            return this.Configuration.AddBinding(navigation, targetEntitySet.Configuration, bindingPath);
        }

        /// <summary>
        /// Configures an optional relationship from this entity type and binds the corresponding navigation property to
        /// the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="entitySetName">The target navigation source (entity set) name for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasOptionalBinding<TTargetType>(
            Expression<Func<TEntityType, TTargetType>> navigationExpression, string entitySetName)
            where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (String.IsNullOrEmpty(entitySetName))
            {
                throw Error.ArgumentNullOrEmpty("entitySetName");
            }

            return this.Configuration.AddBinding(EntityType.HasOptional(navigationExpression),
                _modelBuilder.EntitySet<TTargetType>(entitySetName).Configuration);
        }

        /// <summary>
        /// Configures an optional relationship from this entity type and binds the corresponding navigation property to
        /// the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <typeparam name="TDerivedEntityType">The target entity type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="entitySetName">The target navigation source (entity set) name for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasOptionalBinding<TTargetType, TDerivedEntityType>(
            Expression<Func<TDerivedEntityType, TTargetType>> navigationExpression, string entitySetName)
            where TTargetType : class
            where TDerivedEntityType : class, TEntityType
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (String.IsNullOrEmpty(entitySetName))
            {
                throw Error.ArgumentNullOrEmpty("entitySetName");
            }

            EntityTypeConfiguration<TDerivedEntityType> derivedEntityType =
                _modelBuilder.EntityType<TDerivedEntityType>().DerivesFrom<TEntityType>();

            NavigationPropertyConfiguration navigation = derivedEntityType.HasOptional(navigationExpression);
            IList<object> bindingPath = new List<object>
            {
                typeof(TDerivedEntityType),
                navigation.PropertyInfo
            };

            return this.Configuration.AddBinding(navigation,
                _modelBuilder.EntitySet<TTargetType>(entitySetName).Configuration, bindingPath);
        }

        /// <summary>
        /// Configures an optional relationship from this entity type and binds the corresponding navigation property to
        /// the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetEntitySet">The target navigation source (entity set) for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasOptionalBinding<TTargetType>(
            Expression<Func<TEntityType, TTargetType>> navigationExpression,
            NavigationSourceConfiguration<TTargetType> targetEntitySet) where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (targetEntitySet == null)
            {
                throw Error.ArgumentNull("targetEntitySet");
            }

            return this.Configuration.AddBinding(EntityType.HasOptional(navigationExpression), targetEntitySet.Configuration);
        }

        /// <summary>
        /// Configures an optional relationship from this entity type and binds the corresponding navigation property to
        /// the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <typeparam name="TDerivedEntityType">The target entity type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetEntitySet">The target navigation source (entity set) for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasOptionalBinding<TTargetType, TDerivedEntityType>(
            Expression<Func<TDerivedEntityType, TTargetType>> navigationExpression,
            NavigationSourceConfiguration<TTargetType> targetEntitySet)
            where TTargetType : class
            where TDerivedEntityType : class, TEntityType
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (targetEntitySet == null)
            {
                throw Error.ArgumentNull("targetEntitySet");
            }

            EntityTypeConfiguration<TDerivedEntityType> derivedEntityType =
                _modelBuilder.EntityType<TDerivedEntityType>().DerivesFrom<TEntityType>();

            NavigationPropertyConfiguration navigation = derivedEntityType.HasOptional(navigationExpression);
            IList<object> bindingPath = new List<object>
            {
                typeof(TDerivedEntityType),
                navigation.PropertyInfo
            };

            return this.Configuration.AddBinding(navigation, targetEntitySet.Configuration, bindingPath);
        }

        /// <summary>
        /// Configures a required relationship from this entity type and binds the corresponding navigation property to
        /// the given singleton.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="singletonName">The target navigation source (singleton) name for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasSingletonBinding<TTargetType>(
            Expression<Func<TEntityType, TTargetType>> navigationExpression, string singletonName)
            where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (String.IsNullOrEmpty(singletonName))
            {
                throw Error.ArgumentNullOrEmpty("singletonName");
            }

            return this.Configuration.AddBinding(EntityType.HasRequired(navigationExpression),
                _modelBuilder.Singleton<TTargetType>(singletonName).Configuration);
        }

        /// <summary>
        /// Configures a required relationship from this entity type and binds the corresponding navigation property to
        /// the given singleton.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <typeparam name="TDerivedEntityType">The target entity type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="singletonName">The target navigation source (singleton) name for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasSingletonBinding<TTargetType, TDerivedEntityType>(
            Expression<Func<TDerivedEntityType, TTargetType>> navigationExpression, string singletonName)
            where TTargetType : class
            where TDerivedEntityType : class, TEntityType
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (String.IsNullOrEmpty(singletonName))
            {
                throw Error.ArgumentNullOrEmpty("singletonName");
            }

            EntityTypeConfiguration<TDerivedEntityType> derivedEntityType =
                _modelBuilder.EntityType<TDerivedEntityType>().DerivesFrom<TEntityType>();

            NavigationPropertyConfiguration navigation = derivedEntityType.HasRequired(navigationExpression);
            IList<object> bindingPath = new List<object>
            {
                typeof(TDerivedEntityType),
                navigation.PropertyInfo
            };

            return this.Configuration.AddBinding(navigation,
                _modelBuilder.Singleton<TTargetType>(singletonName).Configuration, bindingPath);
        }

        /// <summary>
        /// Configures a required relationship from this entity type and binds the corresponding navigation property to
        /// the given singleton.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetSingleton">The target navigation source (singleton) for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasSingletonBinding<TTargetType>(
            Expression<Func<TEntityType, TTargetType>> navigationExpression,
            NavigationSourceConfiguration<TTargetType> targetSingleton) where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (targetSingleton == null)
            {
                throw Error.ArgumentNull("targetSingleton");
            }

            return this.Configuration.AddBinding(EntityType.HasRequired(navigationExpression), targetSingleton.Configuration);
        }

        /// <summary>
        /// Configures a required relationship from this entity type and binds the corresponding navigation property to
        /// the given singleton.
        /// </summary>
        /// <typeparam name="TTargetType">The target navigation source type.</typeparam>
        /// <typeparam name="TDerivedEntityType">The target entity type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetSingleton">The target singleton for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasSingletonBinding<TTargetType, TDerivedEntityType>(
            Expression<Func<TDerivedEntityType, TTargetType>> navigationExpression,
            NavigationSourceConfiguration<TTargetType> targetSingleton)
            where TTargetType : class
            where TDerivedEntityType : class, TEntityType
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (targetSingleton == null)
            {
                throw Error.ArgumentNull("targetSingleton");
            }

            EntityTypeConfiguration<TDerivedEntityType> derivedEntityType =
                _modelBuilder.EntityType<TDerivedEntityType>().DerivesFrom<TEntityType>();

            NavigationPropertyConfiguration navigation = derivedEntityType.HasRequired(navigationExpression);
            IList<object> bindingPath = new List<object>
            {
                typeof(TDerivedEntityType),
                navigation.PropertyInfo
            };

            return this.Configuration.AddBinding(navigation, targetSingleton.Configuration, bindingPath);
        }

        /// <summary>
        /// Configures the edit link for the entities from this navigation source.
        /// </summary>
        /// <param name="editLinkFactory">The factory used to generate the edit link.</param>
        /// <param name="followsConventions"><c>true</c> if the factory follows OData edit link conventions;
        /// otherwise, <c>false</c>.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public void HasEditLink(Func<ResourceContext<TEntityType>, Uri> editLinkFactory, bool followsConventions)
        {
            if (editLinkFactory == null)
            {
                throw Error.ArgumentNull("editLinkFactory");
            }

            _configuration.HasEditLink(new SelfLinkBuilder<Uri>((context) => editLinkFactory(UpCastEntityContext(context)), followsConventions));
        }

        /// <summary>
        /// Configures the read link for the entities from this navigation source.
        /// </summary>
        /// <param name="readLinkFactory">The factory used to generate the read link.</param>
        /// <param name="followsConventions"><c>true</c> if the factory follows OData read link conventions;
        /// otherwise, <c>false</c>.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public void HasReadLink(Func<ResourceContext<TEntityType>, Uri> readLinkFactory, bool followsConventions)
        {
            if (readLinkFactory == null)
            {
                throw Error.ArgumentNull("readLinkFactory");
            }

            _configuration.HasReadLink(new SelfLinkBuilder<Uri>((context) =>
                readLinkFactory(UpCastEntityContext(context)), followsConventions));
        }

        /// <summary>
        /// Configures the ID link for the entities from this navigation source.
        /// </summary>
        /// <param name="idLinkFactory">The factory used to generate the ID link.</param>
        /// <param name="followsConventions"><c>true</c> if the factory follows OData ID link conventions;
        /// otherwise, <c>false</c>.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public void HasIdLink(Func<ResourceContext<TEntityType>, Uri> idLinkFactory, bool followsConventions)
        {
            if (idLinkFactory == null)
            {
                throw Error.ArgumentNull("idLinkFactory");
            }

            _configuration.HasIdLink(new SelfLinkBuilder<Uri>((context) =>
                idLinkFactory(UpCastEntityContext(context)), followsConventions));
        }

        /// <summary>
        /// Configures the navigation link for the given navigation property for entities from this navigation source.
        /// </summary>
        /// <param name="navigationProperty">The navigation property for which the navigation link is being generated.</param>
        /// <param name="navigationLinkFactory">The factory used to generate the navigation link.</param>
        /// <param name="followsConventions"><c>true</c> if the factory follows OData navigation link conventions;
        /// otherwise, <c>false</c>.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public void HasNavigationPropertyLink(NavigationPropertyConfiguration navigationProperty,
            Func<ResourceContext<TEntityType>, IEdmNavigationProperty, Uri> navigationLinkFactory, bool followsConventions)
        {
            if (navigationProperty == null)
            {
                throw Error.ArgumentNull("navigationProperty");
            }

            if (navigationLinkFactory == null)
            {
                throw Error.ArgumentNull("navigationLinkFactory");
            }

            _configuration.HasNavigationPropertyLink(navigationProperty, new NavigationLinkBuilder((context, property) =>
                navigationLinkFactory(UpCastEntityContext(context), property), followsConventions));
        }

        /// <summary>
        /// Configures the navigation link for the given navigation properties for entities from this navigation source.
        /// </summary>
        /// <param name="navigationProperties">The navigation properties for which the navigation link is being generated.</param>
        /// <param name="navigationLinkFactory">The factory used to generate the navigation link.</param>
        /// <param name="followsConventions"><c>true</c> if the factory follows OData navigation link conventions;
        /// otherwise, <c>false</c>.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Nested generic appropriate here")]
        public void HasNavigationPropertiesLink(IEnumerable<NavigationPropertyConfiguration> navigationProperties,
            Func<ResourceContext<TEntityType>, IEdmNavigationProperty, Uri> navigationLinkFactory, bool followsConventions)
        {
            if (navigationProperties == null)
            {
                throw Error.ArgumentNull("navigationProperties");
            }

            if (navigationLinkFactory == null)
            {
                throw Error.ArgumentNull("navigationLinkFactory");
            }

            _configuration.HasNavigationPropertiesLink(navigationProperties, new NavigationLinkBuilder((entity, property) =>
                navigationLinkFactory(UpCastEntityContext(entity), property), followsConventions));
        }

        /// <summary>
        /// Finds the bindings <see cref="NavigationPropertyBindingConfiguration"/> for the navigation property with the given name.
        /// </summary>
        /// <param name="propertyName">The name of the navigation property.</param>
        /// <returns>The bindings, if found; otherwise, empty.</returns>
        public IEnumerable<NavigationPropertyBindingConfiguration> FindBindings(string propertyName)
        {
            return _configuration.FindBindings(propertyName);
        }

        /// <summary>
        /// Finds the bindings <see cref="NavigationPropertyBindingConfiguration"/> for the given navigation property
        /// </summary>
        /// <param name="navigationConfiguration">The navigation property.</param>
        /// <returns>The bindings if found</returns>
        public IEnumerable<NavigationPropertyBindingConfiguration> FindBinding(NavigationPropertyConfiguration navigationConfiguration)
        {
            return _configuration.FindBinding(navigationConfiguration);
        }

        /// <summary>
        /// Finds the <see cref="NavigationPropertyBindingConfiguration"/> for the given navigation property.
        /// </summary>
        /// <param name="navigationConfiguration">The navigation property.</param>
        /// <param name="bindingPath">The navigation binding path.</param>
        /// <returns>The binding if found.</returns>
        public NavigationPropertyBindingConfiguration FindBinding(NavigationPropertyConfiguration navigationConfiguration,
            IList<object> bindingPath)
        {
            return _configuration.FindBinding(navigationConfiguration, bindingPath);
        }

        private static ResourceContext<TEntityType> UpCastEntityContext(ResourceContext context)
        {
            return new ResourceContext<TEntityType>
            {
                SerializerContext = context.SerializerContext,
                EdmObject = context.EdmObject,
                StructuredType = context.StructuredType
            };
        }
    }
}
