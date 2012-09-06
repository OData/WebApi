// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataEntryDeserializerTests
    {
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
