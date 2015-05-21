// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Common;

namespace System.Web.OData.Builder.Conventions
{
    internal class NavigationLinksGenerationConvention : INavigationSourceConvention
    {
        public void Apply(INavigationSourceConfiguration configuration, ODataModelBuilder model)
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
                                new NavigationLinkBuilder(
                                    (entityContext, navigationProperty) =>
                                        entityContext.GenerateNavigationPropertyLink(navigationProperty, includeCast: false), followsConventions: true));
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
                                new NavigationLinkBuilder(
                                    (entityContext, navigationProperty) =>
                                        entityContext.GenerateNavigationPropertyLink(navigationProperty, includeCast: true), followsConventions: true));
                    }
                }
            }
        }
    }
}
