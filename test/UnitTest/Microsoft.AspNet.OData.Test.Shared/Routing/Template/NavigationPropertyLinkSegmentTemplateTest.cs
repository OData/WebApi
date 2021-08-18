//-----------------------------------------------------------------------------
// <copyright file="NavigationPropertyLinkSegmentTemplateTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Routing.Template
{
    public class NavigationPropertyLinkSegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_NavigationPropertyLinkSegment()
        {
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new NavigationPropertyLinkSegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void TryMatch_ReturnsTrue()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var ordersProperty = model.Customer.FindProperty("Orders") as IEdmNavigationProperty;

            NavigationPropertyLinkSegmentTemplate template =
                new NavigationPropertyLinkSegmentTemplate(new NavigationPropertyLinkSegment(ordersProperty, model.Orders));
            NavigationPropertyLinkSegment segment = new NavigationPropertyLinkSegment(ordersProperty, model.Orders);

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.True(result);
            Assert.Empty(values);
        }

        [Fact]
        public void TryMatch_ReturnsFalse()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var ordersProperty = model.Customer.FindProperty("Orders") as IEdmNavigationProperty;
            var specialOrderProperty = model.SpecialCustomer.FindProperty("SpecialOrders") as IEdmNavigationProperty;

            NavigationPropertyLinkSegmentTemplate template =
                new NavigationPropertyLinkSegmentTemplate(new NavigationPropertyLinkSegment(ordersProperty, model.Orders));
            NavigationPropertyLinkSegment segment = new NavigationPropertyLinkSegment(specialOrderProperty, model.Orders);

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.False(result);
        }
    }
}
