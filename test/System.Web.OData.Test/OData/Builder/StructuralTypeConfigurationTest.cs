// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder
{
    public class StructuralTypeConfigurationTest
    {
        private StructuralTypeConfiguration _configuration;
        private string _name = "name";
        private string _namespace = "com.contoso";

        public StructuralTypeConfigurationTest()
        {
            Mock<StructuralTypeConfiguration> mockConfiguration = new Mock<StructuralTypeConfiguration> { CallBase = true };
            mockConfiguration.Object.Name = "Name";
            mockConfiguration.Object.Namespace = "Namespace";
            _configuration = mockConfiguration.Object;
        }

        [Fact]
        public void Property_Name_RoundTrips()
        {
            Assert.Reflection.Property(_configuration, c => c.Name, "Name", allowNull: false, roundTripTestValue: _name);
        }

        [Fact]
        public void Property_Namespace_RoundTrips()
        {
            Assert.Reflection.Property(_configuration, c => c.Namespace, "Namespace", allowNull: false, roundTripTestValue: _namespace);
        }

        [Fact]
        public void Property_AddedExplicitly_RoundTrips()
        {
            Assert.Reflection.BooleanProperty(_configuration, c => c.AddedExplicitly, true);
        }

        [Fact]
        public void Property_ThrowsIfTypeIsDateTime()
        {
            // Arrange
            MockType type = new MockType("Customer", @namespace: "Contoso");
            MockPropertyInfo property = new MockPropertyInfo(typeof(DateTime), "Birthday");
            property.SetupGet(p => p.ReflectedType).Returns(type);
            property.SetupGet(p => p.DeclaringType).Returns(type);

            Mock<StructuralTypeConfiguration> mock = new Mock<StructuralTypeConfiguration> { CallBase = true };
            StructuralTypeConfiguration configuration = mock.Object;
            mock.SetupGet(c => c.ClrType).Returns(type);

            // Act & Assert
            Assert.ThrowsArgument(() => configuration.AddProperty(property), "propertyInfo",
                "The type 'System.DateTime' of property 'Birthday' in the 'Contoso.Customer' type is not a supported type.");
        }
    }
}
