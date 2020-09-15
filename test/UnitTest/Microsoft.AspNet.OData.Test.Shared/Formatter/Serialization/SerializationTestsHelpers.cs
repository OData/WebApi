// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Builder;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization
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
            IEdmStructuralProperty city = customerType.AddStructuralProperty(
                "City",
                primitiveTypeReference,
                defaultValue: null);
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
            addressType.AddStructuralProperty("CountryOrRegion", EdmPrimitiveTypeKind.String);
            addressType.AddStructuralProperty("ZipCode", EdmPrimitiveTypeKind.String);
            model.AddElement(addressType);

            // add a derived complex type "UsAddress"
            var usAddressType = new EdmComplexType("Default", "UsAddress", addressType);
            usAddressType.AddStructuralProperty("UsProp", EdmPrimitiveTypeKind.String);
            model.AddElement(usAddressType);

            // add a derived complex type "CnAddress"
            var cnAddressType = new EdmComplexType("Default", "CnAddress", addressType);
            cnAddressType.AddStructuralProperty("CnProp", EdmPrimitiveTypeKind.Guid);
            model.AddElement(cnAddressType);

            // add a complex type "Location" with complex type property
            var location = new EdmComplexType("Default", "Location");
            location.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            location.AddStructuralProperty("Address", new EdmComplexTypeReference(addressType, isNullable: true));
            model.AddElement(location);

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

            // Add contained navigation
            // Reviews
            EdmEntityType customerReview = new EdmEntityType("Default", "Review");
            customerReview.AddKeys(customerReview.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            customerReview.AddStructuralProperty("Comment", EdmPrimitiveTypeKind.String);
            model.AddElement(customerReview);

            EdmNavigationProperty orderLinesNavProp = customerType.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "Reviews",
                    TargetMultiplicity = EdmMultiplicity.Many,
                    Target = customerReview,
                    ContainsTarget = true,
                });

            // Add Entity set
            var container = new EdmEntityContainer("Default", "Container");
            var customerSet = container.AddEntitySet("Customers", customerType);
            model.SetOptimisticConcurrencyAnnotation(customerSet, new[] { city });
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
            simpleEnum.AddMember(new EdmEnumMember(simpleEnum, "First", new EdmEnumMemberValue(0)));
            simpleEnum.AddMember(new EdmEnumMember(simpleEnum, "Second", new EdmEnumMemberValue(1)));
            simpleEnum.AddMember(new EdmEnumMember(simpleEnum, "Third", new EdmEnumMemberValue(2)));
            simpleEnum.AddMember(new EdmEnumMember(simpleEnum, "Fourth", new EdmEnumMemberValue(3)));
            model.AddElement(simpleEnum);

            // Customer is an open entity type
            var customerType = new EdmEntityType("Default", "Customer", null, false, true);
            customerType.AddKeys(customerType.AddStructuralProperty("CustomerId", EdmPrimitiveTypeKind.Int32));
            customerType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            customerType.AddStructuralProperty("Address", addressType.ToEdmTypeReference(false));
            model.AddElement(customerType);

            var container = new EdmEntityContainer("Default", "Container");
            model.AddElement(container);

            var customers = new EdmEntitySet(container, "Customers", customerType);
            container.AddElement(customers);
            return model;
        }
    }
}
