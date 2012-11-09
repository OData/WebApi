// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Routing
{
    public class ODataPathParserTest
    {
        private static ODataPathParser _parser = new ODataPathParser(GetModel());

        public static TheoryDataSet<string, string[]> ParseSegmentsData
        {
            get
            {
                return new TheoryDataSet<string, string[]>()
                {
                    { "", new string[] { } },
                    { "foo(12)/xy/g()", new string[] { "foo", "(12)", "xy", "g" } },
                    { "foo(12/xy", new string[] { "foo(12", "xy" } },
                    { "(/)", new string[] { "(", ")" } },
                    { "()", new string[] { } },
                    { ")(", new string[] { ")(" } },
                    { ")hahaha(", new string[] { ")hahaha(" } },
                    { "foo///bar", new string[] { "foo", "bar" } },
                    { "()/()/()/()ok", new string[] { "ok" } },
                    { "Customers(45)()(5)()()()(8)", new string[] { "Customers", "(45)", "(5)", "(8)" } },
                    { "Customers(45))()ok", new string[] { "Customers", "(45)", ")", "ok"} }
                };
            }
        }

        [Theory]
        [PropertyData("ParseSegmentsData")]
        public void ParseSegments_ParsesODataSegments(string relativePath, string[] expectedSegments)
        {
            IEnumerable<string> segments = _parser.ParseSegments(relativePath);

            Assert.Equal(expectedSegments, segments);
        }

        [Fact]
        public void Parse_RespectsRelativeBaseAddresses()
        {
            Uri uri = new Uri("http://myservice/api/Customers");
            Uri baseUri = new Uri("http://myservice/api");

            ODataPath path = _parser.Parse(uri, baseUri);

            Assert.NotNull(path);
            Assert.Equal("~/entityset", path.PathTemplate);
        }

        [Fact]
        public void Parse_WorksOnEncodedCharacters()
        {
            Uri uri = new Uri("http://myservice/üCategories");
            Uri baseUri = new Uri("http://myservice/");

            ODataPath path = _parser.Parse(uri, baseUri);
            ODataPathSegment segment = path.Segments.Last.Value;

            Assert.NotNull(path);
            Assert.Equal("~/entityset", path.PathTemplate);
            Assert.Equal("üCategories", path.Segments.Last.Value.ToString());
        }

        [Fact]
        public void Parse_ForInvalidCast_ThrowsODataException()
        {
            Uri uri = new Uri("http://myservice/Customers/System.Web.Http.OData.Routing.Product");
            Uri baseUri = new Uri("http://myservice/");

            Assert.Throws<ODataException>(
                () => _parser.Parse(uri, baseUri),
                "Invalid cast encountered. Cast type 'System.Web.Http.OData.Routing.Product' must be the same as or derive from the previous segment's type 'System.Web.Http.OData.Routing.Customer'.");
        }

        [Fact]
        public void Parse_ForSegmentAfterMetadata_ThrowsODataException()
        {
            Uri uri = new Uri("http://myservice/$metadata/foo");
            Uri baseUri = new Uri("http://myservice/");

            Assert.Throws<ODataException>(
                () => _parser.Parse(uri, baseUri),
                "The URI segment 'foo' is invalid after the segment '$metadata'.");
        }

        [Theory]
        [InlineData("http://myservice/", "~")]
        [InlineData("http://myservice/$metadata", "~/$metadata")]
        [InlineData("http://myservice/$batch", "~/$batch")]
        [InlineData("http://myservice/Customers(112)", "~/entityset/key")]
        [InlineData("http://myservice/Customers/System.Web.Http.OData.Routing.VIP", "~/entityset/cast")]
        [InlineData("http://myservice/Customers(100)/Products", "~/entityset/key/navigation")]
        [InlineData("http://myservice/Customers(100)/System.Web.Http.OData.Routing.VIP/RelationshipManager", "~/entityset/key/cast/navigation")]
        [InlineData("http://myservice/GetCustomerById()", "~/action")]
        [InlineData("http://myservice/Customers(112)/Address/Street", "~/entityset/key/property/property")]
        [InlineData("http://myservice/Customers(1)/Name/$value", "~/entityset/key/property/$value")]
        [InlineData("http://myservice/Customers(1)/$links/Products", "~/entityset/key/$links/navigation")]
        [InlineData("http://myservice/Customers(112)/GetRelatedCustomers", "~/entityset/key/action")]
        [InlineData("http://myservice/Customers/System.Web.Http.OData.Routing.VIP/GetMostProfitable", "~/entityset/cast/action")]
        [InlineData("http://myservice/Products(1)/Customers/System.Web.Http.OData.Routing.VIP(1)/RelationshipManager/ManagedProducts", "~/entityset/key/navigation/cast/key/navigation/navigation")]
        public void Parse_ReturnsPath_WithCorrectTemplate(string uri, string template)
        {
            ODataPath path = _parser.Parse(new Uri(uri), new Uri("http://myservice/"));

            Assert.NotNull(path);
            Assert.Equal(template, path.PathTemplate);
        }

        [Fact]
        public void CanParseUrlWithNoModelElements()
        {
            // Arrange
            Uri uri = new Uri("http://myservice/1/2()/3/4()/5");
            Uri baseUri = new Uri("http://myservice/");

            // Act
            ODataPath path = _parser.Parse(uri, baseUri);

            // Assert
            Assert.Null(path);
        }

        [Fact]
        public void CanParseMetadataUrl()
        {
            string testUrl = "http://myservice/$metadata";
            Uri uri = new Uri(testUrl);
            Uri baseUri = new Uri("http://myservice/");

            ODataPath path = _parser.Parse(uri, baseUri);
            ODataPathSegment segment = path.Segments.Last.Value;

            // Assert
            Assert.NotNull(path);
            Assert.Null(path.EntitySet);
            Assert.Null(path.EdmType);
            Assert.Equal("$metadata", segment.ToString());
            Assert.NotNull(segment.Previous);
            Assert.Equal("http://myservice/", segment.Previous.ToString());
        }

        [Fact]
        public void CanParseBatchUrl()
        {
            // Arrange
            string testUrl = "http://myservice/$batch";
            Uri uri = new Uri(testUrl);
            Uri baseUri = new Uri("http://myservice/");

            // Act
            ODataPath path = _parser.Parse(uri, baseUri);
            ODataPathSegment segment = path.Segments.Last.Value;

            // Assert
            Assert.NotNull(path);
            Assert.NotNull(segment);
            Assert.Null(path.EntitySet);
            Assert.Null(path.EdmType);
            Assert.Equal("$batch", segment.ToString());
            Assert.NotNull(segment.Previous);
            Assert.Equal("http://myservice/", segment.Previous.ToString());
        }

        [Fact]
        public void CanParseEntitySetUrl()
        {
            // Arrange
            string testUrl = "http://myservice/Customers";
            string expectedText = "Customers";
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "Customers");
            Uri uri = new Uri(testUrl);
            Uri baseUri = new Uri("http://myservice/");

            // Act
            ODataPath path = _parser.Parse(uri, baseUri);
            ODataPathSegment segment = path.Segments.Last.Value;

            // Assert
            Assert.NotNull(path);
            Assert.NotNull(segment);
            Assert.NotNull(segment.Previous);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, segment.EntitySet);
            Assert.Same(expectedSet.ElementType, (segment.EdmType as IEdmCollectionType).ElementType.Definition);
        }

        [Fact]
        public void CanParseKeyUrl()
        {
            // Arrange
            string testUrl = "http://myservice/Customers(112)";
            string expectedText = "112";
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "Customers");
            Uri uri = new Uri(testUrl);
            Uri baseUri = new Uri("http://myservice/");

            // Act
            ODataPath path = _parser.Parse(uri, baseUri);
            ODataPathSegment segment = path.Segments.Last.Value;

            // Assert
            Assert.NotNull(segment);
            Assert.NotNull(segment.Previous);
            Assert.Equal(expectedText, segment.ToString());
            Assert.IsType<KeyValuePathSegment>(segment);
            Assert.Same(expectedSet, segment.EntitySet);
            Assert.Same(expectedSet.ElementType, segment.EdmType);
        }

        [Fact]
        public void CanParseCastCollectionSegment()
        {
            // Arrange
            string testUrl = "http://myservice/Customers/System.Web.Http.OData.Routing.VIP";
            string expectedText = "System.Web.Http.OData.Routing.VIP";
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "Customers");
            IEdmEntityType expectedType = _parser.Model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "VIP");
            Uri uri = new Uri(testUrl);
            Uri baseUri = new Uri("http://myservice/");

            // Act
            ODataPath path = _parser.Parse(uri, baseUri);
            ODataPathSegment segment = path.Segments.Last.Value;

            // Assert
            Assert.NotNull(segment);
            Assert.NotNull(segment.Previous);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, segment.EntitySet);
            Assert.Equal(expectedType, (segment.EdmType as IEdmCollectionType).ElementType.Definition);
        }

        [Fact]
        public void CanParseCastEntitySegment()
        {
            // Arrange
            string testUrl = "http://myservice/Customers(100)/System.Web.Http.OData.Routing.VIP";
            string expectedText = "System.Web.Http.OData.Routing.VIP";
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "Customers");
            IEdmEntityType expectedType = _parser.Model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "VIP");
            Uri uri = new Uri(testUrl);
            Uri baseUri = new Uri("http://myservice/");

            // Act
            ODataPath path = _parser.Parse(uri, baseUri);
            ODataPathSegment segment = path.Segments.Last.Value;

            // Assert
            Assert.NotNull(segment);
            Assert.NotNull(segment.Previous);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, segment.EntitySet);
            Assert.Equal(expectedType, segment.EdmType);
        }

        [Fact]
        public void CanParseNavigateToCollectionSegment()
        {
            // Arrange
            string testUrl = "http://myservice/Customers(100)/Products";
            string expectedText = "Products";
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "Products");
            IEdmNavigationProperty expectedEdmElement = _parser.Model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "Customer").NavigationProperties().SingleOrDefault(n => n.Name == "Products");
            Uri uri = new Uri(testUrl);
            Uri baseUri = new Uri("http://myservice/");

            // Act
            ODataPath path = _parser.Parse(uri, baseUri);
            ODataPathSegment segment = path.Segments.Last.Value;

            // Assert
            Assert.NotNull(segment);
            Assert.NotNull(segment.Previous);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, segment.EntitySet);
            Assert.Equal(expectedSet.ElementType, (segment.EdmType as IEdmCollectionType).ElementType.Definition);
            NavigationPathSegment navigation = Assert.IsType<NavigationPathSegment>(segment);
            Assert.Same(expectedEdmElement, navigation.NavigationProperty);
        }

        [Fact]
        public void CanParseNavigateToSingleSegment()
        {
            // Arrange
            string testUrl = "http://myservice/Customers(100)/System.Web.Http.OData.Routing.VIP/RelationshipManager";
            string expectedText = "RelationshipManager";
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "SalesPeople");
            IEdmNavigationProperty expectedEdmElement = _parser.Model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "VIP").NavigationProperties().SingleOrDefault(n => n.Name == "RelationshipManager");
            Uri uri = new Uri(testUrl);
            Uri baseUri = new Uri("http://myservice/");

            // Act
            ODataPath path = _parser.Parse(uri, baseUri);
            ODataPathSegment segment = path.Segments.Last.Value;

            // Assert
            Assert.NotNull(segment);
            Assert.NotNull(segment.Previous);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, segment.EntitySet);
            Assert.Equal(expectedSet.ElementType, segment.EdmType);
            NavigationPathSegment navigation = Assert.IsType<NavigationPathSegment>(segment);
            Assert.Same(expectedEdmElement, navigation.NavigationProperty);
        }

        [Fact]
        public void CanParseRootProcedureSegment()
        {
            // Arrange
            string testUrl = "http://myservice/GetCustomerById()";
            string expectedText = "Default.Container.GetCustomerById";
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "Customers");
            IEdmFunctionImport expectedEdmElement = _parser.Model.EntityContainers().First().FunctionImports().SingleOrDefault(s => s.Name == "GetCustomerById");
            Uri uri = new Uri(testUrl);
            Uri baseUri = new Uri("http://myservice/");

            // Act
            ODataPath path = _parser.Parse(uri, baseUri);
            ODataPathSegment segment = path.Segments.Last.Value;

            // Assert
            Assert.NotNull(segment);
            Assert.NotNull(segment.Previous);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, segment.EntitySet);
            Assert.Equal(expectedSet.ElementType, segment.EdmType);
            ActionPathSegment action = Assert.IsType<ActionPathSegment>(segment);
            Assert.Same(expectedEdmElement, action.Action);
        }

        [Fact]
        public void CanParsePropertySegment()
        {
            // Arrange
            string testUrl = "http://myservice/Customers(112)/Name";
            string expectedText = "Name";
            IEdmProperty expectedEdmElement = _parser.Model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(e => e.Name == "Customer").Properties().SingleOrDefault(p => p.Name == "Name");
            IEdmType expectedType = expectedEdmElement.Type.Definition;
            Uri uri = new Uri(testUrl);
            Uri baseUri = new Uri("http://myservice/");

            // Act
            ODataPath path = _parser.Parse(uri, baseUri);
            ODataPathSegment segment = path.Segments.Last.Value;

            // Assert
            Assert.NotNull(segment);
            Assert.NotNull(segment.Previous);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Null(segment.EntitySet);
            PropertyAccessPathSegment propertyAccess = Assert.IsType<PropertyAccessPathSegment>(segment);
            Assert.Same(expectedEdmElement, propertyAccess.Property);
        }

        [Fact]
        public void CanParseComplexPropertySegment()
        {
            // Arrange
            string testUrl = "http://myservice/Customers(112)/Address";
            string expectedText = "Address";
            IEdmProperty expectedEdmElement = _parser.Model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(e => e.Name == "Customer").Properties().SingleOrDefault(p => p.Name == "Address");
            IEdmType expectedType = expectedEdmElement.Type.Definition;
            Uri uri = new Uri(testUrl);
            Uri baseUri = new Uri("http://myservice/");

            // Act
            ODataPath path = _parser.Parse(uri, baseUri);
            ODataPathSegment segment = path.Segments.Last.Value;

            // Assert
            Assert.NotNull(segment);
            Assert.NotNull(segment.Previous);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Null(segment.EntitySet);
            Assert.Same(expectedType, segment.EdmType);
            PropertyAccessPathSegment propertyAccess = Assert.IsType<PropertyAccessPathSegment>(segment);
            Assert.Same(expectedEdmElement, propertyAccess.Property);
        }

        [Fact]
        public void CanParsePropertyOfComplexSegment()
        {
            // Arrange
            string testUrl = "http://myservice/Customers(112)/Address/Street";
            string expectedText = "Street";
            IEdmProperty expectedEdmElement = _parser.Model.SchemaElements.OfType<IEdmComplexType>().SingleOrDefault(e => e.Name == "Address").Properties().SingleOrDefault(p => p.Name == "Street");
            IEdmType expectedType = expectedEdmElement.Type.Definition;
            Uri uri = new Uri(testUrl);
            Uri baseUri = new Uri("http://myservice/");

            // Act
            ODataPath path = _parser.Parse(uri, baseUri);
            ODataPathSegment segment = path.Segments.Last.Value;

            // Assert
            Assert.NotNull(segment);
            Assert.NotNull(segment.Previous);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Null(segment.EntitySet);
            Assert.Same(expectedType, segment.EdmType);
            PropertyAccessPathSegment propertyAccess = Assert.IsType<PropertyAccessPathSegment>(segment);
            Assert.Same(expectedEdmElement, propertyAccess.Property);
        }

        [Fact]
        public void CanParsePropertyValueSegment()
        {
            // Arrange
            string testUrl = "http://myservice/Customers(1)/Name/$value";
            Uri uri = new Uri(testUrl);
            Uri baseUri = new Uri("http://myservice/");

            // Act
            ODataPath path = _parser.Parse(uri, baseUri);
            ODataPathSegment segment = path.Segments.Last.Value;

            // Assert
            Assert.NotNull(segment);
            Assert.Equal("$value", segment.ToString());
            Assert.Null(segment.EntitySet);
            Assert.NotNull(segment.EdmType);
            Assert.Equal("Edm.String", (segment.EdmType as IEdmPrimitiveType).FullName());
        }

        [Fact]
        public void CanParseEntityLinksSegment()
        {
            // Arrange
            string testUrl = "http://myservice/Customers(1)/$links/Products";
            Uri uri = new Uri(testUrl);
            Uri baseUri = new Uri("http://myservice/");
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "Products");
            IEdmEntityType expectedType = expectedSet.ElementType;

            // Act
            ODataPath path = _parser.Parse(uri, baseUri);
            ODataPathSegment segment = path.Segments.Last.Value;

            // Assert
            Assert.NotNull(segment);
            Assert.Same(expectedType, (segment.EdmType as IEdmCollectionType).ElementType.Definition);
            Assert.Same(expectedSet, segment.EntitySet);
            Assert.Same("$links", segment.Previous.ToString());
        }

        [Fact]
        public void CanParseActionBoundToEntitySegment()
        {
            // Arrange
            string testUrl = "http://myservice/Customers(112)/GetRelatedCustomers";
            string expectedText = "Default.Container.GetRelatedCustomers";
            IEdmFunctionImport expectedEdmElement = _parser.Model.EntityContainers().First().FunctionImports().SingleOrDefault(p => p.Name == "GetRelatedCustomers");
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(e => e.Name == "Customers");
            IEdmType expectedType = expectedEdmElement.ReturnType.Definition;
            Uri uri = new Uri(testUrl);
            Uri baseUri = new Uri("http://myservice/");

            // Act
            ODataPath path = _parser.Parse(uri, baseUri);
            ODataPathSegment segment = path.Segments.Last.Value;

            // Assert
            Assert.NotNull(segment);
            Assert.NotNull(segment.Previous);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, segment.EntitySet);
            Assert.Same(expectedType, segment.EdmType);
            ActionPathSegment action = Assert.IsType<ActionPathSegment>(segment);
            Assert.Same(expectedEdmElement, action.Action);
        }

        [Fact]
        public void CanParseActionBoundToCollectionSegment()
        {
            // Arrange
            string testUrl = "http://myservice/Customers/System.Web.Http.OData.Routing.VIP/GetMostProfitable";
            string expectedText = "Default.Container.GetMostProfitable";
            IEdmFunctionImport expectedEdmElement = _parser.Model.EntityContainers().First().FunctionImports().SingleOrDefault(p => p.Name == "GetMostProfitable");
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(e => e.Name == "Customers");
            IEdmType expectedType = expectedEdmElement.ReturnType.Definition;
            Uri uri = new Uri(testUrl);
            Uri baseUri = new Uri("http://myservice/");

            // Act
            ODataPath path = _parser.Parse(uri, baseUri);
            ODataPathSegment segment = path.Segments.Last.Value;

            // Assert
            Assert.NotNull(segment);
            Assert.NotNull(segment.Previous);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, segment.EntitySet);
            Assert.Same(expectedType, segment.EdmType);
            ActionPathSegment action = Assert.IsType<ActionPathSegment>(segment);
            Assert.Same(expectedEdmElement, action.Action);
        }

        [Theory]
        [InlineData("http://myservice/Customers", "Customers", "Customer", true)]
        [InlineData("http://myservice/Customers/", "Customers", "Customer", true)]
        [InlineData("http://myservice/Products", "Products", "Product", true)]
        [InlineData("http://myservice/Products/", "Products", "Product", true)]
        [InlineData("http://myservice/SalesPeople", "SalesPeople", "SalesPerson", true)]
        public void CanResolveSetAndTypeViaSimpleEntitySetSegment(string testUrl, string expectedSetName, string expectedTypeName, bool isCollection)
        {
            // Arrange
            Uri uri = new Uri(testUrl);
            Uri baseUri = new Uri("http://myservice/");
            var model = _parser.Model;
            var expectedSet = model.FindDeclaredEntityContainer("Container").FindEntitySet(expectedSetName);
            var expectedType = model.FindDeclaredType("System.Web.Http.OData.Routing." + expectedTypeName) as IEdmEntityType;

            // Act
            ODataPath path = _parser.Parse(uri, baseUri);
            ODataPathSegment segment = path.Segments.Last.Value;

            // Assert
            Assert.NotNull(segment);
            Assert.NotNull(segment.EntitySet);
            Assert.NotNull(segment.EdmType);
            Assert.Same(expectedSet, segment.EntitySet);
            if (isCollection)
            {
                Assert.Equal(EdmTypeKind.Collection, segment.EdmType.TypeKind);
                Assert.Same(expectedType, (segment.EdmType as IEdmCollectionType).ElementType.Definition);
            }
            else
            {
                Assert.Same(expectedType, segment.EdmType);
            }
        }

        [Theory]
        [InlineData("http://myservice/Customers(1)", "Customers", "Customer", false)]
        [InlineData("http://myservice/Customers(1)/", "Customers", "Customer", false)]
        [InlineData("http://myservice/Products(1)", "Products", "Product", false)]
        [InlineData("http://myservice/Products(1)/", "Products", "Product", false)]
        [InlineData("http://myservice/Products(1)", "Products", "Product", false)]
        public void CanResolveSetAndTypeViaKeySegment(string testUrl, string expectedSetName, string expectedTypeName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(testUrl, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("http://myservice/Customers(1)/Products", "Products", "Product", true)]
        [InlineData("http://myservice/Customers(1)/Products(1)", "Products", "Product", false)]
        [InlineData("http://myservice/Customers(1)/Products/", "Products", "Product", true)]
        [InlineData("http://myservice/Customers(1)/Products", "Products", "Product", true)]
        [InlineData("http://myservice/Products(1)/Customers", "Customers", "Customer", true)]
        [InlineData("http://myservice/Products(1)/Customers(1)", "Customers", "Customer", false)]
        [InlineData("http://myservice/Products(1)/Customers/", "Customers", "Customer", true)]
        public void CanResolveSetAndTypeViaNavigationPropertySegment(string testUrl, string expectedSetName, string expectedTypeName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(testUrl, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("http://myservice/Customers/System.Web.Http.OData.Routing.VIP", "VIP", "Customers", true)]
        [InlineData("http://myservice/Customers(1)/System.Web.Http.OData.Routing.VIP", "VIP", "Customers", false)]
        [InlineData("http://myservice/Products(1)/System.Web.Http.OData.Routing.ImportantProduct", "ImportantProduct", "Products", false)]
        [InlineData("http://myservice/Products(1)/Customers/System.Web.Http.OData.Routing.VIP", "VIP", "Customers", true)]
        [InlineData("http://myservice/SalesPeople(1)/ManagedCustomers", "VIP", "Customers", true)]
        [InlineData("http://myservice/Customers(1)/System.Web.Http.OData.Routing.VIP/RelationshipManager", "SalesPerson", "SalesPeople", false)]
        [InlineData("http://myservice/Products/System.Web.Http.OData.Routing.ImportantProduct(1)/LeadSalesPerson", "SalesPerson", "SalesPeople", false)]
        [InlineData("http://myservice/Products(1)/Customers/System.Web.Http.OData.Routing.VIP(1)/RelationshipManager/ManagedProducts", "ImportantProduct", "Products", true)]
        public void CanResolveSetAndTypeViaCastSegment(string testUrl, string expectedTypeName, string expectedSetName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(testUrl, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("http://myservice/GetCustomerById", "Customer", "Customers", false)]
        [InlineData("http://myservice/GetSalesPersonById", "SalesPerson", "SalesPeople", false)]
        [InlineData("http://myservice/GetAllVIPs", "VIP", "Customers", true)]
        public void CanResolveSetAndTypeViaRootProcedureSegment(string testUrl, string expectedTypeName, string expectedSetName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(testUrl, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("http://myservice/Customers(1)/GetRelatedCustomers", "Customer", "Customers", true)]
        [InlineData("http://myservice/Customers(1)/GetBestRelatedCustomer", "VIP", "Customers", false)]
        [InlineData("http://myservice/Customers(1)/System.Web.Http.OData.Routing.VIP/GetSalesPerson", "SalesPerson", "SalesPeople", false)]
        [InlineData("http://myservice/SalesPeople(1)/GetVIPCustomers", "VIP", "Customers", true)]
        public void CanResolveSetAndTypeViaEntityActionSegment(string testUrl, string expectedTypeName, string expectedSetName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(testUrl, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("http://myservice/Customers/GetVIPs", "VIP", "Customers", true)]
        [InlineData("http://myservice/Customers/GetProducts", "Product", "Products", true)]
        [InlineData("http://myservice/Products(1)/Customers/System.Web.Http.OData.Routing.VIP/GetSalesPeople", "SalesPerson", "SalesPeople", true)]
        [InlineData("http://myservice/SalesPeople/GetVIPCustomers", "VIP", "Customers", true)]
        [InlineData("http://myservice/Customers/System.Web.Http.OData.Routing.VIP/GetMostProfitable", "VIP", "Customers", false)]
        public void CanResolveSetAndTypeViaCollectionActionSegment(string testUrl, string expectedTypeName, string expectedSetName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(testUrl, expectedSetName, expectedTypeName, isCollection);
        }

        private static void AssertTypeMatchesExpectedType(string testUrl, string expectedSetName, string expectedTypeName, bool isCollection)
        {
            // Arrange
            Uri uri = new Uri(testUrl);
            Uri baseUri = new Uri("http://myservice/");
            var model = _parser.Model;
            var expectedSet = model.FindDeclaredEntityContainer("Container").FindEntitySet(expectedSetName);
            var expectedType = model.FindDeclaredType("System.Web.Http.OData.Routing." + expectedTypeName) as IEdmEntityType;

            // Act
            ODataPath path = _parser.Parse(uri, baseUri);
            ODataPathSegment segment = path.Segments.Last.Value;

            // Assert
            Assert.NotNull(segment);
            Assert.NotNull(segment.EntitySet);
            Assert.NotNull(segment.EdmType);
            Assert.Same(expectedSet, segment.EntitySet);
            if (isCollection)
            {
                Assert.Equal(EdmTypeKind.Collection, segment.EdmType.TypeKind);
                Assert.Same(expectedType, (segment.EdmType as IEdmCollectionType).ElementType.Definition);
            }
            else
            {
                Assert.Same(expectedType, segment.EdmType);
            }
        }

        public static IEdmModel GetModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Product>("Products");
            builder.EntitySet<SalesPerson>("SalesPeople");
            builder.EntitySet<EmailAddress>("EmailAddresses");
            builder.EntitySet<üCategory>("üCategories");

            ActionConfiguration getCustomerById = new ActionConfiguration(builder, "GetCustomerById");
            getCustomerById.Parameter<int>("customerId");
            getCustomerById.ReturnsFromEntitySet<Customer>("Customers");

            ActionConfiguration getSalesPersonById = new ActionConfiguration(builder, "GetSalesPersonById");
            getSalesPersonById.Parameter<int>("salesPersonId");
            getSalesPersonById.ReturnsFromEntitySet<SalesPerson>("SalesPeople");

            ActionConfiguration getAllVIPs = new ActionConfiguration(builder, "GetAllVIPs");
            ActionReturnsCollectionFromEntitySet<VIP>(builder, getAllVIPs, "Customers");

            builder.Entity<Customer>().ComplexProperty<Address>(c => c.Address);
            builder.Entity<Customer>().Action("GetRelatedCustomers").ReturnsCollectionFromEntitySet<Customer>("Customers");

            ActionConfiguration getBestRelatedCustomer = builder.Entity<Customer>().Action("GetBestRelatedCustomer");
            ActionReturnsFromEntitySet<VIP>(builder, getBestRelatedCustomer, "Customers");

            ActionConfiguration getVIPS = builder.Entity<Customer>().Collection.Action("GetVIPs");
            ActionReturnsCollectionFromEntitySet<VIP>(builder, getVIPS, "Customers");

            builder.Entity<Customer>().Collection.Action("GetProducts").ReturnsCollectionFromEntitySet<Product>("Products");
            builder.Entity<VIP>().Action("GetSalesPerson").ReturnsFromEntitySet<SalesPerson>("SalesPeople");
            builder.Entity<VIP>().Collection.Action("GetSalesPeople").ReturnsCollectionFromEntitySet<SalesPerson>("SalesPeople");

            ActionConfiguration getMostProfitable = builder.Entity<VIP>().Collection.Action("GetMostProfitable");
            ActionReturnsFromEntitySet<VIP>(builder, getMostProfitable, "Customers");

            ActionConfiguration getVIPCustomers = builder.Entity<SalesPerson>().Action("GetVIPCustomers");
            ActionReturnsCollectionFromEntitySet<VIP>(builder, getVIPCustomers, "Customers");

            ActionConfiguration getVIPCustomersOnCollection = builder.Entity<SalesPerson>().Collection.Action("GetVIPCustomers");
            ActionReturnsCollectionFromEntitySet<VIP>(builder, getVIPCustomersOnCollection, "Customers");

            builder.Entity<VIP>().HasRequired(v => v.RelationshipManager);
            builder.Entity<ImportantProduct>().HasRequired(ip => ip.LeadSalesPerson);

            return builder.GetEdmModel();
        }

        public static ActionConfiguration ActionReturnsFromEntitySet<TEntityType>(ODataModelBuilder builder, ActionConfiguration action, string entitySetName) where TEntityType : class
        {
            action.EntitySet = CreateOrReuseEntitySet<TEntityType>(builder, entitySetName);
            action.ReturnType = builder.GetTypeConfigurationOrNull(typeof(TEntityType));
            return action;
        }

        public static ActionConfiguration ActionReturnsCollectionFromEntitySet<TElementEntityType>(ODataModelBuilder builder, ActionConfiguration action, string entitySetName) where TElementEntityType : class
        {
            Type clrCollectionType = typeof(IEnumerable<TElementEntityType>);
            action.EntitySet = CreateOrReuseEntitySet<TElementEntityType>(builder, entitySetName);
            IEdmTypeConfiguration elementType = builder.GetTypeConfigurationOrNull(typeof(TElementEntityType));
            action.ReturnType = new CollectionTypeConfiguration(elementType, clrCollectionType);
            return action;
        }

        public static EntitySetConfiguration CreateOrReuseEntitySet<TElementEntityType>(ODataModelBuilder builder, string entitySetName) where TElementEntityType : class
        {
            EntitySetConfiguration entitySet = builder.EntitySets.SingleOrDefault(s => s.Name == entitySetName);

            if (entitySet == null)
            {
                builder.EntitySet<TElementEntityType>(entitySetName);
                entitySet = builder.EntitySets.Single(s => s.Name == entitySetName);
            }
            else
            {
                builder.Entity<TElementEntityType>();
            }
            return entitySet;
        }

        public class Customer
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public virtual List<Product> Products { get; set; }
            public Address Address { get; set; }
        }

        public class EmailAddress
        {
            [Key]
            public string Value { get; set; }
            public string Text { get; set; }
        }

        public class Address
        {
            public string Street { get; set; }
            public string City { get; set; }
            public string ZipCode { get; set; }
        }

        public class Product
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public virtual List<Customer> Customers { get; set; }
        }

        public class SalesPerson
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public virtual List<VIP> ManagedCustomers { get; set; }
            public virtual List<ImportantProduct> ManagedProducts { get; set; }
        }

        public class VIP : Customer
        {
            public virtual SalesPerson RelationshipManager { get; set; }
        }

        public class ImportantProduct : Product
        {
            public virtual SalesPerson LeadSalesPerson { get; set; }
        }

        public class üCategory
        {
            public int ID { get; set; }
        }
    }
}
