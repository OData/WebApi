// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization
{
    public class ODataEnumTypeSerializerTests
    {
        [Fact]
        public void AddTypeNameAnnotationAsNeeded_DoesNotAddAnnotation()
        {
            // Arrange
            ODataEnumValue enumValue = new ODataEnumValue("value");
            IEdmEnumTypeReference enumType = new EdmEnumTypeReference(
                new EdmEnumType("TestModel", "EnumType"), isNullable: false);

            // Act
            ODataEnumSerializer.AddTypeNameAnnotationAsNeeded(enumValue, enumType, ODataMetadataLevel.MinimalMetadata);

            // Assert
            ODataTypeAnnotation annotation = enumValue.TypeAnnotation;
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
            ODataTypeAnnotation annotation = enumValue.TypeAnnotation;
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
            ODataTypeAnnotation annotation = enumValue.TypeAnnotation;
            Assert.NotNull(annotation);
            Assert.Null(annotation.TypeName);
        }
    }
}
