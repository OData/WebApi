using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData;
using System.Web.OData.Extensions;
using Nuwa;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.AlternateKeys
{
    [NuwaFramework]
    [NuwaTrace(NuwaTraceAttribute.Tag.Off)]
    public class AlternateKeysTest
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[]
            {
                typeof (CustomersController), typeof (OrdersController), typeof (PeopleController),
                typeof (CompaniesController), typeof (MetadataController)
            };

            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Services.Replace(typeof (IAssembliesResolver), resolver);

            configuration.EnableAlternateKeys(true);

            configuration.Routes.Clear();

            configuration.MapODataServiceRoute("odata", "odata", model: AlternateKeysEdmModel.GetEdmModel());

            configuration.EnsureInitialized();
        }

        [Fact]
        public async Task AlteranteKeysMetadata()
        {
            const string expect = @"<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityType Name=""Customer"">
        <Key>
          <PropertyRef Name=""ID"" />
        </Key>
        <Property Name=""ID"" Type=""Edm.Int32"" />
        <Property Name=""Name"" Type=""Edm.String"" />
        <Property Name=""SSN"" Type=""Edm.String"" />
        <Annotation Term=""OData.Community.Keys.V1.AlternateKeys"">
          <Collection>
            <Record Type=""OData.Community.Keys.V1.AlternateKey"">
              <PropertyValue Property=""Key"">
                <Collection>
                  <Record Type=""OData.Community.Keys.V1.PropertyRef"">
                    <PropertyValue Property=""Alias"" String=""SSN"" />
                    <PropertyValue Property=""Name"" PropertyPath=""SSN"" />
                  </Record>
                </Collection>
              </PropertyValue>
            </Record>
          </Collection>
        </Annotation>
      </EntityType>
      <EntityType Name=""Order"">
        <Key>
          <PropertyRef Name=""OrderId"" />
        </Key>
        <Property Name=""OrderId"" Type=""Edm.Int32"" />
        <Property Name=""Name"" Type=""Edm.String"" />
        <Property Name=""Token"" Type=""Edm.Guid"" />
        <Property Name=""Amount"" Type=""Edm.Int32"" />
        <Annotation Term=""OData.Community.Keys.V1.AlternateKeys"">
          <Collection>
            <Record Type=""OData.Community.Keys.V1.AlternateKey"">
              <PropertyValue Property=""Key"">
                <Collection>
                  <Record Type=""OData.Community.Keys.V1.PropertyRef"">
                    <PropertyValue Property=""Alias"" String=""Name"" />
                    <PropertyValue Property=""Name"" PropertyPath=""Name"" />
                  </Record>
                </Collection>
              </PropertyValue>
            </Record>
            <Record Type=""OData.Community.Keys.V1.AlternateKey"">
              <PropertyValue Property=""Key"">
                <Collection>
                  <Record Type=""OData.Community.Keys.V1.PropertyRef"">
                    <PropertyValue Property=""Alias"" String=""Token"" />
                    <PropertyValue Property=""Name"" PropertyPath=""Token"" />
                  </Record>
                </Collection>
              </PropertyValue>
            </Record>
          </Collection>
        </Annotation>
      </EntityType>
      <EntityType Name=""Person"">
        <Key>
          <PropertyRef Name=""ID"" />
        </Key>
        <Property Name=""ID"" Type=""Edm.Int32"" />
        <Property Name=""Country"" Type=""Edm.String"" />
        <Property Name=""Passport"" Type=""Edm.String"" />
        <Annotation Term=""OData.Community.Keys.V1.AlternateKeys"">
          <Collection>
            <Record Type=""OData.Community.Keys.V1.AlternateKey"">
              <PropertyValue Property=""Key"">
                <Collection>
                  <Record Type=""OData.Community.Keys.V1.PropertyRef"">
                    <PropertyValue Property=""Alias"" String=""Country"" />
                    <PropertyValue Property=""Name"" PropertyPath=""Country"" />
                  </Record>
                  <Record Type=""OData.Community.Keys.V1.PropertyRef"">
                    <PropertyValue Property=""Alias"" String=""Passport"" />
                    <PropertyValue Property=""Name"" PropertyPath=""Passport"" />
                  </Record>
                </Collection>
              </PropertyValue>
            </Record>
          </Collection>
        </Annotation>
      </EntityType>
      <ComplexType Name=""Address"">
        <Property Name=""Street"" Type=""Edm.String"" />
        <Property Name=""City"" Type=""Edm.String"" />
      </ComplexType>
      <EntityType Name=""Company"">
        <Key>
          <PropertyRef Name=""ID"" />
        </Key>
        <Property Name=""ID"" Type=""Edm.Int32"" />
        <Property Name=""Location"" Type=""NS.Address"" />
        <Annotation Term=""OData.Community.Keys.V1.AlternateKeys"">
          <Collection>
            <Record Type=""OData.Community.Keys.V1.AlternateKey"">
              <PropertyValue Property=""Key"">
                <Collection>
                  <Record Type=""OData.Community.Keys.V1.PropertyRef"">
                    <PropertyValue Property=""Alias"" String=""City"" />
                    <PropertyValue Property=""Name"" PropertyPath=""City"" />
                  </Record>
                  <Record Type=""OData.Community.Keys.V1.PropertyRef"">
                    <PropertyValue Property=""Alias"" String=""Street"" />
                    <PropertyValue Property=""Name"" PropertyPath=""Street"" />
                  </Record>
                </Collection>
              </PropertyValue>
            </Record>
          </Collection>
        </Annotation>
      </EntityType>
      <EntityContainer Name=""Default"">
        <EntitySet Name=""Customers"" EntityType=""NS.Customer"" />
        <EntitySet Name=""Orders"" EntityType=""NS.Order"" />
        <EntitySet Name=""People"" EntityType=""NS.Person"" />
        <EntitySet Name=""Companies"" EntityType=""NS.Company"" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
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
            string expect = @"{
  ""@odata.context"":""{XXXX}/odata/$metadata#Edm.String"",""value"":""special-SSN""
}".Replace("{XXXX}", BaseAddress.ToLowerInvariant());

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
        [PropertyData("SingleAlternateKeysCases")]
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
            requestUri = string.Format("{0}/odata/People(Country='United States',Passport='9999')", this.BaseAddress);
            response = await Client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            string composedResponse = await response.Content.ReadAsStringAsync();

            Assert.Equal(primitiveResponse, composedResponse);
        }

        [Fact]
        public async Task QueryFailedIfMissingAnyOfComposedAlternateKeys()
        {
            var requestUri = string.Format("{0}/odata/People(Country='United States')", this.BaseAddress);
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

            requestUri = string.Format("{0}/odata/People(Country='United States',Passport='9999')", this.BaseAddress);
            response = await Client.GetAsync(requestUri);
            responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(requestUri);
            Console.WriteLine(responseContent);
        }
         * */

        [Fact]
        public async Task CanUpdateEntityWithSingleAlternateKeys()
        {
            string expect = @"{
  ""@odata.context"":""{XXXX}/odata/$metadata#Customers/$entity"",""ID"":6,""Name"":""Updated Customer Name"",""SSN"":""SSN-6-T-006""
}".Replace("{XXXX}", BaseAddress.ToLowerInvariant());

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
