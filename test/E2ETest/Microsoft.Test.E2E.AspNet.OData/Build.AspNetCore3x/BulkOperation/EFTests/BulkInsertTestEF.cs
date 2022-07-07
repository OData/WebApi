﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Test.E2E.AspNet.OData.BulkOperation;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.BulkInsert
{
    public class BulkInsertTestEF : WebHostTestBase
    {
        public BulkInsertTestEF(WebHostTestFixture fixture)
            :base(fixture)
        {            
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(EmployeesControllerEF), typeof(MetadataController) };
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

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();                
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains("Sql", json.ToString());
            }
                       
        }

        [Fact]
        public async Task PatchEmployee_WithUpdates_WithEmployees()
        {
            //Arrange

            string requestUri = this.BaseAddress + "/convention/Employees(1)";

            var content = @"{
                    'Name':'SqlFU'  ,
                    'Friends':[{'Id':345,'Name':'Test2'},{'Id':400,'Name':'Test3'},{'Id':900,'Name':'Test93'}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains("SqlFU", json.ToString());
            }

        }

        [Fact]
        public async Task PatchEmployee_WithUpdates_Employees()
        {
            //Arrange
            
            string requestUri = this.BaseAddress + "/convention/Employees";

            var content = @"{'@odata.context':'http://host/service/$metadata#Employees/$delta',     
                    'value':[{ '@odata.type': '#Microsoft.Test.E2E.AspNet.OData.BulkInsert.Employee', 'ID':1,'Name':'Employee1',
                            'Friends@odata.delta':[{'Id':1,'Name':'Friend1',
                            'Orders@odata.delta' :[{'Id':1,'Price': 10}, {'Id':2,'Price': 20} ] },{'Id':2,'Name':'Friend2'}]
                                },
                            {  '@odata.type': '#Microsoft.Test.E2E.AspNet.OData.BulkInsert.Employee', 'ID':2,'Name':'Employee2',
                            'Friends@odata.delta':[{'Id':3,'Name':'Friend3',
                            'Orders@odata.delta' :[{'Id':3,'Price': 30}, {'Id':4,'Price': 40} ]},{'Id':4,'Name':'Friend4'}]
                                }]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            // Act & Assert

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Contains("Employee1", json);
                Assert.Contains("Employee2", json);
            }

        }


        [Fact]
        public async Task PatchEmployee_WithDelete()
        {
            //Arrange
            
            string requestUri = this.BaseAddress + "/convention/Employees(1)";

            var content = @"{
                    'Name':'Sql',
                    'Friends@odata.delta':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Contains("Sql", json);
            }
           
  
        }


        [Fact]
        public async Task PatchEmployee_WithAddUpdateAndDelete()
        {
            //Arrange

            string requestUri = this.BaseAddress + "/convention/Employees(1)";

            var content = @"{
                    'Name':'SqlUD',
                    'Friends@odata.delta':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{'Id':2,'Name':'Test3'},{'Id':3,'Name':'Test4'}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Contains("SqlUD", json);
            }

        }


        [Fact]
        public async Task PatchEmployee_WithMultipleUpdatesinOrder1()
        {
            //Arrange
            
            string requestUri = this.BaseAddress + "/convention/Employees(1)";

            var content = @"{
                    'Name':'SqlMU'  ,
                    'Friends@odata.delta':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{'Id':1,'Name':'Test_1'},{'Id':2,'Name':'Test3'},{'Id':3,'Name':'Test4'}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var json = response.Content.ReadAsStringAsync().Result;                
                Assert.Contains("SqlMU", json);
            }

        }

        [Fact]
        public async Task PatchEmployee_WithMultipleUpdatesinOrder2()
        {
            //Arrange
            
            string requestUri = this.BaseAddress + "/convention/Employees(1)";

            var content = @"{
                    'Name':'SqlMU1'  ,
                    'Friends@odata.delta':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{'Id':1,'Name':'Test_1'},{'Id':2,'Name':'Test3'},{'Id':3,'Name':'Test4'},{ '@odata.removed' : {'reason':'changed'}, 'Id':1}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Contains("SqlMU1", json);
            }
                        
        }


        #endregion

    }
}