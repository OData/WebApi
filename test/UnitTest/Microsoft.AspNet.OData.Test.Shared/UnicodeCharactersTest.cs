// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
#if !NETCORE
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
#endif
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class UnicodeCharactersTest
    {
        [Fact]
        public async Task PostEntity_WithUnicodeCharactersInKey()
        {
            // Arrange
            const string payload = "{" +
                "\"LogonName\":\"Ärne Bjørn\"," +
                "\"Email\":\"ärnebjørn@test.com\"" +
                "}";

            const string uri = "http://localhost/odata/UnicodeCharUsers";
            const string expectedLocation = "http://localhost/odata/UnicodeCharUsers('%C3%84rne%20Bj%C3%B8rn')";

            HttpClient client = GetClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(expectedLocation, response.Headers.Location.AbsoluteUri);
        }

        [Fact]
        public async Task PostEntity_WithUnicodeCharactersInKey_PreferNoContent()
        {
            // Arrange
            const string payload = "{" +
                "\"LogonName\":\"Ärne Bjørn\"," +
                "\"Email\":\"ärnebjørn@test.com\"" +
                "}";

            const string uri = "http://localhost/odata/UnicodeCharUsers";
            const string preferNoContent = "return=minimal";
            const string expectedLocation = "http://localhost/odata/UnicodeCharUsers('%C3%84rne%20Bj%C3%B8rn')";

            HttpClient client = GetClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.TryAddWithoutValidation("Prefer", preferNoContent);

            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(expectedLocation, response.Headers.Location.AbsoluteUri);
            IEnumerable<string> values;
            Assert.True(response.Headers.TryGetValues("OData-EntityId", out values));
            Assert.Single(values);
            Assert.Equal(expectedLocation, values.First());
        }

        private static HttpClient GetClient()
        {
            ODataConventionModelBuilder modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.EntitySet<UnicodeCharUser>("UnicodeCharUsers");

            var controllers = new[] { typeof(MetadataController), typeof(UnicodeCharUsersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.Select().OrderBy().Filter().Expand().Count().MaxTop(null);
                config.MapODataServiceRoute("odata", "odata", modelBuilder.GetEdmModel());
            });

            return TestServerFactory.CreateClient(server);
        }
    }

    public class UnicodeCharUsersController : TestODataController
    {
        public ITestActionResult Post([FromBody] UnicodeCharUser user)
        {
            return Created(user);
        }
    }

    public class UnicodeCharUser
    {
        [Key]
        public string LogonName { get; set; }
        public string Email { get; set; }
    }
}
