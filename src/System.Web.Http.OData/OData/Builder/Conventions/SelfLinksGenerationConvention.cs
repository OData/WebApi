// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder.Conventions
{
    public class SelfLinksGenerationConvention : IEntitySetConvention
    {
        public void Apply(EntitySetConfiguration configuration, ODataModelBuilder model)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            // Configure the self link for the feed
            if (configuration.GetFeedSelfLink() == null)
            {
                configuration.HasFeedSelfLink(entitySetContext =>
                {
                    string routeName = ODataRouteNames.Default;
                    string selfLink = entitySetContext.UrlHelper.Link(
                        routeName,
                        new
                        {
                            controller = configuration.Name
                        });
                    if (selfLink == null)
                    {
                        throw Error.InvalidOperation(SRResources.DefaultRouteMissingOrIncorrect, routeName);
                    }
                    return new Uri(selfLink);
                });
            }

            // We only need to configure the EditLink by convention, ReadLink and IdLink both delegate to EditLink
            if (configuration.GetEditLink() == null)
            {
                bool derivedTypesDefineNavigationProperty = model.DerivedTypes(configuration.EntityType).Any(e => e.NavigationProperties.Any());

                // generate links with cast if any of the derived types define a navigation property
                if (derivedTypesDefineNavigationProperty)
                {
                    configuration.HasEditLink((entityContext) => GenerateSelfLink(configuration, entityContext, includeCast: true));
                }
                else
                {
                    configuration.HasEditLink((entityContext) => GenerateSelfLink(configuration, entityContext, includeCast: false));
                }
            }
        }

        internal static Uri GenerateSelfLink(EntitySetConfiguration configuration, EntityInstanceContext entityContext, bool includeCast)
        {
            string routeName;

            Dictionary<string, object> routeValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            routeValues.Add(LinkGenerationConstants.Controller, configuration.Name);
            routeValues.Add(LinkGenerationConstants.Id, ConventionsHelpers.GetEntityKeyValue(entityContext, configuration.EntityType));

            if (includeCast)
            {
                routeName = ODataRouteNames.GetByIdWithCast;
                routeValues.Add(LinkGenerationConstants.Entitytype, entityContext.EntityType.FullName());
            }
            else
            {
                routeName = ODataRouteNames.GetById;
            }

            string editLink = entityContext.UrlHelper.Link(routeName, routeValues);
            if (editLink == null)
            {
                throw Error.InvalidOperation(SRResources.GetByIdRouteMissingOrIncorrect, routeName);
            }

            return new Uri(editLink);
        }
    }
}
