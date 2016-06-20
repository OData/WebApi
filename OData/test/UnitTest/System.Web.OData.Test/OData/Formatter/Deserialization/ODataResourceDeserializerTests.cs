// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Web.OData.Builder;
using System.Web.OData.TestCommon;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;
using Moq;
using ODataPath = System.Web.OData.Routing.ODataPath;

namespace System.Web.OData.Formatter.Deserialization
{
    public class ODataResourceDeserializerTests
    {
        private readonly IEdmModel _edmModel;
        private readonly ODataDeserializerContext _readContext;
        private readonly IEdmEntityTypeReference _productEdmType;
        private readonly IEdmEntityTypeReference _supplierEdmType;
        private readonly IEdmComplexTypeReference _addressEdmType;
        private readonly ODataDeserializerProvider _deserializerProvider;

        public ODataResourceDeserializerTests()
        {
            _edmModel = EdmTestHelpers.GetModel();
            IEdmEntitySet entitySet = _edmModel.EntityContainer.FindEntitySet("Products");
            _readContext = new ODataDeserializerContext
            {
                Path = new ODataPath(new EntitySetSegment(entitySet)),
                Model = _edmModel,
                ResourceType = typeof(Product)
            };
            _productEdmType = _edmModel.GetEdmTypeReference(typeof(Product)).AsEntity();
            _supplierEdmType = _edmModel.GetEdmTypeReference(typeof(Supplier)).AsEntity();
            _addressEdmType = _edmModel.GetEdmTypeReference(typeof(Address)).AsComplex();
            _deserializerProvider = new DefaultODataDeserializerProvider();
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_DeserializerProvider()
        {
            Assert.ThrowsArgumentNull(() => new ODataResourceDeserializer(deserializerProvider: null), "deserializerProvider");
        }

        [Fact]
        public void Read_ThrowsArgumentNull_MessageReader()
        {
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            Assert.ThrowsArgumentNull(
                () => deserializer.Read(messageReader: null, type: typeof(Product), readContext: _readContext),
                "messageReader");
        }

        [Fact]
        public void Read_ThrowsArgumentNull_ReadContext()
        {
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            Assert.ThrowsArgumentNull(
                () => deserializer.Read(messageReader: ODataTestUtil.GetMockODataMessageReader(), type: typeof(Product), readContext: null),
                "readContext");
        }

        [Fact(Skip = "TODO: Sam Xu")]
        public void Read_ThrowsArgument_ODataPathMissing()
        {
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            Assert.ThrowsArgument(
                () => deserializer.Read(ODataTestUtil.GetMockODataMessageReader(), typeof(Product), new ODataDeserializerContext()),
                "readContext",
                "The operation cannot be completed because no ODataPath is available for the request.");
        }

        [Fact(Skip = "TODO: Sam Xu")]
        public void Read_ThrowsArgument_EntitysetMissing()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);

            // Act & Assert
            Assert.Throws<SerializationException>(
                () => deserializer.Read(ODataTestUtil.GetMockODataMessageReader(), typeof(Product), new ODataDeserializerContext { Path = new ODataPath() }),
                "The related entity set or singleton cannot be found from the OData path. The related entity set or singleton is required to deserialize the payload.");
        }

