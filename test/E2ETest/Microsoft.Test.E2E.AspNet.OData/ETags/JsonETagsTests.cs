// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ETags
{
    public class JsonETagsTests : WebHostTestBase<JsonETagsTests>
    {
        public JsonETagsTests(WebHostTestFixture<JsonETagsTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.AddETagMessageHandler(new ETagMessageHandler());
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();
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

        [Theory]
        [InlineData("application/json")] // default metadata level
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        public async Task JsonWithDifferentMetadataLevelsHaveSameETagsTest(string metadataLevel)
        {
            string requestUri = this.BaseAddress + "/odata/ETagsCustomers";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd(metadataLevel);
            HttpResponseMessage response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);
            var jsonResult = await response.Content.ReadAsObject<JObject>();
            var jsonValue = jsonResult.GetValue("value") as JArray;
            Assert.Equal(10, jsonValue.Count);

            var expectedEtags = new Dictionary<string, string>
            {
                // { "0", ",MA==,Mi4w," }, // DeleteUpdatedEntryWithIfMatchETagsTests will change #"0" customer
                // { "1", ",MA==,NC4w," }, // PutUpdatedEntryWithIfMatchETagsTests will change #"1"customer
                // { "2", ",MA==,Ni4w," }, // PatchUpdatedEntryWithIfMatchETagsTest will change #"2" cusotmer
                { "3", ",MA==,OC4w," },
                { "4", ",MA==,MTAuMA==," },
                { "5", ",MA==,MTIuMA==," },
                { "6", ",MA==,MTQuMA==," },
                { "7", ",MA==,MTYuMA==," },
                { "8", ",MA==,MTguMA==," },
                { "9", ",MA==,MjAuMA==," },
            };

            var jsonETags = jsonValue.Select(e => e["@odata.etag"]);
            foreach (var etag in jsonValue)
            {
                string key = etag["Id"].ToString();
                if (expectedEtags.TryGetValue(key, out string etagValue))
                {
                    Assert.Contains(etagValue, etag["@odata.etag"].ToString());
                }
            }
        }

        [Fact]
        public async Task JsonWithNoneMetadataLevelsNotIncludeETags()
        {
            string requestUri = this.BaseAddress + "/odata/ETagsCustomers";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json;odata.metadata=none");
            HttpResponseMessage response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);
            var jsonResult = await response.Content.ReadAsObject<JObject>();
            var jsonValue = jsonResult.GetValue("value") as JArray;
            Assert.Equal(10, jsonValue.Count());

            foreach (var item in jsonValue)
            {
                JObject itemObject = item as JObject;
                Assert.NotNull(itemObject);
                Assert.DoesNotContain("@odata.etag", itemObject.Properties().Select(p => p.Name));
            }
        }
    }
}