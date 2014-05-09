// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.TestCommon;

namespace System.Web.OData
{
    public class ContentIdHelpersTest
    {
        [Theory]
        [InlineData("$1/Orders", "http://localhost/OData/Customers(42)/Orders")]
        [InlineData("http://localhost/$1/Orders(42)", "http://localhost/OData/Customers(42)/Orders(42)")]
        [InlineData("http://localhost/NoContentID", "http://localhost/NoContentID")]
        public void ResolveContentId_ResolvesContentIDInUrl(string url, string expectedResolvedUrl)
        {
            Dictionary<string, string> contentIdToLocationMapping = new Dictionary<string, string>();
            contentIdToLocationMapping.Add("1", "http://localhost/OData/Customers(42)");

            string resolvedUrl = ContentIdHelpers.ResolveContentId(url, contentIdToLocationMapping);

            Assert.Equal(expectedResolvedUrl, resolvedUrl);
        }

        [Fact]
        public void AddLocationHeaderToMapping_AddsContentIdToLocationMapping()
        {
            // Arrange
            var response = new HttpResponseMessage();
            response.Headers.Location = new Uri("http://any");
            var contentIdToLocationMapping = new Dictionary<string, string>();
            var contentId = Guid.NewGuid().ToString();

            // Act
            ContentIdHelpers.AddLocationHeaderToMapping(response, contentIdToLocationMapping, contentId);

            // Assert
            Assert.True(contentIdToLocationMapping.ContainsKey(contentId));
            Assert.Equal(response.Headers.Location.AbsoluteUri, contentIdToLocationMapping[contentId]);
        }

        [Fact]
        public void AddLocationHeaderToMapping_DoesNotAddContentIdToLocationMapping_IfLocationIsNull()
        {
            // Arrange
            var response = new HttpResponseMessage();
            var contentIdToLocationMapping = new Dictionary<string, string>();
            var contentId = Guid.NewGuid().ToString();

            // Act
            ContentIdHelpers.AddLocationHeaderToMapping(response, contentIdToLocationMapping, contentId);

            // Assert
            Assert.False(contentIdToLocationMapping.ContainsKey(contentId));
        }
    }
}
