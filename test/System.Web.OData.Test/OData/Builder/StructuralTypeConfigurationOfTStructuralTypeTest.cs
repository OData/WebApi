// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder
{
    public class StructuralTypeConfigurationOfTStructuralTypeTest
    {
        private StructuralTypeConfiguration<object> _configuration;
        private string _name = "name";
        private string _namespace = "com.contoso";

        public StructuralTypeConfigurationOfTStructuralTypeTest()
        {
            Mock<StructuralTypeConfiguration> mockConfig = new Mock<StructuralTypeConfiguration> { CallBase = true };
            mockConfig.Object.Name = "Name";
            mockConfig.Object.Namespace = "Namespace";

            Mock<StructuralTypeConfiguration<object>> mockGenericConfig =
                new Mock<StructuralTypeConfiguration<object>>(mockConfig.Object) { CallBase = true };
            _configuration = mockGenericConfig.Object;
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
        public void Property_FullName()
        {
            Assert.Equal("Namespace.Name", _configuration.FullName);
        }
    }
}
