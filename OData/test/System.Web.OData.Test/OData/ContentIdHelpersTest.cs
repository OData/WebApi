// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.TestCommon;

namespace System.Web.OData
{
    public class ContentIdHelpersTest
    {
        [Theory]
        [InlineData("1", "$1/Orders", "http://localhost/OData/Customers(42)/Orders")]
        [InlineData("10", "http://localhost/$10/Orders(42)", "http://localhost/OData/Customers(42)/Orders(42)")]
        [InlineData("a", "http://localhost/$a/Orders(42)", "http://localhost/OData/Customers(42)/Orders(42)")]
        [InlineData("A", "http://localhost/$A/Orders(42)", "http://localhost/OData/Customers(42)/Orders(42)")]
        [InlineData("-", "http://localhost/$-/Orders(42)", "http://localhost/OData/Customers(42)/Orders(42)")]
        [InlineData(".", "http://localhost/$./Orders(42)", "http://localhost/OData/Customers(42)/Orders(42)")]
        [InlineData("_", "http://localhost/$_/Orders(42)", "http://localhost/OData/Customers(42)/Orders(42)")]
        [InlineData("~", "http://localhost/$~/Orders(42)", "http://localhost/OData/Customers(42)/Orders(42)")]
        [InlineData("0aA-._~9zZ", "http://localhost/$0aA-._~9zZ/Orders(42)", "http://localhost/OData/Customers(42)/Orders(42)")]
        [InlineData("1", "http://localhost/NoContentID", "http://localhost/NoContentID")]
        public void ResolveContentId_ResolvesContentIDInUrl(string key, string url, string expectedResolvedUrl)
        {
            // Arrange
            Dictionary<string, string> contentIdToLocationMapping = new Dictionary<string, string>();
            contentIdToLocationMapping.Add(key, "http://localhost/OData/Customers(42)");

            // Act
            string resolvedUrl = ContentIdHelpers.ResolveContentId(url, contentIdToLocationMapping);

            // Assert
            Assert.Equal(expectedResolvedUrl, resolvedUrl);
        }

        [Theory]
        [InlineData("$1/Orders(42)", "http://localhost/OData/Customers(1)/Orders(42)")]
        [InlineData("$9/Orders(42)", "http://localhost/OData/Customers(9)/Orders(42)")]
        [InlineData("$10/Orders(42)", "http://localhost/OData/Customers(10)/Orders(42)")]
        [InlineData("$99/Orders(42)", "http://localhost/OData/Customers(99)/Orders(42)")]
        [InlineData("$100/Orders(42)", "http://localhost/OData/Customers(100)/Orders(42)")]
        [InlineData("$101/Orders(42)", "$101/Orders(42)")]
        [InlineData("$1000/Orders(42)", "$1000/Orders(42)")]
        [InlineData("http://localhost/$1/Orders(42)", "http://localhost/OData/Customers(1)/Orders(42)")]
        [InlineData("http://localhost/$9/Orders(42)", "http://localhost/OData/Customers(9)/Orders(42)")]
        [InlineData("http://localhost/$10/Orders(42)", "http://localhost/OData/Customers(10)/Orders(42)")]
        [InlineData("http://localhost/$100/Orders(42)", "http://localhost/OData/Customers(100)/Orders(42)")]
        [InlineData("http://localhost/$101/Orders(42)", "http://localhost/$101/Orders(42)")]
        [InlineData("http://localhost/$1000/Orders(42)", "http://localhost/$1000/Orders(42)")]
        public void ResolveContentId_ResolvesExactContentID(string url, string expectedResolvedUrl)
        {
            // Arrange
            Dictionary<string, string> contentIdToLocationMapping = new Dictionary<string, string>();
            for (int id = 1; id < 101; id++)
            {
                contentIdToLocationMapping.Add(id.ToString(), string.Format("http://localhost/OData/Customers({0})", id));
            }

            // Act
            string resolvedUrl = ContentIdHelpers.ResolveContentId(url, contentIdToLocationMapping);

            // Assert
            Assert.Equal(expectedResolvedUrl, resolvedUrl);
        }

        [Theory]
        [InlineData("1", "http://localhost/$10/Orders(42)")]
        [InlineData("11", "http://localhost/$111/Orders(42)")]
        [InlineData("99", "http://localhost/$100/Orders(42)")]
        [InlineData("10", "http://localhost/$1/Orders(42)")]
        [InlineData("11", "http://localhost/$1/Orders(42)")]
        [InlineData("B", "http://localhost/$B-/Orders(42)")]
        [InlineData("c", "http://localhost/$c~/Orders(42)")]
        [InlineData("-", "http://localhost/$-a/Orders(42)")]
        [InlineData(".", "http://localhost/$.1/Orders(42)")]
        [InlineData("_", "http://localhost/$_./Orders(42)")]
        [InlineData("~~", "http://localhost/$~~_/Orders(42)")]
        [InlineData("123", "http://localhost/$1/Orders(42)")]
        [InlineData("a1B~", "http://localhost/$~/Orders(42)")]
        public void ResolveContentId_CannotResolveUnmatchedContentIDInUrl(string key, string url)
        {
            // Arrange
            Dictionary<string, string> contentIdToLocationMapping = new Dictionary<string, string>();
            contentIdToLocationMapping.Add(key, "http://localhost/OData/Customers(42)");

            // Act
            string resolvedUrl = ContentIdHelpers.ResolveContentId(url, contentIdToLocationMapping);

            // Assert
            Assert.Equal(url, resolvedUrl);
        }

        [Fact]
        public void ResolveContentId_CannotResolveInvalidContentIDInUrl()
        {
            // Arrange
            string url = "$$/Orders";
            Dictionary<string, string> contentIdToLocationMapping = new Dictionary<string, string>();
            contentIdToLocationMapping.Add("$", "http://localhost/OData/Customers(42)");

            // Act
            string resolvedUrl = ContentIdHelpers.ResolveContentId(url, contentIdToLocationMapping);

            // Assert
            Assert.Equal(url, resolvedUrl);
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
