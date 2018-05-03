// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Routing.Conventions
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
