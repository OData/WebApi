// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.OData.Builder;
using System.Web.OData.Formatter.Serialization.Models;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;
using Moq;

namespace System.Web.OData.Formatter.Deserialization
{
    public class ODataComplexTypeDeserializerTests
    {
        private IEdmModel _edmModel = EdmTestHelpers.GetModel();
        private IEdmComplexTypeReference _addressEdmType = EdmTestHelpers.GetModel().GetEdmTypeReference(typeof(ODataResourceDeserializerTests.Address)).AsComplex();

        [Fact]
        public void Ctor_ThrowsArgumentNull_DeserializerProvider()
        {
            Assert.ThrowsArgumentNull(
                () => new ODataComplexTypeDeserializer(deserializerProvider: null),
                "deserializerProvider");
        }

        [Fact]
        public void ReadInline_ThrowsArgumentNull_ReadContext()
        {
            ODataComplexTypeDeserializer deserializer = new ODataComplexTypeDeserializer(new DefaultODataDeserializerProvider());
            Assert.ThrowsArgumentNull(
                () => deserializer.ReadInline(42, _addressEdmType, readContext: null),
                "readContext");
        }

        [Fact]
        public void ReadInline_Throws_ForNonODataComplexValues()
        {
            var deserializerProvider = new Mock<ODataDeserializerProvider>().Object;
            var deserializer = new ODataComplexTypeDeserializer(deserializerProvider);

            Assert.ThrowsArgument(
                () => deserializer.ReadInline(10, _addressEdmType, new ODataDeserializerContext()),
                "item");
        }

        [Fact]
        public void ReadInline_Throws_ArgumentMustBeOfType()
        {
            var deserializerProvider = new Mock<ODataDeserializerProvider>().Object;
            var deserializer = new ODataComplexTypeDeserializer(deserializerProvider);

            Assert.ThrowsArgument(
                () => deserializer.ReadInline(new ODataComplexValue(), new EdmEntityType("NS", "Name").AsReference(), new ODataDeserializerContext()),
                "edmType", "The argument must be of type 'Complex'.");
        }

        [Fact]
        public void ReadInline_ReturnsNull_IfItemIsNull()
        {
            ODataComplexTypeDeserializer deserializer = new ODataComplexTypeDeserializer(new DefaultODataDeserializerProvider());
            Assert.Null(deserializer.ReadInline(item: null, edmType: _addressEdmType, readContext: new ODataDeserializerContext()));
        }

        [Fact]
        public void ReadInline_Calls_ReadComplexValue()
        {
            // Arrange
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            Mock<ODataComplexTypeDeserializer> deserializer = new Mock<ODataComplexTypeDeserializer>(deserializerProvider);
            ODataComplexValue item = new ODataComplexValue();
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            deserializer.CallBase = true;
            deserializer.Setup(d => d.ReadComplexValue(item, _addressEdmType, readContext)).Returns(42).Verifiable();

            // Act
            object result = deserializer.Object.ReadInline(item, _addressEdmType, readContext);

            // Assert
            deserializer.Verify();
            Assert.Equal(42, result);
        }

        [Fact]
        public void ReadComplexValue_ThrowsArgumentNull_ComplexValue()
        {
            ODataComplexTypeDeserializer deserializer = new ODataComplexTypeDeserializer(new DefaultODataDeserializerProvider());

            Assert.ThrowsArgumentNull(
                () => deserializer.ReadComplexValue(complexValue: null, complexType: _addressEdmType, readContext: new ODataDeserializerContext()),
                "complexValue");
        }

        [Fact]
        public void ReadComplexValue_ThrowsArgumentNull_ReadContext()
        {
            ODataComplexTypeDeserializer deserializer = new ODataComplexTypeDeserializer(new DefaultODataDeserializerProvider());

            Assert.ThrowsArgumentNull(
                () => deserializer.ReadComplexValue(new ODataComplexValue(), _addressEdmType, readContext: null),
                "readContext");
        }

        [Fact]
        public void ReadComplexValue_ThrowsArgument_ModelMissingFromReadContext()
        {
            ODataComplexTypeDeserializer deserializer = new ODataComplexTypeDeserializer(new DefaultODataDeserializerProvider());

            Assert.ThrowsArgument(
                () => deserializer.ReadComplexValue(new ODataComplexValue(), _addressEdmType, readContext: new ODataDeserializerContext()),
                "readContext",
                "The EDM model is missing on the read context. The model is required on the read context to deserialize the payload.");
        }

