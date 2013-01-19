// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.OData.Formatter.Serialization.Models;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataCollectionSerializerTests
    {
        IEdmModel _model;
        IEdmEntitySet _customerSet;
        Customer _customer;
        ODataCollectionSerializer _serializer;
        IEdmPrimitiveType _edmIntType;

        public ODataCollectionSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();
            _customerSet = _model.FindDeclaredEntityContainer("Default.Container").FindEntitySet("Customers");
            _edmIntType = _model.FindType("Edm.Int32") as IEdmPrimitiveType;
            _customer = new Customer()
            {
                FirstName = "Foo",
                LastName = "Bar",
                ID = 10,
            };

            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            _serializer = new ODataCollectionSerializer(
                new EdmCollectionTypeReference(
                    new EdmCollectionType(
                        new EdmPrimitiveTypeReference(_edmIntType, isNullable: false)),
                        isNullable: false), serializerProvider);
        }

        [Fact]
        void CreateProperty_Serializes_AllElementsInTheCollection()
        {
            var property = _serializer.CreateProperty(new int[] { 1, 2, 3 }, "TestCollection", new ODataSerializerContext());

            Assert.Equal(property.Name, "TestCollection");
            var values = Assert.IsType<ODataCollectionValue>(property.Value);

            List<int> elements = new List<int>();
            foreach (var item in values.Items)
            {
                elements.Add(Assert.IsType<int>(item));
            }

            Assert.Equal(elements, new int[] { 1, 2, 3 });
        }

        [Fact]
        public void CreateProperty_ReturnsODataProperty_ForNullValue()
        {
            var property = _serializer.CreateProperty(null, "TestCollection", new ODataSerializerContext());

            Assert.NotNull(property);
            Assert.IsType(typeof(ODataCollectionValue), property.Value);
            ODataCollectionValue collection = (ODataCollectionValue)property.Value;
            Assert.Empty(collection.Items);
        }

        [Fact]
        public void CreateProperty_SetsTypeName()
        {
            // Arrange
            object graph = new int[] { 1, 2, 3 };
            string elementName = "TestCollection";
            ODataSerializerContext context = new ODataSerializerContext();

            // Act
            ODataProperty property = _serializer.CreateProperty(graph, elementName, context);

            // Assert
            Assert.NotNull(property);
            Assert.NotNull(property.Value);
            Assert.IsType<ODataCollectionValue>(property.Value);
            ODataCollectionValue collection = (ODataCollectionValue)property.Value;
            Assert.Equal("Collection(Edm.Int32)", collection.TypeName);
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_DoesNotAddAnnotation_InDefaultMetadataMode()
        {
            // Arrange
            ODataCollectionValue value = new ODataCollectionValue();

            // Act
            ODataCollectionSerializer.AddTypeNameAnnotationAsNeeded(value, ODataMetadataLevel.Default);

            // Assert
            Assert.Null(value.GetAnnotation<SerializationTypeNameAnnotation>());
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_AddsAnnotation_InJsonLightMetadataMode()
        {
            // Arrange
            string expectedTypeName = "TypeName";
            ODataCollectionValue value = new ODataCollectionValue
            {
                TypeName = expectedTypeName
            };

            // Act
            ODataCollectionSerializer.AddTypeNameAnnotationAsNeeded(value, ODataMetadataLevel.FullMetadata);

            // Assert
            SerializationTypeNameAnnotation annotation = value.GetAnnotation<SerializationTypeNameAnnotation>();
            Assert.NotNull(annotation); // Guard
            Assert.Equal(expectedTypeName, annotation.TypeName);
        }

        [Theory]
        [InlineData(TestODataMetadataLevel.Default, false)]
        [InlineData(TestODataMetadataLevel.FullMetadata, true)]
        [InlineData(TestODataMetadataLevel.MinimalMetadata, false)]
        [InlineData(TestODataMetadataLevel.NoMetadata, true)]
        public void ShouldAddTypeNameAnnotation(TestODataMetadataLevel metadataLevel, bool expectedResult)
        {
            // Act
            bool actualResult = ODataCollectionSerializer.ShouldAddTypeNameAnnotation(
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
            bool actualResult = ODataCollectionSerializer.ShouldSuppressTypeNameSerialization(
                (ODataMetadataLevel)metadataLevel);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }
    }
}
