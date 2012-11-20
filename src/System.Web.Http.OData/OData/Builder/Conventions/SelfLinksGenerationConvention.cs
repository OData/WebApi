// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;
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
                    string selfLink = entitySetContext.UrlHelper.ODataLink(entitySetContext.PathHandler, new EntitySetPathSegment(entitySetContext.EntitySet));

                    if (selfLink == null)
                    {
                        return null;
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
            List<ODataPathSegment> editLinkPathSegments = new List<ODataPathSegment>();

            editLinkPathSegments.Add(new EntitySetPathSegment(entityContext.EntitySet));
            editLinkPathSegments.Add(new KeyValuePathSegment(ConventionsHelpers.GetEntityKeyValue(entityContext, configuration.EntityType)));

            if (includeCast)
            {
                editLinkPathSegments.Add(new CastPathSegment(entityContext.EntityType));
            }

            string editLink = entityContext.UrlHelper.ODataLink(entityContext.PathHandler, editLinkPathSegments);

            if (editLink == null)
            {
                return null;
            }

            return new Uri(editLink);
        }
    }
}
