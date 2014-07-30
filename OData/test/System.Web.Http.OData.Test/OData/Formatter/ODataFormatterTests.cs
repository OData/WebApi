// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Web.Http.OData.Formatter.Serialization;
using System.Web.Http.Tracing;
using System.Xml.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Atom;
using Microsoft.TestCommon;
using Moq;
using Newtonsoft.Json.Linq;

namespace System.Web.Http.OData.Formatter
{
    public class ODataFormatterTests
    {
        private const string baseAddress = "http://localhost:8081/";

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetEntryInODataAtomFormat(bool tracingEnabled)
        {
            // Arrange
            using (HttpConfiguration configuration = CreateConfiguration(tracingEnabled))
            using (HttpServer host = new HttpServer(configuration))
            using (HttpClient client = new HttpClient(host))
            using (HttpRequestMessage request = CreateRequestWithDataServiceVersionHeaders("People(10)",
                ODataTestUtil.ApplicationAtomMediaTypeWithQuality))
            // Act
            using (HttpResponseMessage response = client.SendAsync(request).Result)
            {
                // Assert
                AssertODataVersion3AtomResponse(Resources.PersonEntryInAtom, response);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PostEntryInODataAtomFormat(bool tracingEnabled)
        {
            // Arrange
            using (HttpConfiguration configuration = CreateConfiguration(tracingEnabled))
            using (HttpServer host = new HttpServer(configuration))
            using (HttpClient client = new HttpClient(host))
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, baseAddress + "People"))
            {
                request.Content = new StringContent(Resources.PersonEntryInAtom);
                request.Content.Headers.ContentType = ODataTestUtil.ApplicationAtomMediaTypeWithQuality;

                // Act
                using (HttpResponseMessage response = client.SendAsync(request).Result)
                {
                    // Assert
                    AssertODataVersion3AtomResponse(Resources.PersonEntryInAtom, response, HttpStatusCode.Created);
                }
            }
        }

        [Fact]
        public void GetEntryInODataJsonVerboseFormat()
        {
            // Arrange
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpServer host = new HttpServer(configuration))
            using (HttpClient client = new HttpClient(host))
            using (HttpRequestMessage request = CreateRequestWithDataServiceVersionHeaders("People(10)",
                MediaTypeWithQualityHeaderValue.Parse("application/json;odata=verbose")))
            // Act
            using (HttpResponseMessage response = client.SendAsync(request).Result)
            {
                // Assert
                AssertODataVersion3JsonResponse(Resources.PersonEntryInJsonVerbose, response);
            }
        }

