// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.OData.Builder
{
    public class OperationLinkBuilderTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EntityLinkFactory()
        {
            Assert.ThrowsArgumentNull(() => new OperationLinkBuilder((Func<ResourceContext, Uri>)null, true), "linkFactory");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_FeedLinkFactory()
        {
            Assert.ThrowsArgumentNull(() => new OperationLinkBuilder((Func<FeedContext, Uri>)null, true), "linkFactory");
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
            FeedContext feedContext = new FeedContext();

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
            OperationLinkBuilder builder = new OperationLinkBuilder((FeedContext a) => new Uri("http://localhost:456"),
                followsConventions: true);
            ResourceContext entityContext = new ResourceContext();
            FeedContext feedContext = new FeedContext();

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
