// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;

namespace System.Web.OData.Builder.Conventions
{
    internal class SelfLinksGenerationConvention : INavigationSourceConvention
    {
        public void Apply(INavigationSourceConfiguration configuration, ODataModelBuilder model)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            // Configure the self link for the feed
            EntitySetConfiguration entitySet = configuration as EntitySetConfiguration;
            if (entitySet != null && (entitySet.GetFeedSelfLink() == null))
            {
                entitySet.HasFeedSelfLink(feedContext =>
                {
                    string selfLink = feedContext.Url.CreateODataLink(new EntitySetPathSegment(feedContext.EntitySetBase));

                    if (selfLink == null)
                    {
                        return null;
                    }
                    return new Uri(selfLink);
                });
            }

            if (configuration.GetIdLink() == null)
            {
                configuration.HasIdLink(new SelfLinkBuilder<Uri>((entityContext) => entityContext.GenerateSelfLink(includeCast: false), followsConventions: true));
            }

            if (configuration.GetEditLink() == null)
            {
                bool derivedTypesDefineNavigationProperty = model.DerivedTypes(configuration.EntityType).Any(e => e.NavigationProperties.Any());
                
                // generate links with cast if any of the derived types define a navigation property
                if (derivedTypesDefineNavigationProperty)
                {
                    configuration.HasEditLink(
                        new SelfLinkBuilder<Uri>(
                            entityContext => entityContext.GenerateSelfLink(includeCast: true),
                            followsConventions: true));
                }
                else
                {
                    configuration.HasEditLink(
                        new SelfLinkBuilder<Uri>(
                            entityContext => entityContext.GenerateSelfLink(includeCast: false),
                            followsConventions: true));
                }
            }
        }
    }
}
