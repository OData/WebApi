// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder.Conventions
{
    // Denotes a property convention used to discover foreign key properties if there is no any foreign key configured
    // on the navigation property.
    // The basic rule to discover the foreign key is: with the same property type and follow up the naming convention. 
    // The naming convention is:
    //   1) "Principal class name + principal key name" equals the dependent property name
    //      For example: Customer (Id) <--> Order (CustomerId)
    //   2) or "Principal key name" equals the dependent property name.
    //      For example: Customer (CustomerId) <--> Order (CustomerId)
    internal class ForeignKeyDiscoveryConvention : IEdmPropertyConvention<NavigationPropertyConfiguration>
    {
        public void Apply(PropertyConfiguration edmProperty, StructuralTypeConfiguration structuralTypeConfiguration,
            ODataConventionModelBuilder model)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            NavigationPropertyConfiguration navigationProperty = edmProperty as NavigationPropertyConfiguration;
            if (navigationProperty != null)
            {
                Apply(navigationProperty, structuralTypeConfiguration, model);
            }
        }

        public void Apply(NavigationPropertyConfiguration edmProperty, StructuralTypeConfiguration structuralTypeConfiguration,
            ODataConventionModelBuilder model)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            if (structuralTypeConfiguration == null)
            {
                throw Error.ArgumentNull("structuralTypeConfiguration");
            }

            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            EntityTypeConfiguration principalEntityType = model.StructuralTypes.OfType<EntityTypeConfiguration>()
                .FirstOrDefault(e => e.ClrType == edmProperty.RelatedClrType);
            if (principalEntityType == null)
            {
                return;
            }

            // Suppress the foreign key discovery convention for the following scenarios.
            if (edmProperty.DependentProperties.Any() || edmProperty.Multiplicity == EdmMultiplicity.Many)
            {
                return;
            }

            EntityTypeConfiguration dependentEntityType = structuralTypeConfiguration as EntityTypeConfiguration;
            if (dependentEntityType == null)
            {
                return;
            }

            IDictionary<PrimitivePropertyConfiguration, PrimitivePropertyConfiguration> typeNameForeignKeys =
                GetForeignKeys(principalEntityType, dependentEntityType);

            if (typeNameForeignKeys.Any() && typeNameForeignKeys.Count() == principalEntityType.Keys.Count())
            {
                foreach (KeyValuePair<PrimitivePropertyConfiguration, PrimitivePropertyConfiguration> foreignKey
                    in typeNameForeignKeys)
                {
                    edmProperty.HasConstraint(foreignKey.Key.PropertyInfo, foreignKey.Value.PropertyInfo);
                }
            }
        }

        private static IDictionary<PrimitivePropertyConfiguration, PrimitivePropertyConfiguration> GetForeignKeys(
            EntityTypeConfiguration principalEntityType,
            EntityTypeConfiguration dependentEntityType)
        {
            IDictionary<PrimitivePropertyConfiguration, PrimitivePropertyConfiguration> typeNameForeignKeys =
                new Dictionary<PrimitivePropertyConfiguration, PrimitivePropertyConfiguration>();

            foreach (PrimitivePropertyConfiguration principalKey in principalEntityType.Keys)
            {
                foreach (PrimitivePropertyConfiguration dependentProperty in
                    dependentEntityType.Properties.OfType<PrimitivePropertyConfiguration>())
                {
                    if (dependentProperty.PropertyInfo.PropertyType == principalKey.PropertyInfo.PropertyType)
                    {
                        if (String.Equals(dependentProperty.Name, principalEntityType.Name + principalKey.Name,
                            StringComparison.Ordinal))
                        {
                            // Customer (Id)  <--> Order (CustomerId)
                            typeNameForeignKeys.Add(dependentProperty, principalKey);
                        }
                        else if (String.Equals(dependentProperty.Name, principalKey.Name, StringComparison.Ordinal) &&
                            String.Equals(principalKey.Name, principalEntityType.Name + "Id", StringComparison.OrdinalIgnoreCase))
                        {
                            // Customer (CustomerId)  <--> Order (CustomerId)
                            typeNameForeignKeys.Add(dependentProperty, principalKey);
                        }
                    }
                }
            }

            return typeNameForeignKeys;
        }
    }
}
