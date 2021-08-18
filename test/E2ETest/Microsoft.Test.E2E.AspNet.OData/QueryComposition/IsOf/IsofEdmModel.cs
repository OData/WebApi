//-----------------------------------------------------------------------------
// <copyright file="IsofEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf
{
    public static class IsofEdmModel
    {
        private static IEdmModel _model;

        public static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            if (_model != null)
            {
                return _model;
            }

            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<BillingCustomer>("BillingCustomers");
            builder.EntitySet<BillingDetail>("Billings");
            return _model = builder.GetEdmModel();
        }
    }
}
