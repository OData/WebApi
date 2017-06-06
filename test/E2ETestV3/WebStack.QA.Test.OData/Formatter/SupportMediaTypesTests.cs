using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter;
using Microsoft.Data.Edm;
using Nop.Core.Domain.Blogs;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using Xunit;

namespace WebStack.QA.Test.OData.Formatter
{
    [ODataJsonOnlyFormatting]
    public class JsonVerboseOnly_BlogPostsController : InMemoryEntitySetController<BlogPost, int> 
    {
        public JsonVerboseOnly_BlogPostsController()
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

    public class JsonVerboseOnlyTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel(configuration));
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebStack.QA.Common.WebHost.WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        private static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var builder = new ODataConventionModelBuilder(configuration);
            var post = builder.EntitySet<BlogPost>("JsonVerboseOnly_BlogPosts").EntityType;
            post.Ignore(p => p.BlogComments);
            post.Ignore(p => p.Language);

            return builder.GetEdmModel();
        }

        [Fact]
        public void RequestJsonVerboseShouldWork()
        {
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get, 
                this.BaseAddress + "/JsonVerboseOnly_BlogPosts");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=verbose"));

            var response = this.Client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("odata", response.Content.Headers.ContentType.Parameters.First().Name);
            Assert.Equal("verbose", response.Content.Headers.ContentType.Parameters.First().Value);
        }

        [Fact]
        public void RequestAtomShouldNotWork()
        {
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                this.BaseAddress + "/JsonVerboseOnly_BlogPosts");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/atom+xml"));

            var response = this.Client.SendAsync(request).Result;
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("odata", response.Content.Headers.ContentType.Parameters.First().Name);
            Assert.Equal("minimalmetadata", response.Content.Headers.ContentType.Parameters.First().Value);
        }

        [Fact]
        public void RequestHasNoAcceptHeaderShouldNotWork()
        {
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                this.BaseAddress + "/JsonVerboseOnly_BlogPosts");

            var response = this.Client.SendAsync(request).Result;
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("odata", response.Content.Headers.ContentType.Parameters.First().Name);
            Assert.Equal("minimalmetadata", response.Content.Headers.ContentType.Parameters.First().Value);
        }

        [Fact]
        public void RequestXmlShouldWorkWithMetadata()
        {
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                this.BaseAddress + "/$metadata");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml"));

            var response = this.Client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
        }
    }
}
