// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder.Conventions
{
    /// <summary>
    /// The ActionLinkGenerationConvention calls action.HasActionLink(..) for all actions that bind to a single entity if they have not previously been configured.
    /// The convention uses the <see cref="ODataRouteNames"/>.InvokeBoundAction route to build a link that invokes the action.
    /// </summary>
    public class ActionLinkGenerationConvention : IProcedureConvention
    {
        /// <summary>
        /// Gets or sets the route name used for addressing entities by their keys.
        /// </summary>
        public string InvokeBoundActionRouteName { get; set; }

        public void Apply(ProcedureConfiguration configuration, ODataModelBuilder model)
        {
            ActionConfiguration action = configuration as ActionConfiguration;

            // You can only need to create links for bindable actions that bind to a single entity.
            if (action != null && action.IsBindable && action.BindingParameter.TypeConfiguration.Kind == EdmTypeKind.Entity && action.GetActionLink() == null)
            {
                IEntityTypeConfiguration entityType = action.BindingParameter.TypeConfiguration as IEntityTypeConfiguration;
                action.HasActionLink(entityContext =>
                {
                    string routeName = InvokeBoundActionRouteName ?? ODataRouteNames.InvokeBoundAction;
                    string actionLink = entityContext.UrlHelper.Link(
                        routeName,
                        new
                        {
                            controller = entityContext.EntitySet.Name,
                            boundId = ConventionsHelpers.GetEntityKeyValue(entityContext, entityType),
                            odataAction = action.Name
                        });

                    if (actionLink == null)
                    {
                        return null;
                    }
                    else
                    {
                        return new Uri(actionLink);
                    }
                });
            }
        }
    }
}
