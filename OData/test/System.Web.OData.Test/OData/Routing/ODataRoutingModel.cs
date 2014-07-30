// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.TestCommon;
using Microsoft.OData.Edm;
using Microsoft.TestCommon.Types;

namespace System.Web.OData.Routing
{
    public class ODataRoutingModel
    {
        public static IEdmModel GetModel()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver());
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder(configuration);
            builder.EntitySet<RoutingCustomer>("RoutingCustomers");
            builder.EntitySet<Product>("Products");
            builder.EntitySet<SalesPerson>("SalesPeople");
            builder.EntitySet<EmailAddress>("EmailAddresses");
            builder.EntitySet<üCategory>("üCategories");
            builder.EntitySet<EnumCustomer>("EnumCustomers");
            builder.Singleton<RoutingCustomer>("VipCustomer");
            builder.Singleton<Product>("MyProduct");
            builder.EntitySet<DateTimeOffsetKeyCustomer>("DateTimeOffsetKeyCustomers");

            ActionConfiguration getRoutingCustomerById = builder.Action("GetRoutingCustomerById");
            getRoutingCustomerById.Parameter<int>("RoutingCustomerId");
            getRoutingCustomerById.ReturnsFromEntitySet<RoutingCustomer>("RoutingCustomers");

            ActionConfiguration getSalesPersonById = builder.Action("GetSalesPersonById");
            getSalesPersonById.Parameter<int>("salesPersonId");
            getSalesPersonById.ReturnsFromEntitySet<SalesPerson>("SalesPeople");

            ActionConfiguration getAllVIPs = builder.Action("GetAllVIPs");
            ActionReturnsCollectionFromEntitySet<VIP>(builder, getAllVIPs, "RoutingCustomers");

            builder.EntityType<RoutingCustomer>().ComplexProperty<Address>(c => c.Address);
            builder.EntityType<RoutingCustomer>().Action("GetRelatedRoutingCustomers").ReturnsCollectionFromEntitySet<RoutingCustomer>("RoutingCustomers");

            ActionConfiguration getBestRelatedRoutingCustomer = builder.EntityType<RoutingCustomer>().Action("GetBestRelatedRoutingCustomer");
            ActionReturnsFromEntitySet<VIP>(builder, getBestRelatedRoutingCustomer, "RoutingCustomers");

            ActionConfiguration getVIPS = builder.EntityType<RoutingCustomer>().Collection.Action("GetVIPs");
            ActionReturnsCollectionFromEntitySet<VIP>(builder, getVIPS, "RoutingCustomers");

            builder.EntityType<RoutingCustomer>().Collection.Action("GetProducts").ReturnsCollectionFromEntitySet<Product>("Products");
            builder.EntityType<VIP>().Action("GetSalesPerson").ReturnsFromEntitySet<SalesPerson>("SalesPeople");
            builder.EntityType<VIP>().Collection.Action("GetSalesPeople").ReturnsCollectionFromEntitySet<SalesPerson>("SalesPeople");

            ActionConfiguration getMostProfitable = builder.EntityType<VIP>().Collection.Action("GetMostProfitable");
            ActionReturnsFromEntitySet<VIP>(builder, getMostProfitable, "RoutingCustomers");

            ActionConfiguration getVIPRoutingCustomers = builder.EntityType<SalesPerson>().Action("GetVIPRoutingCustomers");
            ActionReturnsCollectionFromEntitySet<VIP>(builder, getVIPRoutingCustomers, "RoutingCustomers");

            ActionConfiguration getVIPRoutingCustomersOnCollection = builder.EntityType<SalesPerson>().Collection.Action("GetVIPRoutingCustomers");
            ActionReturnsCollectionFromEntitySet<VIP>(builder, getVIPRoutingCustomersOnCollection, "RoutingCustomers");

            builder.EntityType<VIP>().HasRequired(v => v.RelationshipManager);
            builder.EntityType<ImportantProduct>().HasRequired(ip => ip.LeadSalesPerson);

            // function bound to an entity
            FunctionConfiguration topProductId = builder.EntityType<Product>().Function("TopProductId");
            topProductId.Returns<int>();

            FunctionConfiguration topProductIdByCity = builder.EntityType<Product>().Function("TopProductIdByCity");
            topProductIdByCity.Parameter<string>("city");
            topProductIdByCity.Returns<string>();

