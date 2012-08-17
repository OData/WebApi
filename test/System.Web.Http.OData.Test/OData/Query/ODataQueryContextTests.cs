// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.OData.Query
{
    public class ODataQueryContextTests
    {
        // All types considered primitive for queries containing $skip and $top
        public static TheoryDataSet<Type> QueryPrimitiveTypes
        {
            get
            {
                return new TheoryDataSet<Type>
                {
                    // Edm primitive kinds
                    typeof(byte[]),
                    typeof(bool),
                    typeof(byte),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(decimal),
                    typeof(double),
                    typeof(Guid),
                    typeof(short),
                    typeof(int),
                    typeof(long),
                    typeof(sbyte),
                    typeof(float),
                    typeof(string),
                    typeof(TimeSpan),

                    // additional types not considered Edm primitives
                    // but which we permit in $skip and $top
                    typeof(int?),
                    typeof(char),
                    typeof(sbyte),
                    typeof(ushort),
                    typeof(uint),
                    typeof(ulong),
                    typeof(Uri),
                    typeof(FlagsEnum),
                    typeof(SimpleEnum),
                    typeof(LongEnum),
                    typeof(FlagsEnum?),
                    typeof(int[]),
                    typeof(int[][]),
                };
            }
        }

        [Theory]
        [PropertyData("QueryPrimitiveTypes")]
        public void ContructorWithOnlyType(Type type)
        {
            // Arrange & Act
            ODataQueryContext context = new ODataQueryContext(type);

            // Assert
            Assert.Null(context.Model);
            Assert.True(context.EntityClrType == type);
            Assert.Null(context.EntitySet);
            Assert.True(context.IsPrimitiveClrType);
        }

        [Fact]
        public void ContructorWithOnlyType_Throws_With_Null_Type()
        {
            // Arrange & Act & Assert
            Assert.ThrowsArgumentNull(
                    () => new ODataQueryContext(null), 
                    "clrType");
        }

        [Fact]
        public void ContructorWithOnlyType_Throws_With_NonPrimitive_Type()
        {
            // Arrange & Act & Assert
            Assert.ThrowsArgument(
                    () => new ODataQueryContext(this.GetType()),
                    "clrType",
                    "The type 'ODataQueryContextTests' is not a primitive type.");
        }

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
            Assert.ThrowsArgumentNull(
                    () => new ODataQueryContext(null, typeof(Customer)), 
                    "model");
        }

        [Fact]
        public void ContructorWithNullModelAndTypeAndEntitySetThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainers().Single().FindEntitySet("Customers");

            Assert.ThrowsArgumentNull(
                    () => new ODataQueryContext(null, typeof(Customer), entitySet), 
                    "model");
        }

        [Fact]
        public void ContructorWithNullModelAndTypeAndEntitySetNameThrows()
        {
            Assert.ThrowsArgumentNull(() =>
                    new ODataQueryContext(null, typeof(Customer), "Customers"), 
                    "model");
        }

        [Fact]
        public void ConstructorWithNullTypeThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            Assert.ThrowsArgumentNull(() =>
                    new ODataQueryContext(model, null),
                    "entityClrType");
        }

        [Fact]
        public void ConstructorWithNullTypeAndEntitySetNameThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            Assert.ThrowsArgumentNull(() =>
                    new ODataQueryContext(model, null, "Customers"),
                    "entityClrType");
        }

        [Fact]
        public void ContructorWithNullTypeAndEntitySetThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainers().Single().FindEntitySet("Customers");

            Assert.ThrowsArgumentNull(() =>
                    new ODataQueryContext(model, null, entitySet), 
                    "entityClrType");
        }

        [Fact]
        public void ConstructorWithNullEntitySetNameThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            Assert.ThrowsArgument(
                    () => new ODataQueryContext(model, typeof(Customer), (string)null),
                    "entitySetName");
        }

        [Fact]
        public void ConstructorWithEmptyEntitySetNameThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            Assert.ThrowsArgument(
                    () => new ODataQueryContext(model, typeof(Customer), string.Empty),
                    "entitySetName",
                    "The argument 'entitySetName' is null or empty." + Environment.NewLine + "Parameter name: entitySetName");
        }

        [Fact]
        public void ConstructorWithNullEntitySetThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            Assert.ThrowsArgumentNull(() =>
                    new ODataQueryContext(model, typeof(Customer), (IEdmEntitySet)null),
                    "entitySet");
        }
    }
}
