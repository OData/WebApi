// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using Xunit;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// End to end functional tests for model binding via routes
    /// </summary>
    public class RouteBindingTests : ModelBindingTests
    {
        [Fact]
        public void Route_Binds_Simple_Types_Get()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(baseAddress + "ModelBinding/GetStringFromRoute"),
                Method = HttpMethod.Get
            };

            // Act
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Assert
            string responseString = response.Content.ReadAsStringAsync().Result;
            Assert.Equal<string>("\"ModelBinding:GetStringFromRoute\"", responseString);
        }
    }
}