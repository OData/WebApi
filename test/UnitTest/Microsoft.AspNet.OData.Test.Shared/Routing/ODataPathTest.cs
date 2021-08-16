//-----------------------------------------------------------------------------
// <copyright file="ODataPathTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Test.Routing
{
    public class ODataPathTest
    {
        [Fact]
        public void ToStringWithNoSegments()
        {
            // Arrange
            ODataPath path = new ODataPath();

            // Act
            string value = path.ToString();

            // Assert
            Assert.Empty(value);
        }

        [Fact]
        public void ToStringWithOneSegment()
        {
            // Arrange
            ODataPath path = new ODataPath(MetadataSegment.Instance);

            // Act
            string value = path.ToString();

            // Assert
            Assert.Equal("$metadata", value);
        }

        [Fact]
        public void ToStringWithOneTwoSegments()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "entity");
            EdmEntityContainer container = new EdmEntityContainer("NS", "default");
            EdmEntitySet entitySet = new EdmEntitySet(container, "set", entityType);
            EdmAction action = new EdmAction("NS", "action", null, true, null);
            ODataPath path = new ODataPath(new EntitySetSegment(entitySet),
                new OperationSegment(new[] { action }, null));

            // Act
            string value = path.ToString();

            // Assert
            Assert.Equal("set/NS.action", value);
        }

        [Fact]
        public void ToStringWithKeyValueSegment()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "entity");
            entityType.AddKeys(entityType.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            var keys = new[] {new KeyValuePair<string, object>("ID", 1)};
            ODataPath path = new ODataPath(new KeySegment(keys, entityType, null));

            // Act
            string value = path.ToString();

            // Assert
            string expectedValue = "(" + 1 + ")";
            Assert.Equal(expectedValue, value);
        }
    }
}
