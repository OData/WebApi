// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder.Conventions
{
    internal class NavigationLinksGenerationConvention : IEntitySetConvention
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
                                new NavigationLinkBuilder(
                                    (entityContext, navigationProperty) =>
                                        GenerateNavigationPropertyLink(entityContext, navigationProperty, includeCast: false), followsConventions: true));
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
                                        GenerateNavigationPropertyLink(entityContext, navigationProperty, includeCast: true), followsConventions: true));
                    }
                }
            }
        }

        internal static Uri GenerateNavigationPropertyLink(EntityInstanceContext entityContext, IEdmNavigationProperty navigationProperty, bool includeCast)
        {
            List<ODataPathSegment> navigationPathSegments = new List<ODataPathSegment>();
            navigationPathSegments.Add(new EntitySetPathSegment(entityContext.EntitySet));
            navigationPathSegments.Add(new KeyValuePathSegment(ConventionsHelpers.GetEntityKeyValue(entityContext)));

            if (includeCast)
            {
                navigationPathSegments.Add(new CastPathSegment(entityContext.EntityType));
            }

            navigationPathSegments.Add(new NavigationPathSegment(navigationProperty));

            string link = entityContext.Url.ODataLink(navigationPathSegments);

            if (link == null)
            {
                return null;
            }

            return new Uri(link);
        }
    }
}
