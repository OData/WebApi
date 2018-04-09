// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.V1;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.ModelBuilder;
using Xunit;

namespace WebStack.QA.Test.OData.ETags
{
    [NuwaFramework]
    public class JsonETagsTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
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
        public void ModelBuilderTest()
        {
            string expectMetadata =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<edmx:Edmx Version=\"4.0\" xmlns:edmx=\"http://docs.oasis-open.org/odata/ns/edmx\">\r\n" +
                "  <edmx:DataServices>\r\n" +
                "    <Schema Namespace=\"WebStack.QA.Test.OData.ETags\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">\r\n" +
                "      <EntityType Name=\"ETagsCustomer\">\r\n" +
                "        <Key>\r\n" +
                "          <PropertyRef Name=\"Id\" />\r\n" +
                "        </Key>\r\n" +
                "        <Property Name=\"Id\" Type=\"Edm.Int32\" Nullable=\"false\" />\r\n" +
                "        <Property Name=\"Name\" Type=\"Edm.String\" />\r\n" +
                "        <Property Name=\"BoolProperty\" Type=\"Edm.Boolean\" Nullable=\"false\" />\r\n" +
                "        <Property Name=\"ByteProperty\" Type=\"Edm.Byte\" Nullable=\"false\" />\r\n" +
                "        <Property Name=\"CharProperty\" Type=\"Edm.String\" Nullable=\"false\" />\r\n" +
                "        <Property Name=\"DecimalProperty\" Type=\"Edm.Decimal\" Nullable=\"false\" />\r\n" +
                "        <Property Name=\"DoubleProperty\" Type=\"Edm.Double\" Nullable=\"false\" />\r\n" +
                "        <Property Name=\"ShortProperty\" Type=\"Edm.Int16\" Nullable=\"false\" />\r\n" +
                "        <Property Name=\"LongProperty\" Type=\"Edm.Int64\" Nullable=\"false\" />\r\n" +
                "        <Property Name=\"SbyteProperty\" Type=\"Edm.SByte\" Nullable=\"false\" />\r\n" +
                "        <Property Name=\"FloatProperty\" Type=\"Edm.Single\" Nullable=\"false\" />\r\n" +
                "        <Property Name=\"UshortProperty\" Type=\"Edm.Int32\" Nullable=\"false\" />\r\n" +
                "        <Property Name=\"UintProperty\" Type=\"Edm.Int64\" Nullable=\"false\" />\r\n" +
                "        <Property Name=\"UlongProperty\" Type=\"Edm.Int64\" Nullable=\"false\" />\r\n" +
                "        <Property Name=\"GuidProperty\" Type=\"Edm.Guid\" Nullable=\"false\" />\r\n" +
                "        <Property Name=\"DateTimeOffsetProperty\" Type=\"Edm.DateTimeOffset\" Nullable=\"false\" />\r\n" +
                "        <Property Name=\"Notes\" Type=\"Collection(Edm.String)\" />\r\n" +
                "        <Property Name=\"StringWithConcurrencyCheckAttributeProperty\" Type=\"Edm.String\" />\r\n" +
                "        <NavigationProperty Name=\"RelatedCustomer\" Type=\"WebStack.QA.Test.OData.ETags.ETagsCustomer\" />\r\n" +
                "        <NavigationProperty Name=\"ContainedCustomer\" Type=\"WebStack.QA.Test.OData.ETags.ETagsCustomer\" ContainsTarget=\"true\" />\r\n" +
                "      </EntityType>\r\n" +
                "      <EntityType Name=\"ETagsDerivedCustomer\" BaseType=\"WebStack.QA.Test.OData.ETags.ETagsCustomer\">\r\n" +
                "        <Property Name=\"Role\" Type=\"Edm.String\" />\r\n" +
                "      </EntityType>\r\n" +
                "    </Schema>\r\n" +
                "    <Schema Namespace=\"Default\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">\r\n" +
                "      <EntityContainer Name=\"Container\">\r\n" +
                "        <EntitySet Name=\"ETagsCustomers\" EntityType=\"WebStack.QA.Test.OData.ETags.ETagsCustomer\">\r\n" +
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
                "        </EntitySet>\r\n" +
                "      </EntityContainer>\r\n" +
                "    </Schema>\r\n" +
                "  </edmx:DataServices>\r\n" +
                "</edmx:Edmx>";

            // Remove indentation
            expectMetadata = Regex.Replace(expectMetadata, @"\r\n\s*<", @"<");

            string requestUri = string.Format("{0}/odata/$metadata", this.BaseAddress);

            HttpResponseMessage response = this.Client.GetAsync(requestUri).Result;

            var content = response.Content.ReadAsStringAsync().Result;
            Assert.Contains(expectMetadata, content);

            var stream = response.Content.ReadAsStreamAsync().Result;
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
        public void JsonWithDifferentMetadataLevelsHaveSameETagsTest()
        {
            string requestUri = this.BaseAddress + "/odata/ETagsCustomers";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json");
            HttpResponseMessage response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);
            var jsonResult = response.Content.ReadAsAsync<JObject>().Result;
            var jsonETags = jsonResult.GetValue("value").Select(e => e["@odata.etag"].ToString());
            Assert.Equal(jsonETags.Count(), jsonETags.Distinct().Count());

            requestUri = this.BaseAddress + "/odata/ETagsCustomers";
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json;odata=nometadata");
            response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);
            var jsonWithNometadataResult = response.Content.ReadAsAsync<JObject>().Result;
            var jsonWithNometadataETags = jsonWithNometadataResult.GetValue("value").Select(e => e["@odata.etag"].ToString());
            Assert.Equal(jsonWithNometadataETags.Count(), jsonWithNometadataETags.Distinct().Count());
            Assert.Equal(jsonETags, jsonWithNometadataETags);

            requestUri = this.BaseAddress + "/odata/ETagsCustomers";
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json;odata=fullmetadata");
            response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);
            var jsonWithFullmetadataResult = response.Content.ReadAsAsync<JObject>().Result;
            var jsonWithFullmetadataETags = jsonWithFullmetadataResult.GetValue("value").Select(e => e["@odata.etag"].ToString());
            Assert.Equal(jsonWithFullmetadataETags.Count(), jsonWithFullmetadataETags.Distinct().Count());
            Assert.Equal(jsonETags, jsonWithFullmetadataETags);

            requestUri = this.BaseAddress + "/odata/ETagsCustomers";
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json;odata=minimalmetadata");
            response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);
            var jsonWithMinimalmetadataResult = response.Content.ReadAsAsync<JObject>().Result;
            var jsonWithMinimalmetadataETags = jsonWithMinimalmetadataResult.GetValue("value").Select(e => e["@odata.etag"].ToString());
            Assert.Equal(jsonWithMinimalmetadataETags.Count(), jsonWithMinimalmetadataETags.Distinct().Count());
            Assert.Equal(jsonETags, jsonWithMinimalmetadataETags);
        }
    }
}