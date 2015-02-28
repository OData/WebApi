// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder.Conventions
{
    // Denotes a property convention used to discover foreign key properties if there is no any foreign key configured
    // on the navigatin property.
    // The basic rule to discover the foreign key is: with the same property type and follow up the naming convention.
    // The naming convention is:
    //   1) "Principal class name + prinicpal key name" equals the dependent property name
    //      For example: Customer (Id) <--> Order (CustomerId)
    //   2) Principal key name equals the dependent property name and Principal key name case insensitive equals "Principal class name + Id".
    //      For example: Customer (CustomerId) <--> Order (CustomerId)
    internal class ForeignKeyDiscoveryConvention : IEdmPropertyConvention<NavigationPropertyConfiguration>
    {
        public void Apply(PropertyConfiguration edmProperty, StructuralTypeConfiguration structuralTypeConfiguration)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            NavigationPropertyConfiguration navigationProperty = edmProperty as NavigationPropertyConfiguration;
            if (navigationProperty != null)
            {
                Apply(navigationProperty, structuralTypeConfiguration);
            }
        }

        public void Apply(NavigationPropertyConfiguration edmProperty, StructuralTypeConfiguration structuralTypeConfiguration)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            if (structuralTypeConfiguration == null)
            {
                throw Error.ArgumentNull("structuralTypeConfiguration");
            }

            // Suppress the foreign key discovery convention for the following scenarios.
            if (edmProperty.AddedExplicitly ||
                edmProperty.ReferentialConstraint.Any() ||
                edmProperty.Multiplicity == EdmMultiplicity.Many)
            {
                return;
            }

            ODataModelBuilder builder = structuralTypeConfiguration.ModelBuilder;
            if (builder == null)
            {
                return;
            }

            EntityTypeConfiguration principalEntityType = builder.StructuralTypes.OfType<EntityTypeConfiguration>()
                .FirstOrDefault(e => e.ClrType == edmProperty.RelatedClrType);
            if (principalEntityType == null)
            {
                return;
            }

            EntityTypeConfiguration dependentEntityType = (EntityTypeConfiguration)structuralTypeConfiguration;
            Contract.Assert(dependentEntityType != null);

            IDictionary<PrimitivePropertyConfiguration, PrimitivePropertyConfiguration> typeNameForeignKeys =
                GetForeignKeys(principalEntityType, dependentEntityType);

            if (typeNameForeignKeys.Any() && typeNameForeignKeys.Count == principalEntityType.Keys.Count())
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
