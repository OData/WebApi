using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Extensions;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Models.Vehicle;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.QueryComposition
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

    public class InheritanceQueryableController : ApiController
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

    public class InheritanceQueryableTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
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

            var resolver = new TestAssemblyResolver(new TypesInjectionAssembly(types));
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.AddODataQueryFilter();
            configuration.EnableDependencyInjection("api default");
        }

        [Theory]
        [InlineData("/api/InheritanceQueryable/GetMotorcycles?$filter=Id eq 1")]
        [InlineData("/api/InheritanceQueryable/GetMotorcycles?$filter=WebStack.QA.Test.OData.Common.Models.Vehicle.MiniSportBike/CanDoAWheelie eq false")]
        [InlineData("/api/InheritanceQueryable/GetMotorcycles?$filter=WebStack.QA.Test.OData.Common.Models.Vehicle.MiniSportBike/TopSpeed gt 20")]
        public void TestSimpleInheritanceModel(string url)
        {
            var response = this.Client.GetAsync(this.BaseAddress + url).Result;
            response.EnsureSuccessStatusCode();
            var actual = response.Content.ReadAsAsync<IEnumerable<Motorcycle>>().Result;
        }

        [Fact]
        public void QueryOnDerivedTypeWithAbstractBaseShouldWork()
        {
            var response = this.Client.GetAsync(this.BaseAddress + "/api/InheritanceQueryable/GetDerivedTypeWithAbstractBase?$filter=WebStack.QA.Test.OData.QueryComposition.InheritanceQueryable_DerivedType/ID eq 1").Result;
            response.EnsureSuccessStatusCode();
            var actual = response.Content.ReadAsAsync<IEnumerable<InheritanceQueryable_DerivedType>>().Result;
            Assert.Equal("First", actual.First().Name);

            response = this.Client.GetAsync(this.BaseAddress + "/api/InheritanceQueryable/GetDerivedTypeWithAbstractBase?$filter=WebStack.QA.Test.OData.QueryComposition.InheritanceQueryable_DerivedType/Name eq 'First'").Result;
            response.EnsureSuccessStatusCode();
            actual = response.Content.ReadAsAsync<IEnumerable<InheritanceQueryable_DerivedType>>().Result;
            Assert.Equal(1, actual.First().ID);

            response = this.Client.GetAsync(this.BaseAddress + "/api/InheritanceQueryable/GetDerivedTypeWithAbstractBase?$filter=WebStack.QA.Test.OData.QueryComposition.InheritanceQueryable_DerivedType/EntityProperty/Name eq 'Fourth'").Result;
            response.EnsureSuccessStatusCode();
            actual = response.Content.ReadAsAsync<IEnumerable<InheritanceQueryable_DerivedType>>().Result;
            Assert.Equal(2, actual.First().ID);

            response = this.Client.GetAsync(this.BaseAddress + "/api/InheritanceQueryable/GetDerivedTypeWithAbstractBase?$filter=WebStack.QA.Test.OData.QueryComposition.InheritanceQueryable_DerivedType/ComplexProperty/ReadOnlyProperty eq 8").Result;
            response.EnsureSuccessStatusCode();
            actual = response.Content.ReadAsAsync<IEnumerable<InheritanceQueryable_DerivedType>>().Result;
            Assert.Equal(2, actual.Count());
        }

        [Fact]
        public void QueryOnReadOnlyPropertShouldWork()
        {
            var response = this.Client.GetAsync(this.BaseAddress + "/api/InheritanceQueryable/GetReadOnlyPropertyType?$filter=ReadOnlyProperty eq 8").Result;
            response.EnsureSuccessStatusCode();
            var actual = response.Content.ReadAsAsync<IEnumerable<ReadOnlyPropertyType>>().Result;
            Assert.Equal(1, actual.Count());

            response = this.Client.GetAsync(this.BaseAddress + "/api/InheritanceQueryable/GetReadOnlyPropertyType?$filter=ReadOnlyProperty eq 7").Result;
            response.EnsureSuccessStatusCode();
            actual = response.Content.ReadAsAsync<IEnumerable<ReadOnlyPropertyType>>().Result;
            Assert.Equal(0, actual.Count());
        }
    }
}
