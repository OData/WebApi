﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;
using HttpClientExtensions = System.Net.Http.HttpClientExtensions;

namespace WebStack.QA.Test.OData.Singleton
{
    [NuwaFramework]
    public class SingletonTest
    {
        private const string NameSpace = "WebStack.QA.Test.OData.Singleton";

        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] { typeof(UmbrellaController), typeof(MonstersIncController), typeof(MetadataController), typeof(PartnersController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("ModelBuilderWithConventionRouting", "expCon", SingletonEdmModel.GetExplicitModel("Umbrella"), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute("ModelBuilderWithAttributeRouting", "expAttr", SingletonEdmModel.GetExplicitModel("MonstersInc"));
            configuration.MapODataServiceRoute("ConventionBuilderwithConventionRouting", "conCon", SingletonEdmModel.GetConventionModel("Umbrella"), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute("ConventionBuilderwithAttributeRouting", "conAttr", SingletonEdmModel.GetConventionModel("MonstersInc"));
            configuration.EnsureInitialized();
        }

        #region Test

        [Theory]
        [InlineData("expCon", "Umbrella")]
        [InlineData("expAttr", "MonstersInc")]
        [InlineData("conCon", "Umbrella")]
        [InlineData("conAttr", "MonstersInc")]
        public async Task SingletonShouldShowInServiceDocument(string model, string singletonName)
        {
            string requestUri = string.Format(this.BaseAddress + "/{0}", model);

            HttpResponseMessage response = this.Client.GetAsync(requestUri).Result;
            var json = await response.Content.ReadAsAsync<JObject>();
            var result = json["value"] as JArray;

            foreach (var r in result)
            {
                if ((string)r["name"] == singletonName)
                {
                    Assert.Equal("Singleton", (string)r["kind"]);
                }
            }
        }

        [Theory]
        [InlineData("expCon", "Umbrella/Partners/$count")]
        [InlineData("conCon", "Umbrella/Partners/$count")]
        public async Task NotCountable(string model, string url)
        {
            // Arrange
            await ResetDataSource(model, "Umbrella");

            string requestUri = string.Format("{0}/{1}/{2}", this.BaseAddress, model, url);

            // Act
            HttpResponseMessage response = await this.Client.GetAsync(requestUri);

            // Assert
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.BadRequest == response.StatusCode, string.Format(
                @"The response code is incorrect, expanded: 400, but actually: {0}, request url: {1}, response: {2}.",
                response.StatusCode,
                requestUri,
                contentOfString));
            JObject content = JObject.Parse(contentOfString);
            Assert.True(content["error"]["message"].Value<string>() ==
                "The query specified in the URI is not valid. The property 'Partners' cannot be used for $count.");
        }

        [Theory]
        [InlineData("expAttr/MonstersInc/Branches/$count", 2)]
        [InlineData("conAttr/MonstersInc/Branches/$count?$filter=City eq 'Shanghai'", 1)]
        public async Task QueryBranchesCount(string url, int expectedCount)
        {
            // Arrange
            await ResetDataSource("expAttr", "MonstersInc");
            await ResetDataSource("conAttr", "MonstersInc");

            var requestUri = string.Format("{0}/{1}", this.BaseAddress, url);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(expectedCount == int.Parse(responseString),
                string.Format("Expected: {0}; Actual: {1}; Request URL: {2}", expectedCount, responseString, requestUri));
        }

        [Theory]
        [InlineData("expCon", "Umbrella")]
        [InlineData("expAttr", "MonstersInc")]
        [InlineData("conCon", "Umbrella")]
        [InlineData("conAttr", "MonstersInc")]
        public async Task SingletonCRUD(string model, string singletonName)
        {
            string requestUri = string.Format(this.BaseAddress + "/{0}/{1}", model, singletonName);

            // Reset data source
            await ResetDataSource(model, singletonName);
            await ResetDataSource(model, "Partners");

            // GET singleton
            HttpResponseMessage response = this.Client.GetAsync(requestUri).Result;
            dynamic result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            // PUT singleton with {"ID":1,"Name":"singletonName","Revenue":2000,"Category":"IT"}
            result["Revenue"] = 2000;
            response = await HttpClientExtensions.PutAsJsonAsync(this.Client, requestUri, result);
            response.EnsureSuccessStatusCode();

            // GET singleton/Revenue
            response = this.Client.GetAsync(requestUri + "/Revenue").Result;
            result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(2000, (int)result["value"]);

            // PATCH singleton with {"@odata.type":"#WebStack.QA.Test.OData.Singleton.Company","Revenue":3000}
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            request.Content = new StringContent(string.Format(@"{{""@odata.type"":""#{0}"",""Revenue"":3000}}", typeof(Company)));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            response = Client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();

            // GET singleton
            response = this.Client.GetAsync(requestUri).Result;
            result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(3000, (int)result["Revenue"]);

            // Negative: Add singleton
            // POST singleton
            var company = new Company();
            var content = new ObjectContent(company.GetType(), company, new JsonMediaTypeFormatter());
            response = this.Client.PostAsync(requestUri, content).Result;
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            // Negative: Delete singleton
            // DELETE singleton
            response = this.Client.DeleteAsync(requestUri).Result;
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Navigation link CRUD, singleton is navigation source and entityset is navigation target
        [Theory]
        [InlineData("expCon", "Umbrella")]
        [InlineData("expAttr", "MonstersInc")]
        [InlineData("conCon", "Umbrella")]
        [InlineData("conAttr", "MonstersInc")]
        public async Task SingletonNavigationLinkCRUD(string model, string singletonName)
        {
            string requestUri = string.Format(this.BaseAddress + "/{0}/{1}/Partners", model, singletonName);

            // Reset data source
            await ResetDataSource(model, singletonName);
            await ResetDataSource(model, "Partners");

            // GET singleton/Partners
            HttpResponseMessage response = this.Client.GetAsync(requestUri).Result;
            var json = await response.Content.ReadAsAsync<JObject>();
            var result = json.GetValue("value") as JArray;
            Assert.Equal<int>(0, result.Count);

            string navigationLinkUri = string.Format(requestUri + "/$ref");

            // POST singleton/Partners/$ref
            string idLinkBase = string.Format(this.BaseAddress + "/{0}/Partners", model);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, navigationLinkUri);
            request.Content = new StringContent("{ \"@odata.id\" : \"" + idLinkBase + "(1)\"}");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            response = Client.SendAsync(request).Result;
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // POST singleton/Partners/$ref
            request = new HttpRequestMessage(HttpMethod.Post, navigationLinkUri);
            request.Content = new StringContent("{ \"@odata.id\" : \"" + idLinkBase + "(2)\"}");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            response = Client.SendAsync(request).Result;
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // POST singleton/Partners/$ref
            request = new HttpRequestMessage(HttpMethod.Post, navigationLinkUri);
            request.Content = new StringContent("{ \"@odata.id\" : \"" + idLinkBase + "(3)\"}");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            response = Client.SendAsync(request).Result;
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // GET singleton/Partners
            response = this.Client.GetAsync(requestUri).Result;
            json = await response.Content.ReadAsAsync<JObject>();
            result = json.GetValue("value") as JArray;
            Assert.Equal<int>(3, result.Count);

            // Add Partner to Company by "Deep Insert"
            // POST singleton/Partners
            Partner partner = new Partner() { ID = 100, Name = "NewHire" };
            request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new ObjectContent(partner.GetType(), partner, new JsonMediaTypeFormatter());
            response = Client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();

            // GET singleton/Partners
            response = this.Client.GetAsync(requestUri).Result;
            json = await response.Content.ReadAsAsync<JObject>();
            result = json.GetValue("value") as JArray;
            Assert.Equal<int>(4, result.Count);

            // Unrelate Partners(3) from Company
            // DELETE singleton/Partners(3)/$ref
            request = new HttpRequestMessage(HttpMethod.Delete, requestUri + "(3)/$ref");
            response = Client.SendAsync(request).Result;
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // GET singleton/Partners
            response = this.Client.GetAsync(requestUri).Result;
            json = await response.Content.ReadAsAsync<JObject>();
            result = json.GetValue("value") as JArray;
            Assert.Equal<int>(3, result.Count);

            // GET singleton/WebStack.QA.Test.OData.Singleton.GetPartnersCount()
            requestUri = string.Format(BaseAddress + "/{0}/{1}/{2}.GetPartnersCount()", model, singletonName, NameSpace);
            response = this.Client.GetAsync(requestUri).Result;
            json = await response.Content.ReadAsAsync<JObject>();
            Assert.Equal(3, (int)json["value"]);
        }

        // Navigation link CRUD, where entityset is navigation source and singleton is navigation target
        [Theory]
        [InlineData("expAttr", "application/json;odata.metadata=full")]
        [InlineData("conAttr", "application/json;odata.metadata=minimal")]
        public async Task EntitySetNavigationLinkCRUD(string model, string format)
        {
            string requestUri = string.Format(this.BaseAddress + "/{0}/Partners(1)/Company", model);
            string navigationUri = requestUri + "/$ref";
            string formatQuery = string.Format("?$format={0}", format);

            //Reset data source
            await ResetDataSource(model, "MonstersInc");
            await ResetDataSource(model, "Partners");

            // PUT Partners(1)/Company/$ref
            string idLinkBase = string.Format(this.BaseAddress + "/{0}/MonstersInc", model);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, navigationUri);
            request.Content = new StringContent("{ \"@odata.id\" : \"" + idLinkBase + "\"}");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            var response = Client.SendAsync(request).Result;
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // GET Partners(1)/Company
            response = this.Client.GetAsync(requestUri).Result;
            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal("MonstersInc", (string)result["Name"]);

            // PUT Partners(1)/Company
            result["Revenue"] = 2000;
            response = await HttpClientExtensions.PutAsJsonAsync(this.Client, requestUri, result);
            response.EnsureSuccessStatusCode();

            // GET Partners(1)/Company/Revenue
            response = this.Client.GetAsync(requestUri + formatQuery).Result;
            result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(2000, (int)result["Revenue"]);

            // PATCH Partners(1)/Company
            request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            request.Content = new StringContent(string.Format(@"{{""@odata.type"":""#{0}"",""Revenue"":3000}}", typeof(Company)));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            response = Client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();

            // GET Partners(1)/Company/Revenue
            response = this.Client.GetAsync(requestUri + formatQuery).Result;
            result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(3000, (int)result["Revenue"]);

            // DELETE Partners(1)/Company/$ref
            response = Client.DeleteAsync(navigationUri).Result;
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // GET Partners(1)/Company
            response = this.Client.GetAsync(requestUri + formatQuery).Result;
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            // Negative: POST Partners(1)/Company
            var company = new Company();
            var content = new ObjectContent(company.GetType(), company, new JsonMediaTypeFormatter());
            response = this.Client.PostAsync(requestUri, content).Result;
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("expCon", "Umbrella", "application/json;odata.metadata=full")]
        [InlineData("expAttr", "MonstersInc", "application/json;odata.metadata=minimal")]
        [InlineData("conCon", "Umbrella", "application/json;odata.metadata=full")]
        [InlineData("conAttr", "MonstersInc", "application/json;odata.metadata=minimal")]
        public async Task SingletonDerivedTypeTest(string model, string singletonName, string format)
        {
            string requestUri = string.Format(this.BaseAddress + "/{0}/{1}/{2}.SubCompany", model, singletonName, NameSpace);
            string formatQuery = string.Format("$format={0}", format);

            await ResetDataSource(model, singletonName);

            // PUT singleton/WebStack.QA.Test.OData.Singleton.SubCompany
            var company = new { ID = 100, Name = "UmbrellaInSouthPole", Category = CompanyCategory.Communication.ToString(), Revenue = 1000, Location = "South Pole", Description = "The Umbrella In South Pole", Partners = new List<Partner>(), Branches = new List<Office>(), Office = new Office() { City = "South", Address = "999" } };
            var request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            request.Content = new StringContent(JsonConvert.SerializeObject(company));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var response = Client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();

            // GET singleton/WebStack.QA.Test.OData.Singleton.SubCompany/Location
            response = this.Client.GetAsync(requestUri + "/Location?" + formatQuery).Result;
            var result = response.Content.ReadAsAsync<JObject>().Result;
            Assert.Equal(company.Location, (string)result["value"]);

            // Query complex type
            // GET GET singleton/WebStack.QA.Test.OData.Singleton.SubCompany/Office
            response = this.Client.GetAsync(requestUri + "/Office?" + formatQuery).Result;
            result = response.Content.ReadAsAsync<JObject>().Result;
            Assert.Equal(company.Office.City, (string)result["City"]);

            // GET singleton/WebStack.QA.Test.OData.Singleton.SubCompany?$select=Location
            response = this.Client.GetAsync(requestUri + "?$select=Location&" + formatQuery).Result;
            result = response.Content.ReadAsAsync<JObject>().Result;
            Assert.Equal(company.Location, (string)result["Location"]);
        }

        [Theory]
        [InlineData("expCon", "Umbrella")]
        [InlineData("expAttr", "MonstersInc")]
        [InlineData("conCon", "Umbrella")]
        [InlineData("conAttr", "MonstersInc")]
        public async Task SingletonQueryOptionsTest(string model, string singletonName)
        {
            string requestUri = string.Format(this.BaseAddress + "/{0}/{1}", model, singletonName);

            await ResetDataSource(model, singletonName);

            // GET /singleton?$select=Name
            var response = this.Client.GetAsync(requestUri + "?$select=Name").Result;
            var result = response.Content.ReadAsAsync<JObject>().Result;
            int i = 0;
            foreach (var pro in result.Properties())
            {
                i++;
            }
            Assert.Equal(2, i);

            // POST /singleton/Partners
            Partner partner = new Partner() { ID = 100, Name = "NewHire" };
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri + "/Partners");
            request.Content = new ObjectContent(partner.GetType(), partner, new JsonMediaTypeFormatter());
            response = Client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();

            // POST /singleton/Partners
            partner = new Partner() { ID = 101, Name = "NewHire2" };
            request = new HttpRequestMessage(HttpMethod.Post, requestUri + "/Partners");
            request.Content = new ObjectContent(partner.GetType(), partner, new JsonMediaTypeFormatter());
            response = Client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();

            // GET /singleton?$expand=Partners($select=Name)
            response = this.Client.GetAsync(requestUri + "?$expand=Partners($select=Name)").Result;
            result = await response.Content.ReadAsAsync<JObject>();
            var json = result.GetValue("Partners") as JArray;
            Assert.Equal(2, json.Count);

            // PUT Partners(1)/Company/$ref
            var navigationUri = string.Format(this.BaseAddress + "/{0}/Partners(1)/Company/$ref", model);
            string idLinkBase = string.Format(this.BaseAddress + "/{0}/{1}", model, singletonName);
            request = new HttpRequestMessage(HttpMethod.Put, navigationUri);
            request.Content = new StringContent("{ \"@odata.id\" : \"" + idLinkBase + "\"}");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            response = Client.SendAsync(request).Result;

            // GET /Partners(1)?$expand=Company($select=Name)
            requestUri = string.Format(this.BaseAddress + "/{0}/Partners(1)", model);
            response = this.Client.GetAsync(requestUri + "?$expand=Company($select=Name)").Result;
            result = await response.Content.ReadAsAsync<JObject>();
            var company = result.GetValue("Company") as JObject;
            Assert.Equal(singletonName, company.GetValue("Name"));
        }
        #endregion

        private async Task<HttpResponseMessage> ResetDataSource(string model, string controller)
        {
            var uriReset = string.Format(this.BaseAddress + "/{0}/{1}/{2}.ResetDataSource", model, controller, NameSpace);
            var response = await this.Client.PostAsync(uriReset, null);

            return response;
        }
    }
}
