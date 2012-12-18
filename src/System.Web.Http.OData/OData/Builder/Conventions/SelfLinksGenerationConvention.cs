// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.OData.Routing;

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

            // We only need to configure the IdLink by convention, ReadLink and EditLink both delegate to IdLink
            if (configuration.GetIdLink() == null)
            {
                bool derivedTypesDefineNavigationProperty = model.DerivedTypes(configuration.EntityType).Any(e => e.NavigationProperties.Any());

                // generate links with cast if any of the derived types define a navigation property
                if (derivedTypesDefineNavigationProperty)
                {
                    configuration.HasIdLink(new SelfLinkBuilder<string>((entityContext) => GenerateSelfLink(configuration, entityContext, includeCast: true), followsConventions: true));
                }
                else
                {
                    configuration.HasIdLink(new SelfLinkBuilder<string>((entityContext) => GenerateSelfLink(configuration, entityContext, includeCast: false), followsConventions: true));
                }
            }
        }

        internal static string GenerateSelfLink(EntitySetConfiguration configuration, EntityInstanceContext entityContext, bool includeCast)
        {
            List<ODataPathSegment> idLinkPathSegments = new List<ODataPathSegment>();

            idLinkPathSegments.Add(new EntitySetPathSegment(entityContext.EntitySet));
            idLinkPathSegments.Add(new KeyValuePathSegment(ConventionsHelpers.GetEntityKeyValue(entityContext, configuration.EntityType)));

            if (includeCast)
            {
                idLinkPathSegments.Add(new CastPathSegment(entityContext.EntityType));
            }

            string idLink = entityContext.UrlHelper.ODataLink(entityContext.PathHandler, idLinkPathSegments);

            if (idLink == null)
            {
                return null;
            }

            return idLink;
        }
    }
}
