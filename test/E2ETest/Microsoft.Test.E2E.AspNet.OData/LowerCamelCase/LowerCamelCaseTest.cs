//-----------------------------------------------------------------------------
// <copyright file="LowerCamelCaseTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.LowerCamelCase
{
    public class LowerCamelCaseTest : WebHostTestBase
    {
        public LowerCamelCaseTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(EmployeesController), typeof(MetadataController) };
            configuration.AddControllers(controllers);

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select().SkipToken();
            configuration.MapODataServiceRoute("OData", "odata", LowerCamelCaseEdmModel.GetConventionModel(configuration));
            configuration.EnsureInitialized();
        }

        public static TheoryDataSet<string> MediaTypes
        {
            get
            {
                TheoryDataSet<string> data = new TheoryDataSet<string>();
                data.Add(Uri.EscapeDataString("json"));
                data.Add(Uri.EscapeDataString("application/json"));
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=none"));
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=minimal"));
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=full"));
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=none;odata.streaming=true"));
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=none;odata.streaming=false"));
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=minimal;odata.streaming=true"));
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=minimal;odata.streaming=false"));
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=full;odata.streaming=true"));
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=full;odata.streaming=false"));

                data.Add(Uri.EscapeDataString("application/json;odata.streaming=true;odata.metadata=none"));
                data.Add(Uri.EscapeDataString("application/json;odata.streaming=false;odata.metadata=none"));
                data.Add(Uri.EscapeDataString("application/json;odata.streaming=true;odata.metadata=minimal"));
                data.Add(Uri.EscapeDataString("application/json;odata.streaming=false;odata.metadata=minimal"));
                data.Add(Uri.EscapeDataString("application/json;odata.streaming=true;odata.metadata=full"));
                data.Add(Uri.EscapeDataString("application/json;odata.streaming=false;odata.metadata=full"));

                return data;
            }
        }

        [Fact]
        public async Task ModelBuilderTest()
        {
            string requestUri = string.Format("{0}/odata/$metadata", this.BaseAddress);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var stream = await response.Content.ReadAsStreamAsync();

            IODataResponseMessage message = new ODataMessageWrapper(stream, response.Content.Headers);
            var reader = new ODataMessageReader(message);
            var edmModel = reader.ReadMetadataDocument();

            var container = edmModel.EntityContainer;
            Assert.Equal("Container", container.Name);

            var employeeType = edmModel.SchemaElements.Single(e => e.Name == "Employee") as IEdmEntityType;
            employeeType.Properties().All(p => this.IsCamelCase(p.Name));

            var managerType = edmModel.SchemaElements.Single(e => e.Name == "Manager") as IEdmEntityType;
            Assert.Equal(7, managerType.Properties().Count());
            managerType.Properties().All(p => this.IsCamelCase(p.Name));

            var addressType = edmModel.SchemaElements.Single(e => e.Name == "Address") as IEdmComplexType;
            addressType.Properties().All(p => this.IsCamelCase(p.Name));
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryEntitySet(string format)
        {
            string requestUri = this.BaseAddress + "/odata/Employees?$expand=Microsoft.Test.E2E.AspNet.OData.LowerCamelCase.Manager/directReports($levels=2)&$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsObject<JObject>();
            var results = json.GetValue("value") as JArray;
            Assert.Equal<int>(10, results.Count);
            var empolyee = results[0] as JObject;
            AssertPropertyNamesAreCamelCase(empolyee);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryEntitySetWithDerivedType(string format)
        {
            string requestUri = this.BaseAddress + "/odata/Employees/Microsoft.Test.E2E.AspNet.OData.LowerCamelCase.Manager?$expand=directReports&$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsObject<JObject>();
            var results = json.GetValue("value") as JArray;
            Assert.Equal<int>(5, results.Count);

            var manager = results.Single(e => e["id"].Value<int>() == 6) as JObject;
            AssertPropertyNamesAreCamelCase(manager);

            var address = manager["address"] as JObject;
            AssertPropertyNamesAreCamelCase(address);

            var directReports = manager["directReports"] as JArray;
            Assert.Single(directReports);

            var employee = directReports[0] as JObject;
            AssertPropertyNamesAreCamelCase(employee);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryEntity(string format)
        {
            string requestUri = this.BaseAddress + "/odata/Employees(1)?$expand=manager($levels=max;$select=id,name)&$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsObject<JObject>();
            Assert.True(8 == (int)result["manager"]["manager"]["manager"]["id"]);
            AssertPropertyNamesAreCamelCase(result);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryEntityWithDollarLevelsEqualZero(string format)
        {
            string requestUri = this.BaseAddress + "/odata/Employees(1)?$expand=manager($levels=0)&$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsObject<JObject>();
            Assert.Null(result["manager"]);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryEntityWithDollarLevelsEqualNegativeOne(string format)
        {
            string requestUri = this.BaseAddress + "/odata/Employees(1)?$expand=manager($levels=-1)&$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            Assert.Contains("Levels option must be a non-negative integer or 'max', it is set to '-1' instead.", result);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryEntityWithDollarLevelsGreaterThanTheMaxExpansionDepth(string format)
        {
            string requestUri = this.BaseAddress + "/odata/Employees(1)?$expand=manager($levels=4)&$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            Assert.Contains("The request includes a $expand path which is " +
                "too deep. The maximum depth allowed is 3. To increase the limit, set the 'MaxExpansionDepth' property " +
                "on EnableQueryAttribute or ODataValidationSettings, or set the 'MaxDepth' property in ExpandAttribute.",
                result);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryEntityWithDollarLevelsEqualToTheMaxExpansionDepth(string format)
        {
            string requestUri = this.BaseAddress + "/odata/Employees(1)?$expand=manager($levels=3;$select=name)&$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject content = await response.Content.ReadAsObject<JObject>();
            Assert.True(1 == content["id"].Value<int>());

            JObject manager8 = content["manager"]["manager"]["manager"] as JObject;
            Assert.Equal("Name8", manager8["name"].Value<string>());
            Assert.Null(manager8["id"]);// As id is not selected

            Assert.Null(manager8["manager"]);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryEntitySetWithExpand(string format)
        {
            string requestUri = this.BaseAddress + "/odata/Employees?$expand=Microsoft.Test.E2E.AspNet.OData.LowerCamelCase.Manager/directReports($levels=2)&$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject content = await response.Content.ReadAsObject<JObject>();
            JArray value = content["value"] as JArray;
            Assert.True(10 == value.Count);

            JObject employee10 = value.Single(e => e["id"].Value<int>() == 10) as JObject;
            JArray directReportsOfE10 = employee10["directReports"] as JArray;
            Assert.True(2 == directReportsOfE10.Count);

            JObject employee5 = directReportsOfE10.Single(e => e["id"].Value<int>() == 5) as JObject;
            Assert.Null(employee5["directReports"]);// Employee with ID 5 is not a manager.

            JObject employee9 = directReportsOfE10.Single(e => e["id"].Value<int>() == 9) as JObject;
            JArray directReportsOfE9 = employee9["directReports"] as JArray;
            Assert.True(2 == directReportsOfE9.Count);

            JObject employee8 = directReportsOfE9.Single(e => e["id"].Value<int>() == 8) as JObject;
            Assert.Null(employee8["directReports"]);// The levels value is 2, but ID 8 is at the third level: 8->9->10
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryEntitySetWithExpandToSameType(string format)
        {
            // The context url is '@odata.context=http://jinfutan13:9123/odata/$metadata#Employees(id,name,next)', 
            // but the '+' follwoing next is missing due to bug the defect 2084225.
            string requestUri = this.BaseAddress + "/odata/Employees?$expand=next($levels=2)&$select=id,name,next&$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject content = await response.Content.ReadAsObject<JObject>();
            JArray value = content["value"] as JArray;
            Assert.True(10 == value.Count);

            JObject employee10 = value.Single(e => e["id"].Value<int>() == 10) as JObject;
            Assert.False(employee10["next"].HasValues);

            JObject employee9 = value.Single(e => e["id"].Value<int>() == 9) as JObject;
            Assert.False(employee9["next"]["next"].HasValues);

            JObject employee8 = value.Single(e => e["id"].Value<int>() == 8) as JObject;
            Assert.True(10 == employee8["next"]["next"]["id"].Value<int>());
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryEntityWithExpandToSameTypeWhereCircularExists(string format)
        {
            // The context url is '@odata.context=http://jinfutan13:9123/odata/$metadata#Employees(id,name,next)', 
            // but the '+' follwoing next is missing due to bug the defect 2084225.
            string requestUri = this.BaseAddress + "/odata/Employees(1)?$expand=next($levels=2)&$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject content = await response.Content.ReadAsObject<JObject>();
            Assert.True(1 == content["id"].Value<int>());

            Assert.True(1 == content["next"]["next"]["id"].Value<int>());
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryEntityWithExpandToSameTypeWithNestedExpand(string format)
        {
            // The context url is '@odata.context=http://jinfutan13:9123/odata/$metadata#Employees(id,name,next)', 
            // but the '+' follwoing next is missing due to bug the defect 2084225.
            string requestUri = this.BaseAddress + "/odata/Employees(3)?$expand=next($expand=next($levels=2))&$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject content = await response.Content.ReadAsObject<JObject>();
            Assert.True(3 == content["id"].Value<int>());

            Assert.True(6 == content["next"]["next"]["next"]["id"].Value<int>());
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task ExpandFromSingleManagerToSingleEmployee(string format)
        {
            string requestUri = this.BaseAddress +
                "/odata/Employees(6)/manager?$expand=next($levels=2;$select=id)&$select=id,name,next&$format=" +
                format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject employeeNo7 = await response.Content.ReadAsObject<JObject>();
            Assert.True(7 == employeeNo7["id"].Value<int>());

            JObject employeeNo8 = employeeNo7["next"] as JObject;
            Assert.True(8 == employeeNo8["id"].Value<int>());

            JObject employeeNo9 = employeeNo8["next"] as JObject;
            Assert.True(9 == employeeNo9["id"].Value<int>());

            JToken employee10 = employeeNo9["next"];
            Assert.Null(employee10);
        }
        
        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task ExpandFromSingleManagerToSingleEmployeeWithNestedExpand(string format)
        {
            string requestUri = this.BaseAddress +
                "/odata/Employees(1)/manager?$expand=next($levels=2;$select=name;$expand=manager($levels=2;$select=id))&$select=id,name,next&$format=" +
                format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject employeeNo6 = await response.Content.ReadAsObject<JObject>();
            Assert.True(6 == employeeNo6["id"].Value<int>());

            JToken employee10 = employeeNo6["next"]["next"]["manager"]["manager"] as JObject;
            Assert.Null(employee10["name"]);
            Assert.Equal(10,employee10["id"].Value<int>());
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task ExpandFromCollectionEmployeeToSingleManager(string format)
        {
            string requestUri = string.Format(
                "{0}/odata/Employees/Microsoft.Test.E2E.AspNet.OData.LowerCamelCase.GetEarliestTwoEmployees()" +
                "?$expand=manager($levels=2)&$select=id&$format={1}",
                 this.BaseAddress,
                 format
             );

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);          
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject content = await response.Content.ReadAsObject<JObject>();
            var results = content.GetValue("value") as JArray;
            Assert.True(2 == results.Count);

            JObject employeeNo1 = results.Single(r => r["id"].Value<int>() == 1) as JObject;
            Assert.Null(employeeNo1["name"]);

            JObject employeeNo7 = employeeNo1["manager"]["manager"] as JObject;
            Assert.Equal("Name7", employeeNo7["name"].Value<string>());

            var thirdLevelManager = employeeNo7["manager"];
            Assert.Null(thirdLevelManager);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryPropertyInEntityType(string format)
        {
            string requestUri = this.BaseAddress + "/odata/Employees(1)/name?$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsObject<JObject>();
            var value = json.GetValue("value").ToString();
            Assert.Equal("Name1", value);
            if (format == "application/json;odata.metadata=full")
            {
                var context = json.GetValue("@odata.context").ToString();
                Assert.True(context.IndexOf("/$metadata#Edm.String") > 0);
            }
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryPropertyValueInEntityType(string format)
        {
            var requestUri = this.BaseAddress + "/odata/Employees(1)/name/$value?$format=" + format;
            var response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Name1", content);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryEntitiesWithFilter(string format)
        {
            string requestUri = string.Format("{0}/odata/Employees?$filter=startswith(name,'Name1')&$format={1}", this.BaseAddress, format);

            using (var response = await this.Client.GetAsync(requestUri))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var result = await response.Content.ReadAsObject<JObject>();
                var value = result.GetValue("value") as JArray;
                Assert.NotNull(value);
                Assert.Equal(2, value.Count);
            }
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryEntitiesWithSelect(string format)
        {
            string requestUri = this.BaseAddress + "/odata/Employees?$select=id,name,address&$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsObject<JObject>();

            var value = result.GetValue("value") as JArray;
            /*
             * Each array element looks like:
             *   {
             *     "@odata.type":"#Microsoft.Test.E2E.AspNet.OData.CamelCase.Manager",
             *     "@odata.id":"http://jinfutanwebapi1:9123/odata/Employees(5)",
             *     "id":5,
             *     "name":"Name5",
             *     "address":{
             *       "street":"Street5","city":"City5"
             *     }
             *   }
             */
            Assert.Equal(10, value.Count);
            var employee = (JObject)value[9];
            var properties = employee.Properties().Where(p => !p.Name.StartsWith("@"));
            Assert.Equal(3, properties.Count());
            AssertPropertyNamesAreCamelCase(employee);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryEntitiesWithOrderBy(string format)
        {
            string requestUri = this.BaseAddress + "/odata/Employees?$orderby=name desc&$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsObject<JObject>();

            var value = result.GetValue("value") as JArray;
            Assert.Equal(10, value.Count);

            var firstEmployee = value[0];
            Assert.Equal(9, firstEmployee["id"].Value<int>());
            var secondEmployee = value[1];
            Assert.Equal(8, secondEmployee["id"].Value<int>());
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryEntitiesWithSkipToken(string format)
        {
            string requestUri = string.Format("{0}/odata/Employees/Microsoft.Test.E2E.AspNet.OData.LowerCamelCase.GetPaged()?format={1}", this.BaseAddress, format);
            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsObject<JObject>();
            var value = result["@odata.nextLink"].Value<string>();
            Assert.Contains("$skiptoken=id", value);
        }

        [Fact]
        public async Task AddEntity()
        {
            var postUri = this.BaseAddress + "/odata/Employees";

            var postContent = JObject.Parse(@"{
                    'name':'Name11',
                    'gender':'Female',
                    'address':{
                            'city':'city11',
                            'street':'street11'
                    }}");
            using (HttpResponseMessage response = await this.Client.PostAsJsonAsync(postUri, postContent))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);

                await ResetDatasource();

                var employee = await response.Content.ReadAsObject<JObject>();
                var result = employee.GetValue("id");
                Assert.Equal(11, result.Value<int>());
            }
        }

        [Fact]
        public async Task UpdateEntity()
        {
            var putUri = this.BaseAddress + "/odata/Employees(2)";
            var putContent = JObject.Parse(@"{'id':2,
                    'name':'Name20',
                    'gender':'Female',
                    'address':{
                            'city':'City20',
                            'street':'Street20'
                    }}");
            using (HttpResponseMessage response = await Client.PutAsJsonAsync(putUri, putContent))
            {
                await ResetDatasource();

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var content = await response.Content.ReadAsObject<JObject>();

                var name = content.GetValue("name");
                Assert.Equal("Name20", name);

                var gender = content.GetValue("gender").ToString();
                Assert.Equal("Female", gender);

                var address = content["address"];
                var city = address["city"].ToString();
                Assert.Equal("City20", city);
            }
        }

        [Fact]
        public async Task DeleteEntity()
        {
            var uriDelete = this.BaseAddress + "/odata/Employees(1)";
            using (HttpResponseMessage response = await Client.DeleteAsync(uriDelete))
            {
                await ResetDatasource();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task ActionCall()
        {
            var postUri = this.BaseAddress + "/odata/SetAddress";
            var postContent = JObject.Parse(@"{'id':10,
                    'address':{
                            'city':'City20',
                            'street':'Street20'
                    }}");

            var response = await this.Client.PostAsJsonAsync(postUri, postContent);
            await ResetDatasource();

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsObject<JObject>();

            var name = content.GetValue("name");
            Assert.Equal("Name10", name);

            var gender = content.GetValue("gender").ToString();
            Assert.Equal("Male", gender);

            var address = content["address"];
            var street = address["street"].ToString();
            Assert.Equal("Street20", street);
        }

        [Fact]
        public async Task FunctionCall()
        {
            var getUri = this.BaseAddress + "/odata/GetAddress(id=1)";
            var response = await this.Client.GetAsync(getUri);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsObject<JObject>();
            var city = content["city"].ToString();
            Assert.Equal("City1", city);
        }

        private async Task<HttpResponseMessage> ResetDatasource()
        {
            var uriReset = this.BaseAddress + "/odata/ResetDataSource";
            var response = await this.Client.PostAsync(uriReset, null);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            return response;
        }

        private void AssertPropertyNamesAreCamelCase(JObject jObject)
        {
            var properties = jObject.Properties();
            foreach (var property in properties.Where(p => !p.Name.StartsWith("@")))
            {
                Assert.True(this.IsCamelCase(property.Name),
                    string.Format("'{0}' should be in camel case", property.Name));
            }
        }

        private bool IsCamelCase(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return true;
            }
            return !char.IsUpper(token[0]);
        }
    }
}
