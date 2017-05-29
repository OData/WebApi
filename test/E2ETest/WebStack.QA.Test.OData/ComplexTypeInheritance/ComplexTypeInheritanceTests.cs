using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData;
using System.Web.OData.Batch;
using System.Web.OData.Extensions;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.ComplexTypeInheritance.Proxy;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.ComplexTypeInheritance
{
    [NuwaFramework]
    [NuwaTrace(NuwaTraceAttribute.Tag.Off)]
    public class ComplexTypeInheritanceTests
    {
        public static TheoryDataSet<string, string> MediaTypes
        {
            get
            {
                string[] modes = new string[] { "convention", "explicit" };
                string[] mimes = new string[]{
                    "json",
                    "application/json",
                    "application/json;odata.metadata=none",
                    "application/json;odata.metadata=minimal",
                    "application/json;odata.metadata=full"};
                TheoryDataSet<string, string> data = new TheoryDataSet<string, string>();
                foreach (string mode in modes)
                {
                    foreach (string mime in mimes)
                    {
                        data.Add(mode, mime);
                    }
                }
                return data;
            }
        }

        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] { typeof(WindowsController), typeof(MetadataController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            configuration.Routes.Clear();
            HttpServer httpServer = configuration.GetHttpServer();
            configuration
                .MapODataServiceRoute(routeName: "convention",
                    routePrefix: "convention",
                    model: ComplexTypeInheritanceEdmModels.GetConventionModel());

            configuration
                .MapODataServiceRoute(routeName: "explicit",
                    routePrefix: "explicit",
                    model: ComplexTypeInheritanceEdmModels.GetExplicitModel(),
                    batchHandler: new DefaultODataBatchHandler(httpServer));
            configuration.EnsureInitialized();
        }


        #region CRUD on the entity
        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // POST ~/Windows
        public async Task CreateWindow(string mode)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = serviceRootUri + "/Windows";
            string content = @"
{
    'Id':0,
    'Name':'Name4',
    'CurrentShape':
    {
        '@odata.type':'#WebStack.QA.Test.OData.ComplexTypeInheritance.Circle',  
        'Radius':10,
        'Center':{'X':1,'Y':2},
        'HasBorder':true
    },
    'OptionalShapes':
    [
        {
            '@odata.type':'#WebStack.QA.Test.OData.ComplexTypeInheritance.Rectangle',
            'HasBorder':true,
            'Width':3,
            'Height':4,
            'TopLeft':{ 'X':1,'Y':2}
        }
    ]
}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var response = await Client.SendAsync(request);

            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.Created == response.StatusCode, String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                HttpStatusCode.Created,
                response.StatusCode,
                requestUri,
                contentOfString));
            JObject contentOfJObject = await response.Content.ReadAsAsync<JObject>();
            string name = (string)contentOfJObject["Name"];
            Assert.True("Name4" == name);
            int radius = (int)contentOfJObject["CurrentShape"]["Radius"];
            Assert.True(10 == radius,
                String.Format("\nExpected that Radius: 10, but actually: {0},\n request uri: {1},\n response payload: {2}", radius, requestUri, contentOfString));

            JArray optionalShapes = contentOfJObject["OptionalShapes"] as JArray;
            Assert.True(1 == optionalShapes.Count,
                String.Format("\nExpected count: {0},\n actual: {1},\n request uri: {2},\n response payload: {3}", 1, optionalShapes.Count, requestUri, contentOfString));
            JArray vertexes = optionalShapes[0]["Vertexes"] as JArray;
            Assert.True(4 == vertexes.Count, "The returned OptionalShapes is not as expected");
        }

        [Theory(Skip = "[Client] Client cant deserialize a property which is declared as abstract, but the payload is concrete.")]
        [PropertyData("MediaTypes")]
        // GET ~/Windows(1)
        public async Task QuerySingleContainingEntity(string mode, string mime)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            Proxy.Container container = new Proxy.Container(new Uri(serviceRootUri));
            container.SendingRequest2 += (sender, eventArgs) => ((Microsoft.OData.Client.HttpWebRequestMessage)eventArgs.RequestMessage).SetHeader("Accept", mime);
            Proxy.Window window = await container.Windows.ByKey(new Dictionary<string, object>() { { "Id", 1 } }).GetValueAsync();
            Proxy.Circle expectedShape = new Proxy.Circle() { Center = new Proxy.Point(), Radius = 2 };
            Assert.Equal(expectedShape, window.CurrentShape);
            Assert.Equal(1, window.OptionalShapes.Count);
        }

        [Theory]
        [PropertyData("MediaTypes")]
        // GET ~/Windows?$select=...&$orderby=...&$expand=...
        public async Task QueryCollectionContainingEntity(string mode, string mime)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format(
                "{0}/Windows?$select=Id,CurrentShape,OptionalShapes&$orderby=CurrentShape/HasBorder&$expand=Parent&$format={1}", serviceRootUri, mime);
            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                HttpStatusCode.Created,
                    response.StatusCode,
                    requestUri,
                    contentOfString));
            JObject content = await response.Content.ReadAsAsync<JObject>();
            JArray windows = content["value"] as JArray;
            Assert.True(3 == windows.Count);

            JObject window1 = (JObject)windows.Single(w => (string)w["Id"] == "1");
            JArray optionalShapes = (JArray)window1["OptionalShapes"];
            Assert.True(1 == optionalShapes.Count);

            JObject window2 = (JObject)windows.Single(w => (string)w["Id"] == "2");
            Assert.Equal("1", (string)window2["Parent"]["Id"]);
        }

        [Theory]
        [PropertyData("MediaTypes")]
        // GET ~/Windows?$filter=CurrentShape/HasBorder eq true
        public async Task QueryEntitiesFilteredByComplexType(string mode, string mime)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format(
                "{0}/Windows?$filter=CurrentShape/HasBorder eq true&$format={1}", serviceRootUri, mime);
            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                HttpStatusCode.Created,
                    response.StatusCode,
                    requestUri,
                    contentOfString));
            JObject content = await response.Content.ReadAsAsync<JObject>();
            JArray windows = content["value"] as JArray;
            Assert.True(1 == windows.Count,
                String.Format("\nExpected count: {0},\n actual: {1},\n request uri: {2},\n response payload: {3}", 1, windows.Count, requestUri, contentOfString));
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // PUT ~/Windows(3)
        public async Task PutContainingEntity(string modelMode)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, modelMode).ToLower();
            string requestUri = serviceRootUri + "/Windows(3)";

            string content = @"
{
    'Id':3,
    'Name':'Name30',
    'CurrentShape':
    {
        '@odata.type':'#WebStack.QA.Test.OData.ComplexTypeInheritance.Circle',  
        'Radius':2,
        'Center':{'X':1,'Y':2},
        'HasBorder':true
    },
    'OptionalShapes':
    [
        {
            '@odata.type':'#WebStack.QA.Test.OData.ComplexTypeInheritance.Rectangle',
            'HasBorder':true,
            'Width':3,
            'Height':4,
            'TopLeft':{ 'X':1,'Y':2}
        }
    ]
}";
            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            HttpResponseMessage response = await Client.PutAsync(requestUri, stringContent);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                    HttpStatusCode.Created,
                    response.StatusCode,
                    requestUri,
                    contentOfString));

            JObject contentOfJObject = await response.Content.ReadAsAsync<JObject>();
            string name = (string)contentOfJObject["Name"];
            Assert.True("Name30" == name);
            int radius = (int)contentOfJObject["CurrentShape"]["Radius"];
            Assert.True(2 == radius,
                String.Format("\nExpected that Radius: 2, but actually: {0},\n request uri: {1},\n response payload: {2}", radius, requestUri, contentOfString));

            JArray windows = contentOfJObject["OptionalShapes"] as JArray;
            Assert.True(1 == windows.Count,
                String.Format("\nExpected count: {0},\n actual: {1},\n request uri: {2},\n response payload: {3}", 1, windows.Count, requestUri, contentOfString));
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // Patch ~/Widnows(1)
        public async Task PatchContainingEntity(string modelMode)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, modelMode).ToLower();
            string requestUri = serviceRootUri + "/Windows(3)";

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            var content = @"
{
    'CurrentShape':
    {
        '@odata.type':'#WebStack.QA.Test.OData.ComplexTypeInheritance.Circle',  
        'Radius':2,
        'Center':{'X':1,'Y':2},
        'HasBorder':true
    },
    'OptionalShapes': [ ]
}";
            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            request.Content = stringContent;
            HttpResponseMessage response = await Client.SendAsync(request);
            string contentOfString = await response.Content.ReadAsStringAsync();
            if (HttpStatusCode.OK != response.StatusCode)
            {
                Assert.True(false, String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                    HttpStatusCode.Created,
                    response.StatusCode,
                    requestUri,
                    contentOfString));
            }
            JObject contentOfJObject = await response.Content.ReadAsAsync<JObject>();
            string name = (string)contentOfJObject["Name"];
            Assert.True("AnotherPopup" == name);
            int radius = (int)contentOfJObject["CurrentShape"]["Radius"];
            Assert.True(2 == radius,
                String.Format("\nExpected that Radius: 2, but actually: {0},\n request uri: {1},\n response payload: {2}", radius, requestUri, contentOfString));

            JArray windows = contentOfJObject["OptionalShapes"] as JArray;
            Assert.True(0 == windows.Count,
                String.Format("\nExpected count: {0},\n actual: {1},\n request uri: {2},\n response payload: {3}", 1, windows.Count, requestUri, contentOfString));
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // DELETE ~/Windows(1)
        public async Task DeleteWindow(string modelMode)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, modelMode).ToLower();
            string requestUri = serviceRootUri + "/Windows(1)";

            HttpResponseMessage response = await Client.DeleteAsync(requestUri);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        #endregion

        #region RUD on complex type

        [Theory]
        [PropertyData("MediaTypes")]
        // GET ~/Windows(1)/CurrentShape
        public async Task QueryComplexTypeProperty(string mode, string mime)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format(
                "{0}/Windows(1)/CurrentShape?$format={1}", serviceRootUri, mime);
            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                HttpStatusCode.Created,
                    response.StatusCode,
                    requestUri,
                    contentOfString));
            JObject content = await response.Content.ReadAsAsync<JObject>();
            bool hasBorder = (bool)content["HasBorder"];
            Assert.True(hasBorder,
                String.Format("\nExpected that HasBorder is true, but actually not,\n request uri: {0},\n response payload: {1}", requestUri, contentOfString));
            int radius = (int)content["Radius"];
            Assert.True(2 == radius,
                String.Format("\nExpected that Radius: 2, but actually: {0},\n request uri: {1},\n response payload: {2}", radius, requestUri, contentOfString));
        }

        [Theory(Skip = "[UriParser] Cast segment following a collection complex type property reports exception.")]
        [InlineData("convention")]
        [InlineData("explicit")]
        // GET ~/Windows(1)/CurrentShape/WebStack.QA.Test.OData.ComplexTypeInheritance.Circle
        public async Task GetOptionalShapesPlusCast(string modelMode)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, modelMode).ToLower();
            string requestUri = serviceRootUri + "/Windows(3)/OptionalShapes/WebStack.QA.Test.OData.ComplexTypeInheritance.Circle";

            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode, String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                    HttpStatusCode.OK,
                    response.StatusCode,
                    requestUri,
                    contentOfString));

            JObject contentOfJObject = await response.Content.ReadAsAsync<JObject>();
            JArray optionalShapes = (JArray)contentOfJObject["value"];
            Assert.True(1 == optionalShapes.Count);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // GET ~/Windows(3)/OptionalShapes
        public async Task GetOptionalShapes(string modelMode)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, modelMode).ToLower();
            string requestUri = serviceRootUri + "/Windows(3)/OptionalShapes";

            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode, String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                    HttpStatusCode.OK,
                    response.StatusCode,
                    requestUri,
                    contentOfString));

            JObject contentOfJObject = await response.Content.ReadAsAsync<JObject>();
            JArray optionalShapes = (JArray)contentOfJObject["value"];
            Assert.True(2 == optionalShapes.Count);
        }

        [Theory]
        [PropertyData("MediaTypes")]
        // GET ~/Windows(1)/CurrentShape/HasBorder
        public async Task QueryPropertyDefinedInComplexTypeProperty(string mode, string mime)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format(
                "{0}/Windows(1)/CurrentShape/HasBorder?$format={1}", serviceRootUri, mime);
            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                HttpStatusCode.Created,
                    response.StatusCode,
                    requestUri,
                    contentOfString));
            JObject content = await response.Content.ReadAsAsync<JObject>();
            bool hasBorder = (bool)content["value"];
            Assert.True(hasBorder,
                String.Format("\nExpected that HasBorder is true, but actually not,\n request uri: {0},\n response payload: {1}", requestUri, contentOfString));
        }

        [Theory]
        [PropertyData("MediaTypes")]
        // GET ~/Windows(1)/CurrentShape/WebStack.QA.Test.OData.ComplexTypeInheritance.Circle/Radius
        public async Task QueryComplexTypePropertyDefinedOnDerivedType(string mode, string mime)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format(
                "{0}/Windows(1)/CurrentShape/WebStack.QA.Test.OData.ComplexTypeInheritance.Circle/Radius?$format={1}", serviceRootUri, mime);
            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                HttpStatusCode.Created,
                    response.StatusCode,
                    requestUri,
                    contentOfString));
            JObject content = await response.Content.ReadAsAsync<JObject>();
            int radius = (int)content["value"];
            Assert.True(2 == radius,
                String.Format("\nExpected that Radius: 2, but actually: {0},\n request uri: {1},\n response payload: {2}", radius, requestUri, contentOfString));
        }

        [Theory(Skip = "Support collection of complex type value as function parameter")]
        [InlineData("convention")]
        [InlineData("explicit")]
        // PUT ~/Windows(3)/OptionalShapes
        public async Task PutCollectionComplexTypeProperty(string modelMode)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, modelMode).ToLower();
            string requestUri = serviceRootUri + "/Windows(3)/OptionalShapes";

            var content = new StringContent(content: @"
{
  'value':[
    {
        '@odata.type':'#WebStack.QA.Test.OData.ComplexTypeInheritance.Polygon',
        'HasBorder':true,'Vertexes':[
        {'X':1,'Y':2},
        {'X':2,'Y':3},
        {'X':4,'Y':8}
      ]
    },
    {
      '@odata.type':'#WebStack.QA.Test.OData.ComplexTypeInheritance.Circle',
        'HasBorder':true,
        'Center':{'X':3,'Y':3},
        'Radius':2
    }
  ]
}
", encoding: Encoding.UTF8, mediaType: "application/json");
            HttpResponseMessage response = await Client.PutAsync(requestUri, content);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                    HttpStatusCode.Created,
                    response.StatusCode,
                    requestUri,
                    contentOfString));

            JArray contentOfJObject = await response.Content.ReadAsAsync<JArray>();
            Assert.True(2 == contentOfJObject.Count,
                String.Format("\nExpected count: {0},\n actual: {1},\n request uri: {2},\n response payload: {3}",
                2,
                contentOfJObject.Count,
                requestUri,
                contentOfString));
        }

        [Theory(Skip = "Support deserialize complex type value")]
        [InlineData("convention")]
        [InlineData("explicit")]
        // PUT ~/Widnows(1)/CurrentShape
        public async Task PutCurrentShape(string modelMode)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, modelMode).ToLower();
            string requestUri = serviceRootUri + "/Windows(3)/CurrentShape";

            var content = new StringContent(content: @"
{
    '@odata.type':'#WebStack.QA.Test.OData.ComplexTypeInheritance.Circle',  
    'Radius':5,
    'Center':{'X':1,'Y':2},
    'HasBorder':true 
}", encoding: Encoding.UTF8, mediaType: "application/json");
            HttpResponseMessage response = await Client.PutAsync(requestUri, content);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode, String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                    HttpStatusCode.OK,
                    response.StatusCode,
                    requestUri,
                    contentOfString));

            JObject contentOfJObject = await response.Content.ReadAsAsync<JObject>();
            int radius = (int)contentOfJObject["Radius"];
            Assert.True(5 == radius,
                String.Format("\nExpected that Radius: 5, but actually: {0},\n request uri: {1},\n response payload: {2}", radius, requestUri, contentOfString));
        }

        #endregion
    }
}
