// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Test.Common;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder
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
            ReflectionAssert.Property<StructuralTypeConfiguration<object>, string, ArgumentException>(_configuration, c => c.Name, "Name", allowNull: false, roundTripTestValue: _name);
        }

        [Fact]
        public void Property_Namespace_RoundTrips()
        {
            ReflectionAssert.Property<StructuralTypeConfiguration<object>, string, ArgumentException>(_configuration, c => c.Namespace, "Namespace", allowNull: false, roundTripTestValue: _namespace);
        }

        [Fact]
        public void Property_FullName()
        {
            Assert.Equal("Namespace.Name", _configuration.FullName);
        }
    }
}