        [Fact]
        public void ReadComplexValue_CanReadComplexValue()
        {
            // Arrange
            var deserializerProvider = new Mock<ODataDeserializerProvider>().Object;
            var deserializer = new ODataComplexTypeDeserializer(deserializerProvider);

            ODataComplexValue complexValue = new ODataComplexValue
            {
                Properties = new[]
                { 
                    new ODataProperty { Name = "Street", Value = "12"},
                    new ODataProperty { Name = "City", Value = "Redmond"}
                },
                TypeName = "ODataDemo.Address"
            };
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = _edmModel };

            // Act
            var address = deserializer.ReadComplexValue(complexValue, _addressEdmType, readContext) as ODataResourceDeserializerTests.Address;

            // Assert
            Assert.NotNull(address);
            Assert.Equal(address.Street, "12");
            Assert.Equal(address.City, "Redmond");
            Assert.Null(address.Country);
            Assert.Null(address.State);
            Assert.Null(address.ZipCode);
        }

        public class MyAddress
        {
            [Column(TypeName = "date")]
            public DateTime CreatedDay { get; set; }

            [Column(TypeName = "time")]
            public TimeSpan EndTime { get; set; }
        }

        [Fact]
        public void ReadComplexValue_CanReadDateTimeRelatedProperties()
        {
            // Arrange
            var builder = new ODataConventionModelBuilder();
            builder.ComplexType<MyAddress>().Namespace = "NS";
            IEdmModel model = builder.GetEdmModel();
            var addressEdmType = model.GetEdmTypeReference(typeof(MyAddress)).AsComplex();

            var deserializerProvider = new Mock<ODataDeserializerProvider>().Object;
            var deserializer = new ODataComplexTypeDeserializer(deserializerProvider);

            ODataComplexValue complexValue = new ODataComplexValue
            {
                Properties = new[]
                { 
                    new ODataProperty { Name = "CreatedDay", Value = new Date(2015, 12, 12)},
                    new ODataProperty { Name = "EndTime", Value = new TimeOfDay(1, 2, 3, 4)}
                },
                TypeName = "NS.MyAddress"
            };
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = model };

            // Act
            var address = deserializer.ReadComplexValue(complexValue, addressEdmType, readContext) as MyAddress;

