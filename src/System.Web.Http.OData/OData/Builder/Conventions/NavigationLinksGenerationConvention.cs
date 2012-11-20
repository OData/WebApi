// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;
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
            List<ODataPathSegment> navigationPathSegments = new List<ODataPathSegment>();
            navigationPathSegments.Add(new EntitySetPathSegment(entityContext.EntitySet));
            navigationPathSegments.Add(new KeyValuePathSegment(ConventionsHelpers.GetEntityKeyValue(entityContext, configuration.EntityType)));

            if (includeCast)
            {
                navigationPathSegments.Add(new CastPathSegment(entityContext.EntityType));
            }

            navigationPathSegments.Add(new NavigationPathSegment(navigationProperty));

            string link = entityContext.UrlHelper.ODataLink(entityContext.PathHandler, navigationPathSegments);

            if (link == null)
            {
                return null;
            }

            return new Uri(link);
        }
    }
}
