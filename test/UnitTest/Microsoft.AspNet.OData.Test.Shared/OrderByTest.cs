using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace Microsoft.AspNet.OData.Test
{
    public class OrderByTest
    {
        [Fact]
        public async Task OrderBy_MultipleLevelOrderByTest()
        {
            const string Uri = "http://localhost/odata/Students?$expand=Backpacks($expand=Address;$orderBy=Name,Address/PlaceNumber,BackpackNumber)";
            HttpClient client = GetClient();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Uri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Asserts
            var student = await ReadEntitiesFromResponse<Student>(response);
            var backpakcs = student.Backpacks;

            var firstBackpack = backpakcs[0];
            Assert.Equal(123, firstBackpack.Address.PlaceNumber);
            Assert.Equal(1, firstBackpack.BackpackNumber);

            var second = backpakcs[1];
            Assert.Equal(342, second.Address.PlaceNumber);
            Assert.Equal(2, second.BackpackNumber);

            var third = backpakcs[2];
            Assert.Equal(987, third.Address.PlaceNumber);
            Assert.Equal(3, third.BackpackNumber);

            var fourth = backpakcs[3];
            Assert.Equal(987, fourth.Address.PlaceNumber);
            Assert.Equal(4, fourth.BackpackNumber);
        }

        private static HttpClient GetClient()
        {
            var controllers = new[] { typeof(MetadataController), typeof(StudentsController) };

            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.Count().OrderBy().Filter().Expand().MaxTop(null).Select();
                config.MapODataServiceRoute("odata", "odata", GetEdmModel());

            });
            return TestServerFactory.CreateClient(server);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<Backpack>();
            builder.EntitySet<Student>("Students");
            builder.EntitySet<Backpack>("Backpacks");
            builder.EntitySet<BackpackAddress>("BackpackAddress");
            return builder.GetEdmModel();
        }

        async Task<T> ReadEntitiesFromResponse<T>(HttpResponseMessage httpResponseMessage) where T : class
        {
            string jsonContent = await httpResponseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ODataResponse<T>>(jsonContent).Value.ToList().First();
        }
    }

    public class StudentsController : TestODataController
    {
        private OrderByModelContext db = new OrderByModelContext();

        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(db.Students);
        }
    }

    class OrderByModelContext
    {
        private static IList<Student> students;

        static OrderByModelContext()
        {
            students = new List<Student>();
            students.Add(new Student
            {
                Id = Guid.NewGuid(),
                Backpacks = new List<Backpack>
                {
                    new Backpack{ Name="Backpack", Address = new BackpackAddress() { PlaceNumber = 342 }, BackpackNumber = 2  },
                    new Backpack{ Name="Backpack", Address = new BackpackAddress() { PlaceNumber = 123 }, BackpackNumber = 1 },
                    new Backpack{ Name="Backpack", Address = new BackpackAddress() { PlaceNumber = 987}, BackpackNumber = 4  },
                    new Backpack{ Name="Backpack", Address = new BackpackAddress() { PlaceNumber = 987}, BackpackNumber = 3  },
                }
            });
        }

        public IEnumerable<Student> Students { get { return students; } }
    }

    class Student
    {
        public Guid Id { get; set; }
        public List<Backpack> Backpacks { get; set; }
    }

    class Backpack
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public BackpackAddress Address { get; set; }
        public string Name { get; set; }
        public int BackpackNumber { get; set; }
    }

    class BackpackAddress
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int PlaceNumber { get; set; }
    }

    class ODataResponse<T>
    {
        public IEnumerable<T> Value { get; set; }
    }
}
