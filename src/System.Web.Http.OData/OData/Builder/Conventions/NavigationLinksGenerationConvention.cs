// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;

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

            foreach (NavigationPropertyConfiguration property in configuration.EntityType.NavigationProperties)
            {
                if (configuration.GetNavigationPropertyLink(property.Name) == null)
                {
                    configuration.HasNavigationPropertyLink(
                            property,
                            (entityContext, navigationProperty) => 
                            {                             
                                string route = PropertyNavigationRouteName ?? ODataRouteNames.PropertyNavigation;
                                string link = entityContext.UrlHelper.Link(
                                    route,
                                    new
                                    {
                                        Controller = configuration.Name,
                                        ParentId = ConventionsHelpers.GetEntityKeyValue(entityContext, configuration.EntityType),
                                        NavigationProperty = navigationProperty.Name
                                    });

                                if (link == null)
                                {
                                    throw Error.InvalidOperation(SRResources.NavigationPropertyRouteMissingOrIncorrect, navigationProperty.Name, ODataRouteNames.PropertyNavigation);
                                }
                                return new Uri(link);
                            });
                }
            }
        }
    }
}
