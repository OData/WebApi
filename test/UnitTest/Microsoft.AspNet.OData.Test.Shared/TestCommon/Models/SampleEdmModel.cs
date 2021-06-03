//-----------------------------------------------------------------------------
// <copyright file="SampleEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Test.Common.Models
{
    public class SampleEdmModel
    {
        public EdmNavigationProperty friendsProperty;
        public EdmEntityType customerType;
        public EdmEntityType personType;
        public EdmEntityType uniquePersonType;
        public EdmEntityType vipCustomerType;
        public IEdmEntitySet customerSet;
        public EdmModel model;

        public SampleEdmModel()
        {
            model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");

            personType = new EdmEntityType("NS", "Person");
            personType.AddKeys(personType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, isNullable: false));
            personType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String, isNullable: false);

            customerType = new EdmEntityType("NS", "Customer");
            customerType.AddKeys(customerType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, isNullable: false));
            customerType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String, isNullable: false);
            friendsProperty = customerType.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    ContainsTarget = true,
                    Name = "Friends",
                    Target = personType,
                    TargetMultiplicity = EdmMultiplicity.Many
                });

            vipCustomerType = new EdmEntityType("NS", "VipCustomer", customerType);
            vipCustomerType.AddKeys(vipCustomerType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, isNullable: false));
            vipCustomerType.AddStructuralProperty("VipName", EdmPrimitiveTypeKind.String, isNullable: false);

            uniquePersonType = new EdmEntityType("NS", "UniquePerson", personType);
            uniquePersonType.AddKeys(uniquePersonType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, isNullable: false));
            uniquePersonType.AddStructuralProperty("UniqueName", EdmPrimitiveTypeKind.String, isNullable: false);

            model.AddElement(customerType);
            model.AddElement(personType);
            model.AddElement(uniquePersonType);
            model.AddElement(vipCustomerType);
            model.AddElement(container);

            customerSet = container.AddEntitySet("Customers", customerType);
        }
    }
}