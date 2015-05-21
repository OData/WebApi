// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder.Conventions.Attributes
{
    // Denotes a property used as a foreign key in a relationship. The annotation may be placed on:
    // 1. the foreign key property and specify the associated navigation property name, or
    // 2. a navigation property and specify the associated foreign key name.
    internal class ForeignKeyAttributeConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public ForeignKeyAttributeConvention()
            : base(attribute => attribute.GetType() == typeof(ForeignKeyAttribute), allowMultiple: false)
        {
        }

        /// <inheritdoc/>
        public override void Apply(PropertyConfiguration edmProperty,
            StructuralTypeConfiguration structuralTypeConfiguration, Attribute attribute,
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

            if (attribute == null)
            {
                throw Error.ArgumentNull("attribute");
            }

            EntityTypeConfiguration declaringEntityType = structuralTypeConfiguration as EntityTypeConfiguration;
            if (declaringEntityType == null)
            {
                return;
            }

            ForeignKeyAttribute foreignKeyAttribute = (ForeignKeyAttribute)attribute;
            switch (edmProperty.Kind)
            {
                case PropertyKind.Navigation:
                    ApplyNavigation((NavigationPropertyConfiguration)edmProperty, declaringEntityType,
                        foreignKeyAttribute);
                    break;
                case PropertyKind.Primitive:
                    ApplyPrimitive((PrimitivePropertyConfiguration)edmProperty, declaringEntityType,
                        foreignKeyAttribute);
                    break;
            }
        }

        private static void ApplyNavigation(NavigationPropertyConfiguration navProperty, EntityTypeConfiguration entityType,
            ForeignKeyAttribute foreignKeyAttribute)
        {
            Contract.Assert(navProperty != null);
            Contract.Assert(entityType != null);
            Contract.Assert(foreignKeyAttribute != null);

            if (navProperty.AddedExplicitly || navProperty.Multiplicity == EdmMultiplicity.Many)
            {
                return;
            }

            EntityTypeConfiguration principalEntity = entityType.ModelBuilder.StructuralTypes
                    .OfType<EntityTypeConfiguration>().FirstOrDefault(e => e.ClrType == navProperty.RelatedClrType);
            if (principalEntity == null)
            {
                return;
            }

            // if a navigation property has multiple foreign keys, use comma to separate the list of foreign key names.
            IEnumerable<string> dependentPropertyNames = foreignKeyAttribute.Name.Split(',').Select(p => p.Trim());
            foreach (string dependentPropertyName in dependentPropertyNames)
            {
                if (String.IsNullOrWhiteSpace(dependentPropertyName))
                {
                    continue;
                }

                PrimitivePropertyConfiguration dependent =
                    entityType.Properties.OfType<PrimitivePropertyConfiguration>()
                        .SingleOrDefault(p => p.Name.Equals(dependentPropertyName, StringComparison.Ordinal));

                if (dependent != null)
                {
                    PrimitivePropertyConfiguration principal = principalEntity.Keys.FirstOrDefault(
                        k => k.PropertyInfo.PropertyType == dependent.PropertyInfo.PropertyType &&
                            navProperty.PrincipalProperties.All(p => p != k.PropertyInfo));

                    if (principal != null)
                    {
                        navProperty.HasConstraint(dependent.PropertyInfo, principal.PropertyInfo);
                    }
                }
            }
        }

        private static void ApplyPrimitive(PrimitivePropertyConfiguration dependent, EntityTypeConfiguration entityType,
            ForeignKeyAttribute foreignKeyAttribute)
        {
            Contract.Assert(dependent != null);
            Contract.Assert(entityType != null);
            Contract.Assert(foreignKeyAttribute != null);

            string navName = foreignKeyAttribute.Name.Trim();
            NavigationPropertyConfiguration navProperty = entityType.NavigationProperties
                .FirstOrDefault(n => n.Name.Equals(navName, StringComparison.Ordinal));
            if (navProperty == null)
            {
                return;
            }

            if (navProperty.Multiplicity == EdmMultiplicity.Many || navProperty.AddedExplicitly)
            {
                return;
            }

            EntityTypeConfiguration principalEntity = entityType.ModelBuilder.StructuralTypes
                .OfType<EntityTypeConfiguration>().FirstOrDefault(e => e.ClrType == navProperty.RelatedClrType);
            if (principalEntity == null)
            {
                return;
            }

            PrimitivePropertyConfiguration principal = principalEntity.Keys.FirstOrDefault(
                k => k.PropertyInfo.PropertyType == dependent.PropertyInfo.PropertyType &&
                    navProperty.PrincipalProperties.All(p => p != k.PropertyInfo));

            if (principal != null)
            {
                navProperty.HasConstraint(dependent.PropertyInfo, principal.PropertyInfo);
            }
        }
    }
}
