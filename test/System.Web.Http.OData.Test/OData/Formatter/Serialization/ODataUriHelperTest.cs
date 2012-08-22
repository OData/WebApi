// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataUriHelperTests
    {
        [Fact]
        public void CanGetEntitySetAndEntityTypeOfEntitySetUri()
        {
            IEdmEntitySet entitySet = null;

            Uri uri = new Uri("http://myservice/foo/Customers(1)");

            var model = CreateCustomerProductsModel();

            var customerSet = model.FindDeclaredEntityContainer("DefaultContainer").FindEntitySet("Customers");

            bool foundSetAndType = ODataUriHelpers.TryGetEntitySetAndEntityType(uri, model, out entitySet);

            Assert.Equal(true, foundSetAndType);
            Assert.NotNull(entitySet);

            Assert.Same(customerSet, entitySet);
        }

        [Fact]
        public void CanGetEntitySetAndEntityTypeOfEntitySetWithNavigationUri()
        {
            IEdmEntitySet entitySet = null;

            Uri uri = new Uri("http://myservice/foo/Customers(1)/Products");
            var model = CreateCustomerProductsModel();
            var productSet = model.FindDeclaredEntityContainer("DefaultContainer").FindEntitySet("Products");

            bool foundSetAndType = ODataUriHelpers.TryGetEntitySetAndEntityType(uri, model, out entitySet);
            Assert.Equal(true, foundSetAndType);

            Assert.NotNull(entitySet);

            Assert.Same(productSet, entitySet);
        }

        [Theory]
        [InlineData("http://localhost/Products(10)/ID", "http://localhost/", "ID")]
        [InlineData("http://localhost/virtualpath/Products(10)/ID", "http://localhost/virtualpath/", "ID")]
        [InlineData("http://localhost/Products(10)/Category/Products(20)/Category/ID", "http://localhost/", "ID")]
        public void GetOperationName(string requestUri, string baseUri, string expectedOpeartionName)
        {
            Assert.Equal(ODataUriHelpers.GetOperationName(new Uri(requestUri), new Uri(baseUri)), expectedOpeartionName);
        }

        private static EdmModel CreateCustomerProductsModel()
        {
            var model = new EdmModel();

            var container = new EdmEntityContainer("defaultNamespace", "DefaultContainer");
            model.AddElement(container);

            var productType = new EdmEntityType("defaultNamespace", "Product");
            model.AddElement(productType);

            var customerType = new EdmEntityType("defaultNamespace", "Customer");
            customerType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo() { Name = "Products", Target = productType, TargetMultiplicity = EdmMultiplicity.Many });
            productType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo() { Name = "Customers", Target = customerType, TargetMultiplicity = EdmMultiplicity.Many });

            model.AddElement(customerType);

            var productSet = new EdmEntitySet(
                container,
                "Products",
                productType);

            container.AddElement(productSet);

            var customerSet = new EdmEntitySet(
                container,
                "Customers",
                customerType);

            var productsNavProp = customerType.NavigationProperties().Single(np => np.Name == "Products");
            customerSet.AddNavigationTarget(productsNavProp, productSet);
            container.AddElement(customerSet);

            return model;
        }
    }
}
