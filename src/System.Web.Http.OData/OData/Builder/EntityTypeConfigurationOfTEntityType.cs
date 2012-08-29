// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    public class EntityTypeConfiguration<TEntityType> : StructuralTypeConfiguration<TEntityType> where TEntityType : class
    {
        private IEntityTypeConfiguration _configuration;
        private EntityCollectionConfiguration<TEntityType> _collection;

        public EntityTypeConfiguration(ODataModelBuilder modelBuilder)
            : this(new EntityTypeConfiguration(modelBuilder, typeof(TEntityType)))
        {
        }

        public EntityTypeConfiguration(IEntityTypeConfiguration configuration)
            : base(configuration)
        {
            _configuration = configuration;
            _collection = new EntityCollectionConfiguration<TEntityType>(configuration);
        }

        public IEnumerable<NavigationPropertyConfiguration> NavigationProperties
        {
            get { return _configuration.NavigationProperties; }
        }

        /// <summary>
        /// Used to access a Collection of Entities throw which you can configure
        /// actions that are bindable to EntityCollections.
        /// </summary>
        public EntityCollectionConfiguration<TEntityType> Collection
        {
            get { return _collection; }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Explicit Expression generic type is more clear")]
        public EntityTypeConfiguration<TEntityType> HasKey<TKey>(Expression<Func<TEntityType, TKey>> keyDefinitionExpression)
        {
            ICollection<PropertyInfo> properties = PropertySelectorVisitor.GetSelectedProperties(keyDefinitionExpression);
            foreach (PropertyInfo property in properties)
            {
                _configuration.HasKey(property);
            }
            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasMany<TTargetEntity>(Expression<Func<TEntityType, ICollection<TTargetEntity>>> navigationPropertyExpression) where TTargetEntity : class
        {
            return GetOrCreateNavigationProperty(navigationPropertyExpression, EdmMultiplicity.Many);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasOptional<TTargetEntity>(Expression<Func<TEntityType, TTargetEntity>> navigationPropertyExpression) where TTargetEntity : class
        {
            return GetOrCreateNavigationProperty(navigationPropertyExpression, EdmMultiplicity.ZeroOrOne);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Explicit Expression generic type is more clear")]
        public NavigationPropertyConfiguration HasRequired<TTargetEntity>(Expression<Func<TEntityType, TTargetEntity>> navigationPropertyExpression) where TTargetEntity : class
        {
            return GetOrCreateNavigationProperty(navigationPropertyExpression, EdmMultiplicity.One);
        }

        /// <summary>
        /// Create an Action that binds to this EntityType.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <returns>The ActionConfiguration to allow further configuration of the new Action.</returns>
        public ActionConfiguration Action(string name)
        {
            ActionConfiguration action = new ActionConfiguration(_configuration.ModelBuilder, name);
            action.SetBindingParameter(BindingParameterConfiguration.DefaultBindingParameterName, _configuration);
            return action;
        }

        internal NavigationPropertyConfiguration GetOrCreateNavigationProperty(Expression navigationPropertyExpression, EdmMultiplicity multiplicity)
        {
            PropertyInfo navigationProperty = PropertySelectorVisitor.GetSelectedProperty(navigationPropertyExpression);
            return _configuration.AddNavigationProperty(navigationProperty, multiplicity);
        }
    }
}
