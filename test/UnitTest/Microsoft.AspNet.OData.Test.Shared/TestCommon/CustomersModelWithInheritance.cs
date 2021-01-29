// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Test.Common
{
    public class CustomersModelWithInheritance
    {
        public CustomersModelWithInheritance()
        {
            EdmModel model = new EdmModel();

            // Enum type simpleEnum
            EdmEnumType simpleEnum = new EdmEnumType("NS", "SimpleEnum");
            simpleEnum.AddMember(new EdmEnumMember(simpleEnum, "First", new EdmEnumMemberValue(0)));
            simpleEnum.AddMember(new EdmEnumMember(simpleEnum, "Second", new EdmEnumMemberValue(1)));
            simpleEnum.AddMember(new EdmEnumMember(simpleEnum, "Third", new EdmEnumMemberValue(2)));
            model.AddElement(simpleEnum);

            // complex type address
            EdmComplexType address = new EdmComplexType("NS", "Address");
            address.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
            address.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
            address.AddStructuralProperty("State", EdmPrimitiveTypeKind.String);
            address.AddStructuralProperty("ZipCode", EdmPrimitiveTypeKind.String);
            address.AddStructuralProperty("CountryOrRegion", EdmPrimitiveTypeKind.String);
            model.AddElement(address);

            // open complex type "Account"
            EdmComplexType account = new EdmComplexType("NS", "Account", null, false, true);
            account.AddStructuralProperty("Bank", EdmPrimitiveTypeKind.String);
            account.AddStructuralProperty("CardNum", EdmPrimitiveTypeKind.Int64);
            account.AddStructuralProperty("BankAddress", new EdmComplexTypeReference(address, isNullable: true));
            model.AddElement(account);

            EdmComplexType specialAccount = new EdmComplexType("NS", "SpecialAccount", account, false, true);
            specialAccount.AddStructuralProperty("SpecialCard", EdmPrimitiveTypeKind.String);
            model.AddElement(specialAccount);

            // entity type customer
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            IEdmProperty customerName = customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            customer.AddStructuralProperty("SimpleEnum", simpleEnum.ToEdmTypeReference(isNullable: false));
            customer.AddStructuralProperty("Address", new EdmComplexTypeReference(address, isNullable: true));
            customer.AddStructuralProperty("Account", new EdmComplexTypeReference(account, isNullable: true));
            customer.AddStructuralProperty("OtherAccounts", new EdmCollectionTypeReference(new EdmCollectionType(new EdmComplexTypeReference(account, true))));
            IEdmTypeReference primitiveTypeReference = EdmCoreModel.Instance.GetPrimitive(
                EdmPrimitiveTypeKind.String,
                isNullable: true);
            var city = customer.AddStructuralProperty(
                "City",
                primitiveTypeReference,
                defaultValue: null);
            model.AddElement(customer);

            // derived entity type special customer
            EdmEntityType specialCustomer = new EdmEntityType("NS", "SpecialCustomer", customer);
            specialCustomer.AddStructuralProperty("SpecialCustomerProperty", EdmPrimitiveTypeKind.Guid);
            specialCustomer.AddStructuralProperty("SpecialAddress", new EdmComplexTypeReference(address, isNullable: true));
            model.AddElement(specialCustomer);

            // entity type order (open entity type)
            EdmEntityType order = new EdmEntityType("NS", "Order", null, false, true);
            // EdmEntityType order = new EdmEntityType("NS", "Order");
            order.AddKeys(order.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            order.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
            order.AddStructuralProperty("Amount", EdmPrimitiveTypeKind.Int32);
            model.AddElement(order);

            // derived entity type special order
            EdmEntityType specialOrder = new EdmEntityType("NS", "SpecialOrder", order, false, true);
            specialOrder.AddStructuralProperty("SpecialOrderProperty", EdmPrimitiveTypeKind.Guid);
            model.AddElement(specialOrder);

            // test entity
            EdmEntityType testEntity = new EdmEntityType("Microsoft.AspNet.OData.Test.Query.Expressions", "TestEntity");
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

           EdmNavigationProperty nonContainedOrderLinesNavProp = myOrder.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "NonContainedOrderLines",
                    TargetMultiplicity = EdmMultiplicity.Many,
                    Target = orderLine,
                    ContainsTarget = false,
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
            EdmSingleton rootOrder = container.AddSingleton("RootOrder", order);

            // annotations
            model.SetOptimisticConcurrencyAnnotation(customers, new[] { city });

            // containment
            IEdmContainedEntitySet orderLines = (IEdmContainedEntitySet)myOrders.FindNavigationTarget(orderLinesNavProp);

            // no-containment
            IEdmNavigationSource nonContainedOrderLines = myOrders.FindNavigationTarget(nonContainedOrderLinesNavProp);

            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmTypeReference stringType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, isNullable: false);
            IEdmTypeReference intType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false);

            // actions
            EdmAction upgrade = new EdmAction("NS", "upgrade", returnType: null, isBound: true, entitySetPathExpression: null);
            upgrade.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            model.AddElement(upgrade);

            EdmAction specialUpgrade =
                new EdmAction("NS", "specialUpgrade", returnType: null, isBound: true, entitySetPathExpression: null);
            specialUpgrade.AddParameter("entity", new EdmEntityTypeReference(specialCustomer, false));
            model.AddElement(specialUpgrade);

            // actions bound to collection
            EdmAction upgradeAll = new EdmAction("NS", "UpgradeAll", returnType: null, isBound: true, entitySetPathExpression: null);
            upgradeAll.AddParameter("entityset",
                new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            model.AddElement(upgradeAll);

            EdmAction upgradeSpecialAll = new EdmAction("NS", "UpgradeSpecialAll", returnType: null, isBound: true, entitySetPathExpression: null);
            upgradeSpecialAll.AddParameter("entityset",
                new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(specialCustomer, false))));
            model.AddElement(upgradeSpecialAll);

            // action with optional parameters
            EdmAction updateSalaray = new EdmAction("NS", "UpdateSalaray", returnType: null, isBound: true, entitySetPathExpression: null);
            updateSalaray.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            updateSalaray.AddParameter("minSalaray", intType);
            updateSalaray.AddOptionalParameter("maxSalaray", intType);
            updateSalaray.AddOptionalParameter("aveSalaray", intType, "129");

            // functions
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

            EdmFunction getOrders = new EdmFunction(
                "NS",
                "GetOrders",
                EdmCoreModel.GetCollection(order.ToEdmTypeReference(false)),
                isBound: true,
                entitySetPathExpression: null,
                isComposable: true);
            getOrders.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            getOrders.AddParameter("parameter", intType);
            model.AddElement(getOrders);

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

            EdmFunction entityFunction = new EdmFunction(
                "NS",
                "GetCustomer",
                returnType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: false);
            entityFunction.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            entityFunction.AddParameter("customer", new EdmEntityTypeReference(customer, false));
            model.AddElement(entityFunction);

            EdmFunction getOrder = new EdmFunction(
                "NS",
                "GetOrder",
                order.ToEdmTypeReference(false),
                isBound: true,
                entitySetPathExpression: null,
                isComposable: true); // Composable
            getOrder.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            getOrder.AddParameter("orderId", intType);
            model.AddElement(getOrder);

            // functions bound to collection
            EdmFunction isAllUpgraded = new EdmFunction("NS", "IsAllUpgraded", returnType, isBound: true,
                entitySetPathExpression: null, isComposable: false);
            isAllUpgraded.AddParameter("entityset",
                new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            isAllUpgraded.AddParameter("param", intType);
            model.AddElement(isAllUpgraded);

            EdmFunction isSpecialAllUpgraded = new EdmFunction("NS", "IsSpecialAllUpgraded", returnType, isBound: true,
                entitySetPathExpression: null, isComposable: false);
            isSpecialAllUpgraded.AddParameter("entityset",
                new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(specialCustomer, false))));
            isSpecialAllUpgraded.AddParameter("param", intType);
            model.AddElement(isSpecialAllUpgraded);

            // function with optional parameters
            EdmFunction getSalaray = new EdmFunction("NS", "GetWholeSalary", intType, isBound: true, entitySetPathExpression: null, isComposable: false);
            getSalaray.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            getSalaray.AddParameter("minSalary", intType);
            getSalaray.AddOptionalParameter("maxSalary", intType);
            getSalaray.AddOptionalParameter("aveSalary", intType, "129");
            model.AddElement(getSalaray);

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
            model.SetAnnotationValue<BindableOperationFinder>(model, new BindableOperationFinder(model));

            // set properties
            Model = model;
            Container = container;
            Customer = customer;
            Order = order;
            MyOrder = myOrder;
            Address = address;
            Account = account;
            SpecialCustomer = specialCustomer;
            SpecialOrder = specialOrder;
            Orders = orders;
            MyOrders = myOrders;
            Customers = customers;
            VipCustomer = vipCustomer;
            Mary = mary;
            RootOrder = rootOrder;
            OrderLine = orderLine;
            OrderLines = orderLines;
            NonContainedOrderLines = nonContainedOrderLines;
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
        public EdmEntityType MyOrder { get; private set; }

        public EdmEntityType SpecialOrder { get; private set; }

        public EdmEntityType OrderLine { get; private set; }

        public EdmComplexType Address { get; private set; }

        public EdmComplexType Account { get; private set; } // Open Complex Type

        public EdmEntitySet Customers { get; private set; }

        public EdmEntitySet Orders { get; private set; }
        public EdmEntitySet MyOrders { get; private set; }

        public EdmSingleton VipCustomer { get; private set; }

        public EdmSingleton Mary { get; private set; }

        public EdmSingleton RootOrder { get; private set; }

        public IEdmContainedEntitySet OrderLines { get; private set; }

        public IEdmNavigationSource NonContainedOrderLines { get; private set; }

        public EdmEntityContainer Container { get; private set; }

        public EdmAction UpgradeCustomer { get; private set; }

        public EdmAction UpgradeSpecialCustomer { get; private set; }

        public EdmAction Tag { get; private set; }

        public IEdmProperty CustomerName { get; private set; }

        public EdmFunction IsCustomerUpgraded { get; private set; }

        public EdmFunction IsSpecialCustomerUpgraded { get; private set; }
    }
}
