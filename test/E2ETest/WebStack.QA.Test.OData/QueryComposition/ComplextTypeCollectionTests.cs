using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.QueryComposition
{
    [NuwaFramework]
    public class ComplextTypeCollectionTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Routes.Clear();

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<ComplextTypeCollectionTests_Person>("ComplextTypeCollectionTests_Persons");

            configuration.MapODataServiceRoute("odataRoute", "odata", builder.GetEdmModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        [Fact]
        public void OrderByTest()
        {
            HttpResponseMessage response = Client.GetAsync(BaseAddress + "/odata/ComplextTypeCollectionTests_Persons(1)/Addresses?$orderby=City").Result;

            response.EnsureSuccessStatusCode();

            IEnumerable<string> expectedData =
                    ComplextTypeCollectionTests_PersonsController.Persons.Where(p => p.Id == 1).First().Addresses.OrderBy(a => a.City).Select(a => a.City);

            string responseContent = response.Content.ReadAsStringAsync().Result;

            JObject jo = JObject.Parse(responseContent);
            JArray addresses = jo["value"] as JArray;
            IEnumerable<string> actualData = addresses.Select(jt => jt["City"].ToString());

            Assert.Equal(expectedData, actualData);
        }

        [Fact]
        public void PageSizeWorksOnCollectionOfComplexProperty()
        {
            string resquestUri = BaseAddress + "/odata/ComplextTypeCollectionTests_Persons(1)/PersonInfos";
            HttpResponseMessage response = Client.GetAsync(resquestUri).Result;

            response.EnsureSuccessStatusCode();

            string responseContent = response.Content.ReadAsStringAsync().Result;

            JObject result = JObject.Parse(responseContent);

            Assert.Equal("XXX/odata/$metadata#Collection(WebStack.QA.Test.OData.QueryComposition.ComplexTypeCollectionTests_PersonInfo)".Replace("XXX", BaseAddress.ToLowerInvariant()),
                result["@odata.context"]);

            Assert.Equal("XXX/odata/ComplextTypeCollectionTests_Persons%281%29/PersonInfos?$skip=2".Replace("XXX", BaseAddress.ToLowerInvariant()),
                result["@odata.nextLink"]);

            JArray personInfos = result["value"] as JArray;
            Assert.NotNull(personInfos);
            IEnumerable<string> actualData = personInfos.Select(jt => jt["CompanyName"].ToString());
            Assert.Equal(new[] {"Company 1", "Company 2"}, actualData);
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("?$count=false", false)]
        [InlineData("?$count=true", true)]
        public void DollarCountWorksOnCollectionOfComplexProperty(string countOption, bool expect)
        {
            string resquestUri = BaseAddress + "/odata/ComplextTypeCollectionTests_Persons(1)/PersonInfos" + countOption;
            HttpResponseMessage response = Client.GetAsync(resquestUri).Result;

            response.EnsureSuccessStatusCode();

            string responseContent = response.Content.ReadAsStringAsync().Result;

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

    public class ComplextTypeCollectionTests_PersonsController : ODataController
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
        public IQueryable<ComplextTypeCollectionTests_Address> GetAddresses([FromODataUri]int key)
        {
            ComplextTypeCollectionTests_Person person = Persons.FirstOrDefault(p => p.Id == key);

            if (person == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return person.Addresses.AsQueryable();
        }

        [EnableQuery(PageSize = 2)]
        public IQueryable<ComplexTypeCollectionTests_PersonInfo> GetPersonInfos([FromODataUri]int key)
        {
            ComplextTypeCollectionTests_Person person = Persons.FirstOrDefault(p => p.Id == key);

            if (person == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return person.PersonInfos.AsQueryable();
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
