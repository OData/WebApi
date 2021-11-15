//-----------------------------------------------------------------------------
// <copyright file="ComplexTypeInheritanceTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance
{
    public class ComplexTypeInheritanceTests : WebHostTestBase
    {
        public ComplexTypeInheritanceTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

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

        public static TheoryDataSet<string, string, string,bool> PostToCollectionNewComplexTypeMembers
        {
            get
            {
                string[] modes = new string[] { "convention", "explicit" };
                string[] targets = { "OptionalShapes", "PolygonalShapes" };
                bool[] representations = { true, false };
                string[] objects = new string[]
                {
                    @"
{
        '@odata.type':'#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Polygon',
        'HasBorder':true,'Vertexes':[
            {'X':21,'Y':12},
            {'X':32,'Y':23},
            {'X':14,'Y':41}
        ]
}",
                    @"
{
        '@odata.type':'#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Rectangle',
        'HasBorder':true,
        'Width':3,
        'Height':4,
        'TopLeft':{ 'X':1,'Y':2}
}",
                };

                TheoryDataSet<string, string, string, bool> data = new TheoryDataSet<string, string, string, bool>();

                foreach(string mode in modes)
                {
                    foreach(string obj in objects)
                    {
                        foreach(string target in targets)
                            foreach(bool representation in representations)
                            {
                                data.Add(mode, obj, target, representation);
                            }
                    }
                }
                return data;
            }

        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(WindowsController), typeof(MetadataController) };
            configuration.AddControllers(controllers);

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration
                .MapODataServiceRoute(routeName: "convention",
                    routePrefix: "convention",
                    model: ComplexTypeInheritanceEdmModels.GetConventionModel());

            configuration
                .MapODataServiceRoute(routeName: "explicit",
                    routePrefix: "explicit",
                    model: ComplexTypeInheritanceEdmModels.GetExplicitModel(),
                    batchHandler: configuration.CreateDefaultODataBatchHandler());

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
        '@odata.type':'#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Circle',  
        'Radius':10,
        'Center':{'X':1,'Y':2},
        'HasBorder':true
    },
    'OptionalShapes':
    [
        {
            '@odata.type':'#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Rectangle',
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

            Assert.Equal("4.0", response.Headers.GetValues("OData-Version").Single());
            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
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

        [Theory]
        [MemberData(nameof(MediaTypes))]
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
            JObject content = await response.Content.ReadAsObject<JObject>();
            JArray windows = content["value"] as JArray;
            Assert.True(3 == windows.Count);

            JObject window1 = (JObject)windows.Single(w => (string)w["Id"] == "1");
            JArray optionalShapes = (JArray)window1["OptionalShapes"];
            Assert.True(1 == optionalShapes.Count);

            JObject window2 = (JObject)windows.Single(w => (string)w["Id"] == "2");
            Assert.Equal("1", (string)window2["Parent"]["Id"]);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
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
            JObject content = await response.Content.ReadAsObject<JObject>();
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
        '@odata.type':'#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Circle',  
        'Radius':2,
        'Center':{'X':1,'Y':2},
        'HasBorder':true
    },
    'OptionalShapes':
    [
        {
            '@odata.type':'#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Rectangle',
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

            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
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
            string requestUri = serviceRootUri + "/Windows(1)";

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            // We should be able to PATCH nested resource with delta object of the same CLR type.
            var content = @"
{
    'CurrentShape':
    {
        '@odata.type':'#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Circle',  
        'Radius':1,
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
            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
            string name = (string)contentOfJObject["Name"];
            Assert.True("CircleWindow" == name);
            int radius = (int)contentOfJObject["CurrentShape"]["Radius"];
            Assert.True(1 == radius,
                String.Format("\nExpected that Radius: 2, but actually: {0},\n request uri: {1},\n response payload: {2}", radius, requestUri, contentOfString));

            JArray windows = contentOfJObject["OptionalShapes"] as JArray;
            Assert.True(0 == windows.Count,
                String.Format("\nExpected count: {0},\n actual: {1},\n request uri: {2},\n response payload: {3}", 1, windows.Count, requestUri, contentOfString));
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // Patch ~/Widnows(3)
        public async Task PatchContainingEntity_Matched_DerivedType(string modelMode)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, modelMode).ToLower();
            string requestUri = serviceRootUri + "/Windows(3)";

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            // Attempt to PATCH nested resource with delta object of the different CLR type
            // will result an error.
            var content = @"
            {
                'CurrentShape':
                {
                    '@odata.type':'#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Circle',
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
            Assert.True(HttpStatusCode.OK == response.StatusCode);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // Patch ~/Widnows(3)
        public async Task Patchy_Matched_DerivedComplexType(string modelMode)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, modelMode).ToLower();
            string requestUri = serviceRootUri + "/Windows(3)/CurrentShape";

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            // Attempt to PATCH nested resource with delta object of the different CLR type
            // will result an error.
            

           var content = @"
    {
        '@odata.type':'#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Circle',
        'Radius':2,
        'Center':{'X':1,'Y':2},
        'HasBorder':true
    }";

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            request.Content = stringContent;
            HttpResponseMessage response = await Client.SendAsync(request);
            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(2, (int)contentOfJObject["Radius"]);
            Assert.True(HttpStatusCode.OK == response.StatusCode);
        }


        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // Patch ~/Widnows(3)
        public async Task PatchContainingEntity_DeltaIsBaseType(string modelMode)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, modelMode).ToLower();
            string requestUri = serviceRootUri + "/Windows(3)";

            // PATCH nested resource with delta object of the base CLR type should work.
            // --- PATCH #1 ---
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            var content = @"
{
    'CurrentShape':
    {
        '@odata.type':'#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Polygon',
        'HasBorder':true
    },
    'OptionalShapes': [ ]
}";
            string contentOfString = await ExecuteAsync(request, content);

            // Only 'HasBoarder' is updated; 'Vertexes' still has the correct value.
            Assert.Contains("\"HasBorder\":true", contentOfString);
            Assert.Contains("\"Vertexes\":[{\"X\":0,\"Y\":0},{\"X\":2,\"Y\":0},{\"X\":2,\"Y\":2},{\"X\":0,\"Y\":2}]", contentOfString);


            // --- PATCH #2 ---
            request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            content = @"
{
    'CurrentShape':
    {
        '@odata.type':'#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Polygon',
        'Vertexes':[ {'X':1,'Y':2}, {'X':2,'Y':3}, {'X':4,'Y':8} ]
    },
    'OptionalShapes': [ ]
}";
            contentOfString = await ExecuteAsync(request, content);

            // Only 'Vertexes' is updated;  'HasBoarder' still has the correct value.
            Assert.Contains("\"Vertexes\":[{\"X\":1,\"Y\":2},{\"X\":2,\"Y\":3},{\"X\":4,\"Y\":8}]", contentOfString);
            Assert.Contains("\"HasBorder\":false", contentOfString);
        }

        private async Task<string> ExecuteAsync(HttpRequestMessage request, string content)
        {
            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            request.Content = stringContent;
            HttpResponseMessage response = await Client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
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
        [MemberData(nameof(MediaTypes))]
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
            JObject content = await response.Content.ReadAsObject<JObject>();
            bool hasBorder = (bool)content["HasBorder"];
            Assert.True(hasBorder,
                String.Format("\nExpected that HasBorder is true, but actually not,\n request uri: {0},\n response payload: {1}", requestUri, contentOfString));
            int radius = (int)content["Radius"];
            Assert.True(2 == radius,
                String.Format("\nExpected that Radius: 2, but actually: {0},\n request uri: {1},\n response payload: {2}", radius, requestUri, contentOfString));
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // GET ~/Windows(1)/OptionalShapes/Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Circle
        public async Task GetOptionalShapesPlusCast(string modelMode)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, modelMode).ToLower();
            string requestUri = serviceRootUri + "/Windows(3)/OptionalShapes/Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Circle";

            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode, String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                    HttpStatusCode.OK,
                    response.StatusCode,
                    requestUri,
                    contentOfString));

            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
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

            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
            JArray optionalShapes = (JArray)contentOfJObject["value"];
            Assert.True(2 == optionalShapes.Count);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
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
            JObject content = await response.Content.ReadAsObject<JObject>();
            bool hasBorder = (bool)content["value"];
            Assert.True(hasBorder,
                String.Format("\nExpected that HasBorder is true, but actually not,\n request uri: {0},\n response payload: {1}", requestUri, contentOfString));
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // GET ~/Windows(1)/CurrentShape/Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Circle/Radius
        public async Task QueryComplexTypePropertyDefinedOnDerivedType(string mode, string mime)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format(
                "{0}/Windows(1)/CurrentShape/Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Circle/Radius?$format={1}", serviceRootUri, mime);
            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                HttpStatusCode.Created,
                    response.StatusCode,
                    requestUri,
                    contentOfString));
            JObject content = await response.Content.ReadAsObject<JObject>();
            int radius = (int)content["value"];
            Assert.True(2 == radius,
                String.Format("\nExpected that Radius: 2, but actually: {0},\n request uri: {1},\n response payload: {2}", radius, requestUri, contentOfString));
        }

        [Theory]
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
        '@odata.type':'#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Polygon',
        'HasBorder':true,'Vertexes':[
        {'X':1,'Y':2},
        {'X':2,'Y':3},
        {'X':4,'Y':8}
      ]
    },
    {
      '@odata.type':'#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Circle',
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

            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
            Assert.True(2 == contentOfJObject.Count,
                String.Format("\nExpected count: {0},\n actual: {1},\n request uri: {2},\n response payload: {3}",
                2,
                contentOfJObject.Count,
                requestUri,
                contentOfString));
        }

        [Theory]
        [MemberData(nameof(PostToCollectionNewComplexTypeMembers))]
        // POST ~/Windows(3)/OptionalShapes
        public async Task PostToCollectionComplexTypeProperty(string modelMode, string jObject, string targetPropertyResource, bool returnRepresentation)
        {
            //Arrange
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, modelMode).ToLower();
            string requestUri = serviceRootUri + "/Windows(3)/"+ targetPropertyResource;

            //send a get request to get the current count
            int count = 0;
            using (HttpResponseMessage getResponse = await this.Client.GetAsync(requestUri))
            {
                getResponse.EnsureSuccessStatusCode();

                var json = await getResponse.Content.ReadAsObject<JObject>();
                var state = json.GetValue("value") as JArray;
                count = state.Count;
            }

            //Set up the post request
            var requestForPost = new HttpRequestMessage(HttpMethod.Post, requestUri);
            requestForPost.Content = new StringContent(content:jObject, encoding: Encoding.UTF8, mediaType: "application/json");
            if (returnRepresentation)
            {
                requestForPost.Headers.Add("Prefer", "return=representation");
            }

            //Act & Assert
            HttpResponseMessage response = await Client.SendAsync(requestForPost);
            string contentOfString = await response.Content.ReadAsStringAsync();

            if(returnRepresentation)
            {
                JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
                var result = contentOfJObject.GetValue("value") as JArray;
            
                Assert.True(count + 1 == result.Count,
                    String.Format("\nExpected count: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                    HttpStatusCode.NoContent,
                    result.Count,
                    requestUri,
                    contentOfString));
            }
            else
            {
                Assert.True(HttpStatusCode.NoContent == response.StatusCode,
                    String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                    HttpStatusCode.NoContent,
                    response.StatusCode,
                    requestUri,
                    contentOfString));
            }

        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // PUT ~/Widnows(1)/CurrentShape
        public async Task PutCurrentShape(string modelMode)
        {
            // Arrange
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, modelMode).ToLower();
            string requestUri = serviceRootUri + "/Windows(1)/CurrentShape/Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Circle";

            var content = new StringContent(content: @"
{
    '@odata.type':'#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Circle',
    'Radius':5,
    'Center':{'X':1,'Y':2},
    'HasBorder':true 
}", encoding: Encoding.UTF8, mediaType: "application/json");

            // Act
            HttpResponseMessage response = await Client.PutAsync(requestUri, content);

            // Assert
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode, String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                    HttpStatusCode.OK,
                    response.StatusCode,
                    requestUri,
                    contentOfString));

            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
            int radius = (int)contentOfJObject["Radius"];
            Assert.True(5 == radius,
                String.Format("\nExpected that Radius: 5, but actually: {0},\n request uri: {1},\n response payload: {2}", radius, requestUri, contentOfString));
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // PATCH ~/Windows(3)/OptionalShapes
        public async Task PatchToCollectionComplexTypePropertyNotSupported(string modelMode)
        {
            // Arrange
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, modelMode).ToLower();
            string requestUri = serviceRootUri + "/Windows(3)/OptionalShapes";

            // Act
            HttpResponseMessage response = await Client.PatchAsync(new Uri(requestUri), "");

            // Assert
            Assert.True(HttpStatusCode.NotFound == response.StatusCode);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task PatchToSingleComplexTypeProperty(string modelMode)
        {
            // Arrange
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, modelMode).ToLower();
            string requestUri = serviceRootUri + "/Windows(1)/CurrentShape/Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Circle";

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("Patch"), requestUri);
            request.Content = new StringContent(@"
{
    '@odata.type':'#Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance.Circle',
    'Radius':15,
    'HasBorder':true
}");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
            int radius = (int)contentOfJObject["Radius"];
            Assert.Equal(15, radius);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task DeleteToNullableComplexTypeProperty(string modelMode)
        {
            // Arrange
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, modelMode).ToLower();
            string requestUri = serviceRootUri + "/Windows(1)/CurrentShape";

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("Delete"), requestUri);

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        #endregion
    }
}
