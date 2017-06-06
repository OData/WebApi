using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Batch;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using Microsoft.Data.Edm;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using Xunit;

namespace WebStack.QA.Test.OData.Batch.Tests.DataServicesClient
{
    public class UnbufferedBatchCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<UnbufferedBatchOrder> Orders { get; set; }
    }

    public class UnbufferedBatchOrder
    {
        public int Id { get; set; }
        public DateTime PurchaseDate { get; set; }
    }

    public class UnbufferedBatchCustomerController : InMemoryEntitySetController<UnbufferedBatchCustomer, int>
    {
        private static bool _initialized;
        public UnbufferedBatchCustomerController()
            : base("Id")
        {
            if (!_initialized)
            {
                IList<UnbufferedBatchCustomer> customers = Enumerable.Range(0, 10).Select(i =>
                   new UnbufferedBatchCustomer
                   {
                       Id = i,
                       Name = string.Format("Name {0}", i)
                   }).ToList();
                foreach (UnbufferedBatchCustomer customer in customers)
                {
                    LocalTable.AddOrUpdate(customer.Id, customer, (key, oldEntity) => oldEntity);
                }
                _initialized = true;
            }
        }

        protected override Task<UnbufferedBatchCustomer> CreateEntityAsync(UnbufferedBatchCustomer entity)
        {
            if (entity.Id < 0)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
            }
            return base.CreateEntityAsync(entity);
        }

        [EnableQuery]
        public IQueryable<UnbufferedBatchCustomer> OddCustomers()
        {
            return base.LocalTable.Where(x => x.Key % 2 == 1).Select(x => x.Value).AsQueryable();
        }

        public override Task CreateLink([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            return Task.FromResult(Request.CreateResponse(HttpStatusCode.NoContent));
        }
    }

    public class UnbufferedBatchOrderController : InMemoryEntitySetController<UnbufferedBatchOrder, int>
    {
        public UnbufferedBatchOrderController()
            : base("Id")
        {
        }
    }

    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class CUDBatchTests : IODataTestBase
    {
        private string baseAddress = null;

        [NuwaBaseAddress]
        public string BaseAddress
        {
            get
            {
                return baseAddress;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.baseAddress = value.Replace("localhost", Environment.MachineName);
                }
            }
        }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            HttpServer server = configuration.Properties["Nuwa.HttpServerKey"] as HttpServer;

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Routes.MapODataServiceRoute("batch", "UnbufferedBatch", GetEdmModel(),
                new UnbufferedODataBatchHandler(server));
        }


        protected static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<UnbufferedBatchCustomer> customers = builder.EntitySet<UnbufferedBatchCustomer>("UnbufferedBatchCustomer");
            EntitySetConfiguration<UnbufferedBatchOrder> orders = builder.EntitySet<UnbufferedBatchOrder>("UnbufferedBatchOrder");
            customers.EntityType.Collection.Action("OddCustomers").ReturnsCollectionFromEntitySet<UnbufferedBatchCustomer>("UnbufferedBatchCustomer");
            builder.MaxDataServiceVersion = builder.DataServiceVersion;
            return builder.GetEdmModel();
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper webConfig)
        {
            webConfig.AddAppSection("aspnet:UseTaskFriendlySynchronizationContext", "true");
        }

        [Fact]
        public void CanPerformCudOperationsOnABatch()
        {
            Uri serviceUrl = new Uri(BaseAddress + "/UnbufferedBatch");
            var client = new Unbuffered.Container(serviceUrl);
            client.Format.UseJson();

            IList<Unbuffered.UnbufferedBatchCustomer> customers = client.UnbufferedBatchCustomer.ToList();

            Unbuffered.UnbufferedBatchCustomer customerToDelete = customers.First();
            Unbuffered.UnbufferedBatchCustomer customerToUpdate = customers.Skip(1).First();
            Unbuffered.UnbufferedBatchCustomer customerToAdd = new Unbuffered.UnbufferedBatchCustomer { Id = 10, Name = "Customer 10" };

            client.DeleteObject(customerToDelete);
            customerToUpdate.Name = "Updated customer name";
            client.UpdateObject(customerToUpdate);
            client.AddToUnbufferedBatchCustomer(customerToAdd);

            client.SaveChanges(SaveChangesOptions.Batch);

            var newClient = new Unbuffered.Container(serviceUrl);
            IList<Unbuffered.UnbufferedBatchCustomer> changedCustomers = newClient.UnbufferedBatchCustomer.ToList();

            Assert.False(changedCustomers.Any(x => x.Id == customerToDelete.Id));
            Assert.Equal(customerToUpdate.Name, changedCustomers.Single(x => x.Id == customerToUpdate.Id).Name);
            Assert.Single(changedCustomers, x => x.Id == 10);

        }

    }

    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class QueryBatchTests : IODataTestBase
    {
        private string baseAddress = null;

        [NuwaBaseAddress]
        public string BaseAddress
        {
            get
            {
                return baseAddress;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.baseAddress = value.Replace("localhost", Environment.MachineName);
                }
            }
        }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            HttpServer server = configuration.Properties["Nuwa.HttpServerKey"] as HttpServer;

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Routes.MapODataServiceRoute("batch", "UnbufferedBatch", GetEdmModel(),
                new UnbufferedODataBatchHandler(server));
        }


        protected static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<UnbufferedBatchCustomer> customers = builder.EntitySet<UnbufferedBatchCustomer>("UnbufferedBatchCustomer");
            EntitySetConfiguration<UnbufferedBatchOrder> orders = builder.EntitySet<UnbufferedBatchOrder>("UnbufferedBatchOrder");
            customers.EntityType.Collection.Action("OddCustomers").ReturnsCollectionFromEntitySet<UnbufferedBatchCustomer>("UnbufferedBatchCustomer");
            builder.MaxDataServiceVersion = builder.DataServiceVersion;
            return builder.GetEdmModel();
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper webConfig)
        {
            webConfig.AddAppSection("aspnet:UseTaskFriendlySynchronizationContext", "true");
        }

        [Fact]
        public void CanBatchQueriesWithDataServicesClient()
        {
            Uri serviceUrl = new Uri(BaseAddress + "/UnbufferedBatch");
            Unbuffered.Container client = new Unbuffered.Container(serviceUrl);
            client.Format.UseJson();
            Uri customersRequestUri = new Uri(BaseAddress + "/UnbufferedBatch/UnbufferedBatchCustomer");
            DataServiceRequest<Unbuffered.UnbufferedBatchCustomer> customersRequest = new DataServiceRequest<Unbuffered.UnbufferedBatchCustomer>(customersRequestUri);
            Uri singleCustomerRequestUri = new Uri(BaseAddress + "/UnbufferedBatch/UnbufferedBatchCustomer(0)");
            DataServiceRequest<Unbuffered.UnbufferedBatchCustomer> singleCustomerRequest = new DataServiceRequest<Unbuffered.UnbufferedBatchCustomer>(singleCustomerRequestUri);

            DataServiceResponse batchResponse = client.ExecuteBatch(customersRequest, singleCustomerRequest);

            if (batchResponse.IsBatchResponse)
            {
                Assert.Equal(202, batchResponse.BatchStatusCode);
            }

            foreach (QueryOperationResponse response in batchResponse)
            {
                Assert.Equal(200, response.StatusCode);
                if (response.Query.RequestUri == customersRequestUri)
                {
                    Assert.Equal(10, response.Cast<Unbuffered.UnbufferedBatchCustomer>().Count());
                    continue;
                }
                if (response.Query.RequestUri == singleCustomerRequestUri)
                {
                    Assert.Equal(1, response.Cast<Unbuffered.UnbufferedBatchCustomer>().Count());
                    continue;
                }
            }
        }
    }

    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class ErrorsBatchTests : IODataTestBase
    {
        private string baseAddress = null;

        [NuwaBaseAddress]
        public string BaseAddress
        {
            get
            {
                return baseAddress;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.baseAddress = value.Replace("localhost", Environment.MachineName);
                }
            }
        }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            HttpServer server = configuration.Properties["Nuwa.HttpServerKey"] as HttpServer;

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Routes.MapODataServiceRoute("batch", "UnbufferedBatch", GetEdmModel(),
                new UnbufferedODataBatchHandler(server));
        }


        protected static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<UnbufferedBatchCustomer> customers = builder.EntitySet<UnbufferedBatchCustomer>("UnbufferedBatchCustomer");
            EntitySetConfiguration<UnbufferedBatchOrder> orders = builder.EntitySet<UnbufferedBatchOrder>("UnbufferedBatchOrder");
            customers.EntityType.Collection.Action("OddCustomers").ReturnsCollectionFromEntitySet<UnbufferedBatchCustomer>("UnbufferedBatchCustomer");
            builder.MaxDataServiceVersion = builder.DataServiceVersion;
            return builder.GetEdmModel();
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper webConfig)
        {
            webConfig.AddAppSection("aspnet:UseTaskFriendlySynchronizationContext", "true");
        }

        [Fact]
        public virtual void SendsIndividualErrorWhenOneOfTheRequestsFails()
        {
            Uri serviceUrl = new Uri(BaseAddress + "/UnbufferedBatch");
            Unbuffered.Container client = new Unbuffered.Container(serviceUrl);
            client.Format.UseJson();

            Unbuffered.UnbufferedBatchCustomer validCustomer = new Unbuffered.UnbufferedBatchCustomer()
            {
                Id = 10,
                Name = "Customer 10"
            };

            Unbuffered.UnbufferedBatchCustomer invalidCustomer = new Unbuffered.UnbufferedBatchCustomer()
            {
                Id = -1,
                Name = "Customer -1"
            };

            client.AddToUnbufferedBatchCustomer(validCustomer);
            client.AddToUnbufferedBatchCustomer(invalidCustomer);
            DataServiceRequestException exception = Assert.Throws<DataServiceRequestException>(() =>
            {
                DataServiceResponse response = client.SaveChanges(SaveChangesOptions.Batch);
            });

            Assert.Equal(202, exception.Response.BatchStatusCode);
            Assert.Equal(1, exception.Response.Count());
        }
    }

    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class LinksBatchTests : IODataTestBase
    {
        private string baseAddress = null;

        [NuwaBaseAddress]
        public string BaseAddress
        {
            get
            {
                return baseAddress;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.baseAddress = value.Replace("localhost", Environment.MachineName);
                }
            }
        }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            HttpServer server = configuration.Properties["Nuwa.HttpServerKey"] as HttpServer;

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Routes.MapODataServiceRoute("batch", "UnbufferedBatch", GetEdmModel(),
                new UnbufferedODataBatchHandler(server));
        }

        protected static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<UnbufferedBatchCustomer> customers = builder.EntitySet<UnbufferedBatchCustomer>("UnbufferedBatchCustomer");
            EntitySetConfiguration<UnbufferedBatchOrder> orders = builder.EntitySet<UnbufferedBatchOrder>("UnbufferedBatchOrder");
            customers.EntityType.Collection.Action("OddCustomers").ReturnsCollectionFromEntitySet<UnbufferedBatchCustomer>("UnbufferedBatchCustomer");
            builder.MaxDataServiceVersion = builder.DataServiceVersion;
            return builder.GetEdmModel();
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper webConfig)
        {
            webConfig.AddAppSection("aspnet:UseTaskFriendlySynchronizationContext", "true");
        }

        [Fact]
        public virtual void CanSetLinksInABatchWithDataServicesClient()
        {
            Uri serviceUrl = new Uri(BaseAddress + "/UnbufferedBatch");
            Unbuffered.Container client = new Unbuffered.Container(serviceUrl);
            client.Format.UseJson();

            Unbuffered.UnbufferedBatchCustomer customer = client.UnbufferedBatchCustomer.ToList().First();
            Unbuffered.UnbufferedBatchOrder order = new Unbuffered.UnbufferedBatchOrder() { Id = 0, PurchaseDate = DateTime.Now };

            client.AddToUnbufferedBatchOrder(order);

            client.AddLink(customer, "Orders", order);

            client.SaveChanges(SaveChangesOptions.Batch);
        }
    }
}
