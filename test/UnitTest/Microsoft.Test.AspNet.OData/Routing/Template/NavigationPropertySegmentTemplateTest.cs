// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.AspNet.OData.TestCommon;

namespace Microsoft.Test.AspNet.OData.Routing.Template
{
    public class NavigationPropertySegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_NavigationPropertyLinkSegment()
        {
            // Assert
            Assert.ThrowsArgumentNull(() => new NavigationPropertySegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void TryMatch_ReturnsTrue()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var ordersProperty = model.Customer.FindProperty("Orders") as IEdmNavigationProperty;

            NavigationPropertySegmentTemplate template =
                new NavigationPropertySegmentTemplate(new NavigationPropertySegment(ordersProperty, model.Orders));
            NavigationPropertySegment segment = new NavigationPropertySegment(ordersProperty, model.Orders);

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

            NavigationPropertySegmentTemplate template =
                new NavigationPropertySegmentTemplate(new NavigationPropertySegment(ordersProperty, model.Orders));
            NavigationPropertySegment segment = new NavigationPropertySegment(specialOrderProperty, model.Orders);

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.False(result);
        }
    }
}