        [Fact]
        public void GetEntryInODataJsonFullMetadataFormat()
        {
            // Arrange
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpServer host = new HttpServer(configuration))
            using (HttpClient client = new HttpClient(host))
            using (HttpRequestMessage request = CreateRequestWithDataServiceVersionHeaders("People(10)",
                MediaTypeWithQualityHeaderValue.Parse("application/json;odata=fullmetadata")))
            // Act
            using (HttpResponseMessage response = client.SendAsync(request).Result)
            {
                // Assert
                AssertODataVersion3JsonResponse(Resources.PersonEntryInJsonFullMetadata, response);
            }
        }

        [Fact]
        public void GetEntry_UsesRouteModel_ForMultipleModels()
        {
            // Model 1 only has Name, Model 2 only has Age
            ODataModelBuilder builder1 = new ODataModelBuilder();
            var personType1 = builder1.Entity<FormatterPerson>().Property(p => p.Name);
            builder1.EntitySet<FormatterPerson>("People").HasIdLink(p => "link", false);
            var model1 = builder1.GetEdmModel();

            ODataModelBuilder builder2 = new ODataModelBuilder();
            builder2.Entity<FormatterPerson>().Property(p => p.Age);
            builder2.EntitySet<FormatterPerson>("People").HasIdLink(p => "link", false);
            var model2 = builder2.GetEdmModel();

            var config = new HttpConfiguration();
            config.Routes.MapODataServiceRoute("OData1", "v1", model1);
            config.Routes.MapODataServiceRoute("OData2", "v2", model2);

            using (HttpServer host = new HttpServer(config))
            using (HttpClient client = new HttpClient(host))
            {
                using (HttpResponseMessage response = client.GetAsync("http://localhost/v1/People(10)").Result)
                {
                    Assert.True(response.IsSuccessStatusCode);
                    JToken json = JToken.Parse(response.Content.ReadAsStringAsync().Result);

                    // Model 1 has the Name property but not the Age property
                    Assert.NotNull(json["Name"]);
                    Assert.Null(json["Age"]);
                }

                using (HttpResponseMessage response = client.GetAsync("http://localhost/v2/People(10)").Result)
                {
                    Assert.True(response.IsSuccessStatusCode);
                    JToken json = JToken.Parse(response.Content.ReadAsStringAsync().Result);

                    // Model 2 has the Age property but not the Name property
                    Assert.Null(json["Name"]);
                    Assert.NotNull(json["Age"]);
                }
            }
        }

        [Fact]
        public void GetFeedInODataJsonFullMetadataFormat()
        {
            // Arrange
            IEdmModel model = CreateModelForFullMetadata(sameLinksForIdAndEdit: false, sameLinksForEditAndRead: false);

            using (HttpConfiguration configuration = CreateConfiguration(model))
            using (HttpServer host = new HttpServer(configuration))
            using (HttpClient client = new HttpClient(host))
            using (HttpRequestMessage request = CreateRequestWithDataServiceVersionHeaders("MainEntity",
                MediaTypeWithQualityHeaderValue.Parse("application/json;odata=fullmetadata")))
            // Act
            using (HttpResponseMessage response = client.SendAsync(request).Result)
            {
                // Assert
                AssertODataVersion3JsonResponse(
                    Resources.MainEntryFeedInJsonFullMetadata, response);
            }
        }

        [Fact]
        public void GetFeedInODataJsonNoMetadataFormat()
        {
            // Arrange
            IEdmModel model = CreateModelForFullMetadata(sameLinksForIdAndEdit: false, sameLinksForEditAndRead: false);

            using (HttpConfiguration configuration = CreateConfiguration(model))
            using (HttpServer host = new HttpServer(configuration))
            using (HttpClient client = new HttpClient(host))
            using (HttpRequestMessage request = CreateRequestWithDataServiceVersionHeaders("MainEntity",
                MediaTypeWithQualityHeaderValue.Parse("application/json;odata=nometadata")))
            // Act
            using (HttpResponseMessage response = client.SendAsync(request).Result)
            {
                // Assert
                AssertODataVersion3JsonResponse(Resources.MainEntryFeedInJsonNoMetadata, response);
            }
        }

        [Fact]
        public void SupportOnlyODataAtomFormat()
        {
            // Arrange #1 and #2
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                foreach (ODataMediaTypeFormatter odataFormatter in
                    configuration.Formatters.OfType<ODataMediaTypeFormatter>())
                {
                    odataFormatter.SupportedMediaTypes.Remove(ODataMediaTypes.ApplicationJsonODataVerbose);
                    odataFormatter.SupportedMediaTypes.Remove(ODataMediaTypes.ApplicationJson);
                }

                using (HttpServer host = new HttpServer(configuration))
                using (HttpClient client = new HttpClient(host))
                {
                    // Arrange #1
                    using (HttpRequestMessage request = CreateRequestWithDataServiceVersionHeaders("People(10)",
                        ODataTestUtil.ApplicationAtomMediaTypeWithQuality))
                    // Act #1
                    using (HttpResponseMessage response = client.SendAsync(request).Result)
                    {
                        // Assert #1
                        AssertODataVersion3AtomResponse(Resources.PersonEntryInAtom, response);
                    }

                    // Arrange #2
                    using (HttpRequestMessage request = CreateRequestWithDataServiceVersionHeaders("People(10)",
                        ODataTestUtil.ApplicationJsonMediaTypeWithQuality))
                    // Act #2
                    using (HttpResponseMessage response = client.SendAsync(request).Result)
                    {
                        // Assert #2
                        Assert.NotNull(response);
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        Assert.Equal(ODataTestUtil.ApplicationJsonMediaTypeWithQuality.MediaType,
                            response.Content.Headers.ContentType.MediaType);
                        ODataTestUtil.VerifyJsonResponse(response.Content, Resources.PersonEntryInPlainOldJson);
                    }
                }
            }
        }

        [Fact]
        public void ConditionallySupportODataIfQueryStringPresent()
        {
            // Arrange #1, #2 and #3
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                foreach (ODataMediaTypeFormatter odataFormatter in
                    configuration.Formatters.OfType<ODataMediaTypeFormatter>())
                {
                    odataFormatter.SupportedMediaTypes.Clear();
                    odataFormatter.MediaTypeMappings.Add(new ODataMediaTypeMapping(ODataTestUtil.ApplicationAtomMediaTypeWithQuality));
                    odataFormatter.MediaTypeMappings.Add(new ODataMediaTypeMapping(ODataTestUtil.ApplicationJsonMediaTypeWithQuality));
                }

                using (HttpServer host = new HttpServer(configuration))
                using (HttpClient client = new HttpClient(host))
                {
                    // Arrange #1 this request should return response in OData atom format
                    using (HttpRequestMessage request = ODataTestUtil.GenerateRequestMessage(
                        CreateAbsoluteUri("People(10)?format=odata"), isAtom: true))
                    // Act #1
                    using (HttpResponseMessage response = client.SendAsync(request).Result)
                    {
                        // Assert #1
                        AssertODataVersion3AtomResponse(Resources.PersonEntryInAtom, response);
                    }

                    // Arrange #2: this request should return response in OData json format
                    using (HttpRequestMessage requestWithJsonHeader = ODataTestUtil.GenerateRequestMessage(
                        CreateAbsoluteUri("People(10)?format=odata"), isAtom: false))
                    // Act #2
                    using (HttpResponseMessage response = client.SendAsync(requestWithJsonHeader).Result)
                    {
                        // Assert #2
                        AssertODataVersion3JsonResponse(Resources.PersonEntryInJsonVerbose, response);
                    }

                    // Arrange #3: when the query string is not present, request should be handled by the regular Json
                    // Formatter
                    using (HttpRequestMessage requestWithNonODataJsonHeader = ODataTestUtil.GenerateRequestMessage(
                        CreateAbsoluteUri("People(10)"), isAtom: false))
                    // Act #3
                    using (HttpResponseMessage response = client.SendAsync(requestWithNonODataJsonHeader).Result)
                    {
                        // Assert #3
                        Assert.NotNull(response);
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        Assert.Equal(ODataTestUtil.ApplicationJsonMediaTypeWithQuality.MediaType,
                            response.Content.Headers.ContentType.MediaType);
                        Assert.Null(ODataTestUtil.GetDataServiceVersion(response.Content.Headers));

                        ODataTestUtil.VerifyJsonResponse(response.Content, Resources.PersonEntryInPlainOldJson);
                    }
                }
            }
        }

        [Fact]
        public void GetFeedInODataAtomFormat_HasSelfLink()
        {
            // Arrange
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpServer host = new HttpServer(configuration))
            using (HttpClient client = new HttpClient(host))
            using (HttpRequestMessage request = CreateRequest("People",
                ODataTestUtil.ApplicationAtomMediaTypeWithQuality))
            // Act
            using (HttpResponseMessage response = client.SendAsync(request).Result)
            {
                // Assert
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                XElement xml = XElement.Load(response.Content.ReadAsStreamAsync().Result);
                XElement[] links = xml.Elements(XName.Get("link", "http://www.w3.org/2005/Atom")).ToArray();
                Assert.Equal("self", links.First().Attribute("rel").Value);
                Assert.Equal(baseAddress + "People", links.First().Attribute("href").Value);
            }
        }

        [Fact]
        public void GetFeedInODataAtomFormat_LimitsResults()
        {
            // Arrange
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpServer host = new HttpServer(configuration))
            using (HttpClient client = new HttpClient(host))
            using (HttpRequestMessage request = CreateRequest("People?$orderby=Name&$inlinecount=allpages",
                    ODataTestUtil.ApplicationAtomMediaTypeWithQuality))
            // Act
            using (HttpResponseMessage response = client.SendAsync(request).Result)
            {
                // Assert
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                XElement xml = XElement.Load(response.Content.ReadAsStreamAsync().Result);
                XElement[] entries = xml.Elements(XName.Get("entry", "http://www.w3.org/2005/Atom")).ToArray();
                XElement nextPageLink = xml.Elements(XName.Get("link", "http://www.w3.org/2005/Atom"))
                    .Where(link => link.Attribute(XName.Get("rel")).Value == "next")
                    .SingleOrDefault();
                XElement count = xml.Element(XName.Get("count", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"));

                // Assert the PageSize correctly limits three results to two
                Assert.Equal(2, entries.Length);
                // Assert there is a next page link
                Assert.NotNull(nextPageLink);
                // Assert the count is included with the number of entities (3)
                Assert.Equal("3", count.Value);
            }
        }

        [Fact]
        [ReplaceCulture]
        public void HttpErrorInODataFormat_GetsSerializedCorrectly()
        {
            // Arrange
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
                using (HttpServer host = new HttpServer(configuration))
                using (HttpClient client = new HttpClient(host))
                using (HttpRequestMessage request = CreateRequest("People?$filter=abc+eq+null",
                    MediaTypeWithQualityHeaderValue.Parse("application/xml")))
                // Act
                using (HttpResponseMessage response = client.SendAsync(request).Result)
                {
                    // Assert
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

                    XElement xml = XElement.Load(response.Content.ReadAsStreamAsync().Result);

                    Assert.Equal("error", xml.Name.LocalName);
                    Assert.Equal("The query specified in the URI is not valid. Could not find a property named 'abc' on type 'System.Web.Http.OData.Formatter.FormatterPerson'.",
                        xml.Element(XName.Get("{http://schemas.microsoft.com/ado/2007/08/dataservices/metadata}message")).Value);
                    XElement innerErrorXml = xml.Element(XName.Get("{http://schemas.microsoft.com/ado/2007/08/dataservices/metadata}innererror"));
                    Assert.NotNull(innerErrorXml);
                    Assert.Equal("Could not find a property named 'abc' on type 'System.Web.Http.OData.Formatter.FormatterPerson'.",
                        innerErrorXml.Element(XName.Get("{http://schemas.microsoft.com/ado/2007/08/dataservices/metadata}message")).Value);
                    Assert.Equal("Microsoft.Data.OData.ODataException",
                        innerErrorXml.Element(XName.Get("{http://schemas.microsoft.com/ado/2007/08/dataservices/metadata}type")).Value);
                }
            }
        }

        [Fact]
        public void CustomSerializerWorks()
        {
            // Arrange
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                configuration.Formatters.InsertRange(
                    0,
                    ODataMediaTypeFormatters.Create(new CustomSerializerProvider(), new DefaultODataDeserializerProvider()));
                using (HttpServer host = new HttpServer(configuration))
                using (HttpClient client = new HttpClient(host))
                using (HttpRequestMessage request = CreateRequest("People", MediaTypeWithQualityHeaderValue.Parse("application/atom+xml")))
                // Act
                using (HttpResponseMessage response = client.SendAsync(request).Result)
                {
                    // Assert
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    XElement xml = XElement.Load(response.Content.ReadAsStreamAsync().Result);

                    Assert.Equal("My amazing feed", xml.Elements().Single(e => e.Name.LocalName == "title").Value);
                }
            }
        }

        private static void AddDataServiceVersionHeaders(HttpRequestMessage request)
        {
            request.Headers.Add("DataServiceVersion", "2.0");
            request.Headers.Add("MaxDataServiceVersion", "3.0");
        }

        private static void AssertODataVersion3AtomResponse(string expectedContent, HttpResponseMessage actual)
        {
            AssertODataVersion3AtomResponse(expectedContent, actual, HttpStatusCode.OK);
        }

        private static void AssertODataVersion3AtomResponse(string expectedContent, HttpResponseMessage actual, HttpStatusCode statusCode)
        {
            Assert.NotNull(actual);
            Assert.Equal(statusCode, actual.StatusCode);
            Assert.Equal(ODataTestUtil.ApplicationAtomMediaTypeWithQuality.MediaType,
                actual.Content.Headers.ContentType.MediaType);
            Assert.Equal(ODataTestUtil.GetDataServiceVersion(actual.Content.Headers),
                ODataTestUtil.Version3NumberString);
            ODataTestUtil.VerifyResponse(actual.Content, expectedContent);
        }

        private static void AssertODataVersion3JsonResponse(string expectedContent, HttpResponseMessage actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
            Assert.Equal(ODataTestUtil.ApplicationJsonMediaTypeWithQuality.MediaType,
                actual.Content.Headers.ContentType.MediaType);
            Assert.Equal(ODataTestUtil.Version3NumberString,
                ODataTestUtil.GetDataServiceVersion(actual.Content.Headers));
            ODataTestUtil.VerifyJsonResponse(actual.Content, expectedContent);
        }

        private static string CreateAbsoluteLink(string relativeUri)
        {
            return CreateAbsoluteUri(relativeUri).AbsoluteUri;
        }

        private static Uri CreateAbsoluteUri(string relativeUri)
        {
            return new Uri(new Uri(baseAddress), relativeUri);
        }

        private static HttpConfiguration CreateConfiguration(bool tracingEnabled = false)
        {
            IEdmModel model = ODataTestUtil.GetEdmModel();
            HttpConfiguration configuration = CreateConfiguration(model);

            if (tracingEnabled)
            {
                configuration.Services.Replace(typeof(ITraceWriter), new Mock<ITraceWriter>().Object);
            }

            return configuration;
        }

        private static HttpConfiguration CreateConfiguration(IEdmModel model)
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Routes.MapODataServiceRoute(model);
            configuration.Formatters.InsertRange(0, ODataMediaTypeFormatters.Create());
            return configuration;
        }

        private static IEdmModel CreateModelForFullMetadata(bool sameLinksForIdAndEdit, bool sameLinksForEditAndRead)
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            EntitySetConfiguration<MainEntity> mainSet = builder.EntitySet<MainEntity>("MainEntity");

            Func<EntityInstanceContext<MainEntity>, string> idLinkFactory = (e) =>
                CreateAbsoluteLink("/MainEntity/id/" + e.GetPropertyValue("Id").ToString());
            mainSet.HasIdLink(idLinkFactory, followsConventions: true);

            Func<EntityInstanceContext<MainEntity>, string> editLinkFactory;

            if (!sameLinksForIdAndEdit)
            {
                editLinkFactory = (e) => CreateAbsoluteLink("/MainEntity/edit/" + e.GetPropertyValue("Id").ToString());
                mainSet.HasEditLink(editLinkFactory, followsConventions: false);
            }

            Func<EntityInstanceContext<MainEntity>, string> readLinkFactory;

            if (!sameLinksForEditAndRead)
            {
                readLinkFactory = (e) => CreateAbsoluteLink("/MainEntity/read/" + e.GetPropertyValue("Id").ToString());
                mainSet.HasReadLink(readLinkFactory, followsConventions: false);
            }

            EntityTypeConfiguration<MainEntity> main = mainSet.EntityType;

            main.HasKey<int>((e) => e.Id);
            main.Property<short>((e) => e.Int16);
            NavigationPropertyConfiguration mainToRelated = mainSet.EntityType.HasRequired((e) => e.Related);

            main.Action("DoAlways").ReturnsCollectionFromEntitySet<MainEntity>("MainEntity").HasActionLink((c) =>
                CreateAbsoluteUri("/MainEntity/DoAlways/" + c.GetPropertyValue("Id")),
                followsConventions: true);
            main.TransientAction("DoSometimes").ReturnsCollectionFromEntitySet<MainEntity>(
                "MainEntity").HasActionLink((c) =>
                    CreateAbsoluteUri("/MainEntity/DoSometimes/" + c.GetPropertyValue("Id")),
                    followsConventions: false);

            mainSet.HasNavigationPropertyLink(mainToRelated, (c, p) => new Uri("/MainEntity/RelatedEntity/" +
                c.GetPropertyValue("Id"), UriKind.Relative), followsConventions: true);

            EntitySetConfiguration<RelatedEntity> related = builder.EntitySet<RelatedEntity>("RelatedEntity");

            return builder.GetEdmModel();
        }

        private static HttpRequestMessage CreateRequest(string pathAndQuery, MediaTypeWithQualityHeaderValue accept)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, CreateAbsoluteUri(pathAndQuery));
            request.Headers.Accept.Add(accept);
            return request;
        }

        private static HttpRequestMessage CreateRequestWithDataServiceVersionHeaders(string pathAndQuery,
            MediaTypeWithQualityHeaderValue accept)
        {
            HttpRequestMessage request = CreateRequest(pathAndQuery, accept);
            AddDataServiceVersionHeaders(request);
            return request;
        }

        private class CustomFeedSerializer : ODataFeedSerializer
        {
            public CustomFeedSerializer(ODataSerializerProvider serializerProvider)
                : base(serializerProvider)
            {
            }

            public override ODataFeed CreateODataFeed(IEnumerable feedInstance, IEdmCollectionTypeReference feedType,
                ODataSerializerContext writeContext)
            {
                ODataFeed feed = base.CreateODataFeed(feedInstance, feedType, writeContext);
                feed.Atom().Title = new AtomTextConstruct { Kind = AtomTextConstructKind.Text, Text = "My amazing feed" };
                return feed;
            }
        }

        private class CustomSerializerProvider : DefaultODataSerializerProvider
        {
            public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
            {
                if (edmType.IsCollection() && edmType.AsCollection().ElementType().IsEntity())
                {
                    return new CustomFeedSerializer(this);
                }

                return base.GetEdmTypeSerializer(edmType);
            }
        }
    }

    public class MainEntity
    {
        public int Id { get; set; }

        public short Int16 { get; set; }

        public RelatedEntity Related { get; set; }
    }

    public class RelatedEntity
    {
        public int Id { get; set; }
    }

    public class MainEntityController : ODataController
    {
        public IEnumerable<MainEntity> Get()
        {
            MainEntity[] entities = new MainEntity[]
            {
                new MainEntity
                {
                    Id = 1,
                    Int16 = -1,
                    Related = new RelatedEntity
                    {
                        Id = 101
                    }
                },
                new MainEntity
                {
                    Id = 2,
                    Int16 = -2,
                    Related = new RelatedEntity
                    {
                        Id = 102
                    }
                }
            };

            return new PageResult<MainEntity>(entities, new Uri("aa:b"), 3);
        }
    }
}
