// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;

namespace System.Web.Http.OData.Builder.Conventions
{
    internal class SelfLinksGenerationConvention : IEntitySetConvention
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
                    string selfLink = entitySetContext.Url.CreateODataLink(new EntitySetPathSegment(entitySetContext.EntitySet));

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
                    configuration.HasIdLink(new SelfLinkBuilder<string>((entityContext) => entityContext.GenerateSelfLink(includeCast: true), followsConventions: true));
                }
                else
                {
                    configuration.HasIdLink(new SelfLinkBuilder<string>((entityContext) => entityContext.GenerateSelfLink(includeCast: false), followsConventions: true));
                }
            }
        }
    }
}
