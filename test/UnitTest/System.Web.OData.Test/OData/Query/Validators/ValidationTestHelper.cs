﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(typeof(QueryCompositionCustomer)));
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder(configuration);
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
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(typeof(Product)));
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder(configuration);
            builder.EntitySet<Product>("Product");
            return builder;
        }
    }
}
