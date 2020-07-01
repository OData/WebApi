// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;
#else
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test.Formatter
{
    public class ODataFormatterTests
    {
        private const string baseAddress = "http://localhost:8081/";
        private const string versionHeader = "OData-Version";

        [Theory]
        [InlineData("application/json;odata.metadata=none", "PersonEntryInJsonLightNoMetadata.json")]
        [InlineData("application/json;odata.metadata=minimal", "PersonEntryInJsonLightMinimalMetadata.json")]
        [InlineData("application/json;odata.metadata=full", "PersonEntryInJsonLightFullMetadata.json")]
        public async Task GetEntityResourceInODataJsonLightFormat(string metadata, string expect)
        {
            // Arrange
            using (HttpClient client = CreateClient())
            using (HttpRequestMessage request = CreateRequestWithDataServiceVersionHeaders("People(10)",
                MediaTypeWithQualityHeaderValue.Parse(metadata)))

            // Act
            using (HttpResponseMessage response = await client.SendAsync(request))
            {
                // Assert
                await AssertODataVersion4JsonResponse(Resources.GetString(expect), response);
            }
        }

        [Theory]
        [InlineData("application/json;odata.metadata=none", "PresidentInJsonLightNoMetadata.json")]
        [InlineData("application/json;odata.metadata=minimal", "PresidentInJsonLightMinimalMetadata.json")]
        [InlineData("application/json;odata.metadata=full", "PresidentInJsonLightFullMetadata.json")]
        public async Task GetSingletonInODataJsonLightFormat(string metadata, string expect)
        {
            // Arrange
            using (HttpClient client = CreateClient())
            using (HttpRequestMessage request = CreateRequestWithDataServiceVersionHeaders("President",
                MediaTypeWithQualityHeaderValue.Parse(metadata)))

            // Act
            using (HttpResponseMessage response = await client.SendAsync(request))
            {
                // Assert
                await AssertODataVersion4JsonResponse(Resources.GetString(expect), response);
            }
        }

        [Fact]
        public async Task GetEntry_UsesRouteModel_ForMultipleModels()
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

            var controllers = new[] { typeof(PeopleController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("OData1", "v1", model1);
                config.MapODataServiceRoute("OData2", "v2", model2);
            });

            using (HttpClient client = TestServerFactory.CreateClient(server))
            {
                using (HttpResponseMessage response = await client.GetAsync("http://localhost/v1/People(10)"))
                {
                    Assert.True(response.IsSuccessStatusCode);
                    JToken json = JToken.Parse(await response.Content.ReadAsStringAsync());

                    // Model 1 has the Name property but not the Age property
                    Assert.NotNull(json["Name"]);
                    Assert.Null(json["Age"]);
                }

                using (HttpResponseMessage response = await client.GetAsync("http://localhost/v2/People(10)"))
                {
                    Assert.True(response.IsSuccessStatusCode);
                    JToken json = JToken.Parse(await response.Content.ReadAsStringAsync());

                    // Model 2 has the Age property but not the Name property
                    Assert.Null(json["Name"]);
                    Assert.NotNull(json["Age"]);
                }
            }
        }

        [Fact]
        public async Task GetFeedInODataJsonFullMetadataFormat()
        {
            // Arrange
            IEdmModel model = CreateModelForFullMetadata(sameLinksForIdAndEdit: false, sameLinksForEditAndRead: false);

            using (HttpClient client = CreateClient(model))
            using (HttpRequestMessage request = CreateRequestWithDataServiceVersionHeaders("MainEntity",
                MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full")))
            // Act
            using (HttpResponseMessage response = await client.SendAsync(request))
            {
                // Assert
                await AssertODataVersion4JsonResponse(
                    Resources.MainEntryFeedInJsonFullMetadata, response);
            }
        }

        [Fact]
        public async Task GetFeedInODataJsonNoMetadataFormat()
        {
            // Arrange
            IEdmModel model = CreateModelForFullMetadata(sameLinksForIdAndEdit: false, sameLinksForEditAndRead: false);

            using (HttpClient client = CreateClient(model))
            using (HttpRequestMessage request = CreateRequestWithDataServiceVersionHeaders("MainEntity",
                MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none")))
            // Act
            using (HttpResponseMessage response = await client.SendAsync(request))
            {
                // Assert
                await AssertODataVersion4JsonResponse(Resources.MainEntryFeedInJsonNoMetadata, response);
            }
        }

        [Fact]
        public async Task SupportOnlyODataFormat()
        {
            // Arrange
            using (HttpClient client = CreateClient(null, (supportedMediaTypes, mediaTypeMappings) =>
            {
                supportedMediaTypes.Remove(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJson));
            }))

            using (HttpRequestMessage request = CreateRequestWithDataServiceVersionHeaders("People(10)",
                ODataTestUtil.ApplicationJsonMediaTypeWithQuality))

            // Act
            using (HttpResponseMessage response = await client.SendAsync(request))
            {
                // Assert
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(ODataTestUtil.ApplicationJsonMediaTypeWithQuality.MediaType,
                    response.Content.Headers.ContentType.MediaType);

#if NETCORE
                string expect = "{\"@odata.context\":\"http://localhost:8081/$metadata#People/$entity\",\"@odata.id\":\"http://localhost:8081/People(10)\",\"PerId\":10,\"Age\":10,\"MyGuid\":\"f99080c0-2f9e-472e-8c72-1a8ecd9f902d\",\"Name\":\"Asha\",\"FavoriteColor\":\"Red, Green\",\"Order\":{\"OrderAmount\":235342,\"OrderName\":\"FirstOrder\"}}";
                await ODataTestUtil.VerifyResponse(response.Content, expect);
#else
                await ODataTestUtil.VerifyResponse(response.Content, Resources.PersonEntryInPlainOldJson);
#endif
            }
        }

        [Fact]
        public async Task ConditionallySupportODataIfQueryStringPresent()
        {
            // Arrange #1, #2 and #3
            Action<IList<MediaTypeHeaderValue>, IList<MediaTypeMapping>> modifyMediaTypes = ((supportedMediaTypes, mediaTypeMappings) =>
            {
                supportedMediaTypes.Clear();
                mediaTypeMappings.Add(ODataTestUtil.ApplicationJsonMediaTypeWithQualityMapping);
            });

            using (HttpClient client = CreateClient(null, modifyMediaTypes))
            {
                // Arrange #1: this request should return response in OData json format
                using (HttpRequestMessage requestWithJsonHeader = ODataTestUtil.GenerateRequestMessage(
                    CreateAbsoluteUri("People(10)?$format=application/json")))
                // Act #1
                using (HttpResponseMessage response = await client.SendAsync(requestWithJsonHeader))
                {
                    // Assert #1
                    await AssertODataVersion4JsonResponse(Resources.PersonEntryInJsonLight, response);
                }

                // Arrange #2: when the query string is not present, request should be handled by the regular Json
                // Formatter
                using (HttpRequestMessage requestWithNonODataJsonHeader = ODataTestUtil.GenerateRequestMessage(
                    CreateAbsoluteUri("People(10)")))
                // Act #2
                using (HttpResponseMessage response = await client.SendAsync(requestWithNonODataJsonHeader))
                {
                    // Assert #2
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal(ODataTestUtil.ApplicationJsonMediaTypeWithQuality.MediaType,
                        response.Content.Headers.ContentType.MediaType);
                    Assert.Null(ODataTestUtil.GetDataServiceVersion(response.Content.Headers));

#if NETCORE
                    string expect = "{\"@odata.context\":\"http://localhost:8081/$metadata#People/$entity\",\"@odata.id\":\"http://localhost:8081/People(10)\",\"PerId\":10,\"Age\":10,\"MyGuid\":\"f99080c0-2f9e-472e-8c72-1a8ecd9f902d\",\"Name\":\"Asha\",\"FavoriteColor\":\"Red, Green\",\"Order\":{\"OrderAmount\":235342,\"OrderName\":\"FirstOrder\"}}";
                    await ODataTestUtil.VerifyResponse(response.Content, expect);
#else
                    await ODataTestUtil.VerifyResponse(response.Content, Resources.PersonEntryInPlainOldJson);
#endif
                }

                // Arrange #3: this request should return response in OData json format
                using (HttpRequestMessage requestWithJsonHeader = ODataTestUtil.GenerateRequestMessage(
                    CreateAbsoluteUri("President?$format=application/json")))
                // Act #3
                using (HttpResponseMessage response = await client.SendAsync(requestWithJsonHeader))
                {
                    // Assert #3
                    await AssertODataVersion4JsonResponse(Resources.GetString("PresidentInJsonLightMinimalMetadata.json"),
                        response);
                }
            }
        }

        [Fact]
        public async Task GetFeedInODataJsonFormat_LimitsResults()
        {
            // Arrange
            using (HttpClient client = CreateClient())
            using (HttpRequestMessage request = CreateRequest("People?$orderby=Name&$count=true",
                    ODataTestUtil.ApplicationJsonMediaTypeWithQuality))
            // Act
            using (HttpResponseMessage response = await client.SendAsync(request))
            {
                // Assert
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                string result = await response.Content.ReadAsStringAsync();
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
        public async Task HttpErrorInODataFormat_GetsSerializedCorrectly()
        {
            // Arrange
            //using (HttpConfiguration configuration = CreateConfiguration())
            //{
            //    configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            using (HttpClient client = CreateClient())
            using (HttpRequestMessage request = CreateRequest("People?$filter=abc+eq+null",
                    MediaTypeWithQualityHeaderValue.Parse("application/json")))
            // Act
            using (HttpResponseMessage response = await client.SendAsync(request))
            {
                // Assert
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

                string result = await response.Content.ReadAsStringAsync();
                dynamic json = JToken.Parse(result);

                Assert.Equal("The query specified in the URI is not valid. " +
                    "Could not find a property named 'abc' on type 'Microsoft.AspNet.OData.Test.Formatter.FormatterPerson'.",
                    json["error"]["message"].Value);

                Assert.Equal("Could not find a property named 'abc' on type 'Microsoft.AspNet.OData.Test.Formatter.FormatterPerson'.",
                    json["error"]["innererror"]["message"].Value);

                Assert.Equal("Microsoft.OData.ODataException",
                    json["error"]["innererror"]["type"].Value);
            }
            //}
        }

        [Theory]
        [InlineData("4.0", null, null, null)]
        [InlineData("4.0", "4.0", null, null)]
        [InlineData("4.0", null, "4.0", null)]
        [InlineData("4.0", null, null, "4.0")]
        [InlineData("4.0", "4.0", null, "4.01")]
        [InlineData("4.01", "4.01", null, null)]
        [InlineData("4.01", null, "4.01", null)]
        [InlineData("4.01", null, null, "4.01")]
        [InlineData("4.01", null, "4.01", "4.0")]
        [InlineData("4.01", "4.01", null, "4.0")]
        [InlineData("4.01", "4.01", "4.0", "4.0")]
        [InlineData("4.01", "4.01", "4.0", null)]
        public async Task ValidateResponseVersion(string expectedVersion, string maxVersionHeader, string minVersionHeader, string requestVersionHeader)
        {
            // Arrange
            using (HttpClient client = CreateClient())
            using (HttpRequestMessage request = CreateRequest("People",
                    ODataTestUtil.ApplicationJsonMediaTypeWithQuality, maxVersionHeader, minVersionHeader, requestVersionHeader))
            // Act
            using (HttpResponseMessage response = await client.SendAsync(request))
            {
                // Assert
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
#if NETCORE
                Assert.True(response.Headers.Contains(versionHeader));
                Assert.Equal(response.Headers.GetValues(versionHeader).FirstOrDefault(), expectedVersion);
#else
                Assert.True(response.Content.Headers.Contains(versionHeader));
                Assert.Equal(response.Content.Headers.GetValues(versionHeader).FirstOrDefault(), expectedVersion);
#endif
                string result = await response.Content.ReadAsStringAsync();
                dynamic json = JToken.Parse(result);

                string context = expectedVersion == "4.0" ? "@odata.context" : "@context";
                Assert.NotNull(json[context]);
            }
        }

        [Fact]
        public async Task CustomSerializerWorks()
        {
            var controllers = new[] { typeof(PeopleController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("IgnoredRouteName", null, builder =>
                    builder.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => ODataTestUtil.GetEdmModel())
                        .AddService<ODataSerializerProvider>(ServiceLifetime.Singleton, sp => new CustomSerializerProvider())
                        .AddService<IEnumerable<IODataRoutingConvention>>(Microsoft.OData.ServiceLifetime.Singleton, sp =>
                            ODataRoutingConventions.CreateDefaultWithAttributeRouting("IgnoredRouteName", config)));
            });

            using (HttpClient client = TestServerFactory.CreateClient(server))
            using (HttpRequestMessage request = CreateRequestWithAnnotationFilter("People", "odata.include-annotations=\"*\""))
            // Act
            using (HttpResponseMessage response = client.SendAsync(request).Result)
            {
                // Assert
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                string payload = await response.Content.ReadAsStringAsync();

                Assert.Contains("\"@Custom.Int32Annotation\":321", payload);
                Assert.Contains("\"@Custom.StringAnnotation\":\"My amazing feed\"", payload);
            }
        }

        [Theory]
        [InlineData("*", "PeopleWithAllAnnotations.json")]
        [InlineData("-*", "PeopleWithoutAnnotations.json")]
        [InlineData("Entry.*", "PeopleWithSpecialAnnotations.json")]
        [InlineData("Property.*,Hello.*", "PeopleWithMultipleAnnotations.json")]
        public async Task CustomSerializerWorks_ForInstanceAnnotationsFilter(string filter, string expect)
        {
            // Remove indentation in expect string
            expect = Regex.Replace(Resources.GetString(expect), @"\r\n\s*([""{}\]])", "$1");

            // Arrange
            var controllers = new[] { typeof(PeopleController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("IgnoredRouteName", null, builder =>
                    builder.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => ODataTestUtil.GetEdmModel())
                        .AddService<ODataSerializerProvider>(ServiceLifetime.Singleton, sp => new CustomSerializerProvider())
                        .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                            ODataRoutingConventions.CreateDefaultWithAttributeRouting("IgnoredRouteName", config)));
            });

            HttpClient client = TestServerFactory.CreateClient(server);

            HttpRequestMessage request = CreateRequestWithAnnotationFilter("People(2)",
                String.Format("odata.include-annotations=\"{0}\"", filter));

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expect, await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("EnumKeyCustomers")] // using CLR as parameter type
        [InlineData("EnumKeyCustomers2")] // using EdmEnumObject as parameter type
        public async Task EnumKeySimpleSerializerTest(string entitySet)
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<EnumCustomer>(entitySet);
            builder.EntityType<EnumCustomer>().HasKey(c => c.Color);
            IEdmModel model = builder.GetEdmModel();
            var controllers = new[] { typeof(EnumKeyCustomersController), typeof(EnumKeyCustomers2Controller) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", null, model);
            });

            HttpClient client = TestServerFactory.CreateClient(server);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                "http://localhost/" + entitySet + "(Microsoft.AspNet.OData.Test.Builder.TestModels.Color'Red')");
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var customer = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(9, customer["ID"]);
            Assert.Equal(Color.Red, Enum.Parse(typeof(Color), customer["Color"].ToString()));
            var colors = customer["Colors"].Select(c => Enum.Parse(typeof(Color), c.ToString()));
            Assert.Equal(2, colors.Count());
            Assert.Contains(Color.Blue, colors);
            Assert.Contains(Color.Red, colors);
        }

        [Fact]
        public async Task EnumTypeRoundTripTest()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<EnumCustomer>("EnumCustomers");
            IEdmModel model = builder.GetEdmModel();
            var controllers = new[] { typeof(EnumCustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", null, model);
            });

            using (HttpClient client = TestServerFactory.CreateClient(server))
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/EnumCustomers"))
            {
                request.Content = new StringContent(
                    string.Format(@"{{'@odata.type':'#Microsoft.AspNet.OData.Test.Formatter.EnumCustomer',
                            'ID':0,'Color':'Green, Blue','Colors':['Red','Red, Blue']}}"));
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request.Headers.Accept.ParseAdd("application/json");

                // Act
                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    // Assert
                    ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
                    var customer = await response.Content.ReadAsObject<JObject>();
                    Assert.Equal(0, customer["ID"]);
                    Assert.Equal(Color.Green | Color.Blue, Enum.Parse(typeof(Color), customer["Color"].ToString()));
                    var colors = customer["Colors"].Select(c => Enum.Parse(typeof(Color), c.ToString()));
                    Assert.Equal(2, colors.Count());
                    Assert.Contains(Color.Red, colors);
                    Assert.Contains(Color.Red | Color.Blue, colors);
                }
            }
        }

        [Fact]
        public async Task EnumSerializer_HasODataType_ForFullMetadata()
        {
            // Arrange & Act
            string acceptHeader = "application/json;odata.metadata=full";
            HttpResponseMessage response = await GetEnumResponse(acceptHeader);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            JObject customer = await response.Content.ReadAsObject<JObject>();
            Assert.Equal("#Microsoft.AspNet.OData.Test.Builder.TestModels.Color",
                customer.GetValue("Color@odata.type"));
            Assert.Equal("#Collection(Microsoft.AspNet.OData.Test.Builder.TestModels.Color)",
                customer.GetValue("Colors@odata.type"));
        }

        [Theory]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task EnumSerializer_HasNoODataType_ForNonFullMetadata(string acceptHeader)
        {
            // Arrange & Act
            HttpResponseMessage response = await GetEnumResponse(acceptHeader);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            JObject customer = await response.Content.ReadAsObject<JObject>();
            Assert.Contains("Color@odata.type", customer.Values());
            Assert.Contains("Colors@odata.type", customer.Values());
        }

        private async Task<HttpResponseMessage> GetEnumResponse(string acceptHeader)
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<EnumCustomer>("EnumCustomers");
            IEdmModel model = builder.GetEdmModel();

            var controllers = new[] { typeof(EnumCustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", null, model);
            });

            HttpClient client = TestServerFactory.CreateClient(server);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/EnumCustomers");
            request.Content = new StringContent(
                string.Format(@"{{'@odata.type':'#Microsoft.AspNet.OData.Test.Formatter.EnumCustomer',
                            'ID':0,'Color':'Green, Blue','Colors':['Red','Red, Blue']}}"));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Headers.Accept.ParseAdd(acceptHeader);

            HttpResponseMessage response = await client.SendAsync(request);
            return response;
        }

        [Fact]
        public async Task EnumSerializer_HasMetadataType()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<EnumCustomer>("EnumCustomers");
            IEdmModel model = builder.GetEdmModel();
            var controllers = new[] { typeof(EnumCustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", null, model);
            });

            using (HttpClient client = TestServerFactory.CreateClient(server))
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/EnumCustomers"))
            {
                request.Content = new StringContent(
                    string.Format(@"{{'@odata.type':'#Microsoft.AspNet.OData.Test.Formatter.EnumCustomer',
                            'ID':0,'Color':'Green, Blue','Colors':['Red','Red, Blue']}}"));
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request.Headers.Accept.ParseAdd("application/json;odata.metadata=full");

                // Act
                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    // Assert
                    ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
                    dynamic payload = JToken.Parse(await response.Content.ReadAsStringAsync());
                    Assert.Equal("#Microsoft.AspNet.OData.Test.Formatter.EnumCustomer", payload["@odata.type"].Value);
                    Assert.Equal("#Microsoft.AspNet.OData.Test.Builder.TestModels.Color", payload["Color@odata.type"].Value);
                    Assert.Equal("#Collection(Microsoft.AspNet.OData.Test.Builder.TestModels.Color)", payload["Colors@odata.type"].Value);
                }
            }
        }

        [Fact]
        public async Task RequestProperty_HasCorrectContextUrl()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<EnumCustomer>("EnumCustomers");
            IEdmModel model = builder.GetEdmModel();
            var controllers = new[] { typeof(EnumCustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", null, model);
            });

            using (HttpClient client = TestServerFactory.CreateClient(server))
            {
                // Act
                using (HttpResponseMessage response = await client.GetAsync("http://localhost/EnumCustomers(5)/Color"))
                {
                    // Assert
                    ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
                    JObject payload = JObject.Parse(await response.Content.ReadAsStringAsync());
                    Assert.Equal("http://localhost/$metadata#EnumCustomers(5)/Color", payload.GetValue("@odata.context"));
                }
            }
        }


        [Theory]
        [InlineData(typeof(Class.CollectionSerializerCustomersController))]
        [InlineData(typeof(Interface.CollectionSerializerCustomersController))]
        public async Task ODataCollectionSerializer_SerializeIQueryableOfIEdmEntityObject(Type controller)
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<CollectionSerializerCustomer>("CollectionSerializerCustomers");
            IEdmModel model = builder.GetEdmModel();
            var controllers = new[] { controller };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", null, model);
            });

            using (HttpClient client = TestServerFactory.CreateClient(server))
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/CollectionSerializerCustomers?$select=ID"))
            {
                // Act
                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    // Assert
                    ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
                }
            }
        }

        [Fact]
        public async Task RequestCollectionProperty_HasNextPageLine_Count()
        {
            // Arrange
            const string expect = @"{
  ""@odata.context"": ""http://localhost/$metadata#Collection(Microsoft.AspNet.OData.Test.Builder.TestModels.Color)"",
  ""@odata.count"": 3,
  ""@odata.nextLink"": ""http://localhost/EnumCustomers(5)/Colors?$count=true&$skip=2"",
  ""value"": [
    ""Blue"",
    ""Green""
  ]
}";
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<EnumCustomer>("EnumCustomers");
            IEdmModel model = builder.GetEdmModel();
            var controllers = new[] { typeof(EnumCustomersController) };
            var server = TestServerFactory.Create(controllers, (configuration) =>
            {
                configuration.Count().OrderBy().Filter().Expand().MaxTop(null);
                configuration.MapODataServiceRoute("odata", routePrefix: null, model: model);
            });

            using (HttpClient client = TestServerFactory.CreateClient(server))
            {
                // Act
                using (HttpResponseMessage response = await client.GetAsync("http://localhost/EnumCustomers(5)/Colors?$count=true"))
                {
                    // Assert
                    ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
                    JObject payload = JObject.Parse(await response.Content.ReadAsStringAsync());
                    Assert.Equal(expect, payload.ToString());
                }
            }
        }

        public class EnumCustomer
        {
            public int ID { get; set; }
            public Color Color { get; set; }
            public List<Color> Colors { get; set; }
        }

        public class EnumCustomersController : TestODataController
        {
            public ITestActionResult Post([FromBody]EnumCustomer customer)
            {
                return Ok(customer);
            }

            public ITestActionResult GetColor(int key)
            {
                return Ok(Color.Green);
            }

            [EnableQuery(PageSize = 2)]
            public ITestActionResult GetColors(int key)
            {
                IList<Color> colors = new[] { Color.Blue, Color.Green, Color.Red };
                return Ok(colors);
            }
        }

        public class EnumKeyCustomersController : TestODataController
        {
            public ITestActionResult Get([FromODataUri]Color key)
            {
                EnumCustomer customer = new EnumCustomer
                {
                    ID = 9,
                    Color = key,
                    Colors = new List<Color> { Color.Blue, Color.Red }
                };

                return Ok(customer);
            }
        }

        public class EnumKeyCustomers2Controller : TestODataController
        {
            public ITestActionResult Get([FromODataUri]EdmEnumObject key)
            {
                EnumCustomer customer = new EnumCustomer
                {
                    ID = 9,
                    Color = (Color)Enum.Parse(typeof(Color), key.Value),
                    Colors = new List<Color> { Color.Blue, Color.Red }
                };

                return Ok(customer);
            }
        }

        [Theory]
        [InlineData("KeyCustomers1")] // without [FromODataUriAttribute] in convention routing
        [InlineData("KeyCustomers2")] // with [FromODataUriAttribute] in convention routing
        [InlineData("KeyCustomers3")] // without [FromODataUriAttribute] in attribute routing
        [InlineData("KeyCustomers4")] // with [FromODataUriAttribute] int attribute routing
        public async Task SingleKeySimpleSerializerTest(string entitySet)
        {
            // Arrange
            IEdmModel model = GetKeyCustomerOrderModel();
            var controllers = new[] { typeof(KeyCustomers1Controller), typeof(KeyCustomers2Controller), typeof(KeyCustomerOrderController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", null, model);
            });

            HttpClient client = TestServerFactory.CreateClient(server);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                "http://localhost/" + entitySet + "(5)");
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var customer = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(5, customer["value"]);
        }

        [Theory]
        [InlineData("KeyOrders1")] // without [FromODataUriAttribute] in convention routing
        [InlineData("KeyOrders2")] // with [FromODataUriAttribute] in convention routing
        [InlineData("KeyOrders3")] // without [FromODataUriAttribute] in attribute routing
        [InlineData("KeyOrders4")] // with [FromODataUriAttribute] int attribute routing
        public async Task MultipleKeySimpleSerializerTest(string entitySet)
        {
            // Arrange
            IEdmModel model = GetKeyCustomerOrderModel();
            var controllers = new[] { typeof(KeyOrders1Controller), typeof(KeyOrders2Controller), typeof(KeyCustomerOrderController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", null, model);
            });

            HttpClient client = TestServerFactory.CreateClient(server);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                "http://localhost/" + entitySet + "(StringKey='my',DateKey=2016-05-11,GuidKey=46538EC2-E497-4DFE-A039-1C22F0999D6C)");
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var customer = await response.Content.ReadAsObject<JObject>();
            Assert.Equal("my", customer["value"]);
        }

        [Theory]
        [InlineData("KeyCustomers1")] // without [FromODataUriAttribute] in convention routing
        [InlineData("KeyCustomers2")] // with [FromODataUriAttribute] in convention routing
        [InlineData("KeyCustomers3")] // without [FromODataUriAttribute] in attribute routing
        [InlineData("KeyCustomers4")] // with [FromODataUriAttribute] int attribute routing
        public async Task RelatedKeySimpleSerializerTest(string entitySet)
        {
            // Arrange
            IEdmModel model = GetKeyCustomerOrderModel();
            var controllers = new[] { typeof(KeyCustomers1Controller), typeof(KeyCustomers2Controller), typeof(KeyCustomerOrderController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", null, model);
            });

            HttpClient client = TestServerFactory.CreateClient(server);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete,
                "http://localhost/" + entitySet + "(6)/Orders(StringKey='my',DateKey=2016-05-11,GuidKey=46538EC2-E497-4DFE-A039-1C22F0999D6C)/$ref");
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var customer = await response.Content.ReadAsObject<JObject>();
            Assert.Equal("6+my", customer["value"]);
        }

        private static IEdmModel GetKeyCustomerOrderModel()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();

            builder.EntityType<KeyCustomer>().HasKey(c => c.Id);
            builder.EntityType<KeyOrder>().HasKey(c => new { c.StringKey, c.DateKey, c.GuidKey });

            // without [FromODataUri]
            builder.EntitySet<KeyCustomer>("KeyCustomers1").HasManyBinding(c => c.Orders, "KeyOrders1");

            // with [FromODataUri]
            builder.EntitySet<KeyCustomer>("KeyCustomers2").HasManyBinding(c => c.Orders, "KeyOrders2");

            // Attribute routing  without [FromODataUri]
            builder.EntitySet<KeyCustomer>("KeyCustomers3").HasManyBinding(c => c.Orders, "KeyOrders3");

            // Attribute routing  with [FromODataUri]
            builder.EntitySet<KeyCustomer>("KeyCustomers4").HasManyBinding(c => c.Orders, "KeyOrders4");

            return builder.GetEdmModel();
        }

        public class KeyCustomer
        {
            public int Id { get; set; }

            public IList<KeyOrder> Orders { get; set; }
        }

        public class KeyOrder
        {
            public string StringKey { get; set; }

            public Date DateKey { get; set; }

            // public TimeOfDay TimeKey { get; set; }

            public Guid GuidKey { get; set; }
        }

        public class KeyCustomers1Controller : TestODataController
        {
            public ITestActionResult Get(int key)
            {
                return Ok(key);
            }

            public ITestActionResult DeleteRef(int key, string navigationProperty, string relatedKeyStringKey, Guid relatedKeyGuidKey,
                [FromODataUri]Date relatedKeyDateKey)
            {
                AssertMultipleKey(relatedKeyStringKey, relatedKeyDateKey, relatedKeyGuidKey);

                return Ok(key + "+" + relatedKeyStringKey);
            }
        }

        public class KeyCustomers2Controller : TestODataController
        {
            public ITestActionResult Get([FromODataUri]int key)
            {
                return Ok(key);
            }

            public ITestActionResult DeleteRef([FromODataUri]int key, [FromODataUri]string navigationProperty,
                [FromODataUri]string relatedKeyStringKey, [FromODataUri]Guid relatedKeyGuidKey, [FromODataUri]Date relatedKeyDateKey)
            {
                AssertMultipleKey(relatedKeyStringKey, relatedKeyDateKey, relatedKeyGuidKey);

                return Ok(key + "+" + relatedKeyStringKey);
            }
        }

        public class KeyOrders1Controller : TestODataController
        {
            // [FromODataUri] before Date type is necessary, otherwise it will use the content binding.
            public ITestActionResult Get(string keyStringKey, [FromODataUri]Date keyDateKey, Guid keyGuidKey)
            {
                AssertMultipleKey(keyStringKey, keyDateKey, keyGuidKey);

                return Ok(keyStringKey);
            }
        }

        public class KeyOrders2Controller : TestODataController
        {
            public ITestActionResult Get([FromODataUri]string keyStringKey, [FromODataUri]Date keyDateKey, [FromODataUri]Guid keyGuidKey)
            {
                AssertMultipleKey(keyStringKey, keyDateKey, keyGuidKey);

                return Ok(keyStringKey);
            }
        }

        public class KeyCustomerOrderController : TestODataController
        {
            [HttpGet]
            [ODataRoute("KeyCustomers3({customerKey})")]
            public ITestActionResult Customers3WithKey(int customerKey)
            {
                return Ok(customerKey);
            }

            [HttpGet]
            [ODataRoute("KeyCustomers4({customerKey})")]
            public ITestActionResult Customers4WithKey([FromODataUri]int customerKey)
            {
                return Ok(customerKey);
            }

            [HttpGet]
            [ODataRoute("KeyOrders3(StringKey={key1},DateKey={key2},GuidKey={key3})")]
            public ITestActionResult Orders3WithKey(string key1, [FromODataUri]Date key2, Guid key3)
            {
                AssertMultipleKey(key1, key2, key3);

                return Ok(key1);
            }

            [HttpGet]
            [ODataRoute("KeyOrders4(StringKey={key1},DateKey={key2},GuidKey={key3})")]
            public ITestActionResult Orders4WithKey([FromODataUri]string key1, [FromODataUri]Date key2, [FromODataUri]Guid key3)
            {
                AssertMultipleKey(key1, key2, key3);

                return Ok(key1);
            }

            [HttpDelete]
            [ODataRoute("KeyCustomers3({customerKey})/Orders(StringKey={key1},DateKey={key2},GuidKey={key3})/$ref")]
            public ITestActionResult DeleteOrderFromCustomer3(int customerKey, string key1, [FromODataUri]Date key2, Guid key3)
            {
                AssertMultipleKey(key1, key2, key3);

                return Ok(customerKey + "+" + key1);
            }

            [HttpDelete]
            [ODataRoute("KeyCustomers4({customerKey})/Orders(StringKey={key1},DateKey={key2},GuidKey={key3})/$ref")]
            public ITestActionResult DeleteOrderFromCustomer4([FromODataUri]int customerKey, [FromODataUri]string key1,
                [FromODataUri]Date key2, [FromODataUri]Guid key3)
            {
                AssertMultipleKey(key1, key2, key3);

                return Ok(customerKey + "+" + key1);
            }
        }

        private static void AssertMultipleKey(string key1, Date key2, Guid key3)
        {
            Assert.Equal("my", key1);
            Assert.Equal(new Date(2016, 5, 11), key2);
            Assert.Equal(new Guid("46538EC2-E497-4DFE-A039-1C22F0999D6C"), key3);
        }

        private static void AddDataServiceVersionHeaders(HttpRequestMessage request)
        {
            request.Headers.Add("OData-Version", "4.0");
            request.Headers.Add("OData-MaxVersion", "4.0");
        }

        private static Task AssertODataVersion4JsonResponse(string expectedContent, HttpResponseMessage actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
            Assert.Equal(ODataTestUtil.ApplicationJsonMediaTypeWithQuality.MediaType,
                actual.Content.Headers.ContentType.MediaType);
#if NETCORE
            Assert.Equal(ODataTestUtil.Version4NumberString, ODataTestUtil.GetDataServiceVersion(actual.Headers));
#else
            Assert.Equal(ODataTestUtil.Version4NumberString, ODataTestUtil.GetDataServiceVersion(actual.Content.Headers));
#endif
            return ODataTestUtil.VerifyResponse(actual.Content, expectedContent);
        }

        private static Uri CreateAbsoluteUri(string relativeUri)
        {
            return new Uri(new Uri(baseAddress), relativeUri);
        }

