// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.OData.TestCommon;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing.Template
{
    public class NavigationPropertyLinkSegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_NavigationPropertyLinkSegment()
        {
            // Assert
            Assert.ThrowsArgumentNull(() => new NavigationPropertyLinkSegmentTemplate(segment: null), "segment");
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
