//-----------------------------------------------------------------------------
// <copyright file="SupportMediaTypesTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Nop.Core.Domain.Blogs;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter
{
    public class JsonLight_BlogPostsController : InMemoryODataController<BlogPost, int>
    {
        public JsonLight_BlogPostsController()
            : base("Id")
        {
        }
    }

    public class SupportMediaTypeTests : WebHostTestBase
    {
        public SupportMediaTypeTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel(configuration));
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            var post = builder.EntitySet<BlogPost>("JsonLight_BlogPosts").EntityType;
            post.Ignore(p => p.BlogComments);
            post.Ignore(p => p.Language);

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task RequestJsonLightShouldWork()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                this.BaseAddress + "/JsonLight_BlogPosts");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));

            // Act
            var response = await this.Client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("odata.metadata", response.Content.Headers.ContentType.Parameters.First().Name);
            Assert.Equal("minimal", response.Content.Headers.ContentType.Parameters.First().Value);
        }

        [Fact]
        public async Task RequestAtomShouldNotWork()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                this.BaseAddress + "/JsonLight_BlogPosts");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/atom+xml"));

            // Act
            var response = await this.Client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("odata.metadata", response.Content.Headers.ContentType.Parameters.First().Name);
            Assert.Equal("minimal", response.Content.Headers.ContentType.Parameters.First().Value);
        }

        [Fact]
        public async Task RequestHasNoAcceptHeaderShouldNotWork()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                this.BaseAddress + "/JsonLight_BlogPosts");

            // Act
            var response = await this.Client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("odata.metadata", response.Content.Headers.ContentType.Parameters.First().Name);
            Assert.Equal("minimal", response.Content.Headers.ContentType.Parameters.First().Value);
        }

        [Fact]
        public async Task RequestXmlShouldWorkWithMetadata()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                this.BaseAddress + "/$metadata");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml"));

            // Act
            var response = await this.Client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
        }
    }
}
