//-----------------------------------------------------------------------------
// <copyright file="ODataPathExtensionsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class ODataPathExtensionsTest
    {
        EdmNavigationProperty friendsProperty;
        EdmEntityType customerType;
        EdmEntityType personType;
        EdmEntityType uniquePersonType;
        EdmEntityType vipCustomerType;
        IEdmEntitySet customerSet;

        KeyValuePair<string, object>[] customerKey = new[] { new KeyValuePair<string, object>("Id", "1") };
        KeyValuePair<string, object>[] friendsKey = new[] { new KeyValuePair<string, object>("Id", "1001") };

        public ODataPathExtensionsTest()
        {
            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");

            personType = new EdmEntityType("NS", "Person");
            personType.AddKeys(personType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, isNullable: false));
            personType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String, isNullable: false);

            customerType = new EdmEntityType("NS", "Customer");
            customerType.AddKeys(customerType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, isNullable: false));
            customerType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String, isNullable: false);
            friendsProperty = customerType.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    ContainsTarget = true,
                    Name = "Friends",
                    Target = personType,
                    TargetMultiplicity = EdmMultiplicity.Many
                });

            vipCustomerType = new EdmEntityType("NS", "VipCustomer", customerType);
            vipCustomerType.AddKeys(vipCustomerType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, isNullable: false));
            vipCustomerType.AddStructuralProperty("VipName", EdmPrimitiveTypeKind.String, isNullable: false);

            uniquePersonType = new EdmEntityType("NS", "UniquePerson", personType);
            uniquePersonType.AddKeys(uniquePersonType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, isNullable: false));
            uniquePersonType.AddStructuralProperty("UniqueName", EdmPrimitiveTypeKind.String, isNullable: false);

            model.AddElement(customerType);
            model.AddElement(personType);
            model.AddElement(uniquePersonType);
            model.AddElement(vipCustomerType);
            model.AddElement(container);

            customerSet = container.AddEntitySet("Customers", customerType);
        }

        [Fact]
        public void GetKeys_PathWithTypeSegmentReturnsKeysFromLastKeySegment()
        {
            // From this path Customers(1)
            // GetKeys() should return { "Id": "1" }

            // Arrange
            ODataPath path = new ODataPath(new ODataPathSegment[]
            {
                new EntitySetSegment(customerSet),
                new KeySegment(customerKey, customerType, customerSet),
                new TypeSegment(vipCustomerType, customerType, null)
            });

            // Act
            Dictionary<string, object> keys = path.GetKeys();

            // Assert
            Assert.Single(keys);
            Assert.Equal("Id", keys.First().Key);
            Assert.Equal("1", keys.First().Value);
        }

        [Fact]
        public void GetKeys_PathWithNavPropReturnsKeysFromLastKeySegment()
        {
            // From this path Customers(1)/Friends(1001)
            // GetKeys() should return { "Id": "1001" }

            // Arrange
            ODataPath path = new ODataPath(new ODataPathSegment[]
            {
                new EntitySetSegment(customerSet),
                new KeySegment(customerKey, customerType, customerSet),
                new NavigationPropertySegment(friendsProperty, customerSet),
                new KeySegment(friendsKey, personType, null)
            });

            // Act
            Dictionary<string, object> keys = path.GetKeys();

            // Assert
            Assert.Single(keys);
            Assert.Equal("Id", keys.First().Key);
            Assert.Equal("1001", keys.First().Value);
        }

        [Fact]
        public void GetLastNonTypeNonKeySegment_ReturnsCorrectSegment()
        {
            // If the path is Customers(1)/Friends(1001)/Ns.UniqueFriend where Ns.UniqueFriend is a type segment
            // and 1001 is a KeySegment,
            // GetLastNonTypeNonKeySegment() should return Friends NavigationPropertySegment.

            // Arrange
            ODataPath path = new ODataPath(new ODataPathSegment[]
            {
                new EntitySetSegment(customerSet),
                new KeySegment(customerKey, customerType, customerSet),
                new NavigationPropertySegment(friendsProperty, customerSet),
                new KeySegment(friendsKey, personType, null),
                new TypeSegment(uniquePersonType, personType, null)
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
                new EntitySetSegment(customerSet)
            });

            // Act
            ODataPathSegment segment = path.GetLastNonTypeNonKeySegment();

            // Assert
            Assert.True(segment is EntitySetSegment);
        }
    }
}
