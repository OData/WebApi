using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;

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

            configuration.Routes.MapODataServiceRoute("odataRoute", "odata", builder.GetEdmModel());
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
            address1.Country = "USA";
            address1.Zipcode = 98007;

            var address2 = new ComplextTypeCollectionTests_Address();
            address2.City = "Redmond";
            address2.State = "WA";
            address2.Country = "USA";
            address2.Zipcode = 98052;

            var address3 = new ComplextTypeCollectionTests_Address();
            address3.City = "Issaquah";
            address3.State = "WA";
            address3.Country = "USA";
            address3.Zipcode = 98029;

            ComplextTypeCollectionTests_Person person = new ComplextTypeCollectionTests_Person();
            person.Id = 1;
            person.Name = "James King";
            person.Addresses.Add(address1);
            person.Addresses.Add(address2);
            person.Addresses.Add(address3);

            Persons.Add(person);
        }

        [EnableQuery]
        public IQueryable<ComplextTypeCollectionTests_Address> GetAddresses([FromODataUri]int key)
        {
            ComplextTypeCollectionTests_Person person = Persons.Where(p => p.Id == key).FirstOrDefault();

            if(person == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return person.Addresses.AsQueryable();
        }
    }

    public  class ComplextTypeCollectionTests_Person
    {
        public ComplextTypeCollectionTests_Person()
        {
            this.Addresses = new List<ComplextTypeCollectionTests_Address>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public List<ComplextTypeCollectionTests_Address> Addresses { get; set; }
    }

    public class ComplextTypeCollectionTests_Address
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public int Zipcode { get; set; }
    }
}
