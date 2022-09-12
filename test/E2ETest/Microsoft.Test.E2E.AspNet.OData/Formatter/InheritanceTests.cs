//-----------------------------------------------------------------------------
// <copyright file="InheritanceTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Instancing;
using Microsoft.Test.E2E.AspNet.OData.Common.Models.Vehicle;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter
{
    public class DeleteAllRoutingConvention : TestEntitySetRoutingConvention
    {
        /// <inheritdoc/>
        protected override string SelectAction(string requestMethod, ODataPath odataPath, TestControllerContext controllerContext, IList<string> actionList)
        {
            if (requestMethod == "DELETE" &&
                  odataPath.PathTemplate == "~/entityset")
            {
                string actionName = "Delete";
                if (actionList.Contains(actionName))
                {
                    return actionName;
                }
            }
            return null;
        }
    }

    #region Controllers

    public class InheritanceTests_MovingObjectsController : InMemoryODataController<MovingObject, int>
    {
        public InheritanceTests_MovingObjectsController()
            : base("Id")
        {
        }
    }

    public class InheritanceTests_VehiclesController : InMemoryODataController<Vehicle, int>
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
        public ITestActionResult CreateRefToSingleNavigationPropertyOnCar(int key, [FromBody] Uri link)
        {
            var found = this.LocalTable[key] as Car;
            if (found == null)
            {
                return NotFound();
            }

            int relatedKey = GetRequestValue<int>(link);
            var relatedObj = this.LocalTable[relatedKey];
            if (relatedObj == null)
            {
                return NotFound();
            }

            found.SingleNavigationProperty = relatedObj;

            return StatusCode(HttpStatusCode.NoContent);
        }

        public Task<ITestActionResult> GetSingleNavigationPropertyOnCar(int key)
        {
            return Task<ITestActionResult>.Factory.StartNew(() =>
            {
                return Ok((LocalTable[key] as Car).SingleNavigationProperty);
            });
        }
    }

    public class InheritanceTests_CarsController : InMemoryODataController<Car, int>
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

        public Task<ITestActionResult> PostBaseTypeNavigationProperty(int key, Vehicle vehicle)
        {
            return Task<ITestActionResult>.Factory.StartNew(() =>
            {
                new InheritanceTests_VehiclesController().LocalTable.AddOrUpdate(vehicle.Id, vehicle, (id, v) => vehicle);
                this.LocalTable[key].BaseTypeNavigationProperty.Add(vehicle);

                IEdmEntitySet entitySet = Request.GetModel().EntityContainer.FindEntitySet("InheritanceTests_Vehicles");

                string location = this.Url.CreateODataLink(
                        new EntitySetSegment(entitySet),
                        new KeySegment(new[] { new KeyValuePair<string, object>("Id", vehicle.Id) }, entitySet.EntityType(), null));

                return Created(location, vehicle);
            });
        }

        public Task<IEnumerable<Vehicle>> GetDerivedTypeNavigationProperty(int key)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.LocalTable[key].DerivedTypeNavigationProperty.OfType<Vehicle>();
            });
        }

        public Task<ITestActionResult> PostDerivedTypeNavigationProperty(int key, MiniSportBike vehicle)
        {
            return Task<ITestActionResult>.Factory.StartNew(() =>
            {
                new InheritanceTests_VehiclesController().LocalTable.AddOrUpdate(vehicle.Id, vehicle, (id, v) => vehicle);
                this.LocalTable[key].DerivedTypeNavigationProperty.Add(vehicle);

                IEdmEntitySet entitySet = Request.GetModel().EntityContainer.FindEntitySet("InheritanceTests_Vehicles");

                string location = this.Url.CreateODataLink(
                        new EntitySetSegment(entitySet),
                        new KeySegment(new[] { new KeyValuePair<string, object>("Id", vehicle.Id) }, entitySet.EntityType(), null));

                return Created(location, vehicle);
            });
        }

        public Task<ITestActionResult> DeleteRef(int key, string relatedKey, string navigationProperty)
        {
            return Task<ITestActionResult>.Factory.StartNew(() =>
            {
                var entity = this.LocalTable[key];
                switch (navigationProperty)
                {
                    case "BaseTypeNavigationProperty":
                        {
                            var vehicle = entity.BaseTypeNavigationProperty.FirstOrDefault(v => v.Id == Convert.ToInt32(relatedKey));
                            if (vehicle == null)
                            {
                                return NotFound();
                            }

                            entity.BaseTypeNavigationProperty.Remove(vehicle);
                        }
                        break;
                    case "DerivedTypeNavigationProperty":
                        {
                            var vehicle = entity.DerivedTypeNavigationProperty.FirstOrDefault(v => v.Id == Convert.ToInt32(relatedKey));
                            if (vehicle == null)
                            {
                                return NotFound();
                            }

                            entity.DerivedTypeNavigationProperty.Remove(vehicle);
                        }
                        break;

                    default:
                        return NotFound();
                }

                return Ok();
            });
        }
    }

    public class InheritanceTests_SportBikesController : InMemoryODataController<SportBike, int>
    {
        public InheritanceTests_SportBikesController()
            : base("Id")
        {
        }
    }

    public class InheritanceTests_CustomersController : InMemoryODataController<Customer, int>
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

    public abstract class InheritanceTests : ODataFormatterTestBase
    {
        public InheritanceTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        public static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();

            builder.EntitySet<MovingObject>("InheritanceTests_MovingObjects");
            builder.EntitySet<Vehicle>("InheritanceTests_Vehicles");
            builder.EntitySet<MiniSportBike>("InheritanceTests_MiniSportBikes");

            var cars = builder.EntitySet<Car>("InheritanceTests_Cars");
            cars.EntityType.Action("Wash");

            builder.OnModelCreating = mb =>
            {
                cars.HasNavigationPropertiesLink(
                    cars.EntityType.NavigationProperties,
                    (entityContext, navigationProperty) =>
                    {
                        object id;
                        entityContext.EdmObject.TryGetPropertyValue("Id", out id);
                        return new Uri(ResourceContextHelper.CreateODataLink(entityContext,
                            new EntitySetSegment(entityContext.NavigationSource as IEdmEntitySet),
                            new KeySegment(new[] { new KeyValuePair<string, object>("Id", id) }, entityContext.StructuredType as IEdmEntityType, null),
                            new NavigationPropertySegment(navigationProperty, null)));
                    },
                    false);

            };

            builder.EntityType<SportBike>().Action("Wash");
            builder.EntitySet<Customer>("InheritanceTests_Customers");

            return builder.GetEdmModel();
        }

        public override DataServiceContext WriterClient(Uri serviceRoot, ODataProtocolVersion protocolVersion)
        {
            return new Container(serviceRoot, protocolVersion);
        }

        public async Task PostGetUpdateAndDelete(Type entityType, string entitySetNam)
        {
            var testMethod = this.GetType().GetMethods()
                .Where(method => method.IsGenericMethod)
                .Where(method => method.Name == "PostGetUpdateAndDelete")
                .FirstOrDefault();

            var concreteTestMethod = testMethod.MakeGenericMethod(entityType);
            var rand = new Random(RandomSeedGenerator.GetRandomSeed());

            await (Task)concreteTestMethod.Invoke(this, new object[] { entitySetNam, rand });
        }

        private static T CreateNewEntity<T>(Random rand)
        {
            var retval = (T)InstanceCreator.CreateInstanceOf(typeof(T), rand, new CreatorSettings()
            {
                NullValueProbability = 0.0
            });

            return retval;
        }

        private static T UpdateEntityName<T>(T instance, Random rand)
        {
            var prop = typeof(T).GetProperty("Name");

            // the property Name should exist and must be an string
            Assert.NotNull(prop);
            Assert.Equal(typeof(string), prop.PropertyType);

            // set new value
            prop.SetValue(instance, InstanceCreator.CreateInstanceOf<string>(rand));

            return instance;
        }

        private static int GetEntityKey<T>(T instance)
        {
            var prop = typeof(T).GetProperty("Id");

            // the property Id should exist and must be an integer
            Assert.NotNull(prop);
            Assert.Equal(typeof(int), prop.PropertyType);

            // set new value
            return (int)prop.GetValue(instance);
        }

        public async Task PostGetUpdateAndDelete<T>(
            string entitySetName,
            Random rand)
            where T : class
        {
            // clear respository
            await this.ClearRepositoryAsync(entitySetName);

            // post new entity to repository
            T baseline = CreateNewEntity<T>(rand);
            var postResponse = await PostNewEntity<T>(baseline, entitySetName);

            // get collection of entities from repository
            var entities = await GetEntities<T>(entitySetName);
            var firstVersion = entities.FirstOrDefault();
            Assert.NotNull(firstVersion);
            AssertExtension.PrimitiveEqual(baseline, firstVersion);

            // update entity and verify if it's saved
            var updateTo = UpdateEntityName(firstVersion, rand);
            var updateResponse = await UpdateEntity<T>(firstVersion, updateTo, entitySetName);

            // retrieve the updated entity
            var entitiesAgain = await GetEntities<T>(entitySetName);
            var secondVersion = entitiesAgain.Where(entity => GetEntityKey(entity) == GetEntityKey(updateTo)).FirstOrDefault();
            Assert.NotNull(secondVersion);
            AssertExtension.PrimitiveEqual(updateTo, secondVersion);

            // delete entity
            var deleteResponse = await DeleteEntityAsync(secondVersion, entitySetName);

            // ensure that the entity has been deleted
            var entitiesFinal = await GetEntities<T>(entitySetName);
            Assert.Empty(entitiesFinal.ToList());
        }

        private async Task<DataServiceResponse> PostNewEntity<T>(T value, string entitySetName)
        {
            DataServiceContext writeClient = WriterClient(new Uri(this.BaseAddress), ODataProtocolVersion.V4);
            writeClient.AddObject(entitySetName, value);

            return await writeClient.SaveChangesAsync();
        }

        private async Task<IEnumerable<T>> GetEntities<T>(string entitySetName)
        {
            DataServiceContext readClient = ReaderClient(new Uri(this.BaseAddress), ODataProtocolVersion.V4);
            var query = readClient.CreateQuery<T>(entitySetName);

            return await query.ExecuteAsync();
        }

        private async Task<DataServiceResponse> DeleteEntityAsync<T>(T entity, string entitySetName)
        {
            DataServiceContext context = WriterClient(new Uri(BaseAddress), ODataProtocolVersion.V4);
            context.AttachTo(entitySetName, entity);
            context.DeleteObject(entity);

            return await context.SaveChangesAsync();
        }

        private async Task<DataServiceResponse> UpdateEntity<T>(T from, T to, string entitySetName)
        {
            DataServiceContext client = WriterClient(new Uri(BaseAddress), ODataProtocolVersion.V4);
            client.AttachTo(entitySetName, from);
            client.UpdateObject(to);

            return await client.SaveChangesAsync();
        }

        public virtual async Task AddAndRemoveBaseNavigationPropertyInDerivedType()
        {
            // clear respository
            await this.ClearRepositoryAsync("InheritanceTests_Cars");

            Random r = new Random(RandomSeedGenerator.GetRandomSeed());

            // post new entity to repository
            CreatorSettings creatorSettings = new CreatorSettings()
            {
                NullValueProbability = 0,
            };
            var car = InstanceCreator.CreateInstanceOf<Car>(r, creatorSettings);
            var vehicle = InstanceCreator.CreateInstanceOf<Vehicle>(r, creatorSettings);
            DataServiceContext ctx = WriterClient(new Uri(this.BaseAddress), ODataProtocolVersion.V4);
            ctx.AddObject("InheritanceTests_Cars", car);
            ctx.AddRelatedObject(car, "BaseTypeNavigationProperty", vehicle);
            await ctx.SaveChangesAsync();

            ctx = ReaderClient(new Uri(this.BaseAddress), ODataProtocolVersion.V4);
            var cars = ctx.CreateQuery<Car>("InheritanceTests_Cars");
            var actual = (await cars.ExecuteAsync()).First();
            await ctx.LoadPropertyAsync(actual, "BaseTypeNavigationProperty");

            AssertExtension.PrimitiveEqual(vehicle, actual.BaseTypeNavigationProperty[0]);

            ctx = WriterClient(new Uri(this.BaseAddress), ODataProtocolVersion.V4);
            ctx.AttachTo("InheritanceTests_Cars", actual);
            ctx.AttachTo("InheritanceTests_Vehicles", actual.BaseTypeNavigationProperty[0]);
            ctx.DeleteLink(actual, "BaseTypeNavigationProperty", actual.BaseTypeNavigationProperty[0]);
            await ctx.SaveChangesAsync();

            ctx = ReaderClient(new Uri(this.BaseAddress), ODataProtocolVersion.V4);
            cars = ctx.CreateQuery<Car>("InheritanceTests_Cars");
            actual = (await cars.ExecuteAsync()).First();
            await ctx.LoadPropertyAsync(actual, "BaseTypeNavigationProperty");

            Assert.Empty(actual.BaseTypeNavigationProperty);

            await this.ClearRepositoryAsync("InheritanceTests_Cars");
        }

        public virtual async Task AddAndRemoveDerivedNavigationPropertyInDerivedType()
        {
            // clear respository
            await this.ClearRepositoryAsync("InheritanceTests_Cars");

            Random r = new Random(RandomSeedGenerator.GetRandomSeed());

            // post new entity to repository
            CreatorSettings creatorSettings = new CreatorSettings()
            {
                NullValueProbability = 0,
            };
            var car = InstanceCreator.CreateInstanceOf<Car>(r, creatorSettings);
            var miniSportBike = InstanceCreator.CreateInstanceOf<MiniSportBike>(r, creatorSettings);
            DataServiceContext ctx = WriterClient(new Uri(this.BaseAddress), ODataProtocolVersion.V4);
            ctx.AddObject("InheritanceTests_Cars", car);
            ctx.AddRelatedObject(car, "DerivedTypeNavigationProperty", miniSportBike);
            await ctx.SaveChangesAsync();

            ctx = ReaderClient(new Uri(this.BaseAddress), ODataProtocolVersion.V4);
            var cars = ctx.CreateQuery<Car>("InheritanceTests_Cars");
            var actual = (await cars.ExecuteAsync()).First();
            await ctx.LoadPropertyAsync(actual, "DerivedTypeNavigationProperty");

            AssertExtension.PrimitiveEqual(miniSportBike, actual.DerivedTypeNavigationProperty[0]);

            ctx = WriterClient(new Uri(this.BaseAddress), ODataProtocolVersion.V4);
            ctx.AttachTo("InheritanceTests_Cars", actual);
            ctx.AttachTo("InheritanceTests_MiniSportBikes", actual.DerivedTypeNavigationProperty[0]);
            ctx.DeleteLink(actual, "DerivedTypeNavigationProperty", actual.DerivedTypeNavigationProperty[0]);
            await ctx.SaveChangesAsync();

            ctx = ReaderClient(new Uri(this.BaseAddress), ODataProtocolVersion.V4);
            cars = ctx.CreateQuery<Car>("InheritanceTests_Cars");
            actual = (await cars.ExecuteAsync()).First();
            await ctx.LoadPropertyAsync(actual, "DerivedTypeNavigationProperty");

            Assert.Empty(actual.DerivedTypeNavigationProperty);

            await this.ClearRepositoryAsync("InheritanceTests_Cars");
        }

        public virtual async Task CreateAndDeleteLinkToDerivedNavigationPropertyOnBaseEntitySet()
        {
            // clear respository
            await this.ClearRepositoryAsync("InheritanceTests_Vehicles");

            Random r = new Random(RandomSeedGenerator.GetRandomSeed());

            // post new entity to repository
            CreatorSettings creatorSettings = new CreatorSettings()
            {
                NullValueProbability = 0,
            };
            var car = InstanceCreator.CreateInstanceOf<Car>(r, creatorSettings);
            var vehicle = InstanceCreator.CreateInstanceOf<MiniSportBike>(r, creatorSettings);
            DataServiceContext ctx = WriterClient(new Uri(this.BaseAddress), ODataProtocolVersion.V4);
            ctx.AddObject("InheritanceTests_Vehicles", car);
            ctx.AddObject("InheritanceTests_Vehicles", vehicle);
            await ctx.SaveChangesAsync();

            ctx.SetLink(car, "SingleNavigationProperty", vehicle);
            await ctx.SaveChangesAsync();

            ctx = ReaderClient(new Uri(this.BaseAddress), ODataProtocolVersion.V4);
            var cars = (await ctx.CreateQuery<Vehicle>("InheritanceTests_Vehicles").ExecuteAsync()).ToList().OfType<Car>();
            var actual = cars.First();
            await ctx.LoadPropertyAsync(actual, "SingleNavigationProperty");
            AssertExtension.PrimitiveEqual(vehicle, actual.SingleNavigationProperty);

            await this.ClearRepositoryAsync("InheritanceTests_Vehicles");
        }

        public virtual async Task InvokeActionWithOverloads(string actionUrl)
        {
            DataServiceContext ctx = new DataServiceContext(new Uri(this.BaseAddress), ODataProtocolVersion.V4);

            var result = await ctx.ExecuteAsync(new Uri(this.BaseAddress + actionUrl), "POST");

            Assert.Equal(204, result.StatusCode);
        }
    }
}
