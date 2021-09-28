//-----------------------------------------------------------------------------
// <copyright file="InheritanceTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Newtonsoft.Json.Linq;
using Xunit;
#else
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Newtonsoft.Json.Linq;
using Xunit;
#endif
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Test.Formatter
{
    public class InheritanceTests
    {
        HttpClient _client;
        IEdmModel _model;

        public InheritanceTests()
        {
            _model = GetEdmModel();
            Type[] controllers = new[] { typeof(InheritanceController) };
            var server = TestServerFactory.Create(controllers, (configuration) =>
            {
                configuration.MapNonODataRoute("default", "{action}", new { Controller = "Inheritance" });
#if NETCORE
                configuration.EnableDependencyInjection(b => b.AddService(Microsoft.OData.ServiceLifetime.Singleton, p => _model));
                var options = configuration.ApplicationBuilder.ApplicationServices.GetRequiredService<IOptions<MvcOptions>>().Value;
                options.Filters.Add(new MyResourceFilter());
#else
                configuration.Routes.MapFakeODataRoute();
                configuration.EnableODataDependencyInjectionSupport();
#endif
            }
#if NETCORE
            ,
            (services) => services.AddSingleton<IUrlHelperFactory, MyUrlHelperFactory>()
#endif
            );

            _client = TestServerFactory.CreateClient(server);
        }

        [Fact]
        public async Task Action_Can_Return_Entity_In_Inheritance()
        {
            // Arrange
            HttpRequestMessage request = GetODataRequest("http://localhost/GetMotorcycleAsVehicle");

            // Act
            HttpResponseMessage response = await _client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            dynamic result = JObject.Parse(await response.Content.ReadAsStringAsync());
            ValidateMotorcycle(result);
        }

        [Fact]
        public async Task Action_Can_Return_Car_As_vehicle()
        {
            // Arrange
            HttpRequestMessage request = GetODataRequest("http://localhost/GetCarAsVehicle");

            // Act
            HttpResponseMessage response = await _client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            dynamic result = JObject.Parse(await response.Content.ReadAsStringAsync());
            ValidateCar(result);
        }

        [Fact]
        public async Task Action_Can_Return_ClrType_NotInModel()
        {
            // Arrange
            HttpRequestMessage request = GetODataRequest("http://localhost/GetSportBikeAsVehicle");

            // Act
            HttpResponseMessage response = await _client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            dynamic result = JObject.Parse(await response.Content.ReadAsStringAsync());
            ValidateSportbike(result);
        }

        [Fact]
        public async Task Action_Can_Return_CollectionOfEntities()
        {
            // Arrange
            HttpRequestMessage request = GetODataRequest("http://localhost/GetVehicles");

            // Act
            HttpResponseMessage response = await _client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            dynamic result = JObject.Parse(await response.Content.ReadAsStringAsync());
            ValidateMotorcycle(result.value[0]);
            ValidateCar(result.value[1]);
            ValidateSportbike(result.value[2]);
        }

        [Fact]
        public async Task Action_Can_Take_Entity_In_Inheritance()
        {
            // Arrange
            Stream body = await GetResponseStream("http://localhost/GetMotorcycleAsVehicle", "application/json");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/PostMotorcycle_When_Expecting_Motorcycle");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            AddRequestInfo(request);
            request.Content = new StreamContent(body);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await _client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());

            dynamic result = JObject.Parse(await response.Content.ReadAsStringAsync());
            ValidateMotorcycle(result);
        }

        [Fact]
        public async Task Can_Patch_Entity_In_Inheritance()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), "http://localhost/PatchMotorcycle_When_Expecting_Motorcycle");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            AddRequestInfo(request);
            request.Content = new StringContent("{ 'CanDoAWheelie' : false }");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await _client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());

            dynamic result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.False((bool)result.CanDoAWheelie);
        }

        [Fact]
        public async Task Can_Patch_Entity_In_Inheritance_DerivedEngine()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), "http://localhost/PatchMotorcycle_When_Expecting_Motorcycle_DerivedEngine");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            AddRequestInfo(request);
            request.Content = new StringContent("{ 'CanDoAWheelie' : false, 'MyEngine' : {'@odata.type' : 'Microsoft.AspNet.OData.Test.Builder.TestModels.V4' ,'Hp':4000} }");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await _client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());

            dynamic result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.False((bool)result.CanDoAWheelie);
            Assert.Equal(4000, (int)result.MyEngine.Hp);
        }


        [Fact]
        public async Task Can_Patch_Entity_In_Inheritance_DerivedEngine_MultiLevel()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), "http://localhost/PatchMotorcycle_When_Expecting_Motorcycle_DerivedEngine");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            AddRequestInfo(request);
            request.Content = new StringContent("{ 'CanDoAWheelie' : false, 'MyEngine' : {'@odata.type' : 'Microsoft.AspNet.OData.Test.Builder.TestModels.V41' ,'Hp':4000, 'MakeName': 'Honda'} }");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await _client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());

            dynamic result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.False((bool)result.CanDoAWheelie);
            Assert.Equal(4000, (int)result.MyEngine.Hp);
            Assert.Equal("Honda", result.MyEngine.MakeName.ToString());
        }

        [Fact]
        public async Task Can_Patch_Entity_In_Inheritance_DerivedEngine_MultiLevel_ParentToChild()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), "http://localhost/PatchMotorcycle_When_Expecting_Motorcycle_DerivedEngine");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            AddRequestInfo(request);
            request.Content = new StringContent("{ 'CanDoAWheelie' : false, 'MyEngine' : {'@odata.type' : 'Microsoft.AspNet.OData.Test.Builder.TestModels.V41' ,'Hp':4000, 'MakeName': 'Honda'}, " +
                "'MyV4Engine' : {'@odata.type' : 'Microsoft.AspNet.OData.Test.Builder.TestModels.V422' ,'Hp':5000, 'Model': 'Hero'} }");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await _client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());

            dynamic result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.False((bool)result.CanDoAWheelie);
            Assert.Equal(4000, (int)result.MyEngine.Hp);
            Assert.Equal("Honda", result.MyEngine.MakeName.ToString());

            Assert.Equal(5000, (int)result.MyV4Engine.Hp);
            Assert.Equal("Hero", result.MyV4Engine.Model.ToString());
        }

        [Fact]
        public async Task Can_Patch_Entity_In_Inheritance_DerivedEngine_MultiLevel_ChildToParent()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), "http://localhost/PatchMotorcycle_When_Expecting_Motorcycle_DerivedEngine2");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            AddRequestInfo(request);
            request.Content = new StringContent("{ 'CanDoAWheelie' : false, 'MyEngine' : {'@odata.type' : 'Microsoft.AspNet.OData.Test.Builder.TestModels.V41' ,'Hp':4000, 'MakeName': 'Honda'}, " +
                "'MyV4Engine' : {'@odata.type' : 'Microsoft.AspNet.OData.Test.Builder.TestModels.V4' ,'Hp':7000 } }");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await _client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());

            dynamic result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.False((bool)result.CanDoAWheelie);
            Assert.Equal(4000, (int)result.MyEngine.Hp);
            Assert.Equal("Honda", result.MyEngine.MakeName.ToString());
            Assert.Equal(7000, (int)result.MyV4Engine.Hp);
        }

        [Fact]
        public async Task Can_Post_DerivedType_To_Action_Expecting_BaseType()
        {
            // Arrange
            Stream body = await GetResponseStream("http://localhost/GetMotorcycleAsVehicle", "application/json");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/PostMotorcycle_When_Expecting_Vehicle");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            AddRequestInfo(request);
            request.Content = new StreamContent(body);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await _client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());

            dynamic result = JObject.Parse(await response.Content.ReadAsStringAsync());
            ValidateMotorcycle(result);
        }

        [Fact]
        public async Task Can_Post_DerivedType_To_Action_Expecting_BaseType_ForJsonLight()
        {
            // Arrange
            Stream body = await GetResponseStream("http://localhost/GetMotorcycleAsVehicle", "application/json;odata.metadata=minimal");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/PostMotorcycle_When_Expecting_Vehicle");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            AddRequestInfo(request);
            request.Content = new StreamContent(body);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal");

            // Act
            HttpResponseMessage response = await _client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());

            dynamic result = JObject.Parse(await response.Content.ReadAsStringAsync());
            ValidateMotorcycle(result);
        }

        [Fact]
        public async Task Can_Patch_DerivedType_To_Action_Expecting_BaseType()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), "http://localhost/PatchMotorcycle_When_Expecting_Vehicle");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            AddRequestInfo(request);
            request.Content = new StringContent("{ '@odata.type': '#Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle', 'CanDoAWheelie' : false }");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await _client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());

            dynamic result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.False((bool)result.CanDoAWheelie);
        }

        [Fact]
        public async Task Can_Patch_DerivedType_To_Action_Expecting_BaseType_ForJsonLight()
        {
            //Arrange
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), "http://localhost/PatchMotorcycle_When_Expecting_Vehicle");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            AddRequestInfo(request);
            request.Content = new StringContent("{ '@odata.type': 'Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle', 'CanDoAWheelie' : false }");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal");

            // Act
            HttpResponseMessage response = await _client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());

            dynamic result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.False((bool)result.CanDoAWheelie);
        }

        [Fact]
        public async Task Posting_NonDerivedType_To_Action_Expecting_BaseType_Throws()
        {
            // Arrange
            StringContent content = new StringContent("{ '@odata.type' : '#Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle' }");
            var headers = FormatterTestHelper.GetContentHeaders("application/json");
            IODataRequestMessage oDataRequest = ODataMessageWrapperHelper.Create(await content.ReadAsStreamAsync(), headers);
            ODataMessageReader reader = new ODataMessageReader(oDataRequest, new ODataMessageReaderSettings(), _model);

            ODataDeserializerProvider deserializerProvider = ODataDeserializerProviderFactory.Create();

            ODataDeserializerContext context = new ODataDeserializerContext { Model = _model };
            IEdmActionImport action = _model.EntityContainer
                .OperationImports()
                .Single(f => f.Name == "PostMotorcycle_When_Expecting_Car") as IEdmActionImport;
            Assert.NotNull(action);
            IEdmEntitySetBase actionEntitySet;
            action.TryGetStaticEntitySet(_model, out actionEntitySet);
            context.Path = new ODataPath(new OperationImportSegment(new[] { action }, actionEntitySet, null));

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(
                () => new ODataResourceDeserializer(deserializerProvider).Read(reader, typeof(Car), context),
                "A resource with type 'Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle' was found, " +
                "but it is not assignable to the expected type 'Microsoft.AspNet.OData.Test.Builder.TestModels.Car'. " +
                "The type specified in the resource must be equal to either the expected type or a derived type.");
        }

        private async Task<Stream> GetResponseStream(string uri, string contentType)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(contentType));
            AddRequestInfo(request);
            HttpResponseMessage response = await _client.SendAsync(request);
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            Stream stream = await response.Content.ReadAsStreamAsync();

            return stream;
        }

        private static void ValidateMotorcycle(dynamic result)
        {
            Assert.Equal("#Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle", (string)result["@odata.type"]);
            Assert.Equal("sample motorcycle", (string)result.Name);
            Assert.Equal("2009", (string)result.Model);
            Assert.Equal(2, (int)result.WheelCount);
            Assert.True((bool)result.CanDoAWheelie);
        }

        private static void ValidateCar(dynamic result)
        {
            Assert.Equal("#Microsoft.AspNet.OData.Test.Builder.TestModels.Car", (string)result["@odata.type"]);
            Assert.Equal("sample car", (string)result.Name);
            Assert.Equal("2009", (string)result.Model);
            Assert.Equal(4, (int)result.WheelCount);
            Assert.Equal(5, (int)result.SeatingCapacity);
        }

        private static void ValidateSportbike(dynamic result)
        {
            Assert.Equal("#Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle", (string)result["@odata.type"]);
            Assert.Equal("sample sportsbike", (string)result.Name);
            Assert.Equal("2009", (string)result.Model);
            Assert.Equal(2, (int)result.WheelCount);
            Assert.True((bool)result.CanDoAWheelie);
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
#if !NETCORE
            // TODO #939: Using HttpRequestMessage which is fine except the extensions
            // for it are defined in the AspNet project.
            request.ODataProperties().Path = new DefaultODataPathHandler()
                .Parse(_model, "http://any/", GetODataPath(request.RequestUri.AbsoluteUri));
            request.EnableODataDependencyInjectionSupport(_model);
#endif
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

            var vehConfig = builder
               .EntityType<Motorcycle>()
               .DerivesFrom<Vehicle>();

            vehConfig.ComplexProperty(m => m.MyEngine);
            vehConfig.ComplexProperty(m => m.MyV4Engine);


            builder.ComplexType<Engine>().Property(m => m.Hp);

            builder.ComplexType<V2>().DerivesFrom<Engine>();
            builder.ComplexType<V4>().DerivesFrom<Engine>();
            builder.ComplexType<V41>().DerivesFrom<V4>();
            builder.ComplexType<V41>().Property(m => m.MakeName);
            builder.ComplexType<V42>().Property(m => m.Model);
            builder.ComplexType<V42>().DerivesFrom<V4>();
            builder.ComplexType<V422>().DerivesFrom<V42>();
            
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
                .Action("PatchMotorcycle_When_Expecting_Motorcycle_DerivedEngine")
                .ReturnsFromEntitySet<Motorcycle>("motorcycles");

            builder
               .Action("PatchMotorcycle_When_Expecting_Motorcycle_DerivedEngine2")
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
            Assert.StartsWith(serverBaseUri, url); // Guard
            return url.Substring(serverBaseUri.Length);
        }
    }

    public class InheritanceController : ODataController
    {
        private Motorcycle motorcycle = new Motorcycle { Model = 2009, Name = "sample motorcycle", CanDoAWheelie = true, MyEngine=new V2 { Hp = 2000 }, MyV4Engine = new V4() };
        private Motorcycle motorcycle1 = new Motorcycle { Model = 2009, Name = "sample motorcycle1", CanDoAWheelie = true, MyEngine = new V2 { Hp = 2000 }, MyV4Engine = new V422() };
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

        public Motorcycle PostMotorcycle_When_Expecting_Motorcycle([FromBody]Motorcycle motorcycle)
        {
            Assert.IsType<Motorcycle>(motorcycle);
            return motorcycle;
        }

        public Motorcycle PatchMotorcycle_When_Expecting_Motorcycle(Delta<Motorcycle> patch)
        {
            patch.Patch(motorcycle);
            return motorcycle;
        }

        public Motorcycle PatchMotorcycle_When_Expecting_Motorcycle_DerivedEngine(Delta<Motorcycle> patch)
        {
            patch.Patch(motorcycle);
            
            var engine = motorcycle.MyEngine as V4;
            
            Assert.NotNull(engine);
            Assert.Equal(4000, engine.Hp);

            return motorcycle;
        }

        public Motorcycle PatchMotorcycle_When_Expecting_Motorcycle_DerivedEngine2(Delta<Motorcycle> patch)
        {
            patch.Patch(motorcycle);

            var engine = motorcycle.MyEngine as V4;

            Assert.NotNull(engine);
            Assert.Equal(4000, engine.Hp);

            var engine2 = motorcycle.MyV4Engine as V4;

            Assert.NotNull(engine2);
            Assert.Equal(7000, engine2.Hp);

            return motorcycle;
        }

        public Motorcycle PutMotorcycle_When_Expecting_Motorcycle(Delta<Motorcycle> patch)
        {
            patch.Put(motorcycle);
            return motorcycle;
        }

        public Vehicle PostMotorcycle_When_Expecting_Vehicle([FromBody]Vehicle motorcycle)
        {
            Assert.IsType<Motorcycle>(motorcycle);
            return motorcycle;
        }

        public Vehicle PatchMotorcycle_When_Expecting_Vehicle(Delta<Vehicle> patch)
        {
            Assert.IsType<Motorcycle>(patch.GetInstance());
            patch.Patch(motorcycle);
            return motorcycle;
        }
    }

