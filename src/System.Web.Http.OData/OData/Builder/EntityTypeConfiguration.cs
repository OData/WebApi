// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    // TODO: support inheritance
    // TODO: add support for FK properties
    // CUT: support for bi-directional properties
    public class EntityTypeConfiguration : StructuralTypeConfiguration, IEntityTypeConfiguration
    {
        private List<PrimitivePropertyConfiguration> _keys = new List<PrimitivePropertyConfiguration>();

        internal EntityTypeConfiguration(ODataModelBuilder modelBuilder, Type clrType)
            : base(modelBuilder, clrType)
        {
        }

        public override StructuralTypeKind Kind
        {
            get { return StructuralTypeKind.EntityType; }
        }

        public IEnumerable<NavigationPropertyConfiguration> NavigationProperties
        {
            get { return ExplicitProperties.Values.OfType<NavigationPropertyConfiguration>(); }
        }

        public IEnumerable<PrimitivePropertyConfiguration> Keys
        {
            get
            {
                return _keys;
            }
        }

        public IEntityTypeConfiguration BaseType
        {
            get { return null; }
        }

        public IEntityTypeConfiguration HasKey(PropertyInfo keyProperty)
        {
            PrimitivePropertyConfiguration propertyConfig = AddProperty(keyProperty);

            // keys are always required
            propertyConfig.IsRequired();

            if (!_keys.Contains(propertyConfig))
            {
                _keys.Add(propertyConfig);
            }

            return this;
        }

        public NavigationPropertyConfiguration AddNavigationProperty(PropertyInfo navigationProperty, EdmMultiplicity multiplicity)
        {
            if (navigationProperty == null)
            {
                throw Error.ArgumentNull("navigationProperty");
            }

            if (!navigationProperty.ReflectedType.IsAssignableFrom(ClrType))
            {
                throw Error.InvalidOperation(SRResources.PropertyDoesNotBelongToType, navigationProperty.Name, ClrType.FullName);
            }

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
                navigationPropertyConfig = new NavigationPropertyConfiguration(navigationProperty, multiplicity);
                ExplicitProperties[navigationProperty] = navigationPropertyConfig;
                // make sure the related type is configured
                ModelBuilder.AddEntity(navigationPropertyConfig.RelatedClrType);
            }
            return navigationPropertyConfig;
        }
    }

    public class EntityTypeConfiguration<TEntityType> : StructuralTypeConfiguration<TEntityType> where TEntityType : class
    {
        private IEntityTypeConfiguration _configuration;

        public EntityTypeConfiguration(ODataModelBuilder modelBuilder)
            : this(new EntityTypeConfiguration(modelBuilder, typeof(TEntityType)))
        {
        }

        public EntityTypeConfiguration(IEntityTypeConfiguration configuration)
            : base(configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<NavigationPropertyConfiguration> NavigationProperties
        {
            get { return _configuration.NavigationProperties; }
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

        internal NavigationPropertyConfiguration GetOrCreateNavigationProperty(Expression navigationPropertyExpression, EdmMultiplicity multiplicity)
        {
            PropertyInfo navigationProperty = PropertySelectorVisitor.GetSelectedProperty(navigationPropertyExpression);
            return _configuration.AddNavigationProperty(navigationProperty, multiplicity);
        }
    }
}
