// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Builder.Conventions
{
    /// <summary>
    /// Convention to apply to <see cref="ProcedureConfiguration"/> instances in the model
    /// </summary>
    internal interface IProcedureConvention : IConvention
    {
        void Apply(ProcedureConfiguration configuration, ODataModelBuilder model);
    }
}
