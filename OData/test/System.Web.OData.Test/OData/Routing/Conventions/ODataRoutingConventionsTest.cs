// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Routing.Conventions
{
    public class ODataRoutingConventionsTest
    {
        [Fact]
        public void CreateDefaultWithAttributeRouting_ContainsAttributeRoutingConvention()
        {
            // Arrange
            var config = new HttpConfiguration();
            var model = new Mock<IEdmModel>().Object;

            // Act
            var conventions = ODataRoutingConventions.CreateDefaultWithAttributeRouting(config, model);

            // Assert
            Assert.Single(conventions.OfType<AttributeRoutingConvention>());
        }
    }
}
