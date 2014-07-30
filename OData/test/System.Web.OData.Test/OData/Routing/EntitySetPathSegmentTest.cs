// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Routing
{
    public class EntitySetPathSegmentTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EntitySet()
        {
            Assert.ThrowsArgumentNull(() => new EntitySetPathSegment(entitySet: null), "entitySet");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_EntitySetName()
        {
            Assert.ThrowsArgumentNull(() => new EntitySetPathSegment(entitySetName: null), "entitySetName");
        }

        [Fact]
        public void Property_SegmentKind_IsEntitySet()
        {
            EntitySetPathSegment segment = new EntitySetPathSegment(entitySetName: "Customers");
            Assert.Equal(ODataSegmentKinds.EntitySet, segment.SegmentKind);
        }

        [Fact]
        public void TryMatch_ReturnsTrue_IfMatchingEntitySet()
        {
            // Arrange
            IEdmEntitySet entitySet = new Mock<IEdmEntitySet>().Object;
            ODataPathSegmentTemplate template = new EntitySetPathSegment(entitySet);
            EntitySetPathSegment segment = new EntitySetPathSegment(entitySet);

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.True(result);
            Assert.Empty(values);
        }
    }
}
