// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
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
        public void AddDynamicPropertyDictionary_ThrowsIfTypeIsNotDictionary()
        {
            // Arrange
            MockPropertyInfo property = new MockPropertyInfo(typeof(Int32), "Test");
            Mock<StructuralTypeConfiguration> mock = new Mock<StructuralTypeConfiguration> { CallBase = true };
            StructuralTypeConfiguration configuration = mock.Object;

            // Act & Assert
            Assert.ThrowsArgument(() => configuration.AddDynamicPropertyDictionary(property),
                "propertyInfo",
                string.Format("The argument must be of type '{0}'.", "IDictionary<string, object>"));
        }
    }
}
