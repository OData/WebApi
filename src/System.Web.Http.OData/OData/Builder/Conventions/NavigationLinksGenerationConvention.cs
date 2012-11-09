// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder.Conventions
{
    public class NavigationLinksGenerationConvention : IEntitySetConvention
    {
        public void Apply(EntitySetConfiguration configuration, ODataModelBuilder model)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            // generate links without cast for declared and inherited navigation properties
            foreach (EntityTypeConfiguration entity in configuration.EntityType.ThisAndBaseTypes())
            {
                foreach (NavigationPropertyConfiguration property in entity.NavigationProperties)
                {
                    if (configuration.GetNavigationPropertyLink(property) == null)
                    {
                        configuration.HasNavigationPropertyLink(
                                property,
                                (entityContext, navigationProperty) => GenerateNavigationPropertyLink(entityContext, navigationProperty, configuration, includeCast: false));
                    }
                }
            }

            // generate links with cast for navigation properties in derived types.
            foreach (EntityTypeConfiguration entity in model.DerivedTypes(configuration.EntityType))
            {
                foreach (NavigationPropertyConfiguration property in entity.NavigationProperties)
                {
                    if (configuration.GetNavigationPropertyLink(property) == null)
                    {
                        configuration.HasNavigationPropertyLink(
                                property,
                                (entityContext, navigationProperty) => GenerateNavigationPropertyLink(entityContext, navigationProperty, configuration, includeCast: true));
                    }
                }
            }
        }

        internal static Uri GenerateNavigationPropertyLink(EntityInstanceContext entityContext, IEdmNavigationProperty navigationProperty, EntitySetConfiguration configuration, bool includeCast)
        {
            string routeName;

            Dictionary<string, object> routeValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            routeValues.Add(LinkGenerationConstants.Controller, configuration.Name);
            routeValues.Add(LinkGenerationConstants.ParentId, ConventionsHelpers.GetEntityKeyValue(entityContext, configuration.EntityType));
            routeValues.Add(LinkGenerationConstants.NavigationProperty, navigationProperty.Name);

            if (includeCast)
            {
                routeName = ODataRouteNames.PropertyNavigationWithCast;
                routeValues.Add(LinkGenerationConstants.Entitytype, entityContext.EntityType.FullName());
            }
            else
            {
                routeName = ODataRouteNames.PropertyNavigation;
            }

            string link = entityContext.UrlHelper.Link(routeName, routeValues);

            if (link == null)
            {
                throw Error.InvalidOperation(SRResources.NavigationPropertyRouteMissingOrIncorrect, navigationProperty.Name, ODataRouteNames.PropertyNavigation);
            }

            return new Uri(link);
        }
    }
}
