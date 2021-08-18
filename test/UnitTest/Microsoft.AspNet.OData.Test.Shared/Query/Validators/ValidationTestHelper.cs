//-----------------------------------------------------------------------------
// <copyright file="ValidationTestHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Query.Expressions;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Test.Query.Validators
{
    internal static class ValidationTestHelper
    {
        internal static ODataQueryContext CreateCustomerContext()
        {
            return CreateCustomerContext(true);
        }

        internal static ODataQueryContext CreateCustomerContext(bool setRequestContainer)
        {
            ODataQueryContext context = new ODataQueryContext(GetCustomersModel(), typeof(QueryCompositionCustomer), null);
            if (setRequestContainer)
            {
                context.RequestContainer = new MockContainer();
            }

            context.DefaultQuerySettings.EnableOrderBy = true;
            context.DefaultQuerySettings.MaxTop = null;
            return context;
        }

        internal static ODataQueryContext CreateProductContext()
        {
            return new ODataQueryContext(GetProductsModel(), typeof(Product));
        }

        internal static ODataQueryContext CreateDerivedProductsContext()
        {
            ODataQueryContext context = new ODataQueryContext(GetDerivedProductsModel(), typeof(Product), null);
            context.RequestContainer = new MockContainer();
            context.DefaultQuerySettings.EnableFilter = true;
            return context;
        }

        private static IEdmModel GetCustomersModel()
        {
            var configuration = RoutingConfigurationFactory.CreateWithTypes(typeof(QueryCompositionCustomer));
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create(configuration);
            builder.EntitySet<QueryCompositionCustomer>("Customer");
            builder.EntityType<QueryCompositionCustomerBase>();
            return builder.GetEdmModel();
        }

        private static IEdmModel GetProductsModel()
        {
            var builder = GetProductsBuilder();
            return builder.GetEdmModel();
        }

        private static IEdmModel GetDerivedProductsModel()
        {
            var builder = GetProductsBuilder();
            builder.EntitySet<Product>("Product");
            builder.EntityType<DerivedProduct>().DerivesFrom<Product>();
            builder.EntityType<DerivedCategory>().DerivesFrom<Category>();
            return builder.GetEdmModel();
        }

        private static ODataConventionModelBuilder GetProductsBuilder()
        {
            var configuration = RoutingConfigurationFactory.CreateWithTypes(typeof(Product));
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create(configuration);
            builder.EntitySet<Product>("Product");
            return builder;
        }
    }
}
