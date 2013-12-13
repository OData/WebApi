// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.OData.Properties;

namespace System.Web.Http.OData.Builder.Conventions
{
    /// <summary>
    /// <see cref="IEntitySetConvention"/> to configure the EDM association sets for the given entity set.
    /// <remarks>This convention adds an association set for each EDM navigation property defined in this type, its base types and all its derived types.
    /// The target entity set chosen is the default entity set for the navigation property's target entity type.
    /// The default entity set for an entity type is the entity set that contains entries of that entity type. If more than one entity sets match, the default entity set is none.
    /// If no entity sets match the default entity set is the default entity set of the base type.</remarks>
    /// </summary>
    internal class AssociationSetDiscoveryConvention : IEntitySetConvention
    {
        public void Apply(EntitySetConfiguration configuration, ODataModelBuilder model)
        {
            foreach (EntityTypeConfiguration entity in model.ThisAndBaseAndDerivedTypes(configuration.EntityType))
            {
                foreach (NavigationPropertyConfiguration navigationProperty in entity.NavigationProperties)
                {
                    EntitySetConfiguration targetEntitySet = GetTargetEntitySet(navigationProperty, model);
                    if (targetEntitySet != null)
                    {
                        configuration.AddBinding(navigationProperty, targetEntitySet);
                    }
                }
            }
        }

        // Get the default target entity set for this navigation property.
        internal static EntitySetConfiguration GetTargetEntitySet(NavigationPropertyConfiguration navigationProperty, ODataModelBuilder model)
        {
            EntityTypeConfiguration targetEntityType =
                model
                .StructuralTypes
                .OfType<EntityTypeConfiguration>()
                .Where(e => e.ClrType == navigationProperty.RelatedClrType).SingleOrDefault();

            if (targetEntityType == null)
            {
                throw Error.InvalidOperation(SRResources.TargetEntityTypeMissing, navigationProperty.Name, navigationProperty.PropertyInfo.ReflectedType.FullName);
            }

            return GetDefaultEntitySet(targetEntityType, model);
        }

        private static EntitySetConfiguration GetDefaultEntitySet(EntityTypeConfiguration targetEntityType, ODataModelBuilder model)
        {
            if (targetEntityType == null)
            {
                return null;
            }

            IEnumerable<EntitySetConfiguration> matchingEntitySets = model.EntitySets.Where(e => e.EntityType == targetEntityType);

            if (matchingEntitySets.Count() > 1)
            {
                // no default entity set if more than one entity set match.
                return null;
            }
            else if (matchingEntitySets.Count() == 1)
            {
                return matchingEntitySets.Single();
            }
            else
            {
                // default entity set is the same as the default entity set for the base type.
                return GetDefaultEntitySet(targetEntityType.BaseType, model);
            }
        }
    }
}
