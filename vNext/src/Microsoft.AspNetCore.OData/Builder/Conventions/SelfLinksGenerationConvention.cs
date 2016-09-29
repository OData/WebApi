// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Builder.Conventions
{
    internal class SelfLinksGenerationConvention : INavigationSourceConvention
    {
        public void Apply(NavigationSourceConfiguration configuration, ODataModelBuilder model)
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
                    string selfLink = feedContext.Url.CreateODataLink(new EntitySetSegment(feedContext.EntitySetBase as IEdmEntitySet));

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
                bool derivedTypesDefineNavigationProperty = model.DerivedTypes(configuration.EntityType)
                    .OfType<EntityTypeConfiguration>().Any(e => e.NavigationProperties.Any());

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
