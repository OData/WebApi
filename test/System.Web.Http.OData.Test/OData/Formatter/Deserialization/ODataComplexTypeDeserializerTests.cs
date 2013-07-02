// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataComplexTypeDeserializerTests
    {
        private IEdmModel _edmModel = EdmTestHelpers.GetModel();
        private IEdmComplexTypeReference _addressEdmType = EdmTestHelpers.GetModel().GetEdmTypeReference(typeof(ODataEntityDeserializerTests.Address)).AsComplex();

        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmType()
        {
            Assert.ThrowsArgumentNull(
                () => new ODataComplexTypeDeserializer(edmType: null, deserializerProvider: new DefaultODataDeserializerProvider()),
                "edmType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_DeserializerProvider()
        {
            Assert.ThrowsArgumentNull(
                () => new ODataComplexTypeDeserializer(_addressEdmType, deserializerProvider: null),
                "deserializerProvider");
        }

        [Fact]
        public void Ctor_SetsProperty_ComplexType()
        {
            IEdmComplexTypeReference complexType = new Mock<IEdmComplexTypeReference>().Object;
            ODataComplexTypeDeserializer deserializer = new ODataComplexTypeDeserializer(complexType, new DefaultODataDeserializerProvider());
            Assert.Equal(deserializer.ComplexType, complexType);
        }

        [Fact]
        public void Constructor_Succeeds_ForValidComplexType()
        {
            var deserializerProvider = new Mock<ODataDeserializerProvider>().Object;
            var deserializer = new ODataComplexTypeDeserializer(_addressEdmType, deserializerProvider);

            Assert.Equal(deserializer.DeserializerProvider, deserializerProvider);
            Assert.Equal(deserializer.ComplexType.Definition, EdmTestHelpers.GetEdmType("ODataDemo.Address"));
            Assert.Equal(deserializer.ODataPayloadKind, ODataPayloadKind.Property);
        }

        [Fact]
        public void ReadInline_ThrowsArgumentNull_ReadContext()
        {
            ODataComplexTypeDeserializer deserializer = new ODataComplexTypeDeserializer(_addressEdmType, new DefaultODataDeserializerProvider());
            Assert.ThrowsArgumentNull(
                () => deserializer.ReadInline(42, readContext: null),
                "readContext");
        }

        [Fact]
        public void ReadInline_Throws_ForNonODataComplexValues()
        {
            var deserializerProvider = new Mock<ODataDeserializerProvider>().Object;
            var deserializer = new ODataComplexTypeDeserializer(_addressEdmType, deserializerProvider);

            Assert.ThrowsArgument(
                () => deserializer.ReadInline(10, new ODataDeserializerContext()),
                "item");
        }

        [Fact]
        public void ReadInline_ReturnsNull_IfItemIsNull()
        {
            ODataComplexTypeDeserializer deserializer = new ODataComplexTypeDeserializer(_addressEdmType, new DefaultODataDeserializerProvider());
            Assert.Null(deserializer.ReadInline(item: null, readContext: new ODataDeserializerContext()));
        }

        [Fact]
        public void ReadInline_Calls_ReadComplexValue()
        {
            // Arrange
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            Mock<ODataComplexTypeDeserializer> deserializer = new Mock<ODataComplexTypeDeserializer>(_addressEdmType, deserializerProvider);
            ODataComplexValue item = new ODataComplexValue();
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            deserializer.CallBase = true;
            deserializer.Setup(d => d.ReadComplexValue(item, readContext)).Returns(42).Verifiable();

            // Act
            object result = deserializer.Object.ReadInline(item, readContext);

            // Assert
            deserializer.Verify();
            Assert.Equal(42, result);
        }

        [Fact]
        public void ReadComplexValue_ThrowsArgumentNull_ComplexValue()
        {
            ODataComplexTypeDeserializer deserializer = new ODataComplexTypeDeserializer(_addressEdmType, new DefaultODataDeserializerProvider());

            Assert.ThrowsArgumentNull(
                () => deserializer.ReadComplexValue(complexValue: null, readContext: new ODataDeserializerContext()),
                "complexValue");
        }

        [Fact]
        public void ReadComplexValue_ThrowsArgumentNull_ReadContext()
        {
            ODataComplexTypeDeserializer deserializer = new ODataComplexTypeDeserializer(_addressEdmType, new DefaultODataDeserializerProvider());

            Assert.ThrowsArgumentNull(
                () => deserializer.ReadComplexValue(new ODataComplexValue(), readContext: null),
                "readContext");
        }

        [Fact]
        public void ReadComplexValue_ThrowsArgument_ModelMissingFromReadContext()
        {
            ODataComplexTypeDeserializer deserializer = new ODataComplexTypeDeserializer(_addressEdmType, new DefaultODataDeserializerProvider());

            Assert.ThrowsArgument(
                () => deserializer.ReadComplexValue(new ODataComplexValue(), readContext: new ODataDeserializerContext()),
                "readContext",
                "The EDM model is missing on the read context. The model is required on the read context to deserialize the payload.");
        }

        [Fact]
        public void ReadComplexValue_CanReadComplexValue()
        {
            // Arrange
            var deserializerProvider = new Mock<ODataDeserializerProvider>().Object;
            var deserializer = new ODataComplexTypeDeserializer(_addressEdmType, deserializerProvider);

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
            var address = deserializer.ReadComplexValue(complexValue, readContext) as ODataEntityDeserializerTests.Address;

            // Assert
            Assert.NotNull(address);
            Assert.Equal(address.Street, "12");
            Assert.Equal(address.City, "Redmond");
            Assert.Null(address.Country);
            Assert.Null(address.State);
            Assert.Null(address.ZipCode);
        }

        [Fact]
        public void CreateResource_Throws_MappingDoesNotContainEntityType()
        {
            Assert.Throws<InvalidOperationException>(
                () => ODataComplexTypeDeserializer.CreateResource(_addressEdmType, new ODataDeserializerContext { Model = EdmCoreModel.Instance }),
                "The provided mapping doesn't contain an entry for the entity type 'ODataDemo.Address'.");
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
