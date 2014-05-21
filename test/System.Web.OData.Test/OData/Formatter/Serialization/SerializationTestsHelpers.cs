// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.OData.Edm.Library.Values;

namespace System.Web.OData.Formatter.Serialization
{
    internal class SerializationTestsHelpers
    {
        public static IEdmModel SimpleCustomerOrderModel()
        {
            var model = new EdmModel();
            var customerType = new EdmEntityType("Default", "Customer");
            customerType.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32);
            customerType.AddStructuralProperty("FirstName", EdmPrimitiveTypeKind.String);
            customerType.AddStructuralProperty("LastName", EdmPrimitiveTypeKind.String);
            IEdmTypeReference primitiveTypeReference = EdmCoreModel.Instance.GetPrimitive(
                EdmPrimitiveTypeKind.String,
                isNullable: true);
            customerType.AddStructuralProperty(
                "City",
                primitiveTypeReference,
                defaultValue: null,
                concurrencyMode: EdmConcurrencyMode.Fixed);
            model.AddElement(customerType);

            var specialCustomerType = new EdmEntityType("Default", "SpecialCustomer", customerType);
            model.AddElement(specialCustomerType);

            var orderType = new EdmEntityType("Default", "Order");
            orderType.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32);
            orderType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            orderType.AddStructuralProperty("Shipment", EdmPrimitiveTypeKind.String);
            model.AddElement(orderType);

            var specialOrderType = new EdmEntityType("Default", "SpecialOrder", orderType);
            model.AddElement(specialOrderType);

            var addressType = new EdmComplexType("Default", "Address");
            addressType.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
            addressType.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
            addressType.AddStructuralProperty("State", EdmPrimitiveTypeKind.String);
            addressType.AddStructuralProperty("Country", EdmPrimitiveTypeKind.String);
            addressType.AddStructuralProperty("ZipCode", EdmPrimitiveTypeKind.String);
            model.AddElement(addressType);

            // Add navigations
            customerType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo() { Name = "Orders", Target = orderType, TargetMultiplicity = EdmMultiplicity.Many });
            orderType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo() { Name = "Customer", Target = customerType, TargetMultiplicity = EdmMultiplicity.One });
            specialCustomerType.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "SpecialOrders",
                    Target = specialOrderType,
                    TargetMultiplicity = EdmMultiplicity.Many
                });
            orderType.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "SpecialCustomer",
                    Target = specialCustomerType,
                    TargetMultiplicity = EdmMultiplicity.One
                });

            // Add Entity set
            var container = new EdmEntityContainer("Default", "Container");
            var customerSet = container.AddEntitySet("Customers", customerType);
            var orderSet = container.AddEntitySet("Orders", orderType);
            customerSet.AddNavigationTarget(customerType.NavigationProperties().Single(np => np.Name == "Orders"), orderSet);
            customerSet.AddNavigationTarget(
                specialCustomerType.NavigationProperties().Single(np => np.Name == "SpecialOrders"),
                orderSet);
            orderSet.AddNavigationTarget(orderType.NavigationProperties().Single(np => np.Name == "Customer"), customerSet);
            orderSet.AddNavigationTarget(
                specialOrderType.NavigationProperties().Single(np => np.Name == "SpecialCustomer"),
                customerSet);

            NavigationSourceLinkBuilderAnnotation linkAnnotation = new MockNavigationSourceLinkBuilderAnnotation();
            model.SetNavigationSourceLinkBuilder(customerSet, linkAnnotation);
            model.SetNavigationSourceLinkBuilder(orderSet, linkAnnotation);

            model.AddElement(container);
            return model;
        }

        public static IEdmModel SimpleOpenTypeModel()
        {
            var model = new EdmModel();

            // Address is an open complex type
            var addressType = new EdmComplexType("Default", "Address", null, false, true);
            addressType.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
            addressType.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
            model.AddElement(addressType);

            // ZipCode is an open complex type also
            var zipCodeType = new EdmComplexType("Default", "ZipCode", null, false, true);
            zipCodeType.AddStructuralProperty("Code", EdmPrimitiveTypeKind.Int32);
            model.AddElement(zipCodeType);

            // Enum type simpleEnum
            EdmEnumType simpleEnum = new EdmEnumType("Default", "SimpleEnum");
            simpleEnum.AddMember(new EdmEnumMember(simpleEnum, "First", new EdmIntegerConstant(0)));
            simpleEnum.AddMember(new EdmEnumMember(simpleEnum, "Second", new EdmIntegerConstant(1)));
            simpleEnum.AddMember(new EdmEnumMember(simpleEnum, "Third", new EdmIntegerConstant(2)));
            simpleEnum.AddMember(new EdmEnumMember(simpleEnum, "Fourth", new EdmIntegerConstant(3)));
            model.AddElement(simpleEnum);

            var container = new EdmEntityContainer("Default", "Container");
            model.AddElement(container);
            return model;
        }
    }
}
