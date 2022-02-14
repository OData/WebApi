//-----------------------------------------------------------------------------
// <copyright file="NavigationPropertySegmentTemplateTest.cs" company=".NET Foundation">
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
    public class NavigationPropertySegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_NavigationPropertyLinkSegment()
        {
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new NavigationPropertySegmentTemplate(segment: null), "segment");
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
