// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;

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
        public void ConstructorWithPrimitiveTypes(Type type)
        {
            // Arrange & Act
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, type);

            // Assert
            Assert.True(context.ElementClrType == type);
        }

        [Fact]
        public void Constructor_Throws_With_Null_Model()
        {
            // Arrange & Act & Assert
            Assert.ThrowsArgumentNull(
                () => new ODataQueryContext(model: null, elementClrType: typeof(int)),
                    "model");
        }

        [Fact]
        public void Constructor_Throws_With_Null_Type()
        {
            // Arrange & Act & Assert
            Assert.ThrowsArgumentNull(
                () => new ODataQueryContext(EdmCoreModel.Instance, elementClrType: null),
                    "elementClrType");
        }

        [Fact]
        public void Constructor()
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
        public void Constructor_Throws_For_UnknownType(Type elementType)
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
    }
}