#if NETCORE
        private static HttpClient CreateClient(IEdmModel model = null,
            Action<IList<MediaTypeHeaderValue>, IList<MediaTypeMapping>> modifyMediaTypes = null)
        {
            var controllers = new[]
            {
                typeof(MainEntityController), typeof(PeopleController), typeof(EnumCustomersController),
                typeof(Class.CollectionSerializerCustomersController), typeof(PresidentController)
            };

            var server = TestServerFactory.CreateWithFormatters(controllers, null, (config) =>
            {
                config.Count().OrderBy().Filter().Expand().MaxTop(null).Select();
                config.MapODataServiceRoute("IgnoredRouteName", null, model != null ? model : ODataTestUtil.GetEdmModel());

                if (modifyMediaTypes != null)
                {
                    foreach (var odataFormatter in config.ServiceProvider.GetServices<ODataOutputFormatter>())
                    {
                        var supportedMediaTypes = new List<MediaTypeHeaderValue>(
                            odataFormatter.SupportedMediaTypes
                            .ToList()
                            .Select(v => MediaTypeHeaderValue.Parse(v)));

                        var mediaTypeMappings = new List<MediaTypeMapping>(odataFormatter.MediaTypeMappings);
                        modifyMediaTypes(supportedMediaTypes, mediaTypeMappings);

                        odataFormatter.SupportedMediaTypes.Clear();
                        foreach (var mediaType in supportedMediaTypes)
                        {
                            odataFormatter.SupportedMediaTypes.Add(mediaType.ToString());
                        }

                        odataFormatter.MediaTypeMappings.Clear();
                        foreach (var mediaTypeMapping in mediaTypeMappings)
                        {
                            odataFormatter.MediaTypeMappings.Add(mediaTypeMapping);
                        }
                    }
                }
            });

            return TestServerFactory.CreateClient(server);
        }
