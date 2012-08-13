// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.Edm;
using Xunit;

namespace System.Web.Http.OData.Query
{
    public class ODataQueryContextTests
    {
        [Fact]
        public void ContructorWithModelAndType()
        {
            // Arrange
            var odataModel = new ODataModelBuilder().Add_Customer_EntityType();
            odataModel.EntitySet<Customer>(typeof(Customer).Name);
            IEdmModel model = odataModel.GetEdmModel();

            // Act
            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer));

            // Assert
            Assert.Same(model, context.Model);
            Assert.True(context.EntityClrType == typeof(Customer));
            Assert.NotNull(context.EntitySet);
            Assert.True(context.EntitySet.Name == typeof(Customer).Name);
        }

        [Fact]
        public void ContructorWithModelTypeAndEntitySetName()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            // Act
            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer), "Customers");

            // Assert
            Assert.Same(model, context.Model);
            Assert.True(context.EntityClrType == typeof(Customer));
            Assert.NotNull(context.EntitySet);
            Assert.True(context.EntitySet.Name == "Customers");
        }

        [Fact]
        public void ContructorWithModelTypeAndEntitySet()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainers().Single().FindEntitySet("Customers");

            // Act
            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer), entitySet);

            // Assert
            Assert.Same(model, context.Model);
            Assert.True(context.EntityClrType == typeof(Customer));
            Assert.NotNull(context.EntitySet);
            Assert.Same(entitySet, context.EntitySet);
        }

        [Fact]
        public void ContructorWithNullModelAndTypeThrows()
        {
            // Act && Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ODataQueryContext(null, typeof(Customer)));
        }

        [Fact]
        public void ContructorWithNullModelAndTypeAndEntitySetThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainers().Single().FindEntitySet("Customers");

            Assert.Throws<ArgumentNullException>(() =>
                new ODataQueryContext(null, typeof(Customer), entitySet));
        }

        [Fact]
        public void ContructorWithNullModelAndTypeAndEntitySetNameThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ODataQueryContext(null, typeof(Customer), "Customers"));
        }

        [Fact]
        public void ConstructorWithNullTypeThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            Assert.Throws<ArgumentNullException>(() =>
                new ODataQueryContext(model, null));
        }

        [Fact]
        public void ConstructorWithNullTypeAndEntitySetNameThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            Assert.Throws<ArgumentNullException>(() =>
                new ODataQueryContext(model, null, "Customers"));
        }

        [Fact]
        public void ContructorWithNullTypeAndEntitySetThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainers().Single().FindEntitySet("Customers");

            Assert.Throws<ArgumentNullException>(() =>
                new ODataQueryContext(model, null, entitySet));
        }

        [Fact]
        public void ConstructorWithNullEntitySetNameThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            Assert.Throws<ArgumentException>(() =>
                new ODataQueryContext(model, typeof(Customer), (string)null));
        }

        [Fact]
        public void ConstructorWithEmptyEntitySetNameThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            Assert.Throws<ArgumentException>(() =>
                new ODataQueryContext(model, typeof(Customer), string.Empty));
        }

        [Fact]
        public void ConstructorWithNullEntitySetThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            Assert.Throws<ArgumentNullException>(() =>
                new ODataQueryContext(model, typeof(Customer), (IEdmEntitySet)null));
        }
    }
}
