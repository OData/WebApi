// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;
using Moq;

namespace System.Web.OData.Query
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
                    // TODO: Investigate how to add support for DataTime in webapi.odata, ODataLib v4 does not support it.
                    // typeof(DateTime),
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
                };
            }
        }

        public static TheoryDataSet<Type> QueryEnumTypes
        {
            get
            {
                return new TheoryDataSet<Type>
                {
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

        [Theory]
        [PropertyData("QueryEnumTypes")]
        public void Constructor_TakingClrType_WithEnumTypes(Type type)
        {
            // Arrange
            ODataModelBuilder odataModel = new ODataModelBuilder()
                .Add_SimpleEnum_EnumType()
                .Add_FlagsEnum_EnumType()
                .Add_LongEnum_EnumType();
            IEdmModel model = odataModel.GetEdmModel();

            // Act
            ODataQueryContext context = new ODataQueryContext(model, type);

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

        [Fact]
        public void Constructor_TakingClrTypeAndPath_SetsProperties()
        {
            // Arrange
            ODataModelBuilder odataModel = new ODataModelBuilder().Add_Customer_EntityType();
            string setName = typeof(Customer).Name;
            odataModel.EntitySet<Customer>(setName);
            IEdmModel model = odataModel.GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet(setName);
            IEdmEntityType entityType = entitySet.EntityType();
            ODataPath path = new ODataPath(new EntitySetPathSegment(entitySet));

            // Act
            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer), path);

            // Assert
            Assert.Same(model, context.Model);
            Assert.Same(entityType, context.ElementType);
            Assert.Same(entitySet, context.NavigationSource);
            Assert.Same(typeof(Customer), context.ElementClrType);
        }

        [Fact]
        public void Constructor_TakingEdmTypeAndPath_SetsProperties()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmEntityType entityType = new Mock<IEdmEntityType>().Object;
            IEdmEntityContainer entityContiner = new Mock<IEdmEntityContainer>().Object;
            EdmEntitySet entitySet = new EdmEntitySet(entityContiner, "entitySet", entityType);
            ODataPath path = new ODataPath(new EntitySetPathSegment(entitySet)); 

            // Act
            ODataQueryContext context = new ODataQueryContext(model, entityType, path);

            // Assert
            Assert.Same(model, context.Model);
            Assert.Same(entityType, context.ElementType);
            Assert.Same(entitySet, context.NavigationSource);
            Assert.Null(context.ElementClrType);
        }
    }
}
