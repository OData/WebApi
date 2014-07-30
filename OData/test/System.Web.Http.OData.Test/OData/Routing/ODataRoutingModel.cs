// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http.Dispatcher;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.TestCommon;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Routing
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

            ActionConfiguration getRoutingCustomerById = builder.Action("GetRoutingCustomerById");
            getRoutingCustomerById.Parameter<int>("RoutingCustomerId");
            getRoutingCustomerById.ReturnsFromEntitySet<RoutingCustomer>("RoutingCustomers");

            ActionConfiguration getSalesPersonById = builder.Action("GetSalesPersonById");
            getSalesPersonById.Parameter<int>("salesPersonId");
            getSalesPersonById.ReturnsFromEntitySet<SalesPerson>("SalesPeople");

            ActionConfiguration getAllVIPs = builder.Action("GetAllVIPs");
            ActionReturnsCollectionFromEntitySet<VIP>(builder, getAllVIPs, "RoutingCustomers");

            builder.Entity<RoutingCustomer>().ComplexProperty<Address>(c => c.Address);
            builder.Entity<RoutingCustomer>().Action("GetRelatedRoutingCustomers").ReturnsCollectionFromEntitySet<RoutingCustomer>("RoutingCustomers");

            ActionConfiguration getBestRelatedRoutingCustomer = builder.Entity<RoutingCustomer>().Action("GetBestRelatedRoutingCustomer");
            ActionReturnsFromEntitySet<VIP>(builder, getBestRelatedRoutingCustomer, "RoutingCustomers");

            ActionConfiguration getVIPS = builder.Entity<RoutingCustomer>().Collection.Action("GetVIPs");
            ActionReturnsCollectionFromEntitySet<VIP>(builder, getVIPS, "RoutingCustomers");

            builder.Entity<RoutingCustomer>().Collection.Action("GetProducts").ReturnsCollectionFromEntitySet<Product>("Products");
            builder.Entity<VIP>().Action("GetSalesPerson").ReturnsFromEntitySet<SalesPerson>("SalesPeople");
            builder.Entity<VIP>().Collection.Action("GetSalesPeople").ReturnsCollectionFromEntitySet<SalesPerson>("SalesPeople");

            ActionConfiguration getMostProfitable = builder.Entity<VIP>().Collection.Action("GetMostProfitable");
            ActionReturnsFromEntitySet<VIP>(builder, getMostProfitable, "RoutingCustomers");

            ActionConfiguration getVIPRoutingCustomers = builder.Entity<SalesPerson>().Action("GetVIPRoutingCustomers");
            ActionReturnsCollectionFromEntitySet<VIP>(builder, getVIPRoutingCustomers, "RoutingCustomers");

            ActionConfiguration getVIPRoutingCustomersOnCollection = builder.Entity<SalesPerson>().Collection.Action("GetVIPRoutingCustomers");
            ActionReturnsCollectionFromEntitySet<VIP>(builder, getVIPRoutingCustomersOnCollection, "RoutingCustomers");

            builder.Entity<VIP>().HasRequired(v => v.RelationshipManager);
            builder.Entity<ImportantProduct>().HasRequired(ip => ip.LeadSalesPerson);

            return builder.GetEdmModel();
        }

        public static ActionConfiguration ActionReturnsFromEntitySet<TEntityType>(ODataModelBuilder builder, ActionConfiguration action, string entitySetName) where TEntityType : class
        {
            action.EntitySet = CreateOrReuseEntitySet<TEntityType>(builder, entitySetName);
            action.ReturnType = builder.GetTypeConfigurationOrNull(typeof(TEntityType));
            return action;
        }

        public static ActionConfiguration ActionReturnsCollectionFromEntitySet<TElementEntityType>(ODataModelBuilder builder, ActionConfiguration action, string entitySetName) where TElementEntityType : class
        {
            Type clrCollectionType = typeof(IEnumerable<TElementEntityType>);
            action.EntitySet = CreateOrReuseEntitySet<TElementEntityType>(builder, entitySetName);
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
                builder.Entity<TElementEntityType>();
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

        public class ImportantProduct : Product
        {
            public virtual SalesPerson LeadSalesPerson { get; set; }
        }

        public class üCategory
        {
            public int ID { get; set; }
        }
    }
}