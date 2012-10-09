// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder.Conventions
{
    public class NavigationLinksGenerationConvention : IEntitySetConvention
    {
        public string PropertyNavigationRouteName { get; set; }

        public void Apply(IEntitySetConfiguration configuration, ODataModelBuilder model)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            foreach (IEntityTypeConfiguration entity in model.ThisAndBaseAndDerivedTypes(configuration.EntityType))
            {
                foreach (NavigationPropertyConfiguration property in entity.NavigationProperties)
                {
                    if (configuration.GetNavigationPropertyLink(property) == null)
                    {
                        configuration.HasNavigationPropertyLink(
                                property,
                                (entityContext, navigationProperty) => GenerateNavigationLink(entityContext, navigationProperty, configuration));
                    }
                }
            }
        }

        internal Uri GenerateNavigationLink(EntityInstanceContext entityContext, IEdmNavigationProperty navigationProperty, IEntitySetConfiguration configuration)
        {
            string route = PropertyNavigationRouteName ?? ODataRouteNames.PropertyNavigation;

            string link = entityContext.UrlHelper.Link(
                route,
                new
                {
                    controller = configuration.Name,
                    parentId = ConventionsHelpers.GetEntityKeyValue(entityContext, configuration.EntityType),
                    navigationProperty = navigationProperty.Name
                });

            if (link == null)
            {
                throw Error.InvalidOperation(SRResources.NavigationPropertyRouteMissingOrIncorrect, navigationProperty.Name, ODataRouteNames.PropertyNavigation);
            }
            return new Uri(link);
        }
    }
}
