//-----------------------------------------------------------------------------
// <copyright file="BulkOperationTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.BulkOperation
{
    public class BulkOperationTest : WebHostTestBase
    {
        public BulkOperationTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(EmployeesController), typeof(CompanyController), typeof(MetadataController) };
            configuration.AddControllers(controllers);

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("convention", "convention", BulkOperationEdmModel.GetConventionModel(configuration));
            configuration.MapODataServiceRoute("explicit", "explicit", BulkOperationEdmModel.GetExplicitModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.EnsureInitialized();
        }

        #region Update
         
        [Fact]
        public async Task PatchEmployee_WithUpdates()
        {
            //Arrange
            string requestUri = this.BaseAddress + "/convention/Employees(1)";

            var content = @"{
                    'Name':'Sql' , 'FavoriteSports' :{'Sport': 'Cricket'},
                    'Friends@odata.delta':[{'Id':1,'Name':'Test2'},{'Id':2,'Name':'Test3'}]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Act & Assert
            requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;

                Assert.Equal(2, result.Count);
                Assert.Contains("Test2", result.ToString());
                Assert.Contains("Test3", result.ToString());
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

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Act & Assert
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
            
            var content = @"{'@odata.type': '#Microsoft.Test.E2E.AspNet.OData.BulkOperation.Friend',
                            '@odata.context':'" + this.BaseAddress + @"/convention/$metadata#Employees(1)/Friends/$delta',     
                    'value':[{ 'Id':1,'Name':'Friend1'}, { 'Id':2,'Name':'Friend2'}]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            requestForPatch.Headers.Add("OData-Version", "4.01");

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            //Act & Assert
            var expected = "$delta\",\"value\":[{\"Id\":1,\"Name\":\"Friend1\",\"Age\":33}," +
                "{\"Id\":2,\"Name\":\"Friend2\",\"Age\":0}]}";

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains(expected, json.ToString());
            }

            //Act & Assert
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
        public async Task PatchEmployee_WithUpdates_Friends_WithRelativeContextURL()
        {
            //Arrange
            string requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";

            var content = @"{
                            '@odata.context':'Employees(1)/Friends/$delta',     
                    'value':[{ 'Id':1,'Name':'Friend1'}, { 'Id':2,'Name':'Friend2'}]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            requestForPatch.Headers.Add("OData-Version", "4.01");

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            //Act & Assert
            var expected = "$delta\",\"value\":[{\"Id\":1,\"Name\":\"Friend1\",\"Age\":33}," +
                "{\"Id\":2,\"Name\":\"Friend2\",\"Age\":0}]}";

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains(expected, json.ToString());
            }

            //Act & Assert
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
        public async Task PatchEmployee_WithUpdates_Friends_WithOnlyDeltaContextURL()
        {
            //Arrange
            string requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";

            var content = @"{
                            '@odata.context':'#$delta',     
                    'value':[{ 'Id':1,'Name':'Friend1'}, { 'Id':2,'Name':'Friend2'}]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            requestForPatch.Headers.Add("OData-Version", "4.01");

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            //Act & Assert
            var expected = "$delta\",\"value\":[{\"Id\":1,\"Name\":\"Friend1\",\"Age\":33}," +
                "{\"Id\":2,\"Name\":\"Friend2\",\"Age\":0}]}";

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains(expected, json.ToString());
            }

            //Act & Assert
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

            var content = @"{'@odata.context':'" + this.BaseAddress + @"/convention/$metadata#Employees(1)/Friends/$delta',     
                    'value':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{ 'Id':2,'Name':'Friend2'}]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            requestForPatch.Headers.Add("OData-Version", "4.01");

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;

            //Act & Assert
            var expected = "$delta\",\"value\":[{\"@removed\":{\"reason\":\"changed\"}," +
                "\"@id\":\""+this.BaseAddress+"/convention/Friends(1)\",\"Id\":1,\"Name\":null,\"Age\":0},{\"Id\":2,\"Name\":\"Friend2\",\"Age\":0}]}";

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains(expected.ToLower(), json.ToString().ToLower());
            }

            //Act & Assert
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

            var content = @"{'@odata.context':'" + this.BaseAddress + @"/convention/$metadata#Employees(1)/Friends/$delta', '@odata.type': '#Microsoft.Test.E2E.AspNet.OData.BulkOperation.Friend',    
                    'value':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1, 'Orders@odata.delta' :[{'Id':1,'Price': 10}, {'Id':2,'Price': 20} ] },{ 'Id':2,'Name':'Friend2'}]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            requestForPatch.Headers.Add("OData-Version", "4.01");

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;

            //Act & Assert
            var expected = "$delta\",\"value\":[{\"@removed\":{\"reason\":\"changed\"}," +
                "\"@id\":\""+this.BaseAddress+"/convention/Friends(1)\",\"Id\":1,\"Name\":null,\"Age\":0},{\"Id\":2,\"Name\":\"Friend2\",\"Age\":0}]}";

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains(expected.ToLower(), json.ToString().ToLower());
            }

            //Act & Assert
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

            var content = @"{'@odata.context':'" + this.BaseAddress + @"/convention/$metadata#Employees(1)/Friends/$delta', '@odata.type': '#Microsoft.Test.E2E.AspNet.OData.BulkOperation.Friend',    
                    'value':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1, 'Orders@odata.delta' :[{'@odata.removed' : {'reason':'changed'}, 'Id':1,'Price': 10}, {'Id':2,'Price': 20} ] },{ 'Id':2,'Name':'Friend2'}]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            requestForPatch.Headers.Add("OData-Version", "4.01");

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;

            //Act & Assert
            var expected = "$delta\",\"value\":[{\"@removed\":{\"reason\":\"changed\"}," +
                "\"@id\":\""+ this.BaseAddress +"/convention/Friends(1)\",\"Id\":1,\"Name\":null,\"Age\":0},{\"Id\":2,\"Name\":\"Friend2\",\"Age\":0}]}";

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains(expected.ToLower(), json.ToString().ToLower());
            }
          
            requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;

                Assert.Contains("Friend2", result.ToString());
            }
        }

        [Fact]
        public async Task PatchEmployee_WithAdds_Friends_WithAnnotations()
        {
            //Arrange
            string requestUri = this.BaseAddress + "/convention/Employees(1)/NewFriends";
            var content = @"{'@odata.context':'" + this.BaseAddress + @"/convention/$metadata#Employees(1)/NewFriends/$delta',     
                    'value':[{ 'Id':3, 'Age':35, '@NS.Test':1}]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            requestForPatch.Headers.Add("OData-Version", "4.01");
            requestForPatch.Content = new StringContent(content);

            requestForPatch.Content.Headers.ContentType= MediaTypeWithQualityHeaderValue.Parse("application/json");
            
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            //Act & Assert
            var expected = "$delta\",\"value\":[{\"@NS.Test\":1,\"Id\":3,\"Name\":null,\"Age\":35}]}";

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains(expected, json.ToString());
            }
        }

        [Fact]
        public async Task PatchEmployee_WithFailedAdds_Friends()
        {
            //Arrange
            string requestUri = this.BaseAddress + "/convention/Employees(1)/NewFriends";
            var content = @"{'@odata.context':'" + this.BaseAddress + @"/convention/$metadata#Employees(1)/NewFriends/$delta',     
                    'value':[{ 'Id':3, 'Age':3, '@NS.Test':1}]
                     }";
           
            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            requestForPatch.Headers.Add("OData-Version", "4.01");

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            //Act & Assert
            var expected = "$delta\",\"value\":[{\"@NS.Test\":1,\"Id\":3,\"Name\":null,\"Age\":3}]}";

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains(expected, json.ToString());
            }
        }

        [Fact]
        public async Task PatchEmployee_WithFailedDeletes_Friends()
        {
            //Arrange            
            string requestUri = this.BaseAddress + "/convention/Employees(2)/NewFriends";
            var content = @"{'@odata.context':'" + this.BaseAddress + @"/convention/$metadata#Employees(1)/NewFriends/$delta',     
                    'value':[{ '@odata.removed' : {'reason':'changed'}, 'Id':2, '@NS.Test':1}]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            requestForPatch.Headers.Add("OData-Version", "4.01");

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            //Act & Assert
            var expected = "{\"@context\":\"" + this.BaseAddress + "/convention/$metadata#NewFriends/$delta\",\"value\":[{\"@NS.Test\":1,\"@Core.DataModificationException\":{\"@type\":\"#Org.OData.Core.V1.DataModificationExceptionType\",\"FailedOperation\":\"Delete\",\"ResponseCode\":0,\"MessageType\":{\"Code\":null,\"Message\":\"The method or operation is not implemented.\",\"Severity\":null,\"Target\":null,\"Details\":null}},\"Id\":2,\"Name\":null,\"Age\":15}]}";

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains("$delta", json);
                Assert.Contains(expected, json.ToString());
            }
        }

        [Fact]
        public async Task PatchEmployee_WithFailedOperation_WithAnnotations()
        {
            //Arrange            
            string requestUri = this.BaseAddress + "/convention/Employees(2)/NewFriends";
            var content = @"{'@odata.context':'" + this.BaseAddress + @"/convention/$metadata#Employees(2)/NewFriends/$delta',     
                    'value':[{ '@odata.removed' : {'reason':'changed'}, 'Id':2, '@Core.ContentID':3, '@NS.Test2':'testing'}]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            requestForPatch.Headers.Add("OData-Version", "4.01");

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            //Act & Assert
            var expected = "{\"@context\":\"" + this.BaseAddress + "/convention/$metadata#NewFriends/$delta\",\"value\":[{\"@NS.Test2\":\"testing\",\"@Core.ContentID\":3,\"@Core.DataModificationException\":{\"@type\":\"#Org.OData.Core.V1.DataModificationExceptionType\",\"FailedOperation\":\"Delete\",\"ResponseCode\":0,\"MessageType\":{\"Code\":null,\"Message\":\"The method or operation is not implemented.\",\"Severity\":null,\"Target\":null,\"Details\":null}},\"Id\":2,\"Name\":null,\"Age\":15}]}";

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var str = json.ToString();
                Assert.Contains("$delta",str);                
                Assert.Contains("NS.Test2", str);
                Assert.Contains("Core.DataModificationException", str);
                Assert.Contains(expected, str);
            }
        }

        [Fact]
        public async Task PatchUntypedEmployee_WithAdds_Friends_Untyped()
        {
            //Arrange
            string requestUri = this.BaseAddress + "/convention/UnTypedEmployees";

            string content = @"{'@odata.context':'" + this.BaseAddress + @"/convention/$metadata#UnTypedEmployees/$delta',
                    'value':[{ 'ID':1,'Name':'Employee1',
                            'UnTypedFriends@odata.delta':[{'Id':1,'Name':'Friend1'},{'Id':2,'Name':'Friend2'}]
                                },
                            { 'ID':2,'Name':'Employee2',
                            'UnTypedFriends@odata.delta':[{'Id':3,'Name':'Friend3'},{'Id':4,'Name':'Friend4'}]
                                }]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            requestForPatch.Headers.Add("OData-Version", "4.01");

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            //Act & Assert
            var expected = "/convention/$metadata#UnTypedEmployees/$delta\",\"value\":[{\"ID\":1,\"Name\":\"Employee1\",\"UnTypedFriends@delta\":" +
                "[{\"Id\":1,\"Name\":\"Friend1\",\"Age\":0},{\"Id\":2,\"Name\":\"Friend2\",\"Age\":0}]},{\"ID\":2,\"Name\":\"Employee2\",\"UnTypedFriends@delta\":" +
                "[{\"Id\":3,\"Name\":\"Friend3\",\"Age\":0},{\"Id\":4,\"Name\":\"Friend4\",\"Age\":0}]}]}";

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains(expected, json.ToString());
            }
        }

        [Fact]
        public async Task PatchEmployee_WithAdds_Friends_WithNested_Untyped()
        {
            //Arrange
            string requestUri = this.BaseAddress + "/convention/Employees(1)/UnTypedFriends";
            var content = @"{'@odata.context':'" + this.BaseAddress + @"/convention/$metadata#Employees(1)/UnTypedFriends/$delta',     
                    'value':[{ 'Id':2, 'Name': 'Friend007', 'Age':35,'Address@odata.delta':{'Id':1, 'Street' : 'Abc 123'}, '@NS.Test':1}]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
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
            var content = @"{'@odata.context':'" + this.BaseAddress + @"/convention/$metadata#Employees(2)/UnTypedFriends/$delta',     
                    'value':[{ 'Id':2, 'Age':35, '@NS.Test':1}]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
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
            var content = @"{'@odata.context':'" + this.BaseAddress + @"/convention/$metadata#Employees(3)/UnTypedFriends/$delta',     
                    'value':[{ 'Id':3, 'Age':3, '@NS.Test':1}]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                json.ToString().Contains("$deletedEntity");
            }
        }

        [Fact]
        public async Task PatchEmployee_WithFailedDeletes_Friends_Untyped()
        {
            //Arrange            
            string requestUri = this.BaseAddress + "/convention/Employees(3)/UnTypedFriends";
            var content = @"{'@odata.context':'" + this.BaseAddress + @"/convention/$metadata#Employees(3)/UnTypedFriends/$delta',     
                    'value':[{ '@odata.removed' : {'reason':'changed'}, 'Id':5, '@NS.Test':1}]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
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
            var content = @"{'@odata.context':'" + this.BaseAddress + @"/convention/$metadata#Employees(3)/UnTypedFriends/$delta',     
                    'value':[{ '@odata.removed' : {'reason':'changed'}, 'Id':5, '@Core.ContentID':3, '@NS.Test2':'testing'}]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var str = json.ToString();
                Assert.Contains("$delta", str);                
                Assert.Contains("NS.Test2", str);
                Assert.Contains("Core.DataModificationException", str);
                Assert.Contains("Core.ContentID", str);                
            }
        }

        [Fact]
        public async Task PatchEmployee_WithUnchanged_Employee()
        {
            //Arrange
            string requestUri = this.BaseAddress + "/convention/Employees";

            var content = @"{'@odata.context':'" + this.BaseAddress + @"/convention/$metadata#Employees/$delta',
                    'value':[{ '@odata.type': '#Microsoft.Test.E2E.AspNet.OData.BulkOperation.Employee', 'ID':1,'Name':'Name1',
                            'Friends@odata.delta':[{'Id':1,'Name':'Test0','Age':33},{'Id':2,'Name':'Test1'}]
                                }]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            requestForPatch.Headers.Add("OData-Version", "4.01");

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;

            //Act & Assert
            var expected = "\"value\":[{\"ID\":1,\"Name\":\"Name1\",\"SkillSet\":[\"CSharp\",\"Sql\"],\"Gender\":\"Female\",\"AccessLevel\":\"Execute\",\"FavoriteSports\":{\"Sport\":\"Football\"}," +
                "\"Friends@delta\":[{\"Id\":1,\"Name\":\"Test0\",\"Age\":33}," +
                "{\"Id\":2,\"Name\":\"Test1\",\"Age\":0}]}]}";

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains(expected, json.ToString());

                // Navigation properties absent from the payload are not serialized in the response.
                Assert.DoesNotContain("NewFriends@delta", json.ToString());
                Assert.DoesNotContain("UntypedFriends@delta", json.ToString());
            }
        }

        [Fact]
        public async Task PatchEmployee_WithUpdates_Employees()
        {
            //Arrange
            string requestUri = this.BaseAddress + "/convention/Employees";

            var content = @"{'@odata.context':'"+ this.BaseAddress + @"/convention/$metadata#Employees/$delta',     
                    'value':[{ '@odata.type': '#Microsoft.Test.E2E.AspNet.OData.BulkOperation.Employee', 'ID':1,'Name':'Employee1',
                            'Friends@odata.delta':[{'Id':1,'Name':'Friend1',
                            'Orders@odata.delta' :[{'Id':1,'Price': 10}, {'Id':2,'Price': 20} ] },{'Id':2,'Name':'Friend2'}]
                                },
                            {  '@odata.type': '#Microsoft.Test.E2E.AspNet.OData.BulkOperation.Employee', 'ID':2,'Name':'Employee2',
                            'Friends@odata.delta':[{'Id':3,'Name':'Friend3',
                            'Orders@odata.delta' :[{'Id':3,'Price': 30}, {'Id':4,'Price': 40} ]},{'Id':4,'Name':'Friend4'}]
                                }]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            requestForPatch.Headers.Add("OData-Version", "4.01");

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;

            //Act & Assert
            var expected = "\"value\":[{\"ID\":1,\"Name\":\"Employee1\",\"SkillSet\":[\"CSharp\",\"Sql\"],\"Gender\":\"Female\",\"AccessLevel\":\"Execute\",\"FavoriteSports\":{\"Sport\":\"Football\"}," +
                "\"Friends@delta\":[{\"Id\":1,\"Name\":\"Friend1\",\"Age\":33,\"Orders@delta\":[{\"Id\":1,\"Price\":10},{\"Id\":2,\"Price\":20}]}," +
                "{\"Id\":2,\"Name\":\"Friend2\",\"Age\":0}]},{\"ID\":2,\"Name\":\"Employee2\",\"SkillSet\":[],\"Gender\":\"Female\",\"AccessLevel\":\"Read\",\"FavoriteSports\":null," +
                "\"Friends@delta\":[{\"Id\":3,\"Name\":\"Friend3\",\"Age\":0,\"Orders@delta\":[{\"Id\":3,\"Price\":30},{\"Id\":4,\"Price\":40}]},{\"Id\":4,\"Name\":\"Friend4\",\"Age\":0}]}]}";

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains(expected, json.ToString());
                Assert.DoesNotContain("NewFriends@delta", json.ToString());
                Assert.DoesNotContain("UntypedFriends@delta", json.ToString());
            }

            //Act & Assert
            requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = response.Content.ReadAsStringAsync().Result;
                
                Assert.Contains("Friend1", json.ToString());
                Assert.Contains("Friend2", json.ToString());
            }

            //Act & Assert
            requestUri = this.BaseAddress + "/convention/Employees(2)?$expand=Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = response.Content.ReadAsStringAsync().Result;

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

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            
            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;

            //Act & Assert
            var expected = "/convention/$metadata#Employees/$entity\",\"ID\":1,\"Name\":\"Sql\"," +
                "\"SkillSet\":[\"CSharp\",\"Sql\"],\"Gender\":\"Female\",\"AccessLevel\":\"Execute\",\"FavoriteSports\":{\"Sport\":\"Football\"}}";

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains(expected, json.ToString());
            }

            //Act & Assert
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
        public async Task PatchEmployee_WithODataBind()
        {
            //Arrange
            string requestUri = this.BaseAddress + "/convention/Employees(1)?$expand=Friends";

            var content = @"{
                    'Name':'Bind1'  ,
                    'Friends@odata.bind':['Friends(3)']
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;

            //Act & Assert
            var expected = "/convention/$metadata#Employees(Friends())/$entity\",\"ID\":1,\"Name\":\"Bind1\"," +
                "\"SkillSet\":[\"CSharp\",\"Sql\"],\"Gender\":\"Female\",\"AccessLevel\":\"Execute\",\"FavoriteSports\":{\"Sport\":\"Football\"},\"Friends\":[{\"Id\":3,\"Name\":null,\"Age\":0}]}";

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains(expected, json.ToString());
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

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Act & Assert
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
        public async Task PatchEmployee_WithMultipleFriendUpdatesAndOneDelete()
        {
            //Arrange
            string requestUri = this.BaseAddress + "/convention/Employees(1)";

            var content = @"{
                    'Name':'Sql'  ,
                    'Friends@odata.delta':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{'Id':1,'Name':'Test_1'},{'Id':2,'Name':'Test3'},{'Id':3,'Name':'Test4'}]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Act & Assert
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
        public async Task PatchEmployee_WithMultipleFriendUpdatesAndMultipleDelete()
        {
            //Arrange
            string requestUri = this.BaseAddress + "/convention/Employees(1)";

            var content = @"{
                    'Name':'Sql'  ,
                    'Friends@odata.delta':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{'Id':1,'Name':'Test_1'},{'Id':2,'Name':'Test3'},{'Id':3,'Name':'Test4'},{ '@odata.removed' : {'reason':'changed'}, 'Id':1}]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Act & Assert
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

        [Fact]
        public async Task PatchCompanies_WithUpdates_ODataId()
        {
            //Arrange
            string requestUri = this.BaseAddress + "/convention/Companies";

            var content = @"{'@odata.context':'" + this.BaseAddress + @"/convention/$metadata#Companies/$delta',
                    'value':[{ '@odata.type': '#Microsoft.Test.E2E.AspNet.OData.BulkOperation.Company', 'Id':1,'Name':'Company01',
                            'OverdueOrders@odata.delta':[{'@odata.id':'Employees(1)/NewFriends(1)/NewOrders(1)', 'Quantity': 9}]
                                }]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            requestForPatch.Headers.Add("OData-Version", "4.01");

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;

            //Act & Assert
            var expected = "/convention/$metadata#Companies/$delta\",\"value\":[{\"Id\":1,\"Name\":\"Company01\",\"OverdueOrders@delta\":" +
                "[{\"Id\":1,\"Price\":101,\"Quantity\":9}]}]}";

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains(expected, json.ToString());
            }

            requestUri = this.BaseAddress + "/convention/Companies(1)/OverdueOrders";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                //Assert.Contains(expected, json.ToString());
            }
        }

        [Fact]
        public async Task PatchCompanies_WithUpdates_ODataId_WithCast()
        {
            //Arrange
            string requestUri = this.BaseAddress + "/convention/Companies";

            var content = @"{'@odata.context':'" + this.BaseAddress + @"/convention/$metadata#Companies/$delta',
                    'value':[{ '@odata.type': '#Microsoft.Test.E2E.AspNet.OData.BulkOperation.Company', 'Id':2,'Name':'Company03',
                            'MyOverdueOrders@odata.delta':[{'@odata.id':'Employees(2)/NewFriends(2)/Microsoft.Test.E2E.AspNet.OData.BulkOperation.MyNewFriend/MyNewOrders(2)', 'Quantity': 9}]
                                }]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            requestForPatch.Headers.Add("OData-Version", "4.01");

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;

            //Act & Assert
            var expected = "$delta\",\"value\":[{\"Id\":2,\"Name\":\"Company03\",\"MyOverdueOrders@delta\":" +
                "[{\"Id\":2,\"Price\":444,\"Quantity\":9}]}]}";

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains(expected, json.ToString());
            }
        }

        [Fact]
        public async Task PatchCompanies_WithMultipleUpdates_ODataId_WithCast()
        {
            //Arrange
            string requestUri = this.BaseAddress + "/convention/Companies";

            var content = @"{'@odata.context':'" + this.BaseAddress + @"/convention/$metadata#Companies/$delta',
                    'value':[{ '@odata.type': '#Microsoft.Test.E2E.AspNet.OData.BulkOperation.Company', 'Id':2,'Name':'Company03',
                            'MyOverdueOrders@odata.delta':[{'Id': 1, 'Price': 10, 'Quantity': 5}, {'@odata.id':'Employees(2)/NewFriends(2)/Microsoft.Test.E2E.AspNet.OData.BulkOperation.MyNewFriend/MyNewOrders(2)', 'Quantity': 9}]
                                }]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            requestForPatch.Headers.Add("OData-Version", "4.01");

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;

            //Act & Assert
            var expected = "\"value\":[{\"Id\":2,\"Name\":\"Company03\",\"MyOverdueOrders@delta\":" +
                "[{\"Id\":1,\"Price\":10,\"Quantity\":5},{\"Id\":2,\"Price\":444,\"Quantity\":9}]}]}";

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains(expected, json.ToString());
            }
        }

        [Fact]
        public async Task PatchCompanies_WithMultipleReplace()
        {
            //Arrange
            string requestUri = this.BaseAddress + "/convention/Companies";

            var content = @"{'@odata.context':'" + this.BaseAddress + @"/convention/$metadata#Companies/$delta',
                    'value':[{ '@odata.type': '#Microsoft.Test.E2E.AspNet.OData.BulkOperation.Company', 'Id':2,'Name':'Company03',
                            'MyOverdueOrders':[{'Id': 1, 'Price': 10, 'Quantity': 5},{'Id': 2, 'Price': 20, 'Quantity': 10}]
                                }]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            requestForPatch.Headers.Add("OData-Version", "4.01");

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Act & Assert
            var expected = "\"value\":[{\"Id\":1,\"Price\":10,\"Quantity\":5},{\"Id\":2,\"Price\":20,\"Quantity\":10}]}";

            requestUri = this.BaseAddress + "/convention/Companies(2)/MyOverdueOrders";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains(expected, json.ToString());
            }
        }

        [Fact]
        public async Task PatchUntypedEmployee_WithOdataId()
        {
            //Arrange
            string requestUri = this.BaseAddress + "/convention/UnTypedEmployees";
            
            var content = @"{'@odata.context':'" + this.BaseAddress + @"/convention/$metadata#UnTypedEmployees/$delta',     
                    'value':[{ 'ID':1,'Name':'Employeeabcd',
                            'UnTypedFriends@odata.delta':[{'@odata.id':'UnTypedEmployees(1)/UnTypedFriends(1)', 'Name':'abcd'}]
                                }]
                     }";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            requestForPatch.Headers.Add("OData-Version", "4.01");

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;
            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            //Act & Assert
            var expected = "/convention/$metadata#UnTypedEmployees/$delta\",\"value\":[{\"ID\":1,\"Name\":\"Employeeabcd\"," +
                "\"UnTypedFriends@delta\":[{\"Id\":0,\"Name\":\"abcd\",\"Age\":0}]}]}";

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains(expected, json.ToString());
            }
        }

        #endregion

        #region Post

        [Theory]
        [InlineData("/convention/Employees")]
        [InlineData("/convention/Employees?$expand=Friends,NewFriends($expand=NewOrders)")]
        public async Task PostEmployee_WithNavigationProperties_OneMultiLevelNesting(string uri)
        {
            //Arrange

            string requestUri = this.BaseAddress + uri;

            var content = @"{
                    'ID':11,
                    'Name':'SqlUD',
                    'Friends':[{ 'Id':1001, 'Name' : 'Friend 1001', 'Age': 31},{ 'Id':1002, 'Name' : 'Friend 1002', 'Age': 32},{ 'Id':1003, 'Name' : 'Friend 1003', 'Age': 33}],
                    'NewFriends':[{ 'Id':2001, 'Name' : 'NewFriend 2001', 'Age': 21},{ 'Id':2002, 'Name' : 'NewFriend 2002', 'Age': 22, 'NewOrders':[{'Id': 101, 'Price': 45,'Quantity': 10}]}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("POST"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            var expectedFriends = "Friends\":[{\"Id\":1001,\"Name\":\"Friend 1001\",\"Age\":31},{\"Id\":1002,\"Name\":\"Friend 1002\",\"Age\":32},{\"Id\":1003,\"Name\":\"Friend 1003\",\"Age\":33}]";
            var expectedNewFriends = "NewFriends\":[{\"Id\":2001,\"Name\":\"NewFriend 2001\",\"Age\":21,\"NewOrders\":[]},{\"Id\":2002,\"Name\":\"NewFriend 2002\",\"Age\":22,\"NewOrders\":[{\"Id\":101,\"Price\":45,\"Quantity\":10}]}]";

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Contains("SqlUD", json);
                Assert.Contains(expectedFriends, json);
                Assert.Contains(expectedNewFriends, json);
            }
        }

        [Theory]
        [InlineData("/convention/Employees")]
        [InlineData("/convention/Employees?$expand=Friends($expand=Orders),NewFriends($expand=NewOrders)")]
        public async Task PostEmployee_WithNavigationProperties_TwoMultiLevelNesting(string uri)
        {
            //Arrange

            string requestUri = this.BaseAddress + uri;

            var content = @"{
                    'ID':11,
                    'Name':'SqlUD',
                    'Friends':[{ 'Id':1001, 'Name' : 'Friend 1001', 'Age': 31},{ 'Id':1002, 'Name' : 'Friend 1002', 'Age': 32},{ 'Id':1003, 'Name' : 'Friend 1003', 'Age': 33, 'Orders':[{'Id': 101, 'Price': 45}]}],
                    'NewFriends':[{ 'Id':2001, 'Name' : 'NewFriend 2001', 'Age': 21},{ 'Id':2002, 'Name' : 'NewFriend 2002', 'Age': 22, 'NewOrders':[{'Id': 101, 'Price': 45,'Quantity': 10}]}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("POST"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            var expectedFriends = "Friends\":[{\"Id\":1001,\"Name\":\"Friend 1001\",\"Age\":31,\"Orders\":[]},{\"Id\":1002,\"Name\":\"Friend 1002\",\"Age\":32,\"Orders\":[]},{\"Id\":1003,\"Name\":\"Friend 1003\",\"Age\":33,\"Orders\":[{\"Id\":101,\"Price\":45}]}]";
            var expectedNewFriends = "NewFriends\":[{\"Id\":2001,\"Name\":\"NewFriend 2001\",\"Age\":21,\"NewOrders\":[]},{\"Id\":2002,\"Name\":\"NewFriend 2002\",\"Age\":22,\"NewOrders\":[{\"Id\":101,\"Price\":45,\"Quantity\":10}]}]";

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Contains("SqlUD", json);
                Assert.Contains(expectedFriends, json);
                Assert.Contains(expectedNewFriends, json);
            }
        }

        [Theory]
        [InlineData("/convention/Employees")]
        [InlineData("/convention/Employees?$expand=Friends($expand=Orders($expand=OrderLines)),NewFriends($expand=NewOrders)")]
        public async Task PostEmployee_WithNavigationProperties_TwoUnevenMultiLevelNesting(string uri)
        {
            //Arrange

            string requestUri = this.BaseAddress + uri;

            var content = @"{
                    'ID':11,
                    'Name':'SqlUD',
                    'Friends':[{ 'Id':1001, 'Name' : 'Friend 1001', 'Age': 31},{ 'Id':1002, 'Name' : 'Friend 1002', 'Age': 32, 'Orders':[{'Id': 101, 'Price': 45}]},{ 'Id':1003, 'Name' : 'Friend 1003', 'Age': 33, 'Orders':[{'Id': 102, 'Price': 55, 'OrderLines':[{'Id': 1001, 'Price': 5}]}]}],
                    'NewFriends':[{ 'Id':2001, 'Name' : 'NewFriend 2001', 'Age': 21},{ 'Id':2002, 'Name' : 'NewFriend 2002', 'Age': 22, 'NewOrders':[{'Id': 101, 'Price': 45,'Quantity': 10}]}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("POST"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            var expectedFriends = "Friends\":[{\"Id\":1001,\"Name\":\"Friend 1001\",\"Age\":31,\"Orders\":[]},{\"Id\":1002,\"Name\":\"Friend 1002\",\"Age\":32,\"Orders\":[{\"Id\":101,\"Price\":45,\"OrderLines\":[]}]},{\"Id\":1003,\"Name\":\"Friend 1003\",\"Age\":33,\"Orders\":[{\"Id\":102,\"Price\":55,\"OrderLines\":[{\"Id\":1001,\"Price\":5}]}]}]";
            var expectedNewFriends = "NewFriends\":[{\"Id\":2001,\"Name\":\"NewFriend 2001\",\"Age\":21,\"NewOrders\":[]},{\"Id\":2002,\"Name\":\"NewFriend 2002\",\"Age\":22,\"NewOrders\":[{\"Id\":101,\"Price\":45,\"Quantity\":10}]}]";

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Contains("SqlUD", json);
                Assert.Contains(expectedFriends, json);
                Assert.Contains(expectedNewFriends, json);
            }
        }

        [Theory]
        [InlineData("/convention/Companies")]
        [InlineData("/convention/Companies?$expand=OverdueOrders")]
        public async Task PostCompany_WithODataId(string uri)
        {
            //Arrange

            string requestUri = this.BaseAddress + uri;

            var content = @"{'Id':3,'Name':'Company03',
                            'OverdueOrders':[{'@odata.id':'Employees(1)/NewFriends(1)/NewOrders(1)'}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("POST"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            var expected = "OverdueOrders\":[{\"Id\":1,\"Price\":101,\"Quantity\":0}]";

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Contains(expected, json);
            }
        }

        [Theory]
        [InlineData("/convention/Companies")]
        [InlineData("/convention/Companies?$expand=OverdueOrders")]
        public async Task PostCompany_WithODataId_AndWithout(string uri)
        {
            //Arrange

            string requestUri = this.BaseAddress + uri;

            var content = @"{'Id':4,'Name':'Company04',
                            'OverdueOrders':[{'@odata.id':'Employees(1)/NewFriends(1)/NewOrders(1)'},{'Id': 5, 'Price': 30, 'Quantity': 5}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("POST"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            var expected = "OverdueOrders\":[{\"Id\":1,\"Price\":101,\"Quantity\":0},{\"Id\":5,\"Price\":30,\"Quantity\":5}]";

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Contains(expected, json);
            }
        }

        [Theory]
        [InlineData("/convention/Employees")]
        [InlineData("/convention/Employees?$expand=Friends")]
        public async Task PostEmployee_WithCreateFriends(string uri)
        {
            //Arrange

            string requestUri = this.BaseAddress + uri;

            var content = @"{
                    'ID':88,
                    'Name':'SqlUD',
                    'Friends':[{ 'Id':1001, 'Name' : 'Friend 1001', 'Age': 31},{ 'Id':1002, 'Name' : 'Friend 1002', 'Age': 32},{ 'Id':1003, 'Name' : 'Friend 1003', 'Age': 33}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("POST"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            var expected = "Friends\":[{\"Id\":1001,\"Name\":\"Friend 1001\",\"Age\":31},{\"Id\":1002,\"Name\":\"Friend 1002\",\"Age\":32},{\"Id\":1003,\"Name\":\"Friend 1003\",\"Age\":33}]";

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Contains("SqlUD", json);
                Assert.Contains(expected, json);
            }
        }

        [Fact]
        public async Task PostEmployee_WithCreateFriendsFullMetadata()
        {
            //Arrange

            string requestUri = this.BaseAddress + "/convention/Employees?$format=application/json;odata.metadata=full";

            string content = @"{
                    'Name':'SqlUD',
                    'Friends':[{ 'Id':1001, 'Name' : 'Friend 1001', 'Age': 31},{ 'Id':1002, 'Name' : 'Friend 1002', 'Age': 32},{ 'Id':1003, 'Name' : 'Friend 1003', 'Age': 33}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("POST"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            string friendsNavigationLink = "Friends@odata.navigationLink";
            string newFriendsNavigationLink = "NewFriends@odata.navigationLink";
            string untypedFriendsNavigationLink = "UnTypedFriends@odata.navigationLink";

            string expected = "Friends\":[{\"@odata.type\":\"#Microsoft.Test.E2E.AspNet.OData.BulkOperation.Friend\"";

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Contains("SqlUD", json);
                Assert.Contains(expected, json);
                Assert.Contains(friendsNavigationLink, json);
                Assert.Contains(newFriendsNavigationLink, json);
                Assert.Contains(untypedFriendsNavigationLink, json);
            }
        }

        [Fact]
        public async Task PostEmployee_WithFullMetadata()
        {
            //Arrange

            string requestUri = this.BaseAddress + "/convention/Employees?$format=application/json;odata.metadata=full";

            var content = @"{
                    'Name':'SqlUD'
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("POST"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            string friendsNavigationLink = "Friends@odata.navigationLink";
            string newFriendsNavigationLink = "NewFriends@odata.navigationLink";
            string untypedFriendsNavigationLink = "UnTypedFriends@odata.navigationLink";

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Contains("SqlUD", json);
                Assert.Contains(friendsNavigationLink, json);
                Assert.Contains(newFriendsNavigationLink, json);
                Assert.Contains(untypedFriendsNavigationLink, json);
            }
        }

#endregion

        #region Full Metadata

        [Fact]
        public async Task GetEmployee_WithFullMetadata()
        {
            //Arrange

            string requestUri = this.BaseAddress + "/convention/Employees(1)?$format=application/json;odata.metadata=full";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("GET"), requestUri);

            string friendsNavigationLink = "Friends@odata.navigationLink";
            string newFriendsNavigationLink = "NewFriends@odata.navigationLink";
            string untypedFriendsNavigationLink = "UnTypedFriends@odata.navigationLink";

            string notexpected = "Friends\":[{\"@odata.type\":\"#Microsoft.Test.E2E.AspNet.OData.BulkOperation.Friend\"";

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.DoesNotContain(notexpected, json);
                Assert.Contains(friendsNavigationLink, json);
                Assert.Contains(newFriendsNavigationLink, json);
                Assert.Contains(untypedFriendsNavigationLink, json);
            }
        }

        #endregion
    }
}