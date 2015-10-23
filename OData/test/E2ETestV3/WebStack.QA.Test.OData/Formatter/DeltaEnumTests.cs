using Microsoft.Data.Edm;
using Nuwa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using WebStack.QA.Common.XUnit;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter
{
    [NuwaFramework]
    [NwHost(Nuwa.HostType.KatanaSelf)]
    public class DeltaEnumTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.Routes.MapODataServiceRoute("odata", "odata", GetModel());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<DeltaEnumCustomer>("DeltaEnumCustomers");
            return builder.GetEdmModel();
        }

        [Fact]
        public void MetadataWorks()
        {
            HttpRequestMessage put = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/$metadata");
            HttpResponseMessage response = Client.SendAsync(put).Result;
            Assert.True(response.IsSuccessStatusCode);
            string payload = response.Content.ReadAsStringAsync().Result;

            Assert.Contains("<Property Name=\"Color\" Type=\"Edm.String\" Nullable=\"false\" />", payload);
            Assert.Contains("<Property Name=\"NullableColor\" Type=\"Edm.String\" />", payload);
            Assert.Contains("<Property Name=\"Colors\" Type=\"Collection(Edm.String)\" Nullable=\"false\" />", payload);
            Assert.Contains("<Property Name=\"NullableColors\" Type=\"Collection(Edm.String)\" />", payload);
        }

        public static TheoryDataSet<string> PayloadContent
        {
            get
            {
                var data = new TheoryDataSet<string>();

                data.Add("{\"Color\":\"Red\",\"NullableColor\":null}");

                data.Add("{\"Color\":\"Red\",\"NullableColor\":\"Blue\"}");

                data.Add("{\"Color\":\"Red\",\"NullableColor\":\"Blue\",\"Colors\":[\"Red\", \"Blue\"]}");

                data.Add("{\"Color\":\"Red\",\"NullableColor\":\"Blue\",\"Colors\":[\"Red\", \"Blue\"],\"NullableColors\":[]}");

                return data;
            }
        }

        [Theory]
        [PropertyData("PayloadContent")]
        public void PutWorks(string payload)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, BaseAddress + "/odata/DeltaEnumCustomers(2)");
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            HttpResponseMessage response = Client.SendAsync(request).Result;

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Theory]
        [PropertyData("PayloadContent")]
        public void PatchWorks(string payload)
        {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("MERGE"), BaseAddress + "/odata/DeltaEnumCustomers(3)");
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            HttpResponseMessage response = Client.SendAsync(request).Result;

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);
        }
    }

    public class DeltaEnumCustomersController : ODataController
    {
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public IHttpActionResult Get([FromODataUri] int key)
        {
            DeltaEnumCustomer customer = GetCustomers().FirstOrDefault(e => e.Id == key);
            if (customer != null)
            {
                return Ok(customer);
            }
            else
            {
                return NotFound();
            }
        }

        public IHttpActionResult Put([FromODataUri] int key,  Delta<DeltaEnumCustomer> entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DeltaEnumCustomer customer = GetCustomers().FirstOrDefault(e => e.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            entity.Put(customer);
            return Updated(customer);
        }

        [AcceptVerbs("PATCH", "MERGE")]
        public IHttpActionResult Patch([FromODataUri] int key, Delta<DeltaEnumCustomer> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DeltaEnumCustomer customer = GetCustomers().FirstOrDefault(e => e.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            patch.Patch(customer);
            return Updated(customer);
        }

        private static IEnumerable<DeltaEnumCustomer> GetCustomers()
        {
            return Enumerable.Range(1, 5).Select(e => new DeltaEnumCustomer
            {
                Id = e,
                Name = "Customer_" + e,
                Color = e % 2 == 0 ? DeltaColor.Red : DeltaColor.Blue,
                NullableColor = e % 2 == 0 ? (DeltaColor?)null : DeltaColor.Red,
                Colors = Enumerable.Range(1, 5).Select(f => f % 2 == 0 ? DeltaColor.Red : DeltaColor.Blue).ToList(),
                NullableColors =
                    Enumerable.Range(1, 5).Select(k => k % 2 == 0 ? (DeltaColor?)null : DeltaColor.Blue).ToList(),
            });
        }
    }

    public enum DeltaColor
    {
        Red,
        Blue,
        Green
    }

    public class DeltaEnumCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DeltaColor Color { get; set; }

        public DeltaColor? NullableColor { get; set; }

        public ICollection<DeltaColor> Colors { get; set; }

        public ICollection<DeltaColor?> NullableColors { get; set; }
    }
}
