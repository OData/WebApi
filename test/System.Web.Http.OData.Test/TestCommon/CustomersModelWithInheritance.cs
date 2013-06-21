// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Library.Expressions;

namespace System.Web.Http.TestCommon
{
    public class CustomersModelWithInheritance
    {
        public CustomersModelWithInheritance()
        {
            EdmModel model = new EdmModel();

            // complex type address
            EdmComplexType address = new EdmComplexType("NS", "Address");
            address.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
            address.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
            address.AddStructuralProperty("State", EdmPrimitiveTypeKind.String);
            address.AddStructuralProperty("ZipCode", EdmPrimitiveTypeKind.String);
            address.AddStructuralProperty("Country", EdmPrimitiveTypeKind.String);
            model.AddElement(address);

            // entity type customer
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            customer.AddStructuralProperty("Address", new EdmComplexTypeReference(address, isNullable: true));
            model.AddElement(customer);

            // derived entity type special customer
            EdmEntityType specialCustomer = new EdmEntityType("NS", "SpecialCustomer", customer);
            specialCustomer.AddStructuralProperty("SpecialCustomerProperty", EdmPrimitiveTypeKind.Guid);
            model.AddElement(specialCustomer);

            // entity type order
            EdmEntityType order = new EdmEntityType("NS", "Order");
            order.AddKeys(order.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            order.AddStructuralProperty("Amount", EdmPrimitiveTypeKind.Int32);
            model.AddElement(order);

            // derived entity type special order
            EdmEntityType specialOrder = new EdmEntityType("NS", "SpecialOrder", order);
            specialOrder.AddStructuralProperty("SpecialOrderProperty", EdmPrimitiveTypeKind.Guid);
            model.AddElement(specialOrder);

            // entity sets
            EdmEntityContainer container = new EdmEntityContainer("NS", "ModelWithInheritance");
            model.AddElement(container);
            EdmEntitySet customers = container.AddEntitySet("Customers", customer);
            EdmEntitySet orders = container.AddEntitySet("Orders", order);

            // actions
            EdmFunctionImport upgradeCustomer = container.AddFunctionImport(
                "upgrade", returnType: null, entitySet: new EdmEntitySetReferenceExpression(customers), sideEffecting: true, composable: false, bindable: true);
            upgradeCustomer.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            EdmFunctionImport upgradeSpecialCustomer = container.AddFunctionImport(
                "specialUpgrade", returnType: null, entitySet: new EdmEntitySetReferenceExpression(customers), sideEffecting: true, composable: false, bindable: true);
            upgradeSpecialCustomer.AddParameter("entity", new EdmEntityTypeReference(specialCustomer, false));

            // navigation properties
            customers.AddNavigationTarget(
                customer.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
                {
                    Name = "Orders",
                    TargetMultiplicity = EdmMultiplicity.Many,
                    Target = order
                }),
                orders);
            orders.AddNavigationTarget(
                 order.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
                 {
                     Name = "Customer",
                     TargetMultiplicity = EdmMultiplicity.ZeroOrOne,
                     Target = customer
                 }),
                customers);

            // navigation properties on derived types.
            customers.AddNavigationTarget(
                specialCustomer.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
                {
                    Name = "SpecialOrders",
                    TargetMultiplicity = EdmMultiplicity.Many,
                    Target = order
                }),
                orders);
            orders.AddNavigationTarget(
                 specialOrder.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
                 {
                     Name = "SpecialCustomer",
                     TargetMultiplicity = EdmMultiplicity.ZeroOrOne,
                     Target = customer
                 }),
                customers);
            model.SetAnnotationValue<BindableProcedureFinder>(model, new BindableProcedureFinder(model));

            // set properties
            Model = model;
            Container = container;
            Customer = customer;
            Order = order;
            Address = address;
            SpecialCustomer = specialCustomer;
            SpecialOrder = specialOrder;
            Orders = orders;
            Customers = customers;
            UpgradeCustomer = upgradeCustomer;
            UpgradeSpecialCustomer = upgradeSpecialCustomer;
        }

        public IEdmModel Model { get; private set; }

        public IEdmEntityType Customer { get; private set; }

        public IEdmEntityType SpecialCustomer { get; private set; }

        public IEdmEntityType Order { get; private set; }

        public IEdmEntityType SpecialOrder { get; private set; }

        public IEdmComplexType Address { get; private set; }

        public IEdmEntitySet Customers { get; private set; }

        public IEdmEntitySet Orders { get; private set; }

        public IEdmEntityContainer Container { get; private set; }

        public IEdmFunctionImport UpgradeCustomer { get; private set; }

        public IEdmFunctionImport UpgradeSpecialCustomer { get; private set; }
    }
}
