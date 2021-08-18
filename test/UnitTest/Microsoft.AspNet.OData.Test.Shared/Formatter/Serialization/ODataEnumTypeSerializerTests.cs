//-----------------------------------------------------------------------------
// <copyright file="ODataEnumTypeSerializerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.OData;
using Microsoft.OData.Edm;
using System.Linq;
using System.Runtime.Serialization;
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

        [Fact]
        public void CreateODataEnumValue_ReturnsCorrectEnumMember()
        {
            // Arrange
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EnumType<BookCategory>().Namespace = "NS";
            IEdmModel model = builder.GetEdmModel();
            IEdmEnumType enumType = model.SchemaElements.OfType<IEdmEnumType>().Single();

            var provider = new DefaultODataSerializerProvider(new MockContainer());
            ODataEnumSerializer serializer = new ODataEnumSerializer(provider);
            ODataSerializerContext writeContext = new ODataSerializerContext
            {
                Model = model
            };

            // Act
            ODataEnumValue value = serializer.CreateODataEnumValue(BookCategory.Newspaper,
                new EdmEnumTypeReference(enumType, false), writeContext);

            // Assert
            Assert.NotNull(value);
            Assert.Equal("news", value.Value);
        }
    }

    [DataContract(Name = "category")]
    public enum BookCategory
    {
        [EnumMember(Value = "cartoon")]
        Cartoon,
        [EnumMember(Value = "news")]
        Newspaper
    }
}
