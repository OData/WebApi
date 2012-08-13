// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;

namespace System.Web.Http.OData.Builder.Conventions
{
    public class SelfLinksGenerationConvention : IEntitySetConvention
    {
        public string SelfRouteName { get; set; }

        public void Apply(IEntitySetConfiguration configuration, ODataModelBuilder model)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            // We only need to configure the EditLink by convention, ReadLink and IdLink both delegate to EditLink
            if (configuration.GetEditLink() == null)
            {
                configuration.HasEditLink(entityContext => 
                    {
                        string routeName = SelfRouteName ?? ODataRouteNames.GetById;
                        string editlink = entityContext.UrlHelper.Link(
                            routeName,
                            new
                            {
                                controller = configuration.Name,
                                id = ConventionsHelpers.GetEntityKeyValue(entityContext, configuration.EntityType)
                            });
                        if (editlink == null)
                        {
                            throw Error.InvalidOperation(SRResources.GetByIdRouteMissingOrIncorrect, routeName);
                        }
                        return new Uri(editlink);
                    });
            }
        }
    }
}
