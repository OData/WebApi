// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Routing
{
    public class DefaultODataPathHandlerTest
    {
        private static DefaultODataPathHandler _parser = new DefaultODataPathHandler();
        private static IEdmModel _model = ODataRoutingModel.GetModel();

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
                    { "RoutingCustomers(45)()(5)()()()(8)", new string[] { "RoutingCustomers", "(45)", "(5)", "(8)" } },
                    { "RoutingCustomers(45))()ok", new string[] { "RoutingCustomers", "(45)", ")", "ok"} }
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
        public void Parse_WorksOnEncodedCharacters()
        {
            string odataPath = "üCategories";

            ODataPath path = _parser.Parse(_model, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            Assert.NotNull(path);
            Assert.Equal("~/entityset", path.PathTemplate);
            Assert.Equal("üCategories", segment.ToString());
        }

        [Fact]
        public void Parse_ForInvalidCast_ThrowsODataException()
        {
            string odataPath = "RoutingCustomers/System.Web.Http.OData.Routing.Product";

            Assert.Throws<ODataException>(
                () => _parser.Parse(_model, odataPath),
                "Invalid cast encountered. Cast type 'System.Web.Http.OData.Routing.Product' must be the same as or derive from the previous segment's type 'System.Web.Http.OData.Routing.RoutingCustomer'.");
        }

        [Fact]
        public void Parse_ForSegmentAfterMetadata_ThrowsODataException()
        {
            string odataPath = "$metadata/foo";

            Assert.Throws<ODataException>(
                () => _parser.Parse(_model, odataPath),
                "The URI segment 'foo' is invalid after the segment '$metadata'.");
        }

        [Theory]
        [InlineData("", "~")]
        [InlineData("$metadata", "~/$metadata")]
        [InlineData("$batch", "~/$batch")]
        [InlineData("RoutingCustomers(112)", "~/entityset/key")]
        [InlineData("RoutingCustomers/System.Web.Http.OData.Routing.VIP", "~/entityset/cast")]
        [InlineData("RoutingCustomers(100)/Products", "~/entityset/key/navigation")]
        [InlineData("RoutingCustomers(100)/System.Web.Http.OData.Routing.VIP/RelationshipManager", "~/entityset/key/cast/navigation")]
        [InlineData("GetRoutingCustomerById()", "~/action")]
        [InlineData("RoutingCustomers(112)/Address/Street", "~/entityset/key/property/property")]
        [InlineData("RoutingCustomers(1)/Name/$value", "~/entityset/key/property/$value")]
        [InlineData("RoutingCustomers(1)/$links/Products", "~/entityset/key/$links/navigation")]
        [InlineData("RoutingCustomers(112)/GetRelatedRoutingCustomers", "~/entityset/key/action")]
        [InlineData("RoutingCustomers/System.Web.Http.OData.Routing.VIP/GetMostProfitable", "~/entityset/cast/action")]
        [InlineData("Products(1)/RoutingCustomers/System.Web.Http.OData.Routing.VIP(1)/RelationshipManager/ManagedProducts", "~/entityset/key/navigation/cast/key/navigation/navigation")]
        public void Parse_ReturnsPath_WithCorrectTemplate(string odataPath, string template)
        {
            ODataPath path = _parser.Parse(_model, odataPath);

            Assert.NotNull(path);
            Assert.Equal(template, path.PathTemplate);
        }

        [Fact]
        public void CanParseUrlWithNoModelElements()
        {
            // Arrange
            string odataPath = "1/2()/3/4()/5";

            // Act
            ODataPath path = _parser.Parse(_model, odataPath);

            // Assert
            Assert.Null(path);
        }

        [Fact]
        public void CanParseMetadataUrl()
        {
            string odataPath = "$metadata";

            ODataPath path = _parser.Parse(_model, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(path);
            Assert.Null(path.EntitySet);
            Assert.Null(path.EdmType);
            Assert.Equal("$metadata", segment.ToString());
        }

        [Fact]
        public void CanParseBatchUrl()
        {
            // Arrange
            string odataPath = "$batch";

            // Act
            ODataPath path = _parser.Parse(_model, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(path);
            Assert.NotNull(segment);
            Assert.Null(path.EntitySet);
            Assert.Null(path.EdmType);
            Assert.Equal("$batch", segment.ToString());
        }

        [Fact]
        public void CanParseEntitySetUrl()
        {
            // Arrange
            string odataPath = "RoutingCustomers";
            string expectedText = "RoutingCustomers";
            IEdmEntitySet expectedSet = _model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "RoutingCustomers");

            // Act
            ODataPath path = _parser.Parse(_model, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(path);
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, path.EntitySet);
            Assert.Same(expectedSet.ElementType, (path.EdmType as IEdmCollectionType).ElementType.Definition);
        }

        [Fact]
        public void CanParseKeyUrl()
        {
            // Arrange
            string odataPath = "RoutingCustomers(112)";
            string expectedText = "112";
            IEdmEntitySet expectedSet = _model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "RoutingCustomers");

            // Act
            ODataPath path = _parser.Parse(_model, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.IsType<KeyValuePathSegment>(segment);
            Assert.Same(expectedSet, path.EntitySet);
            Assert.Same(expectedSet.ElementType, path.EdmType);
        }

        [Fact]
        public void CanParseCastCollectionSegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers/System.Web.Http.OData.Routing.VIP";
            string expectedText = "System.Web.Http.OData.Routing.VIP";
            IEdmEntitySet expectedSet = _model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "RoutingCustomers");
            IEdmEntityType expectedType = _model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "VIP");

            // Act
            ODataPath path = _parser.Parse(_model, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, path.EntitySet);
            Assert.Equal(expectedType, (path.EdmType as IEdmCollectionType).ElementType.Definition);
        }

        [Fact]
        public void CanParseCastEntitySegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers(100)/System.Web.Http.OData.Routing.VIP";
            string expectedText = "System.Web.Http.OData.Routing.VIP";
            IEdmEntitySet expectedSet = _model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "RoutingCustomers");
            IEdmEntityType expectedType = _model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "VIP");

            // Act
            ODataPath path = _parser.Parse(_model, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, path.EntitySet);
            Assert.Equal(expectedType, path.EdmType);
        }

        [Fact]
        public void CanParseNavigateToCollectionSegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers(100)/Products";
            string expectedText = "Products";
            IEdmEntitySet expectedSet = _model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "Products");
            IEdmNavigationProperty expectedEdmElement = _model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "RoutingCustomer").NavigationProperties().SingleOrDefault(n => n.Name == "Products");

            // Act
            ODataPath path = _parser.Parse(_model, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, path.EntitySet);
            Assert.Equal(expectedSet.ElementType, (path.EdmType as IEdmCollectionType).ElementType.Definition);
            NavigationPathSegment navigation = Assert.IsType<NavigationPathSegment>(segment);
            Assert.Same(expectedEdmElement, navigation.NavigationProperty);
        }

        [Fact]
        public void CanParseNavigateToSingleSegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers(100)/System.Web.Http.OData.Routing.VIP/RelationshipManager";
            string expectedText = "RelationshipManager";
            IEdmEntitySet expectedSet = _model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "SalesPeople");
            IEdmNavigationProperty expectedEdmElement = _model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "VIP").NavigationProperties().SingleOrDefault(n => n.Name == "RelationshipManager");

            // Act
            ODataPath path = _parser.Parse(_model, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, path.EntitySet);
            Assert.Equal(expectedSet.ElementType, path.EdmType);
            NavigationPathSegment navigation = Assert.IsType<NavigationPathSegment>(segment);
            Assert.Same(expectedEdmElement, navigation.NavigationProperty);
        }

        [Fact]
        public void CanParseRootProcedureSegment()
        {
            // Arrange
            string odataPath = "GetRoutingCustomerById()";
            string expectedText = "Default.Container.GetRoutingCustomerById";
            IEdmEntitySet expectedSet = _model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "RoutingCustomers");
            IEdmFunctionImport expectedEdmElement = _model.EntityContainers().First().FunctionImports().SingleOrDefault(s => s.Name == "GetRoutingCustomerById");

            // Act
            ODataPath path = _parser.Parse(_model, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, path.EntitySet);
            Assert.Equal(expectedSet.ElementType, path.EdmType);
            ActionPathSegment action = Assert.IsType<ActionPathSegment>(segment);
            Assert.Same(expectedEdmElement, action.Action);
        }

        [Fact]
        public void CanParsePropertySegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers(112)/Name";
            string expectedText = "Name";
            IEdmProperty expectedEdmElement = _model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(e => e.Name == "RoutingCustomer").Properties().SingleOrDefault(p => p.Name == "Name");
            IEdmType expectedType = expectedEdmElement.Type.Definition;

            // Act
            ODataPath path = _parser.Parse(_model, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Null(path.EntitySet);
            PropertyAccessPathSegment propertyAccess = Assert.IsType<PropertyAccessPathSegment>(segment);
            Assert.Same(expectedEdmElement, propertyAccess.Property);
        }

        [Fact]
        public void CanParseComplexPropertySegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers(112)/Address";
            string expectedText = "Address";
            IEdmProperty expectedEdmElement = _model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(e => e.Name == "RoutingCustomer").Properties().SingleOrDefault(p => p.Name == "Address");
            IEdmType expectedType = expectedEdmElement.Type.Definition;

            // Act
            ODataPath path = _parser.Parse(_model, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Null(path.EntitySet);
            Assert.Same(expectedType, path.EdmType);
            PropertyAccessPathSegment propertyAccess = Assert.IsType<PropertyAccessPathSegment>(segment);
            Assert.Same(expectedEdmElement, propertyAccess.Property);
        }

        [Fact]
        public void CanParsePropertyOfComplexSegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers(112)/Address/Street";
            string expectedText = "Street";
            IEdmProperty expectedEdmElement = _model.SchemaElements.OfType<IEdmComplexType>().SingleOrDefault(e => e.Name == "Address").Properties().SingleOrDefault(p => p.Name == "Street");
            IEdmType expectedType = expectedEdmElement.Type.Definition;

            // Act
            ODataPath path = _parser.Parse(_model, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Null(path.EntitySet);
            Assert.Same(expectedType, path.EdmType);
            PropertyAccessPathSegment propertyAccess = Assert.IsType<PropertyAccessPathSegment>(segment);
            Assert.Same(expectedEdmElement, propertyAccess.Property);
        }

        [Fact]
        public void CanParsePropertyValueSegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers(1)/Name/$value";

            // Act
            ODataPath path = _parser.Parse(_model, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal("$value", segment.ToString());
            Assert.Null(path.EntitySet);
            Assert.NotNull(path.EdmType);
            Assert.Equal("Edm.String", (path.EdmType as IEdmPrimitiveType).FullName());
        }

        [Fact]
        public void CanParseEntityLinksSegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers(1)/$links/Products";
            IEdmEntitySet expectedSet = _model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "Products");
            IEdmEntityType expectedType = expectedSet.ElementType;

            // Act
            ODataPath path = _parser.Parse(_model, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Same(expectedType, (path.EdmType as IEdmCollectionType).ElementType.Definition);
            Assert.Same(expectedSet, path.EntitySet);
            Assert.Same("$links", path.Segments[2].ToString());
        }

        [Fact]
        public void CanParseActionBoundToEntitySegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers(112)/GetRelatedRoutingCustomers";
            string expectedText = "Default.Container.GetRelatedRoutingCustomers";
            IEdmFunctionImport expectedEdmElement = _model.EntityContainers().First().FunctionImports().SingleOrDefault(p => p.Name == "GetRelatedRoutingCustomers");
            IEdmEntitySet expectedSet = _model.EntityContainers().First().EntitySets().SingleOrDefault(e => e.Name == "RoutingCustomers");
            IEdmType expectedType = expectedEdmElement.ReturnType.Definition;

            // Act
            ODataPath path = _parser.Parse(_model, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, path.EntitySet);
            Assert.Same(expectedType, path.EdmType);
            ActionPathSegment action = Assert.IsType<ActionPathSegment>(segment);
            Assert.Same(expectedEdmElement, action.Action);
        }

        [Fact]
        public void CanParseActionBoundToCollectionSegment()
        {
            // Arrange
            string odataPath = "RoutingCustomers/System.Web.Http.OData.Routing.VIP/GetMostProfitable";
            string expectedText = "Default.Container.GetMostProfitable";
            IEdmFunctionImport expectedEdmElement = _model.EntityContainers().First().FunctionImports().SingleOrDefault(p => p.Name == "GetMostProfitable");
            IEdmEntitySet expectedSet = _model.EntityContainers().First().EntitySets().SingleOrDefault(e => e.Name == "RoutingCustomers");
            IEdmType expectedType = expectedEdmElement.ReturnType.Definition;

            // Act
            ODataPath path = _parser.Parse(_model, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedText, segment.ToString());
            Assert.Same(expectedSet, path.EntitySet);
            Assert.Same(expectedType, path.EdmType);
            ActionPathSegment action = Assert.IsType<ActionPathSegment>(segment);
            Assert.Same(expectedEdmElement, action.Action);
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
            var expectedSet = model.FindDeclaredEntityContainer("Container").FindEntitySet(expectedSetName);
            var expectedType = model.FindDeclaredType("System.Web.Http.OData.Routing." + expectedTypeName) as IEdmEntityType;

            // Act
            ODataPath path = _parser.Parse(_model, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.NotNull(path.EntitySet);
            Assert.NotNull(path.EdmType);
            Assert.Same(expectedSet, path.EntitySet);
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
        public void CanResolveSetAndTypeViaNavigationPropertySegment(string odataPath, string expectedSetName, string expectedTypeName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("RoutingCustomers/System.Web.Http.OData.Routing.VIP", "VIP", "RoutingCustomers", true)]
        [InlineData("RoutingCustomers(1)/System.Web.Http.OData.Routing.VIP", "VIP", "RoutingCustomers", false)]
        [InlineData("Products(1)/System.Web.Http.OData.Routing.ImportantProduct", "ImportantProduct", "Products", false)]
        [InlineData("Products(1)/RoutingCustomers/System.Web.Http.OData.Routing.VIP", "VIP", "RoutingCustomers", true)]
        [InlineData("SalesPeople(1)/ManagedRoutingCustomers", "VIP", "RoutingCustomers", true)]
        [InlineData("RoutingCustomers(1)/System.Web.Http.OData.Routing.VIP/RelationshipManager", "SalesPerson", "SalesPeople", false)]
        [InlineData("Products/System.Web.Http.OData.Routing.ImportantProduct(1)/LeadSalesPerson", "SalesPerson", "SalesPeople", false)]
        [InlineData("Products(1)/RoutingCustomers/System.Web.Http.OData.Routing.VIP(1)/RelationshipManager/ManagedProducts", "ImportantProduct", "Products", true)]
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
        [InlineData("RoutingCustomers(1)/GetRelatedRoutingCustomers", "RoutingCustomer", "RoutingCustomers", true)]
        [InlineData("RoutingCustomers(1)/GetBestRelatedRoutingCustomer", "VIP", "RoutingCustomers", false)]
        [InlineData("RoutingCustomers(1)/System.Web.Http.OData.Routing.VIP/GetSalesPerson", "SalesPerson", "SalesPeople", false)]
        [InlineData("SalesPeople(1)/GetVIPRoutingCustomers", "VIP", "RoutingCustomers", true)]
        public void CanResolveSetAndTypeViaEntityActionSegment(string odataPath, string expectedTypeName, string expectedSetName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("RoutingCustomers/GetVIPs", "VIP", "RoutingCustomers", true)]
        [InlineData("RoutingCustomers/GetProducts", "Product", "Products", true)]
        [InlineData("Products(1)/RoutingCustomers/System.Web.Http.OData.Routing.VIP/GetSalesPeople", "SalesPerson", "SalesPeople", true)]
        [InlineData("SalesPeople/GetVIPRoutingCustomers", "VIP", "RoutingCustomers", true)]
        [InlineData("RoutingCustomers/System.Web.Http.OData.Routing.VIP/GetMostProfitable", "VIP", "RoutingCustomers", false)]
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
        public void CastTests(string path, string expectedEdmType)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            var vehicle = new EdmEntityType("NS", "Vehicle");
            var car = new EdmEntityType("NS", "Car", vehicle);
            var motorcycle = new EdmEntityType("NS", "Motorcycle", vehicle);
            model.AddElements(new IEdmSchemaElement[] { vehicle, car, motorcycle, container });

            container.AddEntitySet("Vehicles", vehicle);
            container.AddEntitySet("Cars", car);
            container.AddEntitySet("Motorcycles", motorcycle);

            // Act
            ODataPath odataPath = _parser.Parse(model, path);

            // Assert
            Assert.NotNull(odataPath);
            Assert.Equal(expectedEdmType, odataPath.EdmType.ToTraceString());
        }

        [Theory]
        [InlineData("Vehicles/NS.Car/NS.Motorcycle")]
        [InlineData("Vehicles/NS.Motorcycle/NS.Car")]
        [InlineData("Cars/NS.Motorcycle")]
        [InlineData("Motorcycles/NS.Car")]
        [InlineData("Vehicles(42)/NS.Car/NS.Motorcycle")]
        [InlineData("Vehicles(42)/NS.Motorcycle/NS.Car")]
        [InlineData("Cars(42)/NS.Motorcycle")]
        [InlineData("Motorcycles(42)/NS.Car")]
        public void Invalid_CastTests(string path)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            var vehicle = new EdmEntityType("NS", "Vehicle");
            var car = new EdmEntityType("NS", "Car", vehicle);
            var motorcycle = new EdmEntityType("NS", "Motorcycle", vehicle);
            model.AddElements(new IEdmSchemaElement[] { vehicle, car, motorcycle, container });

            container.AddEntitySet("Vehicles", vehicle);
            container.AddEntitySet("Cars", car);
            container.AddEntitySet("Motorcycles", motorcycle);

            // Act
            var exception = Assert.Throws<ODataException>(() => _parser.Parse(model, path));
            Assert.Contains("Invalid cast encountered.", exception.Message);
        }

        [Theory]
        [InlineData("Vehicles(42)/Wash", "Wash", "NS.Vehicle")]
        [InlineData("Vehicles(42)/NS.Car/Wash", "Wash", "NS.Car")] // upcast
        [InlineData("Vehicles(42)/NS.Motorcycle/Wash", "Wash", "NS.Vehicle")]
        [InlineData("Cars(42)/NS.Vehicle/Wash", "Wash", "NS.Vehicle")] // downcast
        [InlineData("Vehicles/WashMultiple", "WashMultiple", "Collection([NS.Vehicle Nullable=False])")]
        [InlineData("Vehicles/NS.Car/WashMultiple", "WashMultiple", "Collection([NS.Car Nullable=False])")] // upcast
        [InlineData("Vehicles/NS.Motorcycle/WashMultiple", "WashMultiple", "Collection([NS.Vehicle Nullable=False])")]
        [InlineData("Cars/NS.Vehicle/WashMultiple", "WashMultiple", "Collection([NS.Vehicle Nullable=False])")] // downcast
        public void ActionOverloadResoultionTests(string path, string actionName, string expectedEntityBound)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            var vehicle = new EdmEntityType("NS", "Vehicle");
            var car = new EdmEntityType("NS", "Car", vehicle);
            var motorcycle = new EdmEntityType("NS", "Motorcycle", vehicle);
            model.AddElements(new IEdmSchemaElement[] { vehicle, car, motorcycle, container });

            var washVehicle = AddBindableAction(container, "Wash", vehicle, isCollection: false);
            var washCar = AddBindableAction(container, "Wash", car, isCollection: false);
            var washVehicles = AddBindableAction(container, "WashMultiple", vehicle, isCollection: true);
            var washCars = AddBindableAction(container, "WashMultiple", car, isCollection: true);

            container.AddEntitySet("Vehicles", vehicle);
            container.AddEntitySet("Cars", car);
            container.AddEntitySet("Motorcycles", motorcycle);

            // Act
            ODataPath odataPath = _parser.Parse(model, path);

            // Assert
            Assert.NotNull(odataPath);
            ActionPathSegment actionSegment = Assert.IsType<ActionPathSegment>(odataPath.Segments.Last());
            Assert.Equal("NS.Container." + actionName, actionSegment.ActionName);
            Assert.Equal(expectedEntityBound, actionSegment.Action.Parameters.First().Type.Definition.ToTraceString());
        }

        private static IEdmFunctionImport AddBindableAction(EdmEntityContainer container, string name, IEdmEntityType bindingType, bool isCollection)
        {
            var action = container.AddFunctionImport(name, returnType: null, entitySet: null, sideEffecting: true, composable: false, bindable: true);

            IEdmTypeReference bindingParamterType = new EdmEntityTypeReference(bindingType, isNullable: false);
            if (isCollection)
            {
                bindingParamterType = new EdmCollectionTypeReference(
                    new EdmCollectionType(bindingParamterType), isNullable: false);
            }

            action.AddParameter("bindingParameter", bindingParamterType);
            return action;
        }

        private static void AssertTypeMatchesExpectedType(string odataPath, string expectedSetName, string expectedTypeName, bool isCollection)
        {
            // Arrange
            var expectedSet = _model.FindDeclaredEntityContainer("Container").FindEntitySet(expectedSetName);
            var expectedType = _model.FindDeclaredType("System.Web.Http.OData.Routing." + expectedTypeName) as IEdmEntityType;

            // Act
            ODataPath path = _parser.Parse(_model, odataPath);
            ODataPathSegment segment = path.Segments.Last();

            // Assert
            Assert.NotNull(segment);
            Assert.NotNull(path.EntitySet);
            Assert.NotNull(path.EdmType);
            Assert.Same(expectedSet, path.EntitySet);
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
    }
}
