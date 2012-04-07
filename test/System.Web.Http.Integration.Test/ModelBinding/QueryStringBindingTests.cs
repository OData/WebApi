// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using Xunit;
using Xunit.Extensions;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// End to end functional tests for model binding via query strings
    /// </summary>
    public class QueryStringBindingTests : ModelBindingTests
    {
        [Theory]
        [InlineData("GetString", "?value=test", "\"test\"")]
        [InlineData("GetInt", "?value=99", "99")]
        [InlineData("GetBool", "?value=false", "false")]
        [InlineData("GetBool", "?value=true", "true")]
        [InlineData("GetIntWithDefault", "?value=99", "99")]    // action has default, but we provide value
        [InlineData("GetIntWithDefault", "", "-1")]             // action has default, we provide no value
        [InlineData("GetIntFromUri", "?value=99", "99")]        // [FromUri]
        [InlineData("GetIntPrefixed", "?somePrefix=99", "99")]  // [FromUri(Prefix=somePrefix)]
        [InlineData("GetIntAsync", "?value=5", "5")]
        public void Query_String_Binds_Simple_Types_Get(string action, string queryString, string expectedResponse)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(baseAddress + String.Format("ModelBinding/{0}{1}", action, queryString)),
                Method = HttpMethod.Get
            };

            // Act
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Assert
            string responseString = response.Content.ReadAsStringAsync().Result;
            Assert.Equal<string>(expectedResponse, responseString);
        }

        [Theory]
        [InlineData("PostString", "?value=test", "\"test\"")]
        [InlineData("PostInt", "?value=99", "99")]
        [InlineData("PostBool", "?value=false", "false")]
        [InlineData("PostBool", "?value=true", "true")]
        [InlineData("PostIntFromUri", "?value=99", "99")]           // [FromUri]
        [InlineData("PostIntUriPrefixed", "?somePrefix=99", "99")]  // [FromUri(Prefix=somePrefix)]
        [InlineData("PostIntArray", "?value={[1,2,3]}", "0")]       // TODO: DevDiv2 333257 -- make this array real when fix JsonValue array model binding
        public void Query_String_Binds_Simple_Types_Post(string action, string queryString, string expectedResponse)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(baseAddress + String.Format("ModelBinding/{0}{1}", action, queryString)),
                Method = HttpMethod.Post
            };

            // Act
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Assert
            string responseString = response.Content.ReadAsStringAsync().Result;
            Assert.Equal<string>(expectedResponse, responseString);
        }

        [Theory]
        [InlineData("GetComplexTypeFromUri", "itemName=Tires&quantity=2&customer.Name=Sue", "Tires", 2, "Sue")]
        public void Query_String_ComplexType_Type_Get(string action, string queryString, string itemName, int quantity, string customerName)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(baseAddress + String.Format("ModelBinding/{0}?{1}", action, queryString)),
                Method = HttpMethod.Get
            };

            ModelBindOrder expectedItem = new ModelBindOrder()
            {
                ItemName = itemName,
                Quantity = quantity,
                Customer = new ModelBindCustomer { Name = customerName }
            };

            // Act
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Assert
            ModelBindOrder actualItem = response.Content.ReadAsAsync<ModelBindOrder>().Result;
            Assert.Equal<ModelBindOrder>(expectedItem, actualItem, new ModelBindOrderEqualityComparer());
        }

        [Theory]
        [InlineData("PostComplexTypeFromUri", "itemName=Tires&quantity=2&customer.Name=Bob", "Tires", 2, "Bob")]
        public void Query_String_ComplexType_Type_Post(string action, string queryString, string itemName, int quantity, string customerName)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(baseAddress + String.Format("ModelBinding/{0}?{1}", action, queryString)),
                Method = HttpMethod.Post
            };
            ModelBindOrder expectedItem = new ModelBindOrder()
            {
                ItemName = itemName,
                Quantity = quantity,
                Customer = new ModelBindCustomer { Name = customerName }
            };

            // Act
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Assert
            ModelBindOrder actualItem = response.Content.ReadAsAsync<ModelBindOrder>().Result;
            Assert.Equal<ModelBindOrder>(expectedItem, actualItem, new ModelBindOrderEqualityComparer());
        }
    }
}