// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.Test.AspNet.OData.Common;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Builder
{
    public class OperationLinkBuilderTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EntityLinkFactory()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new OperationLinkBuilder((Func<ResourceContext, Uri>)null, true), "linkFactory");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_FeedLinkFactory()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new OperationLinkBuilder((Func<ResourceSetContext, Uri>)null, true), "linkFactory");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FollowsConventions_IsSpecifiedValue(bool value)
        {
            // Arrange
            OperationLinkBuilder builder = new OperationLinkBuilder((ResourceContext a) => { throw new NotImplementedException(); },
                followsConventions: value);

            // Act
            bool followsConventions = builder.FollowsConventions;

            // Assert
            Assert.Equal(value, followsConventions);
        }

        [Fact]
        public void BuildOperationLink_ForEntity_ReturnsLink()
        {
            // Arrange
            OperationLinkBuilder builder = new OperationLinkBuilder((ResourceContext a) => new Uri("http://localhost:123"),
                followsConventions: true);
            ResourceContext entityContext = new ResourceContext();
            ResourceSetContext feedContext = new ResourceSetContext();

            // Act
            Uri link = builder.BuildLink(entityContext);
            Uri feedLink = builder.BuildLink(feedContext);

            // Assert
            Assert.NotNull(link);
            Assert.Equal("http://localhost:123/", link.AbsoluteUri);

            Assert.Null(feedLink);
        }

        [Fact]
        public void BuildOperationLink_ForFeed_ReturnsLink()
        {
            // Arrange
            OperationLinkBuilder builder = new OperationLinkBuilder((ResourceSetContext a) => new Uri("http://localhost:456"),
                followsConventions: true);
            ResourceContext entityContext = new ResourceContext();
            ResourceSetContext feedContext = new ResourceSetContext();

            // Act
            Uri link = builder.BuildLink(entityContext);
            Uri feedLink = builder.BuildLink(feedContext);

            // Assert
            Assert.Null(link);

            Assert.NotNull(feedLink);
            Assert.Equal("http://localhost:456/", feedLink.AbsoluteUri);
        }
    }
}
