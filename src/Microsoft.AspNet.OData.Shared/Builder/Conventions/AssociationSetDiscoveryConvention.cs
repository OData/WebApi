//-----------------------------------------------------------------------------
// <copyright file="AssociationSetDiscoveryConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Builder.Conventions
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
        public void Apply(NavigationSourceConfiguration configuration, ODataModelBuilder model)
        {
            IList<Tuple<StructuralTypeConfiguration, IList<MemberInfo>, NavigationPropertyConfiguration>> navigations =
                new List<Tuple<StructuralTypeConfiguration, IList<MemberInfo>, NavigationPropertyConfiguration>>();
            Stack<MemberInfo> path = new Stack<MemberInfo>();
            model.FindAllNavigationProperties(configuration.EntityType, navigations, path);
            foreach (var navigation in navigations)
            {
                NavigationSourceConfiguration targetNavigationSource = GetTargetNavigationSource(
                       navigation.Item3, model);
                if (targetNavigationSource != null)
                {
                    configuration.AddBinding(navigation.Item3, targetNavigationSource, navigation.Item2);
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
                    .OfType<EntityTypeConfiguration>().SingleOrDefault(e => e.ClrType == navigationProperty.RelatedClrType);

            if (targetEntityType == null)
            {
                throw Error.InvalidOperation(SRResources.TargetEntityTypeMissing, navigationProperty.Name,
                    TypeHelper.GetReflectedType(navigationProperty.PropertyInfo).FullName);
            }

            bool hasSingletonAttribute = navigationProperty.PropertyInfo.GetCustomAttributes<SingletonAttribute>().Any();

            return GetDefaultNavigationSource(targetEntityType, model, hasSingletonAttribute);
        }

        private static NavigationSourceConfiguration GetDefaultNavigationSource(
            EntityTypeConfiguration targetEntityType,
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
                if (model.BindingOptions == NavigationPropertyBindingOption.Auto)
                {
                    return matchingNavigationSources[0];
                }

                return null;
            }
            else if (matchingNavigationSources.Length == 1)
            {
                return matchingNavigationSources[0];
            }
            else
            {
                // default navigation source is the same as the default navigation source for the base type.
                return GetDefaultNavigationSource(targetEntityType.BaseType as EntityTypeConfiguration,
                    model, isSingleton);
            }
        }
    }
}
