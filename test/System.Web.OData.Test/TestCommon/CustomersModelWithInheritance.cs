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
            IEdmProperty customerName = customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            customer.AddStructuralProperty("Address", new EdmComplexTypeReference(address, isNullable: true));
            IEdmTypeReference primitiveTypeReference = EdmCoreModel.Instance.GetPrimitive(
                EdmPrimitiveTypeKind.String,
                isNullable: true);
            customer.AddStructuralProperty(
                "City",
                primitiveTypeReference,
                defaultValue: null,
                concurrencyMode: EdmConcurrencyMode.Fixed);
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

            // test entity
            EdmEntityType testEntity = new EdmEntityType("System.Web.Http.OData.Query.Expressions", "TestEntity");
            testEntity.AddStructuralProperty("SampleProperty", EdmPrimitiveTypeKind.Binary);
            model.AddElement(testEntity);

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

            // functions
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            EdmFunctionImport isCustomerUpgraded = container.AddFunctionImport(
                "IsUpgraded", returnType: returnType, entitySet: new EdmEntitySetReferenceExpression(customers), sideEffecting: false, composable: false, bindable: true);
            isCustomerUpgraded.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            EdmFunctionImport isSpecialCustomerUpgraded = container.AddFunctionImport(
                "IsSpecialUpgraded", returnType: returnType, entitySet: new EdmEntitySetReferenceExpression(customers), sideEffecting: false, composable: false, bindable: true);
            isSpecialCustomerUpgraded.AddParameter("entity", new EdmEntityTypeReference(specialCustomer, false));

            EdmFunctionImport isAnyCustomerUpgraded = container.AddFunctionImport(
                "IsAnyUpgraded", returnType: returnType, entitySet: new EdmEntitySetReferenceExpression(customers), sideEffecting: false, composable: false, bindable: true);
            EdmCollectionType edmCollectionType = new EdmCollectionType(new EdmEntityTypeReference(customer, false));
            isAnyCustomerUpgraded.AddParameter("entityset", new EdmCollectionTypeReference(edmCollectionType, false));

            EdmFunctionImport isCustomerUpgradedWithParam = container.AddFunctionImport(
                "IsUpgradedWithParam", returnType: returnType, entitySet: new EdmEntitySetReferenceExpression(customers), sideEffecting: false, composable: false, bindable: true);
            isCustomerUpgradedWithParam.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            isCustomerUpgradedWithParam.AddParameter("city", EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, isNullable: false));

            EdmFunctionImport isCustomerLocal = container.AddFunctionImport(
                "IsLocal", returnType: returnType, entitySet: new EdmEntitySetReferenceExpression(customers), sideEffecting: false, composable: false, bindable: true);
            isCustomerLocal.AddParameter("entity", new EdmEntityTypeReference(customer, false));

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
            CustomerName = customerName;
            IsCustomerUpgraded = isCustomerUpgraded;
            IsSpecialCustomerUpgraded = isSpecialCustomerUpgraded;
        }

        public EdmModel Model { get; private set; }

        public EdmEntityType Customer { get; private set; }

        public EdmEntityType SpecialCustomer { get; private set; }

        public EdmEntityType Order { get; private set; }

        public EdmEntityType SpecialOrder { get; private set; }

        public EdmComplexType Address { get; private set; }

        public EdmEntitySet Customers { get; private set; }

        public EdmEntitySet Orders { get; private set; }

        public EdmEntityContainer Container { get; private set; }

        public EdmFunctionImport UpgradeCustomer { get; private set; }

        public EdmFunctionImport UpgradeSpecialCustomer { get; private set; }

        public IEdmProperty CustomerName { get; private set; }

        public EdmFunctionImport IsCustomerUpgraded { get; private set; }

        public EdmFunctionImport IsSpecialCustomerUpgraded { get; private set; }
    }
}
