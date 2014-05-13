// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Tracing;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter.Deserialization;
using System.Web.OData.Formatter.Serialization;
using System.Web.OData.Query;
using System.Web.OData.TestCommon;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;
using Newtonsoft.Json.Linq;

namespace System.Web.OData.Formatter
{
    public class ODataFormatterTests
    {
        private const string baseAddress = "http://localhost:8081/";

        [Theory]
        [InlineData("application/json;odata.metadata=none", "PersonEntryInJsonLightNoMetadata.json")]
        [InlineData("application/json;odata.metadata=minimal", "PersonEntryInJsonLightMinimalMetadata.json")]
        [InlineData("application/json;odata.metadata=full", "PersonEntryInJsonLightFullMetadata.json")]
        public void GetEntryInODataJsonLightFormat(string metadata, string expect)
        {
            // Arrange
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpServer host = new HttpServer(configuration))
            using (HttpClient client = new HttpClient(host))
            using (HttpRequestMessage request = CreateRequestWithDataServiceVersionHeaders("People(10)",
                MediaTypeWithQualityHeaderValue.Parse(metadata)))

            // Act
            using (HttpResponseMessage response = client.SendAsync(request).Result)
            {
                // Assert
                AssertODataVersion4JsonResponse(Resources.GetString(expect), response);
            }
        }

        [Theory]
        [InlineData("application/json;odata.metadata=none", "PresidentInJsonLightNoMetadata.json")]
        [InlineData("application/json;odata.metadata=minimal", "PresidentInJsonLightMinimalMetadata.json")]
        [InlineData("application/json;odata.metadata=full", "PresidentInJsonLightFullMetadata.json")]
        public void GetSingletonInODataJsonLightFormat(string metadata, string expect)
        {
            // Arrange
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpServer host = new HttpServer(configuration))
            using (HttpClient client = new HttpClient(host))
            using (HttpRequestMessage request = CreateRequestWithDataServiceVersionHeaders("President",
                MediaTypeWithQualityHeaderValue.Parse(metadata)))