            FunctionConfiguration topProductIdByCityAndModel = builder.EntityType<Product>().Function("TopProductIdByCityAndModel");
            topProductIdByCityAndModel.Parameter<string>("city");
            topProductIdByCityAndModel.Parameter<int>("model");
            topProductIdByCityAndModel.Returns<string>();

            // function bound to a collection of entities
            FunctionConfiguration topProductOfAll = builder.EntityType<Product>().Collection.Function("TopProductOfAll");
            topProductOfAll.Returns<string>();

            FunctionConfiguration topProductOfAllByCity = builder.EntityType<Product>().Collection.Function("TopProductOfAllByCity");
            topProductOfAllByCity.Parameter<string>("city");
            topProductOfAllByCity.Returns<string>();

            FunctionConfiguration topProductOfAllByCityAndModel = builder.EntityType<Product>().Collection.Function("TopProductOfAllByCityAndModel");
            topProductOfAllByCityAndModel.Parameter<string>("city");
            topProductOfAllByCityAndModel.Parameter<int>("model");
            topProductOfAllByCityAndModel.Returns<string>();

            // Function bound to the base entity type and derived entity type
            builder.EntityType<RoutingCustomer>().Function("GetOrdersCount").Returns<string>();
            builder.EntityType<VIP>().Function("GetOrdersCount").Returns<string>();

            // Overloaded function only bound to the base entity type with one paramter
            var getOrderCount = builder.EntityType<RoutingCustomer>().Function("GetOrdersCount");
            getOrderCount.Parameter<int>("factor");
            getOrderCount.Returns<string>();

            // Function only bound to the derived entity type
            builder.EntityType<SpecialVIP>().Function("GetSpecialGuid").Returns<string>();

            // Function bound to the collection of the base and the derived entity type
            builder.EntityType<RoutingCustomer>().Collection.Function("GetAllEmployees").Returns<string>();
            builder.EntityType<VIP>().Collection.Function("GetAllEmployees").Returns<string>();

            // Bound function with enum type parameters
            var boundFunction = builder.EntityType<RoutingCustomer>().Collection.Function("BoundFuncWithEnumParameters");
            boundFunction.Parameter<SimpleEnum>("SimpleEnum");
            boundFunction.Parameter<FlagsEnum>("FlagsEnum");
            boundFunction.Returns<string>();

            // Bound function with enum type parameter for attribute routing
            var boundFunctionForAttributeRouting = builder.EntityType<RoutingCustomer>().Collection
                .Function("BoundFuncWithEnumParameterForAttributeRouting");
            boundFunctionForAttributeRouting.Parameter<SimpleEnum>("SimpleEnum");
            boundFunctionForAttributeRouting.Returns<string>();

            // Unbound function with enum type parameters
            var function = builder.Function("UnboundFuncWithEnumParameters");
            function.Parameter<LongEnum>("LongEnum");
            function.Parameter<FlagsEnum>("FlagsEnum");
            function.Returns<string>();

            // Unbound function
            builder.Function("UnboundFunction").Returns<int>();

            // Action only bound to the derived entity type
            builder.EntityType<SpecialVIP>().Action("ActionBoundToSpecialVIP");

            // Action only bound to the derived entity type
            builder.EntityType<SpecialVIP>().Collection.Action("ActionBoundToSpecialVIPs");

            // Function only bound to the base entity collection type
            builder.EntityType<RoutingCustomer>().Collection.Function("FunctionBoundToRoutingCustomers").Returns<int>();

            // Function only bound to the derived entity collection type
            builder.EntityType<VIP>().Collection.Function("FunctionBoundToVIPs").Returns<int>();

            // Bound function with multiple parameters
            var functionBoundToProductWithMultipleParamters = builder.EntityType<Product>().Function("FunctionBoundToProductWithMultipleParamters");
            functionBoundToProductWithMultipleParamters.Parameter<int>("P1");
            functionBoundToProductWithMultipleParamters.Parameter<int>("P2");
            functionBoundToProductWithMultipleParamters.Parameter<string>("P3");
            functionBoundToProductWithMultipleParamters.Returns<int>();

            // Overloaded bound function with no parameter
            builder.EntityType<Product>().Function("FunctionBoundToProduct").Returns<int>();

            // Overloaded bound function with one parameter
            builder.EntityType<Product>().Function("FunctionBoundToProduct").Returns<int>().Parameter<int>("P1");

            // Overloaded bound function with multiple parameters
            var functionBoundToProduct = builder.EntityType<Product>().Function("FunctionBoundToProduct").Returns<int>();
            functionBoundToProduct.Parameter<int>("P1");
            functionBoundToProduct.Parameter<int>("P2");
            functionBoundToProduct.Parameter<string>("P3");

