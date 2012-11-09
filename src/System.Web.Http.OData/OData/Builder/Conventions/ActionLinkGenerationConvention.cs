// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder.Conventions
{
    /// <summary>
    /// The ActionLinkGenerationConvention calls action.HasActionLink(..) for all actions that bind to a single entity if they have not previously been configured.
    /// The convention uses the <see cref="ODataRouteNames"/>.InvokeBoundAction route to build a link that invokes the action.
    /// </summary>
    public class ActionLinkGenerationConvention : IProcedureConvention
    {
        public void Apply(ProcedureConfiguration configuration, ODataModelBuilder model)
        {
            ActionConfiguration action = configuration as ActionConfiguration;

            // You only need to create links for bindable actions that bind to a single entity.
            if (action != null && action.IsBindable && action.BindingParameter.TypeConfiguration.Kind == EdmTypeKind.Entity && action.GetActionLink() == null)
            {
                action.HasActionLink(entityContext => GenerateActionLink(entityContext, action));
            }
        }

        internal static Uri GenerateActionLink(EntityInstanceContext entityContext, ActionConfiguration action)
        {
            // the entity type the action is bound to.
            EntityTypeConfiguration actionEntityType = action.BindingParameter.TypeConfiguration as EntityTypeConfiguration;
            Contract.Assert(actionEntityType != null, "we have already verified that binding paramter type is entity");

            Dictionary<string, object> routeValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            routeValues.Add(LinkGenerationConstants.Controller, entityContext.EntitySet.Name);
            routeValues.Add(LinkGenerationConstants.BoundId, ConventionsHelpers.GetEntityKeyValue(entityContext, actionEntityType));
            routeValues.Add(LinkGenerationConstants.ODataAction, action.Name);

            string routeName;

            // generate link without cast if the entityset type matches the entity type the action is bound to.
            if (entityContext.EntitySet.ElementType.IsOrInheritsFrom(entityContext.EdmModel.FindDeclaredType(actionEntityType.FullName)))
            {
                routeName = ODataRouteNames.InvokeBoundAction;
            }
            else
            {
                routeName = ODataRouteNames.InvokeBoundActionWithCast;
                routeValues.Add(LinkGenerationConstants.Entitytype, entityContext.EntityType.FullName());
            }

            string actionLink = entityContext.UrlHelper.Link(routeName, routeValues);

            if (actionLink == null)
            {
                return null;
            }
            else
            {
                return new Uri(actionLink);
            }
        }
    }
}
