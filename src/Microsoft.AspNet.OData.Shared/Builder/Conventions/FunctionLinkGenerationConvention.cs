//-----------------------------------------------------------------------------
// <copyright file="FunctionLinkGenerationConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Builder.Conventions
{
    /// <summary>
    /// The FunctionLinkGenerationConvention calls function.HasFunctionLink(..) if the function binds to a single entity and has not previously been configured.
    /// </summary>
    internal class FunctionLinkGenerationConvention : IOperationConvention
    {
        public void Apply(OperationConfiguration configuration, ODataModelBuilder model)
        {
            FunctionConfiguration function = configuration as FunctionConfiguration;

            if (function == null || !function.IsBindable)
            {
                return;
            }

            // You only need to create links for bindable functions that bind to a single entity.
            if (function.BindingParameter.TypeConfiguration.Kind == EdmTypeKind.Entity && function.GetFunctionLink() == null)
            {
                string bindingParamterType = function.BindingParameter.TypeConfiguration.FullName;

                function.HasFunctionLink(entityContext => 
                    entityContext.GenerateFunctionLink(bindingParamterType, function.FullyQualifiedName, function.Parameters.Select(p => p.Name)), 
                    followsConventions: true);
            }
            else if (function.BindingParameter.TypeConfiguration.Kind == EdmTypeKind.Collection && function.GetFeedFunctionLink() == null)
            {
                if (((CollectionTypeConfiguration)function.BindingParameter.TypeConfiguration).ElementType.Kind ==
                    EdmTypeKind.Entity)
                {
                    string bindingParamterType = function.BindingParameter.TypeConfiguration.FullName;
                    function.HasFeedFunctionLink(
                        feedContext =>
                            feedContext.GenerateFunctionLink(bindingParamterType, function.FullyQualifiedName, function.Parameters.Select(p => p.Name)),
                        followsConventions: true);
                }
            }
        }
    }
}
