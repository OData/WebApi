// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder.Conventions
{
    public class SelfLinksGenerationConventionTest
    {
        [Fact]
        public void Apply_AddsFeedSelfLink()
        {
            // Arrange
            var mockEntitySet = new Mock<IEntitySetConfiguration>();
            mockEntitySet.Setup(entitySet => entitySet.GetFeedSelfLink()).Returns((Func<FeedContext, Uri>)null).Verifiable();
            mockEntitySet.Setup(entitySet => entitySet.HasFeedSelfLink(It.IsAny<Func<FeedContext, Uri>>())).Returns(mockEntitySet.Object).Verifiable();

            var mockModelBuilder = new Mock<ODataModelBuilder>();

            // Act
            new SelfLinksGenerationConvention().Apply(mockEntitySet.Object, mockModelBuilder.Object);

            // Assert
            mockEntitySet.Verify();
        }

        [Fact]
        public void Apply_AddsFeedSelfLink_ThatThrowsForMissingRoute()
        {
            // Arrange
            Func<FeedContext, Uri> feedSelfLink = null;
            var mockEntitySet = new Mock<IEntitySetConfiguration>();
            mockEntitySet.Setup(entitySet => entitySet.HasFeedSelfLink(It.IsAny<Func<FeedContext, Uri>>()))
                .Returns(mockEntitySet.Object)
                .Callback<Func<FeedContext, Uri>>(selfLink => { feedSelfLink = selfLink; });

            var mockModelBuilder = new Mock<ODataModelBuilder>();

            HttpConfiguration configuration = new HttpConfiguration();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Properties["MS_HttpConfiguration"] = configuration;
            FeedContext context = new FeedContext(new Mock<IEdmEntitySet>().Object, new UrlHelper(request), new Product[0]);

            // Act
            new SelfLinksGenerationConvention().Apply(mockEntitySet.Object, mockModelBuilder.Object);

            // Assert
            Assert.NotNull(feedSelfLink);
            Assert.ThrowsArgument(() => feedSelfLink(context), "name",
                "A route named 'OData.Default' could not be found in the route collection");
        }

        [Fact]
        public void Apply_DoesNotAddFeedSelfLink_IfOneIsPresent()
        {
            // Arrange
            var mockEntitySet = new Mock<IEntitySetConfiguration>();
            mockEntitySet.Setup(entitySet => entitySet.GetFeedSelfLink()).Returns(feedContext => new Uri("http://www.cool.com")).Verifiable();

            var mockModelBuilder = new Mock<ODataModelBuilder>();

            // Act
            new SelfLinksGenerationConvention().Apply(mockEntitySet.Object, mockModelBuilder.Object);

            // Assert
            mockEntitySet.Verify();
            mockEntitySet.Verify(entitySet => entitySet.HasFeedSelfLink(It.IsAny<Func<FeedContext, Uri>>()), Times.Never());
        }

        class SelfLinkConventionTests_EntityType
        {
            public string ID { get; set; }
        }
    }
}