            // Assert
            Assert.NotNull(address);
            Assert.Equal(new DateTime(2015, 12, 12), address.CreatedDay);
            Assert.Equal(new TimeSpan(0, 1, 2, 3, 4), address.EndTime);
        }

        [Fact]
        public void ReadComplexValue_CanReadDerivedComplexValue()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<Address>();
            IEdmModel model = builder.GetEdmModel();
            IEdmComplexTypeReference addressEdmType = model.GetEdmTypeReference(typeof(Address)).AsComplex();

            DefaultODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            ODataComplexTypeDeserializer deserializer = new ODataComplexTypeDeserializer(deserializerProvider);

            ODataComplexValue complexValue = new ODataComplexValue
            {
                Properties = new[]
                { 
                    new ODataProperty { Name = "Street", Value = "12"},
                    new ODataProperty { Name = "City", Value = "Redmond"},
                    new ODataProperty { Name = "UsProp", Value = "UsPropertyValue"}
                },
                TypeName = typeof(UsAddress).FullName
            };
            ODataDeserializerContext readContext = new ODataDeserializerContext { Model = model };

            // Act
            object address = deserializer.ReadComplexValue(complexValue, addressEdmType, readContext);

            // Assert
            Assert.NotNull(address);
            UsAddress usAddress = Assert.IsType<UsAddress>(address);

            Assert.Equal(usAddress.Street, "12");
            Assert.Equal(usAddress.City, "Redmond");
            Assert.Null(usAddress.Country);
            Assert.Null(usAddress.State);
            Assert.Null(usAddress.ZipCode);
            Assert.Equal("UsPropertyValue", usAddress.UsProp);
        }

        [Fact]
        public void ReadComplexValue_CanReadDynamicPropertiesForOpenComplexType()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<SimpleOpenAddress>();
            builder.EnumType<SimpleEnum>();
            IEdmModel model = builder.GetEdmModel();
            IEdmComplexTypeReference addressTypeReference = model.GetEdmTypeReference(typeof(SimpleOpenAddress)).AsComplex();

            var deserializerProvider = new DefaultODataDeserializerProvider();
            var deserializer = new ODataComplexTypeDeserializer(deserializerProvider);

            ODataEnumValue enumValue = new ODataEnumValue("Third", typeof(SimpleEnum).FullName);

            ODataComplexValue complexValue = new ODataComplexValue
            {
                Properties = new[]
                {
                    // declared properties
                    new ODataProperty { Name = "Street", Value = "My Way #599" },
                    new ODataProperty { Name = "City", Value = "Redmond & Shanghai" },

                    // dynamic properties
                    new ODataProperty { Name = "GuidProperty", Value = new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA") },
                    new ODataProperty { Name = "EnumValue", Value = enumValue },
                    new ODataProperty { Name = "DateTimeProperty", Value = new DateTimeOffset(new DateTime(1992, 1, 1)) },
                    new ODataProperty { Name = "DateProperty", Value = new Date(1997, 7, 1)}
                },
                TypeName = typeof(SimpleOpenAddress).FullName
            };

            ODataDeserializerContext readContext = new ODataDeserializerContext()
            {
                Model = model
            };

            // Act
            SimpleOpenAddress address = deserializer.ReadComplexValue(complexValue, addressTypeReference, readContext)
                as SimpleOpenAddress;

            // Assert
            Assert.NotNull(address);

            // Verify the declared properties
            Assert.Equal("My Way #599", address.Street);
            Assert.Equal("Redmond & Shanghai", address.City);

            // Verify the dynamic properties
            Assert.NotNull(address.Properties);
            Assert.Equal(4, address.Properties.Count());
            Assert.Equal(new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA"), address.Properties["GuidProperty"]);
            Assert.Equal(SimpleEnum.Third, address.Properties["EnumValue"]);
            Assert.Equal(new DateTimeOffset(new DateTime(1992, 1, 1)), address.Properties["DateTimeProperty"]);
            Assert.Equal(new Date(1997, 7, 1), address.Properties["DateProperty"]);
        }

        [Fact]
        public void ReadComplexValue_CanReadNestedOpenComplexType()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<SimpleOpenAddress>();
            builder.ComplexType<SimpleOpenZipCode>();

            IEdmModel model = builder.GetEdmModel();
            IEdmComplexTypeReference addressTypeReference = model.GetEdmTypeReference(typeof(SimpleOpenAddress)).AsComplex();

            var deserializerProvider = new DefaultODataDeserializerProvider();
            var deserializer = new ODataComplexTypeDeserializer(deserializerProvider);

            ODataComplexValue zipCodeComplexValue = new ODataComplexValue
            {
                Properties = new[]
                {
                    // declared property
                    new ODataProperty { Name = "Code", Value = 101 },

                    // dynamic property
                    new ODataProperty { Name = "DateTimeProperty", Value = new DateTimeOffset(new DateTime(2014, 4, 22)) }
                },
                TypeName = typeof(SimpleOpenZipCode).FullName
            };

            ODataComplexValue addressComplexValue = new ODataComplexValue
            {
                Properties = new[]
                {
                    // declared properties
                    new ODataProperty { Name = "Street", Value = "TopStreet" },
                    new ODataProperty { Name = "City", Value = "TopCity" },

                    // dynamic properties
                    new ODataProperty { Name = "DoubleProperty", Value = 1.179 },
                    new ODataProperty { Name = "ZipCodeProperty", Value = zipCodeComplexValue }
                },
                TypeName = typeof(SimpleOpenAddress).FullName
            };

            ODataDeserializerContext readContext = new ODataDeserializerContext()
            {
                Model = model
            };

            // Act
            SimpleOpenAddress address = deserializer.ReadComplexValue(addressComplexValue, addressTypeReference, readContext)
                as SimpleOpenAddress;

            // Assert
            Assert.NotNull(address);

            // Verify the declared properties
            Assert.Equal("TopStreet", address.Street);
            Assert.Equal("TopCity", address.City);

            // Verify the dynamic properties
            Assert.NotNull(address.Properties);
            Assert.Equal(2, address.Properties.Count());
            
            Assert.Equal(1.179, address.Properties["DoubleProperty"]);
            
            // nested open complex type
            SimpleOpenZipCode zipCode = Assert.IsType<SimpleOpenZipCode>(address.Properties["ZipCodeProperty"]);
            Assert.Equal(101, zipCode.Code);
            Assert.Equal(1, zipCode.Properties.Count());
            Assert.Equal(new DateTimeOffset(new DateTime(2014, 4, 22)), zipCode.Properties["DateTimeProperty"]);
        }

        [Fact]
        public void ReadComplexValue_CanReadDynamicCollectionPropertiesForOpenComplexType()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<SimpleOpenAddress>();
            builder.EnumType<SimpleEnum>();
            IEdmModel model = builder.GetEdmModel();
            IEdmComplexTypeReference addressTypeReference =
                model.GetEdmTypeReference(typeof(SimpleOpenAddress)).AsComplex();

            var deserializerProvider = new DefaultODataDeserializerProvider();
            var deserializer = new ODataComplexTypeDeserializer(deserializerProvider);

            ODataEnumValue enumValue = new ODataEnumValue("Third", typeof(SimpleEnum).FullName);

            ODataCollectionValue collectionValue = new ODataCollectionValue
            {
                TypeName = "Collection(" + typeof(SimpleEnum).FullName + ")",
                Items = new[] { enumValue, enumValue }
            };

            ODataComplexValue complexValue = new ODataComplexValue
            {
                Properties = new[]
                {
                    // declared properties
                    new ODataProperty { Name = "Street", Value = "My Way #599" },

                    // dynamic properties
                    new ODataProperty { Name = "CollectionProperty", Value = collectionValue }
                },
                TypeName = typeof(SimpleOpenAddress).FullName
            };

            ODataDeserializerContext readContext = new ODataDeserializerContext()
            {
                Model = model
            };

            // Act
            SimpleOpenAddress address = deserializer.ReadComplexValue(complexValue, addressTypeReference, readContext)
                as SimpleOpenAddress;

            // Assert
            Assert.NotNull(address);

            // Verify the declared properties
            Assert.Equal("My Way #599", address.Street);
            Assert.Null(address.City);

            // Verify the dynamic properties
            Assert.NotNull(address.Properties);
            Assert.Equal(1, address.Properties.Count());

            var collectionValues = Assert.IsType<List<SimpleEnum>>(address.Properties["CollectionProperty"]);
            Assert.NotNull(collectionValues);
            Assert.Equal(2, collectionValues.Count());
            Assert.Equal(SimpleEnum.Third, collectionValues[0]);
            Assert.Equal(SimpleEnum.Third, collectionValues[1]);
        }

        [Fact]
        public void ReadComplexValue_Throws_IfDuplicateDynamicPropertyNameFound()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<SimpleOpenAddress>();
            IEdmModel model = builder.GetEdmModel();
            IEdmComplexTypeReference addressTypeReference = model.GetEdmTypeReference(typeof(SimpleOpenAddress)).AsComplex();

            var deserializerProvider = new Mock<ODataDeserializerProvider>().Object;
            var deserializer = new ODataComplexTypeDeserializer(deserializerProvider);

            ODataComplexValue complexValue = new ODataComplexValue
            {
                Properties = new[]
                {
                    // declared properties
                    new ODataProperty { Name = "Street", Value = "My Way #599" },
                    new ODataProperty { Name = "City", Value = "Redmond & Shanghai" },

                    // dynamic properties
                    new ODataProperty { Name = "GuidProperty", Value = new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA") },
                    new ODataProperty { Name = "GuidProperty", Value = new DateTimeOffset(new DateTime(1992, 1, 1)) }
                },
                TypeName = typeof(SimpleOpenAddress).FullName
            };

            ODataDeserializerContext readContext = new ODataDeserializerContext()
            {
                Model = model
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                deserializer.ReadComplexValue(complexValue, addressTypeReference, readContext),
                "Duplicate dynamic property name 'GuidProperty' found in open type 'System.Web.OData.SimpleOpenAddress'. " +
                "Each dynamic property name must be unique.");
        }

        [Fact]
        public void CreateResource_ThrowsArgumentNull_ComplexType()
        {
            Assert.ThrowsArgumentNull(
                () => ODataComplexTypeDeserializer.CreateResource(complexType: null, readContext: new ODataDeserializerContext()),
                "complexType");
        }

        [Fact]
        public void CreateResource_ThrowsArgumentNull_ReadContext()
        {
            Assert.ThrowsArgumentNull(
                () => ODataComplexTypeDeserializer.CreateResource(_addressEdmType, readContext: null),
                "readContext");
        }

        [Fact]
        public void CreateResource_ThrowsArgument_ModelMissingFromReadContext()
        {
            Assert.ThrowsArgument(
                () => ODataComplexTypeDeserializer.CreateResource(_addressEdmType, new ODataDeserializerContext()),
                "readContext",
                "The EDM model is missing on the read context. The model is required on the read context to deserialize the payload.");
        }

        [Fact]
        public void CreateResource_Throws_MappingDoesNotContainEntityType()
        {
            Assert.Throws<InvalidOperationException>(
                () => ODataComplexTypeDeserializer.CreateResource(_addressEdmType, new ODataDeserializerContext { Model = EdmCoreModel.Instance }),
                "The provided mapping does not contain an entry for the entity type 'ODataDemo.Address'.");
        }

        [Fact]
        public void CreateResource_CreatesEdmComplexObject_UnTypedMode()
        {
            // Arrange
            ODataDeserializerContext context = new ODataDeserializerContext { ResourceType = typeof(IEdmObject), Model = _edmModel };

            // Act
            var resource = ODataComplexTypeDeserializer.CreateResource(_addressEdmType, context);

            // Assert
            EdmComplexObject complexObject = Assert.IsType<EdmComplexObject>(resource);
            Assert.Equal(_addressEdmType, complexObject.GetEdmType(), new EdmTypeReferenceEqualityComparer());
        }

        [Fact]
        public void CreateResource_CreatesAddress_TypedMode()
        {
            // Arrange
            ODataDeserializerContext context = new ODataDeserializerContext { Model = _edmModel };

            // Act
            var resource = ODataComplexTypeDeserializer.CreateResource(_addressEdmType, context);

            // Assert
            Assert.IsType<ODataResourceDeserializerTests.Address>(resource);
        }

        [Fact]
        public void CreateResource_CreatesDeltaOfT_IfPatchMode()
        {
            // Arrange
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _edmModel,
                ResourceType = typeof(Delta<ODataResourceDeserializerTests.Address>)
            };

            // Act & Assert
            Assert.IsType<Delta<ODataResourceDeserializerTests.Address>>(
                ODataComplexTypeDeserializer.CreateResource(_addressEdmType, readContext));
        }

        [Fact]
        public void CreateResource_CreatesDeltaWith_ExpectedUpdatableProperties()
        {
            // Arrange
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _edmModel,
                ResourceType = typeof(Delta<ODataResourceDeserializerTests.Address>)
            };
            var structuralProperties = _addressEdmType.StructuralProperties().Select(p => p.Name);

            // Act
            Delta<ODataResourceDeserializerTests.Address> resource =
                ODataComplexTypeDeserializer.CreateResource(_addressEdmType, readContext) as
                    Delta<ODataResourceDeserializerTests.Address>;

            // Assert
            Assert.NotNull(resource);
            Assert.Equal(structuralProperties, resource.GetUnchangedPropertyNames());
        }

        [Fact]
        public void CreateResource_CreateDeltaWith_OpenComplexType()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<SimpleOpenAddress>();
            IEdmModel model = builder.GetEdmModel();
            IEdmComplexTypeReference addressTypeReference = model.GetEdmTypeReference(typeof(SimpleOpenAddress)).AsComplex();

            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = model,
                ResourceType = typeof(Delta<SimpleOpenAddress>)
            };
            var structuralProperties = addressTypeReference.StructuralProperties().Select(p => p.Name);

            // Act
            Delta<SimpleOpenAddress> resource =
                ODataComplexTypeDeserializer.CreateResource(addressTypeReference, readContext) as
                    Delta<SimpleOpenAddress>;

            // Assert
            Assert.NotNull(resource);
            Assert.Equal(structuralProperties, resource.GetUnchangedPropertyNames());
        }

        [Fact]
        public void ReadFromStreamAsync()
        {
            // Arrange
            const string content = "{\"value\":{" +
              "\"Street\":\"MyStreet\"," +
              "\"City\":\"MyCity\"," +
              "\"State\":\"MyState\"," +
              "\"ZipCode\":\"160202\"," +
              "\"Country\":\"MyCountry\"" +
            "}}";

            ODataComplexTypeDeserializer deserializer = new ODataComplexTypeDeserializer(new DefaultODataDeserializerProvider());
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _edmModel,
                ResourceType = typeof(ODataResourceDeserializerTests.Address)
            };

            // Act
            object value = deserializer.Read(GetODataMessageReader(GetODataMessage(content), _edmModel),
                typeof(ODataResourceDeserializerTests.Address), readContext);

            // Assert

            ODataResourceDeserializerTests.Address address = Assert.IsType<ODataResourceDeserializerTests.Address>(value);
            Assert.NotNull(address);

            Assert.Equal("MyStreet", address.Street);
            Assert.Equal("MyCity", address.City);
            Assert.Equal("MyState", address.State);
            Assert.Equal("160202", address.ZipCode);
            Assert.Equal("MyCountry", address.Country);
        }

        [Fact]
        public void ReadFromStreamAsync_ForOpenComplexType()
        {
            // Arrange
            const string content = "{\"value\":{" +
              "\"Street\":\"MyStreet\"," +
              "\"City\":\"MyCity\"," +
              "\"Publish@odata.type\":\"#Date\"," +
              "\"Publish\":\"2016-02-22\"" +
            "}}";

            ODataComplexTypeDeserializer deserializer = new ODataComplexTypeDeserializer(new DefaultODataDeserializerProvider());
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<SimpleOpenAddress>();
            IEdmModel model = builder.GetEdmModel();

            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = model,
                ResourceType = typeof(SimpleOpenAddress)
            };

            // Act
            object value = deserializer.Read(GetODataMessageReader(GetODataMessage(content), model), typeof(SimpleOpenAddress), readContext);

            // Assert

            SimpleOpenAddress address = Assert.IsType<SimpleOpenAddress>(value);
            Assert.NotNull(address);

            Assert.Equal("MyStreet", address.Street);
            Assert.Equal("MyCity", address.City);
            Assert.NotNull(address.Properties);

            KeyValuePair<string, object> dynamicProperty = Assert.Single(address.Properties);
            Assert.Equal("Publish", dynamicProperty.Key);
            Assert.Equal(new Date(2016, 2, 22), dynamicProperty.Value);
        }

        [Fact]
        public void ReadFromStreamAsync_ForOpenComplexType_ForPatchModel()
        {
            // Arrange
            const string content = "{\"value\":{" +
              "\"Street\":\"UpdateStreet\"," +
              "\"Publish@odata.type\":\"#Date\"," +
              "\"Publish\":\"2016-02-22\"" +
            "}}";

            ODataComplexTypeDeserializer deserializer = new ODataComplexTypeDeserializer(new DefaultODataDeserializerProvider());
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<SimpleOpenAddress>();
            IEdmModel model = builder.GetEdmModel();

            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = model,
                ResourceType = typeof(Delta<SimpleOpenAddress>)
            };

            // Act
            object value = deserializer.Read(GetODataMessageReader(GetODataMessage(content), model),
                typeof(Delta<SimpleOpenAddress>), readContext);

            // Assert
            Delta<SimpleOpenAddress> address = Assert.IsType<Delta<SimpleOpenAddress>>(value);
            Assert.NotNull(address);
            Assert.Equal(new[] { "Street" }, address.GetChangedPropertyNames());
            Assert.Equal(new[] { "City" }, address.GetUnchangedPropertyNames());

            SimpleOpenAddress origin = new SimpleOpenAddress();
            Assert.Null(origin.Street); // guard
            Assert.Null(origin.City); // guard
            Assert.Null(origin.Properties); // guard

            address.Patch(origin); // DO PATCH

            Assert.Equal("UpdateStreet", origin.Street);
            Assert.Null(origin.City); // not changed
            KeyValuePair<string, object> dynamicProperty = Assert.Single(origin.Properties);
            Assert.Equal("Publish", dynamicProperty.Key);
            Assert.Equal(new Date(2016, 2, 22), dynamicProperty.Value);
        }

        public class Region
        {
            public string City { get; set; }
            public Location Location { get; set; }
            public IDictionary<string, object> Properties { get; set; }
        }

        public class Location
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        [Fact]
        public void ReadFromStreamAsync_ForComplexType_WithNestedComplexType()
        {
            // Arrange
            const string content = "{\"value\":{" +
              "\"City\":\"UpdatedCity\"," +
              "\"Location\": {" +
                  "\"Latitude\": 30.6," +
                  "\"Longitude\": 101.313" +
                  "}," +
              "\"SubLocation\": {" + // dynamic property
                  "\"@odata.type\":\"#System.Web.OData.Formatter.Deserialization.Location\"," +
                  "\"Latitude\": 15.5," +
                  "\"Longitude\": 130.88" +
                  "}" +
            "}}";

            ODataComplexTypeDeserializer deserializer = new ODataComplexTypeDeserializer(new DefaultODataDeserializerProvider());
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<Region>();
            IEdmModel model = builder.GetEdmModel();

            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = model,
                ResourceType = typeof(Delta<Region>)
            };

            // Act
            object value = deserializer.Read(GetODataMessageReader(GetODataMessage(content), model),
                typeof(Delta<Region>), readContext);

            // Assert
            Delta<Region> region = Assert.IsType<Delta<Region>>(value);
            Assert.NotNull(region);
            Assert.Equal(new[] { "City", "Location" }, region.GetChangedPropertyNames());
            Assert.Empty(region.GetUnchangedPropertyNames());

            object propertyValue;
            Assert.True(region.TryGetPropertyValue("City", out propertyValue));
            string cityValue = Assert.IsType<string>(propertyValue);
            Assert.Equal("UpdatedCity", cityValue);

            Assert.True(region.TryGetPropertyValue("Location", out propertyValue));
            Location locationValue = Assert.IsType<Location>(propertyValue);
            Assert.Equal(30.6, locationValue.Latitude);
            Assert.Equal(101.313, locationValue.Longitude);

            // dynamic property
            Assert.True(region.TryGetPropertyValue("SubLocation", out propertyValue));
            locationValue = Assert.IsType<Location>(propertyValue);
            Assert.Equal(15.5, locationValue.Latitude);
            Assert.Equal(130.88, locationValue.Longitude);
        }

        [Fact]
        public void ReadFromStreamAsync_ForDerivedComplexType()
        {
            // Arrange
            const string content = "{" +
              "\"@odata.type\":\"System.Web.OData.Formatter.Serialization.Models.CnAddress\"," +
              "\"Street\":\"StreetValue\"," +
              "\"City\":\"CityValue\"," +
              "\"State\":\"MyState\"," +
              "\"ZipCode\":\"160202\"," +
              "\"Country\":\"MyCountry\"," +
              "\"CnProp\":\"8E8375AA-D348-49DD-94A0-46E4FB42973C\"" +
            "}";

            ODataComplexTypeDeserializer deserializer = new ODataComplexTypeDeserializer(new DefaultODataDeserializerProvider());
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<Address>();
            IEdmModel model = builder.GetEdmModel();

            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = model,
                ResourceType = typeof(Address)
            };

            // Act
            object value = deserializer.Read(GetODataMessageReader(GetODataMessage(content), model), typeof(Address), readContext);

            // Assert
            CnAddress address = Assert.IsType<CnAddress>(value);
            Assert.NotNull(address);
            Assert.Equal("StreetValue", address.Street);
            Assert.Equal("CityValue", address.City);
            Assert.Equal("MyState", address.State);
            Assert.Equal("160202", address.ZipCode);
            Assert.Equal("MyCountry", address.Country);
            Assert.Equal(new Guid("8E8375AA-D348-49DD-94A0-46E4FB42973C"), address.CnProp);
        }

        private static ODataMessageReader GetODataMessageReader(IODataRequestMessage oDataRequestMessage, IEdmModel edmModel)
        {
            return new ODataMessageReader(oDataRequestMessage, new ODataMessageReaderSettings(), edmModel);
        }

        private static IODataRequestMessage GetODataMessage(string content)
        {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("Patch"), "http://localhost/OData/Suppliers(1)/Address");

            request.Content = new StringContent(content);
            request.Headers.Add("OData-Version", "4.0");

            MediaTypeWithQualityHeaderValue mediaType = new MediaTypeWithQualityHeaderValue("application/json");
            mediaType.Parameters.Add(new NameValueHeaderValue("odata.metadata", "full"));
            request.Headers.Accept.Add(mediaType);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return new HttpRequestODataMessage(request);
        }
    }
}
