// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using System.Web.Http.OData.Extensions;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Web.Http.OData.Formatter
{
    public class CollectionsTests
    {
        private HttpClient _client;

        public CollectionsTests()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<CollectionsTestsModel>("CollectionsTests");
            builder.EntitySet<Vehicle>("vehicles");
            IEdmModel model = builder.GetEdmModel();

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Formatters.Clear();
            configuration.Formatters.AddRange(ODataMediaTypeFormatters.Create());
            configuration.Routes.MapODataServiceRoute(model);

            HttpServer server = new HttpServer(configuration);
            _client = new HttpClient(server);
        }

        [Theory]
        [InlineData("Array")]
        [InlineData("IEnumerable")]
        [InlineData("ICollection")]
        [InlineData("IList")]
        [InlineData("List")]
        [InlineData("Collection")]
        [InlineData("CustomCollection")]
        public void CollectionProperties_Deserialize(string propertyName)
        {
            string message = "{ \"ID\" : 42, \"" + propertyName + "\": { \"results\":[ 1, 2, 3 ] } }";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/CollectionsTests/");
            request.Content = new StringContent(message);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata=verbose");

            HttpResponseMessage response = _client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();

            dynamic result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            result = result.d;

            Assert.Equal(new[] { 1, 2, 3 }, (IEnumerable<int>)result[propertyName]["results"].Values<int>());
        }

        [Fact]
        public void ComplexCollectionProperty_Deserialize()
        {
            string message = "{ \"ID\" : 42, \"ComplexCollection\" : { \"results\":[  { \"A\": 1 }, { \"A\": 2 }, { \"A\": 3 } ] } }";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/CollectionsTests/");
            request.Content = new StringContent(message);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata=verbose");

            HttpResponseMessage response = _client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();

            dynamic result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            result = result.d;
            IEnumerable<JObject> complexCollection = result["ComplexCollection"]["results"].Values<JObject>();
            Assert.Equal(
                new[] { 1, 2, 3 },
                complexCollection.AsQueryable().Select(v => (int)v.Property("A")));
        }

        [Fact]
        public void EntityCollectionProperty_Deserialize()
        {
            string message = "{ 'ID' : 44,  'Vehicles' : [ " +
                "{ '__metadata' : { 'type': 'System.Web.Http.OData.Builder.TestModels.Car' }, 'Model': 2009, 'Name': 'Car'}, " +
                "{ '__metadata' : { 'type': 'System.Web.Http.OData.Builder.TestModels.Motorcycle' }, 'Model': 2010, 'Name': 'Motorcycle'}, " +
                "{ '__metadata' : { 'type': 'System.Web.Http.OData.Builder.TestModels.SportBike' }, 'Model': 2012, 'Name': 'SportBike'} " +
                " ] }";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/CollectionsTests/");
            request.Content = new StringContent(message);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata=verbose");

            HttpResponseMessage response = _client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public void Posting_A_Feed_To_NonCollectionProperty_ODataLibThrows()
        {
            string message = "{ 'ID' : 44,  'Vehicle' : [ " +
                "{ '__metadata' : { 'type': 'System.Web.Http.OData.Builder.TestModels.Car' }, 'Model': 2009, 'Name': 'Car'}, " +
                "{ '__metadata' : { 'type': 'System.Web.Http.OData.Builder.TestModels.Motorcycle' }, 'Model': 2010, 'Name': 'Motorcycle'}, " +
                "{ '__metadata' : { 'type': 'System.Web.Http.OData.Builder.TestModels.SportBike' }, 'Model': 2012, 'Name': 'SportBike'} " +
                " ] }";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/CollectionsTests/");
            request.Content = new StringContent(message);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata=verbose");

            HttpResponseMessage response = _client.SendAsync(request).Result;
            Assert.Equal(HttpStatusCode.ExpectationFailed, response.StatusCode);
        }
    }

    public class CollectionsTestsController : ODataController
    {
        public CollectionsTestsModel Post(CollectionsTestsModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(HttpStatusCode.ExpectationFailed);
            }

            // 44 => posting vehicles
            if (model.ID == 44)
            {
                Assert.NotNull(model.Vehicles);
                Assert.Equal(3, model.Vehicles.Length);

                Assert.IsType(typeof(Car), model.Vehicles[0]);
                Assert.Equal(2009, model.Vehicles[0].Model);
                Assert.Equal("Car", model.Vehicles[0].Name);

                Assert.IsType(typeof(Motorcycle), model.Vehicles[1]);
                Assert.Equal(2010, model.Vehicles[1].Model);
                Assert.Equal("Motorcycle", model.Vehicles[1].Name);

                Assert.IsType(typeof(SportBike), model.Vehicles[2]);
                Assert.Equal(2012, model.Vehicles[2].Model);
                Assert.Equal("SportBike", model.Vehicles[2].Name);
            }

            return model;
        }
    }

    public class CollectionsTestsModel
    {
        public CollectionsTestsModel()
        {
            Array = new int[] { 42 };
            IEnumerable = new int[] { 42 };
            ICollection = new int[] { 42 };
            IList = new int[] { 42 };
            List = new List<int> { 42 };
            Collection = new Collection<int> { 42 };
            ComplexCollection = new Complex[] { new Complex { A = 42 } };
            CustomCollection = new CustomCollection_CollectionsTestsModel<int> { 42 };
        }

        public int ID { get; set; }

        public int[] Array { get; set; }

        public IEnumerable<int> IEnumerable { get; set; }

        public IList<int> IList { get; set; }

        public ICollection<int> ICollection { get; set; }

        public Collection<int> Collection { get; set; }

        public List<int> List { get; set; }

        public IEnumerable<Complex> ComplexCollection { get; set; }

        public Vehicle[] Vehicles { get; set; }

        public Vehicle Vehicle { get; set; }

        public CustomCollection_CollectionsTestsModel<int> CustomCollection { get; set; }
    }

    public class Complex
    {
        public int A { get; set; }
    }

    public class CustomCollection_CollectionsTestsModel<T> : List<T>
    {
    }
}
