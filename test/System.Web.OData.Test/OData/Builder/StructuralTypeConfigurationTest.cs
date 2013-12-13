// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder
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
    }
}
