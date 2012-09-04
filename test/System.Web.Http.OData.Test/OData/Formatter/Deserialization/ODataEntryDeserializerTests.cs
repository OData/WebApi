// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Data.Linq;
using System.Xml.Linq;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataEntryDeserializerTests
    {
        public static TheoryDataSet<object, object, Type> ConvertPrimitiveValue_NonStandardPrimitives_Data
        {
            get
            {
                return new TheoryDataSet<object, object, Type>
                {
                     { "1", (char)'1', typeof(char) },
                     { "1", (char?)'1', typeof(char?) },
                     { "123", (char[]) new char[] {'1', '2', '3' }, typeof(char[]) },
                     { (int)1 , (ushort)1, typeof(ushort)},
                     { (int?)1, (ushort?)1,  typeof(ushort?) },
                     { (long)1, (uint)1,  typeof(uint)},
                     { (long?)1, (uint?)1, typeof(uint?) },
                     { (long)1 , (ulong)1, typeof(ulong)},
                     { (long?)1 ,(ulong?)1, typeof(ulong?)},
                    //(Stream) new MemoryStream(new byte[] { 1 }), // TODO: Enable once we have support for streams
                     { "<element xmlns=\"namespace\" />" ,(XElement) new XElement(XName.Get("element","namespace")), typeof(XElement)},
                     { new byte[] {1} ,(Binary) new Binary(new byte[] {1}), typeof(Binary)}
                };
            }
        }

        [Theory]
        [PropertyData("ConvertPrimitiveValue_NonStandardPrimitives_Data")]
        public void ConvertPrimitiveValue_NonStandardPrimitives(object valueToConvert, object result, Type conversionType)
        {
            Assert.Equal(result.GetType(), ODataEntryDeserializer.ConvertPrimitiveValue(valueToConvert, conversionType, "", "").GetType());
            Assert.Equal(result.ToString(), ODataEntryDeserializer.ConvertPrimitiveValue(valueToConvert, conversionType, "", "").ToString());
        }

        [Theory]
        [InlineData("123")]
        [InlineData("")]
        public void ConvertPrimitiveValueToChar_Throws(string input)
        {
            Assert.Throws<ValidationException>(
                () => ODataEntryDeserializer.ConvertPrimitiveValue(input, typeof(char), "property", "type"),
                "The property 'property' on type 'type' must be a string with a length of 1.");
        }

        [Fact]
        public void ConvertPrimitiveValueToNullableChar_Throws()
        {
            Assert.Throws<ValidationException>(
                () => ODataEntryDeserializer.ConvertPrimitiveValue("123", typeof(char?), "property", "type"),
                "The property 'property' on type 'type' must be a string with a maximum length of 1.");
        }

        [Fact]
        public void ConvertPrimitiveValueToXElement_Throws_IfInputIsNotString()
        {
            Assert.Throws<ValidationException>(
                () => ODataEntryDeserializer.ConvertPrimitiveValue(123, typeof(XElement), "property", "type"),
                "The property 'property' on type 'type' must be a string.");
        }

        [Theory]
        [InlineData("Property", true, typeof(int))]
        [InlineData("Property", false, typeof(int))]
        [InlineData("PropertyNotPresent", true, null)]
        [InlineData("PropertyNotPresent", false, null)]
        public void GetPropertyType_NonDelta(string propertyName, bool isDelta, Type expectedPropertyType)
        {
            object resource = isDelta ? (object)new Delta<GetPropertyType_TestClass>() : new GetPropertyType_TestClass();
            Assert.Equal(
                expectedPropertyType,
                ODataEntryDeserializer.GetPropertyType(resource, propertyName, isDelta));
        }

        [Fact]
        public void ApplyProperty_IgnoresKeyProperty_WhenPatchKeyModeIsIgnore()
        {
            // Arrange
            ODataProperty property = new ODataProperty { Name = "Key1", Value = "Value1" };
            EdmEntityType entityType = new EdmEntityType("namespace", "name");
            entityType.AddKeys(new EdmStructuralProperty(entityType, "Key1", EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string))));
            EdmEntityTypeReference entityTypeReference = new EdmEntityTypeReference(entityType, isNullable: false);
            ODataDeserializerProvider provider = new DefaultODataDeserializerProvider(EdmCoreModel.Instance);

            var resource = new Mock<IDelta>(MockBehavior.Strict);

            // Act
            ODataEntryDeserializer.ApplyProperty(property, entityTypeReference, resource.Object, provider, new ODataDeserializerContext { IsPatchMode = true, PatchKeyMode = PatchKeyMode.Ignore });

            // Assert
            resource.Verify();
        }

        [Fact]
        public void ApplyProperty_AppliesKeyProperty_WhenPatchKeyModeIsPatch()
        {
            // Arrange
            ODataProperty property = new ODataProperty { Name = "Key1", Value = "Value1" };
            EdmEntityType entityType = new EdmEntityType("namespace", "name");
            entityType.AddKeys(new EdmStructuralProperty(entityType, "Key1", EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string))));
            EdmEntityTypeReference entityTypeReference = new EdmEntityTypeReference(entityType, isNullable: false);
            ODataDeserializerProvider provider = new DefaultODataDeserializerProvider(EdmCoreModel.Instance);

            var resource = new Mock<IDelta>(MockBehavior.Strict);
            Type propertyType = typeof(string);
            resource.Setup(r => r.TryGetPropertyType("Key1", out propertyType)).Returns(true);
            resource.Setup(r => r.TrySetPropertyValue("Key1", "Value1")).Returns(true).Verifiable();

            // Act
            ODataEntryDeserializer.ApplyProperty(property, entityTypeReference, resource.Object, provider, new ODataDeserializerContext { IsPatchMode = true, PatchKeyMode = PatchKeyMode.Patch });

            // Assert
            resource.Verify();
        }

        [Fact]
        public void ApplyProperty_ThrowsOnKeyProperty_WhenPatchKeyModeIsThrow()
        {
            // Arrange
            ODataProperty property = new ODataProperty { Name = "Key1", Value = "Value1" };
            EdmEntityType entityType = new EdmEntityType("namespace", "name");
            entityType.AddKeys(new EdmStructuralProperty(entityType, "Key1", EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string))));
            EdmEntityTypeReference entityTypeReference = new EdmEntityTypeReference(entityType, isNullable: false);
            ODataDeserializerProvider provider = new DefaultODataDeserializerProvider(EdmCoreModel.Instance);

            var resource = new Mock<IDelta>(MockBehavior.Strict);

            // Act && Assert
            Assert.Throws<InvalidOperationException>(
                () => ODataEntryDeserializer.ApplyProperty(property, entityTypeReference, resource.Object, provider, new ODataDeserializerContext { IsPatchMode = true, PatchKeyMode = PatchKeyMode.Throw }),
                "Cannot apply PATCH on key property 'Key1' on entity type 'namespace.name' when 'PatchKeyMode' is 'Throw'.");
        }

        private class GetPropertyType_TestClass
        {
            public int Property { get; set; }
        }
    }
}
