// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.OData.Routing.Conventions;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Routing.Conventions
{
    public class ODataRoutingConventionsTest
    {
        [Fact]
        public void CreateDefaultWithAttributeRouting_ContainsAttributeRoutingConvention()
        {
            // Arrange
            var config = new HttpConfiguration();

            // Act
            var conventions = ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", config);

            // Assert
            Assert.Single(conventions.OfType<AttributeRoutingConvention>());
        }
    }
}
