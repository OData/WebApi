// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
            builder.EntitySet<Test>("Tests");

            builder.Namespace = typeof(DLManager).Namespace;
            return builder.GetEdmModel();
        }
    }
}
