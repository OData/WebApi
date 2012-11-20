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
    public class DefaultODataPathHandlerTest
    {
        private static DefaultODataPathHandler _parser = new DefaultODataPathHandler(GetModel());

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
        public void Parse_WorksOnEncodedCharacters()
        {
            string odataPath = "üCategories";

            ODataPath path = _parser.Parse(odataPath);
            ODataPathSegment segment = path.Segments.Last();

            Assert.NotNull(path);
            Assert.Equal("~/entityset", path.PathTemplate);
            Assert.Equal("üCategories", segment.ToString());
        }

        [Fact]
        public void Parse_ForInvalidCast_ThrowsODataException()
        {
            string odataPath = "Customers/System.Web.Http.OData.Routing.Product";

            Assert.Throws<ODataException>(
                () => _parser.Parse(odataPath),
                "Invalid cast encountered. Cast type 'System.Web.Http.OData.Routing.Product' must be the same as or derive from the previous segment's type 'System.Web.Http.OData.Routing.Customer'.");
        }

        [Fact]
        public void Parse_ForSegmentAfterMetadata_ThrowsODataException()
        {
            string odataPath = "$metadata/foo";

            Assert.Throws<ODataException>(
                () => _parser.Parse(odataPath),
                "The URI segment 'foo' is invalid after the segment '$metadata'.");
        }

        [Theory]
        [InlineData("", "~")]
        [InlineData("$metadata", "~/$metadata")]
        [InlineData("$batch", "~/$batch")]
        [InlineData("Customers(112)", "~/entityset/key")]
        [InlineData("Customers/System.Web.Http.OData.Routing.VIP", "~/entityset/cast")]
        [InlineData("Customers(100)/Products", "~/entityset/key/navigation")]
        [InlineData("Customers(100)/System.Web.Http.OData.Routing.VIP/RelationshipManager", "~/entityset/key/cast/navigation")]
        [InlineData("GetCustomerById()", "~/action")]
        [InlineData("Customers(112)/Address/Street", "~/entityset/key/property/property")]
        [InlineData("Customers(1)/Name/$value", "~/entityset/key/property/$value")]
        [InlineData("Customers(1)/$links/Products", "~/entityset/key/$links/navigation")]
        [InlineData("Customers(112)/GetRelatedCustomers", "~/entityset/key/action")]
        [InlineData("Customers/System.Web.Http.OData.Routing.VIP/GetMostProfitable", "~/entityset/cast/action")]
        [InlineData("Products(1)/Customers/System.Web.Http.OData.Routing.VIP(1)/RelationshipManager/ManagedProducts", "~/entityset/key/navigation/cast/key/navigation/navigation")]
        public void Parse_ReturnsPath_WithCorrectTemplate(string odataPath, string template)
        {
            ODataPath path = _parser.Parse(odataPath);

            Assert.NotNull(path);
            Assert.Equal(template, path.PathTemplate);
        }

        [Fact]
        public void CanParseUrlWithNoModelElements()
        {
            // Arrange
            string odataPath = "1/2()/3/4()/5";

            // Act
            ODataPath path = _parser.Parse(odataPath);

            // Assert
            Assert.Null(path);
        }

        [Fact]
        public void CanParseMetadataUrl()
        {
            string odataPath = "$metadata";

            ODataPath path = _parser.Parse(odataPath);
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
            ODataPath path = _parser.Parse(odataPath);
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
            string odataPath = "Customers";
            string expectedText = "Customers";
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "Customers");

            // Act
            ODataPath path = _parser.Parse(odataPath);
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
            string odataPath = "Customers(112)";
            string expectedText = "112";
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "Customers");

            // Act
            ODataPath path = _parser.Parse(odataPath);
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
            string odataPath = "Customers/System.Web.Http.OData.Routing.VIP";
            string expectedText = "System.Web.Http.OData.Routing.VIP";
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "Customers");
            IEdmEntityType expectedType = _parser.Model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "VIP");

            // Act
            ODataPath path = _parser.Parse(odataPath);
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
            string odataPath = "Customers(100)/System.Web.Http.OData.Routing.VIP";
            string expectedText = "System.Web.Http.OData.Routing.VIP";
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "Customers");
            IEdmEntityType expectedType = _parser.Model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "VIP");

            // Act
            ODataPath path = _parser.Parse(odataPath);
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
            string odataPath = "Customers(100)/Products";
            string expectedText = "Products";
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "Products");
            IEdmNavigationProperty expectedEdmElement = _parser.Model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "Customer").NavigationProperties().SingleOrDefault(n => n.Name == "Products");

            // Act
            ODataPath path = _parser.Parse(odataPath);
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
            string odataPath = "Customers(100)/System.Web.Http.OData.Routing.VIP/RelationshipManager";
            string expectedText = "RelationshipManager";
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "SalesPeople");
            IEdmNavigationProperty expectedEdmElement = _parser.Model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(s => s.Name == "VIP").NavigationProperties().SingleOrDefault(n => n.Name == "RelationshipManager");

            // Act
            ODataPath path = _parser.Parse(odataPath);
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
            string odataPath = "GetCustomerById()";
            string expectedText = "Default.Container.GetCustomerById";
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "Customers");
            IEdmFunctionImport expectedEdmElement = _parser.Model.EntityContainers().First().FunctionImports().SingleOrDefault(s => s.Name == "GetCustomerById");

            // Act
            ODataPath path = _parser.Parse(odataPath);
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
            string odataPath = "Customers(112)/Name";
            string expectedText = "Name";
            IEdmProperty expectedEdmElement = _parser.Model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(e => e.Name == "Customer").Properties().SingleOrDefault(p => p.Name == "Name");
            IEdmType expectedType = expectedEdmElement.Type.Definition;

            // Act
            ODataPath path = _parser.Parse(odataPath);
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
            string odataPath = "Customers(112)/Address";
            string expectedText = "Address";
            IEdmProperty expectedEdmElement = _parser.Model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(e => e.Name == "Customer").Properties().SingleOrDefault(p => p.Name == "Address");
            IEdmType expectedType = expectedEdmElement.Type.Definition;

            // Act
            ODataPath path = _parser.Parse(odataPath);
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
            string odataPath = "Customers(112)/Address/Street";
            string expectedText = "Street";
            IEdmProperty expectedEdmElement = _parser.Model.SchemaElements.OfType<IEdmComplexType>().SingleOrDefault(e => e.Name == "Address").Properties().SingleOrDefault(p => p.Name == "Street");
            IEdmType expectedType = expectedEdmElement.Type.Definition;

            // Act
            ODataPath path = _parser.Parse(odataPath);
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
            string odataPath = "Customers(1)/Name/$value";

            // Act
            ODataPath path = _parser.Parse(odataPath);
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
            string odataPath = "Customers(1)/$links/Products";
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(s => s.Name == "Products");
            IEdmEntityType expectedType = expectedSet.ElementType;

            // Act
            ODataPath path = _parser.Parse(odataPath);
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
            string odataPath = "Customers(112)/GetRelatedCustomers";
            string expectedText = "Default.Container.GetRelatedCustomers";
            IEdmFunctionImport expectedEdmElement = _parser.Model.EntityContainers().First().FunctionImports().SingleOrDefault(p => p.Name == "GetRelatedCustomers");
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(e => e.Name == "Customers");
            IEdmType expectedType = expectedEdmElement.ReturnType.Definition;

            // Act
            ODataPath path = _parser.Parse(odataPath);
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
            string odataPath = "Customers/System.Web.Http.OData.Routing.VIP/GetMostProfitable";
            string expectedText = "Default.Container.GetMostProfitable";
            IEdmFunctionImport expectedEdmElement = _parser.Model.EntityContainers().First().FunctionImports().SingleOrDefault(p => p.Name == "GetMostProfitable");
            IEdmEntitySet expectedSet = _parser.Model.EntityContainers().First().EntitySets().SingleOrDefault(e => e.Name == "Customers");
            IEdmType expectedType = expectedEdmElement.ReturnType.Definition;

            // Act
            ODataPath path = _parser.Parse(odataPath);
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
        [InlineData("Customers", "Customers", "Customer", true)]
        [InlineData("Customers/", "Customers", "Customer", true)]
        [InlineData("Products", "Products", "Product", true)]
        [InlineData("Products/", "Products", "Product", true)]
        [InlineData("SalesPeople", "SalesPeople", "SalesPerson", true)]
        public void CanResolveSetAndTypeViaSimpleEntitySetSegment(string odataPath, string expectedSetName, string expectedTypeName, bool isCollection)
        {
            // Arrange
            var model = _parser.Model;
            var expectedSet = model.FindDeclaredEntityContainer("Container").FindEntitySet(expectedSetName);
            var expectedType = model.FindDeclaredType("System.Web.Http.OData.Routing." + expectedTypeName) as IEdmEntityType;

            // Act
            ODataPath path = _parser.Parse(odataPath);
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
        [InlineData("Customers(1)", "Customers", "Customer", false)]
        [InlineData("Customers(1)/", "Customers", "Customer", false)]
        [InlineData("Products(1)", "Products", "Product", false)]
        [InlineData("Products(1)/", "Products", "Product", false)]
        [InlineData("Products(1)", "Products", "Product", false)]
        public void CanResolveSetAndTypeViaKeySegment(string odataPath, string expectedSetName, string expectedTypeName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("Customers(1)/Products", "Products", "Product", true)]
        [InlineData("Customers(1)/Products(1)", "Products", "Product", false)]
        [InlineData("Customers(1)/Products/", "Products", "Product", true)]
        [InlineData("Customers(1)/Products", "Products", "Product", true)]
        [InlineData("Products(1)/Customers", "Customers", "Customer", true)]
        [InlineData("Products(1)/Customers(1)", "Customers", "Customer", false)]
        [InlineData("Products(1)/Customers/", "Customers", "Customer", true)]
        public void CanResolveSetAndTypeViaNavigationPropertySegment(string odataPath, string expectedSetName, string expectedTypeName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("Customers/System.Web.Http.OData.Routing.VIP", "VIP", "Customers", true)]
        [InlineData("Customers(1)/System.Web.Http.OData.Routing.VIP", "VIP", "Customers", false)]
        [InlineData("Products(1)/System.Web.Http.OData.Routing.ImportantProduct", "ImportantProduct", "Products", false)]
        [InlineData("Products(1)/Customers/System.Web.Http.OData.Routing.VIP", "VIP", "Customers", true)]
        [InlineData("SalesPeople(1)/ManagedCustomers", "VIP", "Customers", true)]
        [InlineData("Customers(1)/System.Web.Http.OData.Routing.VIP/RelationshipManager", "SalesPerson", "SalesPeople", false)]
        [InlineData("Products/System.Web.Http.OData.Routing.ImportantProduct(1)/LeadSalesPerson", "SalesPerson", "SalesPeople", false)]
        [InlineData("Products(1)/Customers/System.Web.Http.OData.Routing.VIP(1)/RelationshipManager/ManagedProducts", "ImportantProduct", "Products", true)]
        public void CanResolveSetAndTypeViaCastSegment(string odataPath, string expectedTypeName, string expectedSetName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("GetCustomerById", "Customer", "Customers", false)]
        [InlineData("GetSalesPersonById", "SalesPerson", "SalesPeople", false)]
        [InlineData("GetAllVIPs", "VIP", "Customers", true)]
        public void CanResolveSetAndTypeViaRootProcedureSegment(string odataPath, string expectedTypeName, string expectedSetName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("Customers(1)/GetRelatedCustomers", "Customer", "Customers", true)]
        [InlineData("Customers(1)/GetBestRelatedCustomer", "VIP", "Customers", false)]
        [InlineData("Customers(1)/System.Web.Http.OData.Routing.VIP/GetSalesPerson", "SalesPerson", "SalesPeople", false)]
        [InlineData("SalesPeople(1)/GetVIPCustomers", "VIP", "Customers", true)]
        public void CanResolveSetAndTypeViaEntityActionSegment(string odataPath, string expectedTypeName, string expectedSetName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        [Theory]
        [InlineData("Customers/GetVIPs", "VIP", "Customers", true)]
        [InlineData("Customers/GetProducts", "Product", "Products", true)]
        [InlineData("Products(1)/Customers/System.Web.Http.OData.Routing.VIP/GetSalesPeople", "SalesPerson", "SalesPeople", true)]
        [InlineData("SalesPeople/GetVIPCustomers", "VIP", "Customers", true)]
        [InlineData("Customers/System.Web.Http.OData.Routing.VIP/GetMostProfitable", "VIP", "Customers", false)]
        public void CanResolveSetAndTypeViaCollectionActionSegment(string odataPath, string expectedTypeName, string expectedSetName, bool isCollection)
        {
            AssertTypeMatchesExpectedType(odataPath, expectedSetName, expectedTypeName, isCollection);
        }

        private static void AssertTypeMatchesExpectedType(string odataPath, string expectedSetName, string expectedTypeName, bool isCollection)
        {
            // Arrange
            var model = _parser.Model;
            var expectedSet = model.FindDeclaredEntityContainer("Container").FindEntitySet(expectedSetName);
            var expectedType = model.FindDeclaredType("System.Web.Http.OData.Routing." + expectedTypeName) as IEdmEntityType;

            // Act
            ODataPath path = _parser.Parse(odataPath);
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
