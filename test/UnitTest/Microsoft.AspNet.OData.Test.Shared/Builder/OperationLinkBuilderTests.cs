//-----------------------------------------------------------------------------
// <copyright file="OperationLinkBuilderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Test.Common;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder
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
