// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Test.AspNet.OData.Factories;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Routing.Conventions
{
    public class ODataRoutingConventionsTest
    {
        [Fact]
        public void CreateDefaultWithAttributeRouting_ContainsAttributeRoutingConvention()
        {
            // Arrange
            var config = RoutingConfigurationFactory.CreateWithRootContainer("odata");

            // Act
            var conventions = ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", config);

            // Assert
            Assert.Single(conventions.OfType<AttributeRoutingConvention>());
        }
    }
}
