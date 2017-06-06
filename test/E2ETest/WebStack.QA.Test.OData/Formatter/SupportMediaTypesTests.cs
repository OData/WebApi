using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Builder;
using System.Web.OData.Formatter;
using Microsoft.OData.Edm;
using Nop.Core.Domain.Blogs;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using Xunit;

namespace WebStack.QA.Test.OData.Formatter
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

    public class SupportMediaTypeTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
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
        public void RequestJsonLightShouldWork()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                this.BaseAddress + "/JsonLight_BlogPosts");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));

            // Act
            var response = this.Client.SendAsync(request).Result;

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("odata.metadata", response.Content.Headers.ContentType.Parameters.First().Name);
            Assert.Equal("minimal", response.Content.Headers.ContentType.Parameters.First().Value);
        }

        [Fact]
        public void RequestAtomShouldNotWork()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                this.BaseAddress + "/JsonLight_BlogPosts");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/atom+xml"));

            // Act
            var response = this.Client.SendAsync(request).Result;

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("odata.metadata", response.Content.Headers.ContentType.Parameters.First().Name);
            Assert.Equal("minimal", response.Content.Headers.ContentType.Parameters.First().Value);
        }

        [Fact]
        public void RequestHasNoAcceptHeaderShouldNotWork()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                this.BaseAddress + "/JsonLight_BlogPosts");

            // Act
            var response = this.Client.SendAsync(request).Result;

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("odata.metadata", response.Content.Headers.ContentType.Parameters.First().Name);
            Assert.Equal("minimal", response.Content.Headers.ContentType.Parameters.First().Value);
        }

        [Fact]
        public void RequestXmlShouldWorkWithMetadata()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                this.BaseAddress + "/$metadata");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml"));

            // Act
            var response = this.Client.SendAsync(request).Result;

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
        }
    }
}
