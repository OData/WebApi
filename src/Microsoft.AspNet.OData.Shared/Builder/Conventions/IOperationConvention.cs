//-----------------------------------------------------------------------------
// <copyright file="IOperationConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData.Builder.Conventions
{
    /// <summary>
    /// Convention to apply to <see cref="OperationConfiguration"/> instances in the model
    /// </summary>
    internal interface IOperationConvention : IConvention
    {
        void Apply(OperationConfiguration configuration, ODataModelBuilder model);
    }
}