#else
        private static HttpClient CreateClient(IEdmModel model = null,
            Action<IList<MediaTypeHeaderValue>, IList<MediaTypeMapping>> modifyMediaTypes = null)
        {
            IEdmModel useModel = model != null ? model : ODataTestUtil.GetEdmModel();

            var controllers = new[]
            {
                typeof(MainEntityController), typeof(PeopleController), typeof(EnumCustomersController),
                typeof(Class.CollectionSerializerCustomersController), typeof(PresidentController)
            };

            var server = TestServerFactory.CreateWithFormatters(controllers, null, (config) =>
            {
                config.Count().OrderBy().Filter().Expand().MaxTop(null).Select();
                config.MapODataServiceRoute("IgnoredRouteName", null, useModel);
            });


            if (modifyMediaTypes != null)
            {
                foreach (var odataFormatter in server.Configuration.Formatters.OfType<ODataMediaTypeFormatter>())
                {
                    var supportedMediaTypes = new List<MediaTypeHeaderValue>(odataFormatter.SupportedMediaTypes);
                    var mediaTypeMappings = new List<MediaTypeMapping>(odataFormatter.MediaTypeMappings);
                    modifyMediaTypes(supportedMediaTypes, mediaTypeMappings);

                    odataFormatter.SupportedMediaTypes.Clear();
                    foreach (var mediaType in supportedMediaTypes)
                    {
                        odataFormatter.SupportedMediaTypes.Add(mediaType);
                    }

                    odataFormatter.MediaTypeMappings.Clear();
                    foreach (var mediaTypeMapping in mediaTypeMappings)
                    {
                        odataFormatter.MediaTypeMappings.Add(mediaTypeMapping);
                    }
                }
            }

            return TestServerFactory.CreateClient(server);
        }
