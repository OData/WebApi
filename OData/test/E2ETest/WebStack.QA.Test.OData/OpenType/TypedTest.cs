namespace WebStack.QA.Test.OData.OpenType
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.OData;
    using System.Web.OData.Extensions;
    using System.Web.OData.Routing;
    using System.Web.OData.Routing.Conventions;
    using System.Xml;
    using Microsoft.OData.Client;
    using Microsoft.OData.Edm;
    using Microsoft.OData.Edm.Csdl;
    using Newtonsoft.Json.Linq;
    using Nuwa;
    using WebStack.QA.Test.OData.Common;
    using Xunit;
    using Xunit.Extensions;
    using TypedProxy = WebStack.QA.Test.OData.OpenType.Typed.Client;

    [NuwaFramework]
    [NuwaTrace(NuwaTraceAttribute.Tag.Off)]
    public class TypedOpenTypeTest
    {
        private static string[] Routings = new string[] { "convention", "AttributeRouting" };
        int expectedValueOfInt, actualValueOfInt;
        int? expectedValueOfNullableInt, actualValueOfNullableInt;
        string expectedValueOfString, actualValueOfString;


        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] { typeof(EmployeesController), typeof(AccountsController), typeof(MetadataController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute(
                Routings[0],
                Routings[0],
                OpenComplexTypeEdmModel.GetTypedConventionModel(),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute(Routings[1], Routings[1], OpenComplexTypeEdmModel.GetTypedConventionModel());
            configuration.MapODataServiceRoute("explicit", "explicit", OpenComplexTypeEdmModel.GetTypedExplicitModel());
            configuration.EnsureInitialized();
        }

        #region Http client case

        #region Query

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task QueryEntitySet(string format)
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();
                string requestUri = string.Format(this.BaseAddress + "/{0}/Accounts?$format={1}", routing, format);

                HttpResponseMessage response = await this.Client.GetAsync(requestUri);
                Assert.True(response.IsSuccessStatusCode);

                var json = await response.Content.ReadAsAsync<JObject>();

                var results = json.GetValue("value") as JArray;
                Assert.Equal<int>(3, results.Count);

                var age = results[1]["AccountInfo"]["Age"];
                Assert.Equal(20, age);

                var gender = (string)results[2]["AccountInfo"]["Gender"];
                Assert.Equal("Female", gender);

                var country = results[1]["Address"]["Country"].ToString();
                Assert.Equal("China", country);

                var tag1 = results[0]["Tags"]["Tag1"];
                Assert.Equal("Value 1", tag1);
                var tag2 = results[0]["Tags"]["Tag2"];
                Assert.Equal("Value 2", tag2);
            }
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task QueryEntity(string format)
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                string requestUri = string.Format(this.BaseAddress + "/{0}/Accounts(1)?$format=", routing, format);

                HttpResponseMessage response = await this.Client.GetAsync(requestUri);
                Assert.True(response.IsSuccessStatusCode);

                var result = await response.Content.ReadAsAsync<JObject>();

                var age = result["AccountInfo"]["Age"];
                Assert.Equal(10, age);

                var gender = (string)result["AccountInfo"]["Gender"];
                Assert.Equal("Male", gender);

                var country = result["Address"]["Country"].ToString();
                Assert.Equal("US", country);

                var tag1 = result["Tags"]["Tag1"];
                Assert.Equal("Value 1", tag1);
                var tag2 = result["Tags"]["Tag2"];
                Assert.Equal("Value 2", tag2);
            }
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task QueryPropertyFromDerivedOpenEntity(string format)
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                string requestUri = string.Format(this.BaseAddress + "/{0}/Accounts(1)/WebStack.QA.Test.OData.OpenType.PremiumAccount/Since?$format=", routing, format);

                HttpResponseMessage response = await this.Client.GetAsync(requestUri);
                Assert.True(response.IsSuccessStatusCode);

                var content = await response.Content.ReadAsStringAsync();

                Assert.Contains("2014-05-22T00:00:00+08:00", content);
            }
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task QueryOpenComplexTypePropertyAccountInfo(string format)
        {
            await ResetDatasource();

            string requestUri = this.BaseAddress + "/convention/Accounts(1)/AccountInfo?$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.True(response.IsSuccessStatusCode);

            var json = await response.Content.ReadAsAsync<JObject>();

            var nickName = json.GetValue("NickName").ToString();
            Assert.Equal("NickName1", nickName);

            var age = json.GetValue("Age");
            Assert.Equal(10, age);

            var gender = (string)json.GetValue("Gender");
            Assert.Equal("Male", gender);
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task QueryOpenComplexTypePropertyAddress(string format)
        {
            await ResetDatasource();

            string requestUri = this.BaseAddress + "/AttributeRouting/Accounts(1)/Address/WebStack.QA.Test.OData.OpenType.GlobalAddress?$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.True(response.IsSuccessStatusCode);

            var json = await response.Content.ReadAsAsync<JObject>();

            var city = json.GetValue("City").ToString();
            Assert.Equal("Redmond", city);

            var country = json.GetValue("Country").ToString();
            Assert.Equal("US", country);

            // Property defined in the derived type.
            var countryCode = json.GetValue("CountryCode").ToString();
            Assert.Equal("US", countryCode);
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task QueryDerivedOpenComplexType(string format)
        {
            await ResetDatasource();

            string requestUri = this.BaseAddress + "/AttributeRouting/Accounts(1)/Address/WebStack.QA.Test.OData.OpenType.GlobalAddress?$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.True(response.IsSuccessStatusCode);

            var json = await response.Content.ReadAsAsync<JObject>();

            var city = json.GetValue("City").ToString();
            Assert.Equal("Redmond", city);

            var country = json.GetValue("Country").ToString();
            Assert.Equal("US", country);
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task QueryOpenComplexTypePropertyTags(string format)
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                string requestUri = string.Format(this.BaseAddress + "/{0}/Accounts(1)/Tags?$format={1}", routing, format);

                HttpResponseMessage response = await this.Client.GetAsync(requestUri);
                Assert.True(response.IsSuccessStatusCode);

                var json = await response.Content.ReadAsAsync<JObject>();

                var tag1 = json.GetValue("Tag1").ToString();
                Assert.Equal("Value 1", tag1);

                var tag2 = json.GetValue("Tag2").ToString();
                Assert.Equal("Value 2", tag2);
            }
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task QueryNonDynamicProperty(string format)
        {
            await ResetDatasource();

            string requestUri = this.BaseAddress + "/AttributeRouting/Accounts(1)/Address/City?$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.True(response.IsSuccessStatusCode);

            var json = await response.Content.ReadAsAsync<JObject>();

            var city = json.GetValue("value").ToString();
            Assert.Equal("Redmond", city);
        }

        #endregion

        #region Update

        [Theory(Skip = "https://github.com/OData/odata.net/issues/612")]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task PatchEntityWithOpenComplexType(string format)
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                var patchUri = string.Format(this.BaseAddress + "/{0}/Accounts(1)?$format={1}", routing, format);
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), patchUri);
                request.Content = new StringContent(
                 @"{
                    '@odata.type':'#WebStack.QA.Test.OData.OpenType.Account',
                    'AccountInfo':{'NickName':'NewNickName1','Age':40,'Gender': 'Male'},
                    'Address':{'Country':'United States'},
                    'Tags':{'Tag1':'New Value'},
                    'ShipAddresses@odata.type':'#Collection(WebStack.QA.Test.OData.OpenType.Address)',
                    'ShipAddresses':[],
                    'OwnerGender@odata.type':'#WebStack.QA.Test.OData.OpenType.Gender',
                    'OwnerGender':null
                  }");

                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                using (var patchResponse = this.Client.SendAsync(request).Result)
                {
                    Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);

                    var content = await patchResponse.Content.ReadAsAsync<JObject>();

                    var accountInfo = content["AccountInfo"];
                    Assert.Equal("NewNickName1", accountInfo["NickName"]);
                    Assert.Equal(40, accountInfo["Age"]);

                    Assert.Equal("Male", (string)accountInfo["Gender"]);

                    var address = content["Address"];
                    Assert.Equal("United States", address["Country"]);

                    var tags = content["Tags"];
                    Assert.Equal("New Value", tags["Tag1"]);
                    JsonAssert.DoesNotContainProperty("OwnerGender", content);
                    Assert.Equal("jinfutan", content["OwnerAlias"]);
                    Assert.Equal(0, ((JArray)content["ShipAddresses"]).Count);
                }

                string requestUri = string.Format(this.BaseAddress + "/{0}/Accounts(1)?$format={1}", routing, format);

                HttpResponseMessage response = await this.Client.GetAsync(requestUri);
                Assert.True(response.IsSuccessStatusCode);

                var result = await response.Content.ReadAsAsync<JObject>();

                var updatedAccountinfo = result["AccountInfo"];
                Assert.Equal("NewNickName1", updatedAccountinfo["NickName"]);
                Assert.Equal(40, updatedAccountinfo["Age"]);
                Assert.Equal("Male", updatedAccountinfo["Gender"]);

                var updatedAddress = result["Address"];
                Assert.Equal("United States", updatedAddress["Country"]);

                var updatedTags = result["Tags"];
                Assert.Equal("New Value", updatedTags["Tag1"]);
            }
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task PutEntityWithOpenComplexType(string format)
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                var putUri = string.Format(this.BaseAddress + "/{0}/Accounts(1)?$format={1}", routing, format);
                var putContent = JObject.Parse(@"{'Id':1,'Name':'NewName1',
                'AccountInfo':{'NickName':'NewNickName1','Age':11,'Gender@odata.type':'#WebStack.QA.Test.OData.OpenType.Gender','Gender':'Male'},
                'Address':{'City':'Redmond','Street':'1 Microsoft Way','Country':'United States'},
                'Tags':{'Tag1':'New Value'}}");

                using (HttpResponseMessage putResponse = await Client.PutAsJsonAsync(putUri, putContent))
                {
                    Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

                    var content = await putResponse.Content.ReadAsAsync<JObject>();

                    var accountInfo = content["AccountInfo"];
                    Assert.Equal("NewNickName1", accountInfo["NickName"]);
                    Assert.Equal(11, accountInfo["Age"]);

                    Assert.Equal("Male", accountInfo["Gender"]);

                    var address = content["Address"];
                    Assert.Equal("United States", address["Country"]);

                    var tags = content["Tags"];
                    Assert.Equal("New Value", tags["Tag1"]);
                }

                string requestUri = string.Format(this.BaseAddress + "/{0}/Accounts(1)?$format={1}", routing, format);

                HttpResponseMessage response = await this.Client.GetAsync(requestUri);
                Assert.True(response.IsSuccessStatusCode);

                var result = await response.Content.ReadAsAsync<JObject>();

                var updatedAccountinfo = result["AccountInfo"];
                Assert.Equal("NewNickName1", updatedAccountinfo["NickName"]);
                Assert.Equal(11, updatedAccountinfo["Age"]);

                var updatedAddress = result["Address"];
                Assert.Equal("United States", updatedAddress["Country"]);

                var updatedTags = result["Tags"];
                Assert.Equal("New Value", updatedTags["Tag1"]);
            }
        }

        [Fact]
        public async Task PatchOpenComplexTypeProperty()
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                // Get ~/Accounts(1)/Address
                var requestUri = string.Format(BaseAddress + "/{0}/Accounts(1)/Address", routing);
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                var response = await Client.SendAsync(request);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var content = await response.Content.ReadAsAsync<JObject>();
                Assert.Equal(5, content.Count); // @odata.context + 3 declared properties + 1 dynamic properties
                Assert.Equal("Redmond", content["City"]);
                Assert.Equal("1 Microsoft Way", content["Street"]);
                Assert.Equal("US", content["CountryCode"]);
                Assert.Equal("US", content["Country"]); // dynamic property

                // Patch ~/Accounts(1)/Address
                request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
                request.Content = new StringContent(
                    @"{
                        '@odata.type':'#WebStack.QA.Test.OData.OpenType.Address',
                        'City':'NewCity',
                        'OtherProperty@odata.type':'#Date',
                        'OtherProperty':'2016-02-01'
                  }");
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                response = await this.Client.SendAsync(request);
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                // Get ~/Accounts(1)/Address
                request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                response = await Client.SendAsync(request);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                content = await response.Content.ReadAsAsync<JObject>();
                Assert.Equal(6, content.Count); // @odata.context + 3 declared properties + 1 dynamic properties + 1 new dynamic properties
                Assert.Equal("NewCity", content["City"]); // updated
                Assert.Equal("1 Microsoft Way", content["Street"]);
                Assert.Equal("US", content["CountryCode"]);
                Assert.Equal("US", content["Country"]);
                Assert.Equal("2016-02-01", content["OtherProperty"]);
            }
        }

        [Fact]
        public async Task PatchOpenDerivedComplexTypeProperty()
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                // Get ~/Accounts(1)/Address
                var requestUri = string.Format(BaseAddress + "/{0}/Accounts(1)/Address/WebStack.QA.Test.OData.OpenType.GlobalAddress", routing);
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                var response = await Client.SendAsync(request);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var content = await response.Content.ReadAsAsync<JObject>();
                Assert.Equal(5, content.Count); // @odata.context + 3 declared properties + 1 dynamic properties
                Assert.Equal("Redmond", content["City"]);
                Assert.Equal("1 Microsoft Way", content["Street"]);
                Assert.Equal("US", content["CountryCode"]);
                Assert.Equal("US", content["Country"]);

                // Patch ~/Accounts(1)/Address/WebStack.QA.Test.OData.OpenType.GlobalAddress
                request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
                request.Content = new StringContent(
                    @"{
                        '@odata.type':'#WebStack.QA.Test.OData.OpenType.GlobalAddress',
                        'CountryCode':'NewCountryCode',
                        'Country':'NewCountry'
                  }");
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                response = await this.Client.SendAsync(request);
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                // Get ~/Accounts(1)/Address/WebStack.QA.Test.OData.OpenType.GlobalAddress
                request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                response = await Client.SendAsync(request);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                content = await response.Content.ReadAsAsync<JObject>();
                Assert.Equal(5, content.Count); // @odata.context + 3 declared properties + 1 dynamic properties
                Assert.Equal("Redmond", content["City"]);
                Assert.Equal("1 Microsoft Way", content["Street"]);
                Assert.Equal("NewCountryCode", content["CountryCode"]); // updated
                Assert.Equal("NewCountry", content["Country"]);  // updated
            }
        }

        [Fact]
        public async Task PutOpenComplexTypeProperty()
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                // Get ~/Accounts(1)/Address
                var requestUri = string.Format(BaseAddress + "/{0}/Accounts(1)/Address", routing);
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                var response = await Client.SendAsync(request);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var content = await response.Content.ReadAsAsync<JObject>();
                Assert.Equal(5, content.Count); // @odata.context + 3 declared properties + 1 dynamic properties
                Assert.Equal("Redmond", content["City"]);
                Assert.Equal("1 Microsoft Way", content["Street"]);
                Assert.Equal("US", content["CountryCode"]);
                Assert.Equal("US", content["Country"]); // dynamic property

                // Put ~/Accounts(1)/Address
                request = new HttpRequestMessage(HttpMethod.Put, requestUri);
                request.Content = new StringContent(
                    @"{
                        '@odata.type':'#WebStack.QA.Test.OData.OpenType.Address',
                        'City':'NewCity',
                        'Street':'NewStreet',
                        'OtherProperty@odata.type':'#Date',
                        'OtherProperty':'2016-02-01'
                  }");
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                response = await this.Client.SendAsync(request);
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                // Get ~/Accounts(1)/Address
                request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                response = await Client.SendAsync(request);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                content = await response.Content.ReadAsAsync<JObject>();
                Assert.Equal(5, content.Count); // @odata.context + 3 declared properties + 1 new dynamic properties
                Assert.Equal("NewCity", content["City"]); // updated
                Assert.Equal("NewStreet", content["Street"]); // updated
                Assert.Equal("US", content["CountryCode"]);
                Assert.Null(content["Country"]);
                Assert.Equal("2016-02-01", content["OtherProperty"]);
            }
        }

        #endregion

        #region Insert

        [Fact]
        public async Task InsertEntityWithOpenComplexTypeProperty()
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                var postUri = string.Format(this.BaseAddress + "/{0}/Accounts", routing);

                var postContent = JObject.Parse(
@"{
    'Id':4,
    'Name':'Name4',
    'AccountInfo':
    {
        'NickName':'NickName4','Age':40,'Gender@odata.type':'#WebStack.QA.Test.OData.OpenType.Gender','Gender':'Male'
    },
    'Address':
    {
        '@odata.type':'#WebStack.QA.Test.OData.OpenType.GlobalAddress',
        'City':'London','Street':'Baker street','Country':'UnitedKindom','CountryCode':'Code'
    },
    'Tags':{'Tag1':'Value 1','Tag2':'Value 2'},
    'AnotherGender@odata.type':'#WebStack.QA.Test.OData.OpenType.Gender',
    'AnotherGender':'Female'
}");
                using (HttpResponseMessage response = await this.Client.PostAsJsonAsync(postUri, postContent))
                {
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

                    var json = await response.Content.ReadAsAsync<JObject>();

                    var age = json["AccountInfo"]["Age"];
                    Assert.Equal(40, age);

                    var gender = (string)json["AccountInfo"]["Gender"];
                    Assert.Equal("Male", gender);

                    var country = json["Address"]["Country"];
                    Assert.Equal("UnitedKindom", country);

                    var countryCode = json["Address"]["CountryCode"];
                    Assert.Equal("Code", countryCode);

                    var tag1 = json["Tags"]["Tag1"];
                    Assert.Equal("Value 1", tag1);
                    var tag2 = json["Tags"]["Tag2"];
                    Assert.Equal("Value 2", tag2);
                    var anotherGender = (string)json["AnotherGender"];
                    Assert.Equal("Female", anotherGender);
                }
            }
        }

        #endregion

        #region Delete

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task DeleteEntityWithOpenComplexTypeProperty(string format)
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                var deleteUri = string.Format(this.BaseAddress + "/{0}/Accounts(1)?$format={1}", routing, format);

                using (HttpResponseMessage response = await Client.DeleteAsync(deleteUri))
                {
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                }

                var requestUri = string.Format(this.BaseAddress + "/{0}/Accounts?$format={1}", routing, format);

                using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
                {
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsAsync<JObject>();

                    var results = json.GetValue("value") as JArray;
                    Assert.Equal(2, results.Count);
                }
            }
        }

        [Fact]
        public async Task DeleteOpenComplexTypeProperty()
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                // Get ~/Accounts(1)/Address
                var requestUri = string.Format(BaseAddress + "/{0}/Accounts(1)/Address", routing);
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                var response = await Client.SendAsync(request);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var content = await response.Content.ReadAsAsync<JObject>();
                Assert.Equal(5, content.Count); // @odata.context + 3 declared properties + 1 dynamic properties
                Assert.Equal("Redmond", content["City"]);
                Assert.Equal("1 Microsoft Way", content["Street"]);
                Assert.Equal("US", content["CountryCode"]);
                Assert.Equal("US", content["Country"]); // dynamic property

                // Delete ~/Accounts(1)/Address
                request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
                response = await this.Client.SendAsync(request);
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                // Get ~/Accounts(1)/Address
                request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                response = await Client.SendAsync(request);
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }
        #endregion

        #region Function & Action

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task GetAddressFunction(string format)
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                string requestUri = string.Format(this.BaseAddress + "/{0}/Accounts(1)/WebStack.QA.Test.OData.OpenType.GetAddressFunction()?$format=", routing, format);

                HttpResponseMessage response = await this.Client.GetAsync(requestUri);
                Assert.True(response.IsSuccessStatusCode);

                var json = await response.Content.ReadAsAsync<JObject>();

                var city = json.GetValue("City").ToString();
                Assert.Equal("Redmond", city);

                var country = json.GetValue("Country");
                Assert.Equal("US", country);

                var countryCode = json.GetValue("CountryCode");
                Assert.Equal("US", countryCode);
            }
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task IncreaseAgeAction(string format)
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                string requestUri = string.Format(this.BaseAddress + "/{0}/Accounts(1)/WebStack.QA.Test.OData.OpenType.IncreaseAgeAction()?$format=", routing, format);
                var requestForPost = new HttpRequestMessage(HttpMethod.Post, requestUri);
                requestForPost.Content = new StringContent(string.Empty);
                requestForPost.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                HttpResponseMessage response = await this.Client.SendAsync(requestForPost);
                Assert.True(response.IsSuccessStatusCode);

                var json = await response.Content.ReadAsAsync<JObject>();

                var nickName = json.GetValue("NickName").ToString();
                Assert.Equal("NickName1", nickName);

                var age = json.GetValue("Age");
                Assert.Equal(11, age);
            }
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task UpdateAddressAction(string format)
        {
            await ResetDatasource();

            string uri = this.BaseAddress + "/AttributeRouting/UpdateAddressAction?$format=" + format;
            var content = new { Address = new { Street = "Street 11", City = "City 11", Country = "Country 11" }, ID = 1 };

            var response = await Client.PostAsJsonAsync(uri, content);
            Assert.True(response.IsSuccessStatusCode);

            string getUri = this.BaseAddress + "/AttributeRouting/Accounts(1)";

            HttpResponseMessage getResponse = await this.Client.GetAsync(getUri);
            Assert.True(getResponse.IsSuccessStatusCode);

            var result = await getResponse.Content.ReadAsAsync<JObject>();

            var city = result["Address"]["City"].ToString();
            Assert.Equal("City 11", city);
            var country = result["Address"]["Country"].ToString();
            Assert.Equal("Country 11", country);
        }

        #endregion

        #endregion

        #region OData Client test case

        #region Query

        [Fact]
        public async Task QueryEntitySetClientTest()
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                Uri serviceUrl = new Uri(string.Format(this.BaseAddress + "/{0}/", routing));
                var client = new TypedProxy.Container(serviceUrl);
                client.Format.UseJson();

                var accounts = client.Accounts.Where(a => a.Id <= 2);
                List<TypedProxy.Account> accountList = accounts.ToList();

                Assert.Equal(2, accountList.Count);
                Assert.Equal(1, accountList[0].Id);
                Assert.Equal("US", accountList[0].Address.Country);
                Assert.Equal(10, accountList[0].AccountInfo.Age);

                Assert.Equal(TypedProxy.Gender.Male, accountList[0].AccountInfo.Gender);

                Assert.Equal("Value 1", accountList[0].Tags.Tag1);
                Assert.Equal("Value 2", accountList[0].Tags.Tag2);

                Assert.Equal(TypedProxy.Gender.Female, accountList[0].OwnerGender);
                Assert.Equal("jinfutan", accountList[0].OwnerAlias);
                Assert.Equal(true, accountList[0].IsValid);
                Assert.Equal(2, accountList[0].ShipAddresses.Count);
            }
        }

        [Fact]
        public async Task QueryEntitySetClientTestWithSelect()
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                Uri serviceUrl = new Uri(string.Format(this.BaseAddress + "/{0}/", routing));
                var client = new TypedProxy.Container(serviceUrl);
                client.Format.UseJson();

                var accounts = client.Accounts.Where(a => a.Id <= 2).Select(a => new TypedProxy.Account() { Id = a.Id, Name = a.Name });
                List<TypedProxy.Account> accountList = accounts.ToList();

                Assert.Equal(2, accountList.Count);
                Assert.Equal(1, accountList[0].Id);

                Assert.Null(accountList[0].OwnerAlias);
                Assert.Equal(0, accountList[0].ShipAddresses.Count);
            }
        }

        [Fact]
        public async Task QueryEntityClientTest()
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                Uri serviceUrl = new Uri(string.Format(this.BaseAddress + "/{0}/", routing));
                var client = new TypedProxy.Container(serviceUrl);
                client.Format.UseJson();

                var account = client.Accounts.Where(a => a.Id == 1).Single();

                Assert.Equal(1, account.Id);
                Assert.Equal("US", account.Address.Country);
                Assert.Equal(10, account.AccountInfo.Age);

                Assert.Equal(WebStack.QA.Test.OData.OpenType.Typed.Client.Gender.Male, account.AccountInfo.Gender);

                Assert.Equal("Value 1", account.Tags.Tag1);
                Assert.Equal("Value 2", account.Tags.Tag2);

                Assert.Equal(TypedProxy.Gender.Female, account.OwnerGender);
                Assert.Equal("jinfutan", account.OwnerAlias);
                Assert.Equal(true, account.IsValid);
                Assert.Equal(2, account.ShipAddresses.Count);
            }
        }

        [Fact]
        public async Task QueryDerivedEntityClientTest()
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                Uri serviceUrl = new Uri(string.Format(this.BaseAddress + "/{0}/", routing));
                var client = new TypedProxy.Container(serviceUrl);
                client.Format.UseJson();

                var premiumAcconts = client.Accounts.OfType<TypedProxy.PremiumAccount>().ToList();
                var premiumAccount = premiumAcconts.Single(pa => pa.Id == 1);
                Assert.Equal(1, premiumAccount.Id);
                Assert.Equal("US", premiumAccount.Address.Country);
                Assert.Equal(10, premiumAccount.AccountInfo.Age);

                Assert.Equal(WebStack.QA.Test.OData.OpenType.Typed.Client.Gender.Male, premiumAccount.AccountInfo.Gender);

                Assert.Equal("Value 1", premiumAccount.Tags.Tag1);
                Assert.Equal("Value 2", premiumAccount.Tags.Tag2);

                Assert.Equal(TypedProxy.Gender.Female, premiumAccount.OwnerGender);
                Assert.Equal("jinfutan", premiumAccount.OwnerAlias);
                Assert.Equal(true, premiumAccount.IsValid);
                Assert.Equal(2, premiumAccount.ShipAddresses.Count);
                Assert.Equal(new DateTimeOffset(new DateTime(2014, 5, 22), TimeSpan.FromHours(8)), premiumAccount.Since);
            }
        }

        [Fact]
        public async Task QueryOpenComplexTypePropertyAccountInfoClientTest()
        {
            await ResetDatasource();

            Uri serviceUrl = new Uri(this.BaseAddress + "/convention/");
            var client = new TypedProxy.Container(serviceUrl);
            client.Format.UseJson();

            var accountInfo = client.Accounts.Where(a => a.Id == 1).Select(a => a.AccountInfo).Single();

            Assert.Equal("NickName1", accountInfo.NickName);
            Assert.Equal(10, accountInfo.Age);
        }

        [Fact]
        public async Task QueryOpenComplexTypePropertyAddressClientTest()
        {
            await ResetDatasource();

            Uri serviceUrl = new Uri(this.BaseAddress + "/AttributeRouting/");
            var client = new TypedProxy.Container(serviceUrl);
            client.Format.UseJson();

            var address = client.Accounts.Where(a => a.Id == 1).Select(a => a.Address).Single();

            Assert.Equal("Redmond", address.City);
            Assert.Equal("US", address.Country);
        }

        [Fact]
        public async Task QueryOpenComplexTypePropertyTagsClientTest()
        {
            await ResetDatasource();

            Uri serviceUrl = new Uri(this.BaseAddress + "/AttributeRouting/");
            var client = new TypedProxy.Container(serviceUrl);
            client.Format.UseJson();

            var tags = client.Accounts.Where(a => a.Id == 1).Select(a => a.Tags).Single();

            Assert.Equal("Value 1", tags.Tag1);
            Assert.Equal("Value 2", tags.Tag2);
        }

        [Fact]
        public async Task QueryNonDynamicPropertyClientTest()
        {
            await ResetDatasource();

            Uri serviceUrl = new Uri(this.BaseAddress + "/AttributeRouting/");
            var client = new TypedProxy.Container(serviceUrl);
            client.Format.UseJson();

            var city = client.Accounts.Where(a => a.Id == 1).Select(a => a.Address.City).Single();

            Assert.Equal("Redmond", city);
        }

        #endregion

        #region Update

        [Fact]
        public async Task PatchEntityWithOpenComplexTypeClientTest()
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                Uri serviceUrl = new Uri(string.Format(this.BaseAddress + "/{0}/", routing));
                var client = new TypedProxy.Container(serviceUrl);
                client.MergeOption = MergeOption.OverwriteChanges;
                client.Format.UseJson();
                TypedProxy.Account account = client.Accounts.Where(a => a.Id == 1).Single();

                account.AccountInfo.NickName = "NewNickName";
                account.AccountInfo.Age = 11;

                account.AccountInfo.Gender = TypedProxy.Gender.Male;

                account.AccountInfo.Subs = new Collection<string>() { "1", "2", "3" };

                account.Address.Country = "United States";
                account.Tags.Tag1 = "New Value";
                account.OwnerAlias = "saxu";
                // TODO: client bug, collection should be nullable for dynamic properties.
                //account.ShipAddresses =null;
                account.OwnerGender = TypedProxy.Gender.Male;
                account.Emails = new List<string>() { "c@c.com", "d@d.com" };
                account.LuckyNumbers = new List<int>() { 4 };
                client.UpdateObject(account);
                client.SaveChanges();

                var updatedAccount = client.Accounts.Where(a => a.Id == 1).Single();
                Assert.NotNull(updatedAccount);

                var updatedAccountInfo = updatedAccount.AccountInfo;
                Assert.NotNull(updatedAccountInfo);
                Assert.Equal("NewNickName", updatedAccountInfo.NickName);
                Assert.Equal(11, updatedAccountInfo.Age);
                Assert.Equal(3, updatedAccountInfo.Subs.Count);

                // Defect 2371564 odata.type is missed in client payload for dynamic enum type
                //Assert.Equal(WebStack.QA.Test.OData.OpenType.Typed.Client.Gender.Male, updatedAccountInfo.Gender);

                var updatedAddress = updatedAccount.Address;
                Assert.NotNull(updatedAddress);
                Assert.Equal("United States", updatedAddress.Country);

                Assert.Equal("New Value", updatedAccount.Tags.Tag1);

                Assert.Equal("saxu", updatedAccount.OwnerAlias);
                Assert.Equal(2, updatedAccount.ShipAddresses.Count);
                Assert.Equal(2, updatedAccount.Emails.Count);
                Assert.Equal(1, updatedAccount.LuckyNumbers.Count);
                Assert.NotNull(updatedAccount.Emails.SingleOrDefault(e => e == "c@c.com"));

                // Defect 2371564 odata.type is missed in client payload for dynamic enum type
                //Assert.Equal(WebStack.QA.Test.OData.OpenType.Typed.Client.Gender.Male, updatedAccountInfo.Gender);
                //Assert.Equal(TypedProxy.Gender.Male, updatedAccount.OwnerGender);
            }
        }

        [Fact(Skip = "https://github.com/OData/odata.net/issues/612")]
        public async Task PutEntityWithOpenComplexTypeClientTest()
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                Uri serviceUrl = new Uri(string.Format(this.BaseAddress + "/{0}/", routing));
                var client = new TypedProxy.Container(serviceUrl);

                client.MergeOption = Microsoft.OData.Client.MergeOption.OverwriteChanges;
                client.Format.UseJson();

                var account = client.Accounts.Where(a => a.Id == 1).Single();
                Assert.NotNull(account);

                account.AccountInfo.NickName = "NewNickName";
                account.AccountInfo.Age = 11;

                account.AccountInfo.Gender = WebStack.QA.Test.OData.OpenType.Typed.Client.Gender.Male;

                account.AccountInfo.Subs = new Collection<string>() { "1", "2", "3" };

                account.Address.Country = "United States";
                account.Tags.Tag1 = "New Value";

                account.OwnerAlias = "saxu";
                account.ShipAddresses = new List<TypedProxy.Address>();
                account.OwnerGender = TypedProxy.Gender.Male;
                account.Emails = new List<string>() { "c@c.com", "d@d.com" };
                account.LuckyNumbers = new List<int>() { 4 };

                client.UpdateObject(account);
                client.SaveChanges(Microsoft.OData.Client.SaveChangesOptions.ReplaceOnUpdate);

                var updatedAccount = client.Accounts.Where(a => a.Id == 1).Single();
                Assert.NotNull(updatedAccount);

                var updatedAccountInfo = updatedAccount.AccountInfo;
                Assert.NotNull(updatedAccountInfo);
                Assert.Equal("NewNickName", updatedAccountInfo.NickName);
                Assert.Equal(11, updatedAccountInfo.Age);
                Assert.Equal(3, updatedAccountInfo.Subs.Count);

                // Defect 2371564 odata.type is missed in client payload for dynamic enum type
                //Assert.Equal(WebStack.QA.Test.OData.OpenType.Typed.Client.Gender.Male, updatedAccountInfo.Gender);

                var updatedAddress = updatedAccount.Address;
                Assert.NotNull(updatedAddress);
                Assert.Equal("United States", updatedAddress.Country);

                Assert.Equal("New Value", updatedAccount.Tags.Tag1);

                Assert.Equal("saxu", updatedAccount.OwnerAlias);
                Assert.Equal(0, updatedAccount.ShipAddresses.Count);
                Assert.Equal(1, updatedAccount.LuckyNumbers.Count);
                Assert.Equal(2, updatedAccount.Emails.Count);
                Assert.NotNull(updatedAccount.Emails.SingleOrDefault(e => e == "c@c.com"));
                // Defect 2371564 odata.type is missed in client payload for dynamic enum type
                //Assert.Equal(WebStack.QA.Test.OData.OpenType.Typed.Client.Gender.Male, updatedAccountInfo.Gender);
                //Assert.Equal(TypedProxy.Gender.Male, updatedAccount.OwnerGender);
            }
        }

        #endregion

        #region Insert

        [Fact]
        public async Task InsertBaseEntityWithOpenTypePropertyClientTest()
        {
            await ResetDatasource();

            Uri serviceUrl = new Uri(this.BaseAddress + "/convention/");
            var client = new TypedProxy.Container(serviceUrl);

            client.Format.UseJson();

            TypedProxy.Account newAccount = new TypedProxy.Account()
            {
                Id = 4,
                Name = "Name4",
                AccountInfo = new TypedProxy.AccountInfo
                {
                    NickName = "NickName4",
                    Age = 40,
                    Gender = TypedProxy.Gender.Female,
                },
                Address = new TypedProxy.Address
                {
                    City = "Paris",
                    Street = "1 Microsoft Way",
                    Country = "France",
                },
                Tags = new TypedProxy.Tags
                {
                    Tag1 = "value 1",
                    Tag2 = "value 2"
                },
                OwnerAlias = "saxu",
                ShipAddresses = new List<TypedProxy.Address>()
                {
                    new  TypedProxy.Address
                    {
                        City = "Jinan",
                        Street = "Danling Street"
                    },
                    new  TypedProxy.Address
                    {
                        City="Nanjing",
                        Street="Zixing",
                    },
                
                },
                OwnerGender = TypedProxy.Gender.Male,
                Emails = new List<string>() { "a@a.com", "b@b.com" },
                LuckyNumbers = new List<int>() { 1, 2, 3 },
            };
            client.AddToAccounts(newAccount);
            client.SaveChanges();

            TypedProxy.Account insertedAccount = client.Accounts.Where(a => a.Id == 4).Single();
            Assert.NotNull(insertedAccount);
            Assert.Equal("NickName4", insertedAccount.AccountInfo.NickName);
            Assert.Equal(40, insertedAccount.AccountInfo.Age);

            // Defect 2371564 odata.type is missed in client payload for dynamic enum type
            //Assert.Equal(WebStack.QA.Test.OData.OpenType.Typed.TypedProxy.Gender.Female, insertedAccount.AccountInfo.Gender);

            Assert.Equal("value 1", insertedAccount.Tags.Tag1);
            Assert.Equal("value 2", insertedAccount.Tags.Tag2);
            Assert.Equal("saxu", insertedAccount.OwnerAlias);
            Assert.Equal(2, insertedAccount.ShipAddresses.Count);
            Assert.Equal(2, insertedAccount.Emails.Count);
            Assert.Equal(TypedProxy.Gender.Male, insertedAccount.OwnerGender);
        }

        [Fact]
        public async Task InsertDerivedEntityWithOpenComplexTypePropertyClientTest()
        {
            await ResetDatasource();

            Uri serviceUrl = new Uri(this.BaseAddress + "/convention/");
            var client = new TypedProxy.Container(serviceUrl);

            client.Format.UseJson();

            TypedProxy.PremiumAccount newAccount = new TypedProxy.PremiumAccount()
            {
                Id = 4,
                Name = "Name4",
                AccountInfo = new TypedProxy.AccountInfo
                {
                    NickName = "NickName4",
                    Age = 40,
                    Gender = TypedProxy.Gender.Female,
                },
                Address = new TypedProxy.Address
                {
                    City = "Paris",
                    Street = "1 Microsoft Way",
                    Country = "France",
                },
                Tags = new TypedProxy.Tags
                {
                    Tag1 = "value 1",
                    Tag2 = "value 2"
                },
                OwnerAlias = "saxu",
                ShipAddresses = new List<TypedProxy.Address>()
                {
                    new  TypedProxy.Address
                    {
                        City = "Jinan",
                        Street = "Danling Street"
                    },
                    new  TypedProxy.Address
                    {
                        City="Nanjing",
                        Street="Zixing",
                    },
                
                },
                OwnerGender = TypedProxy.Gender.Male,
                Emails = new List<string>() { "a@a.com", "b@b.com" },
                LuckyNumbers = new List<int>() { 1, 2, 3 },
                Since = new DateTimeOffset(2014, 05, 23, 0, 0, 0, TimeSpan.FromHours(8)),
            };
            client.AddToAccounts(newAccount);
            client.SaveChanges();

            TypedProxy.PremiumAccount insertedAccount = client.Accounts.Where(a => a.Id == 4).Single() as TypedProxy.PremiumAccount;
            Assert.NotNull(insertedAccount);
            Assert.Equal("NickName4", insertedAccount.AccountInfo.NickName);
            Assert.Equal(40, insertedAccount.AccountInfo.Age);

            // Defect 2371564 odata.type is missed in client payload for dynamic enum type
            //Assert.Equal(WebStack.QA.Test.OData.OpenType.Typed.TypedProxy.Gender.Female, insertedAccount.AccountInfo.Gender);

            Assert.Equal("value 1", insertedAccount.Tags.Tag1);
            Assert.Equal("value 2", insertedAccount.Tags.Tag2);
            Assert.Equal("saxu", insertedAccount.OwnerAlias);
            Assert.Equal(2, insertedAccount.ShipAddresses.Count);
            Assert.Equal(2, insertedAccount.Emails.Count);
            Assert.Equal(TypedProxy.Gender.Male, insertedAccount.OwnerGender);
            Assert.Equal(new DateTimeOffset(2014, 05, 23, 0, 0, 0, TimeSpan.FromHours(8)), insertedAccount.Since);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task DeleteEntityWithOpenComplexTypePropertyClientTest()
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                Uri serviceUrl = new Uri(string.Format(this.BaseAddress + "/{0}/", routing));
                var client = new TypedProxy.Container(serviceUrl);

                var accountToDelete = client.Accounts.Where(a => a.Id == 1).Single();
                client.DeleteObject(accountToDelete);
                client.SaveChanges();

                var accounts = client.Accounts.ToList();
                Assert.Equal(2, accounts.Count);

                var queryDeletedAccount = accounts.Where(a => a.Id == 1).ToList();
                Assert.Equal(0, queryDeletedAccount.Count);
            }
        }

        #endregion

        #region Function & Action

        [Fact(Skip = "Potencially a client bug, and the owner is investigating it.")]
        public async Task GetAddressFunctionClientTest()
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                Uri serviceUrl = new Uri(string.Format(this.BaseAddress + "/{0}/", routing));
                var client = new TypedProxy.Container(serviceUrl);
                client.MergeOption = MergeOption.OverwriteChanges;

                client.Format.UseJson();

                var address = client.Accounts.Where(a => a.Id == 1).Single().GetAddressFunction().GetValue();

                Assert.Equal("Redmond", address.City);
                Assert.Equal("US", address.Country);
            }
        }

        [Fact]
        public async Task GetShipAddressesFunctionClientTest()
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                Uri serviceUrl = new Uri(string.Format(this.BaseAddress + "/{0}/", routing));
                var client = new TypedProxy.Container(serviceUrl);
                client.MergeOption = MergeOption.OverwriteChanges;

                client.Format.UseJson();

                IEnumerable<TypedProxy.Address> addresses = client.Accounts.Where(a => a.Id == 1).Single().GetShipAddresses();

                Assert.Equal(2, addresses.Count());
            }
        }

        [Fact]
        public async Task IncreaseAgeActionClientTest2()
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                Uri serviceUrl = new Uri(string.Format(this.BaseAddress + "/{0}/", routing));
                var client = new TypedProxy.Container(serviceUrl);
                client.MergeOption = MergeOption.OverwriteChanges;

                client.Format.UseJson();

                client.Accounts.Where(a => a.Id == 1).Single().IncreaseAgeAction().GetValue();

                var account = client.Accounts.Where(a => a.Id == 1).Single();
                Assert.Equal(11, account.AccountInfo.Age);
            }
        }

        [Fact]
        public async Task AddShipAddressActionClientTest()
        {
            foreach (string routing in Routings)
            {
                await ResetDatasource();

                Uri serviceUrl = new Uri(string.Format(this.BaseAddress + "/{0}/", routing));
                var client = new TypedProxy.Container(serviceUrl);
                client.MergeOption = MergeOption.OverwriteChanges;

                client.Format.UseJson();

                TypedProxy.Address shipAddress = new TypedProxy.Address()
                {
                    City = "Hang zhou",
                    Street = "Anything",
                };
                int shipAddressCount = client.Accounts.Where(a => a.Id == 1).Single().AddShipAddress(shipAddress).GetValue();

                Assert.Equal(3, shipAddressCount);
                shipAddressCount = client.Accounts.Where(a => a.Id == 1).Single().AddShipAddress(shipAddress).GetValue();

                Assert.Equal(4, shipAddressCount);
            }
        }

        [Fact]
        public async Task UpdateAddressActionClientTest()
        {
            await ResetDatasource();

            Uri serviceUrl = new Uri(string.Format(this.BaseAddress + "/AttributeRouting/"));
            var client = new TypedProxy.Container(serviceUrl);
            client.MergeOption = MergeOption.OverwriteChanges;

            TypedProxy.Address address = new TypedProxy.Address()
            {
                City = "New City",
                Street = "New Street",
                Country = "New Country"
            };
            client.UpdateAddressAction(address, 1).GetValue();

            var account = client.Accounts.Where(a => a.Id == 1).Single();
            Assert.Equal("New City", account.Address.City);
            Assert.Equal("New Country", account.Address.Country);
        }

        #endregion

        #endregion
        [Fact]
        public async Task QueryBaseEntityType()
        {
            await ResetDatasource();

            Uri serviceUrl = new Uri(this.BaseAddress + "/AttributeRouting/");
            var client = new TypedProxy.Container(serviceUrl);
            client.Format.UseJson();

            var employees = client.Employees.ToList();

            int expectedInt = 2;
            int actualInt = employees.Count();

            Assert.True(expectedInt == actualInt,
                string.Format("Employee count is in-correct, expected: {0}, actual: {1}", expectedInt, actualInt));
        }

        [Fact]
        public async Task QueryDerivedEntityType()
        {
            await ResetDatasource();

            Uri serviceUrl = new Uri(this.BaseAddress + "/convention/");
            var client = new TypedProxy.Container(serviceUrl);
            client.Format.UseJson();

            var employees = client.Employees.OfType<TypedProxy.Manager>().ToList();

            int expectedInt = 1;
            int actualInt = employees.Count();

            Assert.True(expectedInt == actualInt,
                string.Format("Manager count is in-correct, expected: {0}, actual: {1}", expectedInt, actualInt));

            TypedProxy.Manager manager = employees.Single(e => e.Id == 2);
            expectedValueOfNullableInt = 1;
            actualValueOfNullableInt = manager.Level;
            Assert.True(expectedValueOfNullableInt == actualValueOfNullableInt,
                string.Format("Manager Level is in-correct, expected: {0}, actual: {1}", expectedValueOfNullableInt, actualValueOfNullableInt));
            TypedProxy.Gender? gender = manager.Gender;
            Assert.Equal(TypedProxy.Gender.Male, gender);
            expectedValueOfInt = 2;
            actualValueOfInt = manager.PhoneNumbers.Count();
            Assert.True(expectedValueOfInt == actualValueOfInt,
                string.Format("Manager count is in-correct, expected: {0}, actual: {1}", expectedValueOfInt, actualValueOfInt));
        }

        [Fact]
        public async Task ExpandOpenEntityType()
        {
            await ResetDatasource();

            Uri serviceUrl = new Uri(this.BaseAddress + "/AttributeRouting/");
            var client = new TypedProxy.Container(serviceUrl);
            client.Format.UseJson();

            var employees = client.Employees.Expand("Account").ToList();

            expectedValueOfInt = 2;
            actualValueOfInt = employees.Count();

            Assert.True(expectedValueOfInt == actualValueOfInt,
                string.Format("Employee count is in-correct, expected: {0}, actual: {1}", expectedValueOfInt, actualValueOfInt));

            TypedProxy.Account account = employees.Single(e => e.Id == 1).Account;
            Assert.Equal(TypedProxy.Gender.Female, account.OwnerGender);
            Assert.Equal("jinfutan", account.OwnerAlias);
            Assert.Equal(true, account.IsValid);
            Assert.Equal(2, account.ShipAddresses.Count);
        }

        [Fact]
        public async Task InsertBaseEntity()
        {
            await ResetDatasource();

            Uri serviceUrl = new Uri(this.BaseAddress + "/AttributeRouting/");
            var client = new TypedProxy.Container(serviceUrl);
            client.Format.UseJson();
            string newName = "Name10";
            TypedProxy.Employee newEmployee = new TypedProxy.Employee() { Id = 0, Name = newName };
            client.AddToEmployees(newEmployee);
            client.SaveChanges();

            var insertedEmplyee = client.Employees.Where(e => e.Id >= 3).Single();
            expectedValueOfString = newName;
            actualValueOfString = insertedEmplyee.Name;

            Assert.True(expectedValueOfString == actualValueOfString,
               string.Format("Employee count is in-correct, expected: {0}, actual: {1}", expectedValueOfString, actualValueOfString));
        }

        // POST ~/Employees
        [Fact]
        public async Task InsertDerivedEntityType()
        {
            await ResetDatasource();

            Uri serviceUrl = new Uri(this.BaseAddress + "/convention/");
            var client = new TypedProxy.Container(serviceUrl);
            client.Format.UseJson();

            TypedProxy.Manager manager = new TypedProxy.Manager { Id = 0, Heads = 1 };
            manager.Level = 2;
            manager.Gender = TypedProxy.Gender.Male;
            manager.PhoneNumbers = new List<string>() { "8621-9999-8888" };
            client.AddToEmployees(manager);
            client.SaveChanges();

            var employees = client.Employees.OfType<TypedProxy.Manager>().ToList();
            var insertedManager = employees.Where(m => m.Id == 3).Single();

            expectedValueOfNullableInt = 2;
            actualValueOfNullableInt = insertedManager.Level;
            Assert.True(expectedValueOfNullableInt == actualValueOfNullableInt,
                string.Format("Manager Level is in-correct, expected: {0}, actual: {1}", expectedValueOfNullableInt, actualValueOfNullableInt));
            TypedProxy.Gender? gender = manager.Gender;
            Assert.Equal(TypedProxy.Gender.Male, gender);
            expectedValueOfInt = 1;
            actualValueOfInt = manager.PhoneNumbers.Count();
            Assert.True(expectedValueOfInt == actualValueOfInt,
                string.Format("Manager PhoneNumbers count is in-correct, expected: {0}, actual: {1}", expectedValueOfInt, actualValueOfInt));
        }

        [Fact]
        // PUT ~/Employees(1)/Namespace.Manager
        public async Task ReplaceDerivedEntityType()
        {
            await ResetDatasource();

            Uri serviceUrl = new Uri(this.BaseAddress + "/convention/");
            var client = new TypedProxy.Container(serviceUrl);
            client.MergeOption = MergeOption.OverwriteChanges;
            client.Format.UseJson();

            var employees = client.Employees.OfType<TypedProxy.Manager>().ToList();
            var manager = employees.Where(m => m.Id == 2).Single();

            manager.Level = 3;
            manager.Gender = TypedProxy.Gender.Male;
            manager.PhoneNumbers = new List<string>() { "8621-9999-8888", "2345", "4567" };
            client.UpdateObject(manager);
            client.SaveChanges(SaveChangesOptions.ReplaceOnUpdate);

            var managers = client.Employees.OfType<TypedProxy.Manager>().ToList();
            var updatedManager = managers.Where(m => m.Id == 2).Single();

            expectedValueOfNullableInt = 3;
            actualValueOfNullableInt = updatedManager.Level;
            Assert.True(expectedValueOfNullableInt == actualValueOfNullableInt,
                string.Format("Manager Level is in-correct, expected: {0}, actual: {1}", expectedValueOfNullableInt, actualValueOfNullableInt));
            TypedProxy.Gender? gender = manager.Gender;
            Assert.Equal(TypedProxy.Gender.Male, gender);

            actualValueOfInt = manager.PhoneNumbers.Count();
            expectedValueOfInt = 3;
            Assert.True(expectedValueOfInt == actualValueOfInt,
                string.Format("Manager PhoneNumbers count is in-correct, expected: {0}, actual: {1}", expectedValueOfInt, actualValueOfInt));
        }

        [Fact]
        // PATCH ~/Employees(1)/Namespace.Manager
        public async Task UpdateDerivedEntityType()
        {
            await ResetDatasource();

            Uri serviceUrl = new Uri(this.BaseAddress + "/AttributeRouting/");
            var client = new TypedProxy.Container(serviceUrl);
            client.MergeOption = MergeOption.OverwriteChanges;
            client.Format.UseJson();

            var manager = client.Employees.OfType<TypedProxy.Manager>().Where(m => m.Id == 2).Single();
            manager.Gender = null;
            manager.PhoneNumbers = new List<string>() { "8621-9999-8888", "2345", "4567", "5678" };
            client.UpdateObject(manager);
            client.SaveChanges();

            var updatedManager = client.Employees.OfType<TypedProxy.Manager>().Where(m => m.Id == 2).Single();

            expectedValueOfNullableInt = 1;
            actualValueOfNullableInt = updatedManager.Level;
            Assert.True(expectedValueOfNullableInt == actualValueOfNullableInt,
                string.Format("The manager's Level should not be changed, expected: {0}, actual: {1}", expectedValueOfNullableInt, actualValueOfNullableInt));
            TypedProxy.Gender? gender = manager.Gender;
            Assert.True(manager.Gender == null,
                string.Format("The manager's gender is updated to null, but actually it is {0})", manager.Gender));

            actualValueOfInt = manager.PhoneNumbers.Count();
            expectedValueOfInt = 4;
            Assert.True(expectedValueOfInt == actualValueOfInt,
                string.Format("Manager PhoneNumbers count is in-correct, expected: {0}, actual: {1}", expectedValueOfInt, actualValueOfInt));
        }

        [Fact(Skip = "Used to generate csdl file")]
        public void GetMetadata()
        {
            var directory = Directory.GetCurrentDirectory();
            var strArray = directory.Split(new string[] { "bin" }, StringSplitOptions.None);
            var filePath = Path.Combine(strArray[0], @"src\WebStack.QA.Test.OData\OpenType\TypedMetadata.csdl.xml");

            IEdmModel edmModel = OpenComplexTypeEdmModel.GetTypedConventionModel();
            XmlWriterSettings setting = new XmlWriterSettings();
            setting.Indent = true;
            setting.NewLineOnAttributes = false;
            XmlWriter xmlWriter = XmlWriter.Create(filePath, setting);
            IEnumerable<Microsoft.OData.Edm.Validation.EdmError> errors;
            CsdlWriter.TryWriteCsdl(edmModel, xmlWriter, CsdlTarget.EntityFramework, out errors);
            xmlWriter.Flush();
            xmlWriter.Close();
            //Microsoft.OData.Client.Design.T4.ODataT4CodeGenerator t4CodeGenerator = new ODataT4CodeGenerator
            // {
            //     Edmx = edmx,
            //     NamespacePrefix = referenceName,
            //     TargetLanguage = languageOption,
            //     UseDataServiceCollection = useDataServiceCollection,
            // };

            // return t4CodeGenerator.TransformText();
            Assert.True(0 == errors.Count(), "FAiled to generate the csdl file");
        }
        private async Task<HttpResponseMessage> ResetDatasource()
        {
            var uriReset = this.BaseAddress + "/AttributeRouting/ResetDataSource";
            var response = await this.Client.PostAsync(uriReset, null);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            return response;
        }
    }
}