            // Unbound function with one parameter
            var unboundFunctionWithOneParamters = builder.Function("UnboundFunctionWithOneParamters");
            unboundFunctionWithOneParamters.Parameter<int>("P1");
            unboundFunctionWithOneParamters.ReturnsFromEntitySet<RoutingCustomer>("RoutingCustomers");
            unboundFunctionWithOneParamters.IsComposable = true;

            // Unbound function with multiple parameters
            var functionWithMultipleParamters = builder.Function("UnboundFunctionWithMultipleParamters");
            functionWithMultipleParamters.Parameter<int>("P1");
            functionWithMultipleParamters.Parameter<int>("P2");
            functionWithMultipleParamters.Parameter<string>("P3");
            functionWithMultipleParamters.Returns<int>();

            // Overloaded unbound function with no parameter
            builder.Function("OverloadUnboundFunction").Returns<int>();

            // Overloaded unbound function with one parameter
            builder.Function("OverloadUnboundFunction").Returns<int>().Parameter<int>("P1");

            // Overloaded unbound function with multiple parameters
            var overloadUnboundFunction = builder.Function("OverloadUnboundFunction").Returns<int>();
            overloadUnboundFunction.Parameter<int>("P1");
            overloadUnboundFunction.Parameter<int>("P2");
            overloadUnboundFunction.Parameter<string>("P3");

            return builder.GetEdmModel();
        }

        public static ActionConfiguration ActionReturnsFromEntitySet<TEntityType>(ODataModelBuilder builder, ActionConfiguration action, string entitySetName) where TEntityType : class
        {
            action.NavigationSource = CreateOrReuseEntitySet<TEntityType>(builder, entitySetName);
            action.ReturnType = builder.GetTypeConfigurationOrNull(typeof(TEntityType));
            return action;
        }

        public static ActionConfiguration ActionReturnsCollectionFromEntitySet<TElementEntityType>(ODataModelBuilder builder, ActionConfiguration action, string entitySetName) where TElementEntityType : class
        {
            Type clrCollectionType = typeof(IEnumerable<TElementEntityType>);
            action.NavigationSource = CreateOrReuseEntitySet<TElementEntityType>(builder, entitySetName);
            IEdmTypeConfiguration elementType = builder.GetTypeConfigurationOrNull(typeof(TElementEntityType));
            action.ReturnType = new CollectionTypeConfiguration(elementType, clrCollectionType);
            return action;
        }

        public static EntitySetConfiguration CreateOrReuseEntitySet<TElementEntityType>(ODataModelBuilder builder, string entitySetName) where TElementEntityType : class
        {
            EntitySetConfiguration entitySet = builder.EntitySets.SingleOrDefault(s => s.Name == entitySetName);

            if (entitySet == null)
            {
                builder.EntitySet<TElementEntityType>(entitySetName);
                entitySet = builder.EntitySets.Single(s => s.Name == entitySetName);
            }
            else
            {
                builder.EntityType<TElementEntityType>();
            }
            return entitySet;
        }

        public class RoutingCustomer
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public virtual List<Product> Products { get; set; }
            public Address Address { get; set; }
        }

        public class EmailAddress
        {
            [Key]
            public string Value { get; set; }
            public string Text { get; set; }
        }

        public class Address
        {
            public string Street { get; set; }
            public string City { get; set; }
            public string ZipCode { get; set; }
        }

        public class Product
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public virtual List<RoutingCustomer> RoutingCustomers { get; set; }
        }

        public class SalesPerson
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public virtual List<VIP> ManagedRoutingCustomers { get; set; }
            public virtual List<ImportantProduct> ManagedProducts { get; set; }
        }

        public class VIP : RoutingCustomer
        {
            public virtual SalesPerson RelationshipManager { get; set; }
            public string Company { get; set; }
        }

        public class SpecialVIP : VIP
        {
            public Guid SpecialGuid { get; set; }
        }

        public class ImportantProduct : Product
        {
            public virtual SalesPerson LeadSalesPerson { get; set; }
        }

        public class üCategory
        {
            public int ID { get; set; }
        }

        public class EnumCustomer
        {
            public int ID { get; set; }
            public Color Color { get; set; }
        }

        public class DateTimeOffsetKeyCustomer
        {
            public DateTimeOffset ID { get; set; }
        }
    }
}