//-----------------------------------------------------------------------------
// <copyright file="ODataPathExtensionsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------


using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Common.Models;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class ODataPathExtensionsTest
    {
        SampleEdmModel model = new SampleEdmModel();
        KeyValuePair<string, object>[] customerKey = new[] { new KeyValuePair<string, object>("Id", "1") };
        KeyValuePair<string, object>[] friendsKey = new[] { new KeyValuePair<string, object>("Id", "1001") };

        [Fact]
        public void GetKeys_PathWithTypeSegmentReturnsKeysFromLastKeySegment()
        {
            // From this path Customers(1)
            // GetKeys() should return { "Id": "1" }

            // Arrange
            ODataPath path = new ODataPath(new ODataPathSegment[]
            {
                new EntitySetSegment(model.customerSet),
                new KeySegment(customerKey, model.customerType, model.customerSet),
                new TypeSegment(model.vipCustomerType, model.customerType, null)
            });

            // Act
            Dictionary<string, object> keys = path.GetKeys();

            // Assert
            Assert.Single(keys);
            Assert.Equal("Id", keys.First().Key);
            Assert.Equal("1", keys.First().Value);
        }

        [Fact]
        public void GetKeys_PathWithNoSegmentReturnsEmptyCollection()
        {
            // Arrange
            ODataPath path = new ODataPath(new ODataPathSegment[]
            {
            });

            // Act
            Dictionary<string, object> keys = path.GetKeys();

            // Assert
            Assert.Empty(keys);
        }

        [Fact]
        public void GetKeys_PathWithNoKeySegmentReturnsEmptyCollection()
        {
            // Arrange
            ODataPath path = new ODataPath(new ODataPathSegment[]
            {
                new EntitySetSegment(model.customerSet),
                new TypeSegment(model.vipCustomerType, model.customerType, null)
            });

            // Act
            Dictionary<string, object> keys = path.GetKeys();

            // Assert
            Assert.Empty(keys);
        }

        [Fact]
        public void GetKeys_PathWithNavPropReturnsKeysFromLastKeySegment()
        {
            // From this path Customers(1)/Friends(1001)
            // GetKeys() should return { "Id": "1001" }

            // Arrange
            ODataPath path = new ODataPath(new ODataPathSegment[]
            {
                new EntitySetSegment(model.customerSet),
                new KeySegment(customerKey, model.customerType, model.customerSet),
                new NavigationPropertySegment(model.friendsProperty, model.customerSet),
                new KeySegment(friendsKey, model.personType, null)
            });

            // Act
            Dictionary<string, object> keys = path.GetKeys();

            // Assert
            Assert.Single(keys);
            Assert.Equal("Id", keys.First().Key);
            Assert.Equal("1001", keys.First().Value);
        }

        [Fact]
        public void GetLastNonTypeNonKeySegment_TypeSegmentAsLastSegmentReturnsCorrectSegment()
        {
            // If the path is Customers(1)/Friends(1001)/Ns.UniqueFriend where Ns.UniqueFriend is a type segment
            // and 1001 is a KeySegment,
            // GetLastNonTypeNonKeySegment() should return Friends NavigationPropertySegment.

            // Arrange
            ODataPath path = new ODataPath(new ODataPathSegment[]
            {
                new EntitySetSegment(model.customerSet),
                new KeySegment(customerKey, model.customerType, model.customerSet),
                new NavigationPropertySegment(model.friendsProperty, model.customerSet),
                new KeySegment(friendsKey, model.personType, null),
                new TypeSegment(model.uniquePersonType, model.personType, null)
            });

            // Act
            ODataPathSegment segment = path.GetLastNonTypeNonKeySegment();

            // Assert
            Assert.Equal("Friends", segment.Identifier);
            Assert.True(segment is NavigationPropertySegment);
        }

        [Fact]
        public void GetLastNonTypeNonKeySegment_KeySegmentAsLastSegmentReturnsCorrectSegment()
        {
            // If the path is Customers(1)/Friends(1001) where1001 is a KeySegment,
            // GetLastNonTypeNonKeySegment() should return Friends NavigationPropertySegment.

            // Arrange
            ODataPath path = new ODataPath(new ODataPathSegment[]
            {
                new EntitySetSegment(model.customerSet),
                new KeySegment(customerKey, model.customerType, model.customerSet),
                new NavigationPropertySegment(model.friendsProperty, model.customerSet),
                new KeySegment(friendsKey, model.personType, null)
            });

            // Act
            ODataPathSegment segment = path.GetLastNonTypeNonKeySegment();

            // Assert
            Assert.Equal("Friends", segment.Identifier);
            Assert.True(segment is NavigationPropertySegment);
        }

        [Fact]
        public void GetLastNonTypeNonKeySegment_SingleSegmentPathReturnsCorrectSegment()
        {
            // If the path is /Customers,
            // GetLastNonTypeNonKeySegment() should return Customers EntitySetSegment.

            // Arrange
            ODataPath path = new ODataPath(new ODataPathSegment[]
            {
                new EntitySetSegment(model.customerSet)
            });

            // Act
            ODataPathSegment segment = path.GetLastNonTypeNonKeySegment();

            // Assert
            Assert.True(segment is EntitySetSegment);
        }
    }
}
