// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder.Conventions
{
    /// <summary>
    /// The FunctionLinkGenerationConvention calls function.HasFunctionLink(..) if the function binds to a single entity and has not previously been configured.
    /// </summary>
    internal class FunctionLinkGenerationConvention : IProcedureConvention
    {
        public void Apply(ProcedureConfiguration configuration, ODataModelBuilder model)
        {
            FunctionConfiguration function = configuration as FunctionConfiguration;

            // You only need to create links for bindable functions that bind to a single entity.
            if (function != null && function.IsBindable && function.BindingParameter.TypeConfiguration.Kind == EdmTypeKind.Entity && function.GetFunctionLink() == null)
            {
                string bindingParamterType = function.BindingParameter.TypeConfiguration.FullName;

                function.HasFunctionLink(entityContext => 
                    entityContext.GenerateFunctionLink(bindingParamterType, function.Name, function.Parameters.Select(p => p.Name)), 
                    followsConventions: true);
            }
        }
    }
}