        [Fact]
        public void ReadInline_ThrowsArgumentNull_Item()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => deserializer.ReadInline(item: null, edmType: _productEdmType, readContext: new ODataDeserializerContext()),
                "item");
        }

        [Fact]
        public void ReadInline_Throws_ArgumentMustBeOfType()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);

            // Act & Assert
            Assert.ThrowsArgument(
                () => deserializer.ReadInline(item: 42, edmType: _productEdmType, readContext: new ODataDeserializerContext()),
                "item",
                "The argument must be of type 'ODataResource'");
        }

        [Fact]
        public void ReadInline_Calls_ReadResource()
        {
            // Arrange
            var deserializer = new Mock<ODataResourceDeserializer>(_deserializerProvider);
            ODataResourceWrapper entry = new ODataResourceWrapper(new ODataResource());
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            deserializer.CallBase = true;
            deserializer.Setup(d => d.ReadResource(entry, _productEdmType, readContext)).Returns(42).Verifiable();

            // Act
            var result = deserializer.Object.ReadInline(entry, _productEdmType, readContext);

            // Assert
            deserializer.Verify();
            Assert.Equal(42, result);
        }

        [Fact]
        public void ReadResource_ThrowsArgumentNull_ResourceWrapper()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => deserializer.ReadResource(resourceWrapper: null, structuredType: _productEdmType, readContext: _readContext),
                "resourceWrapper");
        }

        [Fact]
        public void ReadResource_ThrowsArgumentNull_ReadContext()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            ODataResourceWrapper resourceWrapper = new ODataResourceWrapper(new ODataResource());

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => deserializer.ReadResource(resourceWrapper, structuredType: _productEdmType, readContext: null),
                "readContext");
        }

        [Fact]
        public void ReadResource_ThrowsArgument_ModelMissingFromReadContext()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            ODataResourceWrapper resourceWrapper = new ODataResourceWrapper(new ODataResource { TypeName = _supplierEdmType.FullName() });

            // Act & Assert
            Assert.ThrowsArgument(
                () => deserializer.ReadResource(resourceWrapper, _productEdmType, new ODataDeserializerContext()),
                "readContext",
                "The EDM model is missing on the read context. The model is required on the read context to deserialize the payload.");
        }

        [Fact]
        public void ReadResource_ThrowsODataException_EntityTypeNotInModel()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            ODataResourceWrapper entry = new ODataResourceWrapper(new ODataResource { TypeName = "MissingType" });

            // Act & Assert
            Assert.Throws<ODataException>(
                () => deserializer.ReadResource(entry, _productEdmType, _readContext),
                "Cannot find the resource type 'MissingType' in the model.");
        }

        [Fact]
        public void ReadResource_ThrowsODataException_CannotInstantiateAbstractResourceType()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntityType<BaseType>().Abstract();
            IEdmModel model = builder.GetEdmModel();
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            ODataResourceWrapper resourceWrapper =
                new ODataResourceWrapper(new ODataResource
                {
                    TypeName = "System.Web.OData.Formatter.Deserialization.BaseType"
                });

            // Act & Assert
            Assert.Throws<ODataException>(
                () => deserializer.ReadResource(resourceWrapper, _productEdmType, new ODataDeserializerContext { Model = model }),
                "An instance of the abstract resource type 'System.Web.OData.Formatter.Deserialization.BaseType' was found. " +
                "Abstract resource types cannot be instantiated.");
        }

        [Fact]
        public void ReadResource_ThrowsSerializationException_TypeCannotBeDeserialized()
        {
            // Arrange
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            deserializerProvider.Setup(d => d.GetEdmTypeDeserializer(It.IsAny<IEdmTypeReference>())).Returns<ODataEdmTypeDeserializer>(null);
            var deserializer = new ODataResourceDeserializer(deserializerProvider.Object);
            ODataResourceWrapper resourceWrapper = new ODataResourceWrapper(new ODataResource { TypeName = _supplierEdmType.FullName() });

            // Act & Assert
            Assert.Throws<SerializationException>(
                () => deserializer.ReadResource(resourceWrapper, _productEdmType, _readContext),
                "'ODataDemo.Supplier' cannot be deserialized using the ODataMediaTypeFormatter.");
        }

        [Fact]
        public void ReadResource_DispatchesToRightDeserializer_IfEntityTypeNameIsDifferent()
        {
            // Arrange
            Mock<ODataEdmTypeDeserializer> supplierDeserializer = new Mock<ODataEdmTypeDeserializer>(ODataPayloadKind.Resource);
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            var deserializer = new ODataResourceDeserializer(deserializerProvider.Object);
            ODataResourceWrapper resourceWrapper = new ODataResourceWrapper(new ODataResource { TypeName = _supplierEdmType.FullName() });

            deserializerProvider.Setup(d => d.GetEdmTypeDeserializer(It.IsAny<IEdmTypeReference>())).Returns(supplierDeserializer.Object);
            supplierDeserializer
                .Setup(d => d.ReadInline(resourceWrapper, It.Is<IEdmTypeReference>(e => _supplierEdmType.Definition == e.Definition), _readContext))
                .Returns(42).Verifiable();

            // Act
            object result = deserializer.ReadResource(resourceWrapper, _productEdmType, _readContext);

            // Assert
            supplierDeserializer.Verify();
            Assert.Equal(42, result);
        }

        [Fact]
        public void ReadResource_SetsExpectedAndActualEdmType_OnCreatedEdmObject_TypelessMode()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            IEdmEntityTypeReference customerType = EdmLibHelpers.ToEdmTypeReference(model.Customer, isNullable: false).AsEntity();
            ODataDeserializerContext readContext = new ODataDeserializerContext { Model = model.Model, ResourceType = typeof(IEdmObject) };
            ODataResourceWrapper resourceWrapper = new ODataResourceWrapper(new ODataResource
            {
                TypeName = model.SpecialCustomer.FullName(),
                Properties = new ODataProperty[0]
            });

            ODataResourceDeserializer deserializer = new ODataResourceDeserializer(_deserializerProvider);

            // Act
            var result = deserializer.ReadResource(resourceWrapper, customerType, readContext);

            // Assert
            EdmEntityObject resource = Assert.IsType<EdmEntityObject>(result);
            Assert.Equal(model.SpecialCustomer, resource.ActualEdmType);
            Assert.Equal(model.Customer, resource.ExpectedEdmType);
        }

        [Fact]
        public void ReadResource_Calls_CreateResourceInstance()
        {
            // Arrange
            Mock<ODataResourceDeserializer> deserializer = new Mock<ODataResourceDeserializer>(_deserializerProvider);
            ODataResourceWrapper resourceWrapper = new ODataResourceWrapper(new ODataResource { Properties = Enumerable.Empty<ODataProperty>() });
            deserializer.CallBase = true;
            deserializer.Setup(d => d.CreateResourceInstance(_productEdmType, _readContext)).Returns(42).Verifiable();

            // Act
            var result = deserializer.Object.ReadResource(resourceWrapper, _productEdmType, _readContext);

            // Assert
            Assert.Equal(42, result);
            deserializer.Verify();
        }

        [Fact]
        public void ReadResource_Calls_ApplyStructuralProperties()
        {
            // Arrange
            Mock<ODataResourceDeserializer> deserializer = new Mock<ODataResourceDeserializer>(_deserializerProvider);
            ODataResourceWrapper resourceWrapper = new ODataResourceWrapper(new ODataResource { Properties = Enumerable.Empty<ODataProperty>() });
            deserializer.CallBase = true;
            deserializer.Setup(d => d.CreateResourceInstance(_productEdmType, _readContext)).Returns(42);
            deserializer.Setup(d => d.ApplyStructuralProperties(42, resourceWrapper, _productEdmType, _readContext)).Verifiable();

            // Act
            deserializer.Object.ReadResource(resourceWrapper, _productEdmType, _readContext);

            // Assert
            deserializer.Verify();
        }

        [Fact]
        public void ReadResource_Calls_ApplyNestedProperties()
        {
            // Arrange
            Mock<ODataResourceDeserializer> deserializer = new Mock<ODataResourceDeserializer>(_deserializerProvider);
            ODataResourceWrapper resourceWrapper = new ODataResourceWrapper(new ODataResource { Properties = Enumerable.Empty<ODataProperty>() });
            deserializer.CallBase = true;
            deserializer.Setup(d => d.CreateResourceInstance(_productEdmType, _readContext)).Returns(42);
            deserializer.Setup(d => d.ApplyNestedProperties(42, resourceWrapper, _productEdmType, _readContext)).Verifiable();

            // Act
            deserializer.Object.ReadResource(resourceWrapper, _productEdmType, _readContext);

            // Assert
            deserializer.Verify();
        }

        [Fact]
        public void ReadResource_CanReadDynamicPropertiesForOpenEntityType()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntityType<SimpleOpenCustomer>();
            builder.EnumType<SimpleEnum>();
            IEdmModel model = builder.GetEdmModel();

            IEdmEntityTypeReference customerTypeReference = model.GetEdmTypeReference(typeof(SimpleOpenCustomer)).AsEntity();

            var deserializerProvider = new DefaultODataDeserializerProvider();
            var deserializer = new ODataResourceDeserializer(deserializerProvider);

            ODataEnumValue enumValue = new ODataEnumValue("Third", typeof(SimpleEnum).FullName);

            ODataResource[] complexResources =
            {
                new ODataResource
                {
                    TypeName = typeof(SimpleOpenAddress).FullName,
                    Properties = new[]
                    {
                        // declared properties
                        new ODataProperty {Name = "Street", Value = "Street 1"},
                        new ODataProperty {Name = "City", Value = "City 1"},

                        // dynamic properties
                        new ODataProperty
                        {
                            Name = "DateTimeProperty",
                            Value = new DateTimeOffset(new DateTime(2014, 5, 6))
                        }
                    }
                },
                new ODataResource
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

            ODataResource odataResource = new ODataResource
            {
                Properties = new[]
                {
                    // declared properties
                    new ODataProperty { Name = "CustomerId", Value = 991 },
                    new ODataProperty { Name = "Name", Value = "Name #991" },

                    // dynamic properties
                    new ODataProperty { Name = "GuidProperty", Value = new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA") },
                    new ODataProperty { Name = "EnumValue", Value = enumValue },
                },
                TypeName = typeof(SimpleOpenCustomer).FullName
            };

            ODataDeserializerContext readContext = new ODataDeserializerContext()
            {
                Model = model
            };

            ODataResourceWrapper topLevelResourceWrapper = new ODataResourceWrapper(odataResource);

            ODataNestedResourceInfo resourceInfo = new ODataNestedResourceInfo
            {
                IsCollection = true,
                Name = "CollectionProperty"
            };
            ODataNestedResourceInfoWrapper resourceInfoWrapper = new ODataNestedResourceInfoWrapper(resourceInfo);
            ODataResourceSetWrapper resourceSetWrapper = new ODataResourceSetWrapper(new ODataResourceSet());
            foreach (var complexResource in complexResources)
            {
                resourceSetWrapper.Resources.Add(new ODataResourceWrapper(complexResource));
            }
            resourceInfoWrapper.NestedItems.Add(resourceSetWrapper);
            topLevelResourceWrapper.NestedResourceInfos.Add(resourceInfoWrapper);

            // Act
            SimpleOpenCustomer customer = deserializer.ReadResource(topLevelResourceWrapper, customerTypeReference, readContext)
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
        public void ReadSource_CanReadDynamicPropertiesForInheritanceOpenEntityType()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntityType<SimpleOpenCustomer>();
            builder.EnumType<SimpleEnum>();
            IEdmModel model = builder.GetEdmModel();

            IEdmEntityTypeReference vipCustomerTypeReference = model.GetEdmTypeReference(typeof(SimpleVipCustomer)).AsEntity();

            var deserializerProvider = new DefaultODataDeserializerProvider();
            var deserializer = new ODataResourceDeserializer(deserializerProvider);

            ODataResource resource = new ODataResource
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

            ODataResourceWrapper resourceWrapper = new ODataResourceWrapper(resource);

            // Act
            SimpleVipCustomer customer = deserializer.ReadResource(resourceWrapper, vipCustomerTypeReference, readContext)
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

        public class MyCustomer
        {
            public int Id { get; set; }

            [Column(TypeName = "date")]
            public DateTime Birthday { get; set; }

            [Column(TypeName = "time")]
            public TimeSpan ReleaseTime { get; set; }
        }

        [Fact]
        public void ReadResource_CanReadDatTimeRelatedProperties()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntityType<MyCustomer>().Namespace = "NS";
            IEdmModel model = builder.GetEdmModel();

            IEdmEntityTypeReference vipCustomerTypeReference = model.GetEdmTypeReference(typeof(MyCustomer)).AsEntity();

            var deserializerProvider = new DefaultODataDeserializerProvider();
            var deserializer = new ODataResourceDeserializer(deserializerProvider);

            ODataResource resource = new ODataResource
            {
                Properties = new[]
                {
                    new ODataProperty { Name = "Id", Value = 121 },
                    new ODataProperty { Name = "Birthday", Value = new Date(2015, 12, 12) },
                    new ODataProperty { Name = "ReleaseTime", Value = new TimeOfDay(1, 2, 3, 4) },
                },
                TypeName = "NS.MyCustomer"
            };

            ODataDeserializerContext readContext = new ODataDeserializerContext { Model = model };
            ODataResourceWrapper resourceWrapper = new ODataResourceWrapper(resource);

            // Act
            var customer = deserializer.ReadResource(resourceWrapper, vipCustomerTypeReference, readContext) as MyCustomer;

            // Assert
            Assert.NotNull(customer);
            Assert.Equal(121, customer.Id);
            Assert.Equal(new DateTime(2015, 12, 12), customer.Birthday);
            Assert.Equal(new TimeSpan(0, 1, 2, 3, 4), customer.ReleaseTime);
        }

        [Fact]
        public void CreateResourceInstance_ThrowsArgumentNull_ReadContext()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => deserializer.CreateResourceInstance(_productEdmType, readContext: null),
                "readContext");
        }

        [Fact]
        public void CreateResourceInstance_ThrowsArgument_ModelMissingFromReadContext()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);

            // Act & Assert
            Assert.ThrowsArgument(
                () => deserializer.CreateResourceInstance(_productEdmType, new ODataDeserializerContext()),
                "readContext",
                "The EDM model is missing on the read context. The model is required on the read context to deserialize the payload.");
        }

        [Fact]
        public void CreateResourceInstance_ThrowsODataException_MappingDoesNotContainEntityType()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);

            // Act & Assert
            Assert.Throws<ODataException>(
                () => deserializer.CreateResourceInstance(_productEdmType, new ODataDeserializerContext { Model = EdmCoreModel.Instance }),
                "The provided mapping does not contain a resource for the resource type 'ODataDemo.Product'.");
        }

        [Fact]
        public void CreateResourceInstance_CreatesDeltaOfT_IfPatchMode()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _readContext.Model,
                ResourceType = typeof(Delta<Product>)
            };

            // Act & Assert
            Assert.IsType<Delta<Product>>(deserializer.CreateResourceInstance(_productEdmType, readContext));
        }

        [Fact]
        public void CreateResourceInstance_CreatesDeltaWith_ExpectedUpdatableProperties()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _readContext.Model,
                ResourceType = typeof(Delta<Product>)
            };
            var structuralProperties = _productEdmType.StructuralProperties().Select(p => p.Name);

            // Act
            Delta<Product> resource = deserializer.CreateResourceInstance(_productEdmType, readContext) as Delta<Product>;

            // Assert
            Assert.NotNull(resource);
            Assert.Equal(structuralProperties, resource.GetUnchangedPropertyNames());
        }

        [Fact]
        public void CreateResourceInstance_CreatesEdmEntityObject_IfTypeLessMode()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _readContext.Model,
                ResourceType = typeof(IEdmObject)
            };

            // Act
            var result = deserializer.CreateResourceInstance(_productEdmType, readContext);

            // Assert
            EdmEntityObject resource = Assert.IsType<EdmEntityObject>(result);
            Assert.Equal(_productEdmType, resource.GetEdmType(), new EdmTypeReferenceEqualityComparer());
        }

        [Fact]
        public void CreateResourceInstance_CreatesEdmComplexObject_IfTypeLessMode()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _readContext.Model,
                ResourceType = typeof(IEdmObject)
            };

            // Act
            var result = deserializer.CreateResourceInstance(_addressEdmType, readContext);

            // Assert
            EdmComplexObject resource = Assert.IsType<EdmComplexObject>(result);
            Assert.Equal(_addressEdmType, resource.GetEdmType(), new EdmTypeReferenceEqualityComparer());
        }

        [Fact]
        public void CreateResourceInstance_CreatesT_IfNotPatchMode()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _readContext.Model,
                ResourceType = typeof(Product)
            };

            // Act & Assert
            Assert.IsType<Product>(deserializer.CreateResourceInstance(_productEdmType, readContext));
        }

        [Fact]
        public void ApplyNestedProperties_ThrowsArgumentNull_EntryWrapper()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => deserializer.ApplyNestedProperties(42, resourceWrapper: null, structuredType: _productEdmType, readContext: _readContext),
                "resourceWrapper");
        }

        [Fact]
        public void ApplyNestedProperties_Calls_ApplyNavigationPropertyForEachNavigationLink()
        {
            // Arrange
            ODataResourceWrapper resource = new ODataResourceWrapper(new ODataResource());
            resource.NestedResourceInfos.Add(new ODataNestedResourceInfoWrapper(new ODataNestedResourceInfo()));
            resource.NestedResourceInfos.Add(new ODataNestedResourceInfoWrapper(new ODataNestedResourceInfo()));

            Mock<ODataResourceDeserializer> deserializer = new Mock<ODataResourceDeserializer>(_deserializerProvider);
            deserializer.CallBase = true;
            deserializer.Setup(d => d.ApplyNestedProperty(42, resource.NestedResourceInfos[0], _productEdmType, _readContext)).Verifiable();
            deserializer.Setup(d => d.ApplyNestedProperty(42, resource.NestedResourceInfos[1], _productEdmType, _readContext)).Verifiable();

            // Act
            deserializer.Object.ApplyNestedProperties(42, resource, _productEdmType, _readContext);

            // Assert
            deserializer.Verify();
        }

        [Fact]
        public void ApplyNestedProperty_ThrowsArgumentNull_ResourceInfoWrapper()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => deserializer.ApplyNestedProperty(42, resourceInfoWrapper: null, structuredType: _productEdmType,
                    readContext: _readContext),
                "resourceInfoWrapper");
        }

        [Fact]
        public void ApplyNestedProperty_ThrowsArgumentNull_EntityResource()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            ODataNestedResourceInfoWrapper resourceInfoWrapper = new ODataNestedResourceInfoWrapper(new ODataNestedResourceInfo());

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => deserializer.ApplyNestedProperty(resource: null, resourceInfoWrapper: resourceInfoWrapper,
                    structuredType: _productEdmType, readContext: _readContext),
                "resource");
        }

        [Fact]
        public void ApplyNestedProperty_ThrowsODataException_NavigationPropertyNotfound()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            ODataNestedResourceInfoWrapper resourceInfoWrapper = new ODataNestedResourceInfoWrapper(new ODataNestedResourceInfo { Name = "SomeProperty" });

            // Act & Assert
            Assert.Throws<ODataException>(
                () => deserializer.ApplyNestedProperty(42, resourceInfoWrapper, _productEdmType, _readContext),
                "Cannot find nested property 'SomeProperty' on the resource type 'ODataDemo.Product'.");
        }

        [Fact]
        public void ApplyNestedProperty_ThrowsODataException_WhenPatchingNavigationProperty()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            ODataNestedResourceInfoWrapper resourceInfoWrapper = new ODataNestedResourceInfoWrapper(new ODataNestedResourceInfo { Name = "Supplier" });
            resourceInfoWrapper.NestedItems.Add(new ODataResourceWrapper(new ODataResource()));
            _readContext.ResourceType = typeof(Delta<Supplier>);

            // Act & Assert
            Assert.Throws<ODataException>(
                () => deserializer.ApplyNestedProperty(42, resourceInfoWrapper, _productEdmType, _readContext),
                "Cannot apply PATCH to navigation property 'Supplier' on entity type 'ODataDemo.Product'.");
        }

        [Fact]
        public void ApplyNestedProperty_ThrowsODataException_WhenPatchingCollectionNavigationProperty()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            ODataNestedResourceInfoWrapper resourceInfoWrapper = new ODataNestedResourceInfoWrapper(new ODataNestedResourceInfo { Name = "Products" });
            resourceInfoWrapper.NestedItems.Add(new ODataResourceSetWrapper(new ODataResourceSet()));
            _readContext.ResourceType = typeof(Delta<Supplier>);

            // Act & Assert
            Assert.Throws<ODataException>(
                () => deserializer.ApplyNestedProperty(42, resourceInfoWrapper, _supplierEdmType, _readContext),
                "Cannot apply PATCH to navigation property 'Products' on entity type 'ODataDemo.Supplier'.");
        }

        [Fact(Skip = "TODO: Sam Xu")]
        public void ApplyNestProperty_Calls_ReadInlineOnResourceSet()
        {
            // Arrange
            Mock<ODataEdmTypeDeserializer> productsDeserializer = new Mock<ODataEdmTypeDeserializer>(ODataPayloadKind.ResourceSet);
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            var deserializer = new ODataResourceDeserializer(deserializerProvider.Object);
            ODataNestedResourceInfoWrapper resourceInfoWrapper = new ODataNestedResourceInfoWrapper(new ODataNestedResourceInfo { Name = "Products" });
            resourceInfoWrapper.NestedItems.Add(new ODataResourceSetWrapper(new ODataResourceSet()));

            Supplier supplier = new Supplier();
            IEnumerable products = new[] { new Product { ID = 42 } };

            deserializerProvider.Setup(d => d.GetEdmTypeDeserializer(It.IsAny<IEdmTypeReference>())).Returns(productsDeserializer.Object);
            productsDeserializer
                .Setup(d => d.ReadInline(resourceInfoWrapper.NestedItems[0], _supplierEdmType.FindNavigationProperty("Products").Type, _readContext))
                .Returns(products).Verifiable();

            // Act
            deserializer.ApplyNestedProperty(supplier, resourceInfoWrapper, _supplierEdmType, _readContext);

            // Assert
            productsDeserializer.Verify();
            Assert.Equal(1, supplier.Products.Count());
            Assert.Equal(42, supplier.Products.First().ID);
        }

        [Fact(Skip = "TODO: Sam Xu about to create the new deserialize context for nested property")]
        public void ApplyNestedProperty_Calls_ReadInlineOnResource()
        {
            // Arrange
            Mock<ODataEdmTypeDeserializer> supplierDeserializer = new Mock<ODataEdmTypeDeserializer>(ODataPayloadKind.ResourceSet);
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            var deserializer = new ODataResourceDeserializer(deserializerProvider.Object);
            ODataNestedResourceInfoWrapper resourceInfoWrapper = new ODataNestedResourceInfoWrapper(new ODataNestedResourceInfo { Name = "Supplier" });
            resourceInfoWrapper.NestedItems.Add(new ODataResourceWrapper(new ODataResource()));

            Product product = new Product();
            Supplier supplier = new Supplier { ID = 42 };

            deserializerProvider.Setup(d => d.GetEdmTypeDeserializer(It.IsAny<IEdmTypeReference>())).Returns(supplierDeserializer.Object);
            supplierDeserializer
                .Setup(d => d.ReadInline(resourceInfoWrapper.NestedItems[0], _productEdmType.FindNavigationProperty("Supplier").Type, _readContext))
                .Returns(supplier).Verifiable();

            // Act
            deserializer.ApplyNestedProperty(product, resourceInfoWrapper, _productEdmType, _readContext);

            // Assert
            supplierDeserializer.Verify();
            Assert.Equal(supplier, product.Supplier);
        }

        [Fact]
        public void ApplyNestedProperty_UsesThePropertyAlias_ForResourceSet()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            model.Model.SetAnnotationValue(model.Order, new ClrTypeAnnotation(typeof(Order)));
            model.Model.SetAnnotationValue(
                model.Customer.FindProperty("Orders"),
                new ClrPropertyInfoAnnotation(typeof(Customer).GetProperty("AliasedOrders")));
            ODataResourceSetWrapper resourceSetWrapper = new ODataResourceSetWrapper(new ODataResourceSet());
            resourceSetWrapper.Resources.Add(new ODataResourceWrapper(
                new ODataResource { Properties = new[] { new ODataProperty { Name = "ID", Value = 42 } } }));

            Customer customer = new Customer();
            ODataNestedResourceInfoWrapper resourceInfoWrapper =
                new ODataNestedResourceInfoWrapper(new ODataNestedResourceInfo { Name = "Orders" });
            resourceInfoWrapper.NestedItems.Add(resourceSetWrapper);

            ODataDeserializerContext context = new ODataDeserializerContext { Model = model.Model };

            // Act
            new ODataResourceDeserializer(_deserializerProvider)
                .ApplyNestedProperty(customer, resourceInfoWrapper, model.Customer.AsReference(), context);

            // Assert
            Assert.Equal(1, customer.AliasedOrders.Count());
            Assert.Equal(42, customer.AliasedOrders[0].ID);
        }

        [Fact]
        public void ApplyNestedProperty_UsesThePropertyAlias_ForResourceWrapper()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            model.Model.SetAnnotationValue(model.Order, new ClrTypeAnnotation(typeof(Order)));
            model.Model.SetAnnotationValue(
                model.Order.FindProperty("Customer"),
                new ClrPropertyInfoAnnotation(typeof(Order).GetProperty("AliasedCustomer")));
            ODataResource resource = new ODataResource { Properties = new[] { new ODataProperty { Name = "ID", Value = 42 } } };

            Order order = new Order();
            ODataNestedResourceInfoWrapper resourceInfoWrapper =
                new ODataNestedResourceInfoWrapper(new ODataNestedResourceInfo { Name = "Customer" });
            resourceInfoWrapper.NestedItems.Add(new ODataResourceWrapper(resource));

            ODataDeserializerContext context = new ODataDeserializerContext { Model = model.Model };

            // Act
            new ODataResourceDeserializer(_deserializerProvider)
                .ApplyNestedProperty(order, resourceInfoWrapper, model.Order.AsReference(), context);

            // Assert
            Assert.Equal(order.AliasedCustomer.ID, 42);
        }

        [Fact]
        public void ApplyStructuralProperties_ThrowsArgumentNull_resourceWrapper()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => deserializer.ApplyStructuralProperties(42, resourceWrapper: null, structuredType: _productEdmType, readContext: _readContext),
                "resourceWrapper");
        }

        [Fact]
        public void ApplyStructuralProperties_Calls_ApplyStructuralPropertyOnEachPropertyInResource()
        {
            // Arrange
            var deserializer = new Mock<ODataResourceDeserializer>(_deserializerProvider);
            ODataProperty[] properties = new[] { new ODataProperty(), new ODataProperty() };
            ODataResourceWrapper resourceWrapper = new ODataResourceWrapper(new ODataResource { Properties = properties });

            deserializer.CallBase = true;
            deserializer.Setup(d => d.ApplyStructuralProperty(42, properties[0], _productEdmType, _readContext)).Verifiable();
            deserializer.Setup(d => d.ApplyStructuralProperty(42, properties[1], _productEdmType, _readContext)).Verifiable();

            // Act
            deserializer.Object.ApplyStructuralProperties(42, resourceWrapper, _productEdmType, _readContext);

            // Assert
            deserializer.Verify();
        }

        [Fact]
        public void ApplyStructuralProperty_ThrowsArgumentNull_Resource()
        {
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            Assert.ThrowsArgumentNull(
                () => deserializer.ApplyStructuralProperty(resource: null, structuralProperty: new ODataProperty(),
                    structuredType: _productEdmType, readContext: _readContext),
                "resource");
        }

        [Fact]
        public void ApplyStructuralProperty_ThrowsArgumentNull_StructuralProperty()
        {
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            Assert.ThrowsArgumentNull(
                () => deserializer.ApplyStructuralProperty(42, structuralProperty: null, structuredType: _productEdmType, readContext: _readContext),
                "structuralProperty");
        }

        [Fact]
        public void ApplyStructuralProperty_SetsProperty()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
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
            ODataResourceDeserializer deserializer = new ODataResourceDeserializer(_deserializerProvider);

            // Act
            Product product = deserializer.Read(GetODataMessageReader(GetODataMessage(content), _edmModel),
                typeof(Product), _readContext) as Product;

            // Assert
            Assert.Equal(product.ID, 0);
            Assert.Equal(product.Rating, 4);
            Assert.Equal(product.Price, 2.5m);
            Assert.Equal(product.ReleaseDate, new DateTimeOffset(new DateTime(1992, 1, 1, 0, 0, 0), TimeSpan.Zero));
            Assert.Equal(product.PublishDate, new Date(1997, 7, 1));
            Assert.Null(product.DiscontinuedDate);
        }

        [Fact]
        public void ReadFromStreamAsync_ComplexTypeAndInlineData()
        {
            // Arrange
            string content = Resources.SupplierRequestEntry;
            ODataResourceDeserializer deserializer = new ODataResourceDeserializer(_deserializerProvider);

            var readContext = new ODataDeserializerContext
            {
                Path = new ODataPath(new EntitySetSegment(_edmModel.EntityContainer.FindEntitySet("Suppliers"))),
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
            var readContext = new ODataDeserializerContext
            {
                Path = new ODataPath(new EntitySetSegment(_edmModel.EntityContainer.FindEntitySet("Suppliers"))),
                Model = _edmModel,
                ResourceType = typeof(Delta<Supplier>)
            };

            ODataResourceDeserializer deserializer =
                new ODataResourceDeserializer(_deserializerProvider);

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

        [Fact(Skip = "TODO: Sam Xu")]
        public void Read_ThrowsOnUnknownEntityType()
        {
            // Arrange
            string content = Resources.SupplierRequestEntry;
            ODataResourceDeserializer deserializer = new ODataResourceDeserializer(_deserializerProvider);

            // Act & Assert
            Assert.Throws<ODataException>(() => deserializer.Read(GetODataMessageReader(GetODataMessage(content), _edmModel),
                typeof(Product), _readContext), "The property 'Concurrency' does not exist on type 'ODataDemo.Product'. Make sure to only use property names that are defined by the type.");
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

            public Date PublishDate { get; set; }

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
