// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
using Microsoft.OData;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using WebStack.QA.Test.OData.ModelBuilder;
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
        public DateTimeOffset PurchaseDate { get; set; }
    }

    public class UnbufferedBatchCustomerController : InMemoryODataController<UnbufferedBatchCustomer, int>
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

        public Task CreateRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            return Task.FromResult(Request.CreateResponse(HttpStatusCode.NoContent));
        }
    }

    public class UnbufferedBatchOrderController : InMemoryODataController<UnbufferedBatchOrder, int>
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
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute(
                "batch",
                "UnbufferedBatch",
                GetEdmModel(),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
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
        public async Task CanPerformCudOperationsOnABatch()
        {
            Uri serviceUrl = new Uri(BaseAddress + "/UnbufferedBatch");
            var client = new UnbufferedBatchProxy.Container(serviceUrl);
            client.Format.UseJson();

            IEnumerable<UnbufferedBatchProxy.UnbufferedBatchCustomer> customers =
                await client.UnbufferedBatchCustomer.ExecuteAsync();

            var customersList = customers.ToList();
            UnbufferedBatchProxy.UnbufferedBatchCustomer customerToDelete = customersList[0];
            client.DeleteObject(customerToDelete);

            UnbufferedBatchProxy.UnbufferedBatchCustomer customerToUpdate = customersList[1];
            customerToUpdate.Name = "Updated customer name";
            client.UpdateObject(customerToUpdate);

            UnbufferedBatchProxy.UnbufferedBatchCustomer customerToAdd =
                new UnbufferedBatchProxy.UnbufferedBatchCustomer { Id = 10, Name = "Customer 10" };
            client.AddToUnbufferedBatchCustomer(customerToAdd);

            var response = await client.SaveChangesAsync(SaveChangesOptions.BatchWithSingleChangeset);

            var newClient = new UnbufferedBatchProxy.Container(serviceUrl);

            IEnumerable<UnbufferedBatchProxy.UnbufferedBatchCustomer> changedCustomers =
                await newClient.UnbufferedBatchCustomer.ExecuteAsync();

            var changedCustomerList = changedCustomers.ToList();
            Assert.False(changedCustomerList.Any(x => x.Id == customerToDelete.Id));
            Assert.Equal(customerToUpdate.Name, changedCustomerList.Single(x => x.Id == customerToUpdate.Id).Name);
            Assert.Single(changedCustomerList, x => x.Id == 10);
        }

        [Fact]
        public async Task CanHandleAbsoluteAndRelativeUrls()
        {
            // Arrange
            var requestUri = string.Format("{0}/UnbufferedBatch/$batch", this.BaseAddress);
            Uri address = new Uri(this.BaseAddress, UriKind.Absolute);

            string host = address.Host;
            string relativeToServiceRootUri = "UnbufferedBatchCustomer";
            string relativeToHostUri = address.LocalPath.TrimEnd(new char[]{'/'}) + "/UnbufferedBatch/UnbufferedBatchCustomer";
            string absoluteUri = this.BaseAddress + "/UnbufferedBatch/UnbufferedBatchCustomer";

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
            configuration.MapODataServiceRoute(
                "batch",
                "UnbufferedBatch",
                GetEdmModel(),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
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
            UnbufferedBatchProxy.Container client = new UnbufferedBatchProxy.Container(serviceUrl);
            client.Format.UseJson();
            Uri customersRequestUri = new Uri(BaseAddress + "/UnbufferedBatch/UnbufferedBatchCustomer");
            DataServiceRequest<UnbufferedBatchProxy.UnbufferedBatchCustomer> customersRequest = new DataServiceRequest<UnbufferedBatchProxy.UnbufferedBatchCustomer>(customersRequestUri);
            Uri singleCustomerRequestUri = new Uri(BaseAddress + "/UnbufferedBatch/UnbufferedBatchCustomer(0)");
            DataServiceRequest<UnbufferedBatchProxy.UnbufferedBatchCustomer> singleCustomerRequest = new DataServiceRequest<UnbufferedBatchProxy.UnbufferedBatchCustomer>(singleCustomerRequestUri);

            DataServiceResponse batchResponse = client.ExecuteBatchAsync(customersRequest, singleCustomerRequest).Result;

            if (batchResponse.IsBatchResponse)
            {
                Assert.Equal(200, batchResponse.BatchStatusCode);
            }

            foreach (QueryOperationResponse response in batchResponse)
            {
                Assert.Equal(200, response.StatusCode);
                if (response.Query.RequestUri == customersRequestUri)
                {
                    Assert.Equal(10, response.Cast<UnbufferedBatchProxy.UnbufferedBatchCustomer>().Count());
                    continue;
                }
                if (response.Query.RequestUri == singleCustomerRequestUri)
                {
                    Assert.Equal(1, response.Cast<UnbufferedBatchProxy.UnbufferedBatchCustomer>().Count());
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
            configuration.MapODataServiceRoute(
                "batch",
                "UnbufferedBatch",
                GetEdmModel(),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
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
        public void SendsIndividualErrorWhenOneOfTheRequestsFails()
        {
            Uri serviceUrl = new Uri(BaseAddress + "/UnbufferedBatch");
            UnbufferedBatchProxy.Container client = new UnbufferedBatchProxy.Container(serviceUrl);
            client.Format.UseJson();

            UnbufferedBatchProxy.UnbufferedBatchCustomer validCustomer = new UnbufferedBatchProxy.UnbufferedBatchCustomer()
            {
                Id = 10,
                Name = "Customer 10"
            };

            UnbufferedBatchProxy.UnbufferedBatchCustomer invalidCustomer = new UnbufferedBatchProxy.UnbufferedBatchCustomer()
            {
                Id = -1,
                Name = "Customer -1"
            };

            client.AddToUnbufferedBatchCustomer(validCustomer);
            client.AddToUnbufferedBatchCustomer(invalidCustomer);
            var aggregateException = Assert.Throws<AggregateException>(() =>
            {
                DataServiceResponse response = client.SaveChangesAsync(SaveChangesOptions.BatchWithSingleChangeset).Result;
            });

            var exception = aggregateException.InnerExceptions.SingleOrDefault() as DataServiceRequestException;
            Assert.NotNull(exception);
            Assert.Equal(200, exception.Response.BatchStatusCode);
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
            configuration.MapODataServiceRoute(
                "batch",
                "UnbufferedBatch",
                GetEdmModel(),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
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
            UnbufferedBatchProxy.Container client = new UnbufferedBatchProxy.Container(serviceUrl);
            client.Format.UseJson();

            UnbufferedBatchProxy.UnbufferedBatchCustomer customer = client.UnbufferedBatchCustomer.ExecuteAsync().Result.First();
            UnbufferedBatchProxy.UnbufferedBatchOrder order = new UnbufferedBatchProxy.UnbufferedBatchOrder() { Id = 0, PurchaseDate = DateTime.Now };

            client.AddToUnbufferedBatchOrder(order);

            client.AddLink(customer, "Orders", order);

            client.SaveChangesAsync(SaveChangesOptions.BatchWithSingleChangeset).Wait();
        }
    }

    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class ContinueOnErrorBatchTests : IODataTestBase
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
            configuration.MapODataServiceRoute(
                "batch",
                "UnbufferedBatch",
                GetEdmModel(),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
                new UnbufferedODataBatchHandler(server));
            configuration.EnableContinueOnErrorHeader();
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
        public async Task CanNotContinueOnErrorWhenHeaderNotSet()
        {
            // Arrange
            var requestUri = string.Format("{0}/UnbufferedBatch/$batch", this.BaseAddress);
            string absoluteUri = this.BaseAddress + "/UnbufferedBatch/UnbufferedBatchCustomer";
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
            var requestUri = string.Format("{0}/UnbufferedBatch/$batch", this.BaseAddress);
            string absoluteUri = this.BaseAddress + "/UnbufferedBatch/UnbufferedBatchCustomer";
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
