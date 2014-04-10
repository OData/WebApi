// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Routing
{
    public class BatchPathSegmentTest
    {
        [Fact]
        public void PropertySegmentKind_Returns_Batch()
        {
            BatchPathSegment segment = new BatchPathSegment();
            Assert.Equal(ODataSegmentKinds.Batch, segment.SegmentKind);
        }

        [Fact]
        public void GetEdmType_ReturnsNull()
        {
            BatchPathSegment segment = new BatchPathSegment();
            Assert.Null(segment.GetEdmType(previousEdmType: null));
        }

        [Fact]
        public void GetNavigationSource_ReturnsNull()
        {
            BatchPathSegment batchSegment = new BatchPathSegment();
            IEdmEntitySet entitySet = new Mock<IEdmEntitySet>().Object;

            Assert.Null(batchSegment.GetNavigationSource(entitySet));
        }

        [Fact]
        public void TryMatch_ReturnsTrue_ForBatchSegment()
        {
            // Arrange
            BatchPathSegment segment = new BatchPathSegment();
            BatchPathSegment batch = new BatchPathSegment();
            Dictionary<string, object> values = new Dictionary<string, object>();

            // Act
            bool result = segment.TryMatch(batch, values);

            // Assert
            Assert.True(result);
            Assert.Empty(values);
        }
    }
}
