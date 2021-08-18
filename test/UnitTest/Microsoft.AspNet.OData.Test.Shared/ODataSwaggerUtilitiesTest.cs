//-----------------------------------------------------------------------------
// <copyright file="ODataSwaggerUtilitiesTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class ODataSwaggerUtilitiesTest
    {
        private IEdmEntityType _customer;
        private IEdmEntitySet _customers;
        private IEdmActionImport _getCustomers;
        private IEdmFunction _isAnyUpgraded;
        private IEdmFunction _isCustomerUpgradedWithParam;

        public ODataSwaggerUtilitiesTest()
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            _customer = model.Customer;
            _customers = model.Customers;

            IEdmAction action = new EdmAction("NS", "GetCustomers", null, false, null);
            _getCustomers = new EdmActionImport(model.Container, "GetCustomers", action);

            _isAnyUpgraded = model.Model.SchemaElements.OfType<IEdmFunction>().FirstOrDefault(e => e.Name == "IsAnyUpgraded");
            _isCustomerUpgradedWithParam = model.Model.SchemaElements.OfType<IEdmFunction>().FirstOrDefault(e => e.Name == "IsUpgradedWithParam");
        }

        [Fact]
        public void CreateSwaggerPathForEntitySet_ReturnsEmptyObject_IfNavigationSourceNull()
        {
            // Arrange & Act
            JObject obj = ODataSwaggerUtilities.CreateSwaggerPathForEntitySet(navigationSource: null);

            // Assert
            Assert.NotNull(obj);
            Assert.Equal(new JObject(), obj);
        }

        [Fact]
        public void CreateSwaggerPathForEntitySet_ReturnsSwaggerObject()
        {
            // Arrange & Act
            JObject obj = ODataSwaggerUtilities.CreateSwaggerPathForEntitySet(_customers);

            // Assert
            Assert.NotNull(obj);
            Assert.Contains("\"Get EntitySet Customers\"", obj.ToString());
        }

        [Fact]
        public void CreateSwaggerPathForEntity_ReturnsSwaggerObject()
        {
            // Arrange & Act
            JObject obj = ODataSwaggerUtilities.CreateSwaggerPathForEntity(_customers);

            // Assert
            Assert.NotNull(obj);
            Assert.Contains("\"Get entity from Customers by key.\"", obj.ToString());
        }

        [Fact]
        public void CreateSwaggerPathForOperationImport_ReturnsSwaggerObject()
        {
            // Arrange & Act
            JObject obj = ODataSwaggerUtilities.CreateSwaggerPathForOperationImport(_getCustomers);

            // Assert
            Assert.NotNull(obj);
            Assert.Contains("\"Call operation import  GetCustomers\"", obj.ToString());
        }

        [Fact]
        public void CreateSwaggerPathForOperationOfEntitySet_ReturnsSwaggerObject()
        {
            // Arrange & Act
            JObject obj = ODataSwaggerUtilities.CreateSwaggerPathForOperationOfEntitySet(_isAnyUpgraded, _customers);

            // Assert
            Assert.NotNull(obj);
            Assert.Contains("\"Call operation  IsAnyUpgraded\"", obj.ToString());
            Assert.Contains("Customers", obj.ToString());
        }

        [Fact]
        public void CreateSwaggerPathForOperationOfEntity_ReturnsSwaggerObject()
        {
            // Arrange & Act
            JObject obj = ODataSwaggerUtilities.CreateSwaggerPathForOperationOfEntity(_isCustomerUpgradedWithParam, _customers);

            // Assert
            Assert.NotNull(obj);
            Assert.Contains("\"Call operation  IsUpgradedWithParam\"", obj.ToString());
        }

        [Fact]
        public void GetPathForEntity_Returns()
        {
            // Arrange & Act
            string path = ODataSwaggerUtilities.GetPathForEntity(_customers);

            // Assert
            Assert.NotNull(path);
            Assert.Equal("/Customers({ID})", path);
        }

        [Fact]
        public void GetPathForOperationImport_Returns()
        {
            // Arrange & Act
            string path = ODataSwaggerUtilities.GetPathForOperationImport(_getCustomers);

            // Assert
            Assert.NotNull(path);
            Assert.Equal("/GetCustomers()", path);
        }

        [Fact]
        public void GetPathForOperationOfEntitySet_Returns()
        {
            // Arrange & Act
            string path = ODataSwaggerUtilities.GetPathForOperationOfEntitySet(_isAnyUpgraded, _customers);

            // Assert
            Assert.NotNull(path);
            Assert.Equal("/Customers/NS.IsAnyUpgraded()", path);
        }

        [Fact]
        public void GetPathForOperationOfEntity_Returns()
        {
            // Arrange & Act
            string path = ODataSwaggerUtilities.GetPathForOperationOfEntity(_isCustomerUpgradedWithParam, _customers);

            // Assert
            Assert.NotNull(path);
            Assert.Equal("/Customers({ID})/NS.IsUpgradedWithParam(city='{city}')", path);
        }

        [Fact]
        public void CreateSwaggerDefinitionForStructureType_ReturnsSwaggerObject()
        {
            // Arrange & Act
            JObject obj = ODataSwaggerUtilities.CreateSwaggerTypeDefinitionForStructuredType(_customer);

            // Assert
            Assert.NotNull(obj);
            Assert.Contains("\"$ref\": \"#/definitions/NS.Address\"", obj.ToString());
        }
    }
}
