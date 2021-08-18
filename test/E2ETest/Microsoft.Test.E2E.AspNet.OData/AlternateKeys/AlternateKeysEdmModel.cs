//-----------------------------------------------------------------------------
// <copyright file="AlternateKeysEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;

namespace Microsoft.Test.E2E.AspNet.OData.AlternateKeys
{
    public static class AlternateKeysEdmModel
    {
        private static IEdmModel _edmModel;
        public static IEdmModel GetEdmModel()
        {
            if (_edmModel != null)
            {
                return _edmModel;
            }

            EdmModel model = new EdmModel();

            // entity type 'Customer' with single alternate keys
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            var ssn = customer.AddStructuralProperty("SSN", EdmPrimitiveTypeKind.String);
            model.AddAlternateKeyAnnotation(customer, new Dictionary<string, IEdmProperty>
            {
                {"SSN", ssn}
            });
            model.AddElement(customer);

            // entity type 'Order' with multiple alternate keys
            EdmEntityType order = new EdmEntityType("NS", "Order");
            order.AddKeys(order.AddStructuralProperty("OrderId", EdmPrimitiveTypeKind.Int32));
            var orderName = order.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            var orderToken = order.AddStructuralProperty("Token", EdmPrimitiveTypeKind.Guid);
            order.AddStructuralProperty("Amount", EdmPrimitiveTypeKind.Int32);
            model.AddAlternateKeyAnnotation(order, new Dictionary<string, IEdmProperty>
            {
                {"Name", orderName}
            });

            model.AddAlternateKeyAnnotation(order, new Dictionary<string, IEdmProperty>
            {
                {"Token", orderToken}
            });

            model.AddElement(order);

            // entity type 'Person' with composed alternate keys
            EdmEntityType person = new EdmEntityType("NS", "Person");
            person.AddKeys(person.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            var countryRegion = person.AddStructuralProperty("Country_Region", EdmPrimitiveTypeKind.String);
            var passport = person.AddStructuralProperty("Passport", EdmPrimitiveTypeKind.String);
            model.AddAlternateKeyAnnotation(person, new Dictionary<string, IEdmProperty>
            {
                {"Country_Region", countryRegion},
                {"Passport", passport}
            });
            model.AddElement(person);

            // complex type address
            EdmComplexType address = new EdmComplexType("NS", "Address");
            var street = address.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
            var city = address.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
            model.AddElement(address);

            // entity type 'Company' with complex type alternate keys
            EdmEntityType company = new EdmEntityType("NS", "Company");
            company.AddKeys(company.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            company.AddStructuralProperty("Location", new EdmComplexTypeReference(address, isNullable: true));
            model.AddAlternateKeyAnnotation(company, new Dictionary<string, IEdmProperty>
            {
                {"City", city},
                {"Street", street}
            });
            model.AddElement(company);

            // entity sets
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            model.AddElement(container);
            container.AddEntitySet("Customers", customer);
            container.AddEntitySet("Orders", order);
            container.AddEntitySet("People", person);
            container.AddEntitySet("Companies", company);

            return _edmModel = model;
        }
    }
}
