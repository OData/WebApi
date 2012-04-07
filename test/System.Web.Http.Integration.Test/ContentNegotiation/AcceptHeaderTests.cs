// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Headers;
using Xunit;
using Xunit.Extensions;

namespace System.Web.Http.ContentNegotiation
{
    public class AcceptHeaderTests : ContentNegotiationTestBase
    {
        [Theory]
        [InlineData("application/json")]
        [InlineData("application/xml")]
        public void Response_Contains_ContentType(string contentType)
        {
            // Arrange
            MediaTypeWithQualityHeaderValue requestContentType = new MediaTypeWithQualityHeaderValue(contentType);
            MediaTypeHeaderValue responseContentType = null;

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, baseUri);
            request.Headers.Accept.Add(requestContentType);
            HttpResponseMessage response = httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            responseContentType = response.Content.Headers.ContentType;

            // Assert
            Assert.Equal(requestContentType.MediaType, responseContentType.MediaType);
        }
    }
}
