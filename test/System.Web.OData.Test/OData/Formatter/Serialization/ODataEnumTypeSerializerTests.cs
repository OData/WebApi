// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.OData.Formatter.Serialization
{
    public class ODataEnumTypeSerializerTests
    {
        [Theory]
        [InlineData(ODataMetadataLevel.Default)]
        [InlineData(ODataMetadataLevel.MinimalMetadata)]
        public void AddTypeNameAnnotationAsNeeded_DoesNotAddAnnotation(ODataMetadataLevel metadataLevel)
        {
            // Arrange
            ODataEnumValue enumValue = new ODataEnumValue("value");
            IEdmEnumTypeReference enumType = new EdmEnumTypeReference(
                new EdmEnumType("TestModel", "EnumType"), isNullable: false);

            // Act
            ODataEnumSerializer.AddTypeNameAnnotationAsNeeded(enumValue, enumType, metadataLevel);

            // Assert
            SerializationTypeNameAnnotation annotation = enumValue.GetAnnotation<SerializationTypeNameAnnotation>();
            Assert.Null(annotation);
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_AddAnnotation_InFullMetadataMode()
        {
            // Arrange
            ODataEnumValue enumValue = new ODataEnumValue("value");
            IEdmEnumTypeReference enumType = new EdmEnumTypeReference(
                new EdmEnumType("TestModel", "EnumType"), isNullable: false);

            // Act
            ODataEnumSerializer.AddTypeNameAnnotationAsNeeded(enumValue, enumType, ODataMetadataLevel.FullMetadata);

            // Assert
            SerializationTypeNameAnnotation annotation = enumValue.GetAnnotation<SerializationTypeNameAnnotation>();
            Assert.NotNull(annotation);
            Assert.Equal("TestModel.EnumType", annotation.TypeName);
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_AddsNullAnnotation_InNoMetadataMode()
        {
            // Arrange
            ODataEnumValue enumValue = new ODataEnumValue("value");
            IEdmEnumTypeReference enumType = new EdmEnumTypeReference(
                new EdmEnumType("TestModel", "EnumType"), isNullable: false);

            // Act
            ODataEnumSerializer.AddTypeNameAnnotationAsNeeded(enumValue, enumType, ODataMetadataLevel.NoMetadata);

            // Assert
            SerializationTypeNameAnnotation annotation = enumValue.GetAnnotation<SerializationTypeNameAnnotation>();
            Assert.NotNull(annotation);
            Assert.Null(annotation.TypeName);
        }
    }
}
