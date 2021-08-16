//-----------------------------------------------------------------------------
// <copyright file="ComplextTypeCollectionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class ComplextTypeCollectionTests : WebHostTestBase
    {
        public ComplextTypeCollectionTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.Routes.Clear();

            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<ComplextTypeCollectionTests_Person>("ComplextTypeCollectionTests_Persons");

            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("odataRoute", "odata", builder.GetEdmModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        [Fact]
        public async Task OrderByTest()
        {
            HttpResponseMessage response = await Client.GetAsync(BaseAddress + "/odata/ComplextTypeCollectionTests_Persons(1)/Addresses?$orderby=City");

            response.EnsureSuccessStatusCode();

            IEnumerable<string> expectedData =
                    ComplextTypeCollectionTests_PersonsController.Persons.Where(p => p.Id == 1).First().Addresses.OrderBy(a => a.City).Select(a => a.City);

            string responseContent = await response.Content.ReadAsStringAsync();

            JObject jo = JObject.Parse(responseContent);
            JArray addresses = jo["value"] as JArray;
            IEnumerable<string> actualData = addresses.Select(jt => jt["City"].ToString());

            Assert.Equal(expectedData, actualData);
        }

        [Fact]
        public async Task PageSizeWorksOnCollectionOfComplexProperty()
        {
            string resquestUri = BaseAddress + "/odata/ComplextTypeCollectionTests_Persons(1)/PersonInfos";
            HttpResponseMessage response = await Client.GetAsync(resquestUri);

            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();

            JObject result = JObject.Parse(responseContent);

            Assert.Equal("XXX/odata/$metadata#ComplextTypeCollectionTests_Persons(1)/PersonInfos".Replace("XXX", BaseAddress.ToLowerInvariant()),
                result["@odata.context"]);

#if NETCORE
            Assert.Equal("XXX/odata/ComplextTypeCollectionTests_Persons(1)/PersonInfos?$skip=2".Replace("XXX", BaseAddress.ToLowerInvariant()),
                result["@odata.nextLink"]);
#else
            Assert.Equal("XXX/odata/ComplextTypeCollectionTests_Persons%281%29/PersonInfos?$skip=2".Replace("XXX", BaseAddress.ToLowerInvariant()),
                result["@odata.nextLink"]);
#endif

            JArray personInfos = result["value"] as JArray;
            Assert.NotNull(personInfos);
            IEnumerable<string> actualData = personInfos.Select(jt => jt["CompanyName"].ToString());
            Assert.Equal(new[] {"Company 1", "Company 2"}, actualData);
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("?$count=false", false)]
        [InlineData("?$count=true", true)]
        public async Task DollarCountWorksOnCollectionOfComplexProperty(string countOption, bool expect)
        {
            string resquestUri = BaseAddress + "/odata/ComplextTypeCollectionTests_Persons(1)/PersonInfos" + countOption;
            HttpResponseMessage response = await Client.GetAsync(resquestUri);

            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();

            if (expect)
            {
                Assert.Contains("\"@odata.count\":5", responseContent);
            }
            else
            {
                Assert.DoesNotContain("\"@odata.count\":5", responseContent);
            }
        }
    }

    public class ComplextTypeCollectionTests_PersonsController : TestODataController
    {
        public static List<ComplextTypeCollectionTests_Person> Persons = null;

        static ComplextTypeCollectionTests_PersonsController()
        {
            Persons = new List<ComplextTypeCollectionTests_Person>();

            var address1 = new ComplextTypeCollectionTests_Address();
            address1.City = "Bellevue";
            address1.State = "WA";
            address1.CountryOrRegion = "USA";
            address1.Zipcode = 98007;

            var address2 = new ComplextTypeCollectionTests_Address();
            address2.City = "Redmond";
            address2.State = "WA";
            address2.CountryOrRegion = "USA";
            address2.Zipcode = 98052;

            var address3 = new ComplextTypeCollectionTests_Address();
            address3.City = "Issaquah";
            address3.State = "WA";
            address3.CountryOrRegion = "USA";
            address3.Zipcode = 98029;

            ComplextTypeCollectionTests_Person person = new ComplextTypeCollectionTests_Person();
            person.Id = 1;
            person.Name = "James King";
            person.Addresses.Add(address1);
            person.Addresses.Add(address2);
            person.Addresses.Add(address3);

            person.PersonInfos = Enumerable.Range(1, 5).Select(e => new ComplexTypeCollectionTests_PersonInfo
            {
                CompanyName = "Company " + e,
                Years = 10 + e
            });
            Persons.Add(person);
        }

        [EnableQuery]
        public ITestActionResult GetAddresses([FromODataUri]int key)
        {
            ComplextTypeCollectionTests_Person person = Persons.FirstOrDefault(p => p.Id == key);

            if (person == null)
            {
                return NotFound();
            }

            return Ok(person.Addresses.AsQueryable());
        }

        [EnableQuery(PageSize = 2)]
        public ITestActionResult GetPersonInfos([FromODataUri]int key)
        {
            ComplextTypeCollectionTests_Person person = Persons.FirstOrDefault(p => p.Id == key);

            if (person == null)
            {
                return NotFound();
            }

            return Ok(person.PersonInfos.AsQueryable());
        }
    }

    public class ComplextTypeCollectionTests_Person
    {
        public ComplextTypeCollectionTests_Person()
        {
            this.Addresses = new List<ComplextTypeCollectionTests_Address>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public List<ComplextTypeCollectionTests_Address> Addresses { get; set; }

        public IEnumerable<ComplexTypeCollectionTests_PersonInfo> PersonInfos { get; set; }
    }

    public class ComplextTypeCollectionTests_Address
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string CountryOrRegion { get; set; }
        public int Zipcode { get; set; }
    }

    public class ComplexTypeCollectionTests_PersonInfo
    {
        public string CompanyName { get; set; }

        public int Years { get; set; }
    }
}
