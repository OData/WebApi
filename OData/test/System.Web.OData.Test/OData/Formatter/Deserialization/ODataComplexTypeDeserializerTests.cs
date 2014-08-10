// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.OData.Builder;
using System.Web.OData.Formatter.Serialization;
using System.Web.OData.Formatter.Serialization.Models;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;
using Moq;

namespace System.Web.OData.Formatter.Deserialization
{
    public class ODataComplexTypeDeserializerTests
    {
        private IEdmModel _edmModel = EdmTestHelpers.GetModel();
        private IEdmComplexTypeReference _addressEdmType = EdmTestHelpers.GetModel().GetEdmTypeReference(typeof(ODataEntityDeserializerTests.Address)).AsComplex();

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
            var address = deserializer.ReadComplexValue(complexValue, _addressEdmType, readContext) as ODataEntityDeserializerTests.Address;

            // Assert
            Assert.NotNull(address);
            Assert.Equal(address.Street, "12");
            Assert.Equal(address.City, "Redmond");
            Assert.Null(address.Country);
            Assert.Null(address.State);
            Assert.Null(address.ZipCode);
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

            var deserializerProvider = new Mock<ODataDeserializerProvider>().Object;
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
                    new ODataProperty { Name = "DateTimeProperty", Value = new DateTimeOffset(new DateTime(1992, 1, 1)) }
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
            Assert.Equal(3, address.Properties.Count());
            Assert.Equal(new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA"), address.Properties["GuidProperty"]);
            Assert.Equal(SimpleEnum.Third, address.Properties["EnumValue"]);
            Assert.Equal(new DateTimeOffset(new DateTime(1992, 1, 1)), address.Properties["DateTimeProperty"]);
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
            ODataDeserializerContext context = new ODataDeserializerContext { ResourceType = typeof(IEdmObject) };

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
            Assert.IsType<ODataEntityDeserializerTests.Address>(resource);
        }
    }
}
