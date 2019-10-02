// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.OData.Edm;

namespace Microsoft.Test.E2E.AspNet.OData.AlternateKeys
{
    public static class AlternateKeysDataSource
    {
        public static EdmEntityObjectCollection Customers { get; private set; }

        public static EdmEntityObjectCollection Orders { get; private set; }

        public static EdmEntityObjectCollection People { get; private set; }

        public static EdmEntityObjectCollection Companies { get; private set; }

        static AlternateKeysDataSource()
        {
            IEdmModel model = AlternateKeysEdmModel.GetEdmModel();

            BuildCustomers(model);

            BuildOrderss(model);

            BuildPeople(model);

            BuildCompanies(model);
        }

        private static void BuildCustomers(IEdmModel model)
        {
            IEdmEntityType customerType = model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "Customer");

            IEdmEntityObject[] untypedCustomers = new IEdmEntityObject[6];
            for (int i = 1; i <= 5; i++)
            {
                dynamic untypedCustomer = new EdmEntityObject(customerType);
                untypedCustomer.ID = i;
                untypedCustomer.Name = string.Format("Name {0}", i);
                untypedCustomer.SSN = "SSN-" + i + "-" + (100 + i);
                untypedCustomers[i-1] = untypedCustomer;
            }

            // create a special customer for "PATCH"
            dynamic customer = new EdmEntityObject(customerType);
            customer.ID = 6;
            customer.Name = "Name 6";
            customer.SSN = "SSN-6-T-006";
            untypedCustomers[5] = customer;

            IEdmCollectionTypeReference entityCollectionType =
                new EdmCollectionTypeReference(
                    new EdmCollectionType(
                        new EdmEntityTypeReference(customerType, isNullable: false)));

            Customers = new EdmEntityObjectCollection(entityCollectionType, untypedCustomers.ToList());
        }

        private static void BuildOrderss(IEdmModel model)
        {
            IEdmEntityType orderType = model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "Order");

            Guid[] guids =
            {
                new Guid("196B3584-EF3D-41FD-90B4-76D59F9B929C"),
                new Guid("6CED5600-28BA-40EE-A2DF-E80AFADBE6C7"),
                new Guid("75036B94-C836-4946-8CC8-054CF54060EC"),
                new Guid("B3FF5460-6E77-4678-B959-DCC1C4937FA7"),
                new Guid("ED773C85-4E3C-4FC4-A3E9-9F1DA0A626DA")
            };

            IEdmEntityObject[] untypedOrders = new IEdmEntityObject[5];
            for (int i = 0; i < 5; i++)
            {
                dynamic untypedOrder = new EdmEntityObject(orderType);
                untypedOrder.OrderId = i;
                untypedOrder.Name = string.Format("Order-{0}", i);
                untypedOrder.Token = guids[i];
                untypedOrder.Amount = 10 + i;
                untypedOrders[i] = untypedOrder;
            }

            IEdmCollectionTypeReference entityCollectionType =
                new EdmCollectionTypeReference(
                    new EdmCollectionType(
                        new EdmEntityTypeReference(orderType, isNullable: false)));

            Orders = new EdmEntityObjectCollection(entityCollectionType, untypedOrders.ToList());
        }

        private static void BuildPeople(IEdmModel model)
        {
            IEdmEntityType personType = model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "Person");

            IEdmEntityObject[] untypedPeople = new IEdmEntityObject[5];
            for (int i = 0; i < 5; i++)
            {
                dynamic untypedPerson = new EdmEntityObject(personType);
                untypedPerson.ID = i;
                untypedPerson.Country_Region = new[] { "CountryRegion1", "China", "United States", "Russia", "Japan" }[i];
                untypedPerson.Passport = new[] { "1001", "2010", "9999", "3199992", "00001"}[i];
                untypedPeople[i] = untypedPerson;
            }

            IEdmCollectionTypeReference entityCollectionType =
                new EdmCollectionTypeReference(
                    new EdmCollectionType(
                        new EdmEntityTypeReference(personType, isNullable: false)));

            People = new EdmEntityObjectCollection(entityCollectionType, untypedPeople.ToList());
        }

        private static void BuildCompanies(IEdmModel model)
        {
            IEdmEntityType companyType = model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "Company");

            IList<IEdmComplexObject> addresses = BuildAddrsses(model);

            IEdmEntityObject[] untypedCompanies = new IEdmEntityObject[5];
            for (int i = 0; i < 5; i++)
            {
                dynamic untypedCompany = new EdmEntityObject(companyType);
                untypedCompany.ID = i;
                untypedCompany.Location = addresses[i];
                untypedCompanies[i] = untypedCompany;
            }

            IEdmCollectionTypeReference entityCollectionType =
                new EdmCollectionTypeReference(
                    new EdmCollectionType(
                        new EdmEntityTypeReference(companyType, isNullable: false)));

            Companies = new EdmEntityObjectCollection(entityCollectionType, untypedCompanies.ToList());
        }

        private static IList<IEdmComplexObject> BuildAddrsses(IEdmModel model)
        {
            IEdmComplexType addressType = model.SchemaElements.OfType<IEdmComplexType>().First(e => e.Name == "Address");

            return Enumerable.Range(1, 5).Select(e =>
            {
                dynamic address = new EdmComplexObject(addressType);
                address.Street = new[] {"Fuxing Rd", "Zixing Rd", "Xiaoxiang Rd", "Kehua Rd", "Taoyuan Rd"}[e - 1];
                address.City = new[] {"Beijing", "Shanghai", "Guangzhou", "Chengdu", "Wuhan"}[e - 1];
                return address as IEdmComplexObject;
            }).ToList();
        }
    }
}
