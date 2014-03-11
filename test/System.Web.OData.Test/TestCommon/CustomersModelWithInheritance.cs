// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.OData.Builder;
using System.Web.OData.Formatter;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.OData.Edm.Library.Expressions;
using Microsoft.OData.Edm.Library.Values;

namespace System.Web.OData.TestCommon
{
    public class CustomersModelWithInheritance
    {
        public CustomersModelWithInheritance()
        {
            EdmModel model = new EdmModel();

            // Enum type simpleEnum
            EdmEnumType simpleEnum = new EdmEnumType("NS", "SimpleEnum");
            simpleEnum.AddMember(new EdmEnumMember(simpleEnum, "First", new EdmIntegerConstant(0)));
            simpleEnum.AddMember(new EdmEnumMember(simpleEnum, "Second", new EdmIntegerConstant(1)));
            simpleEnum.AddMember(new EdmEnumMember(simpleEnum, "Third", new EdmIntegerConstant(2)));
            model.AddElement(simpleEnum);

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
            customer.AddStructuralProperty("SimpleEnum", simpleEnum.ToEdmTypeReference(isNullable: false));
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
            specialCustomer.AddStructuralProperty("SpecialAddress", new EdmComplexTypeReference(address, isNullable: true));
            model.AddElement(specialCustomer);

            // entity type order
            EdmEntityType order = new EdmEntityType("NS", "Order");
            order.AddKeys(order.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            order.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
            order.AddStructuralProperty("Amount", EdmPrimitiveTypeKind.Int32);
            model.AddElement(order);

            // derived entity type special order
            EdmEntityType specialOrder = new EdmEntityType("NS", "SpecialOrder", order);
            specialOrder.AddStructuralProperty("SpecialOrderProperty", EdmPrimitiveTypeKind.Guid);
            model.AddElement(specialOrder);

            // test entity
            EdmEntityType testEntity = new EdmEntityType("System.Web.OData.Query.Expressions", "TestEntity");
            testEntity.AddStructuralProperty("SampleProperty", EdmPrimitiveTypeKind.Binary);
            model.AddElement(testEntity);

            // containment
            // my order
            EdmEntityType myOrder = new EdmEntityType("NS", "MyOrder");
            myOrder.AddKeys(myOrder.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            myOrder.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            model.AddElement(myOrder);

            // order line
            EdmEntityType orderLine = new EdmEntityType("NS", "OrderLine");
            orderLine.AddKeys(orderLine.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            orderLine.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            model.AddElement(orderLine);

            EdmNavigationProperty orderLinesNavProp = myOrder.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "OrderLines",
                    TargetMultiplicity = EdmMultiplicity.Many,
                    Target = orderLine,
                    ContainsTarget = true,
                });

            EdmAction tag = new EdmAction("NS", "tag", returnType: null, isBound: true, entitySetPathExpression: null);
            tag.AddParameter("entity", new EdmEntityTypeReference(orderLine, false));
            model.AddElement(tag);
            
            // entity sets
            EdmEntityContainer container = new EdmEntityContainer("NS", "ModelWithInheritance");
            model.AddElement(container);
            EdmEntitySet customers = container.AddEntitySet("Customers", customer);
            EdmEntitySet orders = container.AddEntitySet("Orders", order);
            EdmEntitySet myOrders = container.AddEntitySet("MyOrders", myOrder);

            // singletons
            EdmSingleton vipCustomer = container.AddSingleton("VipCustomer", customer);
            EdmSingleton mary = container.AddSingleton("Mary", customer);

            // containment
            IEdmContainedEntitySet orderLines = (IEdmContainedEntitySet)myOrders.FindNavigationTarget(orderLinesNavProp);

            // actions
            EdmAction upgrade = new EdmAction("NS", "upgrade", returnType: null, isBound: true, entitySetPathExpression: null);
            upgrade.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            model.AddElement(upgrade);

            EdmAction specialUpgrade =
                new EdmAction("NS", "specialUpgrade", returnType: null, isBound: true, entitySetPathExpression: null);
            specialUpgrade.AddParameter("entity", new EdmEntityTypeReference(specialCustomer, false));
            model.AddElement(specialUpgrade);

            // functions
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmTypeReference stringType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, isNullable: false);
            IEdmTypeReference intType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false);

            EdmFunction IsUpgraded = new EdmFunction(
                "NS",
                "IsUpgraded",
                returnType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: false);
            IsUpgraded.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            model.AddElement(IsUpgraded);

