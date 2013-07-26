// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;
using Moq;

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
                    typeof(FlagsEnum),
                    typeof(SimpleEnum),
                    typeof(LongEnum),
                    typeof(FlagsEnum?),
                };
            }
        }

        [Theory]
        [PropertyData("QueryPrimitiveTypes")]
        public void Constructor_TakingClrType_WithPrimitiveTypes(Type type)
        {
            // Arrange & Act
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, type);

            // Assert
            Assert.True(context.ElementClrType == type);
        }

        [Fact]
        public void Constructor_TakingClrType_Throws_With_Null_Model()
        {
            // Arrange & Act & Assert
            Assert.ThrowsArgumentNull(
                () => new ODataQueryContext(model: null, elementClrType: typeof(int)),
                    "model");
        }

        [Fact]
        public void Constructor_TakingClrType_Throws_With_Null_Type()
        {
            // Arrange & Act & Assert
            Assert.ThrowsArgumentNull(
                () => new ODataQueryContext(EdmCoreModel.Instance, elementClrType: null),
                    "elementClrType");
        }

        [Fact]
        public void Constructor_TakingClrType_SetsProperties()
        {
            // Arrange
            var odataModel = new ODataModelBuilder().Add_Customer_EntityType();
            odataModel.EntitySet<Customer>(typeof(Customer).Name);
            IEdmModel model = odataModel.GetEdmModel();

            // Act
            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer));

            // Assert
            Assert.Same(model, context.Model);
            Assert.True(context.ElementClrType == typeof(Customer));
        }

        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(Order))]
        public void Constructor_TakingClrType_Throws_For_UnknownType(Type elementType)
        {
            // Arrange
            var odataModel = new ODataModelBuilder().Add_Customer_EntityType();
            odataModel.EntitySet<Customer>(typeof(Customer).Name);
            IEdmModel model = odataModel.GetEdmModel();

            // Act && Assert
            Assert.ThrowsArgument(
                () => new ODataQueryContext(model, elementType),
                "elementClrType",
                Error.Format("The given model does not contain the type '{0}'.", elementType.FullName));
        }

        [Fact]
        public void Ctor_TakingEdmType_ThrowsArgumentNull_Model()
        {
            Assert.ThrowsArgumentNull(() => new ODataQueryContext(model: null, elementType: new Mock<IEdmType>().Object),
                "model");
        }

        [Fact]
        public void Ctor_TakingEdmType_ThrowsArgumentNull_ElementType()
        {
            Assert.ThrowsArgumentNull(() => new ODataQueryContext(EdmCoreModel.Instance, elementType: null),
                "elementType");
        }

        [Fact]
        public void Ctor_TakingEdmType_InitializesProperties()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmType elementType = new Mock<IEdmType>().Object;

            // Act
            var context = new ODataQueryContext(model, elementType);

            // Assert
            Assert.Same(model, context.Model);
            Assert.Same(elementType, context.ElementType);
            Assert.Null(context.ElementClrType);
        }
    }
}
