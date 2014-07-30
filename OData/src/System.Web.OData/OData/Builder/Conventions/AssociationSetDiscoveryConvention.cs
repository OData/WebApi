// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.OData.Properties;

namespace System.Web.OData.Builder.Conventions
{
    /// <summary>
    /// <see cref="INavigationSourceConvention"/> to configure the EDM association sets for the given entity set.
    /// <remarks>This convention adds an association set for each EDM navigation property defined in this type, its base types and all its derived types.
    /// The target navigation source chosen is the default navigation source for the navigation property's target entity type.
    /// The default navigation source for an entity type is the navigation source that contains entity of that entity type. 
    /// If more than one navigation source match, the default navigation source is none.
    /// If no navigation sources match the default navigation source is the default navigation source of the base type.</remarks>
    /// </summary>
    internal class AssociationSetDiscoveryConvention : INavigationSourceConvention
    {
        public void Apply(INavigationSourceConfiguration configuration, ODataModelBuilder model)
        {
            foreach (EntityTypeConfiguration entity in model.ThisAndBaseAndDerivedTypes(configuration.EntityType))
            {
                foreach (NavigationPropertyConfiguration navigationProperty in entity.NavigationProperties)
                {
                    NavigationSourceConfiguration targetNavigationSource = GetTargetNavigationSource(navigationProperty, model);
                    if (targetNavigationSource != null)
                    {
                        configuration.AddBinding(navigationProperty, targetNavigationSource);
                    }
                }
            }
        }

        // Get the default target navigation source for this navigation property.
        internal static NavigationSourceConfiguration GetTargetNavigationSource(NavigationPropertyConfiguration navigationProperty,
            ODataModelBuilder model)
        {
            EntityTypeConfiguration targetEntityType =
                model
                .StructuralTypes
                .OfType<EntityTypeConfiguration>()
                .Where(e => e.ClrType == navigationProperty.RelatedClrType).SingleOrDefault();

            if (targetEntityType == null)
            {
                throw Error.InvalidOperation(SRResources.TargetEntityTypeMissing, navigationProperty.Name,
                    navigationProperty.PropertyInfo.ReflectedType.FullName);
            }

            bool hasSingletonAttribute = navigationProperty.PropertyInfo.GetCustomAttributes<SingletonAttribute>().Any();

            return GetDefaultNavigationSource(targetEntityType, model, hasSingletonAttribute);
        }

        private static NavigationSourceConfiguration GetDefaultNavigationSource(EntityTypeConfiguration targetEntityType,
            ODataModelBuilder model, bool isSingleton)
        {
            if (targetEntityType == null)
            {
                return null;
            }

            NavigationSourceConfiguration[] matchingNavigationSources = null;
            if (isSingleton)
            {
                matchingNavigationSources = model.Singletons.Where(e => e.EntityType == targetEntityType).ToArray();
            }
            else
            {
                matchingNavigationSources = model.EntitySets.Where(e => e.EntityType == targetEntityType).ToArray();
            }

            if (matchingNavigationSources.Length > 1)
            {
                return null;
            }
            else if (matchingNavigationSources.Length == 1)
            {
                return matchingNavigationSources[0];
            }
            else
            {
                // default navigation source is the same as the default navigation source for the base type.
                return GetDefaultNavigationSource(targetEntityType.BaseType, model, isSingleton);
            }
        }
    }
}
