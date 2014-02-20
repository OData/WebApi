// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder;
using System.Web.OData.Query.Expressions;
using System.Web.OData.TestCommon;
using Microsoft.OData.Edm;

namespace System.Web.OData.Query.Validators
{
    internal static class ValidationTestHelper
    {
        internal static ODataQueryContext CreateCustomerContext()
        {
            return new ODataQueryContext(GetCustomersModel(), typeof(QueryCompositionCustomer));
        }

        internal static ODataQueryContext CreateProductContext()
        {
            return new ODataQueryContext(GetProductsModel(), typeof(Product));
        }

        private static IEdmModel GetCustomersModel()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(typeof(QueryCompositionCustomer)));
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder(configuration);
            builder.EntitySet<QueryCompositionCustomer>("Customer");
            builder.EntityType<QueryCompositionCustomerBase>();
            return builder.GetEdmModel();
        }

        private static IEdmModel GetProductsModel()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(typeof(Product)));
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder(configuration);
            builder.EntitySet<Product>("Product");
            return builder.GetEdmModel();
        }
    }
}
