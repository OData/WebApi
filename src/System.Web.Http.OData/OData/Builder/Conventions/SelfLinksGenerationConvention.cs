// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;

namespace System.Web.Http.OData.Builder.Conventions
{
    public class SelfLinksGenerationConvention : IEntitySetConvention
    {
        /// <summary>
        /// Gets or sets the route name used for addressing root level entity sets.
        /// </summary>
        public string DefaultRouteName { get; set; }

        /// <summary>
        /// Gets or sets the route name used for addressing entities by their keys.
        /// </summary>
        public string GetByIdRouteName { get; set; }

        public void Apply(IEntitySetConfiguration configuration, ODataModelBuilder model)
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
                    string routeName = DefaultRouteName ?? ODataRouteNames.Default;
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
                configuration.HasEditLink(entityContext => 
                    {
                        string routeName = GetByIdRouteName ?? ODataRouteNames.GetById;
                        string editLink = entityContext.UrlHelper.Link(
                            routeName,
                            new
                            {
                                controller = configuration.Name,
                                id = ConventionsHelpers.GetEntityKeyValue(entityContext, configuration.EntityType)
                            });
                        if (editLink == null)
                        {
                            throw Error.InvalidOperation(SRResources.GetByIdRouteMissingOrIncorrect, routeName);
                        }
                        return new Uri(editLink);
                    });
            }
        }
    }
}