            EdmFunction orderByCityAndAmount = new EdmFunction(
                "NS",
                "OrderByCityAndAmount",
                stringType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: false);
            orderByCityAndAmount.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            orderByCityAndAmount.AddParameter("city", stringType);
            orderByCityAndAmount.AddParameter("amount", intType);
            model.AddElement(orderByCityAndAmount);

            EdmFunction IsSpecialUpgraded = new EdmFunction(
                "NS",
                "IsSpecialUpgraded",
                returnType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: false);
            IsSpecialUpgraded.AddParameter("entity", new EdmEntityTypeReference(specialCustomer, false));
            model.AddElement(IsSpecialUpgraded);

            EdmFunction getSalary = new EdmFunction(
                "NS",
                "GetSalary",
                stringType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: false);
            getSalary.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            model.AddElement(getSalary);

            getSalary = new EdmFunction(
                "NS",
                "GetSalary",
                stringType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: false);
            getSalary.AddParameter("entity", new EdmEntityTypeReference(specialCustomer, false));
            model.AddElement(getSalary);

            EdmFunction IsAnyUpgraded = new EdmFunction(
                "NS",
                "IsAnyUpgraded",
                returnType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: false);
            EdmCollectionType edmCollectionType = new EdmCollectionType(new EdmEntityTypeReference(customer, false));
            IsAnyUpgraded.AddParameter("entityset", new EdmCollectionTypeReference(edmCollectionType));
            model.AddElement(IsAnyUpgraded);

            EdmFunction isCustomerUpgradedWithParam = new EdmFunction(
                "NS",
                "IsUpgradedWithParam",
                returnType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: false);
            isCustomerUpgradedWithParam.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            isCustomerUpgradedWithParam.AddParameter("city", EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, isNullable: false));
            model.AddElement(isCustomerUpgradedWithParam);

            EdmFunction isCustomerLocal = new EdmFunction(
                "NS",
                "IsLocal",
                returnType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: false);
            isCustomerLocal.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            model.AddElement(isCustomerLocal);

            // navigation properties
            EdmNavigationProperty ordersNavProp = customer.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "Orders",
                    TargetMultiplicity = EdmMultiplicity.Many,
                    Target = order
                });
            mary.AddNavigationTarget(ordersNavProp, orders);
            vipCustomer.AddNavigationTarget(ordersNavProp, orders);
            customers.AddNavigationTarget(ordersNavProp, orders);
            orders.AddNavigationTarget(
                 order.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
                 {
                     Name = "Customer",
                     TargetMultiplicity = EdmMultiplicity.ZeroOrOne,
                     Target = customer
                 }),
                customers);

            // navigation properties on derived types.
            EdmNavigationProperty specialOrdersNavProp = specialCustomer.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "SpecialOrders",
                    TargetMultiplicity = EdmMultiplicity.Many,
                    Target = order
                });
            vipCustomer.AddNavigationTarget(specialOrdersNavProp, orders);
            customers.AddNavigationTarget(specialOrdersNavProp, orders);
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
            VipCustomer = vipCustomer;
            Mary = mary;
            OrderLine = orderLine;
            OrderLines = orderLines;
            UpgradeCustomer = upgrade;
            UpgradeSpecialCustomer = specialUpgrade;
            CustomerName = customerName;
            IsCustomerUpgraded = isCustomerUpgradedWithParam;
            IsSpecialCustomerUpgraded = IsSpecialUpgraded;
            Tag = tag;
        }

        public EdmModel Model { get; private set; }

        public EdmEntityType Customer { get; private set; }

        public EdmEntityType SpecialCustomer { get; private set; }

        public EdmEntityType Order { get; private set; }

        public EdmEntityType SpecialOrder { get; private set; }

        public EdmEntityType OrderLine { get; private set; }

        public EdmComplexType Address { get; private set; }

        public EdmEntitySet Customers { get; private set; }

        public EdmEntitySet Orders { get; private set; }

        public EdmSingleton VipCustomer { get; private set; }

        public EdmSingleton Mary { get; private set; }

        public IEdmContainedEntitySet OrderLines { get; private set; }

        public EdmEntityContainer Container { get; private set; }

        public EdmAction UpgradeCustomer { get; private set; }

        public EdmAction UpgradeSpecialCustomer { get; private set; }

        public EdmAction Tag { get; private set; }
        
        public IEdmProperty CustomerName { get; private set; }

        public EdmFunction IsCustomerUpgraded { get; private set; }

        public EdmFunction IsSpecialCustomerUpgraded { get; private set; }
    }
}
