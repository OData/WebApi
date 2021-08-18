//-----------------------------------------------------------------------------
// <copyright file="AlternateKeysTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.AlternateKeys
{
    public class AlternateKeysTest : WebHostTestBase
    {
        public AlternateKeysTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[]
            {
                typeof (CustomersController), typeof (OrdersController), typeof (PeopleController),
                typeof (CompaniesController), typeof (MetadataController)
            };

            configuration.AddControllers(controllers);

            IEdmModel model = AlternateKeysEdmModel.GetEdmModel();

            configuration.Routes.Clear();

            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);

            configuration.MapODataServiceRoute("odata", "odata",
                builder =>
                    builder.AddService(ServiceLifetime.Singleton, sp => model)
                        .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                            ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", configuration))
                        .AddService<ODataUriResolver>(ServiceLifetime.Singleton, sp => new AlternateKeysODataUriResolver(model)));

            configuration.EnsureInitialized();
        }

        [Fact]
        public async Task AlteranteKeysMetadata()
        {
            string expect = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
"<edmx:Edmx Version=\"4.0\" xmlns:edmx=\"http://docs.oasis-open.org/odata/ns/edmx\">\r\n" +
"  <edmx:DataServices>\r\n" +
"    <Schema Namespace=\"NS\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">\r\n" +
"      <EntityType Name=\"Customer\">\r\n" +
"        <Key>\r\n" +
"          <PropertyRef Name=\"ID\" />\r\n" +
"        </Key>\r\n" +
"        <Property Name=\"ID\" Type=\"Edm.Int32\" />\r\n" +
"        <Property Name=\"Name\" Type=\"Edm.String\" />\r\n" +
"        <Property Name=\"SSN\" Type=\"Edm.String\" />\r\n" +
"        <Annotation Term=\"OData.Community.Keys.V1.AlternateKeys\">\r\n" +
"          <Collection>\r\n" +
"            <Record Type=\"OData.Community.Keys.V1.AlternateKey\">\r\n" +
"              <PropertyValue Property=\"Key\">\r\n" +
"                <Collection>\r\n" +
"                  <Record Type=\"OData.Community.Keys.V1.PropertyRef\">\r\n" +
"                    <PropertyValue Property=\"Alias\" String=\"SSN\" />\r\n" +
"                    <PropertyValue Property=\"Name\" PropertyPath=\"SSN\" />\r\n" +
"                  </Record>\r\n" +
"                </Collection>\r\n" +
"              </PropertyValue>\r\n" +
"            </Record>\r\n" +
"          </Collection>\r\n" +
"        </Annotation>\r\n" +
"      </EntityType>\r\n" +
"      <EntityType Name=\"Order\">\r\n" +
"        <Key>\r\n" +
"          <PropertyRef Name=\"OrderId\" />\r\n" +
"        </Key>\r\n" +
"        <Property Name=\"OrderId\" Type=\"Edm.Int32\" />\r\n" +
"        <Property Name=\"Name\" Type=\"Edm.String\" />\r\n" +
"        <Property Name=\"Token\" Type=\"Edm.Guid\" />\r\n" +
"        <Property Name=\"Amount\" Type=\"Edm.Int32\" />\r\n" +
"        <Annotation Term=\"OData.Community.Keys.V1.AlternateKeys\">\r\n" +
"          <Collection>\r\n" +
"            <Record Type=\"OData.Community.Keys.V1.AlternateKey\">\r\n" +
"              <PropertyValue Property=\"Key\">\r\n" +
"                <Collection>\r\n" +
"                  <Record Type=\"OData.Community.Keys.V1.PropertyRef\">\r\n" +
"                    <PropertyValue Property=\"Alias\" String=\"Name\" />\r\n" +
"                    <PropertyValue Property=\"Name\" PropertyPath=\"Name\" />\r\n" +
"                  </Record>\r\n" +
"                </Collection>\r\n" +
"              </PropertyValue>\r\n" +
"            </Record>\r\n" +
"            <Record Type=\"OData.Community.Keys.V1.AlternateKey\">\r\n" +
"              <PropertyValue Property=\"Key\">\r\n" +
"                <Collection>\r\n" +
"                  <Record Type=\"OData.Community.Keys.V1.PropertyRef\">\r\n" +
"                    <PropertyValue Property=\"Alias\" String=\"Token\" />\r\n" +
"                    <PropertyValue Property=\"Name\" PropertyPath=\"Token\" />\r\n" +
"                  </Record>\r\n" +
"                </Collection>\r\n" +
"              </PropertyValue>\r\n" +
"            </Record>\r\n" +
"          </Collection>\r\n" +
"        </Annotation>\r\n" +
"      </EntityType>\r\n" +
"      <EntityType Name=\"Person\">\r\n" +
"        <Key>\r\n" +
"          <PropertyRef Name=\"ID\" />\r\n" +
"        </Key>\r\n" +
"        <Property Name=\"ID\" Type=\"Edm.Int32\" />\r\n" +
"        <Property Name=\"Country_Region\" Type=\"Edm.String\" />\r\n" +
"        <Property Name=\"Passport\" Type=\"Edm.String\" />\r\n" +
"        <Annotation Term=\"OData.Community.Keys.V1.AlternateKeys\">\r\n" +
"          <Collection>\r\n" +
"            <Record Type=\"OData.Community.Keys.V1.AlternateKey\">\r\n" +
"              <PropertyValue Property=\"Key\">\r\n" +
"                <Collection>\r\n" +
"                  <Record Type=\"OData.Community.Keys.V1.PropertyRef\">\r\n" +
"                    <PropertyValue Property=\"Alias\" String=\"Country_Region\" />\r\n" +
"                    <PropertyValue Property=\"Name\" PropertyPath=\"Country_Region\" />\r\n" +
"                  </Record>\r\n" +
"                  <Record Type=\"OData.Community.Keys.V1.PropertyRef\">\r\n" +
"                    <PropertyValue Property=\"Alias\" String=\"Passport\" />\r\n" +
"                    <PropertyValue Property=\"Name\" PropertyPath=\"Passport\" />\r\n" +
"                  </Record>\r\n" +
"                </Collection>\r\n" +
"              </PropertyValue>\r\n" +
"            </Record>\r\n" +
"          </Collection>\r\n" +
"        </Annotation>\r\n" +
"      </EntityType>\r\n" +
"      <ComplexType Name=\"Address\">\r\n" +
"        <Property Name=\"Street\" Type=\"Edm.String\" />\r\n" +
"        <Property Name=\"City\" Type=\"Edm.String\" />\r\n" +
"      </ComplexType>\r\n" +
"      <EntityType Name=\"Company\">\r\n" +
"        <Key>\r\n" +
"          <PropertyRef Name=\"ID\" />\r\n" +
"        </Key>\r\n" +
"        <Property Name=\"ID\" Type=\"Edm.Int32\" />\r\n" +
"        <Property Name=\"Location\" Type=\"NS.Address\" />\r\n" +
"        <Annotation Term=\"OData.Community.Keys.V1.AlternateKeys\">\r\n" +
"          <Collection>\r\n" +
"            <Record Type=\"OData.Community.Keys.V1.AlternateKey\">\r\n" +
"              <PropertyValue Property=\"Key\">\r\n" +
"                <Collection>\r\n" +
"                  <Record Type=\"OData.Community.Keys.V1.PropertyRef\">\r\n" +
"                    <PropertyValue Property=\"Alias\" String=\"City\" />\r\n" +
"                    <PropertyValue Property=\"Name\" PropertyPath=\"City\" />\r\n" +
"                  </Record>\r\n" +
"                  <Record Type=\"OData.Community.Keys.V1.PropertyRef\">\r\n" +
"                    <PropertyValue Property=\"Alias\" String=\"Street\" />\r\n" +
"                    <PropertyValue Property=\"Name\" PropertyPath=\"Street\" />\r\n" +
"                  </Record>\r\n" +
"                </Collection>\r\n" +
"              </PropertyValue>\r\n" +
"            </Record>\r\n" +
"          </Collection>\r\n" +
"        </Annotation>\r\n" +
"      </EntityType>\r\n" +
"      <EntityContainer Name=\"Default\">\r\n" +
"        <EntitySet Name=\"Customers\" EntityType=\"NS.Customer\" />\r\n" +
"        <EntitySet Name=\"Orders\" EntityType=\"NS.Order\" />\r\n" +
"        <EntitySet Name=\"People\" EntityType=\"NS.Person\" />\r\n" +
"        <EntitySet Name=\"Companies\" EntityType=\"NS.Company\" />\r\n" +
"      </EntityContainer>\r\n" +
"    </Schema>\r\n" +
"  </edmx:DataServices>\r\n" +
"</edmx:Edmx>";

            // Remove indentation
            expect = Regex.Replace(expect, @"\r\n\s*<", @"<");

            var requestUri = string.Format("{0}/odata/$metadata", this.BaseAddress);
            HttpResponseMessage response = await Client.GetAsync(requestUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();

            Assert.Equal(expect, responseContent);
        }

        [Fact]
        public async Task QueryEntityWithSingleAlternateKeysWorks()
        {
            // query with alternate keys
            string expect = "{" +
                            "\"@odata.context\":\"{XXXX}\",\"value\":\"special-SSN\"" +
                            "}";
            expect = expect.Replace("{XXXX}", string.Format("{0}/odata/$metadata#Edm.String", BaseAddress.ToLowerInvariant()));

            var requestUri = string.Format("{0}/odata/Customers(SSN='special-SSN')", this.BaseAddress);
            HttpResponseMessage response = await Client.GetAsync(requestUri);

            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();

            Assert.Equal(expect, responseContent);
        }

        public static TheoryDataSet<string, string> SingleAlternateKeysCases
        {
            get
            {
                var data = new TheoryDataSet<string, string>();
                for (int i = 1; i <= 5; i++)
                {
                    data.Add("Customers(" + i + ")", "Customers(SSN='SSN-" + i + "-" + (100 + i) + "')");
                }

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(SingleAlternateKeysCases))]
        public async Task EntityWithSingleAlternateKeys_ReturnsSame_WithPrimitiveKey(string declaredKeys, string alternatekeys)
        {
            // query with declared key
            var requestUri = string.Format("{0}/odata/{1}", this.BaseAddress, declaredKeys);
            HttpResponseMessage response = await Client.GetAsync(requestUri);

            response.EnsureSuccessStatusCode();
            string primitiveResponse = await response.Content.ReadAsStringAsync();

            // query with alternate key
            requestUri = string.Format("{0}/odata/{1}", this.BaseAddress, alternatekeys);
            response = await Client.GetAsync(requestUri);

            response.EnsureSuccessStatusCode();
            string alternatekeyResponse = await response.Content.ReadAsStringAsync();

            Assert.Equal(primitiveResponse, alternatekeyResponse);
        }

        [Fact]
        public async Task QueryEntityWithMultipleAlternateKeys_Returns_SameEntityWithPrimitiveKey()
        {
            // query with declared key
            var requestUri = string.Format("{0}/odata/Orders(2)", this.BaseAddress);
            HttpResponseMessage response = await Client.GetAsync(requestUri);

            response.EnsureSuccessStatusCode();
            string primitiveResponse = await response.Content.ReadAsStringAsync();

            // query with one alternate key
            requestUri = string.Format("{0}/odata/Orders(Name='Order-2')", this.BaseAddress);
            response = await Client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            string nameResponse = await response.Content.ReadAsStringAsync();

            // query with another alternate key
            requestUri = string.Format("{0}/odata/Orders(Token=75036B94-C836-4946-8CC8-054CF54060EC)", this.BaseAddress);
            response = await Client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            string tokenResponse = await response.Content.ReadAsStringAsync();

            Assert.Equal(primitiveResponse, nameResponse);
            Assert.Equal(primitiveResponse, tokenResponse);
        }

        [Fact]
        public async Task QueryEntityWithComposedAlternateKeys_Returns_SameEntityWithPrimitiveKey()
        {
            // query with declared key
            var requestUri = string.Format("{0}/odata/People(2)", this.BaseAddress);
            HttpResponseMessage response = await Client.GetAsync(requestUri);

            response.EnsureSuccessStatusCode();
            string primitiveResponse = await response.Content.ReadAsStringAsync();

            // query with composed alternate keys
            requestUri = string.Format("{0}/odata/People(Country_Region='United States',Passport='9999')", this.BaseAddress);
            response = await Client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            string composedResponse = await response.Content.ReadAsStringAsync();

            Assert.Equal(primitiveResponse, composedResponse);
        }

        [Fact]
        public async Task QueryFailedIfMissingAnyOfComposedAlternateKeys()
        {
            var requestUri = string.Format("{0}/odata/People(Country_Region='United States')", this.BaseAddress);
            var response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /* Not supported now: see github issue: https://github.com/OData/odata.net/issues/294
         * if supported. modify the following test
        [Fact]
        public async Task QueryEntityWithComplexPropertyAlternateKeys_Returns_SameEntityWithPrimitiveKey()
        {
            var requestUri = string.Format("{0}/odata/Companies(2)", this.BaseAddress);
            HttpResponseMessage response = await Client.GetAsync(requestUri);

            Console.WriteLine(response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(requestUri);
            Console.WriteLine(responseContent);

            requestUri = string.Format("{0}/odata/People(Country_Region='United States',Passport='9999')", this.BaseAddress);
            response = await Client.GetAsync(requestUri);
            responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(requestUri);
            Console.WriteLine(responseContent);
        }
         * */

        [Fact]
        public async Task CanUpdateEntityWithSingleAlternateKeys()
        {
            string expect = "{" +
                            "\"@odata.context\":\"{XXXX}\",\"ID\":6,\"Name\":\"Updated Customer Name\",\"SSN\":\"SSN-6-T-006\"" +
                            "}";
            expect = expect.Replace("{XXXX}", string.Format("{0}/odata/$metadata#Customers/$entity", BaseAddress.ToLowerInvariant()));

            var requestUri = string.Format("{0}/odata/Customers(SSN='SSN-6-T-006')", this.BaseAddress);

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            const string content = @"{'Name':'Updated Customer Name'}";
            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            HttpResponseMessage response = await Client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();

            Assert.Equal(expect, responseContent);
        }
    }
}
