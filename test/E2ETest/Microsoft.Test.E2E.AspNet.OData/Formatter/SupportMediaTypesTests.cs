// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Nop.Core.Domain.Blogs;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter
{
    [ODataJsonOnlyFormatting]
    public class JsonLight_BlogPostsController : InMemoryODataController<BlogPost, int>
    {
        public JsonLight_BlogPostsController()
            : base("Id")
        {
        }
    }

    public class ODataJsonOnlyFormattingAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            controllerSettings.Formatters.Clear();
            var odataFormatters = ODataMediaTypeFormatters.Create();
            odataFormatters = odataFormatters.Where(f =>
                f.SupportedMediaTypes.Any(t =>
                    string.Equals(t.MediaType, "application/json", StringComparison.InvariantCultureIgnoreCase))).ToList();
            controllerSettings.Formatters.InsertRange(0, odataFormatters);
        }
    }

    public class SupportMediaTypeTests : WebHostTestBase
    {
        public SupportMediaTypeTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel(configuration));
        }

        private static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var builder = new ODataConventionModelBuilder(configuration);
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
