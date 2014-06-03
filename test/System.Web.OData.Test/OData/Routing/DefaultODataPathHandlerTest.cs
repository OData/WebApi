// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.OData.Builder;
using System.Web.OData.Formatter;
using System.Web.OData.TestCommon;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Expressions;
using Microsoft.OData.Edm.Library;
using Microsoft.OData.Edm.Library.Expressions;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;

namespace System.Web.OData.Routing
{
    public class DefaultODataPathHandlerTest
    {
        private static DefaultODataPathHandler _parser = new DefaultODataPathHandler();
        private static IEdmModel _model = ODataRoutingModel.GetModel();
        private const string _serviceRoot = "http://any/";

        public static TheoryDataSet<object, Type> NullFunctionParameterData
        {
            get
            {
                TheoryDataSet<object, Type> data = new TheoryDataSet<object, Type>();

                data.Add(null, typeof(int?));
                data.Add(null, typeof(bool?));
                data.Add(null, typeof(long?));
                data.Add(null, typeof(Single?));
                data.Add(null, typeof(double?));
                data.Add(null, typeof(string));
                data.Add(null, typeof(DateTimeOffset?));
                data.Add(null, typeof(TimeSpan?));
                data.Add(null, typeof(Guid?));
                data.Add(null, typeof(SimpleEnum?));

                return data;
            }
        }

        public static TheoryDataSet<object, Type> EnumFunctionParameterData
        {
            get
            {
                TheoryDataSet<object, Type> data = new TheoryDataSet<object, Type>();
                data.Add(new ODataEnumValue("1", "NS.SimpleEnum"), typeof(SimpleEnum));
                data.Add(new ODataEnumValue("0", "NS.SimpleEnum"), typeof(SimpleEnum?));
                return data;
            }
        }

        public static TheoryDataSet<object, Type> FunctionParameterData
        {
            get
            {
                TheoryDataSet<object, Type> data = new TheoryDataSet<object, Type>();

                data.Add(1, typeof(int));
                data.Add(true, typeof(bool));
                data.Add((long)-123, typeof(long));
                data.Add((Single)1.23, typeof(Single));
                data.Add(4.56, typeof(double));
                data.Add(new DateTimeOffset(new DateTime(2000, 1, 2, 3, 4, 5, DateTimeKind.Utc)), typeof(DateTimeOffset));
                data.Add(new TimeSpan(23, 59, 59), typeof(TimeSpan));
                data.Add(Guid.NewGuid(), typeof(Guid));

                data.Add(-1, typeof(int?));
                data.Add(false, typeof(bool?));
                data.Add((long)123, typeof(long?));
                data.Add((Single)123, typeof(Single?));
                data.Add(1.23, typeof(double?));
                data.Add("abc", typeof(string));
                data.Add(new DateTimeOffset(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc)), typeof(DateTimeOffset?));
                data.Add(new TimeSpan(1, 2, 3), typeof(TimeSpan?));
                data.Add(Guid.Empty, typeof(Guid?));
                return data;
            }
        }

        [Fact]
        public void Parse_WorksOnEncodedCharacters()
        {
            string odataPath = "üCategories";

            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            Assert.NotNull(path);
            Assert.Equal("~/entityset", path.PathTemplate);
            Assert.Equal("üCategories", segment.ToString());
        }

        [Fact]
        public void Parse_ForInvalidCast_ThrowsODataException()
        {
            string odataPath = "RoutingCustomers/System.Web.OData.Routing.Product";

            Assert.Throws<ODataException>(
                () => _parser.Parse(_model, _serviceRoot, odataPath),
                "The type 'System.Web.OData.Routing.Product' specified in the URI is neither a base type " +
                "nor a sub-type of the previously-specified type 'System.Web.OData.Routing.RoutingCustomer'.");
        }

        [Fact]
        public void Parse_ForSegmentAfterMetadata_ThrowsODataException()
        {
            string odataPath = "$metadata/foo";

            Assert.Throws<ODataUnrecognizedPathException>(
                () => _parser.Parse(_model, _serviceRoot, odataPath),
                "The request URI is not valid. The segment '$metadata' must be the last segment in the URI because " +
                "it is one of the following: $batch, $value, $metadata, a collection property, a named media resource, " +
                "an action, a noncomposable function, an action import, or a noncomposable function import.");
        }

        [Theory]
        [InlineData("", "~", "")]
        [InlineData("$metadata", "~/$metadata", "$metadata")]
        [InlineData("$batch", "~/$batch", "$batch")]
        [InlineData("RoutingCustomers(112)", "~/entityset/key", "RoutingCustomers(112)")]
        [InlineData("RoutingCustomers/System.Web.OData.Routing.VIP", "~/entityset/cast", "RoutingCustomers/System.Web.OData.Routing.VIP")]
        [InlineData("RoutingCustomers(100)/Products", "~/entityset/key/navigation", "RoutingCustomers(100)/Products")]
        [InlineData("RoutingCustomers(100)/Address/Unknown", "~/entityset/key/property/unresolved", "RoutingCustomers(100)/Address/Unknown")]
        [InlineData("RoutingCustomers(100)/Products()", "~/entityset/key/navigation", "RoutingCustomers(100)/Products")]
        [InlineData("RoutingCustomers(100)/System.Web.OData.Routing.VIP/RelationshipManager", "~/entityset/key/cast/navigation",
            "RoutingCustomers(100)/System.Web.OData.Routing.VIP/RelationshipManager")]
        [InlineData("GetRoutingCustomerById()", "~/unboundaction", "GetRoutingCustomerById")]
        [InlineData("UnboundFunction()", "~/unboundfunction", "UnboundFunction()")]
        [InlineData("UnboundFunctionWithOneParamters(P1=1)", "~/unboundfunction", "UnboundFunctionWithOneParamters(P1=1)")]
        [InlineData("UnboundFunctionWithMultipleParamters(P1=1,P2=2,P3='a')", "~/unboundfunction", "UnboundFunctionWithMultipleParamters(P1=1,P2=2,P3='a')")]
        [InlineData("OverloadUnboundFunction()", "~/unboundfunction", "OverloadUnboundFunction()")]
        [InlineData("OverloadUnboundFunction(P1=1)", "~/unboundfunction", "OverloadUnboundFunction(P1=1)")]
        [InlineData("OverloadUnboundFunction(P1=1,P2=2,P3='a')", "~/unboundfunction", "OverloadUnboundFunction(P1=1,P2=2,P3='a')")]
        [InlineData("RoutingCustomers(112)/Address/Street", "~/entityset/key/property/property", "RoutingCustomers(112)/Address/Street")]
        [InlineData("RoutingCustomers(1)/Name/$value", "~/entityset/key/property/$value", "RoutingCustomers(1)/Name/$value")]
        [InlineData("RoutingCustomers(1)/Products/$ref", "~/entityset/key/navigation/$ref", "RoutingCustomers(1)/Products/$ref")]
        [InlineData("RoutingCustomers(112)/Default.GetRelatedRoutingCustomers", "~/entityset/key/action",
            "RoutingCustomers(112)/Default.GetRelatedRoutingCustomers")]
        [InlineData("RoutingCustomers/System.Web.OData.Routing.VIP/Default.GetMostProfitable", "~/entityset/cast/action",
            "RoutingCustomers/System.Web.OData.Routing.VIP/Default.GetMostProfitable")]
        [InlineData("RoutingCustomers(112)/Default.GetOrdersCount(factor=1)", "~/entityset/key/function",
            "RoutingCustomers(112)/Default.GetOrdersCount(factor=1)")]
        [InlineData("RoutingCustomers(112)/System.Web.OData.Routing.VIP/Default.GetOrdersCount(factor=1)", "~/entityset/key/cast/function",
            "RoutingCustomers(112)/System.Web.OData.Routing.VIP/Default.GetOrdersCount(factor=1)")]
        [InlineData("RoutingCustomers/Default.FunctionBoundToRoutingCustomers()", "~/entityset/function",
            "RoutingCustomers/Default.FunctionBoundToRoutingCustomers()")]
        [InlineData("RoutingCustomers/System.Web.OData.Routing.VIP/Default.FunctionBoundToRoutingCustomers()", "~/entityset/cast/function",
            "RoutingCustomers/System.Web.OData.Routing.VIP/Default.FunctionBoundToRoutingCustomers()")]
        [InlineData("Products(1)/RoutingCustomers/System.Web.OData.Routing.VIP(1)/RelationshipManager/ManagedProducts",
            "~/entityset/key/navigation/cast/key/navigation/navigation",
            "Products(1)/RoutingCustomers/System.Web.OData.Routing.VIP(1)/RelationshipManager/ManagedProducts")]
        [InlineData("Products(1)/Default.FunctionBoundToProductWithMultipleParamters(P1=1,P2=2,P3='a')", "~/entityset/key/function",
            "Products(1)/Default.FunctionBoundToProductWithMultipleParamters(P1=1,P2=2,P3='a')")]
        [InlineData("Products(1)/Default.FunctionBoundToProduct()", "~/entityset/key/function",
            "Products(1)/Default.FunctionBoundToProduct()")]
        [InlineData("Products(1)/Default.FunctionBoundToProduct(P1=1)", "~/entityset/key/function",
            "Products(1)/Default.FunctionBoundToProduct(P1=1)")]
        [InlineData("Products(1)/Default.FunctionBoundToProduct(P1=1,P2=2,P3='a')", "~/entityset/key/function",
            "Products(1)/Default.FunctionBoundToProduct(P1=1,P2=2,P3='a')")]
        [InlineData("EnumCustomers(1)/Color", "~/entityset/key/property", "EnumCustomers(1)/Color")]
        [InlineData("EnumCustomers(1)/Color/$value", "~/entityset/key/property/$value", "EnumCustomers(1)/Color/$value")]
        [InlineData("VipCustomer", "~/singleton", "VipCustomer")]
        [InlineData("VipCustomer/System.Web.OData.Routing.VIP", "~/singleton/cast", "VipCustomer/System.Web.OData.Routing.VIP")]
        [InlineData("VipCustomer/Products", "~/singleton/navigation", "VipCustomer/Products")]
        [InlineData("VipCustomer/System.Web.OData.Routing.VIP/RelationshipManager", "~/singleton/cast/navigation",
            "VipCustomer/System.Web.OData.Routing.VIP/RelationshipManager")]
        [InlineData("VipCustomer/Name/$value", "~/singleton/property/$value", "VipCustomer/Name/$value")]
        [InlineData("VipCustomer/Products/$ref", "~/singleton/navigation/$ref", "VipCustomer/Products/$ref")]
        [InlineData("VipCustomer/Default.GetRelatedRoutingCustomers", "~/singleton/action", "VipCustomer/Default.GetRelatedRoutingCustomers")]
        [InlineData("MyProduct/Default.TopProductId()", "~/singleton/function", "MyProduct/Default.TopProductId()")]
        public void Parse_ReturnsPath_WithCorrectTemplateAndPathString(string odataPath, string template, string pathString)
        {
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);

