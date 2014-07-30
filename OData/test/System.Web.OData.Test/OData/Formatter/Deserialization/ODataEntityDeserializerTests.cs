// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Web.OData.Builder;
using System.Web.OData.Routing;
using System.Web.OData.TestCommon;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;
using Moq;

namespace System.Web.OData.Formatter.Deserialization
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
            IEdmEntitySet entitySet = _edmModel.EntityContainer.FindEntitySet("Products");
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
                "The related entity set or singleton cannot be found from the OData path. The related entity set or singleton is required to deserialize the payload.");
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
            builder.EntityType<BaseType>().Abstract();
            IEdmModel model = builder.GetEdmModel();
            var deserializer = new ODataEntityDeserializer(_deserializerProvider);
            ODataEntryWithNavigationLinks entry = new ODataEntryWithNavigationLinks(new ODataEntry { TypeName = "System.Web.OData.Formatter.Deserialization.BaseType" });

            Assert.Throws<ODataException>(
                () => deserializer.ReadEntry(entry, _productEdmType, new ODataDeserializerContext { Model = model }),
                "An instance of the abstract entity type 'System.Web.OData.Formatter.Deserialization.BaseType' was found. Abstract entity types cannot be instantiated.");
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
        public void ReadEntry_CanReadDynamicPropertiesForOpenEntityType()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntityType<SimpleOpenCustomer>();
            builder.EnumType<SimpleEnum>();
            IEdmModel model = builder.GetEdmModel();

            IEdmEntityTypeReference customerTypeReference = model.GetEdmTypeReference(typeof(SimpleOpenCustomer)).AsEntity();

            var deserializerProvider = new DefaultODataDeserializerProvider();
            var deserializer = new ODataEntityDeserializer(deserializerProvider);

            ODataEnumValue enumValue = new ODataEnumValue("Third", typeof(SimpleEnum).FullName);

            ODataComplexValue[] complexValues =
            {
                new ODataComplexValue
                {
                    TypeName = typeof(SimpleOpenAddress).FullName,
                    Properties = new[]
                    {
                        // declared properties
                        new ODataProperty { Name = "Street", Value = "Street 1" },
                        new ODataProperty { Name = "City", Value = "City 1" },

                        // dynamic properties
                        new ODataProperty
                        {
                            Name = "DateTimeProperty",
                            Value = new DateTimeOffset(new DateTime(2014, 5, 6))
                        }
                    }
                },
                new ODataComplexValue
                {
                    TypeName = typeof(SimpleOpenAddress).FullName,
                    Properties = new[]
                    {
                        // declared properties
                        new ODataProperty { Name = "Street", Value = "Street 2" },
                        new ODataProperty { Name = "City", Value = "City 2" },

                        // dynamic properties
                        new ODataProperty
                        {
                            Name = "ArrayProperty",
                            Value = new ODataCollectionValue { TypeName = "Collection(Edm.Int32)", Items = new[] {1, 2, 3, 4} }
                        }
                    }
                }
            };

            ODataCollectionValue collectionValue = new ODataCollectionValue
            {
                TypeName = "Collection(" + typeof(SimpleOpenAddress).FullName + ")",
                Items = complexValues
            };

            ODataEntry odataEntry = new ODataEntry
            {
                Properties = new[]
                {
                    // declared properties
                    new ODataProperty { Name = "CustomerId", Value = 991 },
                    new ODataProperty { Name = "Name", Value = "Name #991" },

                    // dynamic properties
                    new ODataProperty { Name = "GuidProperty", Value = new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA") },
                    new ODataProperty { Name = "EnumValue", Value = enumValue },
                    new ODataProperty { Name = "CollectionProperty", Value = collectionValue }
                },
                TypeName = typeof(SimpleOpenCustomer).FullName
            };

            ODataDeserializerContext readContext = new ODataDeserializerContext()
            {
                Model = model
            };

            ODataEntryWithNavigationLinks entry = new ODataEntryWithNavigationLinks(odataEntry);

            // Act
            SimpleOpenCustomer customer = deserializer.ReadEntry(entry, customerTypeReference, readContext)
                as SimpleOpenCustomer;

            // Assert
            Assert.NotNull(customer);

            // Verify the declared properties
            Assert.Equal(991, customer.CustomerId);
            Assert.Equal("Name #991", customer.Name);

            // Verify the dynamic properties
            Assert.NotNull(customer.CustomerProperties);
            Assert.Equal(3, customer.CustomerProperties.Count());
            Assert.Equal(new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA"), customer.CustomerProperties["GuidProperty"]);
            Assert.Equal(SimpleEnum.Third, customer.CustomerProperties["EnumValue"]);

            // Verify the dynamic collection property
            var collectionValues = Assert.IsType<List<SimpleOpenAddress>>(customer.CustomerProperties["CollectionProperty"]);
            Assert.NotNull(collectionValues);
            Assert.Equal(2, collectionValues.Count());

            Assert.Equal(new DateTimeOffset(new DateTime(2014, 5, 6)), collectionValues[0].Properties["DateTimeProperty"]);
            Assert.Equal(new List<int> { 1, 2, 3, 4 }, collectionValues[1].Properties["ArrayProperty"]);
        }

        [Fact]
        public void ReadEntry_CanReadDynamicPropertiesForInheritanceOpenEntityType()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntityType<SimpleOpenCustomer>();
            builder.EnumType<SimpleEnum>();
            IEdmModel model = builder.GetEdmModel();

            IEdmEntityTypeReference vipCustomerTypeReference = model.GetEdmTypeReference(typeof(SimpleVipCustomer)).AsEntity();

            var deserializerProvider = new DefaultODataDeserializerProvider();
            var deserializer = new ODataEntityDeserializer(deserializerProvider);

            ODataEntry odataEntry = new ODataEntry
            {
                Properties = new[]
                {
                    // declared properties
                    new ODataProperty { Name = "CustomerId", Value = 121 },
                    new ODataProperty { Name = "Name", Value = "VipName #121" },
                    new ODataProperty { Name = "VipNum", Value = "Vip Num 001" },

                    // dynamic properties
                    new ODataProperty { Name = "GuidProperty", Value = new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA") },
                },
                TypeName = typeof(SimpleVipCustomer).FullName
            };

            ODataDeserializerContext readContext = new ODataDeserializerContext()
            {
                Model = model
            };

            ODataEntryWithNavigationLinks entry = new ODataEntryWithNavigationLinks(odataEntry);

            // Act
            SimpleVipCustomer customer = deserializer.ReadEntry(entry, vipCustomerTypeReference, readContext)
                as SimpleVipCustomer;

            // Assert
            Assert.NotNull(customer);

            // Verify the declared properties
            Assert.Equal(121, customer.CustomerId);
            Assert.Equal("VipName #121", customer.Name);
            Assert.Equal("Vip Num 001", customer.VipNum);

            // Verify the dynamic properties
            Assert.NotNull(customer.CustomerProperties);
            Assert.Equal(1, customer.CustomerProperties.Count());
            Assert.Equal(new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA"), customer.CustomerProperties["GuidProperty"]);
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
                "The provided mapping does not contain an entry for the entity type 'ODataDemo.Product'.");
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
            IEdmCollectionTypeReference productsType = new EdmCollectionTypeReference(new EdmCollectionType(_productEdmType));
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
        public void ApplyNavigationProperty_UsesThePropertyAlias_ForFeed()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            model.Model.SetAnnotationValue(model.Order, new ClrTypeAnnotation(typeof(Order)));
            model.Model.SetAnnotationValue(
                model.Customer.FindProperty("Orders"),
                new ClrPropertyInfoAnnotation(typeof(Customer).GetProperty("AliasedOrders")));
            ODataFeedWithEntries feedWrapper = new ODataFeedWithEntries(new ODataFeed());
            feedWrapper.Entries.Add(new ODataEntryWithNavigationLinks(
                new ODataEntry { Properties = new[] { new ODataProperty { Name = "ID", Value = 42 } } }));

            Customer customer = new Customer();
            ODataNavigationLinkWithItems navLink =
                new ODataNavigationLinkWithItems(new ODataNavigationLink { Name = "Orders" });
            navLink.NestedItems.Add(feedWrapper);

            ODataDeserializerContext context = new ODataDeserializerContext { Model = model.Model };

            // Act
            new ODataEntityDeserializer(_deserializerProvider)
                .ApplyNavigationProperty(customer, navLink, model.Customer.AsReference(), context);

            // Assert
            Assert.Equal(1, customer.AliasedOrders.Count());
            Assert.Equal(42, customer.AliasedOrders[0].ID);
        }

        [Fact]
        public void ApplyNavigationProperty_UsesThePropertyAlias_ForEntry()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            model.Model.SetAnnotationValue(model.Order, new ClrTypeAnnotation(typeof(Order)));
            model.Model.SetAnnotationValue(
                model.Order.FindProperty("Customer"),
                new ClrPropertyInfoAnnotation(typeof(Order).GetProperty("AliasedCustomer")));
            ODataEntry entry = new ODataEntry { Properties = new[] { new ODataProperty { Name = "ID", Value = 42 } } };

            Order order = new Order();
            ODataNavigationLinkWithItems navLink =
                new ODataNavigationLinkWithItems(new ODataNavigationLink { Name = "Customer" });
            navLink.NestedItems.Add(new ODataEntryWithNavigationLinks(entry));

            ODataDeserializerContext context = new ODataDeserializerContext { Model = model.Model };

            // Act
            new ODataEntityDeserializer(_deserializerProvider)
                .ApplyNavigationProperty(order, navLink, model.Order.AsReference(), context);

            // Assert
            Assert.Equal(order.AliasedCustomer.ID, 42);
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
        public void ReadFromStreamAsync()
        {
            // Arrange
            string content = Resources.ProductRequestEntry;
            ODataEntityDeserializer deserializer = new ODataEntityDeserializer(_deserializerProvider);

            // Act
            Product product = deserializer.Read(GetODataMessageReader(GetODataMessage(content), _edmModel),
                typeof(Product), _readContext) as Product;

            // Assert
            Assert.Equal(product.ID, 0);
            Assert.Equal(product.Rating, 4);
            Assert.Equal(product.Price, 2.5m);
            Assert.Equal(product.ReleaseDate, new DateTimeOffset(new DateTime(1992, 1, 1, 0, 0, 0), TimeSpan.Zero));
            Assert.Null(product.DiscontinuedDate);
        }

        [Fact]
        public void ReadFromStreamAsync_ComplexTypeAndInlineData()
        {
            // Arrange
            string content = Resources.SupplierRequestEntry;
            IEdmEntityType supplierEntityType =
                EdmTestHelpers.GetModel().FindType("ODataDemo.Supplier") as IEdmEntityType;

            ODataEntityDeserializer deserializer = new ODataEntityDeserializer(_deserializerProvider);

            var readContext = new ODataDeserializerContext
            {
                Path = new ODataPath(new EntitySetPathSegment(_edmModel.EntityContainer.FindEntitySet("Suppliers"))),
                Model = _edmModel,
                ResourceType = typeof(Supplier)
            };

            // Act
            Supplier supplier = deserializer.Read(GetODataMessageReader(GetODataMessage(content), _edmModel),
                typeof(Supplier), readContext) as Supplier;

            // Assert
            Assert.Equal(supplier.Name, "Supplier Name");

            Assert.NotNull(supplier.Products);
            Assert.Equal(6, supplier.Products.Count);
            Assert.Equal("soda", supplier.Products.ToList()[1].Name);

            Assert.NotNull(supplier.Address);
            Assert.Equal("Supplier City", supplier.Address.City);
            Assert.Equal("123456", supplier.Address.ZipCode);
        }

        [Fact]
        public void Read_PatchMode()
        {
            // Arrange
            string content = Resources.SupplierPatch;
            IEdmEntityType supplierEntityType =
                EdmTestHelpers.GetModel().FindType("ODataDemo.Supplier") as IEdmEntityType;

            var readContext = new ODataDeserializerContext
            {
                Path = new ODataPath(new EntitySetPathSegment(_edmModel.EntityContainer.FindEntitySet("Suppliers"))),
                Model = _edmModel,
                ResourceType = typeof(Delta<Supplier>)
            };

            ODataEntityDeserializer deserializer =
                new ODataEntityDeserializer(_deserializerProvider);

            // Act
            Delta<Supplier> supplier = deserializer.Read(GetODataMessageReader(GetODataMessage(content), _edmModel),
                typeof(Delta<Supplier>), readContext) as Delta<Supplier>;

            // Assert
            Assert.NotNull(supplier);
            Assert.Equal(supplier.GetChangedPropertyNames(), new string[] { "ID", "Name", "Address" });

            Assert.Equal((supplier as dynamic).Name, "Supplier Name");
            Assert.Equal("Supplier City", (supplier as dynamic).Address.City);
            Assert.Equal("123456", (supplier as dynamic).Address.ZipCode);
        }

        [Fact]
        public void Read_ThrowsOnUnknownEntityType()
        {
            // Arrange
            string content = Resources.SupplierRequestEntry;
            IEdmEntityType supplierEntityType =
                EdmTestHelpers.GetModel().FindType("ODataDemo.Supplier") as IEdmEntityType;

            ODataEntityDeserializer deserializer = new ODataEntityDeserializer(_deserializerProvider);

            // Act & Assert
            Assert.Throws<ODataException>(() => deserializer.Read(GetODataMessageReader(GetODataMessage(content), _edmModel),
                typeof(Product), _readContext), "The property 'Concurrency' does not exist on type 'ODataDemo.Product'. Make sure to only use property names that are defined by the type.");
        }

        private static Type EdmTypeResolver(IEdmTypeReference edmType)
        {
            return Type.GetType(edmType.FullName());
        }

        private static ODataMessageReader GetODataMessageReader(IODataRequestMessage oDataRequestMessage, IEdmModel edmModel)
        {
            return new ODataMessageReader(oDataRequestMessage, new ODataMessageReaderSettings(), edmModel);
        }

        private static IODataRequestMessage GetODataMessage(string content)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/OData/OData.svc/Products");

            request.Content = new StringContent(content);
            request.Headers.Add("OData-Version", "4.0");

            MediaTypeWithQualityHeaderValue mediaType = new MediaTypeWithQualityHeaderValue("application/json");
            mediaType.Parameters.Add(new NameValueHeaderValue("odata.metadata", "full"));
            request.Headers.Accept.Add(mediaType);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

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

            public DateTimeOffset? ReleaseDate { get; set; }

            public DateTimeOffset? DiscontinuedDate { get; set; }

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

        private class Customer
        {
            public int ID { get; set; }

            public Order[] AliasedOrders { get; set; }
        }

        private class Order
        {
            public int ID { get; set; }

            public Customer AliasedCustomer { get; set; }
        }
    }
}
