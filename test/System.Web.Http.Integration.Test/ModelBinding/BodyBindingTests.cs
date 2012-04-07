// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using Xunit;
using Xunit.Extensions;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// End to end functional tests for model binding via request body
    /// </summary>
    public class BodyBindingTests : ModelBindingTests
    {
        [Fact]
        public void Body_Bad_Input_Receives_Validation_Error()
        {
            // Arrange
            string formUrlEncodedString = "Id=101&Name=testFirstNameTooLong";
            StringContent stringContent = new StringContent(formUrlEncodedString, Encoding.UTF8, "application/x-www-form-urlencoded");

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(baseAddress + "ModelBinding/PostComplexWithValidation"),
                Method = HttpMethod.Post,
                Content = stringContent,
            };

            // Act
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Failed to bind customer.RequiredValue. The errors are:\nErrorMessage: The RequiredValue property is required.\nFailed to bind customer.Name. The errors are:\nErrorMessage: The field Name must be a string with a maximum length of 6.\n", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Body_Good_Input_Succeed()
        {
            // Arrange
            string formUrlEncodedString = "Id=111&Name=John&RequiredValue=9";
            StringContent stringContent = new StringContent(formUrlEncodedString, Encoding.UTF8, "application/x-www-form-urlencoded");

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(baseAddress + "ModelBinding/PostComplexWithValidation"),
                Method = HttpMethod.Post,
                Content = stringContent,
            };

            // Act
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("111", response.Content.ReadAsStringAsync().Result);
        }

        [Theory]
        [InlineData("PostComplexType", "application/json")]
        [InlineData("PostComplexType", "application/xml")]
        [InlineData("PostComplexTypeFromBody", "application/json")]
        [InlineData("PostComplexTypeFromBody", "application/xml")]
        public void Body_Binds_ComplexType_Type_Key_Value_Read(string action, string mediaType)
        {
            // Arrange
            ModelBindOrder expectedItem = new ModelBindOrder()
            {
                ItemName = "Bike",
                Quantity = 1,
                Customer = new ModelBindCustomer { Name = "Fred" }
            };
            var formatter = new MediaTypeFormatterCollection().FindWriter(typeof(ModelBindOrder), new MediaTypeHeaderValue(mediaType));
            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = new ObjectContent<ModelBindOrder>(expectedItem, formatter),
                RequestUri = new Uri(baseAddress + String.Format("ModelBinding/{0}", action)),
                Method = HttpMethod.Post,
            };

            // Act
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Assert
            ModelBindOrder actualItem = response.Content.ReadAsAsync<ModelBindOrder>().Result;
            Assert.Equal<ModelBindOrder>(expectedItem, actualItem, new ModelBindOrderEqualityComparer());
        }

        [Theory]
        [InlineData("PostComplexType", "application/json")]
        [InlineData("PostComplexType", "application/xml")]
        [InlineData("PostComplexTypeFromBody", "application/json")]
        [InlineData("PostComplexTypeFromBody", "application/xml")]
        public void Body_Binds_ComplexType_Type_Whole_Body_Read(string action, string mediaType)
        {
            // Arrange
            ModelBindOrder expectedItem = new ModelBindOrder()
            {
                ItemName = "Bike",
                Quantity = 1,
                Customer = new ModelBindCustomer { Name = "Fred" }
            };
            var formatter = new MediaTypeFormatterCollection().FindWriter(typeof(ModelBindOrder), new MediaTypeHeaderValue(mediaType));
            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = new ObjectContent<ModelBindOrder>(expectedItem, formatter),
                RequestUri = new Uri(baseAddress + String.Format("ModelBinding/{0}", action)),
                Method = HttpMethod.Post,
            };

            // Act
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Assert
            ModelBindOrder actualItem = response.Content.ReadAsAsync<ModelBindOrder>().Result;
            Assert.Equal<ModelBindOrder>(expectedItem, actualItem, new ModelBindOrderEqualityComparer());
        }
    }
}
