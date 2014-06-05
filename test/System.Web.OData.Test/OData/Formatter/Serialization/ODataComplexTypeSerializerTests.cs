// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.OData.Builder;
using System.Web.OData.Formatter.Serialization.Models;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;
using Moq;

namespace System.Web.OData.Formatter.Serialization
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

            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/"), }
            };
            settings.SetContentType(ODataFormat.Json);

            ODataMessageWriter messageWriter = new ODataMessageWriter(message, settings);
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
            string result = new StreamReader(stream).ReadToEnd();
            Assert.Equal("{\"@odata.context\":\"http://any/$metadata#NS.Name\",\"Property1\":42}", result);
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
        public void CreateODataComplexValue_WritesAllDeclaredAndDynamicProperties_ForOpenComplexType()
        {
            // Arrange
            IEdmModel model = SerializationTestsHelpers.SimpleOpenTypeModel();

            IEdmComplexType addressType = model.FindDeclaredType("Default.Address") as IEdmComplexType;
            Type simpleOpenAddress = typeof(SimpleOpenAddress);
            model.SetAnnotationValue<ClrTypeAnnotation>(addressType, new ClrTypeAnnotation(simpleOpenAddress));

            IEdmEnumType enumType = model.FindDeclaredType("Default.SimpleEnum") as IEdmEnumType;
            Type simpleEnumType = typeof(SimpleEnum);
            model.SetAnnotationValue<ClrTypeAnnotation>(enumType, new ClrTypeAnnotation(simpleEnumType));

            model.SetAnnotationValue(addressType, new DynamicPropertyDictionaryAnnotation(
                simpleOpenAddress.GetProperty("Properties")));

            IEdmComplexTypeReference addressTypeRef = addressType.ToEdmTypeReference(isNullable: false).AsComplex();

            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            ODataComplexTypeSerializer serializer = new ODataComplexTypeSerializer(serializerProvider);
            ODataSerializerContext context = new ODataSerializerContext
            {
                Model = model
            };

            SimpleOpenAddress address = new SimpleOpenAddress()
            {
                Street = "My Way",
                City = "Redmond",
                Properties = new Dictionary<string, object>()
            };
            address.Properties.Add("EnumProperty", SimpleEnum.Fourth);
            address.Properties.Add("GuidProperty", new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA"));
            address.Properties.Add("DoubleProperty", 99.109);

            // Act
            var odataValue = serializer.CreateODataComplexValue(address, addressTypeRef, context);

            // Assert
            ODataComplexValue complexValue = Assert.IsType<ODataComplexValue>(odataValue);

            Assert.Equal(complexValue.TypeName, "Default.Address");
            Assert.Equal(5, complexValue.Properties.Count());

            // Verify the declared properties
            ODataProperty street = Assert.Single(complexValue.Properties.Where(p => p.Name == "Street"));
            Assert.Equal("My Way", street.Value);

            ODataProperty city = Assert.Single(complexValue.Properties.Where(p => p.Name == "City"));
            Assert.Equal("Redmond", city.Value);

            // Verify the dynamic properties
            ODataProperty enumProperty = Assert.Single(complexValue.Properties.Where(p => p.Name == "EnumProperty"));
            ODataEnumValue enumValue = Assert.IsType<ODataEnumValue>(enumProperty.Value);
            Assert.Equal("Fourth", enumValue.Value);
            Assert.Equal("Default.SimpleEnum", enumValue.TypeName);

            ODataProperty guidProperty = Assert.Single(complexValue.Properties.Where(p => p.Name == "GuidProperty"));
            Assert.Equal(new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA"), guidProperty.Value);

            ODataProperty doubleProperty = Assert.Single(complexValue.Properties.Where(p => p.Name == "DoubleProperty"));
            Assert.Equal(99.109, doubleProperty.Value);
        }

        [Fact]
        public void CreateODataComplexValue_WritesNestedOpenComplexTypes()
        {
            // Arrange
            IEdmModel model = SerializationTestsHelpers.SimpleOpenTypeModel();
            
            IEdmComplexType addressType = model.FindDeclaredType("Default.Address") as IEdmComplexType;
            Type simpleOpenAddress = typeof(SimpleOpenAddress);
            model.SetAnnotationValue<ClrTypeAnnotation>(addressType, new ClrTypeAnnotation(simpleOpenAddress));
            model.SetAnnotationValue(addressType, new DynamicPropertyDictionaryAnnotation(
                simpleOpenAddress.GetProperty("Properties")));

            IEdmComplexType zipCodeType = model.FindDeclaredType("Default.ZipCode") as IEdmComplexType;
            Type simpleOpenZipCode = typeof(SimpleOpenZipCode);
            model.SetAnnotationValue<ClrTypeAnnotation>(zipCodeType, new ClrTypeAnnotation(simpleOpenZipCode));
            model.SetAnnotationValue(zipCodeType, new DynamicPropertyDictionaryAnnotation(
                simpleOpenZipCode.GetProperty("Properties")));

            IEdmComplexTypeReference addressTypeRef = addressType.ToEdmTypeReference(isNullable: false).AsComplex();

            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            ODataComplexTypeSerializer serializer = new ODataComplexTypeSerializer(serializerProvider);
            ODataSerializerContext context = new ODataSerializerContext
            {
                Model = model
            };

            SimpleOpenAddress topAddress = new SimpleOpenAddress()
            {
                Street = "TopStreet",
                City = "TopCity",
                Properties = new Dictionary<string, object>() { { "PropertOfAddress", "Value1"} }
            };

            SimpleOpenZipCode zipCode = new SimpleOpenZipCode()
            {
                Code = 101,
                Properties = new Dictionary<string, object>() { { "PropertyOfZipCode", "Value2"} }
            };

            SimpleOpenAddress subAddress = new SimpleOpenAddress()
            {
                Street = "SubStreet",
                City = "SubCity",
                Properties = new Dictionary<string, object>() { { "PropertOfSubAddress", "Value3" } }
            };

            //  TopAddress (SimpleOpenAddress)
            //       |-- Street                               (declare property, string)
            //       |-- City                                 (declare property, string)
            //       |-- PropertyOfAddress                    (dynamic property, string)
            //       |-- ZipCodeOfAddress                     (dynamic property, SimpleOpenZipCode)
            //              |-- Code                          (declare property, int)
            //              |-- PropertyOfZipCode             (dynamic property, string)
            //              |-- SubAddressOfZipCode           (dynamic property, SimpleOpenAddress)
            //                         |-- Street             (declare property, string)
            //                         |-- City               (declare property, string)
            //                         |-- PropertyOfAddress  (dynamic property, string)
            zipCode.Properties.Add("SubAddressOfZipCode", subAddress);
            topAddress.Properties.Add("ZipCodeOfAddress", zipCode);
                         
            // Act
            var odataValue = serializer.CreateODataComplexValue(topAddress, addressTypeRef, context);

            // Assert
            ODataComplexValue topAddressComplexValue = Assert.IsType<ODataComplexValue>(odataValue);

            Assert.Equal(topAddressComplexValue.TypeName, "Default.Address");
            Assert.Equal(4, topAddressComplexValue.Properties.Count());

            // Verify the dynamic "ZipCodeOfAddress" property, it's nested open complex type
            ODataProperty dynamicProperty = Assert.Single(topAddressComplexValue.Properties.Where(p => p.Name == "ZipCodeOfAddress"));
            ODataComplexValue zipCodeComplexValue = Assert.IsType<ODataComplexValue>(dynamicProperty.Value);

            Assert.Equal(zipCodeComplexValue.TypeName, "Default.ZipCode");
            Assert.Equal(3, zipCodeComplexValue.Properties.Count());

            // Verify the declared "Code" property of ZipCode
            ODataProperty code = Assert.Single(zipCodeComplexValue.Properties.Where(p => p.Name == "Code"));
            Assert.Equal(101, code.Value);

            // Verify the dynamic "SubAddressOfZipCode" property of ZipCode
            dynamicProperty = Assert.Single(zipCodeComplexValue.Properties.Where(p => p.Name == "SubAddressOfZipCode"));
            ODataComplexValue subAddressComplexValue = Assert.IsType<ODataComplexValue>(dynamicProperty.Value);

            Assert.Equal(subAddressComplexValue.TypeName, "Default.Address");
            Assert.Equal(3, subAddressComplexValue.Properties.Count());
        }

        [Fact]
        public void CreateODataComplexValue_Throws_IfDynamicPropertyNameSameAsDeclaredPropertyName()
        {
            // Arrange
            IEdmModel model = SerializationTestsHelpers.SimpleOpenTypeModel();
            IEdmComplexType addressType = model.FindDeclaredType("Default.Address") as IEdmComplexType;
            Type simpleOpenAddress = typeof(SimpleOpenAddress);
            model.SetAnnotationValue<ClrTypeAnnotation>(addressType, new ClrTypeAnnotation(simpleOpenAddress));
            model.SetAnnotationValue(addressType, new DynamicPropertyDictionaryAnnotation(
                simpleOpenAddress.GetProperty("Properties")));

            IEdmComplexTypeReference addressTypeRef = addressType.ToEdmTypeReference(isNullable: false).AsComplex();

            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            ODataComplexTypeSerializer serializer = new ODataComplexTypeSerializer(serializerProvider);
            ODataSerializerContext context = new ODataSerializerContext
            {
                Model = model
            };

            SimpleOpenAddress address = new SimpleOpenAddress()
            {
                Street = "My Way",
                City = "Redmond",
                Properties = new Dictionary<string, object>()
            };
            address.Properties.Add("StringProperty", "My Country");
            address.Properties.Add("Street", new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA"));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                serializer.CreateODataComplexValue(address, addressTypeRef, context),
                "The name of dynamic property 'Street' was already used as the declared property name " +
                "of open complex type 'Default.Address'.");
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

            ODataSerializerContext context = new ODataSerializerContext();
            TypedEdmComplexObject edmObject = new TypedEdmComplexObject(new { Property = 42 }, edmTypeReference, context.Model);
            ODataComplexTypeSerializer serializer = new ODataComplexTypeSerializer(new DefaultODataSerializerProvider());

            // Act
            ODataComplexValue result = serializer.CreateODataComplexValue(edmObject, edmTypeReference, context);

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
            ODataComplexTypeSerializer.AddTypeNameAnnotationAsNeeded(value, ODataMetadataLevel.MinimalMetadata);

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
