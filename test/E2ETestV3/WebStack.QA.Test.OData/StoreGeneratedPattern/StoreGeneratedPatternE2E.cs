using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using Microsoft.Data.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using Xunit;

namespace WebStack.QA.Test.OData.StoreGeneratedPattern
{
    [NuwaFramework]
    public class StoreGeneratedPatternE2E
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.Routes.MapODataServiceRoute("odata", "odata", GetEdmModel(),
                new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<StoreGeneratedPatternCustomer>("StoreGeneratedPatternCustomers");
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task GetMetadataShouldIncludStoreGeneratedPatternAnnotations()
        {
            var getUri = this.BaseAddress + "/odata/$metadata";
            string metadataString;
            string expectedString1 = ":StoreGeneratedPattern=\"Identity\"";
            string expectedString2 = ":StoreGeneratedPattern=\"Computed\"";

            using (var response = await Client.GetAsync(getUri))
            {
                var stream = response.Content.ReadAsStreamAsync().Result;
                var reader = new StreamReader(stream);
                metadataString = reader.ReadToEnd();
            }

            Assert.Contains(expectedString1, metadataString);
            Assert.Contains(expectedString2, metadataString);
        }

        [Fact]
        public async Task CreateEntityWithStoreGeneratedProperty()
        {
            int num;
            const string expectedComputedProperty = "ComputedProperty";
            // Get all customers
            var getAllUri = this.BaseAddress + "/odata/StoreGeneratedPatternCustomers";
            using (var response = await Client.GetAsync(getAllUri))
            {
                Assert.True(response.IsSuccessStatusCode);
                var json = await response.Content.ReadAsAsync<JObject>();
                var result = json.GetValue("value") as JArray;
                num = result.Count;
            }

            // Post a new customer
            var postUri = this.BaseAddress + "/odata/StoreGeneratedPatternCustomers";
            var postCustomer = new StoreGeneratedPatternCustomer()
            {
                Name = "PostedName",
                ComputedProperty = "TestProperty"
            };
            var content = new ObjectContent(postCustomer.GetType(), postCustomer, new JsonMediaTypeFormatter());
            using (var response = await Client.PostAsync(postUri, content))
            {
                Assert.True(response.IsSuccessStatusCode);
                JObject result = response.Content.ReadAsAsync<JObject>().Result;
                var id = (int)result["Id"];
                var computedProperty = (string)result["ComputedProperty"];
                Assert.Equal(num, id);
                Assert.Equal(expectedComputedProperty, computedProperty);
            }

            // Get to verify
            var getUri = string.Format("{0}/odata/StoreGeneratedPatternCustomers({1})", this.BaseAddress, num);
            using (var response = await Client.GetAsync(getUri))
            {
                Assert.True(response.IsSuccessStatusCode);
                JObject result = response.Content.ReadAsAsync<JObject>().Result;
                var id = (int)result["Id"];
                var computedProperty = (string)result["ComputedProperty"];
                Assert.Equal(num, id);
                Assert.Equal(expectedComputedProperty, computedProperty);
            }
        }

        //[Fact]
        //[Trait("Category", "LocalOnly")]
        public async Task PutEntityWithStoreGeneratedProperty()
        {
            const string expectedComputedProperty = "ComputedProperty";
            const string expectedName = "ChangedName";

            // Put a customer
            var putUri = this.BaseAddress + "/odata/StoreGeneratedPatternCustomers(0)";
            var putCustomer = new StoreGeneratedPatternCustomer()
            {
                Id = 0,
                Name = expectedName,
                ComputedProperty = "TestProperty"
            };
            using (var response = await Client.PutAsJsonAsync(putUri, putCustomer))
            {
                Assert.True(response.IsSuccessStatusCode);
            }

            // Get to verify
            var getUri = this.BaseAddress + "/odata/StoreGeneratedPatternCustomers(0)";
            using (var response = await Client.GetAsync(getUri))
            {
                Assert.True(response.IsSuccessStatusCode);
                JObject result = response.Content.ReadAsAsync<JObject>().Result;
                var id = (int)result["Id"];
                var name = (string)result["Name"];
                var computedProperty = (string)result["ComputedProperty"];
                Assert.Equal(0, id);
                Assert.Equal(expectedName, name);
                Assert.Equal(expectedComputedProperty, computedProperty);
            }
        }

    }
}