#if NETCORE
    public class MyUrlHelper : IUrlHelper
    {
        public MyUrlHelper(ActionContext context)
        {
            ActionContext = context;
        }

        public ActionContext ActionContext { get; }

        public string Action(UrlActionContext actionContext) => String.Empty;

        public string Content(string contentPath) => String.Empty;

        public bool IsLocalUrl(string url) => true;

        public string Link(string routeName, object values) => "http://any/";

        public string RouteUrl(UrlRouteContext routeContext) => String.Empty;
    }

    public class MyUrlHelperFactory : IUrlHelperFactory
    {
        public IUrlHelper GetUrlHelper(ActionContext context)
        {
            return new MyUrlHelper(context);
        }
    }

    public class MyResourceFilter : IResourceFilter
    {
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // nothing
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            context.HttpContext.Request.GetRequestContainer();

            if (context.HttpContext.Request.ODataFeature().Path == null)
            {
                IEdmModel model = context.HttpContext.Request.GetModel();
                context.HttpContext.Request.ODataFeature().Path = new DefaultODataPathHandler()
                    .Parse(model, "http://any/", context.HttpContext.Request.Path);
            }

            if (String.IsNullOrEmpty(context.HttpContext.Request.ODataFeature().RouteName))
            {
                context.HttpContext.Request.ODataFeature().RouteName = "OData";
            }
        }
    }
#endif
}