#endif

        private static IEdmModel CreateModelForFullMetadata(bool sameLinksForIdAndEdit, bool sameLinksForEditAndRead)
        {
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();

            EntitySetConfiguration<MainEntity> mainSet = builder.EntitySet<MainEntity>("MainEntity");

            Func<ResourceContext<MainEntity>, Uri> idLinkFactory = (e) =>
                CreateAbsoluteUri("/MainEntity/id/" + e.GetPropertyValue("Id").ToString());
            mainSet.HasIdLink(idLinkFactory, followsConventions: true);

            if (!sameLinksForIdAndEdit)
            {
                Func<ResourceContext<MainEntity>, Uri> editLinkFactory =
                    (e) => CreateAbsoluteUri("/MainEntity/edit/" + e.GetPropertyValue("Id").ToString());
                mainSet.HasEditLink(editLinkFactory, followsConventions: false);
            }

            if (!sameLinksForEditAndRead)
            {
                Func<ResourceContext<MainEntity>, Uri> readLinkFactory =
                    (e) => CreateAbsoluteUri("/MainEntity/read/" + e.GetPropertyValue("Id").ToString());
                mainSet.HasReadLink(readLinkFactory, followsConventions: false);
            }

            EntityTypeConfiguration<MainEntity> main = mainSet.EntityType;

            main.HasKey<int>((e) => e.Id);
            main.Property<short>((e) => e.Int16);
            NavigationPropertyConfiguration mainToRelated = mainSet.EntityType.HasRequired((e) => e.Related);

            main.Action("DoAlways").ReturnsCollectionFromEntitySet<MainEntity>("MainEntity").HasActionLink((c) =>
                CreateAbsoluteUri("/MainEntity/DoAlways/" + c.GetPropertyValue("Id")),
                followsConventions: false);
            main.Action("DoSometimes").ReturnsCollectionFromEntitySet<MainEntity>(
                "MainEntity").HasActionLink((c) =>
                    CreateAbsoluteUri("/MainEntity/DoSometimes/" + c.GetPropertyValue("Id")),
                    followsConventions: false);

            main.Function("IsAlways").ReturnsCollectionFromEntitySet<MainEntity>("MainEntity").HasFunctionLink(c =>
                CreateAbsoluteUri(String.Format(
                    "/MainEntity({0})/Default.IsAlways()", c.GetPropertyValue("Id"))),
                followsConventions: false);

            // action and function bound to collection
            main.Collection.Action("DoAllAction")
                .HasFeedActionLink(c => CreateAbsoluteUri("/MainEntity/Default.DoAllAction"), followsConventions: false);

            main.Collection.Function("DoAllFunction").Returns<int>()
                .HasFeedFunctionLink(
                    c => CreateAbsoluteUri("/MainEntity/Default.DoAllFunction()"),
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

        private static HttpRequestMessage CreateRequest(string pathAndQuery, MediaTypeWithQualityHeaderValue accept, string maxVersion, string minVersion, string requestVersion)
        {
            HttpRequestMessage request = CreateRequest(pathAndQuery, accept);
            
            if (!string.IsNullOrEmpty(maxVersion))
            {
                request.Headers.Add("OData-MaxVersion", maxVersion);
            }

            if (!string.IsNullOrEmpty(minVersion))
            {
                request.Headers.Add("OData-MinVersion", minVersion);
            }

            if (!string.IsNullOrEmpty(requestVersion))
            {
                request.Headers.Add("OData-Version", requestVersion);
            }

            return request;
        }

        private static HttpRequestMessage CreateRequestWithDataServiceVersionHeaders(string pathAndQuery,
            MediaTypeWithQualityHeaderValue accept)
        {
            HttpRequestMessage request = CreateRequest(pathAndQuery, accept);
            AddDataServiceVersionHeaders(request);
            return request;
        }

        private static HttpRequestMessage CreateRequestWithAnnotationFilter(string pathAndQuery, string annotationHeader)
        {
            HttpRequestMessage request = CreateRequest(pathAndQuery, MediaTypeWithQualityHeaderValue.Parse("application/json"));
            request.Headers.Add("Prefer", annotationHeader);
            return request;
        }

        private class CustomFeedSerializer : ODataResourceSetSerializer
        {
            public CustomFeedSerializer(ODataSerializerProvider serializerProvider)
                : base(serializerProvider)
            {
            }

            public override ODataResourceSet CreateResourceSet(IEnumerable feedInstance, IEdmCollectionTypeReference feedType,
                ODataSerializerContext writeContext)
            {
                ODataResourceSet feed = base.CreateResourceSet(feedInstance, feedType, writeContext);

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
            public CustomSerializerProvider()
                : base(new MockContainer())
            {
            }

            public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
            {
                if (edmType.IsCollection() && edmType.AsCollection().ElementType().IsEntity())
                {
                    return new CustomFeedSerializer(this);
                }
                else if (edmType.IsEntity())
                {
                    return new CustomEntrySerializer(this);
                }

                return base.GetEdmTypeSerializer(edmType);
            }
        }

        private class CustomEntrySerializer : ODataResourceSerializer
        {
            public CustomEntrySerializer(ODataSerializerProvider serializerProvider)
                : base(serializerProvider)
            {
            }

            public override ODataResource CreateResource(SelectExpandNode selectExpandNode, ResourceContext entityContext)
            {
                ODataResource entry = base.CreateResource(selectExpandNode, entityContext);

                // instance annotation on entry
                ODataPrimitiveValue guidValue = new ODataPrimitiveValue(new Guid("A6E07EAC-AD49-4BF7-A06E-203FF4D4B0D8"));
                entry.InstanceAnnotations.Add(new ODataInstanceAnnotation("Entry.GuidAnnotation", guidValue));

                ODataPrimitiveValue strValue = new ODataPrimitiveValue("Hello World.");
                entry.InstanceAnnotations.Add(new ODataInstanceAnnotation("Hello.World", strValue));
                return entry;
            }

            public override ODataProperty CreateStructuralProperty(IEdmStructuralProperty structuralProperty, ResourceContext entityContext)
            {
                ODataProperty property = base.CreateStructuralProperty(structuralProperty, entityContext);

                // instance annotation on property
                if (property.Name == "Age")
                {
                    ODataPrimitiveValue dateValue = new ODataPrimitiveValue(new Date(2010, 1, 2));
                    property.InstanceAnnotations.Add(new ODataInstanceAnnotation("Property.BirthdayAnnotation",
                        dateValue));
                }

                return property;
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
}
