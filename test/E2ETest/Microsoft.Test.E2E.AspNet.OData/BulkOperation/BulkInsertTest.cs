// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.BulkInsert1
{
    public class BulkInsertTest : WebHostTestBase
    {
        public BulkInsertTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(EmployeesController), typeof(MetadataController) };
            configuration.AddControllers(controllers);

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("convention", "convention", BulkInsertEdmModel.GetConventionModel(configuration));
            configuration.MapODataServiceRoute("explicit", "explicit", BulkInsertEdmModel.GetExplicitModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.EnsureInitialized();
        }


        #region Update
         
        [Fact]
        public async Task PatchEmployee_WithUpdates()
        {
            //Arrange
            
            string requestUri = this.BaseAddress + "/convention/Employees(1)";

            var content = @"{
                    'Name':'Sql'  ,
                    'Friends@odata.delta':[{'Id':1,'Name':'Test2'},{'Id':2,'Name':'Test3'}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            //Act
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Assert
            requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;

                Assert.Equal(2, result.Count);
                Assert.Contains("Test2", result.ToString());
            }

        }

        [Fact]
        public async Task PatchEmployee_WithUpdates_WithEmployees()
        {
            //Arrange

            string requestUri = this.BaseAddress + "/convention/Employees(1)";

            var content = @"{
                    'Name':'Sql'  ,
                    'Friends':[{'Id':345,'Name':'Test2'},{'Id':400,'Name':'Test3'},{'Id':900,'Name':'Test93'}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Assert
            requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;

                Assert.Equal(3, result.Count);
                Assert.Contains("345", result.ToString());
                Assert.Contains("400", result.ToString());
                Assert.Contains("900", result.ToString());
            }

        }


        [Fact]
        public async Task PatchEmployee_WithUpdates_Friends()
        {
            //Arrange
            
            string requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            
            var content = @"{'@odata.context':'http://host/service/$metadata#Employees(1)/Friends/$delta',     
                    'value':[{ 'Id':1,'Name':'Friend1'}, { 'Id':2,'Name':'Friend2'}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Assert
            requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;

                Assert.Equal(2, result.Count);
                Assert.Contains("Friend1", result.ToString());
                Assert.Contains("Friend2", result.ToString());
            }
        }

        [Fact]
        public async Task PatchEmployee_WithDeletes_Friends()
        {
            //Arrange
            
            string requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";

            var content = @"{'@odata.context':'http://host/service/$metadata#Employees(1)/Friends/$delta',     
                    'value':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{ 'Id':2,'Name':'Friend2'}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Assert
            requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;

                Assert.Single(result);                
                Assert.Contains("Friend2", result.ToString());
            }

        }

        [Fact]
        public async Task PatchEmployee_WithDeletes_Friends_WithNestedTypes()
        {
            //Arrange

            string requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";

            var content = @"{'@odata.context':'http://host/service/$metadata#Employees(1)/Friends/$delta',     
                    'value':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1, 'Orders@odata.delta' :[{'Id':1,'Price': 10}, {'Id':2,'Price': 20} ] },{ 'Id':2,'Name':'Friend2'}]
                     }";


            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Assert
            requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;

                Assert.Single(result);
                Assert.Contains("Friend2", result.ToString());
            }

        }

        [Fact]
        public async Task PatchEmployee_WithDeletes_Friends_WithNestedDeletes()
        {
            //Arrange

            string requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";

            var content = @"{'@odata.context':'http://host/service/$metadata#Employees(1)/Friends/$delta',     
                    'value':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1, 'Orders@odata.delta' :[{'@odata.removed' : {'reason':'changed'}, 'Id':1,'Price': 10}, {'Id':2,'Price': 20} ] },{ 'Id':2,'Name':'Friend2'}]
                     }";


            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Assert
            requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;

                Assert.Single(result);
                Assert.Contains("Friend2", result.ToString());
            }

        }

        [Fact]
        public async Task PatchEmployee_WithAdds_Friends_WithAnnotations()
        {
            //Arrange

            string requestUri = this.BaseAddress + "/convention/Employees(1)/NewFriends";
            //{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{ '@odata.removed' : {'reason':'deleted'}, 'Id':2},
            var content = @"{'@odata.context':'http://host/service/$metadata#Employees(1)/NewFriends/$delta',     
                    'value':[{ 'Id':3, 'Age':35, '@NS.Test':1}]
                     }";


            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            requestForPost.Content = new StringContent(content);

            requestForPost.Content.Headers.ContentType= MediaTypeWithQualityHeaderValue.Parse("application/json");
           // StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            //requestForPost.Content = stringContent;
            
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                json.ToString().Contains("$deletedEntity");
            }
        }

        [Fact]
        public async Task PatchEmployee_WithFailedAdds_Friends()
        {
            //Arrange
            
            string requestUri = this.BaseAddress + "/convention/Employees(1)/NewFriends";
            //{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{ '@odata.removed' : {'reason':'deleted'}, 'Id':2},
            var content = @"{'@odata.context':'http://host/service/$metadata#Employees(1)/NewFriends/$delta',     
                    'value':[{ 'Id':3, 'Age':3, '@NS.Test':1}]
                     }";

           
            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                json.ToString().Contains("$deletedEntity");
            }
        }

        [Fact]
        public async Task PatchEmployee_WithFailedDeletes_Friends()
        {
            //Arrange            
            string requestUri = this.BaseAddress + "/convention/Employees(2)/NewFriends";
            //{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{ '@odata.removed' : {'reason':'deleted'}, 'Id':2},
            var content = @"{'@odata.context':'http://host/service/$metadata#Employees(1)/NewFriends/$delta',     
                    'value':[{ '@odata.removed' : {'reason':'changed'}, 'Id':2, '@NS.Test':1}]
                     }";


            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains("$delta", json.ToString());
            }

        }


        [Fact]
        public async Task PatchEmployee_WithFailedOperation_WithAnnotations()
        {
            //Arrange            
            string requestUri = this.BaseAddress + "/convention/Employees(2)/NewFriends";
            //{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{ '@odata.removed' : {'reason':'deleted'}, 'Id':2},
            var content = @"{'@odata.context':'http://host/service/$metadata#Employees(2)/NewFriends/$delta',     
                    'value':[{ '@odata.removed' : {'reason':'changed'}, 'Id':2, '@Core.ContentID':3, '@NS.Test2':'testing'}]
                     }";


            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var str = json.ToString();
                Assert.Contains("$delta",str);                
                Assert.Contains("NS.Test2", str);
                Assert.Contains("Core.DataModificationException", str);
            }

        }


        [Fact]
        public async Task PatchUntypedEmployee_WithAdds_Friends_Untyped()
        {
            //Arrange

            string requestUri = this.BaseAddress + "/convention/UnTypedEmployees";
            //{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{ '@odata.removed' : {'reason':'deleted'}, 'Id':2},
            var content = @"{'@odata.context':'http://host/service/$metadata#Employees(2)/UnTypedFriends/$delta',     
                    'value':[{ 'Id':3, 'Age':35,}]
                     }";

            content = @"{'@odata.context':'http://host/service/$metadata#UnTypedEmployees/$delta',     
                    'value':[{ 'ID':1,'Name':'Employee1',
                            'UnTypedFriends@odata.delta':[{'Id':1,'Name':'Friend1'},{'Id':2,'Name':'Friend2'}]
                                },
                            { 'ID':2,'Name':'Employee2',
                            'UnTypedFriends@odata.delta':[{'Id':3,'Name':'Friend3'},{'Id':4,'Name':'Friend4'}]
                                }]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                json.ToString().Contains("$deletedEntity");
            }
        }

        [Fact]
        public async Task PatchEmployee_WithAdds_Friends_WithNested_Untyped()
        {
            //Arrange

            string requestUri = this.BaseAddress + "/convention/Employees(1)/UnTypedFriends";
            //{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{ '@odata.removed' : {'reason':'deleted'}, 'Id':2},
            var content = @"{'@odata.context':'http://host/service/$metadata#Employees(1)/UnTypedFriends/$delta',     
                    'value':[{ 'Id':2, 'Name': 'Friend007', 'Age':35,'Address@odata.delta':{'Id':1, 'Street' : 'Abc 123'}, '@NS.Test':1}]
                     }";


            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                json.ToString().Contains("$delta");
                json.ToString().Contains("@NS.Test");
            }

        }


        [Fact]
        public async Task PatchEmployee_WithAdds_Friends_WithAnnotations_Untyped()
        {
            //Arrange

            string requestUri = this.BaseAddress + "/convention/Employees(2)/UnTypedFriends";
            //{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{ '@odata.removed' : {'reason':'deleted'}, 'Id':2},
            var content = @"{'@odata.context':'http://host/service/$metadata#Employees(2)/UnTypedFriends/$delta',     
                    'value':[{ 'Id':2, 'Age':35, '@NS.Test':1}]
                     }";


            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                json.ToString().Contains("$delta");
                json.ToString().Contains("@NS.Test");
            }
        }

        [Fact]
        public async Task PatchEmployee_WithFailedAdds_Friends_Untyped()
        {
            //Arrange

            string requestUri = this.BaseAddress + "/convention/Employees(3)/UnTypedFriends";
            //{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{ '@odata.removed' : {'reason':'deleted'}, 'Id':2},
            var content = @"{'@odata.context':'http://host/service/$metadata#Employees(3)/UnTypedFriends/$delta',     
                    'value':[{ 'Id':3, 'Age':3, '@NS.Test':1}]
                     }";


            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                json.ToString().Contains("$deletedEntity");
            }
        }

        [Fact]
        public async Task PatchEmployee_WithFailedDeletes_Friends_Untyped()
        {
            //Arrange            
            string requestUri = this.BaseAddress + "/convention/Employees(3)/UnTypedFriends";
            //{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{ '@odata.removed' : {'reason':'deleted'}, 'Id':2},
            var content = @"{'@odata.context':'http://host/service/$metadata#Employees(3)/UnTypedFriends/$delta',     
                    'value':[{ '@odata.removed' : {'reason':'changed'}, 'Id':5, '@NS.Test':1}]
                     }";


            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains("@Core.DataModificationException", json.ToString());
                Assert.Contains("@NS.Test", json.ToString());
            }

        }


        [Fact]
        public async Task PatchEmployee_WithFailedOperation_WithAnnotations_Untyped()
        {
            //Arrange            
            string requestUri = this.BaseAddress + "/convention/Employees(3)/UnTypedFriends";
            //{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{ '@odata.removed' : {'reason':'deleted'}, 'Id':2},
            var content = @"{'@odata.context':'http://host/service/$metadata#Employees(3)/UnTypedFriends/$delta',     
                    'value':[{ '@odata.removed' : {'reason':'changed'}, 'Id':5, '@Core.ContentID':3, '@NS.Test2':'testing'}]
                     }";


            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var str = json.ToString();
                Assert.Contains("$delta", str);                
                Assert.Contains("NS.Test2", str);
                Assert.Contains("Core.DataModificationException", str);
                Assert.Contains("Core.ContentID", str);                
            }

        }

        [Fact]
        public async Task PatchEmployee_WithUpdates_Employees()
        {
            //Arrange
            
            string requestUri = this.BaseAddress + "/convention/Employees";

            var content = @"{'@odata.context':'http://host/service/$metadata#Employees/$delta',     
                    'value':[{ '@odata.type': '#Microsoft.Test.E2E.AspNet.OData.BulkInsert1.Employee', 'ID':1,'Name':'Employee1',
                            'Friends@odata.delta':[{'Id':1,'Name':'Friend1',
                            'Orders@odata.delta' :[{'Id':1,'Price': 10}, {'Id':2,'Price': 20} ] },{'Id':2,'Name':'Friend2'}]
                                },
                            {  '@odata.type': '#Microsoft.Test.E2E.AspNet.OData.BulkInsert1.Employee', 'ID':2,'Name':'Employee2',
                            'Friends@odata.delta':[{'Id':3,'Name':'Friend3',
                            'Orders@odata.delta' :[{'Id':3,'Price': 30}, {'Id':4,'Price': 40} ]},{'Id':4,'Name':'Friend4'}]
                                }]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Assert
            requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                
                Assert.Contains("Friend1", json.ToString());
                Assert.Contains("Friend2", json.ToString());
            }

            requestUri = this.BaseAddress + "/convention/Employees(2)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();

                Assert.Contains("Friend3", json.ToString());
                Assert.Contains("Friend4", json.ToString());
            }
        }


        [Fact]
        public async Task PatchEmployee_WithDelete()
        {
            //Arrange
            
            string requestUri = this.BaseAddress + "/convention/Employees(1)";

            var content = @"{
                    'Name':'Sql'  ,
                    'Friends@odata.delta':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Assert
            requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;

                Assert.Single(result);
                Assert.DoesNotContain("Test0", result.ToString());
            }
        }


        [Fact]
        public async Task PatchEmployee_WithAddUpdateAndDelete()
        {
            //Arrange
            
            string requestUri = this.BaseAddress + "/convention/Employees(1)";

            var content = @"{
                    'Name':'Sql'  ,
                    'Friends@odata.delta':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{'Id':2,'Name':'Test3'},{'Id':3,'Name':'Test4'}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Assert
            requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;

                Assert.Equal(2, result.Count);
                Assert.DoesNotContain("Test0", result.ToString());
                Assert.Contains("Test3", result.ToString());
                Assert.Contains("Test4", result.ToString());
            }
        }


        [Fact]
        public async Task PatchEmployee_WithMultipleUpdatesinOrder1()
        {
            //Arrange
            
            string requestUri = this.BaseAddress + "/convention/Employees(1)";

            var content = @"{
                    'Name':'Sql'  ,
                    'Friends@odata.delta':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{'Id':1,'Name':'Test_1'},{'Id':2,'Name':'Test3'},{'Id':3,'Name':'Test4'}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Assert
            requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;

                Assert.Equal(3, result.Count);
                Assert.DoesNotContain("Test0", result.ToString());
                Assert.Contains("Test_1", result.ToString());
                Assert.Contains("Test3", result.ToString());
                Assert.Contains("Test4", result.ToString());
            }
        }

        [Fact]
        public async Task PatchEmployee_WithMultipleUpdatesinOrder2()
        {
            //Arrange
            
            string requestUri = this.BaseAddress + "/convention/Employees(1)";

            var content = @"{
                    'Name':'Sql'  ,
                    'Friends@odata.delta':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{'Id':1,'Name':'Test_1'},{'Id':2,'Name':'Test3'},{'Id':3,'Name':'Test4'},{ '@odata.removed' : {'reason':'changed'}, 'Id':1}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Assert
            requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;

                Assert.Equal(2, result.Count);
                Assert.DoesNotContain("Test0", result.ToString());
                Assert.DoesNotContain("Test_1", result.ToString());
                Assert.Contains("Test3", result.ToString());
                Assert.Contains("Test4", result.ToString());
            }
        }


        #endregion

        private async Task<HttpResponseMessage> ResetDatasource()
        {
            var uriReset = this.BaseAddress + "/convention/ResetDataSource";
            var response = await this.Client.PostAsync(uriReset, null);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            return response;
        }
    }
}