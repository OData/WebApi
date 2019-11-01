// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Types;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Test.Routing
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
                data.Add(null, typeof(Date?));
                data.Add(null, typeof(TimeOfDay?));
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
                // Enable the following if fix https://github.com/OData/odata.net/issues/83
                // data.Add(new Date(1997, 7, 1), typeof(Date));
                data.Add(new TimeOfDay(10, 11, 12, 13), typeof(TimeOfDay));
                data.Add(new TimeSpan(23, 59, 59), typeof(TimeSpan));
                data.Add(Guid.NewGuid(), typeof(Guid));

                data.Add(-1, typeof(int?));
                data.Add(false, typeof(bool?));
                data.Add((long)123, typeof(long?));
                data.Add((Single)123, typeof(Single?));
                data.Add(1.23, typeof(double?));
                data.Add("abc", typeof(string));
                data.Add(new DateTimeOffset(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc)), typeof(DateTimeOffset?));
                // Enable the following if fix https://github.com/OData/odata.net/issues/83
                // data.Add(new Date(1997, 7, 1), typeof(Date?));
                data.Add(new TimeOfDay(10, 11, 12, 13), typeof(TimeOfDay?));
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

            EntitySetSegment entitySetSegment = Assert.IsType<EntitySetSegment>(path.Segments.First());
            Assert.Equal("üCategories", entitySetSegment.EntitySet.Name);

            Assert.Equal("Collection(Microsoft.AspNet.OData.Test.Routing.üCategory)", entitySetSegment.EdmType.FullTypeName());
        }

        [Fact]
        public void Parse_ForInvalidCast_ThrowsODataException()
        {
            string odataPath = "RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.Product";

            ExceptionAssert.Throws<ODataException>(
                () => _parser.Parse(_model, _serviceRoot, odataPath),
                "The type 'Microsoft.AspNet.OData.Test.Routing.Product' specified in the URI is neither a base type " +
                "nor a sub-type of the previously-specified type 'Microsoft.AspNet.OData.Test.Routing.RoutingCustomer'.");
        }

        [Fact]
        public void Parse_ForSegmentAfterMetadata_ThrowsODataException()
        {
            string odataPath = "$metadata/foo";

            ExceptionAssert.Throws<ODataUnrecognizedPathException>(
                () => _parser.Parse(_model, _serviceRoot, odataPath),
                "The request URI is not valid. The segment '$metadata' must be the last segment in the URI because " +
                "it is one of the following: $ref, $batch, $count, $value, $metadata, a named media resource, " +
                "an action, a noncomposable function, an action import, a noncomposable function import, " +
                "an operation with void return type, or an operation import with void return type.");
        }

        [Theory]
        [InlineData("", "~", "")]
        [InlineData("$metadata", "~/$metadata", "$metadata")]
        [InlineData("$batch", "~/$batch", "$batch")]
        [InlineData("RoutingCustomers(112)", "~/entityset/key", "RoutingCustomers(112)")]
        [InlineData("RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP", "~/entityset/cast", "RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP")]
        [InlineData("RoutingCustomers(100)/Products", "~/entityset/key/navigation", "RoutingCustomers(100)/Products")]
        [InlineData("RoutingCustomers(100)/Address/Unknown", "~/entityset/key/property/unresolved", "RoutingCustomers(100)/Address/Unknown")]
        [InlineData("RoutingCustomers(100)/Products()", "~/entityset/key/navigation", "RoutingCustomers(100)/Products")]
        [InlineData("RoutingCustomers(100)/Microsoft.AspNet.OData.Test.Routing.VIP/RelationshipManager", "~/entityset/key/cast/navigation",
            "RoutingCustomers(100)/Microsoft.AspNet.OData.Test.Routing.VIP/RelationshipManager")]
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
        [InlineData("RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetMostProfitable", "~/entityset/cast/action",
            "RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetMostProfitable")]
        [InlineData("RoutingCustomers(112)/Default.GetOrdersCount(factor=1)", "~/entityset/key/function",
            "RoutingCustomers(112)/Default.GetOrdersCount(factor=1)")]
        [InlineData("RoutingCustomers(112)/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetOrdersCount(factor=1)", "~/entityset/key/cast/function",
            "RoutingCustomers(112)/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetOrdersCount(factor=1)")]
        [InlineData("RoutingCustomers/Default.FunctionBoundToRoutingCustomers()", "~/entityset/function",
            "RoutingCustomers/Default.FunctionBoundToRoutingCustomers()")]
        [InlineData("RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP/Default.FunctionBoundToRoutingCustomers()", "~/entityset/cast/function",
            "RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP/Default.FunctionBoundToRoutingCustomers()")]
        [InlineData("Products(1)/RoutingCustomers(1)/Microsoft.AspNet.OData.Test.Routing.VIP/RelationshipManager/ManagedProducts",
            "~/entityset/key/navigation/key/cast/navigation/navigation",
            "Products(1)/RoutingCustomers(1)/Microsoft.AspNet.OData.Test.Routing.VIP/RelationshipManager/ManagedProducts")]
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
        [InlineData("VipCustomer/Microsoft.AspNet.OData.Test.Routing.VIP", "~/singleton/cast", "VipCustomer/Microsoft.AspNet.OData.Test.Routing.VIP")]
        [InlineData("VipCustomer/Products", "~/singleton/navigation", "VipCustomer/Products")]
        [InlineData("VipCustomer/Microsoft.AspNet.OData.Test.Routing.VIP/RelationshipManager", "~/singleton/cast/navigation",
            "VipCustomer/Microsoft.AspNet.OData.Test.Routing.VIP/RelationshipManager")]
        [InlineData("VipCustomer/Name/$value", "~/singleton/property/$value", "VipCustomer/Name/$value")]
        [InlineData("VipCustomer/Products/$ref", "~/singleton/navigation/$ref", "VipCustomer/Products/$ref")]
        [InlineData("VipCustomer/Default.GetRelatedRoutingCustomers", "~/singleton/action", "VipCustomer/Default.GetRelatedRoutingCustomers")]
        [InlineData("MyProduct/Default.TopProductId()", "~/singleton/function", "MyProduct/Default.TopProductId()")]
        [InlineData("RoutingCustomers/$count", "~/entityset/$count", "RoutingCustomers/$count")]
        [InlineData("RoutingCustomers(100)/Products/$count", "~/entityset/key/navigation/$count", "RoutingCustomers(100)/Products/$count")]
        [InlineData("UnboundFunction()/$count", "~/unboundfunction/$count", "UnboundFunction()/$count")]
        [InlineData("SalesPeople(100)/Foo", "~/entityset/key/dynamicproperty", "SalesPeople(100)/Foo")]
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
        [InlineData("RoutingCustomers(100)/Microsoft.AspNet.OData.Test.Routing.VIP/RelationshipManager", new string[] { "RoutingCustomers(100)", "Microsoft.AspNet.OData.Test.Routing.VIP", "RelationshipManager" })]
        [InlineData("RoutingCustomers(112)/Address/Street", new string[] { "RoutingCustomers(112)", "Address", "Street" })]
        [InlineData("RoutingCustomers(1)/Name/$value", new string[] { "RoutingCustomers(1)", "Name", "$value" })]
        [InlineData("RoutingCustomers(1)/Products/$ref", new string[] { "RoutingCustomers(1)", "Products", "$ref" })]
        [InlineData("VipCustomer/Default.GetRelatedRoutingCustomers", new string[] { "VipCustomer", "Default.GetRelatedRoutingCustomers" })]
        [InlineData("SalesPeople(100)/Foo", new string[] { "SalesPeople(100)", "Foo" })]
        public void ParseSegmentsCorrectly(string odataPath, string[] expectedSegments)
        {
            // Arrange & Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);

            // Assert
            Assert.Equal(String.Join("/", expectedSegments), path.ToString());
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

            MetadataSegment metadataSegment = Assert.IsType<MetadataSegment>(segment);
            Assert.Equal("$metadata", metadataSegment.ToUriLiteral());
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

            BatchSegment batch = Assert.IsType<BatchSegment>(segment);
            Assert.Same(BatchSegment.Instance, batch);
            Assert.Equal("$batch", batch.ToUriLiteral());
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

            EntitySetSegment entitySet = Assert.IsType<EntitySetSegment>(segment);

            Assert.Equal(expectedText, entitySet.ToUriLiteral());
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
                .SingleOrDefault(s => s.FullName() == "Microsoft.AspNet.OData.Test.Routing.VIP");

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, "RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP");
            Assert.NotNull(path); // Guard
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal("~/entityset/cast", path.PathTemplate);

            TypeSegment typeSegment = Assert.IsType<TypeSegment>(segment);
            Assert.Equal("Microsoft.AspNet.OData.Test.Routing.VIP", typeSegment.ToUriLiteral());
            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Same(entityType, ((IEdmCollectionType)path.EdmType).ElementType.Definition);
        }

        [Fact]
        public void CanParseEntityCastUrl()
        {
            // Arrange
            IEdmEntitySet expectedSet = _model.EntityContainer.EntitySets()
                .SingleOrDefault(s => s.Name == "RoutingCustomers");
            IEdmEntityType entityType = _model.SchemaElements.OfType<IEdmEntityType>()
                .SingleOrDefault(s => s.FullName() == "Microsoft.AspNet.OData.Test.Routing.VIP");

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, "RoutingCustomers(1)/Microsoft.AspNet.OData.Test.Routing.VIP");
            Assert.NotNull(path); // Guard
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal("~/entityset/key/cast", path.PathTemplate);

            TypeSegment typeSegment = Assert.IsType<TypeSegment>(segment);

            Assert.Equal("Microsoft.AspNet.OData.Test.Routing.VIP", typeSegment.ToUriLiteral());
            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Same(entityType, path.EdmType);
        }

        [Fact]
        public void CanParseComplexCastUrl()
        {
            // Arrange
            IEdmComplexType complexType = _model.SchemaElements.OfType<IEdmComplexType>()
                .SingleOrDefault(s => s.FullName() == "Microsoft.AspNet.OData.Test.Routing.UsAddress");

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, "RoutingCustomers(1)/Address/Microsoft.AspNet.OData.Test.Routing.UsAddress");
            Assert.NotNull(path); // Guard
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal("~/entityset/key/property/cast", path.PathTemplate);
            TypeSegment typeSegment = Assert.IsType<TypeSegment>(segment);

            Assert.Equal("Microsoft.AspNet.OData.Test.Routing.UsAddress", typeSegment.ToUriLiteral());

            Assert.NotNull(path.NavigationSource);

            Assert.Same(complexType, path.EdmType);
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
            SingletonSegment singletonSegment = Assert.IsType<SingletonSegment>(segment);
            Assert.Equal("VipCustomer", singletonSegment.ToUriLiteral());

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

            // Act: key-in-parentheses url should be parsed successfully by default parser using key-as-segment delimiter.
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);

            KeySegment keySegment = Assert.IsType<KeySegment>(segment);

            Assert.Equal(expectedText, keySegment.ToUriLiteral());
            Assert.IsType<KeySegment>(segment);
            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Same(expectedSet.EntityType(), path.EdmType);
        }

        [Fact]
        public void CanParseKeyInParenthesesUrlUsingParenthesesAsDelimiter()
        {
            // Arrange
            string odataPath = "RoutingCustomers(112)";
            string expectedText = "112";
            IEdmEntitySet expectedSet = _model.EntityContainer.EntitySets().SingleOrDefault(s => s.Name == "RoutingCustomers");

            var parenthesisAsDelimiterParser = new DefaultODataPathHandler();
            parenthesisAsDelimiterParser.UrlKeyDelimiter = ODataUrlKeyDelimiter.Parentheses;

            // Act: key-in-parentheses url should be parsed successfully by parser using key-in-parentheses delimiter.
            ODataPath path = parenthesisAsDelimiterParser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);

            KeySegment keySegment = Assert.IsType<KeySegment>(segment);

            Assert.Equal(expectedText, keySegment.ToUriLiteral());
            Assert.IsType<KeySegment>(segment);
            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Same(expectedSet.EntityType(), path.EdmType);
        }

        [Fact]
        public void CanParseKeyAsSegmentUrl()
        {
            // Arrange
            string odataPath = "RoutingCustomers/112";
            string expectedText = "112";
            IEdmEntitySet expectedSet = _model.EntityContainer.EntitySets().SingleOrDefault(s => s.Name == "RoutingCustomers");

            // Act: key-as-segment url should be able to parsed by default parser using key-as-segment delimiter.
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);

            KeySegment keySegment = Assert.IsType<KeySegment>(segment);
            Assert.Equal(expectedText, keySegment.ToUriLiteral());

            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Same(expectedSet.EntityType(), path.EdmType);
        }

        [Fact]
        public void ParseKeyAsSegmentUrlUsingParenthesesAsDelimiterShouldThrow()
        {
            // Arrange
            string odataPath = "RoutingCustomers/112";
            IEdmEntitySet expectedSet = _model.EntityContainer.EntitySets().SingleOrDefault(s => s.Name == "RoutingCustomers");
            var parenthesisAsDelimiterParser = new DefaultODataPathHandler();
            parenthesisAsDelimiterParser.UrlKeyDelimiter = ODataUrlKeyDelimiter.Parentheses;

            // Act: should throw when try to parse key-as-segment url by parser using key-in-parentheses delimiter.
            ExceptionAssert.Throws<ODataException>(
                () => parenthesisAsDelimiterParser.Parse(_model, _serviceRoot, odataPath),
                "The request URI is not valid. Since the segment 'RoutingCustomers' refers to a collection, " +
                "this must be the last segment in the request URI or it must be followed by an function or action" +
                " that can be bound to it otherwise all intermediate segments must refer to a single resource.");
        }

        [Fact]
        public void CanParseUrlWithDefaultKeyAsSegment()
        {
            // Arrange: OData path specified with key as segment.
            string odataPath = "RoutingCustomers/112";
            string expectedText = "112";
            IEdmEntitySet expectedSet = _model.EntityContainer.EntitySets().SingleOrDefault(s => s.Name == "RoutingCustomers");

            // Create path handler (parser) with default UrlKeyDelimiter = null.
            var simplifiedParser = new DefaultODataPathHandler();

            // Act: The parse using default UrlKeyDemiliter can parse OData path with key as segment correctly.
            ODataPath path = simplifiedParser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);

            KeySegment keySegment = Assert.IsType<KeySegment>(segment);
            Assert.Equal(expectedText, keySegment.ToUriLiteral());

            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Same(expectedSet.EntityType(), path.EdmType);
        }

        [Fact]
        public void CanParseCastCollectionSegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP";
            string expectedText = "Microsoft.AspNet.OData.Test.Routing.VIP";
            IEdmEntitySet expectedSet = _model.EntityContainer.EntitySets().SingleOrDefault(s => s.Name == "RoutingCustomers");
            IEdmEntityType expectedType = _model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "VIP");

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);

            TypeSegment typeSegment = Assert.IsType<TypeSegment>(segment);

            Assert.Equal(expectedText, typeSegment.ToUriLiteral());

            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Equal(expectedType, (path.EdmType as IEdmCollectionType).ElementType.Definition);
        }

        [Fact]
        public void CanParseCastSingletonSegment()
        {
            // Arrange
            string odataPath = "VipCustomer/Microsoft.AspNet.OData.Test.Routing.VIP";
            string expectedText = "Microsoft.AspNet.OData.Test.Routing.VIP";
            IEdmSingleton expectedSingleton = _model.EntityContainer.FindSingleton("VipCustomer");
            IEdmEntityType expectedType = _model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "VIP");

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);

            TypeSegment typeSegment = Assert.IsType<TypeSegment>(segment);
            Assert.Equal(expectedText, typeSegment.ToUriLiteral());

            Assert.Same(expectedSingleton, path.NavigationSource);
            Assert.Same(expectedType, path.EdmType);
        }

        [Fact]
        public void CanParseCastEntitySegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers(100)/Microsoft.AspNet.OData.Test.Routing.VIP";
            string expectedText = "Microsoft.AspNet.OData.Test.Routing.VIP";
            IEdmEntitySet expectedSet = _model.EntityContainer.EntitySets().SingleOrDefault(s => s.Name == "RoutingCustomers");
            IEdmEntityType expectedType = _model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "VIP");

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            TypeSegment typeSegment = Assert.IsType<TypeSegment>(segment);

            Assert.Equal(expectedText, typeSegment.ToUriLiteral());

            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Same(expectedType, path.EdmType);
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

            NavigationPropertySegment navigationPropertySegment = Assert.IsType<NavigationPropertySegment>(segment);

            Assert.Equal(expectedText, navigationPropertySegment.ToUriLiteral());
            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Equal(expectedSet.EntityType(), (path.EdmType as IEdmCollectionType).ElementType.Definition);

            Assert.Same(expectedEdmElement, navigationPropertySegment.NavigationProperty);
        }

        [Fact]
        public void CanParseNavigateToSingleSegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers(100)/Microsoft.AspNet.OData.Test.Routing.VIP/RelationshipManager";
            string expectedText = "RelationshipManager";
            IEdmEntitySet expectedSet = _model.EntityContainer.EntitySets().SingleOrDefault(s => s.Name == "SalesPeople");
            IEdmNavigationProperty expectedEdmElement = _model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "VIP").NavigationProperties().SingleOrDefault(n => n.Name == "RelationshipManager");

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);

            NavigationPropertySegment navigationPropertySegment = Assert.IsType<NavigationPropertySegment>(segment);

            Assert.Equal(expectedText, navigationPropertySegment.ToUriLiteral());
            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Equal(expectedSet.EntityType(), path.EdmType);

            Assert.Same(expectedEdmElement, navigationPropertySegment.NavigationProperty);
        }

        [Fact]
        public void CanParseRootOperationSegment()
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
            OperationImportSegment operationSegment = Assert.IsType<OperationImportSegment>(segment);
            Assert.Equal(expectedText, operationSegment.ToUriLiteral());

            EdmActionImport actionImport = Assert.IsType<EdmActionImport>(operationSegment.OperationImports.First());
            Assert.Same(expectedEdmElement, actionImport);

            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Same(expectedSet.EntityType(), path.EdmType);
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

            PropertySegment propertySegment = Assert.IsType<PropertySegment>(segment);

            Assert.Equal(expectedText, propertySegment.ToUriLiteral());
            Assert.Null(path.NavigationSource);

            Assert.Same(expectedEdmElement, propertySegment.Property);
        }

        [Fact]
        public void CanParseDynamicPropertySegment()
        {
            // Arrange
            string odataPath = "SalesPeople(100)/Foo";

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            DynamicPathSegment openPropertySegment = Assert.IsType<DynamicPathSegment>(segment);
            Assert.Equal("Foo", openPropertySegment.ToUriLiteral());

            Assert.Null(path.NavigationSource);
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

            PropertySegment propertySegment = Assert.IsType<PropertySegment>(segment);

            Assert.Equal(expectedText, propertySegment.ToUriLiteral());

            Assert.Null(path.NavigationSource);
            Assert.Same(expectedType, path.EdmType);
            Assert.Same(expectedEdmElement, propertySegment.Property);
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

            PropertySegment propertySegment = Assert.IsType<PropertySegment>(segment);
            Assert.Equal(expectedText, propertySegment.ToUriLiteral());
            Assert.Null(path.NavigationSource);
            Assert.Same(expectedType, path.EdmType);

            Assert.Same(expectedEdmElement, propertySegment.Property);
        }

        [Theory]
        [InlineData("RoutingCustomers(112)/Pet/Microsoft.AspNet.OData.Test.Routing.Dog")]
        [InlineData("VipCustomer/Pet/Microsoft.AspNet.OData.Test.Routing.Dog")]
        public void CanParseComplexCastSegment(string odataPath)
        {
            // Arrange
            const string ExpectedText = "Microsoft.AspNet.OData.Test.Routing.Dog";
            IEdmComplexType expectedType =
                _model.SchemaElements.OfType<IEdmComplexType>().SingleOrDefault(e => e.Name == "Dog");

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            TypeSegment typeSegment = Assert.IsType<TypeSegment>(segment);

            Assert.Equal(ExpectedText, typeSegment.ToUriLiteral());

            Assert.NotNull(path.NavigationSource);
            Assert.Same(expectedType, path.EdmType);
        }

        [Theory]
        [InlineData("RoutingCustomers(112)/Pet/Microsoft.AspNet.OData.Test.Routing.Dog/CanBark", "CanBark")]
        [InlineData("VipCustomer/Pet/Microsoft.AspNet.OData.Test.Routing.Cat/CanMeow", "CanMeow")]
        public void CanParsePropertyValueAfterComplexCastSegment(string odataPath, string expectText)
        {
            // Arrange & Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);

            PropertySegment propertySegment = Assert.IsType<PropertySegment>(segment);

            Assert.Equal(expectText, propertySegment.ToUriLiteral());

            Assert.Null(path.NavigationSource);
            Assert.NotNull(path.EdmType);
            Assert.Equal("Edm.Boolean", (path.EdmType as IEdmPrimitiveType).FullName());
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

            ValueSegment valueSegment = Assert.IsType<ValueSegment>(segment);

            Assert.Equal("$value", valueSegment.ToUriLiteral());

            Assert.Null(path.NavigationSource);
            Assert.NotNull(path.EdmType);
            Assert.Equal("Edm.String", (path.EdmType as IEdmPrimitiveType).FullName());
        }

        [Theory]
        [InlineData("RoutingCustomers(1)/Address/$value")]
        [InlineData("VipCustomer/Address/$value")]
        public void CanParseComplexPropertyValueSegment(string odataPath)
        {
            // Arrange & Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);

            ValueSegment valueSegment = Assert.IsType<ValueSegment>(segment);

            Assert.Equal("$value", valueSegment.ToUriLiteral());

            Assert.Null(path.NavigationSource);
            Assert.NotNull(path.EdmType);
            Assert.Equal("Microsoft.AspNet.OData.Test.Routing.Address", (path.EdmType as IEdmComplexType).FullName());
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

            NavigationPropertyLinkSegment linkSegment = Assert.IsType<NavigationPropertyLinkSegment>(segment);
            Assert.Equal("Products/$ref", linkSegment.ToUriLiteral());
        }

        [Theory]
        [InlineData("RoutingCustomers(1)/Products/$ref?$id=../../Products(5)")]
        [InlineData("VipCustomer/Products/$ref?$id=" + _serviceRoot + "Products(5)")]
        public void CanParseDollarId(string odataPath)
        {
            // Arrange & Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, odataPath);

            // Assert
            KeySegment keySegment =
                Assert.IsType<KeySegment>(path.Segments.Last());

            KeyValuePair<string, object> keyValues = Assert.Single(keySegment.Keys);

            Assert.Equal("ID", keyValues.Key);
            Assert.Equal(5, keyValues.Value);


            Assert.IsType<NavigationPropertyLinkSegment>(path.Segments[path.Segments.Count - 2]);
        }

        [Theory]
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
            ExceptionAssert.Throws<ODataException>(() => _parser.Parse(_model, _serviceRoot, odataPath), expectedError);
        }

        [Theory]
        [InlineData("RoutingCustomers(1)/GetRelatedRoutingCustomers", "GetRelatedRoutingCustomers")]
        [InlineData("Products(7)/TopProductId", "TopProductId")]
        [InlineData("Products(7)/Microsoft.AspNet.OData.Test.Routing.ImportantProduct/TopProductId", "TopProductId")]
        public void ParseAsValidPathSegment_UnqualifiedOperationPath(string odataPath, string expectedOperationName)
        {
            // With Microsoft.OData.Core.UnqualifiedODataUriResolver as default dependency injection in WebAPI,
            // Unqualified functions & actions should be resolved successfully.

            // Arrange & Act & Assert
            OperationSegment validPathSegment = Assert.IsType<OperationSegment>(
                _parser.Parse(_model, _serviceRoot, odataPath).Segments.Last());
            Assert.Single(validPathSegment.Operations);
            Assert.Equal(expectedOperationName, validPathSegment.Operations.First().Name);
        }

        [Theory]
        [InlineData("RoutingCustomers(2)/Microsoft.AspNet.OData.Test.Routing.VIP/GetMostProfitable", "GetMostProfitable")]
        public void ParseAsUnresolvedPathSegment_UnqualifiedOperationPathWithIncorrectBindingType(string odataPath, string unresolveValue)
        {
            // Verify that unqualified function bound to wrong type cannot be resolved.
            // The 'GetMostProfitable' function should be bound to EntityType<VIP>().Collection, not EntityType<VIP>.

            // Arrange & Act & Assert
            UnresolvedPathSegment unresolvedPathSegment = Assert.IsType<UnresolvedPathSegment>(
                _parser.Parse(_model, _serviceRoot, odataPath).Segments.Last());
            Assert.Equal(unresolveValue, unresolvedPathSegment.SegmentValue);
        }

        [Fact]
        public void CannotParseSegmentAfterUnresolvedPathSegment()
        {
            // Arrange & Act & Assert
            ExceptionAssert.Throws<ODataException>(
                () => _parser.Parse(_model, _serviceRoot, _serviceRoot + "RoutingCustomers(1)/InvalidFunctionName/Segment"),
                "The URI segment 'Segment' is invalid after the segment 'InvalidFunctionName'.");
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

            OperationSegment operation = Assert.IsType<OperationSegment>(segment);
            IEdmOperation edmOperation = Assert.Single(operation.Operations);

            EdmAction action = Assert.IsType<EdmAction>(edmOperation);
            Assert.Equal(expectedText, action.FullName());

            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Same(expectedType, path.EdmType);

            Assert.Same(expectedEdmElement, action);
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

            OperationSegment operationSegment = Assert.IsType<OperationSegment>(segment);

            Assert.Equal(expectedText, operationSegment.ToUriLiteral());
            Assert.Same(expectedEntitySet, path.NavigationSource);
            Assert.Same(expectedType, path.EdmType);

            EdmAction action = operationSegment.Operations.First() as EdmAction;
            Assert.NotNull(action);
            Assert.Same(expectedEdmElement, action);
        }

        [Fact]
        public void CanParseActionBoundToCollectionSegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetMostProfitable";
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
            OperationSegment operation = Assert.IsType<OperationSegment>(segment);
            IEdmOperation edmOperation = Assert.Single(operation.Operations);

            EdmAction action = Assert.IsType<EdmAction>(edmOperation);
            Assert.Equal(expectedText, action.FullName());

            Assert.Same(expectedSet, path.NavigationSource);
            Assert.Same(expectedType, path.EdmType);
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
            ExceptionAssert.Throws<ODataUnrecognizedPathException>(
                () => _parser.Parse(model.Model, _serviceRoot, odataPath),
                String.Format("Resource not found for the segment '{0}'.", segmentName));
        }

        [Theory]
        [InlineData("RoutingCustomers(5)/Default.GetSpecialGuid()", "Default.GetSpecialGuid()")]
        [InlineData("RoutingCustomers(5)/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetSpecialGuid()", "Default.GetSpecialGuid()")]
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
            // When EnableKeyAsSegment is enabled, in ODataPathParser.CreateNextSegment(), after attempting to resolve
            // function bound to derived type with no lucks, ODataPathParser will further attempt to resolve the segment text as Key
            // resulting in exception thrown indicating invalid key.

            // Arrange & Act & Assert
            ExceptionAssert.Throws<ODataException>(
                () => _parser.Parse(_model, _serviceRoot, _serviceRoot + uri),
                "Bad Request - Error in query syntax.");
        }

        [Fact]
        public void CanParseOperationBoundToCollectionType()
        {
            string uri = "RoutingCustomers/Default.FunctionBoundToRoutingCustomers()";
            // Arrange & Act & Assert
            ODataPath odataPath = _parser.Parse(_model, _serviceRoot, _serviceRoot + uri);
            Assert.Equal(2, odataPath.Segments.Count);
        }

        [Theory]
        [InlineData("RoutingCustomers/FunctionBoundToRoutingCustomers()", "FunctionBoundToRoutingCustomers")]
        [InlineData("RoutingCustomers/Default.FunctionBoundToRoutingCustomers()", "Default.FunctionBoundToRoutingCustomers")]
        public void CanParseBothUnQualifiedAndQualifiedOperationByDefault(string uri, string expectedIdentifier)
        {
            // Arrange & Act & Assert
            ODataPath odataPath = _parser.Parse(_model, _serviceRoot, _serviceRoot + uri);
            Assert.Equal(2, odataPath.Segments.Count);

            ODataPathSegment firstSegment = odataPath.Segments.First(), secondSegment = odataPath.Segments.Last();
            Assert.True(
                firstSegment is EntitySetSegment &&
                firstSegment.Identifier.Equals("RoutingCustomers", StringComparison.Ordinal));
            Assert.True(
                secondSegment is OperationSegment &&
                secondSegment.Identifier.Equals(expectedIdentifier, StringComparison.Ordinal));
        }

        [Fact]
        public void CanParseQualified_WhenRestoreOldDefaultUriResolver()
        {
            // Arrange & Act & Assert
            // Restore to the old default value (an ODataUriResolver instance), and verify the old behavior is restored.
            string uri = "RoutingCustomers/Default.FunctionBoundToRoutingCustomers()";
            string expectedIdentifier = "Default.FunctionBoundToRoutingCustomers";

            ODataUriResolver oldResolver = new ODataUriResolver();
            DefaultODataPathHandler parserUsingOldDefaultResolver = new DefaultODataPathHandler();
            ODataPath odataPath = parserUsingOldDefaultResolver.Parse(_model, _serviceRoot, _serviceRoot + uri, oldResolver);

            Assert.Equal(2, odataPath.Segments.Count);

            ODataPathSegment firstSegment = odataPath.Segments.First(), secondSegment = odataPath.Segments.Last();
            Assert.True(
                firstSegment is EntitySetSegment &&
                firstSegment.Identifier.Equals("RoutingCustomers", StringComparison.Ordinal));
            Assert.True(
                secondSegment is OperationSegment &&
                secondSegment.Identifier.Equals(expectedIdentifier, StringComparison.Ordinal));
        }

        [Fact]
        public void CannotParseUnQualified_WhenRestoreToOldDefaultUriResolver()
        {
            // Arrange & Act & Assert
            // Restore to the old default value (an ODataUriResolver instance), and verify the old behavior is restored.
            string uri = "RoutingCustomers/FunctionBoundToRoutingCustomers()";
            ODataUriResolver oldResolver = new ODataUriResolver();
            DefaultODataPathHandler parserUsingOldDefaultResolver = new DefaultODataPathHandler();
            ExceptionAssert.Throws<ODataException>(
                () => parserUsingOldDefaultResolver.Parse(_model, _serviceRoot, _serviceRoot + uri, oldResolver),
                "Bad Request - Error in query syntax.");
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
            // The unbound operation cannot be resolved for the entity collection type. Subsequently, with EnableKeyAsSegment enabled,
            // ODataPathParser will further attempt to resolve the segment text as Key
            // resulting in exception thrown indicating invalid key.

            // Arrange & Act & Assert
            ExceptionAssert.Throws<ODataException>(
                () => _parser.Parse(_model, _serviceRoot, _serviceRoot + "RoutingCustomers/Default.GetAllVIPs()"),
                "Bad Request - Error in query syntax.");
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
            ExceptionAssert.Throws<ODataException>(
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
            ExceptionAssert.Throws<ODataUnrecognizedPathException>(
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
            ExceptionAssert.Throws<ODataUnrecognizedPathException>(
                () => _parser.Parse(model, _serviceRoot, _serviceRoot + uri),
                "Resource not found for the segment 'OverloadUnboundFunction'.");
        }

        [Theory]
        [InlineData(typeof(SimpleEnum), "Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum'123'")]
        [InlineData(typeof(SimpleEnum), "Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum'-9999'")]
        [InlineData(typeof(FlagsEnum), "Microsoft.AspNet.OData.Test.Common.Types.FlagsEnum'999'")]
        [InlineData(typeof(FlagsEnum), "Microsoft.AspNet.OData.Test.Common.Types.FlagsEnum'-12345'")]
        public void CanParseUndefinedEnumValue(Type enumerationType, string enumerationExpression)
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            EnumTypeConfiguration enumTypeConfiguration = builder.AddEnumType(enumerationType);
            FunctionConfiguration functionConfiguration = builder.Function("FunctionWithEnumParam");
            functionConfiguration.AddParameter("Enum", enumTypeConfiguration);
            functionConfiguration.Returns<int>();
            IEdmModel model = builder.GetEdmModel();
            string uri = String.Format("FunctionWithEnumParam(Enum={0})", enumerationExpression);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => _parser.Parse(model, _serviceRoot, uri));
        }

        [Theory]
        [InlineData(typeof(SimpleEnum), "Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum'First, Second'")]
        [InlineData(typeof(SimpleEnum), "Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum'UnknownValue'")]
        [InlineData(typeof(FlagsEnum), "Microsoft.AspNet.OData.Test.Common.Types.FlagsEnum'UnknownValue'")]
        [InlineData(typeof(FlagsEnum), "Microsoft.AspNet.OData.Test.Common.Types.FlagsEnum'abc'")]
        public void CannotParseInvalidEnumValue(Type enumerationType, string enumerationExpression)
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            EnumTypeConfiguration enumTypeConfiguration = builder.AddEnumType(enumerationType);
            FunctionConfiguration functionConfiguration = builder.Function("FunctionWithEnumParam");
            functionConfiguration.AddParameter("Enum", enumTypeConfiguration);
            functionConfiguration.Returns<int>();
            IEdmModel model = builder.GetEdmModel();
            string uri = String.Format("FunctionWithEnumParam(Enum={0})", enumerationExpression);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(
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
            Assert.Single(path.Segments);
            var functionSegment = Assert.IsType<OperationImportSegment>(path.Segments.First());

            IEdmOperationImport opertionImport = functionSegment.OperationImports.First();
            EdmFunctionImport functionImport = Assert.IsType<EdmFunctionImport>(opertionImport);
            Assert.Same(function, functionImport);
            Assert.Empty(functionSegment.Parameters);
        }

        [Theory]
        [InlineData("Customers(42)/NS.IsSpecial", 3)]
        [InlineData("VipCustomer/NS.IsSpecial", 2)]
        public void CanParse_BoundFunction_AtEntity(string odataPath, int segmentCount)
        {
            // Arrange
            var model = new CustomersModelWithInheritance();
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmExpression entitySet = new EdmPathExpression(model.Customers.Name);
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

            var functionSegment = Assert.IsType<OperationSegment>(path.Segments.Last());

            IEdmOperation opertion = functionSegment.Operations.First();
            EdmFunction edmFunction = Assert.IsType<EdmFunction>(opertion);
            Assert.Same(function, edmFunction);
            Assert.Empty(functionSegment.Parameters);
        }

        [Fact]
        public void CanParse_BoundFunction_AtEntityCollection()
        {
            // Arrange
            var model = new CustomersModelWithInheritance();
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmExpression entitySet = new EdmPathExpression(model.Customers.Name);
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
            var functionSegment = Assert.IsType<OperationSegment>(path.Segments.Last());

            IEdmOperation opertion = functionSegment.Operations.First();
            EdmFunction edmFunction = Assert.IsType<EdmFunction>(opertion);
            Assert.Same(function, edmFunction);
            Assert.Empty(functionSegment.Parameters);

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
            var functionSegment = Assert.IsType<OperationImportSegment>(path.Segments.Last());

            // Assert
            int intParameter = (int)functionSegment.GetParameterValue("IntParameter");
            Assert.Equal(1, intParameter);
        }

        [Theory]
        [MemberData(nameof(NullFunctionParameterData))]
        public void CanParse_FunctionParameters_CanResolveAliasedParameterValueWithNull(object value, Type type)
        {
            // Arrange & Act
            object parameter = GetAliasedParameterValue(value, type);

            // Assert
            Assert.Null(parameter);
        }

        [Theory]
        [MemberData(nameof(EnumFunctionParameterData))]
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
        [MemberData(nameof(FunctionParameterData))]
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

            OperationImportSegment segment = Assert.IsType<OperationImportSegment>(path.Segments.Last());
            return segment.GetParameterValue("Parameter");
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

            OperationSegment functionSegment = (OperationSegment)path.Segments.Last();
            object intParameter = functionSegment.GetParameterValue("IntParameter");
            object nullableDoubleParameter = functionSegment.GetParameterValue("NullableDoubleParameter");
            object stringParameter = functionSegment.GetParameterValue("StringParameter");
            object guidParameter = functionSegment.GetParameterValue("GuidParameter");
            object enumParameter = functionSegment.GetParameterValue("EnumParameter");

            // Assert
            Assert.Equal(123, intParameter);
            Assert.Null(nullableDoubleParameter);
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
            var builder = ODataConventionModelBuilderFactory.Create();
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

            var functionSegment = (OperationImportSegment)path.Segments.First();
            object p1 = functionSegment.GetParameterValue("P1");
            object p2 = functionSegment.GetParameterValue("P2");

            // Assert
            Assert.Equal(1, p1);
            Assert.Equal(2, p2);
        }

        [Fact]
        public void CanParse_UntouchedFunctionParametersAlias_WithUnresolvedPathSegment()
        {
            // Arrange
            var builder = ODataConventionModelBuilderFactory.Create();
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
                "UnboundFunction(P1=@p1,P2=@p2)/unknownFunc(a=@p4)?@p1=1&@p3=2&@p2=@p3&@p4='abc'");

            var functionSegment = (OperationImportSegment)path.Segments.First();
            object p1 = functionSegment.GetParameterValue("P1");
            object p2 = functionSegment.GetParameterValue("P2");

            // Assert
            Assert.Equal(1, p1);
            Assert.Equal(2, p2);
            Assert.IsType<UnresolvedPathSegment>(path.Segments.Last());
            Assert.Equal("unknownFunc(a=@p4)", ((UnresolvedPathSegment)path.Segments.Last()).SegmentValue);
        }

        [Theory]
        [MemberData(nameof(NullFunctionParameterData))]
        public void CanParse_NullFunctionParameters(object value, Type type)
        {
            // Arrange & Act
            object parameter = GetParameterValue(value, type);

            // Assert
            Assert.Null(parameter);
        }

        [Theory]
        [MemberData(nameof(EnumFunctionParameterData))]
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
        [MemberData(nameof(FunctionParameterData))]
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

            OperationImportSegment operationSegment = Assert.IsType<OperationImportSegment>(path.Segments.Last());
            return operationSegment.GetParameterValue("Parameter");
        }

        [Theory]
        [InlineData(
            "(address=@p)?@p={\"@odata.type\":\"Microsoft.AspNet.OData.Test.Routing.Address\",\"Street\":\"NE 24th St.\",\"City\":\"Redmond\"}"
            )]
        public void CanParse_ComplexTypeAsFunctionParameterInAlias(string parameterValue)
        {
            // Arrange & Act
            ODataPath path = _parser.Parse(_model, _serviceRoot,
                "RoutingCustomers(1)/Default.CanMoveToAddress" + parameterValue);

            // Assert
            Assert.Equal("~/entityset/key/function", path.PathTemplate);

            OperationSegment operationSegment = Assert.IsType<OperationSegment>(path.Segments.Last());

            object value = operationSegment.GetParameterValue("address");

            string address = Assert.IsType<string>(parameterValue);
            Assert.Equal(parameterValue, address);
        }

        [Fact]
        public void ParseComplexTypeAsFunctionParameterInlineSuccessed()
        {
            // Arrange & Act
            string requestUri = "RoutingCustomers(1)/Default.CanMoveToAddress(address={\"@odata.type\":\"Microsoft.AspNet.OData.Test.Routing.Address\",\"Street\":\"NE 24th St.\",\"City\":\"Redmond\"})";

            // Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, requestUri);

            // Assert
            Assert.NotNull(path);
            Assert.Equal(3, path.Segments.Count);

            OperationSegment operationSegment = Assert.IsType<OperationSegment>(path.Segments.Last());
            Assert.NotNull(operationSegment);

            OperationSegmentParameter parameter = Assert.Single(operationSegment.Parameters);
            Assert.Equal("address", parameter.Name);

            ConstantNode parameterValue = Assert.IsType<ConstantNode>(parameter.Value);
            Assert.NotNull(parameterValue);

            var resourceValue = Assert.IsType<ODataResourceValue>(parameterValue.Value);
            Assert.Equal("Microsoft.AspNet.OData.Test.Routing.Address", resourceValue.TypeName);

            Assert.Equal(2, resourceValue.Properties.Count());
            ODataProperty street = resourceValue.Properties.FirstOrDefault(p => p.Name == "Street");
            Assert.NotNull(street);
            Assert.Equal("NE 24th St.", street.Value);

            ODataProperty city = resourceValue.Properties.FirstOrDefault(p => p.Name == "City");
            Assert.NotNull(city);
            Assert.Equal("Redmond", city.Value);
        }

        [Theory]
        [InlineData("(addresses=@p)?@p=[{\"Street\":\"NE 24th St.\",\"City\":\"Redmond\"},{\"Street\":\"Pine St.\",\"City\":\"Seattle\"}]")]
        public void CanParse_CollectionOfComplexTypeAsFunctionParameter_Alias(string parametervalue)
        {
            // Arrange & Act
            ODataPath path = _parser.Parse(_model, _serviceRoot, "RoutingCustomers(1)/Default.MoveToAddresses" + parametervalue);

            // Assert
            Assert.Equal("~/entityset/key/function", path.PathTemplate);
            OperationSegment functionSegment = (OperationSegment)path.Segments.Last();

            object parameterValue = functionSegment.GetParameterValue("addresses");
            string addresses = Assert.IsType<string>(parameterValue);
            Assert.Equal(parametervalue.Substring(parametervalue.IndexOf("[{", StringComparison.Ordinal)), addresses);
        }

        [Theory]
        [InlineData("(intValues=@p)?@p=[1,2,4,7,8]")]
        [InlineData("(intValues=[1,2,4,7,8])")]
        public void CanParse_CollectionOfPrimitiveTypeAsFunctionParameter(string parametervalue)
        {
            // Arrange & Act
            ODataPath path = _parser.Parse(_model, _serviceRoot,
                "RoutingCustomers(1)/Default.CollectionOfPrimitiveTypeFunction" + parametervalue);

            // Assert
            Assert.Equal("~/entityset/key/function", path.PathTemplate);
            OperationSegment functionSegment = (OperationSegment)path.Segments.Last();

            object parameterValue = functionSegment.GetParameterValue("intValues");
            ODataCollectionValue intValues = Assert.IsType<ODataCollectionValue>(parameterValue);
            Assert.Equal("Collection(Edm.Int32)", intValues.TypeName);
        }

        [Theory]
        [InlineData("{\"@odata.type\":\"Microsoft.AspNet.OData.Test.Routing.Product\",\"ID\":9,\"Name\":\"Phone\"}")]
        [InlineData("{\"@odata.Id\":\"http://localhost/odata/Products(9)\"}")]
        public void CanParse_EntityTypeAsFunctionParameter_ParametersAlias(string entityAlias)
        {
            // Arrange & Act
            ODataPath path = _parser.Parse(_model, _serviceRoot,
                "RoutingCustomers(1)/Default.EntityTypeFunction(product=@p)?@p=" + entityAlias);

            // Assert
            Assert.Equal("~/entityset/key/function", path.PathTemplate);
            OperationSegment functionSegment = (OperationSegment)path.Segments.Last();

            object parameterValue = functionSegment.GetParameterValue("product");
            string product = Assert.IsType<string>(parameterValue);
            Assert.Equal(entityAlias, product);
        }

        [Fact]
        public void CanParse_EntityTypeAsFunctionParameter_ThrowsForInlineParameter()
        {
            // Arrange
            const string odataPath =
                "RoutingCustomers(1)/Default.EntityTypeFunction(product={\"@odata.type\":\"Microsoft.AspNet.OData.Test.Routing.Product\",\"ID\":9,\"Name\":\"Phone\"}";

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => _parser.Parse(_model, _serviceRoot, odataPath));
        }

        [Theory]
        [InlineData("{\"value\":[{\"@odata.type\":\"Microsoft.AspNet.OData.Test.Routing.Product\",\"ID\":9,\"Name\":\"Phone\"}," +
                    "{\"@odata.type\":\"Microsoft.AspNet.OData.Test.Routing.Product\",\"ID\":10,\"Name\":\"TV\"}]}")]
        [InlineData("[{\"@odata.Id\":\"http://localhost/odata/Products(9)\"},{\"@odata.Id\":\"http://localhost/odata/Products(10)\"}]")]
        public void CanParse_CollectionEntityTypeAsFunctionParameter_ParametersAlias(string entityAlias)
        {
            // Arrange & Act
            ODataPath path = _parser.Parse(_model, _serviceRoot,
                "RoutingCustomers(1)/Default.CollectionEntityTypeFunction(products=@p)?@p=" + entityAlias);

            // Assert
            Assert.Equal("~/entityset/key/function", path.PathTemplate);
            OperationSegment functionSegment = (OperationSegment)path.Segments.Last();

            object parameterValue = functionSegment.GetParameterValue("products");
            string product = Assert.IsType<string>(parameterValue);
            Assert.Equal(entityAlias, product);
        }

        [Fact]
        public void CanParse_CollectionEntityTypeAsFunctionParameter_ThrowsForInlineParameter()
        {
            // Arrange
            const string odataPath = "RoutingCustomers(1)/Default.CollectionEntityTypeFunction(products=" +
                "{\"value\":[{\"@odata.type\":\"Microsoft.AspNet.OData.Test.Routing.Product\",\"ID\":9,\"Name\":\"Phone\"}," +
                "{\"@odata.type\":\"Microsoft.AspNet.OData.Test.Routing.Product\",\"ID\":10,\"Name\":\"TV\"}]}";

            // & Act
            ExceptionAssert.Throws<ODataException>(() => _parser.Parse(_model, _serviceRoot, odataPath));
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
            ExceptionAssert.Throws<ODataException>(
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
            ExceptionAssert.Throws<ODataException>(
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
            ExceptionAssert.Throws<ODataUnrecognizedPathException>(
                () => _parser.Parse(_model, _serviceRoot, path),
                "The request URI is not valid. The segment '$ref' must be the last segment in the URI because " +
                "it is one of the following: $ref, $batch, $count, $value, $metadata, a named media resource, " +
                "an action, a noncomposable function, an action import, a noncomposable function import, " +
                "an operation with void return type, or an operation import with void return type.");
        }

        [Theory]
        [InlineData("RoutingCustomers(1)/ID/$value/$ref")]
        [InlineData("RoutingCustomers(1)/ID/$value/$count")]
        [InlineData("RoutingCustomers(1)/ID/$value/$value")]
        [InlineData("RoutingCustomers(1)/ID/$value/$value/something")]
        [InlineData("RoutingCustomers(1)/ID/$value/something")]
        [InlineData("RoutingCustomers(1)/ID/$value/GetSpecialGuid()")]
        [InlineData("EnumCustomers(1)/Color/$value/something")]
        public void DefaultODataPathHandler_ThrowsIfDollarValueIsNotTheLastSegment(string path)
        {
            // Arrange & Act & Assert
            ExceptionAssert.Throws<ODataUnrecognizedPathException>(
                () => _parser.Parse(_model, _serviceRoot, path),
                "The request URI is not valid. The segment '$value' must be the last segment in the URI because " +
                "it is one of the following: $ref, $batch, $count, $value, $metadata, a named media resource, " +
                "an action, a noncomposable function, an action import, a noncomposable function import, " +
                "an operation with void return type, or an operation import with void return type.");
        }

        [Theory]
        [InlineData("RoutingCustomers/$count/$value")]
        [InlineData("RoutingCustomers(100)/Products/$count/unknown")]
        [InlineData("UnboundFunction()/$count/somesegment")]
        public void DefaultODataPathHandler_ThrowsIfDollarCountIsNotTheLastSegment(string path)
        {
            // Arrange & Act & Assert
            ExceptionAssert.Throws<ODataUnrecognizedPathException>(
                () => _parser.Parse(_model, _serviceRoot, path),
                "The request URI is not valid. The segment '$count' must be the last segment in the URI because " +
                "it is one of the following: $ref, $batch, $count, $value, $metadata, a named media resource, " +
                "an action, a noncomposable function, an action import, a noncomposable function import, " +
                "an operation with void return type, or an operation import with void return type.");
        }

        [Theory]
        [InlineData("RoutingCustomers(1)/Name/$count", "Name")]
        [InlineData("DateTimeOffsetKeyCustomers(2001-01-01T12:00:00.000+08:00)/ID/$count", "ID")]
        public void DefaultODataPathHandler_Throws_DollarCountFollowsNonCollectionPrimitive(
            string path, string segment)
        {
            // Arrange & Act & Assert
            ExceptionAssert.Throws<ODataUnrecognizedPathException>(
                () => _parser.Parse(_model, _serviceRoot, path),
                String.Format(
                    "The segment '$count' in the request URI is not valid. The segment '{0}' refers to a primitive property, " +
                    "function, or service operation, so the only supported value from the next segment is '$value'.",
                    segment));
        }

        [Theory]
        [InlineData("EnumCustomers(3)/Color/$count", "Color")]
        [InlineData("RoutingCustomers(4)/Address/$count", "Address")]
        [InlineData("RoutingCustomers(5)/$count", "RoutingCustomers")]
        [InlineData("RoutingCustomers(5)/Products(6)/$count", "Products")]
        [InlineData("VipCustomer/$count", "VipCustomer")]
        public void DefaultODataPathHandler_Throws_DollarCountFollowsNonCollectionNonPrimitive(
            string path, string segment)
        {
            // Arrange & Act & Assert
            ExceptionAssert.Throws<ODataUnrecognizedPathException>(
                () => _parser.Parse(_model, _serviceRoot, path),
                String.Format(
                    "The request URI is not valid. $count cannot be applied to the segment '{0}' since $count can only " +
                    "follow an entity set, a collection navigation property, a structural property of collection type, " +
                    "an operation returning collection type or an operation import returning collection type.",
                    segment));
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
            var expectedType = model.FindDeclaredType("Microsoft.AspNet.OData.Test.Routing." + expectedTypeName) as IEdmEntityType;

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
        public void CanResolveSetAndTypeViaKeySegment(string odataPath, string expectedSetName, string expectedTypeName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("RoutingCustomers(1)/Products", "Products", "Product", true)]
        [InlineData("RoutingCustomers(1)/Products(1)", "Products", "Product", false)]
        [InlineData("RoutingCustomers(1)/Products/", "Products", "Product", true)]
        [InlineData("Products(1)/RoutingCustomers", "RoutingCustomers", "RoutingCustomer", true)]
        [InlineData("Products(1)/RoutingCustomers(1)", "RoutingCustomers", "RoutingCustomer", false)]
        [InlineData("Products(1)/RoutingCustomers/", "RoutingCustomers", "RoutingCustomer", true)]
        [InlineData("VipCustomer/Products", "Products", "Product", true)]
        [InlineData("VipCustomer/Products(1)", "Products", "Product", false)]
        [InlineData("VipCustomer/Products/", "Products", "Product", true)]
        [InlineData("MyProduct/RoutingCustomers", "RoutingCustomers", "RoutingCustomer", true)]
        [InlineData("MyProduct/RoutingCustomers(1)", "RoutingCustomers", "RoutingCustomer", false)]
        [InlineData("MyProduct/RoutingCustomers/", "RoutingCustomers", "RoutingCustomer", true)]
        public void CanResolveSetAndTypeViaNavigationPropertySegment(string odataPath, string expectedSetName, string expectedTypeName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP", "VIP", "RoutingCustomers", true)]
        [InlineData("RoutingCustomers(1)/Microsoft.AspNet.OData.Test.Routing.VIP", "VIP", "RoutingCustomers", false)]
        [InlineData("Products(1)/Microsoft.AspNet.OData.Test.Routing.ImportantProduct", "ImportantProduct", "Products", false)]
        [InlineData("Products(1)/RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP", "VIP", "RoutingCustomers", true)]
        [InlineData("SalesPeople(1)/ManagedRoutingCustomers", "VIP", "RoutingCustomers", true)]
        [InlineData("RoutingCustomers(1)/Microsoft.AspNet.OData.Test.Routing.VIP/RelationshipManager", "SalesPerson", "SalesPeople", false)]
        [InlineData("Products(1)/Microsoft.AspNet.OData.Test.Routing.ImportantProduct/LeadSalesPerson", "SalesPerson", "SalesPeople", false)]
        [InlineData("Products(1)/RoutingCustomers(1)/Microsoft.AspNet.OData.Test.Routing.VIP/RelationshipManager/ManagedProducts", "ImportantProduct", "Products", true)]
        public void CanResolveSetAndTypeViaCastSegment(string odataPath, string expectedTypeName, string expectedSetName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("GetRoutingCustomerById", "RoutingCustomer", "RoutingCustomers", false)]
        [InlineData("GetSalesPersonById", "SalesPerson", "SalesPeople", false)]
        [InlineData("GetAllVIPs", "VIP", "RoutingCustomers", true)]
        public void CanResolveSetAndTypeViaRootOperationSegment(string odataPath, string expectedTypeName, string expectedSetName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("RoutingCustomers(1)/Default.GetRelatedRoutingCustomers", "RoutingCustomer", "RoutingCustomers", true)]
        [InlineData("RoutingCustomers(1)/Default.GetBestRelatedRoutingCustomer", "VIP", "RoutingCustomers", false)]
        [InlineData("RoutingCustomers(1)/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetSalesPerson", "SalesPerson", "SalesPeople", false)]
        [InlineData("SalesPeople(1)/Default.GetVIPRoutingCustomers", "VIP", "RoutingCustomers", true)]
        public void CanResolveSetAndTypeViaEntityActionSegment(string odataPath, string expectedTypeName, string expectedSetName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("VipCustomer/Default.GetRelatedRoutingCustomers", "RoutingCustomer", "RoutingCustomers", true)]
        [InlineData("VipCustomer/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetSalesPerson", "SalesPerson", "SalesPeople", false)]
        public void CanResolveSetAndTypeViaSingletonSegment(string odataPath, string expectedTypeName, string expectedSetName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("RoutingCustomers/Default.GetVIPs", "VIP", "RoutingCustomers", true)]
        [InlineData("RoutingCustomers/Default.GetProducts", "Product", "Products", true)]
        [InlineData("RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetProducts", "Product", "Products", true)]
        [InlineData("Products(1)/RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetSalesPeople", "SalesPerson", "SalesPeople", true)]
        [InlineData("MyProduct/RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetSalesPeople", "SalesPerson", "SalesPeople", true)]
        [InlineData("SalesPeople/Default.GetVIPRoutingCustomers", "VIP", "RoutingCustomers", true)]
        [InlineData("RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetMostProfitable", "VIP", "RoutingCustomers", false)]
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
            ExceptionAssert.Throws<ODataException>(
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
            OperationSegment actionSegment = Assert.IsType<OperationSegment>(odataPath.Segments.Last());

            IEdmOperation operation = actionSegment.Operations.First();
            EdmAction edmAction = Assert.IsType<EdmAction>(operation);
            Assert.Equal("NS." + actionName, edmAction.FullName());
            Assert.Equal(expectedEntityBound, edmAction.Parameters.First().Type.Definition.ToTraceString());
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
            var builder = ODataConventionModelBuilderFactory.Create();
            var customer = builder.EntitySet<ODataRoutingModel.RoutingCustomer>("Customers").EntityType;
            customer.HasMany(c => c.Products).IsNotNavigable();
            builder.EntitySet<ODataRoutingModel.Product>("Products");
            var model = builder.GetEdmModel();

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(
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
        [InlineData("Function(foo=42,bar=true)", "Function(foo={newFoo},bar={newBar})", new string[] { "newFoo:42", "newBar:True" })]
        [InlineData("Function(bar=false,foo=24)", "Function(foo={newFoo},bar={newBar})", new string[] { "newFoo:24", "newBar:False" })]
        [InlineData("Customers(42)/Account/DynamicPropertyName", "Customers({ID})/Account/{propertyname:dynamicproperty}", new string[] { "ID:42", "propertyname:DynamicPropertyName" })]
        [InlineData("Orders(24)/DynamicPropertyName", "Orders({ID})/{propertyname:dynamicproperty}", new string[] { "ID:24", "propertyname:DynamicPropertyName" })]
        [InlineData("RootOrder/DynamicPropertyName", "RootOrder/{propertyname:dynamicproperty}", new string[] { "propertyname:DynamicPropertyName" })]
        [InlineData("Customers(42)/Orders(24)/DynamicPropertyName", "Customers({ID})/Orders({key})/{propertyname:dynamicproperty}", new string[] { "ID:42", "key:24", "propertyname:DynamicPropertyName" })]
        [InlineData("Customers/NS.GetWholeSalary(minSalary=7)", "Customers/NS.GetWholeSalary(minSalary={min})", new string[] { "min:7" })]
        [InlineData("Customers/NS.GetWholeSalary(minSalary=7,maxSalary=8)", "Customers/NS.GetWholeSalary(minSalary={min},maxSalary={max})", new string[] { "min:7", "max:8" })]
        [InlineData("Customers/NS.GetWholeSalary(minSalary=7,maxSalary=8,aveSalary=9)", "Customers/NS.GetWholeSalary(minSalary={min},maxSalary={max},aveSalary={ave})", new string[] { "min:7", "max:8", "ave:9" })]
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

            Assert.Equal(keyValues.OrderBy(k => k), routeData.Where(d => !d.Key.StartsWith(ODataParameterValue.ParameterValuePrefix))
                .Select(d => d.Key + ":" + d.Value).OrderBy(d => d));
        }

        [Theory]
        [InlineData("Customer", "Customer")] // Customer is not a correct entity set in the model
        [InlineData("UnknowFunction(foo={newFoo})", "UnknowFunction")] // UnknowFunction is not a function name in the model
        public void ParseTemplate_ThrowODataException_InvalidODataPathSegmentTemplate(string template, string segmentValue)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();

            // Act & Assert
            ExceptionAssert.Throws<ODataUnrecognizedPathException>(
                () => _parser.ParseTemplate(model.Model, template),
                String.Format("Resource not found for the segment '{0}'.", segmentValue));
        }

        [Fact]
        public void ParseTemplate_ThrowODataException_UnResolvedPathSegment()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => _parser.ParseTemplate(model.Model, "Customers(ID={key})/Order"),
                "Found an unresolved path segment 'Order' in the OData path template 'Customers(ID={key})/Order'.");
        }

        [Theory]
        [InlineData("Customers({key})/Account/{pName:dynamic:test}", "{pName:dynamic:test}")]
        [InlineData("Customers({key})/Account/{pName:dynamic}", "{pName:dynamic}")]
        [InlineData("Customers({key})/Account/{aa}", "{aa}")]
        [InlineData("Customers({key})/Account/{pName : dynamic}", "{pName : dynamic}")]
        [InlineData("Orders({key})/{pName : dynamic}", "{pName : dynamic}")]
        public void ParseTemplate_ThrowODataException_InvalidAttributeRoutingTemplateSegment(string template, string error)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => _parser.ParseTemplate(model.Model, template),
                string.Format("The attribute routing template contains invalid segment '{0}'.", error));
        }

        public static TheoryDataSet<string, string, string> PathSegmentIdentifierCaseInsensitiveCases
        {
            get
            {
                // $batch, $metadata, $count, $ref, $value
                return new TheoryDataSet<string, string, string>()
                {
                    { "$BatcH", "~/$batch", "$batch"},
                    { "$meTadata", "~/$metadata", "$metadata"},
                    { "RoutingCUsTomers(1)/Name/$VALuE", "~/entityset/key/property/$value",
                        "RoutingCustomers(1)/Name/$value" },
                    { "RoutingCUsTomers(1)/Products/$rEf", "~/entityset/key/navigation/$ref",
                        "RoutingCustomers(1)/Products/$ref" },
                    { "RoutingCUsTomers/$CounT", "~/entityset/$count", "RoutingCustomers/$count" }
                };
            }
        }

        public static TheoryDataSet<string, string, string> UserMetadataCaseInsensitiveCases
        {
            get
            {
                return new TheoryDataSet<string, string, string>()
                {
                    // EntitySet/Singleton name
                    { "routingCUSTOMERS", "~/entityset", "RoutingCustomers" },
                    { "routingCUSTOMERS(112)", "~/entityset/key", "RoutingCustomers(112)" },
                    { "vIpCustomEr", "~/singleton", "VipCustomer" },

                    // Property name
                    { "RoutingCusTomers(100)/proDucts", "~/entityset/key/navigation", "RoutingCustomers(100)/Products" },
                    { "EnumCusTomers(1)/COloR", "~/entityset/key/property", "EnumCustomers(1)/Color" },
                    { "vIpCustomEr/prOduCts", "~/singleton/navigation", "VipCustomer/Products" },

                    // Type Name
                    { "rouTingcustomers/Microsoft.AspNet.OData.Test.RouTing.VIP", "~/entityset/cast", "RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP" },
                    { "vIpCustomEr/Microsoft.AspNet.OData.Test.Routing.VIP", "~/singleton/cast", "VipCustomer/Microsoft.AspNet.OData.Test.Routing.VIP" },

                    // Action
                    { "gETroutingCUstomerById()", "~/unboundaction", "GetRoutingCustomerById" },
                    { "routINGCustomers(112)/dEfAulT.GetRelatedRoutingCustomers", "~/entityset/key/action",
                        "RoutingCustomers(112)/Default.GetRelatedRoutingCustomers" },

                    // Function name/parameter name
                    { "UnBOUNDFunction()", "~/unboundfunction", "UnboundFunction()" },
                    { "routINGCustomers(112)/Default.GeTordersCount(faCTor=1)", "~/entityset/key/function",
                        "RoutingCustomers(112)/Default.GetOrdersCount(factor=1)" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(PathSegmentIdentifierCaseInsensitiveCases))]
        [MemberData(nameof(UserMetadataCaseInsensitiveCases))]
        public void DefaultUriResolverHandler_Throws_ForCaseSensitive(string path, string template, string expect)
        {
            Assert.NotNull(template);
            Assert.NotNull(expect);
            ExceptionAssert.Throws<ODataUnrecognizedPathException>(
                () => new DefaultODataPathHandler().Parse(_model, _serviceRoot, path));
        }

        [Theory]
        [MemberData(nameof(PathSegmentIdentifierCaseInsensitiveCases))]
        [MemberData(nameof(UserMetadataCaseInsensitiveCases))]
        public void DefaultUriResolverHandler_Works_CaseInsensitive(string path, string template, string expect)
        {
            // Arrange & Act
            DefaultODataPathHandler pathHandler = new DefaultODataPathHandler();
            ODataUriResolver resolver = new ODataUriResolver
            {
                EnableCaseInsensitive = true,
            };

            ODataPath odataPath = pathHandler.Parse(_model, _serviceRoot, path, resolver);

            // Assert
            Assert.NotNull(odataPath);
            Assert.Equal(template, odataPath.PathTemplate);
            Assert.Equal(expect, odataPath.ToString());
        }

        public static TheoryDataSet<string, string, string> UnqualifiedCallCases
        {
            get
            {
                return new TheoryDataSet<string, string, string>()
                {
                    // Bound Action
                    { "routINGCustomers(112)/GetRelaTEDRoutingCustomers", "~/entityset/key/action",
                        "RoutingCustomers(112)/Default.GetRelatedRoutingCustomers" },

                    // Bound Function name/parameter name
                    { "routINGCustomers(112)/GeTORDersCount(faCTor=1)", "~/entityset/key/function",
                        "RoutingCustomers(112)/Default.GetOrdersCount(factor=1)" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(UnqualifiedCallCases))]
        public void Unqualified_Throws_DefaultUriResolverHandler(string path, string template, string expect)
        {
            Assert.NotNull(template);
            Assert.NotNull(expect);
            ExceptionAssert.Throws<ODataUnrecognizedPathException>(
                () => new DefaultODataPathHandler().Parse(_model, _serviceRoot, path));
        }

        [Theory]
        [MemberData(nameof(UnqualifiedCallCases))]
        public void Unqualified_Works_CustomUriResolverHandler(string path, string template, string expect)
        {
            // Arrange
            DefaultODataPathHandler pathHandler = new DefaultODataPathHandler();
            ODataUriResolver resolver = new UnqualifiedODataUriResolver
            {
                EnableCaseInsensitive = true,
            };

            // Act
            ODataPath odataPath = pathHandler.Parse(_model, _serviceRoot, path, resolver);

            // Assert
            Assert.NotNull(odataPath);
            Assert.Equal(template, odataPath.PathTemplate);
            Assert.Equal(expect, odataPath.ToString());
        }

        public static TheoryDataSet<string, string, string> PrefixFreeEnumCases
        {
            get
            {
                return new TheoryDataSet<string, string, string>()
                {
                    { "UnboundFuncWithEnumParameters(LongEnum='ThirdLong', FlagsEnum='7')",
                      "~/unboundfunction",
                      "UnboundFuncWithEnumParameters(LongEnum=Microsoft.AspNet.OData.Test.Common.Types.LongEnum'2',FlagsEnum=Microsoft.AspNet.OData.Test.Common.Types.FlagsEnum'7')" },

                    { "RoutingCustomers/Default.BoundFuncWithEnumParameters(SimpleEnum='1', FlagsEnum='One, Four')",
                      "~/entityset/function",
                      "RoutingCustomers/Default.BoundFuncWithEnumParameters(SimpleEnum=Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum'1',FlagsEnum=Microsoft.AspNet.OData.Test.Common.Types.FlagsEnum'5')"}
                };
            }
        }

        [Theory]
        [MemberData(nameof(PrefixFreeEnumCases))]
        public void PrefixFreeEnumValue_Works_DefaultResolver(string path, string template, string expect)
        {
            Assert.NotNull(template);
            Assert.NotNull(expect);

            ODataPath odataPath = _parser.Parse(_model, _serviceRoot, path);
            Assert.NotNull(odataPath);
            Assert.Equal(template, odataPath.PathTemplate);
            Assert.Equal(expect, odataPath.ToString());
        }

        [Theory]
        [MemberData(nameof(PrefixFreeEnumCases))]
        public void PrefixFreeEnumValue_Works_PrefixFreeResolverAndDefaultResolver(string path, string template, string expect)
        {
            // Arrange: try default and non-default Uri resolvers.
            DefaultODataPathHandler pathHandler = new DefaultODataPathHandler();
            ODataUriResolver[] resolvers = { new StringAsEnumResolver(), null};

            // Act
            foreach (ODataUriResolver resolver in resolvers)
            {
                ODataPath odataPath = pathHandler.Parse(_model, _serviceRoot, path, resolver);

                // Assert
                Assert.NotNull(odataPath);
                Assert.Equal(template, odataPath.PathTemplate);
                Assert.Equal(expect, odataPath.ToString());
            }
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
            var expectedType = _model.FindDeclaredType("Microsoft.AspNet.OData.Test.Routing." + expectedTypeName) as IEdmEntityType;

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
            IEdmExpression expression = entitySet == null ? null : new EdmPathExpression(entitySet.Name);

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
