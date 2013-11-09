// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Routing;
using System.Web.Http.TestCommon;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataEntityDeserializerTests
    {
        private readonly IEdmModel _edmModel;
        private readonly ODataDeserializerContext _readContext;
        private readonly IEdmEntityTypeReference _productEdmType;
        private readonly IEdmEntityTypeReference _supplierEdmType;
        private readonly ODataDeserializerProvider _deserializerProvider;

        public ODataEntityDeserializerTests()
        {
            _edmModel = EdmTestHelpers.GetModel();
            IEdmEntitySet entitySet = _edmModel.EntityContainers().Single().FindEntitySet("Products");
            _readContext = new ODataDeserializerContext
            {
                Path = new ODataPath(new EntitySetPathSegment(entitySet)),
                Model = _edmModel,
                ResourceType = typeof(Product)
            };
            _productEdmType = _edmModel.GetEdmTypeReference(typeof(Product)).AsEntity();
            _supplierEdmType = _edmModel.GetEdmTypeReference(typeof(Supplier)).AsEntity();
            _deserializerProvider = new DefaultODataDeserializerProvider();
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_DeserializerProvider()
        {
            Assert.ThrowsArgumentNull(() => new ODataEntityDeserializer(deserializerProvider: null), "deserializerProvider");
        }

        [Fact]
        public void Read_ThrowsArgumentNull_MessageReader()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            Assert.ThrowsArgumentNull(
                () => deserializer.Read(messageReader: null, type: typeof(Product), readContext: _readContext),
                "messageReader");
        }

        [Fact]
        public void Read_ThrowsArgumentNull_ReadContext()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            Assert.ThrowsArgumentNull(
                () => deserializer.Read(messageReader: ODataTestUtil.GetMockODataMessageReader(), type: typeof(Product), readContext: null),
                "readContext");
        }

        [Fact]
        public void Read_ThrowsArgument_ODataPathMissing()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            Assert.ThrowsArgument(
                () => deserializer.Read(ODataTestUtil.GetMockODataMessageReader(), typeof(Product), new ODataDeserializerContext()),
                "readContext",
                "The operation cannot be completed because no ODataPath is available for the request.");
        }

        [Fact]
        public void Read_ThrowsArgument_EntitysetMissing()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            Assert.Throws<SerializationException>(
                () => deserializer.Read(ODataTestUtil.GetMockODataMessageReader(), typeof(Product), new ODataDeserializerContext { Path = new ODataPath() }),
                "The related entity set could not be found from the OData path. The related entity set is required to deserialize the payload.");
        }

        [Fact]
        public void ReadInline_ThrowsArgumentNull_Item()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            Assert.ThrowsArgumentNull(
                () => deserializer.ReadInline(item: null, edmType: _productEdmType, readContext: new ODataDeserializerContext()),
                "item");
        }

        [Fact]
        public void ReadInline_Throws_ArgumentMustBeOfType()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            Assert.ThrowsArgument(
                () => deserializer.ReadInline(item: 42, edmType: _productEdmType, readContext: new ODataDeserializerContext()),
                "item",
                "The argument must be of type 'ODataEntry'");
        }

        [Fact]
        public void ReadInline_Calls_ReadEntry()
        {
            // Arrange
            var deserializer = new Mock<ODataEntityDeserializer>(_deserializerProvider);
            ODataEntryWithNavigationLinks entry = new ODataEntryWithNavigationLinks(new ODataEntry());
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            deserializer.CallBase = true;
            deserializer.Setup(d => d.ReadEntry(entry, _productEdmType, readContext)).Returns(42).Verifiable();

            // Act
            var result = deserializer.Object.ReadInline(entry, _productEdmType, readContext);

            // Assert
            deserializer.Verify();
            Assert.Equal(42, result);
        }

        [Fact]
        public void ReadEntry_ThrowsArgumentNull_EntryWrapper()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            Assert.ThrowsArgumentNull(
                () => deserializer.ReadEntry(entryWrapper: null, entityType: _productEdmType, readContext: _readContext),
                "entryWrapper");
        }

        [Fact]
        public void ReadEntry_ThrowsArgumentNull_ReadContext()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            ODataEntryWithNavigationLinks entry = new ODataEntryWithNavigationLinks(new ODataEntry());
            Assert.ThrowsArgumentNull(
                () => deserializer.ReadEntry(entry, entityType: _productEdmType, readContext: null),
                "readContext");
        }

        [Fact]
        public void ReadEntry_ThrowsArgument_ModelMissingFromReadContext()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            ODataEntryWithNavigationLinks entry = new ODataEntryWithNavigationLinks(new ODataEntry { TypeName = _supplierEdmType.FullName() });

            Assert.ThrowsArgument(
                () => deserializer.ReadEntry(entry, _productEdmType, new ODataDeserializerContext()),
                "readContext",
                "The EDM model is missing on the read context. The model is required on the read context to deserialize the payload.");
        }

        [Fact]
        public void ReadEntry_ThrowsODataException_EntityTypeNotInModel()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            ODataEntryWithNavigationLinks entry = new ODataEntryWithNavigationLinks(new ODataEntry { TypeName = "MissingType" });

            Assert.Throws<ODataException>(
                () => deserializer.ReadEntry(entry, _productEdmType, _readContext),
                "Cannot find the entity type 'MissingType' in the model.");
        }

        [Fact]
        public void ReadEntry_ThrowsODataException_CannotInstantiateAbstractEntityType()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.Entity<BaseType>().Abstract();
            IEdmModel model = builder.GetEdmModel();
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            ODataEntryWithNavigationLinks entry = new ODataEntryWithNavigationLinks(new ODataEntry { TypeName = "System.Web.Http.OData.Formatter.Deserialization.BaseType" });

            Assert.Throws<ODataException>(
                () => deserializer.ReadEntry(entry, _productEdmType, new ODataDeserializerContext { Model = model }),
                "An instance of the abstract entity type 'System.Web.Http.OData.Formatter.Deserialization.BaseType' was found. Abstract entity types cannot be instantiated.");
        }

        [Fact]
        public void ReadEntry_ThrowsSerializationException_TypeCannotBeDeserialized()
        {
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            deserializerProvider.Setup(d => d.GetEdmTypeDeserializer(It.IsAny<IEdmTypeReference>())).Returns<ODataEdmTypeDeserializer>(null);
            var deserializer = new ODataEntityDeserializer(deserializerProvider.Object);
            ODataEntryWithNavigationLinks entry = new ODataEntryWithNavigationLinks(new ODataEntry { TypeName = _supplierEdmType.FullName() });

            Assert.Throws<SerializationException>(
                () => deserializer.ReadEntry(entry, _productEdmType, _readContext),
                "'ODataDemo.Supplier' cannot be deserialized using the ODataMediaTypeFormatter.");
        }

        [Fact]
        public void ReadEntry_DispatchesToRightDeserializer_IfEntityTypeNameIsDifferent()
        {
            // Arrange
            Mock<ODataEdmTypeDeserializer> supplierDeserializer = new Mock<ODataEdmTypeDeserializer>(ODataPayloadKind.Entry);
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            var deserializer = new ODataEntityDeserializer(deserializerProvider.Object);
            ODataEntryWithNavigationLinks entry = new ODataEntryWithNavigationLinks(new ODataEntry { TypeName = _supplierEdmType.FullName() });

            deserializerProvider.Setup(d => d.GetEdmTypeDeserializer(It.IsAny<IEdmTypeReference>())).Returns(supplierDeserializer.Object);
            supplierDeserializer
                .Setup(d => d.ReadInline(entry, It.Is<IEdmTypeReference>(e => _supplierEdmType.Definition == e.Definition), _readContext))
                .Returns(42).Verifiable();

            // Act
            object result = deserializer.ReadEntry(entry, _productEdmType, _readContext);

            // Assert
            supplierDeserializer.Verify();
            Assert.Equal(42, result);
        }

        [Fact]
        public void ReadEntry_SetsExpectedAndActualEdmType_OnCreatedEdmObject_TypelessMode()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            IEdmEntityTypeReference customerType = EdmLibHelpers.ToEdmTypeReference(model.Customer, isNullable: false).AsEntity();
            ODataDeserializerContext readContext = new ODataDeserializerContext { Model = model.Model, ResourceType = typeof(IEdmObject) };
            ODataEntryWithNavigationLinks entry = new ODataEntryWithNavigationLinks(new ODataEntry
            {
                TypeName = model.SpecialCustomer.FullName(),
                Properties = new ODataProperty[0]
            });

            ODataEntityDeserializer deserializer = new ODataEntityDeserializer(_deserializerProvider);

            // Act
            var result = deserializer.ReadEntry(entry, customerType, readContext);

            // Assert
            EdmEntityObject resource = Assert.IsType<EdmEntityObject>(result);
            Assert.Equal(model.SpecialCustomer, resource.ActualEdmType);
            Assert.Equal(model.Customer, resource.ExpectedEdmType);
        }

        [Fact]
        public void ReadEntry_Calls_CreateEntityResource()
        {
            // Arrange
            Mock<ODataEntityDeserializer> deserializer = new Mock<ODataEntityDeserializer>(_deserializerProvider);
            ODataEntryWithNavigationLinks entry = new ODataEntryWithNavigationLinks(new ODataEntry { Properties = Enumerable.Empty<ODataProperty>() });
            deserializer.CallBase = true;
            deserializer.Setup(d => d.CreateEntityResource(_productEdmType, _readContext)).Returns(42).Verifiable();

            // Act
            var result = deserializer.Object.ReadEntry(entry, _productEdmType, _readContext);

            // Assert
            Assert.Equal(42, result);
            deserializer.Verify();
        }

        [Fact]
        public void ReadEntry_Calls_ApplyStructuralProperties()
        {
            // Arrange
            Mock<ODataEntityDeserializer> deserializer = new Mock<ODataEntityDeserializer>(_deserializerProvider);
            ODataEntryWithNavigationLinks entry = new ODataEntryWithNavigationLinks(new ODataEntry { Properties = Enumerable.Empty<ODataProperty>() });
            deserializer.CallBase = true;
            deserializer.Setup(d => d.CreateEntityResource(_productEdmType, _readContext)).Returns(42);
            deserializer.Setup(d => d.ApplyStructuralProperties(42, entry, _productEdmType, _readContext)).Verifiable();

            // Act
            deserializer.Object.ReadEntry(entry, _productEdmType, _readContext);

            // Assert
            deserializer.Verify();
        }

        [Fact]
        public void ReadEntry_Calls_ApplyNaviagationProperties()
        {
            // Arrange
            Mock<ODataEntityDeserializer> deserializer = new Mock<ODataEntityDeserializer>(_deserializerProvider);
            ODataEntryWithNavigationLinks entry = new ODataEntryWithNavigationLinks(new ODataEntry { Properties = Enumerable.Empty<ODataProperty>() });
            deserializer.CallBase = true;
            deserializer.Setup(d => d.CreateEntityResource(_productEdmType, _readContext)).Returns(42);
            deserializer.Setup(d => d.ApplyNavigationProperties(42, entry, _productEdmType, _readContext)).Verifiable();

            // Act
            deserializer.Object.ReadEntry(entry, _productEdmType, _readContext);

            // Assert
            deserializer.Verify();
        }

        [Fact]
        public void CreateEntityResource_ThrowsArgumentNull_ReadContext()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            Assert.ThrowsArgumentNull(
                () => deserializer.CreateEntityResource(_productEdmType, readContext: null),
                "readContext");
        }

        [Fact]
        public void CreateEntityResource_ThrowsArgument_ModelMissingFromReadContext()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            Assert.ThrowsArgument(
                () => deserializer.CreateEntityResource(_productEdmType, new ODataDeserializerContext()),
                "readContext",
                "The EDM model is missing on the read context. The model is required on the read context to deserialize the payload.");
        }

        [Fact]
        public void CreateEntityResource_ThrowsODataException_MappingDoesNotContainEntityType()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            Assert.Throws<ODataException>(
                () => deserializer.CreateEntityResource(_productEdmType, new ODataDeserializerContext { Model = EdmCoreModel.Instance }),
                "The provided mapping doesn't contain an entry for the entity type 'ODataDemo.Product'.");
        }

        [Fact]
        public void CreateEntityResource_CreatesDeltaOfT_IfPatchMode()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _readContext.Model,
                ResourceType = typeof(Delta<Product>)
            };

            Assert.IsType<Delta<Product>>(deserializer.CreateEntityResource(_productEdmType, readContext));
        }

        [Fact]
        public void CreateEntityResource_CreatesDeltaWith_ExpectedUpdatableProperties()
        {
            // Arrange
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _readContext.Model,
                ResourceType = typeof(Delta<Product>)
            };
            var structuralProperties = _productEdmType.StructuralProperties().Select(p => p.Name);

            // Act
            Delta<Product> resource = deserializer.CreateEntityResource(_productEdmType, readContext) as Delta<Product>;

            // Assert
            Assert.NotNull(resource);
            Assert.Equal(structuralProperties, resource.GetUnchangedPropertyNames());
        }

        [Fact]
        public void CreateEntityResource_CreatesEdmEntityObject_IfTypeLessMode()
        {
            // Arrange
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _readContext.Model,
                ResourceType = typeof(IEdmObject)
            };

            // Act
            var result = deserializer.CreateEntityResource(_productEdmType, readContext);

            // Assert
            EdmEntityObject resource = Assert.IsType<EdmEntityObject>(result);
            Assert.Equal(_productEdmType, resource.GetEdmType(), new EdmTypeReferenceEqualityComparer());
        }

        [Fact]
        public void CreateEntityResource_CreatesT_IfNotPatchMode()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _readContext.Model,
                ResourceType = typeof(Product)
            };

            Assert.IsType<Product>(deserializer.CreateEntityResource(_productEdmType, readContext));
        }

        [Fact]
        public void ApplyNavigationProperties_ThrowsArgumentNull_EntryWrapper()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            Assert.ThrowsArgumentNull(
                () => deserializer.ApplyNavigationProperties(42, entryWrapper: null, entityType: _productEdmType, readContext: _readContext),
                "entryWrapper");
        }

        [Fact]
        public void ApplyNavigationProperties_Calls_ApplyNavigationPropertyForEachNavigationLink()
        {
            // Arrange
            ODataEntryWithNavigationLinks entry = new ODataEntryWithNavigationLinks(new ODataEntry());
            entry.NavigationLinks.Add(new ODataNavigationLinkWithItems(new ODataNavigationLink()));
            entry.NavigationLinks.Add(new ODataNavigationLinkWithItems(new ODataNavigationLink()));

            Mock<ODataEntityDeserializer> deserializer = new Mock<ODataEntityDeserializer>(_deserializerProvider);
            deserializer.CallBase = true;
            deserializer.Setup(d => d.ApplyNavigationProperty(42, entry.NavigationLinks[0], _productEdmType, _readContext)).Verifiable();
            deserializer.Setup(d => d.ApplyNavigationProperty(42, entry.NavigationLinks[1], _productEdmType, _readContext)).Verifiable();

            // Act
            deserializer.Object.ApplyNavigationProperties(42, entry, _productEdmType, _readContext);

            // Assert
            deserializer.Verify();
        }

        [Fact]
        public void ApplyNavigationProperty_ThrowsArgumentNull_NavigationLinkWrapper()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            Assert.ThrowsArgumentNull(
                () => deserializer.ApplyNavigationProperty(42, navigationLinkWrapper: null, entityType: _productEdmType,
                    readContext: _readContext),
                "navigationLinkWrapper");
        }

        [Fact]
        public void ApplyNavigationProperty_ThrowsArgumentNull_EntityResource()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            ODataNavigationLinkWithItems navigationLink = new ODataNavigationLinkWithItems(new ODataNavigationLink());
            Assert.ThrowsArgumentNull(
                () => deserializer.ApplyNavigationProperty(entityResource: null, navigationLinkWrapper: navigationLink,
                    entityType: _productEdmType, readContext: _readContext),
                "entityResource");
        }

        [Fact]
        public void ApplyNavigationProperty_ThrowsODataException_NavigationPropertyNotfound()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            ODataNavigationLinkWithItems navigationLink = new ODataNavigationLinkWithItems(new ODataNavigationLink { Name = "SomeProperty" });

            Assert.Throws<ODataException>(
                () => deserializer.ApplyNavigationProperty(42, navigationLink, _productEdmType, _readContext),
                "Cannot find navigation property 'SomeProperty' on the entity type 'ODataDemo.Product'.");
        }

        [Fact]
        public void ApplyNavigationProperty_ThrowsODataException_WhenPatchingNavigationProperty()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            ODataNavigationLinkWithItems navigationLink = new ODataNavigationLinkWithItems(new ODataNavigationLink { Name = "Supplier" });
            navigationLink.NestedItems.Add(new ODataEntryWithNavigationLinks(new ODataEntry()));
            _readContext.ResourceType = typeof(Delta<Supplier>);

            Assert.Throws<ODataException>(
                () => deserializer.ApplyNavigationProperty(42, navigationLink, _productEdmType, _readContext),
                "Cannot apply PATCH to navigation property 'Supplier' on entity type 'ODataDemo.Product'.");
        }

        [Fact]
        public void ApplyNavigationProperty_ThrowsODataException_WhenPatchingCollectionNavigationProperty()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            ODataNavigationLinkWithItems navigationLink = new ODataNavigationLinkWithItems(new ODataNavigationLink { Name = "Products" });
            navigationLink.NestedItems.Add(new ODataFeedWithEntries(new ODataFeed()));
            _readContext.ResourceType = typeof(Delta<Supplier>);

            Assert.Throws<ODataException>(
                () => deserializer.ApplyNavigationProperty(42, navigationLink, _supplierEdmType, _readContext),
                "Cannot apply PATCH to navigation property 'Products' on entity type 'ODataDemo.Supplier'.");
        }

        [Fact]
        public void ApplyNavigationProperty_Calls_ReadInlineOnFeed()
        {
            // Arrange
            IEdmCollectionTypeReference productsType = new EdmCollectionTypeReference(new EdmCollectionType(_productEdmType), isNullable: false);
            Mock<ODataEdmTypeDeserializer> productsDeserializer = new Mock<ODataEdmTypeDeserializer>(ODataPayloadKind.Feed);
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            var deserializer = new ODataEntityDeserializer(deserializerProvider.Object);
            ODataNavigationLinkWithItems navigationLink = new ODataNavigationLinkWithItems(new ODataNavigationLink { Name = "Products" });
            navigationLink.NestedItems.Add(new ODataFeedWithEntries(new ODataFeed()));

            Supplier supplier = new Supplier();
            IEnumerable products = new[] { new Product { ID = 42 } };

            deserializerProvider.Setup(d => d.GetEdmTypeDeserializer(It.IsAny<IEdmTypeReference>())).Returns(productsDeserializer.Object);
            productsDeserializer
                .Setup(d => d.ReadInline(navigationLink.NestedItems[0], _supplierEdmType.FindNavigationProperty("Products").Type, _readContext))
                .Returns(products).Verifiable();

            // Act
            deserializer.ApplyNavigationProperty(supplier, navigationLink, _supplierEdmType, _readContext);

            // Assert
            productsDeserializer.Verify();
            Assert.Equal(1, supplier.Products.Count());
            Assert.Equal(42, supplier.Products.First().ID);
        }

        [Fact]
        public void ApplyNavigationProperty_Calls_ReadInlineOnEntry()
        {
            // Arrange
            Mock<ODataEdmTypeDeserializer> supplierDeserializer = new Mock<ODataEdmTypeDeserializer>(ODataPayloadKind.Feed);
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            var deserializer = new ODataEntityDeserializer(deserializerProvider.Object);
            ODataNavigationLinkWithItems navigationLink = new ODataNavigationLinkWithItems(new ODataNavigationLink { Name = "Supplier" });
            navigationLink.NestedItems.Add(new ODataEntryWithNavigationLinks(new ODataEntry()));

            Product product = new Product();
            Supplier supplier = new Supplier { ID = 42 };

            deserializerProvider.Setup(d => d.GetEdmTypeDeserializer(It.IsAny<IEdmTypeReference>())).Returns(supplierDeserializer.Object);
            supplierDeserializer
                .Setup(d => d.ReadInline(navigationLink.NestedItems[0], _productEdmType.FindNavigationProperty("Supplier").Type, _readContext))
                .Returns(supplier).Verifiable();

            // Act
            deserializer.ApplyNavigationProperty(product, navigationLink, _productEdmType, _readContext);

            // Assert
            supplierDeserializer.Verify();
            Assert.Equal(supplier, product.Supplier);
        }

        [Fact]
        public void ApplyStructuralProperties_ThrowsArgumentNull_entryWrapper()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            Assert.ThrowsArgumentNull(
                () => deserializer.ApplyStructuralProperties(42, entryWrapper: null, entityType: _productEdmType, readContext: _readContext),
                "entryWrapper");
        }

        [Fact]
        public void ApplyStructuralProperties_Calls_ApplyStructuralPropertyOnEachPropertyInEntry()
        {
            // Arrange
            var deserializer = new Mock<ODataEntityDeserializer>(_deserializerProvider);
            ODataProperty[] properties = new[] { new ODataProperty(), new ODataProperty() };
            ODataEntryWithNavigationLinks entry = new ODataEntryWithNavigationLinks(new ODataEntry { Properties = properties });

            deserializer.CallBase = true;
            deserializer.Setup(d => d.ApplyStructuralProperty(42, properties[0], _productEdmType, _readContext)).Verifiable();
            deserializer.Setup(d => d.ApplyStructuralProperty(42, properties[1], _productEdmType, _readContext)).Verifiable();

            // Act
            deserializer.Object.ApplyStructuralProperties(42, entry, _productEdmType, _readContext);

            // Assert
            deserializer.Verify();
        }

        [Fact]
        public void ApplyStructuralProperty_ThrowsArgumentNull_EntityResource()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            Assert.ThrowsArgumentNull(
                () => deserializer.ApplyStructuralProperty(entityResource: null, structuralProperty: new ODataProperty(),
                    entityType: _productEdmType, readContext: _readContext),
                "entityResource");
        }

        [Fact]
        public void ApplyStructuralProperty_ThrowsArgumentNull_StructuralProperty()
        {
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            Assert.ThrowsArgumentNull(
                () => deserializer.ApplyStructuralProperty(42, structuralProperty: null, entityType: _productEdmType, readContext: _readContext),
                "structuralProperty");
        }

        [Fact]
        public void ApplyStructuralProperty_SetsProperty()
        {
            // Arrange
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            Product product = new Product();
            ODataProperty property = new ODataProperty { Name = "ID", Value = 42 };

            // Act
            deserializer.ApplyStructuralProperty(product, property, _productEdmType, _readContext);

            // Assert
            Assert.Equal(42, product.ID);
        }

        [Fact]
        public void ReadFromStreamAsync_ForJsonLight()
        {
            ReadFromStreamAsync(Resources.ProductRequestEntryInPlainOldJson, true);
        }

        [Fact]
        public void ReadFromStreamAsync_ForAtom()
        {
            ReadFromStreamAsync(Resources.ProductRequestEntryInAtom, false);
        }

        private void ReadFromStreamAsync(string content, bool json)
        {
            ODataEntityDeserializer deserializer = new ODataEntityDeserializer(_deserializerProvider);
            Product product = deserializer.Read(GetODataMessageReader(GetODataMessage(content, json), _edmModel),
                typeof(Product), _readContext) as Product;

            Assert.Equal(product.ID, 0);
            Assert.Equal(product.Rating, 4);
            Assert.Equal(product.Price, 2.5m);
            Assert.Equal(product.ReleaseDate, new DateTime(1992, 1, 1, 0, 0, 0));
            Assert.Null(product.DiscontinuedDate);
        }

        [Fact]
        public void ReadFromStreamAsync_ComplexTypeAndInlineData_ForJsonLight()
        {
            ReadFromStreamAsync_ComplexTypeAndInlineData(Resources.SupplierRequestEntryInPlainOldJson, true);
        }

        [Fact]
        public void ReadFromStreamAsync_ComplexTypeAndInlineData_ForAtom()
        {
            ReadFromStreamAsync_ComplexTypeAndInlineData(Resources.SupplierRequestEntryInAtom, false);
        }

        private void ReadFromStreamAsync_ComplexTypeAndInlineData(string content, bool json)
        {
            IEdmEntityType supplierEntityType =
                EdmTestHelpers.GetModel().FindType("ODataDemo.Supplier") as IEdmEntityType;

            ODataEntityDeserializer deserializer = new ODataEntityDeserializer(_deserializerProvider);
            Supplier supplier = deserializer.Read(GetODataMessageReader(GetODataMessage(content, json), _edmModel),
                typeof(Supplier), _readContext) as Supplier;

            Assert.Equal(supplier.Name, "Supplier Name");

            Assert.NotNull(supplier.Products);
            Assert.Equal(6, supplier.Products.Count);
            Assert.Equal("soda", supplier.Products.ToList()[1].Name);

            Assert.NotNull(supplier.Address);
            Assert.Equal("Supplier City", supplier.Address.City);
            Assert.Equal("123456", supplier.Address.ZipCode);
        }

        [Fact]
        public void Read_PatchMode_ForJsonLight()
        {
            Read_PatchMode(Resources.SupplierPatchInPlainOldJson, true);
        }

        [Fact]
        public void Read_PatchMode_ForAtom()
        {
            Read_PatchMode(Resources.SupplierPatchInAtom, false);
        }

        private void Read_PatchMode(string content, bool json)
        {
            IEdmEntityType supplierEntityType =
                EdmTestHelpers.GetModel().FindType("ODataDemo.Supplier") as IEdmEntityType;
            _readContext.ResourceType = typeof(Delta<Supplier>);

            ODataEntityDeserializer deserializer =
                new ODataEntityDeserializer(_deserializerProvider);
            Delta<Supplier> supplier = deserializer.Read(GetODataMessageReader(GetODataMessage(content, json), _edmModel),
                typeof(Delta<Supplier>), _readContext) as Delta<Supplier>;

            Assert.NotNull(supplier);
            Assert.Equal(supplier.GetChangedPropertyNames(), new string[] { "ID", "Name", "Address" });

            Assert.Equal((supplier as dynamic).Name, "Supplier Name");
            Assert.Equal("Supplier City", (supplier as dynamic).Address.City);
            Assert.Equal("123456", (supplier as dynamic).Address.ZipCode);
        }

        [Fact]
        public void Read_ThrowsOnUnknownEntityType_ForJsonLight()
        {
            Read_ThrowsOnUnknownEntityType(Resources.SupplierRequestEntryInPlainOldJson, true,
                "The property 'Concurrency' does not exist on type 'ODataDemo.Product'. Make sure to only use property names that are defined by the type.");
        }

        [Fact]
        public void Read_ThrowsOnUnknownEntityType_ForAtom()
        {
            Read_ThrowsOnUnknownEntityType(Resources.SupplierRequestEntryInAtom, false,
                "An entry with type 'ODataDemo.Supplier' was found, but it is not assignable to the expected type 'ODataDemo.Product'. The type specified in the entry must be equal to either the expected type or a derived type.");
        }

        private void Read_ThrowsOnUnknownEntityType(string content, bool json, string expectedMessage)
        {
            IEdmEntityType supplierEntityType =
                EdmTestHelpers.GetModel().FindType("ODataDemo.Supplier") as IEdmEntityType;

            ODataEntityDeserializer deserializer = new ODataEntityDeserializer(_deserializerProvider);

            Assert.Throws<ODataException>(() => deserializer.Read(GetODataMessageReader(GetODataMessage(content, json), _edmModel),
                typeof(Product), _readContext), expectedMessage);
        }

        private static Type EdmTypeResolver(IEdmTypeReference edmType)
        {
            return Type.GetType(edmType.FullName());
        }

        private static ODataMessageReader GetODataMessageReader(IODataRequestMessage oDataRequestMessage, IEdmModel edmModel)
        {
            return new ODataMessageReader(oDataRequestMessage, new ODataMessageReaderSettings(), edmModel);
        }

        private static IODataRequestMessage GetODataMessage(string content, bool json)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/OData/OData.svc/Products");

            request.Content = new StringContent(content);
            request.Headers.Add("DataServiceVersion", "1.0");

            if (json)
            {
                MediaTypeWithQualityHeaderValue mediaType = new MediaTypeWithQualityHeaderValue("application/json");
                mediaType.Parameters.Add(new NameValueHeaderValue("odata", "fullmetadata"));
                request.Headers.Accept.Add(mediaType);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
            else
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/atom+xml");
            }

            return new HttpRequestODataMessage(request);
        }

        public abstract class BaseType
        {
            public int ID { get; set; }
        }

        public class Product
        {
            public int ID { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public DateTime? ReleaseDate { get; set; }

            public DateTime? DiscontinuedDate { get; set; }

            public int Rating { get; set; }

            public decimal Price { get; set; }

            public virtual Category Category { get; set; }

            public virtual Supplier Supplier { get; set; }
        }

        public class Category
        {
            public int ID { get; set; }

            public string Name { get; set; }

            public virtual ICollection<Product> Products { get; set; }
        }

        public class Supplier
        {
            public int ID { get; set; }

            public string Name { get; set; }

            public Address Address { get; set; }

            public int Concurrency { get; set; }

            public SupplierRating SupplierRating { get; set; }

            public virtual ICollection<Product> Products { get; set; }
        }

        public class Address
        {
            public string Street { get; set; }

            public string City { get; set; }

            public string State { get; set; }

            public string ZipCode { get; set; }

            public string Country { get; set; }
        }

        public enum SupplierRating
        {
            Gold,
            Silver,
            Bronze
        }
    }
}
