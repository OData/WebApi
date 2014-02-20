// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Edm;

namespace System.Web.OData.Builder.Conventions
{
    /// <summary>
    /// The ActionLinkGenerationConvention calls action.HasActionLink(..) if the action binds to a single entity and has not previously been configured.
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
                action.HasActionLink(entityContext => entityContext.GenerateActionLink(bindingParamterType, action.FullyQualifiedName), followsConventions: true);
            }
        }
    }
}
