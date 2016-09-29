﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Builder.Conventions
{
    /// <summary>
    /// The ActionLinkGenerationConvention calls action.HasActionLink(..) if the action binds to a single entity and has not previously been configured.
    /// </summary>
    internal class ActionLinkGenerationConvention : IOperationConvention
    {
        public void Apply(OperationConfiguration configuration, ODataModelBuilder model)
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
