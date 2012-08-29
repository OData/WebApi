// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
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

        public EntityTypeConfiguration(ODataModelBuilder modelBuilder, Type clrType)
            : base(modelBuilder, clrType)
        {
        }

        public override EdmTypeKind Kind
        {
            get { return EdmTypeKind.Entity; }
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
}
