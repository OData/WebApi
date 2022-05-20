using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class DecimalTest
    {
        [Fact]
        public async Task Decimal_ScaleTest()
        {
            const string Uri = "http://localhost/odata/DecimalRoundTestEntity";
            using (HttpClient client = GetClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Uri);
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));

                // Act
                HttpResponseMessage response = await client.SendAsync(request);

                // Assert
                IEnumerable<DecimalRoundTestEntity> decimalRoundTestEntities = await ReadEntitiesFromResponse<DecimalRoundTestEntity>(response);
                foreach (DecimalRoundTestEntity decimalRoundTestEntity in decimalRoundTestEntities)
                {
                    Assert.Equal(decimalRoundTestEntity.ExpectedDecimalValue, decimalRoundTestEntity.ActualDecimalValue);
                }
            }
        }

        private static HttpClient GetClient()
        {
            var controllers = new[] { typeof(MetadataController), typeof(DecimalRoundTestEntityController) };
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
            builder.EntitySet<DecimalRoundTestEntity>("DecimalRoundTestEntity");
            builder.EntityType<DecimalRoundTestEntity>().Property(s => s.ActualDecimalValue).Scale = 2;
            return builder.GetEdmModel();
        }

        async Task<IEnumerable<T>> ReadEntitiesFromResponse<T>(HttpResponseMessage httpResponseMessage) where T : class
        {
            string jsonContent = await httpResponseMessage.Content.ReadAsStringAsync();
            
            return JsonConvert.DeserializeObject<ODataResponse<T>>(jsonContent).Value;
        }
    }

    public class DecimalRoundTestEntityController : TestODataController
    {
        private DecimalModelContext db = new DecimalModelContext();

        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(db.DecimalRoundTestEntities);
        }
    }

    class DecimalModelContext
    {
        private static IList<DecimalRoundTestEntity> _decimalRoundTestEntities;

        static DecimalModelContext()
        {
            _decimalRoundTestEntities = new List<DecimalRoundTestEntity>();
            _decimalRoundTestEntities.Add(new DecimalRoundTestEntity(10, 10));
            _decimalRoundTestEntities.Add(new DecimalRoundTestEntity(10.01m, 10.01m));
            _decimalRoundTestEntities.Add(new DecimalRoundTestEntity(10.001m, 10.00m));
            _decimalRoundTestEntities.Add(new DecimalRoundTestEntity(10.999m, 11m));
            _decimalRoundTestEntities.Add(new DecimalRoundTestEntity(10.335m, 10.34m));
            _decimalRoundTestEntities.Add(new DecimalRoundTestEntity(-10.335m, -10.34m));
        }

        public IEnumerable<DecimalRoundTestEntity> DecimalRoundTestEntities { get { return _decimalRoundTestEntities; } }
    }

    class ODataResponse<T>
    {
        public IEnumerable<T> Value { get; set; }
    }
}
