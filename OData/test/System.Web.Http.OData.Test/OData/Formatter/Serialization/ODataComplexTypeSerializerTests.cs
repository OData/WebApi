// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Web.Http.OData.Formatter.Serialization.Models;
using System.Xml.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataComplexTypeSerializerTests
    {
        private IEdmModel _model;
        private Address _address;
        private ODataComplexTypeSerializer _serializer;
        private IEdmComplexType _addressType;
        private IEdmComplexTypeReference _addressTypeRef;

        public ODataComplexTypeSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();
            _address = new Address()
            {
                Street = "One Microsoft Way",
                City = "Redmond",
                State = "Washington",
                Country = "United States",
                ZipCode = "98052"
            };

            _addressType = _model.FindDeclaredType("Default.Address") as IEdmComplexType;
            _model.SetAnnotationValue(_addressType, new ClrTypeAnnotation(typeof(Address)));
            _addressTypeRef = _addressType.ToEdmTypeReference(isNullable: false).AsComplex();

            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            _serializer = new ODataComplexTypeSerializer(serializerProvider);
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_SerializerProvider()
        {
            Assert.ThrowsArgumentNull(() => new ODataComplexTypeSerializer(serializerProvider: null), "serializerProvider");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_MessageWriter()
        {
            Assert.ThrowsArgumentNull(
                () => _serializer.WriteObject(42, typeof(int), messageWriter: null, writeContext: new ODataSerializerContext()),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_WriteContext()
        {
            Assert.ThrowsArgumentNull(
                () => _serializer.WriteObject(42, typeof(int), ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObject_ThrowsArgument_WriteContext_RootElementNameMissing()
        {
            Assert.ThrowsArgument(
                () => _serializer.WriteObject(42, typeof(int), ODataTestUtil.GetMockODataMessageWriter(), new ODataSerializerContext()),
                "writeContext",
                "The 'RootElementName' property is required on 'ODataSerializerContext'.");
        }

        [Fact]
        public void WriteObject_Calls_CreateODataComplexValue()
        {
            // Arrange
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);
            ODataMessageWriter messageWriter = new ODataMessageWriter(message);
            Mock<ODataComplexTypeSerializer> serializer = new Mock<ODataComplexTypeSerializer>(new DefaultODataSerializerProvider());
            ODataSerializerContext writeContext = new ODataSerializerContext { RootElementName = "ComplexPropertyName", Model = _model };
            object graph = new object();
            ODataComplexValue complexValue = new ODataComplexValue
            {
                TypeName = "NS.Name",
                Properties = new[] { new ODataProperty { Name = "Property1", Value = 42 } }
            };

            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataComplexValue(graph, It.Is<IEdmComplexTypeReference>(e => e.Definition == _addressType), writeContext))
                .Returns(complexValue).Verifiable();

            // Act
            serializer.Object.WriteObject(graph, typeof(Address), messageWriter, writeContext);

            // Assert
            serializer.Verify();
            stream.Seek(0, SeekOrigin.Begin);
            XElement element = XElement.Load(stream);
            Assert.Equal("ComplexPropertyName", element.Name.LocalName);
            Assert.Equal("NS.Name", element.Attributes().Single(a => a.Name.LocalName == "type").Value);
            Assert.Equal(1, element.Descendants().Count());
            Assert.Equal("42", element.Descendants().Single().Value);
            Assert.Equal("Property1", element.Descendants().Single().Name.LocalName);
        }

        [Fact]
        public void CreateODataValue_Calls_CreateODataComplexValue()
        {
            // Arrange
            var oDataComplexValue = new ODataComplexValue();
            var complexObject = new object();
            Mock<ODataComplexTypeSerializer> serializer = new Mock<ODataComplexTypeSerializer>(new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer
                .Setup(s => s.CreateODataComplexValue(complexObject, _addressTypeRef, It.IsAny<ODataSerializerContext>()))
                .Returns(oDataComplexValue)
                .Verifiable();

            // Act
            ODataValue value = serializer.Object.CreateODataValue(complexObject, _addressTypeRef, new ODataSerializerContext());

            // Assert
            serializer.Verify();
            Assert.Same(oDataComplexValue, value);
        }

        [Fact]
        public void CreateODataComplexValue_ThrowsArgumentNull_WriteContext()
        {
            Assert.ThrowsArgumentNull(
                () => _serializer.CreateODataComplexValue(graph: 42, complexType: _addressTypeRef, writeContext: null),
                "writeContext");
        }

        [Fact]
        public void CreateODataComplexValue_ThrowsArgumentNull_Complextype()
        {
            Assert.ThrowsArgumentNull(
                () => _serializer.CreateODataComplexValue(graph: 42, complexType: null, writeContext: null),
                "writeContext");
        }

        [Fact]
        public void CreateODataComplexValue_WritesAllDeclaredProperties()
        {
            var odataValue = _serializer.CreateODataComplexValue(_address, _addressTypeRef, new ODataSerializerContext());

            ODataComplexValue complexValue = Assert.IsType<ODataComplexValue>(odataValue);

            Assert.Equal(complexValue.TypeName, "Default.Address");
            Assert.Equal(
                complexValue.Properties.Select(p => Tuple.Create(p.Name, p.Value as string)),
                new[] { 
                    Tuple.Create("Street","One Microsoft Way"), 
                    Tuple.Create("City","Redmond"),
                    Tuple.Create("State","Washington"),
                    Tuple.Create("Country", "United States"),
                    Tuple.Create("ZipCode","98052") });
        }

        [Fact]
        public void CreateODataComplexValue_ReturnsNull_ForNullValue()
        {
            var odataValue = _serializer.CreateODataComplexValue(null, _addressTypeRef, new ODataSerializerContext());

            Assert.Null(odataValue);
        }

        [Fact]
        public void CreateODataComplexValue_ReturnsNull_ForNullEdmComplexObject()
        {
            IEdmComplexTypeReference edmType = new EdmComplexTypeReference(_addressType, isNullable: true);
            ODataComplexTypeSerializer serializer = new ODataComplexTypeSerializer(new DefaultODataSerializerProvider());

            var result = serializer.CreateODataComplexValue(new NullEdmComplexObject(edmType), edmType, new ODataSerializerContext());

            Assert.Null(result);
        }

        [Fact]
        public void CreateODataComplexValue_ThrowsTypeCannotBeSerialized()
        {
            IEdmPrimitiveTypeReference stringType = EdmCoreModel.Instance.GetString(isNullable: true);
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(stringType)).Returns<ODataEdmTypeSerializer>(null);
            ODataComplexTypeSerializer serializer = new ODataComplexTypeSerializer(serializerProvider.Object);

            Assert.Throws<NotSupportedException>(
                () => serializer.CreateODataComplexValue(_address, _addressTypeRef, new ODataSerializerContext()),
                "'Edm.String' cannot be serialized using the ODataMediaTypeFormatter.");
        }

        [Fact]
        public void CreateODataComplexValue_Understands_IEdmComplexTypeObject()
        {
            // Arrange
            EdmComplexType complexEdmType = new EdmComplexType("NS", "ComplexType");
            complexEdmType.AddStructuralProperty("Property", EdmPrimitiveTypeKind.Int32);
            IEdmComplexTypeReference edmTypeReference = new EdmComplexTypeReference(complexEdmType, isNullable: false);

            TypedEdmComplexObject edmObject = new TypedEdmComplexObject(new { Property = 42 }, edmTypeReference);
            ODataComplexTypeSerializer serializer = new ODataComplexTypeSerializer(new DefaultODataSerializerProvider());

            // Act
            ODataComplexValue result = serializer.CreateODataComplexValue(edmObject, edmTypeReference, new ODataSerializerContext());

            // Assert
            Assert.Equal("Property", result.Properties.Single().Name);
            Assert.Equal(42, result.Properties.Single().Value);
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_DoesNotAddAnnotation_InDefaultMetadataMode()
        {
            // Arrange
            ODataComplexValue value = new ODataComplexValue();

            // Act
            ODataComplexTypeSerializer.AddTypeNameAnnotationAsNeeded(value, ODataMetadataLevel.Default);

            // Assert
            Assert.Null(value.GetAnnotation<SerializationTypeNameAnnotation>());
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_AddsAnnotation_InJsonLightMetadataMode()
        {
            // Arrange
            string expectedTypeName = "TypeName";
            ODataComplexValue value = new ODataComplexValue
            {
                TypeName = expectedTypeName
            };

            // Act
            ODataComplexTypeSerializer.AddTypeNameAnnotationAsNeeded(value, ODataMetadataLevel.FullMetadata);

            // Assert
            SerializationTypeNameAnnotation annotation = value.GetAnnotation<SerializationTypeNameAnnotation>();
            Assert.NotNull(annotation); // Guard
            Assert.Equal(expectedTypeName, annotation.TypeName);
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_AddsNullAnnotation_InJsonLightNoMetadataMode()
        {
            // Arrange
            string expectedTypeName = "TypeName";
            ODataComplexValue value = new ODataComplexValue
            {
                TypeName = expectedTypeName
            };

            // Act
            ODataComplexTypeSerializer.AddTypeNameAnnotationAsNeeded(value, ODataMetadataLevel.NoMetadata);

            // Assert
            SerializationTypeNameAnnotation annotation = value.GetAnnotation<SerializationTypeNameAnnotation>();
            Assert.NotNull(annotation); // Guard
            Assert.Null(annotation.TypeName);
        }

        [Theory]
        [InlineData(TestODataMetadataLevel.Default, false)]
        [InlineData(TestODataMetadataLevel.FullMetadata, true)]
        [InlineData(TestODataMetadataLevel.MinimalMetadata, false)]
        [InlineData(TestODataMetadataLevel.NoMetadata, true)]
        public void ShouldAddTypeNameAnnotation(TestODataMetadataLevel metadataLevel, bool expectedResult)
        {
            // Act
            bool actualResult = ODataComplexTypeSerializer.ShouldAddTypeNameAnnotation(
                (ODataMetadataLevel)metadataLevel);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData(TestODataMetadataLevel.FullMetadata, false)]
        [InlineData(TestODataMetadataLevel.NoMetadata, true)]
        public void ShouldSuppressTypeNameSerialization(TestODataMetadataLevel metadataLevel, bool expectedResult)
        {
            // Act
            bool actualResult = ODataComplexTypeSerializer.ShouldSuppressTypeNameSerialization(
                (ODataMetadataLevel)metadataLevel);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }
    }
}
