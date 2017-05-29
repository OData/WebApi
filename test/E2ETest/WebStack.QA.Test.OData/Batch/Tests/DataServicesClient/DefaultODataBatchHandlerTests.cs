using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Batch;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Client;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using WebStack.QA.Test.OData.ModelBuilder;
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
        public DateTimeOffset PurchaseDate { get; set; }
    }

    public class DefaultBatchCustomerController : InMemoryODataController<DefaultBatchCustomer, int>
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

        public Task CreateRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            return Task.FromResult(Request.CreateResponse(HttpStatusCode.NoContent));
        }
    }

    public class DefaultBatchOrderController : InMemoryODataController<DefaultBatchOrder, int>
    {
        public DefaultBatchOrderController()
            : base("Id")
        {
        }
    }

    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class DefaultBatchHandlerCUDBatchTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            HttpServer server = configuration.Properties["Nuwa.HttpServerKey"] as HttpServer;

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.MapODataServiceRoute(
                "batch",
                "DefaultBatch",
                GetEdmModel(),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
                new DefaultODataBatchHandler(server));
        }

        protected static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<DefaultBatchCustomer> customers = builder.EntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");
            EntitySetConfiguration<DefaultBatchOrder> orders = builder.EntitySet<DefaultBatchOrder>("DefaultBatchOrder");
            customers.EntityType.Collection.Action("OddCustomers").ReturnsCollectionFromEntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");
            builder.MaxDataServiceVersion = builder.DataServiceVersion;
            builder.Namespace = typeof(DefaultBatchCustomer).Namespace;
            return builder.GetEdmModel();
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper webConfig)
        {
            webConfig.AddAppSection("aspnet:UseTaskFriendlySynchronizationContext", "true");
        }

        [Fact]
        public async Task CanPerformCudOperationsOnABatch()
        {
            Uri serviceUrl = new Uri(BaseAddress + "/DefaultBatch");
            var client = new DefaultBatchProxy.Container(serviceUrl);
            client.Format.UseJson();

            var customers = await client.DefaultBatchCustomer.ExecuteAsync();
            List<DefaultBatchProxy.DefaultBatchCustomer> customerList = customers.ToList();

            DefaultBatchProxy.DefaultBatchCustomer customerToDelete = customerList[0];
            DefaultBatchProxy.DefaultBatchCustomer customerToUpdate = customerList[1];
            DefaultBatchProxy.DefaultBatchCustomer customerToAdd = new DefaultBatchProxy.DefaultBatchCustomer { Id = 10, Name = "Customer 10" };

            client.DeleteObject(customerToDelete);

            customerToUpdate.Name = "Updated customer name";
            client.UpdateObject(customerToUpdate);

            client.AddToDefaultBatchCustomer(customerToAdd);

            var response = await client.SaveChangesAsync(SaveChangesOptions.BatchWithSingleChangeset);

            var newClient = new DefaultBatchProxy.Container(serviceUrl);
            var changedCustomers = await newClient.DefaultBatchCustomer.ExecuteAsync();
            List<DefaultBatchProxy.DefaultBatchCustomer> changedCustomersList = changedCustomers.ToList();

            Assert.False(changedCustomersList.Any(x => x.Id == customerToDelete.Id));
            Assert.Equal(customerToUpdate.Name, changedCustomersList.Single(x => x.Id == customerToUpdate.Id).Name);
            Assert.Single(changedCustomersList, x => x.Id == 10);
        }

        [Fact]
        public async Task CanHandleAbsoluteAndRelativeUrls()
        {
            // Arrange
            var requestUri = string.Format("{0}/DefaultBatch/$batch", this.BaseAddress);
            Uri address = new Uri(this.BaseAddress, UriKind.Absolute);

            string host = address.Host;
            string relativeToServiceRootUri = "DefaultBatchCustomer";
            string relativeToHostUri = address.LocalPath.TrimEnd(new char[] { '/' }) + "/DefaultBatch/DefaultBatchCustomer";
            string absoluteUri = this.BaseAddress + "/DefaultBatch/DefaultBatchCustomer";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("multipart/mixed"));
            HttpContent content = new StringContent(@"
--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0
Content-Type: multipart/mixed; boundary=changeset_6c67825c-8938-4f11-af6b-a25861ee53cc

--changeset_6c67825c-8938-4f11-af6b-a25861ee53cc
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 2

POST " + relativeToServiceRootUri + @" HTTP/1.1
OData-Version: 4.0;NetFx
OData-MaxVersion: 4.0;NetFx
Content-Type: application/json;odata.metadata=minimal
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8

{'Id':1,'Name':'MyName1'}
--changeset_6c67825c-8938-4f11-af6b-a25861ee53cc
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 3

POST " + relativeToHostUri + @" HTTP/1.1
Host: " + host + @"
OData-Version: 4.0;NetFx
OData-MaxVersion: 4.0;NetFx
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8
Content-Type: application/json;odata.metadata=minimal

{'Id':2,'Name':'MyName2'}
--changeset_6c67825c-8938-4f11-af6b-a25861ee53cc
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 4

POST " + absoluteUri + @" HTTP/1.1
OData-Version: 4.0;NetFx
OData-MaxVersion: 4.0;NetFx
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8
Content-Type: application/json;odata.metadata=minimal

{'Id':3,'Name':'MyName3'}
--changeset_6c67825c-8938-4f11-af6b-a25861ee53cc--
--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0--
");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/mixed; boundary=batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0");
            request.Content = content;
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var stream = response.Content.ReadAsStreamAsync().Result;
            IODataResponseMessage odataResponseMessage = new ODataMessageWrapper(stream, response.Content.Headers);
            int subResponseCount = 0;
            using (var messageReader = new ODataMessageReader(odataResponseMessage, new ODataMessageReaderSettings(), GetEdmModel()))
            {
                var batchReader = messageReader.CreateODataBatchReader();
                while (batchReader.Read())
                {
                    switch (batchReader.State)
                    {
                        case ODataBatchReaderState.Operation:
                            var operationMessage = batchReader.CreateOperationResponseMessage();
                            subResponseCount++;
                            Assert.Equal(201, operationMessage.StatusCode);
                            break;
                    }
                }
            }
            Assert.Equal(3, subResponseCount);
        }
    }

    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class DefaultBatchHandlerQueryBatchTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            HttpServer server = configuration.Properties["Nuwa.HttpServerKey"] as HttpServer;

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.MapODataServiceRoute(
                "batch",
                "DefaultBatch",
                GetEdmModel(),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
                new DefaultODataBatchHandler(server));
        }

        protected static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<DefaultBatchCustomer>("DefaultBatchCustomer")
                   .EntityType
                   .Collection
                   .Action("OddCustomers")
                   .ReturnsCollectionFromEntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");

            builder.EntitySet<DefaultBatchOrder>("DefaultBatchOrder");
            builder.MaxDataServiceVersion = builder.DataServiceVersion;
            return builder.GetEdmModel();
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper webConfig)
        {
            webConfig.AddAppSection("aspnet:UseTaskFriendlySynchronizationContext", "true");
        }

        [Fact]
        public async Task CanBatchQueriesWithDataServicesClient()
        {
            Uri serviceUrl = new Uri(BaseAddress + "/DefaultBatch");
            DefaultBatchProxy.Container client = new DefaultBatchProxy.Container(serviceUrl);
            client.Format.UseJson();
            Uri customersRequestUri = new Uri(BaseAddress + "/DefaultBatch/DefaultBatchCustomer");
            DataServiceRequest<DefaultBatchProxy.DefaultBatchCustomer> customersRequest = new DataServiceRequest<DefaultBatchProxy.DefaultBatchCustomer>(customersRequestUri);
            Uri singleCustomerRequestUri = new Uri(BaseAddress + "/DefaultBatch/DefaultBatchCustomer(0)");
            DataServiceRequest<DefaultBatchProxy.DefaultBatchCustomer> singleCustomerRequest = new DataServiceRequest<DefaultBatchProxy.DefaultBatchCustomer>(singleCustomerRequestUri);

            DataServiceResponse batchResponse = await client.ExecuteBatchAsync(customersRequest, singleCustomerRequest);

            if (batchResponse.IsBatchResponse)
            {
                Assert.Equal(200, batchResponse.BatchStatusCode);
            }

            foreach (QueryOperationResponse response in batchResponse)
            {
                Assert.Equal(200, response.StatusCode);
                if (response.Query.RequestUri == customersRequestUri)
                {
                    Assert.Equal(10, response.Cast<DefaultBatchProxy.DefaultBatchCustomer>().Count());
                    continue;
                }
                if (response.Query.RequestUri == singleCustomerRequestUri)
                {
                    Assert.Equal(1, response.Cast<DefaultBatchProxy.DefaultBatchCustomer>().Count());
                    continue;
                }
            }
        }
    }

    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class DefaultBatchHandlerErrorsBatchTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            HttpServer server = configuration.Properties["Nuwa.HttpServerKey"] as HttpServer;

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.MapODataServiceRoute(
                "batch",
                "DefaultBatch",
                GetEdmModel(),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
                new DefaultODataBatchHandler(server));
        }

        protected static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<DefaultBatchCustomer>("DefaultBatchCustomer")
                   .EntityType
                   .Collection
                   .Action("OddCustomers")
                   .ReturnsCollectionFromEntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");

            builder.EntitySet<DefaultBatchOrder>("DefaultBatchOrder");

            builder.MaxDataServiceVersion = builder.DataServiceVersion;
            return builder.GetEdmModel();
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper webConfig)
        {
            webConfig.AddAppSection("aspnet:UseTaskFriendlySynchronizationContext", "true");
        }

        [Fact]
        public void SendsIndividualErrorWhenOneOfTheRequestsFails()
        {
            Uri serviceUrl = new Uri(BaseAddress + "/DefaultBatch");
            DefaultBatchProxy.Container client = new DefaultBatchProxy.Container(serviceUrl);
            client.Format.UseJson();

            DefaultBatchProxy.DefaultBatchCustomer validCustomer = new DefaultBatchProxy.DefaultBatchCustomer()
            {
                Id = 10,
                Name = "Customer 10"
            };

            DefaultBatchProxy.DefaultBatchCustomer invalidCustomer = new DefaultBatchProxy.DefaultBatchCustomer()
            {
                Id = -1,
                Name = "Customer -1"
            };

            client.AddToDefaultBatchCustomer(validCustomer);
            client.AddToDefaultBatchCustomer(invalidCustomer);
            var aggregate = Assert.Throws<AggregateException>(() =>
            {
                DataServiceResponse response = client.SaveChangesAsync(SaveChangesOptions.BatchWithSingleChangeset).Result;
            });

            var exception = aggregate.InnerExceptions.Single() as DataServiceRequestException;
            Assert.NotNull(exception);
            Assert.Equal(200, exception.Response.BatchStatusCode);
            Assert.Equal(1, exception.Response.Count());
        }
    }

    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class DefaultBatchHandlerLinksBatchTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            HttpServer server = configuration.Properties["Nuwa.HttpServerKey"] as HttpServer;

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.MapODataServiceRoute(
                "batch",
                "DefaultBatch",
                GetEdmModel(),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
                new DefaultODataBatchHandler(server));
        }

        protected static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<DefaultBatchCustomer>("DefaultBatchCustomer")
                   .EntityType
                   .Collection.Action("OddCustomers")
                   .ReturnsCollectionFromEntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");

            builder.EntitySet<DefaultBatchOrder>("DefaultBatchOrder");

            builder.MaxDataServiceVersion = builder.DataServiceVersion;

            return builder.GetEdmModel();
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper webConfig)
        {
            webConfig.AddAppSection("aspnet:UseTaskFriendlySynchronizationContext", "true");
        }

        [Fact]
        public async Task CanSetLinksInABatchWithDataServicesClient()
        {
            Uri serviceUrl = new Uri(BaseAddress + "/DefaultBatch");
            DefaultBatchProxy.Container client = new DefaultBatchProxy.Container(serviceUrl);
            client.Format.UseJson();

            DefaultBatchProxy.DefaultBatchCustomer customer = client.DefaultBatchCustomer.ExecuteAsync().Result.First();
            DefaultBatchProxy.DefaultBatchOrder order = new DefaultBatchProxy.DefaultBatchOrder() { Id = 0, PurchaseDate = DateTime.Now };

            client.AddToDefaultBatchOrder(order);

            client.AddLink(customer, "Orders", order);

            var response = await client.SaveChangesAsync(SaveChangesOptions.BatchWithSingleChangeset);
            Assert.Equal(200, response.BatchStatusCode);
        }
    }

    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class DefaultBatchHandlerContinueOnErrorBatchTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            HttpServer server = configuration.Properties["Nuwa.HttpServerKey"] as HttpServer;

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.MapODataServiceRoute(
                "batch",
                "DefaultBatch",
                GetEdmModel(),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
                new DefaultODataBatchHandler(server));
            configuration.EnableContinueOnErrorHeader();
        }

        protected static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<DefaultBatchCustomer> customers = builder.EntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");
            EntitySetConfiguration<DefaultBatchOrder> orders = builder.EntitySet<DefaultBatchOrder>("DefaultBatchOrder");
            customers.EntityType.Collection.Action("OddCustomers").ReturnsCollectionFromEntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");
            builder.MaxDataServiceVersion = builder.DataServiceVersion;
            builder.Namespace = typeof(DefaultBatchCustomer).Namespace;
            return builder.GetEdmModel();
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper webConfig)
        {
            webConfig.AddAppSection("aspnet:UseTaskFriendlySynchronizationContext", "true");
        }

        [Fact]
        public async Task CanNotContinueOnErrorWhenHeaderNotSet()
        {
            // Arrange
            var requestUri = string.Format("{0}/DefaultBatch/$batch", this.BaseAddress);
            string absoluteUri = this.BaseAddress + "/DefaultBatch/DefaultBatchCustomer";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("multipart/mixed"));
            HttpContent content = new StringContent(
@"--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0
Content-Type: application/http
Content-Transfer-Encoding: binary

GET " + absoluteUri + @"(0) HTTP/1.1

--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0
Content-Type: application/http
Content-Transfer-Encoding: binary

GET " + absoluteUri + @"(-1) HTTP/1.1

--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0
Content-Type: application/http
Content-Transfer-Encoding: binary

GET " + absoluteUri + @"(1) HTTP/1.1

--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0--
");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/mixed; boundary=batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0");
            request.Content = content;

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);
            var stream = response.Content.ReadAsStreamAsync().Result;
            IODataResponseMessage odataResponseMessage = new ODataMessageWrapper(stream, response.Content.Headers);
            int subResponseCount = 0;

            // Assert
            using (var messageReader = new ODataMessageReader(odataResponseMessage, new ODataMessageReaderSettings(), GetEdmModel()))
            {
                var batchReader = messageReader.CreateODataBatchReader();
                while (batchReader.Read())
                {
                    switch (batchReader.State)
                    {
                        case ODataBatchReaderState.Operation:
                            var operationMessage = batchReader.CreateOperationResponseMessage();
                            subResponseCount++;
                            if (subResponseCount == 2)
                            {
                                Assert.Equal(500, operationMessage.StatusCode);
                            }
                            else
                            {
                                Assert.Equal(200, operationMessage.StatusCode);
                            }
                            break;
                    }
                }
            }
            Assert.Equal(2, subResponseCount);
        }

        [Fact]
        public async Task CanContinueOnErrorWhenHeaderSet()
        {
            // Arrange
            var requestUri = string.Format("{0}/DefaultBatch/$batch", this.BaseAddress);
            string absoluteUri = this.BaseAddress + "/DefaultBatch/DefaultBatchCustomer";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("multipart/mixed"));
            request.Headers.Add("prefer", "odata.continue-on-error");
            HttpContent content = new StringContent(
@"--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0
Content-Type: application/http
Content-Transfer-Encoding: binary

GET " + absoluteUri + @"(0) HTTP/1.1

--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0
Content-Type: application/http
Content-Transfer-Encoding: binary

GET " + absoluteUri + @"(-1) HTTP/1.1

--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0
Content-Type: application/http
Content-Transfer-Encoding: binary

GET " + absoluteUri + @"(1) HTTP/1.1

--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0--
");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/mixed; boundary=batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0");
            request.Content = content;

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);
            var stream = response.Content.ReadAsStreamAsync().Result;
            IODataResponseMessage odataResponseMessage = new ODataMessageWrapper(stream, response.Content.Headers);
            int subResponseCount = 0;

            // Assert
            using (var messageReader = new ODataMessageReader(odataResponseMessage, new ODataMessageReaderSettings(), GetEdmModel()))
            {
                var batchReader = messageReader.CreateODataBatchReader();
                while (batchReader.Read())
                {
                    switch (batchReader.State)
                    {
                        case ODataBatchReaderState.Operation:
                            var operationMessage = batchReader.CreateOperationResponseMessage();
                            subResponseCount++;
                            if (subResponseCount == 2)
                            {
                                Assert.Equal(500, operationMessage.StatusCode);
                            }
                            else
                            {
                                Assert.Equal(200, operationMessage.StatusCode);
                            }
                            break;
                    }
                }
            }
            Assert.Equal(3, subResponseCount);
        }
    }
}
