// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;
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
        public void Constructor_Succeeds_ForValidComplexType()
        {
            var deserializerProvider = new Mock<ODataDeserializerProvider>(EdmTestHelpers.GetModel()).Object;
            var deserializer = new ODataComplexTypeDeserializer(_addressEdmType, deserializerProvider);

            Assert.Equal(deserializer.DeserializerProvider, deserializerProvider);
            Assert.Equal(deserializer.EdmModel, deserializerProvider.EdmModel);
            Assert.Equal(deserializer.EdmComplexType.Definition, EdmTestHelpers.GetEdmType("ODataDemo.Address"));
            Assert.Equal(deserializer.ODataPayloadKind, ODataPayloadKind.Property);
        }

        [Fact]
        public void ReadInline_Throws_ForNonODataComplexValues()
        {
            var deserializerProvider = new Mock<ODataDeserializerProvider>(EdmTestHelpers.GetModel()).Object;
            var deserializer = new ODataComplexTypeDeserializer(_addressEdmType, deserializerProvider);

            Assert.ThrowsArgument(() =>
            {
                deserializer.ReadInline(10, new ODataDeserializerReadContext());
            }, "item");
        }

        [Fact]
        public void ReadInline()
        {
            // Arrange
            var deserializerProvider = new Mock<ODataDeserializerProvider>(EdmTestHelpers.GetModel()).Object;
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

            // Act
            ODataEntityDeserializerTests.Address address =
                deserializer.ReadInline(
                complexValue,
                new ODataDeserializerReadContext()) as ODataEntityDeserializerTests.Address;

            // Assert
            Assert.NotNull(address);
            Assert.Equal(address.Street, "12");
            Assert.Equal(address.City, "Redmond");
            Assert.Null(address.Country);
            Assert.Null(address.State);
            Assert.Null(address.ZipCode);
        }
    }
}
