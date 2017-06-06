using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using WebStack.QA.Instancing;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using WebStack.QA.Test.OData.Common.Models.Vehicle;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter
{
    #region Controllers
    public class InheritanceTests_MovingObjectsController : InMemoryEntitySetController<MovingObject, int>
    {
        public InheritanceTests_MovingObjectsController()
            : base("Id")
        {
        }
    }

    public class InheritanceTests_VehiclesController : InMemoryEntitySetController<Vehicle, int>
    {
        public InheritanceTests_VehiclesController()
            : base("Id")
        {
        }

        public void WashOnCar(int key)
        {
        }

        public void WashOnSportBike(int key)
        {
        }

        [HttpPut]
        public HttpResponseMessage CreateLinkToSingleNavigationPropertyOnCar(int key, [FromBody] Uri link)
        {
            var found = this.LocalTable[key] as Car;
            if (found == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Car with key {0} is not found", key));
            }

            int relatedKey = Request.GetKeyValue<int>(link);
            var relatedObj = this.LocalTable[relatedKey];
            if (relatedObj == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("The link with key {0} is not found", relatedKey));
            }

            found.SingleNavigationProperty = relatedObj;

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        public Task<Vehicle> GetSingleNavigationPropertyOnCar(int key)
        {
            return Task.Factory.StartNew(() =>
            {
                return (this.LocalTable[key] as Car).SingleNavigationProperty;
            });
        }
    }

    public class InheritanceTests_CarsController : InMemoryEntitySetController<Car, int>
    {
        public InheritanceTests_CarsController()
            : base("Id")
        {
        }

        public Task<IEnumerable<Vehicle>> GetBaseTypeNavigationProperty(int key)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.LocalTable[key].BaseTypeNavigationProperty.AsEnumerable();
            });
        }

        public Task<HttpResponseMessage> PostBaseTypeNavigationProperty(int key, Vehicle vehicle)
        {
            return Task.Factory.StartNew(() =>
            {
                new InheritanceTests_VehiclesController().LocalTable.AddOrUpdate(vehicle.Id, vehicle, (id, v) => vehicle);
                this.LocalTable[key].BaseTypeNavigationProperty.Add(vehicle);

                var response = this.Request.CreateResponse(System.Net.HttpStatusCode.Created, vehicle);
                response.Headers.Location = new Uri(this.Url.CreateODataLink(
                        new EntitySetPathSegment("InheritanceTests_Vehicles"),
                        new KeyValuePathSegment(vehicle.Id.ToString())));
                return response;
            });
        }

        public Task<IEnumerable<Vehicle>> GetDerivedTypeNavigationProperty(int key)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.LocalTable[key].DerivedTypeNavigationProperty.OfType<Vehicle>();
            });
        }

        public Task<HttpResponseMessage> PostDerivedTypeNavigationProperty(int key, MiniSportBike vehicle)
        {
            return Task.Factory.StartNew(() =>
            {
                new InheritanceTests_VehiclesController().LocalTable.AddOrUpdate(vehicle.Id, vehicle, (id, v) => vehicle);
                this.LocalTable[key].DerivedTypeNavigationProperty.Add(vehicle);

                var response = this.Request.CreateResponse(System.Net.HttpStatusCode.Created, vehicle);
                response.Headers.Location = new Uri(this.Url.CreateODataLink(
                        new EntitySetPathSegment("InheritanceTests_Vehicles"),
                        new KeyValuePathSegment(vehicle.Id.ToString())));
                return response;
            });
        }

        public override Task DeleteLink(int key, string relatedKey, string navigationProperty)
        {
            return Task.Factory.StartNew(() =>
            {
                var entity = this.LocalTable[key];
                switch (navigationProperty)
                {
                    case "BaseTypeNavigationProperty":
                        {
                            var vehicle = entity.BaseTypeNavigationProperty.FirstOrDefault(v => v.Id == Convert.ToInt32(relatedKey));
                            if (vehicle == null)
                            {
                                throw new HttpResponseException(this.Request.CreateResponse(HttpStatusCode.NotFound));
                            }

                            entity.BaseTypeNavigationProperty.Remove(vehicle);
                        }
                        break;
                    case "DerivedTypeNavigationProperty":
                        {
                            var vehicle = entity.DerivedTypeNavigationProperty.FirstOrDefault(v => v.Id == Convert.ToInt32(relatedKey));
                            if (vehicle == null)
                            {
                                throw new HttpResponseException(this.Request.CreateResponse(HttpStatusCode.NotFound));
                            }

                            entity.DerivedTypeNavigationProperty.Remove(vehicle);
                        }
                        break;
                    default:
                        return;
                }
            });
        }
    }

    public class InheritanceTests_SportBikesController : InMemoryEntitySetController<SportBike, int>
    {
        public InheritanceTests_SportBikesController()
            : base("Id")
        {
        }
    }

    public class InheritanceTests_CustomersController : InMemoryEntitySetController<Customer, int>
    {
        public InheritanceTests_CustomersController()
            : base("Id")
        {
        }

        public IEnumerable<Vehicle> GetVehicles(int key)
        {
            var customer = this.LocalTable[key];
            return customer.Vehicles;
        }
    }
    #endregion

    public class InheritanceTests : ODataFormatterTestBase
    {
        public static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder(configuration);
            builder.EntitySet<MovingObject>("InheritanceTests_MovingObjects");
            var vehicle = builder.EntitySet<Vehicle>("InheritanceTests_Vehicles").EntityType;
            var cars = builder.EntitySet<Car>("InheritanceTests_Cars");
            cars.EntityType.Action("Wash");

            // Skip: http://aspnetwebstack.codeplex.com/workitem/780
            builder.OnModelCreating = mb =>
                {
                    //cars = builder.EntitySets.OfType<EntitySetConfiguration<Car>>().First();
                    cars.HasNavigationPropertiesLink(
                        cars.EntityType.NavigationProperties,
                        (entityContext, navigationProperty) =>
                        {
                            object id;
                            entityContext.EdmObject.TryGetPropertyValue("Id", out id);
                            return new Uri(entityContext.Url.CreateODataLink(
                                new EntitySetPathSegment("InheritanceTests_Cars"),
                                new KeyValuePathSegment(id.ToString()),
                                new NavigationPathSegment(navigationProperty.Name)));
                        },
                        false);

                };

            builder.Entity<SportBike>().Action("Wash");
            builder.EntitySet<Customer>("InheritanceTests_Customers");

            return builder.GetEdmModel();
        }

        public override DataServiceContext WriterClient(Uri serviceRoot, DataServiceProtocolVersion protocolVersion)
        {
            return new Container(serviceRoot, protocolVersion);
        }

        public virtual void PostGetUpdateAndDelete(Type entityType, string entitySetName)
        {
            // clear respository
            this.ClearRepository(entitySetName);

            Random r = new Random(RandomSeedGenerator.GetRandomSeed());

            // post new entity to repository
            var value = InstanceCreator.CreateInstanceOf(entityType, r, new CreatorSettings()
            {
                NullValueProbability = 0.0
            });
            DataServiceContext ctx = WriterClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            ctx.AddObject(entitySetName, value);
            ctx.SaveChanges();

            // get collection of entities from repository
            ctx = ReaderClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            var entities = ctx.CreateQuery<Vehicle>(entitySetName);
            var beforeUpdate = entities.ToList().First();
            AssertExtension.PrimitiveEqual(value, beforeUpdate);

            // update entity and verify if it's saved
            ctx = WriterClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            ctx.AttachTo(entitySetName, beforeUpdate);
            beforeUpdate.Name = InstanceCreator.CreateInstanceOf<string>(r);
            ctx.UpdateObject(beforeUpdate);
            ctx.SaveChanges();

            // retrieve the updated entity
            ctx = ReaderClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            entities = ctx.CreateQuery<Vehicle>(entitySetName);
            var afterUpdate = entities.Where(e => e.Id == beforeUpdate.Id).First();
            Assert.Equal(beforeUpdate.Name, afterUpdate.Name);

            // delete entity
            ctx = WriterClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            ctx.AttachTo(entitySetName, afterUpdate);
            ctx.DeleteObject(afterUpdate);
            ctx.SaveChanges();

            // ensure that the entity has been deleted
            ctx = ReaderClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            entities = ctx.CreateQuery<Vehicle>(entitySetName);
            Assert.Equal(0, entities.ToList().Count());

            // clear repository
            this.ClearRepository(entitySetName);
        }

        public virtual void AddAndRemoveBaseNavigationPropertyInDerivedType()
        {
            // clear respository
            this.ClearRepository("InheritanceTests_Cars");

            Random r = new Random(RandomSeedGenerator.GetRandomSeed());

            // post new entity to repository
            var car = InstanceCreator.CreateInstanceOf<Car>(r);
            var vehicle = InstanceCreator.CreateInstanceOf<Vehicle>(r);
            DataServiceContext ctx = WriterClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            ctx.AddObject("InheritanceTests_Cars", car);
            ctx.AddRelatedObject(car, "BaseTypeNavigationProperty", vehicle);
            ctx.SaveChanges();

            ctx = ReaderClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            var cars = ctx.CreateQuery<Car>("InheritanceTests_Cars");
            var actual = cars.ToList().First();
            ctx.LoadProperty(actual, "BaseTypeNavigationProperty");

            AssertExtension.PrimitiveEqual(vehicle, actual.BaseTypeNavigationProperty[0]);

            ctx = WriterClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            ctx.AttachTo("InheritanceTests_Cars", actual);
            ctx.AttachTo("InheritanceTests_Cars", actual.BaseTypeNavigationProperty[0]);
            ctx.DeleteLink(actual, "BaseTypeNavigationProperty", actual.BaseTypeNavigationProperty[0]);
            ctx.SaveChanges();

            ctx = ReaderClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            cars = ctx.CreateQuery<Car>("InheritanceTests_Cars");
            actual = cars.ToList().First();
            ctx.LoadProperty(actual, "BaseTypeNavigationProperty");

            Assert.Empty(actual.BaseTypeNavigationProperty);

            this.ClearRepository("InheritanceTests_Cars");
        }

        public virtual void AddAndRemoveDerivedNavigationPropertyInDerivedType()
        {
            // clear respository
            this.ClearRepository("InheritanceTests_Cars");

            Random r = new Random(RandomSeedGenerator.GetRandomSeed());

            // post new entity to repository
            var car = InstanceCreator.CreateInstanceOf<Car>(r);
            var vehicle = InstanceCreator.CreateInstanceOf<MiniSportBike>(r, new CreatorSettings()
            {
                NullValueProbability = 0.0
            });
            DataServiceContext ctx = WriterClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            ctx.AddObject("InheritanceTests_Cars", car);
            ctx.AddRelatedObject(car, "DerivedTypeNavigationProperty", vehicle);
            ctx.SaveChanges();

            ctx = ReaderClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            var cars = ctx.CreateQuery<Car>("InheritanceTests_Cars");
            var actual = cars.ToList().First();
            ctx.LoadProperty(actual, "DerivedTypeNavigationProperty");

            AssertExtension.PrimitiveEqual(vehicle, actual.DerivedTypeNavigationProperty[0]);

            ctx = WriterClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            ctx.AttachTo("InheritanceTests_Cars", actual);
            ctx.AttachTo("InheritanceTests_Cars", actual.DerivedTypeNavigationProperty[0]);
            ctx.DeleteLink(actual, "DerivedTypeNavigationProperty", actual.DerivedTypeNavigationProperty[0]);
            ctx.SaveChanges();

            ctx = ReaderClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            cars = ctx.CreateQuery<Car>("InheritanceTests_Cars");
            actual = cars.ToList().First();
            ctx.LoadProperty(actual, "DerivedTypeNavigationProperty");

            Assert.Empty(actual.DerivedTypeNavigationProperty);

            this.ClearRepository("InheritanceTests_Cars");
        }

        public virtual void CreateAndDeleteLinkToDerivedNavigationPropertyOnBaseEntitySet()
        {
            // clear respository
            this.ClearRepository("InheritanceTests_Vehicles");

            Random r = new Random(RandomSeedGenerator.GetRandomSeed());

            // post new entity to repository
            var car = InstanceCreator.CreateInstanceOf<Car>(r);
            var vehicle = InstanceCreator.CreateInstanceOf<MiniSportBike>(r, new CreatorSettings()
            {
                NullValueProbability = 0.0
            });
            DataServiceContext ctx = WriterClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            ctx.AddObject("InheritanceTests_Vehicles", car);
            ctx.AddObject("InheritanceTests_Vehicles", vehicle);
            ctx.SaveChanges();

            ctx.SetLink(car, "SingleNavigationProperty", vehicle);
            ctx.SaveChanges();

            ctx = ReaderClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            var cars = ctx.CreateQuery<Vehicle>("InheritanceTests_Vehicles").ToList().OfType<Car>();
            var actual = cars.First();
            ctx.LoadProperty(actual, "SingleNavigationProperty");
            AssertExtension.PrimitiveEqual(vehicle, actual.SingleNavigationProperty);

            this.ClearRepository("InheritanceTests_Vehicles");
        }

        public virtual void InvokeActionWithOverloads(string actionUrl)
        {
            DataServiceContext ctx = new DataServiceContext(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            var result = ctx.Execute(
                new Uri(this.BaseAddress + actionUrl),
                "POST");

            Assert.Equal(204, result.StatusCode);
        }
    }

    // Skip: uncommon scenario
    /*
    public class InheritanceWithExplicitModelBuilderTests : InheritanceTests
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration1(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel1());
            //configuration.Services.Replace(typeof(IHttpActionSelector), new ODataActionSelector());
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig1(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        public static IEdmModel GetEdmModel1()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            var vehicle = builder
                .Entity<Vehicle>()
                .Abstract();
            vehicle.HasKey(v => v.Id);
            vehicle.Property(v => v.Name);
            vehicle.Property(v => v.Model);
            vehicle.Property(v => v.WheelCount);

            var motocycle = builder
                .Entity<Motorcycle>()
                .DerivesFrom<Vehicle>();
            motocycle.Property(m => m.CanDoAWheelie).IsRequired();

            var car = builder
                .Entity<Car>()
                .DerivesFrom<Vehicle>();
            car.Property(c => c.SeatingCapacity);
            car.HasMany(c => c.BaseTypeNavigationProperty);
            car.HasMany(c => c.DerivedTypeNavigationProperty);

            builder
                .Entity<SportBike>()
                .DerivesFrom<Motorcycle>()
                .Property(b => b.TopSpeed);

            builder
                .Entity<MiniSportBike>()
                .DerivesFrom<SportBike>()
                .Property(b => b.Size);

            var vehicles = builder.EntitySet<Vehicle>("InheritanceTests_Vehicles");
            vehicles.HasEditLink(entityContext => entityContext.Url.Link(ODataRouteNames.Default, new
            {
                odataPath = entityContext.PathParser.EntitySetLink(
                    entityContext.EntitySet,
                    entityContext.EntityInstance.Id)
            }));
            var motocycles = builder.EntitySet<Motorcycle>("InheritanceTests_Motorcycles");
            motocycles.HasEditLink(entityContext => entityContext.Url.Link(ODataRouteNames.Default, new
            {
                odataPath = entityContext.PathParser.EntitySetLink(
                    entityContext.EntitySet,
                    entityContext.EntityInstance.Id)
            }));
            var cars = builder.EntitySet<Car>("InheritanceTests_Cars");
            cars.HasEditLink(entityContext => entityContext.Url.Link(ODataRouteNames.Default, new
            {
                odataPath = entityContext.PathParser.EntitySetLink(
                    entityContext.EntitySet,
                    entityContext.EntityInstance.Id)
            }));
            cars.HasManyBinding(c => c.BaseTypeNavigationProperty, vehicles);
            cars.HasManyBinding(c => c.DerivedTypeNavigationProperty, vehicles);

            var miniSportBike = builder.EntitySet<MiniSportBike>("InheritanceTests_MiniSportBikes");
            miniSportBike.HasEditLink(entityContext => entityContext.Url.Link(ODataRouteNames.Default, new
            {
                odataPath = entityContext.PathParser.EntitySetLink(
                    entityContext.EntitySet,
                    entityContext.EntityInstance.Id)
            }));

            cars.HasNavigationPropertiesLink(
                car.NavigationProperties,
                (entityContext, navigationProperty) => new Uri(entityContext.Url.Link(
                    ODataRouteNames.Default,
                    new
                    {
                        odataPath = entityContext.PathParser.NavigationLink(
                            entityContext.EntitySet,
                            entityContext.EntityInstance.Id,
                            navigationProperty)
                    })));

            vehicles.HasNavigationPropertiesLink(
                vehicle.NavigationProperties,
                (entityContext, navigationProperty) => new Uri(entityContext.Url.Link(
                    ODataRouteNames.Default,
                    new
                    {
                        odataPath = entityContext.PathParser.NavigationLink(
                            entityContext.EntitySet,
                            entityContext.EntityInstance.Id,
                            navigationProperty)
                    })));

            return builder.GetEdmModel();
        }
    }*/
}
