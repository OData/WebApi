// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;

namespace System.Web.Http.OData.Formatter.Serialization
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
            model.AddElement(customerType);

            var orderType = new EdmEntityType("Default", "Order");
            orderType.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32);
            orderType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            model.AddElement(orderType);

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

            var container = new EdmEntityContainer("Default", "Container");
            var customerSet = container.AddEntitySet("Customers", customerType);
            var orderSet = container.AddEntitySet("Orders", orderType);
            customerSet.AddNavigationTarget(customerType.NavigationProperties().Single(np => np.Name == "Orders"), orderSet);
            orderSet.AddNavigationTarget(orderType.NavigationProperties().Single(np => np.Name == "Customer"), customerSet);

            EntitySetLinkBuilderAnnotation linkAnnotation = new MockEntitySetLinkBuilderAnnotation();
            model.SetEntitySetLinkBuilder(customerSet, linkAnnotation);
            model.SetEntitySetLinkBuilder(orderSet, linkAnnotation);

            model.AddElement(container);
            return model;
        }
    }
}
