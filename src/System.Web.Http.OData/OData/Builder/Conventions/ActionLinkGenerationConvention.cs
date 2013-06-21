// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
                string bindingParamterType = action.BindingParameter.TypeConfiguration.FullName;
                action.HasActionLink(entityContext => entityContext.GenerateActionLink(bindingParamterType, action.Name), followsConventions: true);
            }
        }
    }
}
