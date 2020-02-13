// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Models.Vehicle;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class InheritanceQueryable_Customer
    {
        public ICollection<Vehicle> Vehicles { get; set; }

        public ICollection<Vehicle> AnotherVehicles { get; set; }

        public Vehicle Vehicle { get; set; }
    }

    public abstract class InheritanceQueryable_AbstractBase
    {
        public int ID { get; set; }
        public List<string> Links { get; set; }
        public NameValueCollection CustomCollection { get; set; }
        public InheritanceQueryable_DerivedType EntityProperty { get; set; }
        public ReadOnlyPropertyType ComplexProperty { get; set; }
    }

    public class InheritanceQueryable_DerivedType : InheritanceQueryable_AbstractBase
    {
        public string Name { get; set; }
    }

    public class ReadOnlyPropertyType
    {
        public int ReadOnlyProperty
        {
            get
            {
                return 8;
            }
        }
    }

    public class InheritanceQueryableController : TestNonODataController
    {
        public IQueryable GetMotorcycles()
        {
            return new Motorcycle[] 
            {
                new Motorcycle()
                {
                    CanDoAWheelie = true,
                    Id = 1,
                    Model = "abc",
                    Name = "xxx"
                },
                new MiniSportBike()
                {
                    CanDoAWheelie = false,
                    Id = 2,
                    Model = "def",
                    Name = "zzz",
                    TopSpeed = 30
                }
            }.AsQueryable();
        }

        public IQueryable GetCustomers()
        {
            var customers = new InheritanceQueryable_Customer[] 
            { 
                new InheritanceQueryable_Customer 
                {
                    Vehicles = new Collection<Vehicle>(new Vehicle[]
                    {
                        new Motorcycle()
                        {
                            CanDoAWheelie = true,
                            Id = 1,
                            Model = "abc",
                            Name = "xxx"
                        },
                        new MiniSportBike()
                        {
                            CanDoAWheelie = false,
                            Id = 2,
                            Model = "def",
                            Name = "zzz",
                            TopSpeed = 30
                        }
                    }),
                    Vehicle = new Car
                    {
                        Model = "abc",
                        SeatingCapacity = 4
                    }
                },
                new InheritanceQueryable_Customer 
                {
                    Vehicles = new Collection<Vehicle>(new Vehicle[]
                    {
                        new Motorcycle()
                        {
                            CanDoAWheelie = true,
                            Id = 1,
                            Model = "abc",
                            Name = "xxx"
                        },
                        new MiniSportBike()
                        {
                            CanDoAWheelie = false,
                            Id = 2,
                            Model = "def",
                            Name = "zzz",
                            TopSpeed = 30
                        }
                    }),
                    Vehicle = new Car
                    {
                        Model = "abc",
                        SeatingCapacity = 4
                    }
                }
            };

            return customers.AsQueryable();
        }

        public IQueryable GetDerivedTypeWithAbstractBase()
        {
            return new InheritanceQueryable_AbstractBase[] 
            {
                new InheritanceQueryable_DerivedType
                {
                    ID = 1,
                    Name = "First",
                    EntityProperty = new InheritanceQueryable_DerivedType
                    {
                        ID = 3,
                        Name = "Third"
                    },
                    ComplexProperty = new ReadOnlyPropertyType()
                },
                new InheritanceQueryable_DerivedType
                {
                    ID = 2,
                    Name = "Second",
                    EntityProperty = new InheritanceQueryable_DerivedType
                    {
                        ID = 4,
                        Name = "Fourth"
                    },
                    ComplexProperty = new ReadOnlyPropertyType()
                }
            }.AsQueryable();
        }

        public IQueryable GetReadOnlyPropertyType()
        {
            return new ReadOnlyPropertyType[] 
            { 
                new ReadOnlyPropertyType()
            }.AsQueryable();
        }
    }

    public class InheritanceQueryableTests : WebHostTestBase<InheritanceQueryableTests>
    {
        public InheritanceQueryableTests(WebHostTestFixture<InheritanceQueryableTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            var types = new[] {
                typeof(InheritanceQueryable_Customer),
                typeof(InheritanceQueryable_AbstractBase),
                typeof(InheritanceQueryable_DerivedType),
                typeof(Vehicle),
                typeof(Motorcycle),
                typeof(MiniSportBike),
                typeof(SportBike),
                typeof(NameValueCollection),
                typeof(ReadOnlyPropertyType),
                typeof(InheritanceQueryableController) };

            configuration.AddControllers(types);

            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.AddODataQueryFilter();
            configuration.EnableDependencyInjection();
        }

        [Theory]
        [InlineData("/api/InheritanceQueryable/GetMotorcycles?$filter=Id eq 1")]
        [InlineData("/api/InheritanceQueryable/GetMotorcycles?$filter=Microsoft.Test.E2E.AspNet.OData.Common.Models.Vehicle.MiniSportBike/CanDoAWheelie eq false")]
        [InlineData("/api/InheritanceQueryable/GetMotorcycles?$filter=Microsoft.Test.E2E.AspNet.OData.Common.Models.Vehicle.MiniSportBike/TopSpeed gt 20")]
        public async Task TestSimpleInheritanceModel(string url)
        {
            var response = await this.Client.GetAsync(this.BaseAddress + url);
            response.EnsureSuccessStatusCode();
            var actual = await response.Content.ReadAsObject<IEnumerable<Motorcycle>>();
        }

        [Fact]
        public async Task QueryOnDerivedTypeWithAbstractBaseShouldWork()
        {
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/InheritanceQueryable/GetDerivedTypeWithAbstractBase?$filter=Microsoft.Test.E2E.AspNet.OData.QueryComposition.InheritanceQueryable_DerivedType/ID eq 1");
            response.EnsureSuccessStatusCode();
            var actual = await response.Content.ReadAsObject<IEnumerable<InheritanceQueryable_DerivedType>>();
            Assert.Equal("First", actual.First().Name);

            response = await this.Client.GetAsync(this.BaseAddress + "/api/InheritanceQueryable/GetDerivedTypeWithAbstractBase?$filter=Microsoft.Test.E2E.AspNet.OData.QueryComposition.InheritanceQueryable_DerivedType/Name eq 'First'");
            response.EnsureSuccessStatusCode();
            actual = await response.Content.ReadAsObject<IEnumerable<InheritanceQueryable_DerivedType>>();
            Assert.Equal(1, actual.First().ID);

            response = await this.Client.GetAsync(this.BaseAddress + "/api/InheritanceQueryable/GetDerivedTypeWithAbstractBase?$filter=Microsoft.Test.E2E.AspNet.OData.QueryComposition.InheritanceQueryable_DerivedType/EntityProperty/Name eq 'Fourth'");
            response.EnsureSuccessStatusCode();
            actual = await response.Content.ReadAsObject<IEnumerable<InheritanceQueryable_DerivedType>>();
            Assert.Equal(2, actual.First().ID);

            response = await this.Client.GetAsync(this.BaseAddress + "/api/InheritanceQueryable/GetDerivedTypeWithAbstractBase?$filter=Microsoft.Test.E2E.AspNet.OData.QueryComposition.InheritanceQueryable_DerivedType/ComplexProperty/ReadOnlyProperty eq 8");
            response.EnsureSuccessStatusCode();
            actual = await response.Content.ReadAsObject<IEnumerable<InheritanceQueryable_DerivedType>>();
            Assert.Equal(2, actual.Count());
        }

        [Fact]
        public async Task QueryOnReadOnlyPropertShouldWork()
        {
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/InheritanceQueryable/GetReadOnlyPropertyType?$filter=ReadOnlyProperty eq 8");
            response.EnsureSuccessStatusCode();
            var actual = await response.Content.ReadAsObject<IEnumerable<ReadOnlyPropertyType>>();
            Assert.Single(actual);

            response = await this.Client.GetAsync(this.BaseAddress + "/api/InheritanceQueryable/GetReadOnlyPropertyType?$filter=ReadOnlyProperty eq 7");
            response.EnsureSuccessStatusCode();
            actual = await response.Content.ReadAsObject<IEnumerable<ReadOnlyPropertyType>>();
            Assert.Empty(actual);
        }
    }
}
