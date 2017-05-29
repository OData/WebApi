// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.OData.Builder
{
    public class ActionLinkBuilderTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EntityLinkFactory()
        {
            Assert.ThrowsArgumentNull(() => new ActionLinkBuilder((Func<EntityInstanceContext, Uri>)null, true), "linkFactory");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_FeedLinkFactory()
        {
            Assert.ThrowsArgumentNull(() => new ActionLinkBuilder((Func<FeedContext, Uri>)null, true), "linkFactory");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FollowsConventions_IsSpecifiedValue(bool value)
        {
            // Arrange
            ActionLinkBuilder builder = new ActionLinkBuilder((EntityInstanceContext a) => { throw new NotImplementedException(); },
                followsConventions: value);

            // Act
            bool followsConventions = builder.FollowsConventions;

            // Assert
            Assert.Equal(value, followsConventions);
        }

        [Fact]
        public void BuildActionLink_ForEntity_ReturnsLink()
        {
            // Arrange
            ActionLinkBuilder builder = new ActionLinkBuilder((EntityInstanceContext a) => new Uri("http://localhost:123"),
                followsConventions: true);
            EntityInstanceContext entityContext = new EntityInstanceContext();
            FeedContext feedContext = new FeedContext();

            // Act
            Uri link = builder.BuildActionLink(entityContext);
            Uri feedLink = builder.BuildActionLink(feedContext);

            // Assert
            Assert.NotNull(link);
            Assert.Equal("http://localhost:123/", link.AbsoluteUri);

            Assert.Null(feedLink);
        }

        [Fact]
        public void BuildActionLink_ForFeed_ReturnsLink()
        {
            // Arrange
            ActionLinkBuilder builder = new ActionLinkBuilder((FeedContext a) => new Uri("http://localhost:456"),
                followsConventions: true);
            EntityInstanceContext entityContext = new EntityInstanceContext();
            FeedContext feedContext = new FeedContext();

            // Act
            Uri link = builder.BuildActionLink(entityContext);
            Uri feedLink = builder.BuildActionLink(feedContext);

            // Assert
            Assert.Null(link);

            Assert.NotNull(feedLink);
            Assert.Equal("http://localhost:456/", feedLink.AbsoluteUri);
        }
    }
}
