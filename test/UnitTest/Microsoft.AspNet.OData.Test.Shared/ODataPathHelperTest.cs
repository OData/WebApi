//-----------------------------------------------------------------------------
// <copyright file="ODataPathHelperTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Test.Common.Models;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class ODataPathHelperTest
    {
        SampleEdmModel model = new SampleEdmModel();
        KeyValuePair<string, object>[] customerKey = new[] { new KeyValuePair<string, object>("Id", "1"), new KeyValuePair<string, object>("AlternateId", "2") };
        KeyValuePair<string, object>[] friendsKey = new[] { new KeyValuePair<string, object>("Id", "1001") };

        [Fact]
        public void GetKeysFromKeySegment_ReturnsCorrectKeysDictionary()
        {
            // Arrange
            KeySegment keySegment = new KeySegment(customerKey, model.customerType, model.customerSet);

            // Act
            Dictionary<string, object> keys = ODataPathHelper.KeySegmentAsDictionary(keySegment);

            // Assert
            Assert.Equal(2, keys.Count);
            Assert.Equal("Id", keys.First().Key);
            Assert.Equal("1", keys.First().Value);
            Assert.Equal("AlternateId", keys.Last().Key);
            Assert.Equal("2", keys.Last().Value);
        }

        [Fact]
        public void GetNextKeySegmentPosition_ReturnsCorrectPosition()
        {
            // If the path is Customers(1)/Friends(1001)/Ns.UniqueFriend where Ns.UniqueFriend is a type segment
            // and 1001 is a KeySegment, and the starting position is index 1, the next keysegment position is index 3.

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
            int position = ODataPathHelper.GetNextKeySegmentPosition(path.AsList(), 1);

            // Assert
            Assert.Equal(3, position);
        }
    }
}
