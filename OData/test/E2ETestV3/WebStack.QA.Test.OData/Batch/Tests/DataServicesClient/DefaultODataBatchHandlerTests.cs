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
    public class DefaultBatchCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<DefaultBatchOrder> Orders { get; set; }
    }

    public class DefaultBatchOrder
    {
        public int Id { get; set; }
        public DateTime PurchaseDate { get; set; }
    }

    public class DefaultBatchCustomerController : InMemoryEntitySetController<DefaultBatchCustomer, int>
    {
        private static bool _initialized;
        public DefaultBatchCustomerController()
            : base("Id")
        {
            if (!_initialized)
            {
                IList<DefaultBatchCustomer> customers = Enumerable.Range(0, 10).Select(i =>
                   new DefaultBatchCustomer
                   {
                       Id = i,
                       Name = string.Format("Name {0}", i)
                   }).ToList();
                foreach (DefaultBatchCustomer customer in customers)
                {
                    LocalTable.AddOrUpdate(customer.Id, customer, (key, oldEntity) => oldEntity);
                }
                _initialized = true;
            }
        }

        protected override Task<DefaultBatchCustomer> CreateEntityAsync(DefaultBatchCustomer entity)
        {
            if (entity.Id < 0)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
            }
            return base.CreateEntityAsync(entity);
        }

        [EnableQuery]
        public IQueryable<DefaultBatchCustomer> OddCustomers()
        {
            return base.LocalTable.Where(x => x.Key % 2 == 1).Select(x => x.Value).AsQueryable();
        }

        public override Task CreateLink([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            return Task.FromResult(Request.CreateResponse(HttpStatusCode.NoContent));
        }
    }

    public class DefaultBatchOrderController : InMemoryEntitySetController<DefaultBatchOrder, int>
    {
        public DefaultBatchOrderController()
            : base("Id")
        {
        }
    }

    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class DefaultBatchHandlerCUDBatchTests : IODataTestBase
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
            configuration.Routes.MapODataServiceRoute("batch", "DefaultBatch", GetEdmModel(),
                new DefaultODataBatchHandler(server));
        }


        protected static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<DefaultBatchCustomer> customers = builder.EntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");
            EntitySetConfiguration<DefaultBatchOrder> orders = builder.EntitySet<DefaultBatchOrder>("DefaultBatchOrder");
            customers.EntityType.Collection.Action("OddCustomers").ReturnsCollectionFromEntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");
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
            Uri serviceUrl = new Uri(BaseAddress + "/DefaultBatch");
            var client = new Default.Container(serviceUrl);
            client.Format.UseJson();

            IList<Default.DefaultBatchCustomer> customers = client.DefaultBatchCustomer.ToList();

            Default.DefaultBatchCustomer customerToDelete = customers.First();
            Default.DefaultBatchCustomer customerToUpdate = customers.Skip(1).First();
            Default.DefaultBatchCustomer customerToAdd = new Default.DefaultBatchCustomer { Id = 10, Name = "Customer 10" };

            client.DeleteObject(customerToDelete);
            customerToUpdate.Name = "Updated customer name";
            client.UpdateObject(customerToUpdate);
            client.AddToDefaultBatchCustomer(customerToAdd);

            client.SaveChanges(SaveChangesOptions.Batch);

            var newClient = new Default.Container(serviceUrl);
            IList<Default.DefaultBatchCustomer> changedCustomers = newClient.DefaultBatchCustomer.ToList();

            Assert.False(changedCustomers.Any(x => x.Id == customerToDelete.Id));
            Assert.Equal(customerToUpdate.Name, changedCustomers.Single(x => x.Id == customerToUpdate.Id).Name);
            Assert.Single(changedCustomers, x => x.Id == 10);

        }

    }

    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class DefaultBatchHandlerQueryBatchTests : IODataTestBase
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
            configuration.Routes.MapODataServiceRoute("batch", "DefaultBatch", GetEdmModel(),
                new DefaultODataBatchHandler(server));
        }


        protected static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<DefaultBatchCustomer> customers = builder.EntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");
            EntitySetConfiguration<DefaultBatchOrder> orders = builder.EntitySet<DefaultBatchOrder>("DefaultBatchOrder");
            customers.EntityType.Collection.Action("OddCustomers").ReturnsCollectionFromEntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");
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
            Uri serviceUrl = new Uri(BaseAddress + "/DefaultBatch");
            Default.Container client = new Default.Container(serviceUrl);
            client.Format.UseJson();
            Uri customersRequestUri = new Uri(BaseAddress + "/DefaultBatch/DefaultBatchCustomer");
            DataServiceRequest<Default.DefaultBatchCustomer> customersRequest = new DataServiceRequest<Default.DefaultBatchCustomer>(customersRequestUri);
            Uri singleCustomerRequestUri = new Uri(BaseAddress + "/DefaultBatch/DefaultBatchCustomer(0)");
            DataServiceRequest<Default.DefaultBatchCustomer> singleCustomerRequest = new DataServiceRequest<Default.DefaultBatchCustomer>(singleCustomerRequestUri);

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
                    Assert.Equal(10, response.Cast<Default.DefaultBatchCustomer>().Count());
                    continue;
                }
                if (response.Query.RequestUri == singleCustomerRequestUri)
                {
                    Assert.Equal(1, response.Cast<Default.DefaultBatchCustomer>().Count());
                    continue;
                }
            }
        }
    }

    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class DefaultBatchHandlerErrorsBatchTests : IODataTestBase
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
            configuration.Routes.MapODataServiceRoute("batch", "DefaultBatch", GetEdmModel(),
                new DefaultODataBatchHandler(server));
        }


        protected static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<DefaultBatchCustomer> customers = builder.EntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");
            EntitySetConfiguration<DefaultBatchOrder> orders = builder.EntitySet<DefaultBatchOrder>("DefaultBatchOrder");
            customers.EntityType.Collection.Action("OddCustomers").ReturnsCollectionFromEntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");
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
            Uri serviceUrl = new Uri(BaseAddress + "/DefaultBatch");
            Default.Container client = new Default.Container(serviceUrl);
            client.Format.UseJson();

            Default.DefaultBatchCustomer validCustomer = new Default.DefaultBatchCustomer()
            {
                Id = 10,
                Name = "Customer 10"
            };

            Default.DefaultBatchCustomer invalidCustomer = new Default.DefaultBatchCustomer()
            {
                Id = -1,
                Name = "Customer -1"
            };

            client.AddToDefaultBatchCustomer(validCustomer);
            client.AddToDefaultBatchCustomer(invalidCustomer);
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
    public class DefaultBatchHandlerLinksBatchTests : IODataTestBase
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
            configuration.Routes.MapODataServiceRoute("batch", "DefaultBatch", GetEdmModel(),
                new DefaultODataBatchHandler(server));
        }

        protected static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<DefaultBatchCustomer> customers = builder.EntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");
            EntitySetConfiguration<DefaultBatchOrder> orders = builder.EntitySet<DefaultBatchOrder>("DefaultBatchOrder");
            customers.EntityType.Collection.Action("OddCustomers").ReturnsCollectionFromEntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");
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
            Uri serviceUrl = new Uri(BaseAddress + "/DefaultBatch");
            Default.Container client = new Default.Container(serviceUrl);
            client.Format.UseJson();

            Default.DefaultBatchCustomer customer = client.DefaultBatchCustomer.ToList().First();
            Default.DefaultBatchOrder order = new Default.DefaultBatchOrder() { Id = 0, PurchaseDate = DateTime.Now };

            client.AddToDefaultBatchOrder(order);

            client.AddLink(customer, "Orders", order);

            client.SaveChanges(SaveChangesOptions.Batch);
        }
    }
}
