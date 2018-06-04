// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Test.Common;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder
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
            ReflectionAssert.Property(_configuration, c => c.Name, "Name", allowNull: false, roundTripTestValue: _name);
        }

        [Fact]
        public void Property_Namespace_RoundTrips()
        {
            ReflectionAssert.Property(_configuration, c => c.Namespace, "Namespace", allowNull: false, roundTripTestValue: _namespace);
        }

        [Fact]
        public void Property_AddedExplicitly_RoundTrips()
        {
            ReflectionAssert.BooleanProperty(_configuration, c => c.AddedExplicitly, true);
        }

        [Fact]
        public void AddDynamicPropertyDictionary_ThrowsIfTypeIsNotDictionary()
        {
            // Arrange
            MockPropertyInfo property = new MockPropertyInfo(typeof(Int32), "Test");
            Mock<StructuralTypeConfiguration> mock = new Mock<StructuralTypeConfiguration> { CallBase = true };
            StructuralTypeConfiguration configuration = mock.Object;

            // Act & Assert
            ExceptionAssert.ThrowsArgument(() => configuration.AddDynamicPropertyDictionary(property),
                "propertyInfo",
                string.Format("The argument must be of type '{0}'.", "IDictionary<string, object>"));
        }
    }
}
