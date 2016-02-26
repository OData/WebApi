// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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

            if (action == null || !action.IsBindable)
            {
                return;
            }

            // You only need to create links for bindable actions that bind to a single entity.
            if (action.BindingParameter.TypeConfiguration.Kind == EdmTypeKind.Entity && action.GetActionLink() == null)
            {
                if (action.BindingParameter.TypeConfiguration.Kind == EdmTypeKind.Entity &&
                    action.GetActionLink() == null)
                {
                    string bindingParamterType = action.BindingParameter.TypeConfiguration.FullName;
                    action.HasActionLink(
                        entityContext =>
                            entityContext.GenerateActionLink(bindingParamterType, action.FullyQualifiedName),
                        followsConventions: true);
                }
            }
            else if (action.BindingParameter.TypeConfiguration.Kind == EdmTypeKind.Collection &&
                     action.GetFeedActionLink() == null)
            {
                if (((CollectionTypeConfiguration)action.BindingParameter.TypeConfiguration).ElementType.Kind ==
                    EdmTypeKind.Entity)
                {
                    string bindingParamterType = action.BindingParameter.TypeConfiguration.FullName;
                    action.HasFeedActionLink(
                        feedContext =>
                            feedContext.GenerateActionLink(bindingParamterType, action.FullyQualifiedName),
                        followsConventions: true);
                }
            }
        }
    }
}
