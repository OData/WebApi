// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Routing
{
    public class UnboundFunctionPathSegmentTest
    {
        private IEdmModel _model;
        private IEdmEntityContainer _container;

        public UnboundFunctionPathSegmentTest()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<MyCustomer>().HasKey(c => c.Id).Property(c => c.Name);
            builder.EntitySet<MyCustomer>("Customers");
            FunctionConfiguration function = builder.Function("TopCustomer");
            function.ReturnsFromEntitySet<MyCustomer>("Customers");
            builder.Function("MyFunction").Returns<string>();
            _model = builder.GetEdmModel();
            _container = _model.EntityContainer;
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_UnboundFunction()
        {
            // Arrange
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => new UnboundFunctionPathSegment(function: null, model: _model, parameterValues: parameters),
                "function");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_Model()
        {
            // Arrange
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            IEdmFunctionImport functionImport = _container.FindOperationImports("TopCustomer").SingleOrDefault() as IEdmFunctionImport;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => new UnboundFunctionPathSegment(function: functionImport, model: null, parameterValues: parameters),
                "model");
        }

        [Fact]
        public void Ctor_TakingFunction_InitializesFunctionProperty()
        {
            // Arrange
            Mock<IEdmFunctionImport> edmFunction = new Mock<IEdmFunctionImport>();
            edmFunction.Setup(a => a.Name).Returns("Function");

            // Act
            UnboundFunctionPathSegment functionPathSegment = new UnboundFunctionPathSegment(edmFunction.Object, _model, null);

            // Assert
            Assert.Same(edmFunction.Object, functionPathSegment.Function);
        }

        [Fact]
        public void Ctor_TakingFunction_InitializesFunctionNameProperty()
        {
            // Arrange
            IEdmFunctionImport function = _container.FindOperationImports("MyFunction").SingleOrDefault() as IEdmFunctionImport;

            // Act
            UnboundFunctionPathSegment functionPathSegment = new UnboundFunctionPathSegment(function, _model, null);

            // Assert
            Assert.Equal("MyFunction", functionPathSegment.FunctionName);
        }

        [Fact]
        public void Property_SegmentKind_IsUnboundFunction()
        {
            // Arrange
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            UnboundFunctionPathSegment segment = new UnboundFunctionPathSegment("function", parameters);

            // Assert
            Assert.Equal(ODataSegmentKinds.UnboundFunction, segment.SegmentKind);
        }

        [Fact]
        public void ToString_ReturnsSameString_UnboundFunction()
        {
            // Arrange
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Id", "123");
            parameters.Add("Name", "John");
            UnboundFunctionPathSegment segment = new UnboundFunctionPathSegment("function", parameters);

            // Act
            string actual = segment.ToString();

            // Assert
            Assert.Equal("function(Id=123,Name=John)", actual);
        }

        [Fact]
        public void GetEdmType_ThrowsArgumentException_IfArgumentNotNull()
        {
            // Arrange
            var segment = new UnboundFunctionPathSegment("GetTopCustomer", parameterValues: null);
            Mock<IEdmType> edmType = new Mock<IEdmType>();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => segment.GetEdmType(edmType.Object));
        }

        [Fact]
        public void GetEdmType_ReturnsNotNull_EntityType()
        {
            // Arrange
            IEdmFunctionImport functionImport = _container.FindOperationImports("TopCustomer").SingleOrDefault() as IEdmFunctionImport;
            UnboundFunctionPathSegment segment = new UnboundFunctionPathSegment(functionImport, _model, parameterValues: null);

            // Act
            var result = segment.GetEdmType(previousEdmType: null);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("System.Web.OData.Routing.MyCustomer", result.FullTypeName());
        }

        [Fact]
        public void GetEdmType_ReturnsNull_IfActionNull()
        {
            // Arrange
            var segment = new UnboundActionPathSegment("TopCustomer");

            // Act
            var result = segment.GetEdmType(previousEdmType: null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetEdmType_ReturnsNotNull_PrimitiveReturnType()
        {
            // Arrange
            IEdmFunctionImport functionImport = _container.FindOperationImports("MyFunction").SingleOrDefault() as IEdmFunctionImport;
            UnboundFunctionPathSegment segment = new UnboundFunctionPathSegment(functionImport, _model, parameterValues: null);

            // Act
            var result = segment.GetEdmType(previousEdmType: null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Edm.String", result.FullTypeName());
        }

        [Fact]
        public void GetNavigationSource_ReturnsNotNull_UnboundFunctionEntitySetType()
        {
            // Arrange
            IEdmFunctionImport functionImport = _container.FindOperationImports("TopCustomer").SingleOrDefault() as IEdmFunctionImport;
            UnboundFunctionPathSegment segment = new UnboundFunctionPathSegment(functionImport, _model, parameterValues: null);

            // Act
            var result = segment.GetNavigationSource(previousNavigationSource: null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("System.Web.OData.Routing.MyCustomer", result.EntityType().FullName());
        }

        [Fact]
        public void GetNavigationSource_ReturnsNull_UnboundFunctionEntitySetType()
        {
            // Arrange
            IEdmFunctionImport functionImport = _container.FindOperationImports("MyFunction").SingleOrDefault() as IEdmFunctionImport;
            UnboundFunctionPathSegment segment = new UnboundFunctionPathSegment(functionImport, _model, parameterValues: null);

            // Act
            var result = segment.GetNavigationSource(previousNavigationSource: null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void TryMatch_ReturnsTrue_IfSameUnboundFunction()
        {
            // Arrange
            IEdmFunctionImport function = _container.FindOperationImports("TopCustomer").SingleOrDefault() as IEdmFunctionImport;
            UnboundFunctionPathSegment template = new UnboundFunctionPathSegment(function, _model, parameterValues: null);
            UnboundFunctionPathSegment segment = new UnboundFunctionPathSegment(function, _model, parameterValues: null);

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.True(result);
            Assert.Empty(values);
        }

        internal class MyCustomer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
