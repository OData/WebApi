//-----------------------------------------------------------------------------
// <copyright file="ODataStreamPropertyTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
#else
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test
{
    public class ODataStreamPropertyTest
    {
        [Fact]
        public async Task GetMetadata_WithStreamProperty()
        {
            // Arrange
            const string RequestUri = "http://localhost/odata/$metadata";

            var controllers = new[] { typeof(MetadataController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", GetEdmModel());
            });
            var client = TestServerFactory.CreateClient(server);

            // Act
            HttpResponseMessage response = await client.GetAsync(RequestUri);

            // Assert
            var payload = await response.Content.ReadAsStringAsync();
            Assert.Contains("<Property Name=\"Photo\" Type=\"Edm.Stream\" />", payload);
        }

        [Fact]
        public async Task Get_EntityWithStreamProperty()
        {
            // Arrange
            const string RequestUri = "http://localhost/odata/StreamCustomers(1)";

            var controllers = new[] { typeof(StreamCustomersController) };

            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", GetEdmModel());
            });
            var client = TestServerFactory.CreateClient(server);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("http://localhost/odata/$metadata#StreamCustomers/$entity", result["@odata.context"]);
            Assert.Equal("#Microsoft.AspNet.OData.Test.StreamCustomer", result["@odata.type"]);
            Assert.Equal("\u0002\u0003\u0004\u0005", result["PhotoText"]);
            Assert.Equal("http://localhost/odata/StreamCustomers(1)/Photo", result["Photo@odata.mediaEditLink"]);
            Assert.Equal("http://localhost/odata/StreamCustomers(1)/Photo", result["Photo@odata.mediaReadLink"]);
        }

        [Fact]
        public async Task Get_SingleStreamProperty()
        {
            // Arrange
            const string RequestUri = "http://localhost/odata/StreamCustomers(2)/Photo";

            var controllers = new[] { typeof(StreamCustomersController) };

            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", GetEdmModel());
            });
            var client = TestServerFactory.CreateClient(server);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            Assert.Equal("application/octet-stream", response.Content.Headers.ContentType.MediaType);
            var stream = await response.Content.ReadAsStreamAsync();

            StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();
            Assert.Equal("\u0003\u0004\u0005\u0006", text);

            byte[] byteArray = stream.ReadAllBytes();
            Assert.Equal(new byte[] { 3, 4, 5, 6 }, byteArray);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<StreamCustomer>("StreamCustomers");
            return builder.GetEdmModel();
        }
    }

    public static class StreamExtensions
    {
        public static byte[] ReadAllBytes(this Stream instream)
        {
            if (instream is MemoryStream)
                return ((MemoryStream)instream).ToArray();

            using (var memoryStream = new MemoryStream())
            {
                instream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }

    // Controller
    public class StreamCustomersController : TestODataController
    {
        [EnableQuery]
        public IQueryable<StreamCustomer> Get()
        {
            return CreateCustomers().AsQueryable();
        }

        public ITestActionResult Get(int key)
        {
            IList<StreamCustomer> customers = CreateCustomers();
            StreamCustomer customer = customers.FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        [HttpGet]
        public ITestActionResult GetName(int key)
        {
            IList<StreamCustomer> customers = CreateCustomers();
            StreamCustomer customer = customers.FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer.Name);
        }

        [HttpGet]
        public ITestActionResult GetPhoto(int key)
        {
            IList<StreamCustomer> customers = CreateCustomers();
            StreamCustomer customer = customers.FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer.Photo);
        }
        private static IList<StreamCustomer> CreateCustomers()
        {
            byte[] byteArray = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            IList<StreamCustomer> customers = Enumerable.Range(0, 5).Select(i =>
                new StreamCustomer
                {
                    Id = i,
                    Name = "FirstName " + i,
                    Photo = new MemoryStream(byteArray, i, 4),
                }).ToList();

            foreach (var c in customers)
            {
                c.PhotoText = new StreamReader(c.Photo).ReadToEnd();
                c.Photo.Seek(0, SeekOrigin.Begin);
            }

            return customers;
        }
    }

    public class StreamCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        // this property saves the string of the Photo
        public string PhotoText { get; set; }

        public Stream Photo { get; set; }
    }
}
