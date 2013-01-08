// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Query.Expressions;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Query.Validators
{
    internal static class ValidationTestHelper
    {
        internal static ODataQueryContext CreateCustomerContext()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<QueryCompositionCustomer>("Customer");
            builder.Entity<QueryCompositionCustomerBase>();
            IEdmModel model = builder.GetEdmModel();
            return new ODataQueryContext(model, typeof(QueryCompositionCustomer));
        } 

        internal static ODataQueryContext CreateProductContext()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Product>("Product");
            IEdmModel model = builder.GetEdmModel();
            return new ODataQueryContext(model, typeof(Product));
        }
    }
}
