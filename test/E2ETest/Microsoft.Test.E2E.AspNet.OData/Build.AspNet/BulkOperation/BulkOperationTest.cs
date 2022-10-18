//-----------------------------------------------------------------------------
// <copyright file="BulkOperationTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.BulkOperation
{
    public class BulkOperationTest : WebHostTestBase
    {
        public BulkOperationTest(WebHostTestFixture fixture)
            : base(fixture)
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

        [Fact]
        public async Task PatchEmployee_WithNestedFriends_WithNestedOrders_IsSerializedSuccessfully()
        {
            //Arrange
            string requestUri = this.BaseAddress + "/convention/Employees";
            var content = @"{
                '@odata.context':'http://host/service/$metadata#Employees/$delta',
                'value':[
                    {'ID':1,'Name':'Employee1','Friends@odata.delta':[{'Id':1,'Name':'Friend1','Orders@odata.delta':[{'Id':1,'Price': 10},{'Id':2,'Price':20} ]},{'Id':2,'Name':'Friend2'}]},
                    {'ID':2,'Name':'Employee2','Friends@odata.delta':[{'Id':3,'Name':'Friend3','Orders@odata.delta' :[{'Id':3,'Price': 30}, {'Id':4,'Price': 40} ]},{'Id':4,'Name':'Friend4'}]}
                ]}";

            var requestForPatch = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            
            string expectedResponse = "{" +
                "\"@context\":\""+ this.BaseAddress + "/convention/$metadata#Employees/$delta\"," +
                "\"value\":[" +
                "{\"ID\":1,\"Name\":\"Employee1\",\"SkillSet\":[],\"Gender\":\"0\",\"AccessLevel\":\"0\",\"FavoriteSports\":null,\"Friends@delta\":[{\"Id\":1,\"Name\":\"Friend1\",\"Age\":0,\"Orders@delta\":[{\"Id\":1,\"Price\":10},{\"Id\":2,\"Price\":20}]},{\"Id\":2,\"Name\":\"Friend2\",\"Age\":0}]}," +
                "{\"ID\":2,\"Name\":\"Employee2\",\"SkillSet\":[],\"Gender\":\"0\",\"AccessLevel\":\"0\",\"FavoriteSports\":null,\"Friends@delta\":[{\"Id\":3,\"Name\":\"Friend3\",\"Age\":0,\"Orders@delta\":[{\"Id\":3,\"Price\":30},{\"Id\":4,\"Price\":40}]},{\"Id\":4,\"Name\":\"Friend4\",\"Age\":0}]}]}";
            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPatch.Content = stringContent;

            // Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPatch))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(expectedResponse.ToString().ToLower(), json.ToString().ToLower());
                Assert.Contains("Employee1", json);
                Assert.Contains("Employee2", json);
            }
        }

        [Fact]
        public async Task PatchEmployee_WithDeletedAndODataId_IsSerializedSuccessfully()
        {
            //Arrange
            string requestUri = this.BaseAddress + "/convention/Employees";

            var content = @"{
                '@odata.context':'http://host/service/$metadata#Employees/$delta',
                'value':[
                    {'ID':1,'Name':'Employee1','Friends@odata.delta':[{'@odata.removed':{'reason':'changed'},'Id':1}]},
                    {'ID':2,'Name':'Employee2','Friends@odata.delta':[{'@odata.id':'Friends(1)'}]}
                ]}";

            string expectedResponse = "{\"@context\":\""+ this.BaseAddress + "/convention/$metadata#Employees/$delta\",\"value\":[{\"ID\":1,\"Name\":\"Employee1\",\"SkillSet\":[],\"Gender\":\"0\",\"AccessLevel\":\"0\",\"FavoriteSports\":null,\"Friends@delta\":[{\"@removed\":{\"reason\":\"changed\"},\"@id\":\"http://host/service/Friends(1)\",\"Id\":1,\"Name\":null,\"Age\":0}]},{\"ID\":2,\"Name\":\"Employee2\",\"SkillSet\":[],\"Gender\":\"0\",\"AccessLevel\":\"0\",\"FavoriteSports\":null,\"Friends@delta\":[{\"Id\":0,\"Name\":null,\"Age\":0}]}]}";

            var requestForUpdate = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            
            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForUpdate.Content = stringContent;

            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForUpdate))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var json = response.Content.ReadAsStringAsync().Result;
                Assert.Equal(expectedResponse.ToString().ToLower(), json.ToString().ToLower());
            }
        }
    }
}
