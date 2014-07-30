// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter.Deserialization;
using System.Web.OData.Routing;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Web.OData.Formatter
{
    public class InheritanceTests
    {
        HttpServer _server;
        HttpClient _client;
        IEdmModel _model;

        public InheritanceTests()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            _model = GetEdmModel();
            IEnumerable<ODataMediaTypeFormatter> formatters = ODataMediaTypeFormatters.Create();

            configuration.Formatters.Clear();
            configuration.Formatters.AddRange(formatters);

            configuration.Routes.MapHttpRoute("default", "{action}", new { Controller = "Inheritance" });
            configuration.Routes.MapFakeODataRoute();

            _server = new HttpServer(configuration);
            _client = new HttpClient(_server);
        }

        [Fact]
        public void Action_Can_Return_Entity_In_Inheritance()
        {
            // Arrange
            HttpRequestMessage request = GetODataRequest("http://localhost/GetMotorcycleAsVehicle");

            // Act
            HttpResponseMessage response = _client.SendAsync(request).Result;

            // Assert
            response.EnsureSuccessStatusCode();
            dynamic result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            ValidateMotorcycle(result);
        }

        [Fact]
        public void Action_Can_Return_Car_As_vehicle()
        {
            // Arrange
            HttpRequestMessage request = GetODataRequest("http://localhost/GetCarAsVehicle");

            // Act
            HttpResponseMessage response = _client.SendAsync(request).Result;

            // Assert
            response.EnsureSuccessStatusCode();
            dynamic result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            ValidateCar(result);
        }

        [Fact]
        public void Action_Can_Return_ClrType_NotInModel()
        {
            // Arrange
            HttpRequestMessage request = GetODataRequest("http://localhost/GetSportBikeAsVehicle");

            // Act
            HttpResponseMessage response = _client.SendAsync(request).Result;

            // Assert
            response.EnsureSuccessStatusCode();
            dynamic result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            ValidateSportbike(result);
        }

        [Fact]
        public void Action_Can_Return_CollectionOfEntities()
        {
            // Arrange
            HttpRequestMessage request = GetODataRequest("http://localhost/GetVehicles");

            // Act
            HttpResponseMessage response = _client.SendAsync(request).Result;

            // Assert
            response.EnsureSuccessStatusCode();
            dynamic result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            ValidateMotorcycle(result.value[0]);
            ValidateCar(result.value[1]);
            ValidateSportbike(result.value[2]);
        }

        [Fact]
        public void Action_Can_Take_Entity_In_Inheritance()
        {
            // Arrange
            Stream body = GetResponseStream("http://localhost/GetMotorcycleAsVehicle", "application/json");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/PostMotorcycle_When_Expecting_Motorcycle");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            AddRequestInfo(request);
            request.Content = new StreamContent(body);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = _client.SendAsync(request).Result;

            // Assert
            response.EnsureSuccessStatusCode();

            dynamic result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            ValidateMotorcycle(result);
        }

        [Fact]
        public void Can_Patch_Entity_In_Inheritance()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), "http://localhost/PatchMotorcycle_When_Expecting_Motorcycle");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            AddRequestInfo(request);
            request.Content = new StringContent("{ 'CanDoAWheelie' : false }");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = _client.SendAsync(request).Result;

            // Assert
            response.EnsureSuccessStatusCode();

            dynamic result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.False((bool)result.CanDoAWheelie);
        }

        [Fact]
        public void Can_Post_DerivedType_To_Action_Expecting_BaseType()
        {
            // Arrange
            Stream body = GetResponseStream("http://localhost/GetMotorcycleAsVehicle", "application/json");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/PostMotorcycle_When_Expecting_Vehicle");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            AddRequestInfo(request);
            request.Content = new StreamContent(body);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = _client.SendAsync(request).Result;

            // Assert
            response.EnsureSuccessStatusCode();

            dynamic result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            ValidateMotorcycle(result);
        }

        [Fact]
        public void Can_Post_DerivedType_To_Action_Expecting_BaseType_ForJsonLight()
        {
            // Arrange
            Stream body = GetResponseStream("http://localhost/GetMotorcycleAsVehicle", "application/json;odata.metadata=minimal");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/PostMotorcycle_When_Expecting_Vehicle");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            AddRequestInfo(request);
            request.Content = new StreamContent(body);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal");

            // Act
            HttpResponseMessage response = _client.SendAsync(request).Result;

            // Assert
            response.EnsureSuccessStatusCode();

            dynamic result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            ValidateMotorcycle(result);
        }

        [Fact]
        public void Can_Patch_DerivedType_To_Action_Expecting_BaseType()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), "http://localhost/PatchMotorcycle_When_Expecting_Vehicle");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            AddRequestInfo(request);
            request.Content = new StringContent("{ '@odata.type': '#System.Web.OData.Builder.TestModels.Motorcycle', 'CanDoAWheelie' : false }");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = _client.SendAsync(request).Result;

            // Assert
            response.EnsureSuccessStatusCode();

            dynamic result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.False((bool)result.CanDoAWheelie);
        }

        [Fact]
        public void Can_Patch_DerivedType_To_Action_Expecting_BaseType_ForJsonLight()
        {
            //Arrange
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), "http://localhost/PatchMotorcycle_When_Expecting_Vehicle");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            AddRequestInfo(request);
            request.Content = new StringContent("{ '@odata.type': 'System.Web.OData.Builder.TestModels.Motorcycle', 'CanDoAWheelie' : false }");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal");

            // Act
            HttpResponseMessage response = _client.SendAsync(request).Result;

            // Assert
            response.EnsureSuccessStatusCode();

            dynamic result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.False((bool)result.CanDoAWheelie);
        }

        [Fact]
        public void Posting_NonDerivedType_To_Action_Expecting_BaseType_Throws()
        {
            // Arrange
            StringContent content = new StringContent("{ '@odata.type' : '#System.Web.OData.Builder.TestModels.Motorcycle' }");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            IODataRequestMessage oDataRequest = new ODataMessageWrapper(content.ReadAsStreamAsync().Result, content.Headers);
            ODataMessageReader reader = new ODataMessageReader(oDataRequest, new ODataMessageReaderSettings(), _model);

            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();

            ODataDeserializerContext context = new ODataDeserializerContext { Model = _model };
            IEdmActionImport action = _model.EntityContainer
                .OperationImports()
                .Single(f => f.Name == "PostMotorcycle_When_Expecting_Car") as IEdmActionImport;
            Assert.NotNull(action);

            context.Path = new ODataPath(new UnboundActionPathSegment(action));

            // Act & Assert
            Assert.Throws<ODataException>(
                () => new ODataEntityDeserializer(deserializerProvider).Read(reader, typeof(Car), context),
                "An entry with type 'System.Web.OData.Builder.TestModels.Motorcycle' was found, " +
                "but it is not assignable to the expected type 'System.Web.OData.Builder.TestModels.Car'. " +
                "The type specified in the entry must be equal to either the expected type or a derived type.");
        }

        private Stream GetResponseStream(string uri, string contentType)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(contentType));
            AddRequestInfo(request);
            HttpResponseMessage response = _client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            Stream stream = response.Content.ReadAsStreamAsync().Result;

            return stream;
        }

        private static void ValidateMotorcycle(dynamic result)
        {
            Assert.Equal("#System.Web.OData.Builder.TestModels.Motorcycle", (string)result["@odata.type"]);
            Assert.Equal("sample motorcycle", (string)result.Name);
            Assert.Equal("2009", (string)result.Model);
            Assert.Equal(2, (int)result.WheelCount);
            Assert.Equal(true, (bool)result.CanDoAWheelie);
        }

        private static void ValidateCar(dynamic result)
        {
            Assert.Equal("#System.Web.OData.Builder.TestModels.Car", (string)result["@odata.type"]);
            Assert.Equal("sample car", (string)result.Name);
            Assert.Equal("2009", (string)result.Model);
            Assert.Equal(4, (int)result.WheelCount);
            Assert.Equal(5, (int)result.SeatingCapacity);
        }

        private static void ValidateSportbike(dynamic result)
        {
            Assert.Equal("#System.Web.OData.Builder.TestModels.Motorcycle", (string)result["@odata.type"]);
            Assert.Equal("sample sportsbike", (string)result.Name);
            Assert.Equal("2009", (string)result.Model);
            Assert.Equal(2, (int)result.WheelCount);
            Assert.Equal(true, (bool)result.CanDoAWheelie);
            Assert.Null(result.SportBikeProperty_NotVisible);
        }

        private HttpRequestMessage GetODataRequest(string uri)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            AddRequestInfo(request);
            return request;
        }

        private void AddRequestInfo(HttpRequestMessage request)
        {
            request.ODataProperties().Path = new DefaultODataPathHandler()
                .Parse(_model, "http://any/", GetODataPath(request.RequestUri.AbsoluteUri));
            request.ODataProperties().Model = _model;
            request.SetFakeODataRouteName();
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            builder
                .EntityType<Vehicle>()
                .HasKey(v => v.Name)
                .HasKey(v => v.Model)
                .Property(v => v.WheelCount);

            builder
                .EntityType<Motorcycle>()
                .DerivesFrom<Vehicle>()
                .Property(m => m.CanDoAWheelie);

            builder
                .EntityType<Car>()
                .DerivesFrom<Vehicle>()
                .Property(c => c.SeatingCapacity);

            builder.EntitySet<Vehicle>("vehicles").HasIdLink(
                (v) => new Uri("http://localhost/vehicles/" + v.GetPropertyValue("Name")), followsConventions: false);
            builder.EntitySet<Motorcycle>("motorcycles").HasIdLink(
                (m) => new Uri("http://localhost/motorcycles/" + m.GetPropertyValue("Name")), followsConventions: false);
            builder.EntitySet<Car>("cars");

            builder
                .Action("GetCarAsVehicle")
                .ReturnsFromEntitySet<Vehicle>("vehicles");
            builder
                .Action("GetMotorcycleAsVehicle")
                .ReturnsFromEntitySet<Vehicle>("vehicles");
            builder
                .Action("GetSportBikeAsVehicle")
                .ReturnsFromEntitySet<Vehicle>("vehicles");
            builder
                .Action("GetVehicles")
                .ReturnsFromEntitySet<Vehicle>("vehicles");

            builder
                .Action("PatchMotorcycle_When_Expecting_Motorcycle")
                .ReturnsFromEntitySet<Motorcycle>("motorcycles");
            builder
                .Action("PostMotorcycle_When_Expecting_Motorcycle")
                .ReturnsFromEntitySet<Motorcycle>("motorcycles");
            builder
                .Action("PatchMotorcycle_When_Expecting_Vehicle")
                .ReturnsFromEntitySet<Vehicle>("vehicles");
            builder
                .Action("PostMotorcycle_When_Expecting_Vehicle")
                .ReturnsFromEntitySet<Vehicle>("vehicles");
            builder
                .Action("PostMotorcycle_When_Expecting_Car")
                .ReturnsFromEntitySet<Car>("cars");
            builder
                .Action("PatchMotorcycle_When_Expecting_Car")
                .ReturnsFromEntitySet<Car>("cars");

            return builder.GetEdmModel();
        }

        private static string GetODataPath(string url)
        {
            string serverBaseUri = "http://localhost/";
            Assert.True(url.StartsWith(serverBaseUri)); // Guard
            return url.Substring(serverBaseUri.Length);
        }
    }

    public class InheritanceController : ODataController
    {
        private Motorcycle motorcycle = new Motorcycle { Model = 2009, Name = "sample motorcycle", CanDoAWheelie = true };
        private Car car = new Car { Model = 2009, Name = "sample car", SeatingCapacity = 5 };
        private SportBike sportBike = new SportBike { Model = 2009, Name = "sample sportsbike", CanDoAWheelie = true, SportBikeProperty_NotVisible = 100 };

        public Vehicle GetMotorcycleAsVehicle()
        {
            return motorcycle;
        }

        public Vehicle GetCarAsVehicle()
        {
            return car;
        }

        public Vehicle GetSportBikeAsVehicle()
        {
            return sportBike;
        }

        public IEnumerable<Vehicle> GetVehicles()
        {
            return new Vehicle[] { motorcycle, car, sportBike };
        }

        public Motorcycle PostMotorcycle_When_Expecting_Motorcycle(Motorcycle motorcycle)
        {
            Assert.IsType<Motorcycle>(motorcycle);
            return motorcycle;
        }

        public Motorcycle PatchMotorcycle_When_Expecting_Motorcycle(Delta<Motorcycle> patch)
        {
            patch.Patch(motorcycle);
            return motorcycle;
        }

        public Motorcycle PutMotorcycle_When_Expecting_Motorcycle(Delta<Motorcycle> patch)
        {
            patch.Put(motorcycle);
            return motorcycle;
        }

        public Vehicle PostMotorcycle_When_Expecting_Vehicle(Vehicle motorcycle)
        {
            Assert.IsType<Motorcycle>(motorcycle);
            return motorcycle;
        }

        public Vehicle PatchMotorcycle_When_Expecting_Vehicle(Delta<Vehicle> patch)
        {
            Assert.IsType<Motorcycle>(patch.GetEntity());
            patch.Patch(motorcycle);
            return motorcycle;
        }
    }
}
