// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.Conventions;
using System.Web.Http.OData.TestCommon;
using System.Web.Http.OData.TestCommon.Models;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.OData.Formatter
{
    public class ODataMediaTypeFormatterTests
    {
        [Fact]
        [Trait("Description", "ODataMediaTypeFormatter is public, concrete, and unsealed.")]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(ODataMediaTypeFormatter), TypeAssert.TypeProperties.IsPublicVisibleClass, typeof(MediaTypeFormatter));
        }

        [Theory]
        [InlineData("application/atom+xml")]
        [InlineData("application/json;odata=verbose")]
        [Trait("Description", "ODataMediaTypeFormatter() constructor sets standard Atom+xml/Json media types in SupportedMediaTypes.")]
        public void Constructor(string mediaType)
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter();
            Assert.True(formatter.SupportedMediaTypes.Contains(MediaTypeHeaderValue.Parse(mediaType)), string.Format("SupportedMediaTypes should have included {0}.", mediaType.ToString()));
        }

        [Fact]
        [Trait("Description", "DefaultMediaType property returns application/atom+xml.")]
        public void DefaultMediaTypeReturnsApplicationAtomXml()
        {
            MediaTypeHeaderValue mediaType = ODataMediaTypeFormatter.DefaultMediaType;
            Assert.NotNull(mediaType);
            Assert.Equal("application/atom+xml", mediaType.MediaType);
        }

        [Fact]
        [Trait("Description", "WriteToStreamAsync() returns the expected OData representation for the type.")]
        public void WriteToStreamAsyncReturnsODataRepresentation()
        {
            ODataConventionModelBuilder model = new ODataConventionModelBuilder();
            model.EntitySet<WorkItem>("WorkItems");

            HttpConfiguration configuration = new HttpConfiguration();
            var route = configuration.Routes.MapHttpRoute(ODataRouteNames.GetById, "{controller}({id})");
            configuration.Routes.MapHttpRoute(ODataRouteNames.PropertyNavigation, "{controller}({id})/{property}");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/WorkItems(10)");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(route);

            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(model.GetEdmModel()) { Request = request };

            ObjectContent<WorkItem> content = new ObjectContent<WorkItem>((WorkItem)TypeInitializer.GetInstance(SupportedTypes.WorkItem), formatter);

            RegexReplacement replaceUpdateTime = new RegexReplacement("<updated>*.*</updated>", "<updated>UpdatedTime</updated>");
            Assert.Xml.Equal(BaselineResource.TestEntityWorkItem, content.ReadAsStringAsync().Result, regexReplacements: replaceUpdateTime);
        }

        [Theory]
        [InlineData(null, null, "3.0")]
        [InlineData("1.0", null, "1.0")]
        [InlineData("2.0", null, "2.0")]
        [InlineData("3.0", null, "3.0")]
        [InlineData(null, "1.0", "1.0")]
        [InlineData(null, "2.0", "2.0")]
        [InlineData(null, "3.0", "3.0")]
        [InlineData("1.0", "1.0", "1.0")]
        [InlineData("1.0", "2.0", "2.0")]
        [InlineData("1.0", "3.0", "3.0")]
        public void SetDefaultContentHeaders_SetsRightODataServiceVersion(string requestDataServiceVersion, string requestMaxDataServiceVersion, string expectedDataServiceVersion)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            if (requestDataServiceVersion != null)
            {
                request.Headers.TryAddWithoutValidation("DataServiceVersion", requestDataServiceVersion);
            }
            if (requestMaxDataServiceVersion != null)
            {
                request.Headers.TryAddWithoutValidation("MaxDataServiceVersion", requestMaxDataServiceVersion);
            }

            HttpContentHeaders contentHeaders = new StringContent("").Headers;

            new ODataMediaTypeFormatter()
            .GetPerRequestFormatterInstance(typeof(int), request, MediaTypeHeaderValue.Parse("application/xml"))
            .SetDefaultContentHeaders(typeof(int), contentHeaders, MediaTypeHeaderValue.Parse("application/xml"));

            IEnumerable<string> headervalues;
            Assert.True(contentHeaders.TryGetValues("DataServiceVersion", out headervalues));
            Assert.Equal(headervalues, new string[] { expectedDataServiceVersion + ";" });
        }
    }
}