            Assert.NotNull(path);
            Assert.Equal(template, path.PathTemplate);
            Assert.Equal(pathString, path.ToString());
        }

        [Theory]
        [InlineData("", new string[] { })]
        [InlineData("UnboundFunctionWithOneParamters(P1=1)/Products", new string[] { "UnboundFunctionWithOneParamters(P1=1)", "Products" })]
        [InlineData("UnboundFunctionWithMultipleParamters(P1=1,P2=2,P3='a')", new string[] { "UnboundFunctionWithMultipleParamters(P1=1,P2=2,P3='a')" })]
        [InlineData("RoutingCustomers(100)/System.Web.OData.Routing.VIP/RelationshipManager", new string[] { "RoutingCustomers", "100", "System.Web.OData.Routing.VIP", "RelationshipManager" })]
        [InlineData("RoutingCustomers(112)/Address/Street", new string[] { "RoutingCustomers", "112", "Address", "Street" })]
        [InlineData("RoutingCustomers(1)/Name/$value", new string[] { "RoutingCustomers", "1", "Name", "$value" })]
        [InlineData("RoutingCustomers(1)/Products/$ref", new string[] { "RoutingCustomers", "1", "Products", "$ref" })]
        [InlineData("VipCustomer/Default.GetRelatedRoutingCustomers", new string[] { "VipCustomer", "Default.GetRelatedRoutingCustomers" })]
        public void ParseSegmentsCorrectly(string odataPath, string[] expectedSegments)
        {
            // Arrange & Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);

            // Assert
            Assert.Equal(expectedSegments, path.Segments.Select(segment => segment.ToString()));
        }

        [Fact]
        public void CanParseMetadataUrl()
        {
            string odataPath = "$metadata";

            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(path);
            Assert.Null(path.NavigationSource);
            Assert.Null(path.EdmType);
            Assert.Equal("$metadata", segment.ToString());
        }

        [Fact]
        public void CanParseBatchUrl()
        {
            // Arrange
            string odataPath = "$batch";

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(path);
            Assert.NotNull(segment);
            Assert.Null(path.NavigationSource);
            Assert.Null(path.EdmType);
            Assert.Equal("$batch", segment.ToString());
        }

        [Fact]
        public void CanParseEntitySetUrl()
        {
            // Arrange
            string odataPath = "RoutingCustomers";
            string expectedText = "RoutingCustomers";
            IEdmEntitySet expectedSet = _model.EntityContainer.EntitySets().SingleOrDefault(s => s.Name == "RoutingCustomers");

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(path);
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Same(expectedSet.EntityType(), (path.EdmType as IEdmCollectionType).ElementType.Definition);
        }

        [Fact]
        public void CanParseEntitySetCastUrl()
        {
            // Arrange
            IEdmEntitySet expectedSet = _model.EntityContainer.EntitySets()
                .SingleOrDefault(s => s.Name == "RoutingCustomers");
            IEdmEntityType entityType = _model.SchemaElements.OfType<IEdmEntityType>()
                .SingleOrDefault(s => s.FullName() == "System.Web.OData.Routing.VIP");

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, "RoutingCustomers/System.Web.OData.Routing.VIP");
            Assert.NotNull(path); // Guard
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal("~/entityset/cast", path.PathTemplate);
            Assert.Equal("System.Web.OData.Routing.VIP", segment.ToString());
            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Same(entityType, ((IEdmCollectionType)path.EdmType).ElementType.Definition);
        }

        [Fact]
        public void CanParseSingletonUrl()
        {
            // Arrange
            const string ODataPath = "VipCustomer";
            IEdmSingleton expectedSingleton = _model.EntityContainer.FindSingleton("VipCustomer");
            Assert.NotNull(expectedSingleton); // Guard

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, ODataPath);
            Assert.NotNull(path); // Guard
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal("VipCustomer", segment.ToString());
            Assert.Equal("~/singleton", path.PathTemplate);
            Assert.Same(expectedSingleton, path.NavigationSource);
            Assert.Same(expectedSingleton.EntityType(), path.EdmType);
        }

        [Fact]
        public void CanParseKeyUrl()
        {
            // Arrange
            string odataPath = "RoutingCustomers(112)";
            string expectedText = "112";
            IEdmEntitySet expectedSet = _model.EntityContainer.EntitySets().SingleOrDefault(s => s.Name == "RoutingCustomers");

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.IsType<KeyValuePathSegment>(segment);
            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Same(expectedSet.EntityType(), path.EdmType);
        }

        [Fact]
        public void CanParseCastCollectionSegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers/System.Web.OData.Routing.VIP";
            string expectedText = "System.Web.OData.Routing.VIP";
            IEdmEntitySet expectedSet = _model.EntityContainer.EntitySets().SingleOrDefault(s => s.Name == "RoutingCustomers");
            IEdmEntityType expectedType = _model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "VIP");

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Equal(expectedType, (path.EdmType as IEdmCollectionType).ElementType.Definition);
        }

        [Fact]
        public void CanParseCastSingletonSegment()
        {
            // Arrange
            string odataPath = "VipCustomer/System.Web.OData.Routing.VIP";
            string expectedText = "System.Web.OData.Routing.VIP";
            IEdmSingleton expectedSingleton = _model.EntityContainer.FindSingleton("VipCustomer");
            IEdmEntityType expectedType = _model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "VIP");

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSingleton, path.NavigationSource);
            Assert.Equal(expectedType, path.EdmType);
        }

        [Fact]
        public void CanParseCastEntitySegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers(100)/System.Web.OData.Routing.VIP";
            string expectedText = "System.Web.OData.Routing.VIP";
            IEdmEntitySet expectedSet = _model.EntityContainer.EntitySets().SingleOrDefault(s => s.Name == "RoutingCustomers");
            IEdmEntityType expectedType = _model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "VIP");

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Equal(expectedType, path.EdmType);
        }

        [Fact]
        public void CanParseNavigateToCollectionSegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers(100)/Products";
            string expectedText = "Products";
            IEdmEntitySet expectedSet = _model.EntityContainer.EntitySets().SingleOrDefault(s => s.Name == "Products");
            IEdmNavigationProperty expectedEdmElement = _model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "RoutingCustomer").NavigationProperties().SingleOrDefault(n => n.Name == "Products");

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Equal(expectedSet.EntityType(), (path.EdmType as IEdmCollectionType).ElementType.Definition);
            NavigationPathSegment navigation = Assert.IsType<NavigationPathSegment>(segment);
            Assert.Same(expectedEdmElement, navigation.NavigationProperty);
        }

        [Fact]
        public void CanParseNavigateToSingleSegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers(100)/System.Web.OData.Routing.VIP/RelationshipManager";
            string expectedText = "RelationshipManager";
            IEdmEntitySet expectedSet = _model.EntityContainer.EntitySets().SingleOrDefault(s => s.Name == "SalesPeople");
            IEdmNavigationProperty expectedEdmElement = _model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "VIP").NavigationProperties().SingleOrDefault(n => n.Name == "RelationshipManager");

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Equal(expectedSet.EntityType(), path.EdmType);
            NavigationPathSegment navigation = Assert.IsType<NavigationPathSegment>(segment);
            Assert.Same(expectedEdmElement, navigation.NavigationProperty);
        }

        [Fact]
        public void CanParseRootProcedureSegment()
        {
            // Arrange
            string odataPath = "GetRoutingCustomerById()";
            string expectedText = "GetRoutingCustomerById";
            IEdmEntitySet expectedSet = _model.EntityContainer.EntitySets().SingleOrDefault(s => s.Name == "RoutingCustomers");
            IEdmActionImport expectedEdmElement = _model.EntityContainer
                .OperationImports()
                .SingleOrDefault(s => s.Name == "GetRoutingCustomerById") as IEdmActionImport;

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Equal(expectedSet.EntityType(), path.EdmType);
            UnboundActionPathSegment action = Assert.IsType<UnboundActionPathSegment>(segment);
            Assert.Same(expectedEdmElement, action.Action);
        }

        [Theory]
        [InlineData("RoutingCustomers(112)/Name")]
        [InlineData("VipCustomer/Name")]
        public void CanParsePropertySegment(string odataPath)
        {
            // Arrange
            string expectedText = "Name";
            IEdmProperty expectedEdmElement = _model.SchemaElements.OfType<IEdmEntityType>()
                .SingleOrDefault(e => e.Name == "RoutingCustomer").Properties().SingleOrDefault(p => p.Name == "Name");
            IEdmType expectedType = expectedEdmElement.Type.Definition;

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Null(path.NavigationSource);
            PropertyAccessPathSegment propertyAccess = Assert.IsType<PropertyAccessPathSegment>(segment);
            Assert.Same(expectedEdmElement, propertyAccess.Property);
        }

        [Theory]
        [InlineData("RoutingCustomers(112)/Address")]
        [InlineData("VipCustomer/Address")]
        public void CanParseComplexPropertySegment(string odataPath)
        {
            // Arrange
            string expectedText = "Address";
            IEdmProperty expectedEdmElement = _model.SchemaElements.OfType<IEdmEntityType>()
                .SingleOrDefault(e => e.Name == "RoutingCustomer").Properties().SingleOrDefault(p => p.Name == "Address");
            IEdmType expectedType = expectedEdmElement.Type.Definition;

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Null(path.NavigationSource);
            Assert.Same(expectedType, path.EdmType);
            PropertyAccessPathSegment propertyAccess = Assert.IsType<PropertyAccessPathSegment>(segment);
            Assert.Same(expectedEdmElement, propertyAccess.Property);
        }

        [Theory]
        [InlineData("RoutingCustomers(112)/Address/Street")]
        [InlineData("VipCustomer/Address/Street")]
        public void CanParsePropertyOfComplexSegment(string odataPath)
        {
            // Arrange
            string expectedText = "Street";
            IEdmProperty expectedEdmElement = _model.SchemaElements.OfType<IEdmComplexType>()
                .SingleOrDefault(e => e.Name == "Address").Properties().SingleOrDefault(p => p.Name == "Street");
            IEdmType expectedType = expectedEdmElement.Type.Definition;

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Null(path.NavigationSource);
            Assert.Same(expectedType, path.EdmType);
            PropertyAccessPathSegment propertyAccess = Assert.IsType<PropertyAccessPathSegment>(segment);
            Assert.Same(expectedEdmElement, propertyAccess.Property);
        }

        [Theory]
        [InlineData("RoutingCustomers(1)/Name/$value")]
        [InlineData("VipCustomer/Name/$value")]
        public void CanParsePropertyValueSegment(string odataPath)
        {
            // Arrange & Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal("$value", segment.ToString());
            Assert.Null(path.NavigationSource);
            Assert.NotNull(path.EdmType);
            Assert.Equal("Edm.String", (path.EdmType as IEdmPrimitiveType).FullName());
        }

        [Theory]
        [InlineData("RoutingCustomers(1)/Products/$ref")]
        [InlineData("VipCustomer/Products/$ref")]
        public void CanParseEntityLinksSegment(string odataPath)
        {
            // Arrange
            IEdmEntitySet expectedSet = _model.EntityContainer.EntitySets().SingleOrDefault(s => s.Name == "Products");
            IEdmEntityType expectedType = expectedSet.EntityType();

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Same(expectedType, (path.EdmType as IEdmCollectionType).ElementType.Definition);
            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Same("$ref", segment.ToString());
        }

        [Theory]
        [InlineData("RoutingCustomers(1)/Products/$ref?$id=../../Products(5)")]
        [InlineData("VipCustomer/Products/$ref?$id=" + _serviceRoot + "Products(5)")]
        public void CanParseDollarId(string odataPath)
        {
            // Arrange & Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);

            // Assert
            KeyValuePathSegment keyValuePathSegment =
                Assert.IsType<KeyValuePathSegment>(path.Segments[path.Segments.Count - 2]);
            Assert.Equal("5", keyValuePathSegment.Value);
            Assert.IsType<RefPathSegment>(path.Segments[path.Segments.Count - 1]);
        }

        [Theory]
        [InlineData("RoutingCustomers(1)/Products/$ref?$id=5", "The value of $id '5' is invalid.")]
        [InlineData("RoutingCustomers(1)/Products/$ref?$id=Products(5)", "The value of $id 'Products(5)' is invalid.")]
        [InlineData("RoutingCustomers(1)/Products/$ref?$id=../../RoutingCustomers(5)", "The value of $id '../../RoutingCustomers(5)' is invalid.")]
        [InlineData("VipCustomer/Products/$ref?$id=" + _serviceRoot + "RoutingCustomers(5)", "The value of $id '" + _serviceRoot + "RoutingCustomers(5)' is invalid.")]
        [InlineData("RoutingCustomers(1)/Products/$ref?$id=../../RoutingCustomers", "The value of $id '../../RoutingCustomers' is invalid.")]
        [InlineData("RoutingCustomers(1)/Products/$ref?$id=../../RoutingCustomers(1)/ID", "The value of $id '../../RoutingCustomers(1)/ID' is invalid.")]
        [InlineData("RoutingCustomers(1)/Products/$ref?$id=../../RoutingCustomers(1)/Products", "The value of $id '../../RoutingCustomers(1)/Products' is invalid.")]
        [InlineData("RoutingCustomers(1)/Products/$ref?$id=" + _serviceRoot, "The value of $id '" + _serviceRoot + "' is invalid.")]
        [InlineData("RoutingCustomers(1)/Products/$ref?$id=../unknown", "The value of $id '../unknown' is invalid.")]
        [InlineData("RoutingCustomers(1)/Products/$ref?$id=" + _serviceRoot + "unknown", "The value of $id '" + _serviceRoot + "unknown' is invalid.")]
        public void CannotParseDollarId_ThrowsODataException_InvalidDollarId(string odataPath, string expectedError)
        {
            // Arrange & Act & Assert
            Assert.Throws<ODataException>(() => _parser.Parse(_model, _serviceRoot, odataPath), expectedError);
        }

        [Theory]
        [InlineData("RoutingCustomers(1)/GetRelatedRoutingCustomers", "GetRelatedRoutingCustomers")]
        [InlineData("RoutingCustomers(2)/System.Web.OData.Routing.VIP/GetMostProfitable", "GetMostProfitable")]
        [InlineData("Products(7)/TopProductId", "TopProductId")]
        [InlineData("Products(7)/System.Web.OData.Routing.ImportantProduct/TopProductId", "TopProductId")]
        public void ParseAsUnresolvedPathSegment_UnqualifiedOperationPath(string odataPath, string unresolveValue)
        {
            // Arrange & Act & Assert
            UnresolvedPathSegment unresolvedPathSegment = Assert.IsType<UnresolvedPathSegment>(
                _parser.Parse(_model, _serviceRoot, odataPath).Segments.Last());
            Assert.Equal(unresolveValue, unresolvedPathSegment.SegmentValue);
        }

        [Fact]
        public void CannotParseSegmentAfterUnresolvedPathSegment()
        {
            // Arrange & Act & Assert
            Assert.Throws<ODataException>(
                () => _parser.Parse(_model, _serviceRoot, _serviceRoot + "RoutingCustomers(1)/GetRelatedRoutingCustomers/Segment"),
                "The URI segment 'Segment' is invalid after the segment 'GetRelatedRoutingCustomers'.");
        }

        [Fact]
        public void CanParseActionBoundToEntitySegment()
        {
            // Arrange
            string expectedText = "Default.GetRelatedRoutingCustomers";

            IEdmAction expectedEdmElement = _model.SchemaElements.OfType<IEdmAction>()
                .SingleOrDefault(e => e.Name == "GetRelatedRoutingCustomers");
            IEdmEntitySet expectedSet = _model.EntityContainer.EntitySets()
                .SingleOrDefault(e => e.Name == "RoutingCustomers");
            IEdmType expectedType = expectedEdmElement.ReturnType.Definition;

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, "RoutingCustomers(112)/Default.GetRelatedRoutingCustomers");
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Same(expectedType, path.EdmType);
            BoundActionPathSegment action = Assert.IsType<BoundActionPathSegment>(segment);
            Assert.Same(expectedEdmElement, action.Action);
        }

        [Fact]
        public void CanParseOnSingletonForActionBoundToEntitySegment()
        {
            // Arrange
            string expectedText = "Default.GetRelatedRoutingCustomers";

            IEdmAction expectedEdmElement = _model.SchemaElements.OfType<IEdmAction>()
                .SingleOrDefault(e => e.Name == "GetRelatedRoutingCustomers");
            IEdmEntitySet expectedEntitySet = _model.EntityContainer.FindEntitySet("RoutingCustomers");
            IEdmType expectedType = expectedEdmElement.ReturnType.Definition;

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, "VipCustomer/Default.GetRelatedRoutingCustomers");
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedEntitySet, path.NavigationSource);
            Assert.Same(expectedType, path.EdmType);
            BoundActionPathSegment action = Assert.IsType<BoundActionPathSegment>(segment);
            Assert.Same(expectedEdmElement, action.Action);
        }

        [Fact]
        public void CanParseActionBoundToCollectionSegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers/System.Web.OData.Routing.VIP/Default.GetMostProfitable";
            string expectedText = "Default.GetMostProfitable";
            IEdmAction expectedEdmElement = _model.SchemaElements.OfType<IEdmAction>().SingleOrDefault(e => e.Name == "GetMostProfitable");
            Assert.NotNull(expectedEdmElement);
            IEdmEntitySet expectedSet = _model.EntityContainer.EntitySets().SingleOrDefault(e => e.Name == "RoutingCustomers");
            IEdmType expectedType = expectedEdmElement.ReturnType.Definition;

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Same(expectedType, path.EdmType);
            BoundActionPathSegment action = Assert.IsType<BoundActionPathSegment>(segment);
            Assert.Same(expectedEdmElement, action.Action);
        }

        [Theory]
        [InlineData("Default.FunctionAtRoot()", "Default.FunctionAtRoot")]
        [InlineData("Default.Container.FunctionAtRoot()", "Default.Container.FunctionAtRoot")]
        [InlineData("Default.ActionAtRoot", "Default.ActionAtRoot")]
        [InlineData("Default.Container.ActionAtRoot", "Default.Container.ActionAtRoot")]
        public void CannotParseQualifiedUnboundOperation(string odataPath, string segmentName)
        {
            // Arrange
            var model = new CustomersModelWithInheritance();
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);

            model.Container.AddFunctionImport(new EdmFunction(model.Container.Namespace, "FunctionAtRoot",
                    returnType, isBound: false, entitySetPathExpression: null, isComposable: true));

            model.Container.AddActionImport(new EdmAction(model.Container.Namespace, "ActionAtRoot", returnType));

            // Act & Assert
            Assert.Throws<ODataUnrecognizedPathException>(
                () => _parser.Parse(model.Model, _serviceRoot, odataPath),
                String.Format("Resource not found for the segment '{0}'.", segmentName));
        }

        [Theory]
        [InlineData("RoutingCustomers(5)/Default.GetSpecialGuid()", "Default.GetSpecialGuid()")]
        [InlineData("RoutingCustomers(5)/System.Web.OData.Routing.VIP/Default.GetSpecialGuid()", "Default.GetSpecialGuid()")]
        [InlineData("RoutingCustomers(5)/Default.ActionBoundToSpecialVIP()", "Default.ActionBoundToSpecialVIP()")]
        public void ParseAsUnresolvePathSegment_OperationBoundToDerivedType(string uri, string unresolvedValue)
        {
            // Arrange & Act & Assert
            UnresolvedPathSegment unresolvedPathSegment = Assert.IsType<UnresolvedPathSegment>(
                _parser.Parse(_model, _serviceRoot, _serviceRoot + uri).Segments.Last());
            Assert.Equal(unresolvedValue, unresolvedPathSegment.SegmentValue);
        }

        [Theory]
        [InlineData("RoutingCustomers/Default.ActionBoundToSpecialVIPs()")]
        [InlineData("RoutingCustomers/Default.FunctionBoundToVIPs()")]
        public void CannotParseOperationBoundToDerivedCollectionType(string uri)
        {
            // Arrange & Act & Assert
            Assert.Throws<ODataException>(
                () => _parser.Parse(_model, _serviceRoot, _serviceRoot + uri),
                "The request URI is not valid. Since the segment 'RoutingCustomers' refers to a collection," +
                " this must be the last segment in the request URI or it must be followed by an function or action " +
                "that can be bound to it otherwise all intermediate segments must refer to a single resource.");
        }

        [Fact]
        public void ParseAsUnresolvedPathSegment_UnboundOperationAfterEntityType()
        {
            // Arrange & Act & Assert
            UnresolvedPathSegment unresolvedPathSegment = Assert.IsType<UnresolvedPathSegment>(
                _parser.Parse(_model, _serviceRoot, _serviceRoot + "RoutingCustomers(1)/Default.GetAllVIPs()").Segments.Last());
            Assert.Equal("Default.GetAllVIPs()", unresolvedPathSegment.SegmentValue);
        }

        [Fact]
        public void CannotParseUnboundOperationAfterEntityCollectionType()
        {
            // Arrange & Act & Assert
            Assert.Throws<ODataException>(
                () => _parser.Parse(_model, _serviceRoot, _serviceRoot + "RoutingCustomers/Default.GetAllVIPs()"),
                "The request URI is not valid. Since the segment 'RoutingCustomers' refers to a collection," +
                " this must be the last segment in the request URI or it must be followed by an function or action " +
                "that can be bound to it otherwise all intermediate segments must refer to a single resource.");
        }

        [Fact]
        public void CannotParseAmbiguousAction()
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            model.AddElement(container);

            container.AddActionImport(new EdmAction("NS", "AmbiguousAction", returnType: null));
            container.AddActionImport(new EdmAction("NS", "AmbiguousAction", returnType: null));

            // Act & Assert
            Assert.Throws<ODataException>(
                () => _parser.Parse(model, _serviceRoot, _serviceRoot + "AmbiguousAction"),
                "Multiple action import overloads were found with the same binding parameter for 'AmbiguousAction'.");
        }

        [Theory]
        [InlineData("Products(1)/Default.FunctionBoundToProductWithMultipleParamters()", "Default.FunctionBoundToProductWithMultipleParamters()")]
        [InlineData("Products(1)/Default.FunctionBoundToProductWithMultipleParamters(P1=1,P2=2)", "Default.FunctionBoundToProductWithMultipleParamters(P1=1,P2=2)")]
        [InlineData("Products(1)/Default.FunctionBoundToProductWithMultipleParamters(P1=1,P2=2,UnknownP3='a')", "Default.FunctionBoundToProductWithMultipleParamters(P1=1,P2=2,UnknownP3='a')")]
        [InlineData("Products(1)/Default.FunctionBoundToProductWithMultipleParamters(P1=1,P2=2,P3='a',UnknownP4=1)", "Default.FunctionBoundToProductWithMultipleParamters(P1=1,P2=2,P3='a',UnknownP4=1)")]
        [InlineData("Products(1)/Default.FunctionBoundToProduct(UnknownP1=1)", "Default.FunctionBoundToProduct(UnknownP1=1)")]
        [InlineData("Products(1)/Default.FunctionBoundToProduct(P1=1,P2=2,UnknownP3='a')", "Default.FunctionBoundToProduct(P1=1,P2=2,UnknownP3='a')")]
        [InlineData("Products(1)/Default.FunctionBoundToProduct(P1=1,P2=2,P3='a',UnknownP4=1)", "Default.FunctionBoundToProduct(P1=1,P2=2,P3='a',UnknownP4=1)")]
        public void ParseAsUnresolvePathSegment_FunctionBoundToEntityWithInvalidParameters(string uri, string unresolvedValue)
        {
            // Arrange & Act & Assert
            UnresolvedPathSegment unresolvedPathSegment = Assert.IsType<UnresolvedPathSegment>(
                _parser.Parse(_model, _serviceRoot, _serviceRoot + uri).Segments.Last());
            Assert.Equal(unresolvedValue, unresolvedPathSegment.SegmentValue);
        }

        [Theory]
        [InlineData("UnboundFunction(somekey=1)", "Resource not found for the segment 'UnboundFunction'.")]
        [InlineData("UnboundFunctionWithOneParamters(UnknownP1=1)", "Resource not found for the segment 'UnboundFunctionWithOneParamters'.")]
        [InlineData("UnboundFunctionWithOneParamters(UnknownP1=1,UnknownP2=2)", "Resource not found for the segment 'UnboundFunctionWithOneParamters'.")]
        [InlineData("UnboundFunctionWithOneParamters(P1=1,UnknownP2=2)", "Resource not found for the segment 'UnboundFunctionWithOneParamters'.")]
        [InlineData("UnboundFunctionWithMultipleParamters(P1=1,P2=2)", "Resource not found for the segment 'UnboundFunctionWithMultipleParamters'.")]
        public void CannotParseFunctionImportWithInvalidParameters(string uri, string expectedError)
        {
            // Arrange & Act & Assert
            Assert.Throws<ODataUnrecognizedPathException>(
                () => _parser.Parse(_model, _serviceRoot, _serviceRoot + uri),
                expectedError);
        }

        [Theory]
        [InlineData("EntitySet(1)/NS.OverloadBoundFunction(FunctionParameter=1)", "NS.OverloadBoundFunction(FunctionParameter=1)")]
        [InlineData("EntitySet(1)/NS.OverloadBoundFunction(FunctionParameter='abc')", "NS.OverloadBoundFunction(FunctionParameter='abc')")]
        public void ParseAsUnresolvePathSegment_OverloadBoundFunctionWithDifferentParamterType(string uri, string unresolvedValue)
        {
            // Arrange
            EdmModel model = new EdmModel();
            var entityType = new EdmEntityType("NS", "EntityTypeName");
            entityType.AddKeys(entityType.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            model.AddElement(entityType);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            model.AddElement(container);
            container.AddEntitySet("EntitySet", entityType);

            var boundFunction = new EdmFunction("NS", "OverloadBoundFunction", EdmCoreModel.Instance.GetInt32(false));
            boundFunction.AddParameter("bindingParameter", entityType.ToEdmTypeReference(false));
            boundFunction.AddParameter("FunctionParameter", EdmCoreModel.Instance.GetInt32(false));
            model.AddElement(boundFunction);

            boundFunction = new EdmFunction("NS", "OverloadBoundFunction", EdmCoreModel.Instance.GetInt32(false));
            boundFunction.AddParameter("bindingParameter", entityType.ToEdmTypeReference(false));
            boundFunction.AddParameter("FunctionParameter", EdmCoreModel.Instance.GetString(false));
            model.AddElement(boundFunction);

            // Act & Assert
            UnresolvedPathSegment unresolvedPathSegment = Assert.IsType<UnresolvedPathSegment>(
                _parser.Parse(model, _serviceRoot, _serviceRoot + uri).Segments.Last());
            Assert.Equal(unresolvedValue, unresolvedPathSegment.SegmentValue);
        }

        [Theory]
        [InlineData("OverloadUnboundFunction(Parameter=1)")]
        [InlineData("OverloadUnboundFunction(Parameter='abc')")]
        public void CannotParseOverloadUnboundFunctionWithDifferentParamterType(string uri)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            model.AddElement(container);

            var unboundFunction = new EdmFunction("NS", "OverloadUnboundFunction", EdmCoreModel.Instance.GetInt32(false));
            unboundFunction.AddParameter("Parameter", EdmCoreModel.Instance.GetInt32(false));
            model.AddElement(unboundFunction);

            unboundFunction = new EdmFunction("NS", "OverloadUnboundFunction", EdmCoreModel.Instance.GetInt32(false));
            unboundFunction.AddParameter("Parameter", EdmCoreModel.Instance.GetString(false));
            model.AddElement(unboundFunction);

            // Act & Assert
            Assert.Throws<ODataUnrecognizedPathException>(
                () => _parser.Parse(model, _serviceRoot, _serviceRoot + uri),
                "Resource not found for the segment 'OverloadUnboundFunction'.");
        }

        [Theory]
        [InlineData(typeof(SimpleEnum), "Microsoft.TestCommon.Types.SimpleEnum'123'")]
        [InlineData(typeof(SimpleEnum), "Microsoft.TestCommon.Types.SimpleEnum'-9999'")]
        [InlineData(typeof(FlagsEnum), "Microsoft.TestCommon.Types.FlagsEnum'999'")]
        [InlineData(typeof(FlagsEnum), "Microsoft.TestCommon.Types.FlagsEnum'-12345'")]
        public void CanParseUndefinedEnumValue(Type enumerationType, string enumerationExpression)
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EnumTypeConfiguration enumTypeConfiguration = builder.AddEnumType(enumerationType);
            FunctionConfiguration functionConfiguration = builder.Function("FunctionWithEnumParam");
            functionConfiguration.AddParameter("Enum", enumTypeConfiguration);
            functionConfiguration.Returns<int>();
            IEdmModel model = builder.GetEdmModel();
            string uri = String.Format("FunctionWithEnumParam(Enum={0})", enumerationExpression);

            // Act & Assert
            Assert.DoesNotThrow(() => _parser.Parse(model, _serviceRoot, uri));
        }

        [Theory]
        [InlineData(typeof(SimpleEnum), "Microsoft.TestCommon.Types.SimpleEnum'First, Second'")]
        [InlineData(typeof(SimpleEnum), "Microsoft.TestCommon.Types.SimpleEnum'UnknownValue'")]
        [InlineData(typeof(FlagsEnum), "Microsoft.TestCommon.Types.FlagsEnum'UnknownValue'")]
        [InlineData(typeof(FlagsEnum), "Microsoft.TestCommon.Types.FlagsEnum'abc'")]
        public void CannotParseInvalidEnumValue(Type enumerationType, string enumerationExpression)
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EnumTypeConfiguration enumTypeConfiguration = builder.AddEnumType(enumerationType);
            FunctionConfiguration functionConfiguration = builder.Function("FunctionWithEnumParam");
            functionConfiguration.AddParameter("Enum", enumTypeConfiguration);
            functionConfiguration.Returns<int>();
            IEdmModel model = builder.GetEdmModel();
            string uri = String.Format("FunctionWithEnumParam(Enum={0})", enumerationExpression);

            // Act & Assert
            Assert.Throws<ODataException>(
                () => _parser.Parse(model, _serviceRoot, uri),
                String.Format("The string '{0}' is not a valid enumeration type constant.", enumerationExpression));
        }

        [Fact]
        public void CanParse_UnboundFunction_AtRoot()
        {
            // Arrange
            var model = new CustomersModelWithInheritance();
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            var function = model.Container.AddFunctionImport(
                new EdmFunction(
                    model.Container.Namespace,
                    "FunctionAtRoot",
                    returnType,
                    isBound: false,
                    entitySetPathExpression: null,
                    isComposable: true));

            // Act
            ODataPath path = _parser.Parse(model.Model, _serviceRoot, "FunctionAtRoot");

            // Assert
            Assert.NotNull(path);
            Assert.Equal(1, path.Segments.Count);
            var functionSegment = Assert.IsType<UnboundFunctionPathSegment>(path.Segments.First());
            Assert.Same(function, functionSegment.Function);
            Assert.Empty(functionSegment.Values);
        }

        [Theory]
        [InlineData("Customers(42)/NS.IsSpecial", 3)]
        [InlineData("VipCustomer/NS.IsSpecial", 2)]
        public void CanParse_BoundFunction_AtEntity(string odataPath, int segmentCount)
        {
            // Arrange
            var model = new CustomersModelWithInheritance();
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmExpression entitySet = new EdmEntitySetReferenceExpression(model.Customers);
            var function = new EdmFunction(
                model.Container.Namespace,
                "IsSpecial",
                returnType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: true);
            function.AddParameter("entity", new EdmEntityTypeReference(model.Customer, isNullable: false));
            model.Model.AddElement(function);

            // Act
            ODataPath path = _parser.Parse(model.Model, _serviceRoot, odataPath);

            // Assert
            Assert.NotNull(path);
            Assert.Equal(segmentCount, path.Segments.Count);
            var functionSegment = Assert.IsType<BoundFunctionPathSegment>(path.Segments.Last());
            Assert.Same(function, functionSegment.Function);
            Assert.Empty(functionSegment.Values);
        }

        [Fact]
        public void CanParse_BoundFunction_AtEntityCollection()
        {
            // Arrange
            var model = new CustomersModelWithInheritance();
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmExpression entitySet = new EdmEntitySetReferenceExpression(model.Customers);
            var function = new EdmFunction(
                model.Container.Namespace,
                "Count",
                returnType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: true);
            IEdmTypeReference bindingParameterType = new EdmCollectionTypeReference(
                new EdmCollectionType(new EdmEntityTypeReference(model.Customer, isNullable: false)));
            function.AddParameter("customers", bindingParameterType);
            model.Model.AddElement(function);

            // Act
            ODataPath path = _parser.Parse(model.Model, _serviceRoot, "Customers/NS.Count");

            // Assert
            Assert.NotNull(path);
            Assert.Equal(2, path.Segments.Count);
            var functionSegment = Assert.IsType<BoundFunctionPathSegment>(path.Segments.Last());
            Assert.Same(function, functionSegment.Function);
            Assert.Empty(functionSegment.Values);
        }

        [Fact]
        public void CanParse_FunctionParameters_CanResolveInlineParameterValue()
        {
            // Arrange
            var model = new CustomersModelWithInheritance();
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            var function = new EdmFunction(
                model.Container.Namespace,
                "FunctionAtRoot",
                returnType,
                isBound: false,
                entitySetPathExpression: null,
                isComposable: true);
            function.AddParameter("IntParameter", EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false));
            model.Container.AddFunctionImport("FunctionAtRoot", function, entitySet: null);

            // Act
            ODataPath path = _parser.Parse(model.Model, _serviceRoot, "FunctionAtRoot(IntParameter=1)");
            UnboundFunctionPathSegment functionSegment = (UnboundFunctionPathSegment)path.Segments.Last();

            // Assert
            int intParameter = (int)functionSegment.GetParameterValue("IntParameter");
            Assert.Equal(1, intParameter);
        }

        [Theory]
        [PropertyData("NullFunctionParameterData")]
        public void CanParse_FunctionParameters_CanResolveAliasedParameterValueWithNull(object value, Type type)
        {
            // Arrange & Act
            object parameter = GetAliasedParameterValue(value, type);

            // Assert
            Assert.IsType<ODataNullValue>(parameter);
        }

        [Theory]
        [PropertyData("EnumFunctionParameterData")]
        public void CanParse_FunctionParameters_CanResolveAliasedParameterValueWithEnum(object value, Type type)
        {
            // Arrange & Act
            object parameter = GetAliasedParameterValue(value, type);

            // Assert
            Assert.IsType<ODataEnumValue>(parameter);
            Assert.Equal(((ODataEnumValue)value).Value, ((ODataEnumValue)parameter).Value);
            Assert.Equal(((ODataEnumValue)value).TypeName, ((ODataEnumValue)parameter).TypeName);
        }

        [Theory]
        [PropertyData("FunctionParameterData")]
        public void CanParse_FunctionParameters_CanResolveAliasedParameterValue(object value, Type type)
        {
            // Arrange & Act
            object parameter = GetAliasedParameterValue(value, type);

            // Assert
            Assert.Equal(value, parameter);
        }

        private object GetAliasedParameterValue(object value, Type type)
        {
            var model = new CustomersModelWithInheritance();
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            var function = new EdmFunction(
                model.Container.Namespace,
                "FunctionAtRoot",
                returnType,
                isBound: false,
                entitySetPathExpression: null,
                isComposable: true);

            model.Model.SetAnnotationValue(model.Model.FindType("NS.SimpleEnum"), new ClrTypeAnnotation(typeof(SimpleEnum)));
            function.AddParameter("Parameter", model.Model.GetEdmTypeReference(type));
            model.Container.AddFunctionImport("FunctionAtRoot", function, entitySet: null);

            ODataPath path = _parser.Parse(
                model.Model,
                _serviceRoot,
                "FunctionAtRoot(Parameter=@param)?@param=" + ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V4));
            UnboundFunctionPathSegment functionSegment = (UnboundFunctionPathSegment)path.Segments.Last();

            return functionSegment.GetParameterValue("Parameter");
        }

        [Fact]
        public void CanParse_FunctionParameters_CanResolveNestedAliasedParameterValues()
        {
            // Arrange
            var model = new CustomersModelWithInheritance();
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            var function = new EdmFunction(
                model.Container.Namespace,
                "BoundFunction",
                returnType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: true);

            model.Model.SetAnnotationValue(model.Model.FindType("NS.SimpleEnum"), new ClrTypeAnnotation(typeof(SimpleEnum)));
            function.AddParameter("bindingParameter", model.Customer.ToEdmTypeReference(false));
            function.AddParameter("IntParameter", model.Model.GetEdmTypeReference(typeof(int)));
            function.AddParameter("NullableDoubleParameter", model.Model.GetEdmTypeReference(typeof(double?)));
            function.AddParameter("StringParameter", model.Model.GetEdmTypeReference(typeof(string)));
            function.AddParameter("GuidParameter", model.Model.GetEdmTypeReference(typeof(Guid)));
            function.AddParameter("EnumParameter", model.Model.GetEdmTypeReference(typeof(SimpleEnum)));
            model.Model.AddElement(function);

            // Act
            ODataPath path = _parser.Parse(model.Model, _serviceRoot, String.Format(
                "Customers(1)/NS.BoundFunction(StringParameter=@p2,IntParameter=@p0,NullableDoubleParameter=@p1," +
                "EnumParameter=@p4,GuidParameter=@p3)?@p2={2}&@p4={4}&@p1={1}&@p0={0}&@p999={3}&@p3=@p999",
                ODataUriUtils.ConvertToUriLiteral(123, ODataVersion.V4),
                ODataUriUtils.ConvertToUriLiteral(null, ODataVersion.V4),
                ODataUriUtils.ConvertToUriLiteral("123", ODataVersion.V4),
                ODataUriUtils.ConvertToUriLiteral(Guid.Empty, ODataVersion.V4),
                ODataUriUtils.ConvertToUriLiteral(new ODataEnumValue("Third", "NS.SimpleEnum"), ODataVersion.V4)));

            BoundFunctionPathSegment functionSegment = (BoundFunctionPathSegment)path.Segments.Last();
            object intParameter = functionSegment.GetParameterValue("IntParameter");
            object nullableDoubleParameter = functionSegment.GetParameterValue("NullableDoubleParameter");
            object stringParameter = functionSegment.GetParameterValue("StringParameter");
            object guidParameter = functionSegment.GetParameterValue("GuidParameter");
            object enumParameter = functionSegment.GetParameterValue("EnumParameter");

            // Assert
            Assert.Equal(123, intParameter);
            Assert.IsType<ODataNullValue>(nullableDoubleParameter);
            Assert.Equal("123", stringParameter);
            Assert.Equal(Guid.Empty, guidParameter);
            Assert.IsType<ODataEnumValue>(enumParameter);
            Assert.Equal("2", ((ODataEnumValue)enumParameter).Value);
            Assert.Equal("NS.SimpleEnum", ((ODataEnumValue)enumParameter).TypeName);
        }

        [Fact]
        public void CanParse_FunctionParametersAlias_WithUnresolvedPathSegment()
        {
            // Arrange
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<ConventionCustomer>("Customers");
            FunctionConfiguration function = builder.Function("UnboundFunction");
            function.Parameter<int>("P1");
            function.Parameter<int>("P2");
            function.ReturnsFromEntitySet<ConventionCustomer>("Customers");
            function.IsComposable = true;
            IEdmModel model = builder.GetEdmModel();

            // Act
            ODataPath path = _parser.Parse(
                model,
                _serviceRoot,
                "UnboundFunction(P1=@p1,P2=@p2)/unknown?@p1=1&@p3=2&@p2=@p3");

            var functionSegment = (UnboundFunctionPathSegment)path.Segments.First();
            object p1 = functionSegment.GetParameterValue("P1");
            object p2 = functionSegment.GetParameterValue("P2");

            // Assert
            Assert.Equal(1, p1);
            Assert.Equal(2, p2);
        }

        [Theory]
        [PropertyData("NullFunctionParameterData")]
        public void CanParse_NullFunctionParameters(object value, Type type)
        {
            // Arrange & Act
            object parameter = GetParameterValue(value, type);

            // Assert
            Assert.IsType<ODataNullValue>(parameter);
        }

        [Theory]
        [PropertyData("EnumFunctionParameterData")]
        public void CanParse_EnumFunctionParameters(object value, Type type)
        {
            // Arrange & Act
            object parameter = GetParameterValue(value, type);

            // Assert
            Assert.IsType<ODataEnumValue>(parameter);
            Assert.Equal(((ODataEnumValue)value).Value, ((ODataEnumValue)parameter).Value);
            Assert.Equal(((ODataEnumValue)value).TypeName, ((ODataEnumValue)parameter).TypeName);
        }

        [Theory]
        [PropertyData("FunctionParameterData")]
        public void CanParse_FunctionParameters(object value, Type type)
        {
            // Arrange & Act
            object parameter = GetParameterValue(value, type);

            // Assert
            Assert.Equal(value, parameter);
        }

        private object GetParameterValue(object value, Type type)
        {
            var model = new CustomersModelWithInheritance();
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            var function = new EdmFunction(
                model.Container.Namespace,
                "FunctionAtRoot",
                returnType,
                isBound: false,
                entitySetPathExpression: null,
                isComposable: true);

            model.Model.SetAnnotationValue(model.Model.FindType("NS.SimpleEnum"), new ClrTypeAnnotation(typeof(SimpleEnum)));
            function.AddParameter("Parameter", model.Model.GetEdmTypeReference(type));
            model.Container.AddFunctionImport("FunctionAtRoot", function, entitySet: null);

            ODataPath path = _parser.Parse(
                model.Model,
                _serviceRoot,
                "FunctionAtRoot(Parameter=" + ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V4) + ")");
            UnboundFunctionPathSegment functionSegment = (UnboundFunctionPathSegment)path.Segments.Last();

            return functionSegment.GetParameterValue("Parameter");
        }

        [Theory]
        [InlineData("unBoundWithoutParams", 1, "Edm.Boolean", "~/unboundfunction")]
        [InlineData("unBoundWithoutParams()", 1, "Edm.Boolean", "~/unboundfunction")]
        [InlineData("unBoundWithOneParam(Param=false)", 1, "Edm.Boolean", "~/unboundfunction")]
        [InlineData("unBoundWithMultipleParams(Param1=false,Param2=false,Param3=true)", 1, "Edm.Boolean", "~/unboundfunction")]
        [InlineData("Customers(42)/NS.BoundToEntityNoParams()", 3, "Edm.Boolean", "~/entityset/key/function")]
        [InlineData("Customers(42)/NS.BoundToEntityNoParams", 3, "Edm.Boolean", "~/entityset/key/function")]
        [InlineData("Customers(42)/NS.BoundToEntity(Param=false)", 3, "Edm.Boolean", "~/entityset/key/function")]
        [InlineData("Customers(42)/NS.BoundToEntityReturnsEntityNoParams()", 3, "NS.Customer", "~/entityset/key/function")]
        [InlineData("Customers(42)/NS.BoundToEntityReturnsEntityNoParams()/ID", 4, "Edm.Int32", "~/entityset/key/function/property")]
        [InlineData("Customers(42)/NS.BoundToEntityReturnsEntityNoParams()/Orders(42)", 5, "NS.Order", "~/entityset/key/function/navigation/key")]
        [InlineData("Customers(42)/NS.BoundToEntityReturnsEntityNoParams()/NS.BoundToEntityReturnsEntityNoParams", 4, "NS.Customer", "~/entityset/key/function/function")]
        [InlineData("Customers(42)/NS.BoundToEntityReturnsEntityCollectionNoParams()", 3, "Collection([NS.Customer Nullable=False])", "~/entityset/key/function")]
        [InlineData("Customers/NS.BoundToEntityCollection", 2, "Edm.Boolean", "~/entityset/function")]
        [InlineData("Customers/NS.BoundToEntityCollection()", 2, "Edm.Boolean", "~/entityset/function")]
        [InlineData("Customers/NS.BoundToEntityCollectionReturnsComplex()", 2, "NS.Address", "~/entityset/function")]
        [InlineData("Customers/NS.BoundToEntityCollectionReturnsComplex()/City", 3, "Edm.String", "~/entityset/function/property")]
        [InlineData("VipCustomer/NS.BoundToEntityNoParams", 2, "Edm.Boolean", "~/singleton/function")]
        public void CanParse_Functions(string odataPath, int expectedCount, string expectedTypeName, string expectedTemplate)
        {
            // Arrange
            var model = GetModelWithFunctions();

            // Act
            ODataPath path = _parser.Parse(model, _serviceRoot, odataPath);

            // Assert
            Assert.NotNull(path);
            Assert.Equal(expectedCount, path.Segments.Count());
            Assert.Equal(expectedTypeName, path.EdmType.ToString());
            Assert.Equal(expectedTemplate, path.PathTemplate);
        }

        [Fact]
        public void CannotParse_KeySegmentAfterFunctionSegment()
        {
            // Arrange
            var model = GetModelWithFunctions();

            // Act & Assert
            Assert.Throws<ODataException>(
                () => _parser.Parse(
                    model,
                    _serviceRoot,
                    "Customers(42)/NS.BoundToEntityReturnsEntityCollectionNoParams()(42)"),
                "Bad Request - Error in query syntax.");
        }

        [Theory]
        [InlineData("BoundToEntityNoParams()")]
        [InlineData("BoundToEntityCollection()")]
        [InlineData("Customers(42)/BoundToEntityCollection()")]
        [InlineData("Customers(42)/ID/BoundToEntityCollection()")]
        [InlineData("Customers/unBoundWithoutParams()")]
        [InlineData("Customers(42)/unBoundWithoutParams()")]
        [InlineData("VipCustomer/unBoundWithoutParams()")]
        [InlineData("unBoundWithoutParams()/ID")]
        [InlineData("unBoundWithoutParams()/BoundToEntityNoParams()")]
        [InlineData("unBoundWithoutParams(Param=24)")]
        [InlineData("Customer(42)/BoundToEntityReturnsEntityCollectionNoParams(42)")] // should have an empty parentheses after the function call
        [InlineData("unBoundWithOneParam(ID='unterminated string literal")]
        public void ParseFunction_NegativeTests(string odataPath)
        {
            // Arrange
            bool exceptionThrown = false;
            var model = GetModelWithFunctions();
            ODataPath path = null;

            // Act
            try
            {
                path = _parser.Parse(model, _serviceRoot, odataPath);
            }
            catch (ODataException)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.True(path == null || path.EdmType == null || exceptionThrown);
        }

        [Theory]
        [InlineData("CustomersWithMultiKeys(Key1=1)")]
        [InlineData("CustomersWithMultiKeys(Key2=2)")]
        [InlineData("CustomersWithMultiKeys(Key3=3)")]
        [InlineData("CustomersWithMultiKeys(Key1=1,Key2=2,Key3=3)")]
        public void CannotParse_UnmatchedCountOfKeys(string path)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            EdmEntityType customerWithMultiKeys = new EdmEntityType("NS", "CustomerWithMultiKeys");
            customerWithMultiKeys.AddKeys(customerWithMultiKeys.AddStructuralProperty("Key1", EdmPrimitiveTypeKind.Int32));
            customerWithMultiKeys.AddKeys(customerWithMultiKeys.AddStructuralProperty("Key2", EdmPrimitiveTypeKind.Int32));
            model.Model.AddElement(customerWithMultiKeys);
            model.Container.AddEntitySet("CustomersWithMultiKeys", customerWithMultiKeys);

            // Act & Assert
            Assert.Throws<ODataException>(
                () => _parser.Parse(model.Model, _serviceRoot, path),
                "The number of keys specified in the URI does not match number of key properties for the resource 'NS.CustomerWithMultiKeys'.");
        }

        [Theory]
        [InlineData("RoutingCustomers(1)/Products/$ref/$value")]
        [InlineData("RoutingCustomers(1)/Products/$ref/$value/$value")]
        [InlineData("RoutingCustomers(1)/Products/$ref/$ref")]
        [InlineData("RoutingCustomers(1)/Products/$ref/$ref/something")]
        [InlineData("RoutingCustomers(1)/Products/$ref/something")]
        [InlineData("RoutingCustomers(1)/Products/$ref/5")]
        [InlineData("RoutingCustomers(1)/Products/$ref/$count")]
        public void DefaultODataPathHandler_ThrowsIfDollarRefIsNotTheLastSegment(string path)
        {
            // Arrange & Act & Assert
            Assert.Throws<ODataUnrecognizedPathException>(
                () => _parser.Parse(_model, _serviceRoot, path),
                "The request URI is not valid. The segment '$ref' must be the last segment in the URI because " +
                "it is one of the following: $batch, $value, $metadata, a collection property, a named media resource, " +
                "an action, a noncomposable function, an action import, or a noncomposable function import.");
        }

        [Theory]
        [InlineData("RoutingCustomers(1)/ID/$value/$ref")]
        [InlineData("RoutingCustomers(1)/ID/$value/$count")]
        [InlineData("RoutingCustomers(1)/ID/$value/$value")]
        [InlineData("RoutingCustomers(1)/ID/$value/$value/something")]
        [InlineData("RoutingCustomers(1)/ID/$value/something")]
        [InlineData("RoutingCustomers(1)/ID/$value/GetSpecialGuid()")]
        public void DefaultODataPathHandler_ThrowsIfDollarValueIsNotTheLastSegment(string path)
        {
            // Arrange & Act & Assert
            Assert.Throws<ODataUnrecognizedPathException>(
                () => _parser.Parse(_model, _serviceRoot, path),
                "The request URI is not valid. The segment '$value' must be the last segment in the URI because " +
                "it is one of the following: $batch, $value, $metadata, a collection property, a named media resource, " +
                "an action, a noncomposable function, an action import, or a noncomposable function import.");
        }

        private static IEdmModel GetModelWithFunctions()
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var container = model.Container;
            IEdmTypeReference boolType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmTypeReference customerType = model.Customer.AsReference();
            IEdmTypeReference addressType = new EdmComplexTypeReference(model.Address, isNullable: false);
            IEdmTypeReference customersType = new EdmCollectionTypeReference(new EdmCollectionType(customerType));

            AddFunction(model, "unBoundWithoutParams");

            var unBoundWithOneParam = AddFunction(model, "unBoundWithOneParam");
            unBoundWithOneParam.AddParameter("Param", boolType);

            var unBoundWithMultipleParams = AddFunction(model, "unBoundWithMultipleParams");
            unBoundWithMultipleParams.AddParameter("Param1", boolType);
            unBoundWithMultipleParams.AddParameter("Param2", boolType);
            unBoundWithMultipleParams.AddParameter("Param3", boolType);

            AddFunction(model, "BoundToEntityNoParams", bindingParameterType: customerType);

            var boundToEntity = AddFunction(model, "BoundToEntity", bindingParameterType: customerType);
            boundToEntity.AddParameter("Param", boolType);

            AddFunction(model, "BoundToEntityReturnsEntityNoParams",
                returnType: customerType, entitySet: model.Customers, bindingParameterType: customerType, entitySetPath: "bindingParameter");

            AddFunction(model, "BoundToEntityReturnsEntityCollectionNoParams",
                returnType: customersType, entitySet: model.Customers, bindingParameterType: customerType);

            AddFunction(model, "BoundToEntityCollection", bindingParameterType: customersType);

            AddFunction(model, "BoundToEntityCollectionReturnsComplex", bindingParameterType: customersType, returnType: addressType);

            return model.Model;
        }

        [Theory]
        [InlineData("RoutingCustomers", "RoutingCustomers", "RoutingCustomer", true)]
        [InlineData("RoutingCustomers/", "RoutingCustomers", "RoutingCustomer", true)]
        [InlineData("Products", "Products", "Product", true)]
        [InlineData("Products/", "Products", "Product", true)]
        [InlineData("SalesPeople", "SalesPeople", "SalesPerson", true)]
        public void CanResolveSetAndTypeViaSimpleEntitySetSegment(string odataPath, string expectedSetName, string expectedTypeName, bool isCollection)
        {
            // Arrange
            var model = _model;
            var expectedSet = model.EntityContainer.FindEntitySet(expectedSetName);
            var expectedType = model.FindDeclaredType("System.Web.OData.Routing." + expectedTypeName) as IEdmEntityType;

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.NotNull(path.NavigationSource);
            Assert.NotNull(path.EdmType);
            Assert.Same(expectedSet, path.NavigationSource);
            if (isCollection)
            {
                Assert.Equal(EdmTypeKind.Collection, path.EdmType.TypeKind);
                Assert.Same(expectedType, (path.EdmType as IEdmCollectionType).ElementType.Definition);
            }
            else
            {
                Assert.Same(expectedType, path.EdmType);
            }
        }

        [Theory]
        [InlineData("RoutingCustomers(1)", "RoutingCustomers", "RoutingCustomer", false)]
        [InlineData("RoutingCustomers(1)/", "RoutingCustomers", "RoutingCustomer", false)]
        [InlineData("Products(1)", "Products", "Product", false)]
        [InlineData("Products(1)/", "Products", "Product", false)]
        [InlineData("Products(1)", "Products", "Product", false)]
        public void CanResolveSetAndTypeViaKeySegment(string odataPath, string expectedSetName, string expectedTypeName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("RoutingCustomers(1)/Products", "Products", "Product", true)]
        [InlineData("RoutingCustomers(1)/Products(1)", "Products", "Product", false)]
        [InlineData("RoutingCustomers(1)/Products/", "Products", "Product", true)]
        [InlineData("RoutingCustomers(1)/Products", "Products", "Product", true)]
        [InlineData("Products(1)/RoutingCustomers", "RoutingCustomers", "RoutingCustomer", true)]
        [InlineData("Products(1)/RoutingCustomers(1)", "RoutingCustomers", "RoutingCustomer", false)]
        [InlineData("Products(1)/RoutingCustomers/", "RoutingCustomers", "RoutingCustomer", true)]
        [InlineData("VipCustomer/Products", "Products", "Product", true)]
        [InlineData("VipCustomer/Products(1)", "Products", "Product", false)]
        [InlineData("VipCustomer/Products/", "Products", "Product", true)]
        [InlineData("VipCustomer/Products", "Products", "Product", true)]
        [InlineData("MyProduct/RoutingCustomers", "RoutingCustomers", "RoutingCustomer", true)]
        [InlineData("MyProduct/RoutingCustomers(1)", "RoutingCustomers", "RoutingCustomer", false)]
        [InlineData("MyProduct/RoutingCustomers/", "RoutingCustomers", "RoutingCustomer", true)]
        public void CanResolveSetAndTypeViaNavigationPropertySegment(string odataPath, string expectedSetName, string expectedTypeName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("RoutingCustomers/System.Web.OData.Routing.VIP", "VIP", "RoutingCustomers", true)]
        [InlineData("RoutingCustomers(1)/System.Web.OData.Routing.VIP", "VIP", "RoutingCustomers", false)]
        [InlineData("Products(1)/System.Web.OData.Routing.ImportantProduct", "ImportantProduct", "Products", false)]
        [InlineData("Products(1)/RoutingCustomers/System.Web.OData.Routing.VIP", "VIP", "RoutingCustomers", true)]
        [InlineData("SalesPeople(1)/ManagedRoutingCustomers", "VIP", "RoutingCustomers", true)]
        [InlineData("RoutingCustomers(1)/System.Web.OData.Routing.VIP/RelationshipManager", "SalesPerson", "SalesPeople", false)]
        [InlineData("Products/System.Web.OData.Routing.ImportantProduct(1)/LeadSalesPerson", "SalesPerson", "SalesPeople", false)]
        [InlineData("Products(1)/RoutingCustomers/System.Web.OData.Routing.VIP(1)/RelationshipManager/ManagedProducts", "ImportantProduct", "Products", true)]
        public void CanResolveSetAndTypeViaCastSegment(string odataPath, string expectedTypeName, string expectedSetName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("GetRoutingCustomerById", "RoutingCustomer", "RoutingCustomers", false)]
        [InlineData("GetSalesPersonById", "SalesPerson", "SalesPeople", false)]
        [InlineData("GetAllVIPs", "VIP", "RoutingCustomers", true)]
        public void CanResolveSetAndTypeViaRootProcedureSegment(string odataPath, string expectedTypeName, string expectedSetName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("RoutingCustomers(1)/Default.GetRelatedRoutingCustomers", "RoutingCustomer", "RoutingCustomers", true)]
        [InlineData("RoutingCustomers(1)/Default.GetBestRelatedRoutingCustomer", "VIP", "RoutingCustomers", false)]
        [InlineData("RoutingCustomers(1)/System.Web.OData.Routing.VIP/Default.GetSalesPerson", "SalesPerson", "SalesPeople", false)]
        [InlineData("SalesPeople(1)/Default.GetVIPRoutingCustomers", "VIP", "RoutingCustomers", true)]
        public void CanResolveSetAndTypeViaEntityActionSegment(string odataPath, string expectedTypeName, string expectedSetName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("VipCustomer/Default.GetRelatedRoutingCustomers", "RoutingCustomer", "RoutingCustomers", true)]
        [InlineData("VipCustomer/System.Web.OData.Routing.VIP/Default.GetSalesPerson", "SalesPerson", "SalesPeople", false)]
        public void CanResolveSetAndTypeViaSingletonSegment(string odataPath, string expectedTypeName, string expectedSetName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("RoutingCustomers/Default.GetVIPs", "VIP", "RoutingCustomers", true)]
        [InlineData("RoutingCustomers/Default.GetProducts", "Product", "Products", true)]
        [InlineData("RoutingCustomers/System.Web.OData.Routing.VIP/Default.GetProducts", "Product", "Products", true)]
        [InlineData("Products(1)/RoutingCustomers/System.Web.OData.Routing.VIP/Default.GetSalesPeople", "SalesPerson", "SalesPeople", true)]
        [InlineData("MyProduct/RoutingCustomers/System.Web.OData.Routing.VIP/Default.GetSalesPeople", "SalesPerson", "SalesPeople", true)]
        [InlineData("SalesPeople/Default.GetVIPRoutingCustomers", "VIP", "RoutingCustomers", true)]
        [InlineData("RoutingCustomers/System.Web.OData.Routing.VIP/Default.GetMostProfitable", "VIP", "RoutingCustomers", false)]
        public void CanResolveSetAndTypeViaCollectionActionSegment(string odataPath, string expectedTypeName, string expectedSetName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("Vehicles/NS.Car", "Collection([NS.Car Nullable=False])")]
        [InlineData("Vehicles/NS.Motorcycle", "Collection([NS.Motorcycle Nullable=False])")]
        [InlineData("Vehicles/NS.Vehicle", "Collection([NS.Vehicle Nullable=False])")]
        [InlineData("Cars/NS.Vehicle", "Collection([NS.Vehicle Nullable=False])")]
        [InlineData("Motorcycles/NS.Vehicle", "Collection([NS.Vehicle Nullable=False])")]
        [InlineData("Vehicles(42)/NS.Car", "NS.Car")]
        [InlineData("Vehicles(42)/NS.Motorcycle", "NS.Motorcycle")]
        [InlineData("Vehicles(42)/NS.Vehicle", "NS.Vehicle")]
        [InlineData("Cars(42)/NS.Vehicle", "NS.Vehicle")]
        [InlineData("Motorcycles(42)/NS.Vehicle", "NS.Vehicle")]
        [InlineData("Contoso/NS.Car", "NS.Car")]
        [InlineData("Contoso/NS.Motorcycle", "NS.Motorcycle")]
        [InlineData("Contoso/NS.Vehicle", "NS.Vehicle")]
        public void CastTests(string path, string expectedEdmType)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            var vehicle = new EdmEntityType("NS", "Vehicle");
            vehicle.AddKeys(vehicle.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            var car = new EdmEntityType("NS", "Car", vehicle);
            var motorcycle = new EdmEntityType("NS", "Motorcycle", vehicle);
            model.AddElements(new IEdmSchemaElement[] { vehicle, car, motorcycle, container });

            container.AddEntitySet("Vehicles", vehicle);
            container.AddEntitySet("Cars", car);
            container.AddEntitySet("Motorcycles", motorcycle);
            container.AddSingleton("Contoso", vehicle);

            // Act
            ODataPath odataPath = _parser.Parse(model, _serviceRoot, path);

            // Assert
            Assert.NotNull(odataPath);
            Assert.Equal(expectedEdmType, odataPath.EdmType.ToTraceString());
        }

        [Theory]
        [InlineData("Vehicles/NS.Car/NS.Motorcycle",
            "The type 'NS.Motorcycle' specified in the URI is neither a base type nor a sub-type of the previously-specified type 'NS.Car'.")]
        [InlineData("Vehicles/NS.Motorcycle/NS.Car",
            "The type 'NS.Car' specified in the URI is neither a base type nor a sub-type of the previously-specified type 'NS.Motorcycle'.")]
        [InlineData("Cars/NS.Motorcycle",
            "The type 'NS.Motorcycle' specified in the URI is neither a base type nor a sub-type of the previously-specified type 'NS.Car'.")]
        [InlineData("Motorcycles/NS.Car",
            "The type 'NS.Car' specified in the URI is neither a base type nor a sub-type of the previously-specified type 'NS.Motorcycle'.")]
        [InlineData("Vehicles(42)/NS.Car/NS.Motorcycle",
            "The type 'NS.Motorcycle' specified in the URI is neither a base type nor a sub-type of the previously-specified type 'NS.Car'.")]
        [InlineData("Cars(42)/NS.Motorcycle",
            "The type 'NS.Motorcycle' specified in the URI is neither a base type nor a sub-type of the previously-specified type 'NS.Car'.")]
        [InlineData("Motorcycles(42)/NS.Car",
            "The type 'NS.Car' specified in the URI is neither a base type nor a sub-type of the previously-specified type 'NS.Motorcycle'.")]
        public void Invalid_CastTests(string path, string expectedError)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            var vehicle = new EdmEntityType("NS", "Vehicle");
            vehicle.AddKeys(vehicle.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            var car = new EdmEntityType("NS", "Car", vehicle);
            var motorcycle = new EdmEntityType("NS", "Motorcycle", vehicle);
            model.AddElements(new IEdmSchemaElement[] { vehicle, car, motorcycle, container });

            container.AddEntitySet("Vehicles", vehicle);
            container.AddEntitySet("Cars", car);
            container.AddEntitySet("Motorcycles", motorcycle);

            // Act & Assert
            Assert.Throws<ODataException>(
                () => _parser.Parse(model, _serviceRoot, path),
                expectedError);
        }

        [Theory]
        [InlineData("Vehicles(42)/NS.Wash", "Wash", "NS.Vehicle")]
        [InlineData("Vehicles(42)/NS.Car/NS.Wash", "Wash", "NS.Car")] // upcast
        [InlineData("Vehicles(42)/NS.Motorcycle/NS.Wash", "Wash", "NS.Vehicle")]
        [InlineData("Cars(42)/NS.Vehicle/NS.Wash", "Wash", "NS.Vehicle")] // downcast
        [InlineData("Contoso/NS.Car/NS.Wash", "Wash", "NS.Car")] // singleton
        [InlineData("Vehicles/NS.WashMultiple", "WashMultiple", "Collection([NS.Vehicle Nullable=False])")]
        [InlineData("Vehicles/NS.Car/NS.WashMultiple", "WashMultiple", "Collection([NS.Car Nullable=False])")] // upcast
        [InlineData("Vehicles/NS.Motorcycle/NS.WashMultiple", "WashMultiple", "Collection([NS.Vehicle Nullable=False])")]
        [InlineData("Cars/NS.Vehicle/NS.WashMultiple", "WashMultiple", "Collection([NS.Vehicle Nullable=False])")] // downcast
        public void ActionOverloadResoultionTests(string path, string actionName, string expectedEntityBound)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            var vehicle = new EdmEntityType("NS", "Vehicle");
            vehicle.AddKeys(new EdmStructuralProperty(vehicle, "ID", EdmCoreModel.Instance.GetInt32(false)));
            var car = new EdmEntityType("NS", "Car", vehicle);
            var motorcycle = new EdmEntityType("NS", "Motorcycle", vehicle);
            model.AddElements(new IEdmSchemaElement[] { vehicle, car, motorcycle, container });

            var washVehicle = AddBindableAction(model, "Wash", vehicle, isCollection: false);
            var washCar = AddBindableAction(model, "Wash", car, isCollection: false);
            var washVehicles = AddBindableAction(model, "WashMultiple", vehicle, isCollection: true);
            var washCars = AddBindableAction(model, "WashMultiple", car, isCollection: true);

            container.AddEntitySet("Vehicles", vehicle);
            container.AddEntitySet("Cars", car);
            container.AddEntitySet("Motorcycles", motorcycle);
            container.AddSingleton("Contoso", vehicle);

            // Act
            ODataPath odataPath = _parser.Parse(model, _serviceRoot, path);

            // Assert
            Assert.NotNull(odataPath);
            BoundActionPathSegment actionSegment = Assert.IsType<BoundActionPathSegment>(odataPath.Segments.Last());
            Assert.Equal("NS." + actionName, actionSegment.ActionName);
            Assert.Equal(expectedEntityBound, actionSegment.Action.Parameters.First().Type.Definition.ToTraceString());
        }

        [Theory]
        [InlineData("Customers(1)/Products")]
        [InlineData("Customers(1)/Products(5)")]
        [InlineData("Customers(1)/Products(5)/ID")]
        [InlineData("Customers(1)/Products/$ref")]
        [InlineData("Customers(1)/Products(5)/$ref")]
        public void DefaultODataPathHandler_Throws_NotNavigablePropertyInPath(string path)
        {
            // Arrange
            var builder = new ODataConventionModelBuilder();
            var customer = builder.EntitySet<ODataRoutingModel.RoutingCustomer>("Customers").EntityType;
            customer.HasMany(c => c.Products).IsNotNavigable();
            builder.EntitySet<ODataRoutingModel.Product>("Products");
            var model = builder.GetEdmModel();

            // Act & Assert
            Assert.Throws<ODataException>(
                () => _parser.Parse(model, _serviceRoot, path),
                "The property 'Products' cannot be used for navigation.");
        }

        private static IEdmActionImport AddUnboundAction(EdmEntityContainer container, string name, IEdmEntityType bindingType, bool isCollection)
        {
            var action = new EdmAction(
                container.Namespace, name, returnType: null, isBound: true, entitySetPathExpression: null);

            IEdmTypeReference bindingParamterType = new EdmEntityTypeReference(bindingType, isNullable: false);
            if (isCollection)
            {
                bindingParamterType = new EdmCollectionTypeReference(new EdmCollectionType(bindingParamterType));
            }

            action.AddParameter("bindingParameter", bindingParamterType);
            var actionImport = container.AddActionImport(action);
            return actionImport;
        }

        private static IEdmAction AddBindableAction(EdmModel model, string name, IEdmEntityType bindingType, bool isCollection)
        {
            IEdmEntityContainer container = model.EntityContainer;
            var action = new EdmAction(
                container.Namespace, name, returnType: null, isBound: true, entitySetPathExpression: null);

            IEdmTypeReference bindingParamterType = new EdmEntityTypeReference(bindingType, isNullable: false);
            if (isCollection)
            {
                bindingParamterType = new EdmCollectionTypeReference(new EdmCollectionType(bindingParamterType));
            }

            action.AddParameter("bindingParameter", bindingParamterType);
            model.AddElement(action);
            return action;
        }

        [Theory]
        [InlineData("Customers", "Customers", new string[] { })]
        [InlineData("Customers(42)", "Customers({key})", new string[] { "key:42" })]
        [InlineData("Customers(ID=42)", "Customers(ID={key})", new string[] { "key:42" })]
        [InlineData("CustomersWithMultiKeys(ID1=1,ID2=2)", "CustomersWithMultiKeys(ID1={key1},ID2={key2})", new string[] { "key1:1", "key2:2" })]
        [InlineData("CustomersWithMultiKeys(ID2=2,ID1=1)", "CustomersWithMultiKeys(ID1={key1},ID2={key2})", new string[] { "key1:1", "key2:2" })]
        [InlineData("Customers(42)/Orders(24)", "Customers({customerID})/Orders({orderID})", new string[] { "customerID:42", "orderID:24" })]
        [InlineData("Function(foo=42,bar=true)", "Function(foo={newFoo},bar={newBar})", new string[] { "newFoo:42", "newBar:true" })]
        [InlineData("Function(bar=false,foo=24)", "Function(foo={newFoo},bar={newBar})", new string[] { "newFoo:24", "newBar:false" })]
        public void ParseTemplate(string path, string template, string[] keyValues)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var function = AddFunction(model, "Function");
            function.AddParameter("foo", EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false));
            function.AddParameter("bar", EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false));

            EdmEntityType customerWithMultiKeys = new EdmEntityType("NS", "CustomerWithMultiKeys");
            customerWithMultiKeys.AddKeys(customerWithMultiKeys.AddStructuralProperty("ID1", EdmPrimitiveTypeKind.Int32));
            customerWithMultiKeys.AddKeys(customerWithMultiKeys.AddStructuralProperty("ID2", EdmPrimitiveTypeKind.Int32));
            model.Model.AddElement(customerWithMultiKeys);
            model.Container.AddEntitySet("CustomersWithMultiKeys", customerWithMultiKeys);

            // Act
            ODataPath odataPath = _parser.Parse(model.Model, _serviceRoot, path);
            ODataPathTemplate odataPathTemplate = _parser.ParseTemplate(model.Model, template);

            // Assert
            Dictionary<string, object> routeData = new Dictionary<string, object>();
            Assert.True(odataPathTemplate.TryMatch(odataPath, routeData));
            Assert.Equal(keyValues.OrderBy(k => k), routeData.Select(d => d.Key + ":" + d.Value).OrderBy(d => d));
        }

        [Theory]
        [InlineData("Customer", "Customer")] // Customer is not a correct entity set in the model
        [InlineData("UnknowFunction(foo={newFoo})", "UnknowFunction")] // UnknowFunction is not a function name in the model
        public void ParseTemplate_ThrowODataException_InvalidODataPathSegmentTemplate(string template, string segmentValue)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();

            // Act & Assert
            Assert.Throws<ODataUnrecognizedPathException>(
                () => _parser.ParseTemplate(model.Model, template),
                String.Format("Resource not found for the segment '{0}'.", segmentValue));
        }

        [Fact]
        public void ParseTemplate_ThrowODataException_UnResolvedPathSegment()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();

            // Act & Assert
            Assert.Throws<ODataException>(() => _parser.ParseTemplate(model.Model, "Customers(ID={key})/Order"),
                "Found an unresolved path segment 'Order' in the OData path template 'Customers(ID={key})/Order'.");
        }

        private static void AssertTypeMatchesExpectedTypeForSingleton(string odataPath, string expectedSingletonName, string expectedTypeName, bool isCollection)
        {
            var expectedSet = _model.EntityContainer.FindSingleton(expectedSingletonName);
            AssertTypeMatchesExpectedTypeInline(odataPath, expectedSet, expectedTypeName, isCollection);
        }

        private static void AssertTypeMatchesExpectedType(string odataPath, string expectedSetName, string expectedTypeName, bool isCollection)
        {
            var expectedSet = _model.EntityContainer.FindEntitySet(expectedSetName);
            AssertTypeMatchesExpectedTypeInline(odataPath, expectedSet, expectedTypeName, isCollection);
        }

        private static void AssertTypeMatchesExpectedTypeInline(string odataPath, IEdmNavigationSource expectedNavigationSource,
            string expectedTypeName, bool isCollection)
        {
            // Arrange
            var expectedType = _model.FindDeclaredType("System.Web.OData.Routing." + expectedTypeName) as IEdmEntityType;

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.NotNull(path.NavigationSource);
            Assert.NotNull(path.EdmType);
            Assert.Same(expectedNavigationSource, path.NavigationSource);
            if (isCollection)
            {
                Assert.Equal(EdmTypeKind.Collection, path.EdmType.TypeKind);
                Assert.Same(expectedType, (path.EdmType as IEdmCollectionType).ElementType.Definition);
            }
            else
            {
                Assert.Same(expectedType, path.EdmType);
            }
        }

        private static EdmFunction AddFunction(CustomersModelWithInheritance model, string name, IEdmTypeReference returnType = null,
            IEdmEntitySet entitySet = null, IEdmTypeReference bindingParameterType = null, string entitySetPath = null)
        {
            returnType = returnType ?? EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmExpression expression = entitySet == null ? null : new EdmEntitySetReferenceExpression(entitySet);

            var function = new EdmFunction(
                model.Container.Namespace,
                name,
                returnType,
                isBound: bindingParameterType != null,
                entitySetPathExpression: entitySetPath == null ? null : new EdmPathExpression(entitySetPath),
                isComposable: true);
            if (bindingParameterType != null)
            {
                function.AddParameter("bindingParameter", bindingParameterType);
                model.Model.AddElement(function);
            }
            else
            {
                model.Container.AddFunctionImport(name, function, expression);
            }

            return function;
        }
    }
}
