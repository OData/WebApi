// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder.Conventions
{
    /// <summary>
    /// The ActionLinkGenerationConvention calls action.HasActionLink(..) for all actions that bind to a single entity if they have not previously been configured.
    /// The convention uses the <see cref="ODataRouteConstants"/>.InvokeBoundAction route to build a link that invokes the action.
    /// </summary>
    internal class ActionLinkGenerationConvention : IProcedureConvention
    {
        public void Apply(ProcedureConfiguration configuration, ODataModelBuilder model)
        {
            ActionConfiguration action = configuration as ActionConfiguration;

            // You only need to create links for bindable actions that bind to a single entity.
            if (action != null && action.IsBindable && action.BindingParameter.TypeConfiguration.Kind == EdmTypeKind.Entity && action.GetActionLink() == null)
            {
                action.HasActionLink(entityContext => GenerateActionLink(entityContext, action), followsConventions: true);
            }
        }

        internal static Uri GenerateActionLink(EntityInstanceContext entityContext, ActionConfiguration action)
        {
            // the entity type the action is bound to.
            EntityTypeConfiguration actionEntityType = action.BindingParameter.TypeConfiguration as EntityTypeConfiguration;
            Contract.Assert(actionEntityType != null, "we have already verified that binding paramter type is entity");

            List<ODataPathSegment> actionPathSegments = new List<ODataPathSegment>();
            actionPathSegments.Add(new EntitySetPathSegment(entityContext.EntitySet));
            actionPathSegments.Add(new KeyValuePathSegment(ConventionsHelpers.GetEntityKeyValue(entityContext)));

            // generate link with cast if the entityset type doesn't match the entity type the action is bound to.
            if (!entityContext.EntitySet.ElementType.IsOrInheritsFrom(entityContext.EdmModel.FindDeclaredType(actionEntityType.FullName)))
            {
                actionPathSegments.Add(new CastPathSegment(entityContext.EntityType));
            }

            actionPathSegments.Add(new ActionPathSegment(action.Name));

            string actionLink = entityContext.Url.ODataLink(actionPathSegments);

            if (actionLink == null)
            {
                return null;
            }

            return new Uri(actionLink);
        }
    }
}