            // Act
            using (HttpResponseMessage response = client.SendAsync(request).Result)
            {
                // Assert
                AssertODataVersion4JsonResponse(Resources.GetString(expect), response);
            }
        }

        [Fact]
        public void GetEntry_UsesRouteModel_ForMultipleModels()
        {
            // Model 1 only has Name, Model 2 only has Age
            ODataModelBuilder builder1 = new ODataModelBuilder();
            var personType1 = builder1.EntityType<FormatterPerson>();
            personType1.HasKey(p => p.PerId);
            personType1.Property(p => p.Name);
            builder1.EntitySet<FormatterPerson>("People").HasIdLink(p => new Uri("http://link/"), false);
            var model1 = builder1.GetEdmModel();

            ODataModelBuilder builder2 = new ODataModelBuilder();
            var personType2 = builder2.EntityType<FormatterPerson>();
            personType2.HasKey(p => p.PerId);
            personType2.Property(p => p.Age);
            builder2.EntitySet<FormatterPerson>("People").HasIdLink(p => new Uri("http://link/"), false);
            var model2 = builder2.GetEdmModel();

            var config = new[] { typeof(PeopleController) }.GetHttpConfiguration();
            config.MapODataServiceRoute("OData1", "v1", model1);
            config.MapODataServiceRoute("OData2", "v2", model2);

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
                MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full")))
            // Act
            using (HttpResponseMessage response = client.SendAsync(request).Result)
            {
                // Assert
                AssertODataVersion4JsonResponse(
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
                MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none")))
            // Act
            using (HttpResponseMessage response = client.SendAsync(request).Result)
            {
                // Assert
                AssertODataVersion4JsonResponse(Resources.MainEntryFeedInJsonNoMetadata, response);
            }
        }

        [Fact]
        public void SupportOnlyODataFormat()
        {
            // Arrange
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                foreach (ODataMediaTypeFormatter odataFormatter in
                    configuration.Formatters.OfType<ODataMediaTypeFormatter>())
                {
                    odataFormatter.SupportedMediaTypes.Remove(ODataMediaTypes.ApplicationJson);
                }

                using (HttpServer host = new HttpServer(configuration))
                using (HttpClient client = new HttpClient(host))
                {
                    using (HttpRequestMessage request = CreateRequestWithDataServiceVersionHeaders("People(10)",
                        ODataTestUtil.ApplicationJsonMediaTypeWithQuality))

                    // Act
                    using (HttpResponseMessage response = client.SendAsync(request).Result)
                    {
                        // Assert
                        Assert.NotNull(response);
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        Assert.Equal(ODataTestUtil.ApplicationJsonMediaTypeWithQuality.MediaType,
                            response.Content.Headers.ContentType.MediaType);
                        ODataTestUtil.VerifyResponse(response.Content, Resources.PersonEntryInPlainOldJson);
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
                    odataFormatter.MediaTypeMappings.Add(new ODataMediaTypeMapping(ODataTestUtil.ApplicationJsonMediaTypeWithQuality));
                }

                using (HttpServer host = new HttpServer(configuration))
                using (HttpClient client = new HttpClient(host))
                {
                    // Arrange #1: this request should return response in OData json format
                    using (HttpRequestMessage requestWithJsonHeader = ODataTestUtil.GenerateRequestMessage(
                        CreateAbsoluteUri("People(10)?$format=application/json")))
                    // Act #1
                    using (HttpResponseMessage response = client.SendAsync(requestWithJsonHeader).Result)
                    {
                        // Assert #1
                        AssertODataVersion4JsonResponse(Resources.PersonEntryInJsonLight, response);
                    }

                    // Arrange #2: when the query string is not present, request should be handled by the regular Json
                    // Formatter
                    using (HttpRequestMessage requestWithNonODataJsonHeader = ODataTestUtil.GenerateRequestMessage(
                        CreateAbsoluteUri("People(10)")))
                    // Act #2
                    using (HttpResponseMessage response = client.SendAsync(requestWithNonODataJsonHeader).Result)
                    {
                        // Assert #2
                        Assert.NotNull(response);
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        Assert.Equal(ODataTestUtil.ApplicationJsonMediaTypeWithQuality.MediaType,
                            response.Content.Headers.ContentType.MediaType);
                        Assert.Null(ODataTestUtil.GetDataServiceVersion(response.Content.Headers));

                        ODataTestUtil.VerifyResponse(response.Content, Resources.PersonEntryInPlainOldJson);
                    }

                    // Arrange #3: this request should return response in OData json format
                    using (HttpRequestMessage requestWithJsonHeader = ODataTestUtil.GenerateRequestMessage(
                        CreateAbsoluteUri("President?$format=application/json")))
                    // Act #3
                    using (HttpResponseMessage response = client.SendAsync(requestWithJsonHeader).Result)
                    {
                        // Assert #3
                        AssertODataVersion4JsonResponse(Resources.GetString("PresidentInJsonLightMinimalMetadata.json"),
                            response);
                    }
                }
            }
        }

        [Fact]
        public void GetFeedInODataJsonFormat_LimitsResults()
        {
            // Arrange
            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpServer host = new HttpServer(configuration))
            using (HttpClient client = new HttpClient(host))
            using (HttpRequestMessage request = CreateRequest("People?$orderby=Name&$count=true",
                    ODataTestUtil.ApplicationJsonMediaTypeWithQuality))
            // Act
            using (HttpResponseMessage response = client.SendAsync(request).Result)
            {
                // Assert
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                string result = response.Content.ReadAsStringAsync().Result;
                dynamic json = JToken.Parse(result);

                // Assert the PageSize correctly limits three results to two
                Assert.Equal(2, json["value"].Count);
                // Assert there is a next page link
                Assert.NotNull(json["@odata.nextLink"]);
                Assert.Equal("http://localhost:8081/People?$orderby=Name&$count=true&$skip=2", json["@odata.nextLink"].Value);
                // Assert the count is included with the number of entities (3)
                Assert.Equal(3, json["@odata.count"].Value);
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
                    MediaTypeWithQualityHeaderValue.Parse("application/json")))
                // Act
                using (HttpResponseMessage response = client.SendAsync(request).Result)
                {
                    // Assert
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

                    string result = response.Content.ReadAsStringAsync().Result;
                    dynamic json = JToken.Parse(result);

                    Assert.Equal("The query specified in the URI is not valid. " +
                        "Could not find a property named 'abc' on type 'System.Web.OData.Formatter.FormatterPerson'.",
                        json["error"]["message"].Value);

                    Assert.Equal("Could not find a property named 'abc' on type 'System.Web.OData.Formatter.FormatterPerson'.",
                        json["error"]["innererror"]["message"].Value);

                    Assert.Equal("Microsoft.OData.Core.ODataException",
                        json["error"]["innererror"]["type"].Value);
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
                using (HttpRequestMessage request = CreateRequest("People", MediaTypeWithQualityHeaderValue.Parse("application/json")))
                // Act
                using (HttpResponseMessage response = client.SendAsync(request).Result)
                {
                    // Assert
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    string payload = response.Content.ReadAsStringAsync().Result;

                    // Change DoesNotContain() as Contain() after fix https://aspnetwebstack.codeplex.com/workitem/1880
                    Assert.DoesNotContain("\"@Custom.Int32Annotation\":321", payload);
                    Assert.DoesNotContain("\"@Custom.StringAnnotation\":\"My amazing feed\"", payload);
                }
            }
        }

        [Fact]
        public void EnumTypeRoundTripTest()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<EnumCustomer>("EnumCustomers");
            IEdmModel model = builder.GetEdmModel();
            var controllers = new[] { typeof(EnumCustomersController) };

            using (HttpConfiguration configuration = controllers.GetHttpConfiguration())
            {
                configuration.MapODataServiceRoute("odata", routePrefix: null, model: model);
                using (HttpServer host = new HttpServer(configuration))
                using (HttpClient client = new HttpClient(host))
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/EnumCustomers"))
                {
                    request.Content = new StringContent(
                        string.Format(@"{{'@odata.type':'#System.Web.OData.Formatter.EnumCustomer',
                            'ID':0,'Color':'Green, Blue','Colors':['Red','Red, Blue']}}"));
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    request.Headers.Accept.ParseAdd("application/json");

                    // Act
                    using (HttpResponseMessage response = client.SendAsync(request).Result)
                    {
                        // Assert
                        response.EnsureSuccessStatusCode();
                        var customer = response.Content.ReadAsAsync<JObject>().Result;
                        Assert.Equal(0, customer["ID"]);
                        Assert.Equal(Color.Green | Color.Blue, Enum.Parse(typeof(Color), customer["Color"].ToString()));
                        var colors = customer["Colors"].Select(c => Enum.Parse(typeof(Color), c.ToString()));
                        Assert.Equal(2, colors.Count());
                        Assert.Contains(Color.Red, colors);
                        Assert.Contains(Color.Red | Color.Blue, colors);
                    }
                }
            }
        }

        [Fact]
        public void EnumSerializer_HasODataType_ForFullMetadata()
        {
            // Arrange & Act
            string acceptHeader = "application/json;odata.metadata=full";
            HttpResponseMessage response = GetEnumResponse(acceptHeader);

            // Assert
            response.EnsureSuccessStatusCode();
            JObject customer = response.Content.ReadAsAsync<JObject>().Result;
            Assert.Equal("#System.Web.OData.Builder.TestModels.Color",
                customer.GetValue("Color@odata.type"));
            Assert.Equal("#Collection(System.Web.OData.Builder.TestModels.Color)",
                customer.GetValue("Colors@odata.type"));
        }

        [Theory]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public void EnumSerializer_HasNoODataType_ForNonFullMetadata(string acceptHeader)
        {
            // Arrange & Act
            HttpResponseMessage response = GetEnumResponse(acceptHeader);

            // Assert
            response.EnsureSuccessStatusCode();
            JObject customer = response.Content.ReadAsAsync<JObject>().Result;
            Assert.False(customer.Values().Contains("Color@odata.type"));
            Assert.False(customer.Values().Contains("Colors@odata.type"));
        }

        private HttpResponseMessage GetEnumResponse(string acceptHeader)
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<EnumCustomer>("EnumCustomers");
            IEdmModel model = builder.GetEdmModel();

            HttpConfiguration configuration = new[] { typeof(EnumCustomersController) }.GetHttpConfiguration();
            configuration.MapODataServiceRoute("odata", routePrefix: null, model: model);
            HttpServer host = new HttpServer(configuration);
            HttpClient client = new HttpClient(host);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/EnumCustomers");
            request.Content = new StringContent(
                string.Format(@"{{'@odata.type':'#System.Web.OData.Formatter.EnumCustomer',
                            'ID':0,'Color':'Green, Blue','Colors':['Red','Red, Blue']}}"));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Headers.Accept.ParseAdd(acceptHeader);

            HttpResponseMessage response = client.SendAsync(request).Result;
            return response;
        }

        [Fact]
        public void EnumSerializer_HasMetadataType()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<EnumCustomer>("EnumCustomers");
            IEdmModel model = builder.GetEdmModel();
            var controllers = new[] { typeof(EnumCustomersController) };

            using (HttpConfiguration configuration = controllers.GetHttpConfiguration())
            {
                configuration.MapODataServiceRoute("odata", routePrefix: null, model: model);
                using (HttpServer host = new HttpServer(configuration))
                using (HttpClient client = new HttpClient(host))
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/EnumCustomers"))
                {
                    request.Content = new StringContent(
                        string.Format(@"{{'@odata.type':'#System.Web.OData.Formatter.EnumCustomer',
                            'ID':0,'Color':'Green, Blue','Colors':['Red','Red, Blue']}}"));
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    request.Headers.Accept.ParseAdd("application/json;odata.metadata=full");

                    // Act
                    using (HttpResponseMessage response = client.SendAsync(request).Result)
                    {
                        // Assert
                        response.EnsureSuccessStatusCode();
                        dynamic payload = JToken.Parse(response.Content.ReadAsStringAsync().Result);
                        Assert.Equal("#System.Web.OData.Formatter.EnumCustomer", payload["@odata.type"].Value);
                        Assert.Equal("#System.Web.OData.Builder.TestModels.Color", payload["Color@odata.type"].Value);
                        Assert.Equal("#Collection(System.Web.OData.Builder.TestModels.Color)", payload["Colors@odata.type"].Value);
                    }
                }
            }
        }

        [Fact]
        public void RequestProperty_HasCorrectContextUrl()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<EnumCustomer>("EnumCustomers");
            IEdmModel model = builder.GetEdmModel();
            var controllers = new[] { typeof(EnumCustomersController) };

            using (HttpConfiguration configuration = controllers.GetHttpConfiguration())
            {
                configuration.MapODataServiceRoute("odata", routePrefix: null, model: model);
                using (HttpServer host = new HttpServer(configuration))
                using (HttpClient client = new HttpClient(host))

                // Act
                using (HttpResponseMessage response = client.GetAsync("http://localhost/EnumCustomers(5)/Color").Result)
                {
                    // Assert
                    response.EnsureSuccessStatusCode();
                    JObject payload = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    Assert.Equal("http://localhost/$metadata#EnumCustomers(5)/Color", payload.GetValue("@odata.context"));
                }
            }
        }

        [Fact]
        public void ODataCollectionSerializer_SerializeIQueryableOfIEdmEntityObject()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<CollectionSerializerCustomer>("CollectionSerializerCustomers");
            IEdmModel model = builder.GetEdmModel();
            var controllers = new[] { typeof(CollectionSerializerCustomersController) };

            using (HttpConfiguration configuration = controllers.GetHttpConfiguration())
            {
                configuration.MapODataServiceRoute("odata", routePrefix: null, model: model);
                using (HttpServer host = new HttpServer(configuration))
                using (HttpClient client = new HttpClient(host))
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/CollectionSerializerCustomers?$select=ID"))
                {
                    // Act
                    using (HttpResponseMessage response = client.SendAsync(request).Result)
                    {
                        // Assert
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
        }

        public class EnumCustomer
        {
            public int ID { get; set; }
            public Color Color { get; set; }
            public List<Color> Colors { get; set; }
        }

        public class EnumCustomersController : ODataController
        {
            public IHttpActionResult Post(EnumCustomer customer)
            {
                return Ok(customer);
            }

            public IHttpActionResult GetColor(int key)
            {
                return Ok(Color.Green);
            }
        }

        public class CollectionSerializerCustomer
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public class CollectionSerializerCustomersController : ODataController
        {
            public IHttpActionResult Get(ODataQueryOptions<CollectionSerializerCustomer> options)
            {
                IQueryable<CollectionSerializerCustomer> customers = new[]
                {
                    new CollectionSerializerCustomer{ID = 1, Name = "Name 1"},
                    new CollectionSerializerCustomer{ID = 2, Name = "Name 2"},
                    new CollectionSerializerCustomer{ID = 3, Name = "Name 3"},
                }.AsQueryable();

                IQueryable<IEdmEntityObject> appliedCustomers = options.ApplyTo(customers) as IQueryable<IEdmEntityObject>;

                return Ok(appliedCustomers);
            }
        }

        private static void AddDataServiceVersionHeaders(HttpRequestMessage request)
        {
            request.Headers.Add("OData-Version", "4.0");
            request.Headers.Add("OData-MaxVersion", "4.0");
        }

        private static void AssertODataVersion4JsonResponse(string expectedContent, HttpResponseMessage actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
            Assert.Equal(ODataTestUtil.ApplicationJsonMediaTypeWithQuality.MediaType,
                actual.Content.Headers.ContentType.MediaType);
            Assert.Equal(ODataTestUtil.Version4NumberString,
                ODataTestUtil.GetDataServiceVersion(actual.Content.Headers));
            ODataTestUtil.VerifyResponse(actual.Content, expectedContent);
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
            HttpConfiguration configuration =
                new[]
                {
                    typeof(MainEntityController), typeof(PeopleController), typeof(EnumCustomersController),
                    typeof(CollectionSerializerCustomersController), typeof(PresidentController)
                }.GetHttpConfiguration();
            configuration.MapODataServiceRoute(model);
            configuration.Formatters.InsertRange(0, ODataMediaTypeFormatters.Create());
            return configuration;
        }

        private static IEdmModel CreateModelForFullMetadata(bool sameLinksForIdAndEdit, bool sameLinksForEditAndRead)
        {
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();

            EntitySetConfiguration<MainEntity> mainSet = builder.EntitySet<MainEntity>("MainEntity");

            Func<EntityInstanceContext<MainEntity>, Uri> idLinkFactory = (e) =>
                CreateAbsoluteUri("/MainEntity/id/" + e.GetPropertyValue("Id").ToString());
            mainSet.HasIdLink(idLinkFactory, followsConventions: true);

            if (!sameLinksForIdAndEdit)
            {
                Func<EntityInstanceContext<MainEntity>, Uri> editLinkFactory =
                    (e) => CreateAbsoluteUri("/MainEntity/edit/" + e.GetPropertyValue("Id").ToString());
                mainSet.HasEditLink(editLinkFactory, followsConventions: false);
            }

            if (!sameLinksForEditAndRead)
            {
                Func<EntityInstanceContext<MainEntity>, Uri> readLinkFactory =
                    (e) => CreateAbsoluteUri("/MainEntity/read/" + e.GetPropertyValue("Id").ToString());
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

                // Int32
                ODataPrimitiveValue intValue = new ODataPrimitiveValue(321);
                feed.InstanceAnnotations.Add(new ODataInstanceAnnotation("Custom.Int32Annotation", intValue));

                // String
                ODataPrimitiveValue stringValue = new ODataPrimitiveValue("My amazing feed");
                feed.InstanceAnnotations.Add(new ODataInstanceAnnotation("Custom.StringAnnotation", stringValue));
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
