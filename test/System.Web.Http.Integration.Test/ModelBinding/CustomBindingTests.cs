// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using Xunit;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// End to end functional tests for model binding via custom providers
    /// </summary>
    public class CustomBindingTests : ModelBindingTests
    {
        [Fact]
        public void Custom_ValueProvider_Binds_Simple_Types_Get()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(baseAddress + String.Format("ModelBinding/{0}", "GetIntCustom")),
                Method = HttpMethod.Get
            };

            request.Headers.Add("value", "5");

            // Act
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Assert
            string responseString = response.Content.ReadAsStringAsync().Result;
            Assert.Equal<string>("5", responseString);
        }

    }
}