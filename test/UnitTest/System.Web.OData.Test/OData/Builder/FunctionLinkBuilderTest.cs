// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.OData.Builder
{
    public class FunctionLinkBuilderTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EntityLinkFactory()
        {
            Assert.ThrowsArgumentNull(() => new FunctionLinkBuilder((Func<EntityInstanceContext, Uri>)null, true), "linkFactory");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_FeedLinkFactory()
        {
            Assert.ThrowsArgumentNull(() => new FunctionLinkBuilder((Func<FeedContext, Uri>)null, true), "linkFactory");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FollowsConventions_IsSpecifiedValue(bool value)
        {
            // Arrange
            FunctionLinkBuilder builder = new FunctionLinkBuilder((EntityInstanceContext a) => { throw new NotImplementedException(); },
                followsConventions: value);

            // Act
            bool followsConventions = builder.FollowsConventions;

            // Assert
            Assert.Equal(value, followsConventions);
        }

        [Fact]
        public void BuildFunctionLink_ForEntity_ReturnsLink()
        {
            // Arrange
            FunctionLinkBuilder builder = new FunctionLinkBuilder((EntityInstanceContext a) => new Uri("http://localhost:123"),
                followsConventions: true);
            EntityInstanceContext entityContext = new EntityInstanceContext();
            FeedContext feedContext = new FeedContext();

            // Act
            Uri link = builder.BuildFunctionLink(entityContext);
            Uri feedLink = builder.BuildFunctionLink(feedContext);

            // Assert
            Assert.NotNull(link);
            Assert.Equal("http://localhost:123/", link.AbsoluteUri);

            Assert.Null(feedLink);
        }

        [Fact]
        public void BuildFunctionLink_ForFeed_ReturnsLink()
        {
            // Arrange
            FunctionLinkBuilder builder = new FunctionLinkBuilder((FeedContext a) => new Uri("http://localhost:456"),
                followsConventions: true);
            EntityInstanceContext entityContext = new EntityInstanceContext();
            FeedContext feedContext = new FeedContext();

            // Act
            Uri link = builder.BuildFunctionLink(entityContext);
            Uri feedLink = builder.BuildFunctionLink(feedContext);

            // Assert
            Assert.Null(link);

            Assert.NotNull(feedLink);
            Assert.Equal("http://localhost:456/", feedLink.AbsoluteUri);
        }
    }
}
