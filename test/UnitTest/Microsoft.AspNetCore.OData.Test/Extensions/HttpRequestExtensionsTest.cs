//-----------------------------------------------------------------------------
// <copyright file="HttpRequestExtensionsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class HttpRequestMessageExtensionsTest
    {
        [Theory]
        [InlineData("http://localhost/Customers", 10, "http://localhost/Customers?$skip=10")]
        [InlineData("http://localhost/Customers?$filter=Age ge 18", 10, "http://localhost/Customers?$filter=Age%20ge%2018&$skip=10")]
        [InlineData("http://localhost/Customers?$top=20", 10, "http://localhost/Customers?$top=10&$skip=10")]
        [InlineData("http://localhost/Customers?$skip=5&$top=10", 2, "http://localhost/Customers?$top=8&$skip=7")]
        [InlineData("http://localhost/Customers?$filter=Age ge 18&$orderby=Name&$top=11&$skip=6", 10, "http://localhost/Customers?$filter=Age%20ge%2018&$orderby=Name&$top=1&$skip=16")]
        [InlineData("http://localhost/Customers?testkey%23%2B%3D%3F%26=testvalue%23%2B%3D%3F%26", 10, "http://localhost/Customers?testkey%23%2B%3D%3F%26=testvalue%23%2B%3D%3F%26&$skip=10")]
        public void GetNextPageLink_GetsNextPageLink(string requestUri, int pageSize, string nextPageUri)
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethod.Get, requestUri);

            // Act
            Uri nextPageLink = request.GetNextPageLink(pageSize);

            // Assert
            Assert.Equal(nextPageUri, nextPageLink.AbsoluteUri);
        }
    
        [Theory]
        [InlineData("http://localhost/Customers", 10, "http://localhost/Customers?$skip=10")]
        [InlineData("https://localhost/Customers", 10, "https://localhost/Customers?$skip=10")]
        [InlineData("http://example.com/Customers", 10, "http://example.com/Customers?$skip=10")]
        [InlineData("https://example.com/Customers", 10, "https://example.com/Customers?$skip=10")]
        public void GetNextPageLink_UsesNoPortWhenNotSpecified(string requestUri, int pageSize, string nextPageUri)
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethod.Get, requestUri);

            // Act
            Uri nextPageLink = request.GetNextPageLink(pageSize);

            // Assert
            Assert.Equal(nextPageUri, nextPageLink.AbsoluteUri);
        }

        [Theory]
        [InlineData("http://example.com:80/Customers", 10, "http://example.com/Customers?$skip=10")]
        [InlineData("https://example.com:443/Customers", 10, "https://example.com/Customers?$skip=10")]
        public void GetNextPageLink_UsesPortWhenDefaultPortIsProvided(string requestUri, int pageSize, string nextPageUri)
        {
            // Arrange
            // The RequestFactory removes the default port, so we explicitly add it
            HttpRequest request = RequestFactory.Create(HttpMethod.Get, requestUri);
            Uri uri = new Uri(requestUri);
            var port = request.Scheme == "http" ? 80 : 443;
            request.Host = new HostString(uri.Host, port);

            // Act
            Uri nextPageLink = request.GetNextPageLink(pageSize);

            // Assert
            Assert.Equal(nextPageUri, nextPageLink.AbsoluteUri);
        }

        [Theory]
        [InlineData("http://localhost:808/Customers", 10, "http://localhost:808/Customers?$skip=10")]
        [InlineData("https://localhost:4443/Customers", 10, "https://localhost:4443/Customers?$skip=10")]
        [InlineData("http://localhost:5000/Customers", 10, "http://localhost:5000/Customers?$skip=10")]
        public void GetNextPageLink_UsesCorrectPortWhenSpecified(string requestUri, int pageSize, string nextPageUri)
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethod.Get, requestUri);

            // Act
            Uri nextPageLink = request.GetNextPageLink(pageSize);

            // Assert
            Assert.Equal(nextPageUri, nextPageLink.AbsoluteUri);
        }
    }
}
