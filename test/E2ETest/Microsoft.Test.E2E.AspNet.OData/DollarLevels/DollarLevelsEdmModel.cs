//-----------------------------------------------------------------------------
// <copyright file="DollarLevelsEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.DollarLevels
{
    public class DollarLevelsEdmModel
    {
        public static IEdmModel GetConventionModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<DLManager>("DLManagers");
            builder.EntitySet<DLEmployee>("DLEmployees");
            builder.EntitySet<TestQueryOptions>("Tests");

            builder.Namespace = typeof(DLManager).Namespace;
            return builder.GetEdmModel();
        }
    }
}
