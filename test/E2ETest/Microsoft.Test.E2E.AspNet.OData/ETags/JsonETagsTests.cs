// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.V1;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ETags
{
    public class JsonETagsTests : WebHostTestBase
    {
        public JsonETagsTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MessageHandlers.Add(new ETagMessageHandler());
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<ETagsCustomer> eTagsCustomersSet = builder.EntitySet<ETagsCustomer>("ETagsCustomers");
            EntityTypeConfiguration<ETagsCustomer> eTagsCustomers = eTagsCustomersSet.EntityType;
            eTagsCustomers.Property(c => c.Id).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.Name).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.BoolProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.ByteProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.CharProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.DecimalProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.DoubleProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.ShortProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.LongProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.SbyteProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.FloatProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.UshortProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.UintProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.UlongProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.GuidProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.DateTimeOffsetProperty).IsConcurrencyToken();
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task ModelBuilderTest()
        {
            string expectMetadata =
                "<EntitySet Name=\"ETagsCustomers\" EntityType=\"Microsoft.Test.E2E.AspNet.OData.ETags.ETagsCustomer\">\r\n" +
                "          <NavigationPropertyBinding Path=\"RelatedCustomer\" Target=\"ETagsCustomers\" />\r\n" +
                "          <Annotation Term=\"Org.OData.Core.V1.OptimisticConcurrency\">\r\n" +
                "            <Collection>\r\n" +
                "              <PropertyPath>Id</PropertyPath>\r\n" +
                "              <PropertyPath>Name</PropertyPath>\r\n" +
                "              <PropertyPath>BoolProperty</PropertyPath>\r\n" +
                "              <PropertyPath>ByteProperty</PropertyPath>\r\n" +
                "              <PropertyPath>CharProperty</PropertyPath>\r\n" +
                "              <PropertyPath>DecimalProperty</PropertyPath>\r\n" +
                "              <PropertyPath>DoubleProperty</PropertyPath>\r\n" +
                "              <PropertyPath>ShortProperty</PropertyPath>\r\n" +
                "              <PropertyPath>LongProperty</PropertyPath>\r\n" +
                "              <PropertyPath>SbyteProperty</PropertyPath>\r\n" +
                "              <PropertyPath>FloatProperty</PropertyPath>\r\n" +
                "              <PropertyPath>UshortProperty</PropertyPath>\r\n" +
                "              <PropertyPath>UintProperty</PropertyPath>\r\n" +
                "              <PropertyPath>UlongProperty</PropertyPath>\r\n" +
                "              <PropertyPath>GuidProperty</PropertyPath>\r\n" +
                "              <PropertyPath>DateTimeOffsetProperty</PropertyPath>\r\n" +
                "              <PropertyPath>StringWithConcurrencyCheckAttributeProperty</PropertyPath>\r\n" +
                "            </Collection>\r\n" +
                "          </Annotation>\r\n" +
                "        </EntitySet>";

            // Remove indentation
            expectMetadata = Regex.Replace(expectMetadata, @"\r\n\s*<", @"<");

            string requestUri = string.Format("{0}/odata/$metadata", this.BaseAddress);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectMetadata, content);

            var stream = await response.Content.ReadAsStreamAsync();
            IODataResponseMessage message = new ODataMessageWrapper(stream, response.Content.Headers);
            var reader = new ODataMessageReader(message);
            var edmModel = reader.ReadMetadataDocument();
            Assert.NotNull(edmModel);

            var etagCustomers = edmModel.FindDeclaredEntitySet("ETagsCustomers");
            Assert.NotNull(etagCustomers);

            var annotations = edmModel.FindDeclaredVocabularyAnnotations(etagCustomers);
            IEdmVocabularyAnnotation annotation = Assert.Single(annotations);
            Assert.NotNull(annotation);

            Assert.Same(CoreVocabularyModel.ConcurrencyTerm, annotation.Term);
            Assert.Same(etagCustomers, annotation.Target);

            IEdmVocabularyAnnotation valueAnnotation = annotation as IEdmVocabularyAnnotation;
            Assert.NotNull(valueAnnotation);
            Assert.NotNull(valueAnnotation.Value);

            IEdmCollectionExpression collection = valueAnnotation.Value as IEdmCollectionExpression;
            Assert.NotNull(collection);
            Assert.Equal(new[]
            {
                "Id", "Name", "BoolProperty", "ByteProperty", "CharProperty", "DecimalProperty",
                "DoubleProperty", "ShortProperty", "LongProperty", "SbyteProperty",
                "FloatProperty", "UshortProperty", "UintProperty", "UlongProperty",
                "GuidProperty", "DateTimeOffsetProperty",
                "StringWithConcurrencyCheckAttributeProperty"
            },
                collection.Elements.Select(e => ((IEdmPathExpression) e).PathSegments.Single()));
        }

        [Fact]
        public async Task JsonWithDifferentMetadataLevelsHaveSameETagsTest()
        {
            string requestUri = this.BaseAddress + "/odata/ETagsCustomers";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json");
            HttpResponseMessage response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);
            var jsonResult = await response.Content.ReadAsAsync<JObject>();
            var jsonETags = jsonResult.GetValue("value").Select(e => e["@odata.etag"].ToString());
            Assert.Equal(jsonETags.Count(), jsonETags.Distinct().Count());

            requestUri = this.BaseAddress + "/odata/ETagsCustomers";
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json;odata=nometadata");
            response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);
            var jsonWithNometadataResult = await response.Content.ReadAsAsync<JObject>();
            var jsonWithNometadataETags = jsonWithNometadataResult.GetValue("value").Select(e => e["@odata.etag"].ToString());
            Assert.Equal(jsonWithNometadataETags.Count(), jsonWithNometadataETags.Distinct().Count());
            Assert.Equal(jsonETags, jsonWithNometadataETags);

            requestUri = this.BaseAddress + "/odata/ETagsCustomers";
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json;odata=fullmetadata");
            response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);
            var jsonWithFullmetadataResult = await response.Content.ReadAsAsync<JObject>();
            var jsonWithFullmetadataETags = jsonWithFullmetadataResult.GetValue("value").Select(e => e["@odata.etag"].ToString());
            Assert.Equal(jsonWithFullmetadataETags.Count(), jsonWithFullmetadataETags.Distinct().Count());
            Assert.Equal(jsonETags, jsonWithFullmetadataETags);

            requestUri = this.BaseAddress + "/odata/ETagsCustomers";
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json;odata=minimalmetadata");
            response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);
            var jsonWithMinimalmetadataResult = await response.Content.ReadAsAsync<JObject>();
            var jsonWithMinimalmetadataETags = jsonWithMinimalmetadataResult.GetValue("value").Select(e => e["@odata.etag"].ToString());
            Assert.Equal(jsonWithMinimalmetadataETags.Count(), jsonWithMinimalmetadataETags.Distinct().Count());
            Assert.Equal(jsonETags, jsonWithMinimalmetadataETags);
        }
    }
}