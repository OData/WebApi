// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.ModelBuilder;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Batch.Tests.DataServicesClient
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

        protected override Task<ITestActionResult> CreateEntityAsync(DefaultBatchCustomer entity)
        {
            if (entity.Id < 0)
            {
                return Task.FromResult(BadRequest() as ITestActionResult);
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
            return Task.FromResult(StatusCode(HttpStatusCode.NoContent));
        }
    }

    public class DefaultBatchOrderController : InMemoryODataController<DefaultBatchOrder, int>
    {
        public DefaultBatchOrderController()
            : base("Id")
        {
        }
    }

    public class DefaultBatchHandlerCUDBatchTests : WebHostTestBase
    {
        public DefaultBatchHandlerCUDBatchTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            ODataModelBuilder builder = configuration.CreateConventionModelBuilder();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute(
                "batch",
                "DefaultBatch",
                GetEdmModel(builder),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
                configuration.CreateDefaultODataBatchHandler());
        }

        protected static IEdmModel GetEdmModel(ODataModelBuilder builder)
        {
            EntitySetConfiguration<DefaultBatchCustomer> customers = builder.EntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");
            EntitySetConfiguration<DefaultBatchOrder> orders = builder.EntitySet<DefaultBatchOrder>("DefaultBatchOrder");
            customers.EntityType.Collection.Action("OddCustomers").ReturnsCollectionFromEntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");
            builder.MaxDataServiceVersion = builder.DataServiceVersion;
            builder.Namespace = typeof(DefaultBatchCustomer).Namespace;
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task CanPerformCudOperationsOnABatch()
        {
            Uri serviceUrl = new Uri(BaseAddress + "/DefaultBatch");
            var client = new DefaultBatchProxy.Container(serviceUrl);
            client.Format.UseJson();

            var customers = await client.DefaultBatchCustomer.ExecuteAsync();
            List<DefaultBatchProxy.DefaultBatchCustomer> customerList = customers.ToList();

            DefaultBatchProxy.DefaultBatchCustomer customerToUpdate = customerList[1];
            DefaultBatchProxy.DefaultBatchCustomer customerToDelete = customerList[9];
            DefaultBatchProxy.DefaultBatchCustomer customerToAdd = new DefaultBatchProxy.DefaultBatchCustomer { Id = 10, Name = "Customer 10" };

            client.DeleteObject(customerToDelete);

            customerToUpdate.Name = "Updated customer name";
            client.UpdateObject(customerToUpdate);

            client.AddToDefaultBatchCustomer(customerToAdd);

            var response = await client.SaveChangesAsync(SaveChangesOptions.BatchWithSingleChangeset);

            var newClient = new DefaultBatchProxy.Container(serviceUrl);
            var changedCustomers = await newClient.DefaultBatchCustomer.ExecuteAsync();
            List<DefaultBatchProxy.DefaultBatchCustomer> changedCustomersList = changedCustomers.ToList();

            Assert.DoesNotContain(changedCustomersList, (x) => x.Id == customerToDelete.Id);
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

{'Id':11,'Name':'MyName11'}
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

{'Id':12,'Name':'MyName12'}
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

{'Id':13,'Name':'MyName13'}
--changeset_6c67825c-8938-4f11-af6b-a25861ee53cc--
--batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0--
");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/mixed; boundary=batch_abbe2e6f-e45b-4458-9555-5fc70e3aebe0");
            request.Content = content;
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var stream = await response.Content.ReadAsStreamAsync();
            Assert.Equal("multipart/mixed", response.Content.Headers.ContentType.MediaType.ToString());
            IODataResponseMessage odataResponseMessage = new ODataMessageWrapper(stream, response.Content.Headers);
            int subResponseCount = 0;
            using (var messageReader = new ODataMessageReader(odataResponseMessage, new ODataMessageReaderSettings(), GetEdmModel(new ODataConventionModelBuilder())))
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


        [Fact]
        public async Task CanHandleAbsoluteAndRelativeUrlsJSON()
        {
            // Arrange
            var requestUri = string.Format("{0}/DefaultBatch/$batch", this.BaseAddress);
            Uri address = new Uri(this.BaseAddress, UriKind.Absolute);

            string relativeToServiceRootUri = "DefaultBatchCustomer";
            string relativeToHostUri = address.LocalPath.TrimEnd(new char[] { '/' }) + "/DefaultBatch/DefaultBatchCustomer";
            string absoluteUri = this.BaseAddress + "/DefaultBatch/DefaultBatchCustomer";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            HttpContent content = new StringContent(@"
            {                                                                                                        
                ""requests"":[                                                                                             
                    {
                    ""id"": ""2"",                                                                                               
                    ""atomicityGroup"": ""transaction"",                                                                         
                    ""method"": ""post"",                                                                                        
                    ""url"": """ + relativeToServiceRootUri + @""",                                                                                   
                    ""headers"": { ""content-type"": ""application/json"", ""Accept"": ""application/json"", ""odata-version"": ""4.0"" }, 
                    ""body"": {'Id':11,'Name':'MyName11'}
                    },
                    {
                    ""id"": ""3"",                                                                                               
                    ""atomicityGroup"": ""transaction"",                                                                         
                    ""method"": ""post"",                                                                                        
                    ""url"": """ + relativeToHostUri + @""",                                                                                   
                    ""headers"": { ""content-type"": ""application/json"", ""Accept"": ""application/json"", ""odata-version"": ""4.0"" }, 
                    ""body"": {'Id':12,'Name':'MyName12'}
                    },
                    {
                    ""id"": ""4"",                                                                                               
                    ""atomicityGroup"": ""transaction"",                                                                         
                    ""method"": ""post"",                                                                                        
                    ""url"": """ + absoluteUri + @""",                                                                                   
                    ""headers"": { ""content-type"": ""application/json"", ""Accept"": ""application/json"", ""odata-version"": ""4.0"" }, 
                    ""body"": {'Id':13,'Name':'MyName13'}
                    }
                ]
            }");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Content = content;
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", content.Headers.ContentType.MediaType);

            var stream = await response.Content.ReadAsStreamAsync();
            IODataResponseMessage odataResponseMessage = new ODataMessageWrapper(stream, response.Content.Headers);
            int subResponseCount = 0;
            using (var messageReader = new ODataMessageReader(odataResponseMessage, new ODataMessageReaderSettings(), GetEdmModel(new ODataConventionModelBuilder())))
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
                            Assert.Contains("application/json", operationMessage.Headers.Single(h => String.Equals(h.Key, "Content-Type", StringComparison.OrdinalIgnoreCase)).Value);
                            break;
                    }
                }
            }
            Assert.Equal(3, subResponseCount);
        }

        [Fact]
        public async Task CanReadDataInBatch()
        {
            // Arrange
            var requestUri = string.Format("{0}/DefaultBatch/$batch", this.BaseAddress);
            Uri address = new Uri(this.BaseAddress, UriKind.Absolute);

            string relativeToServiceRootUri = "DefaultBatchCustomer";
            string relativeToHostUri = address.LocalPath.TrimEnd(new char[] { '/' }) + "/DefaultBatch/DefaultBatchCustomer";
            string absoluteUri = this.BaseAddress + "/DefaultBatch/DefaultBatchCustomer";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpContent content = new StringContent(@"
            {                                                                                                        
                ""requests"":[                                                                                             
                    {
                    ""id"": ""2"",                                                                                               
                    ""method"": ""get"",                                                                                        
                    ""url"": """ + relativeToServiceRootUri + @""",                                                                                   
                    ""headers"": { ""Accept"": ""application/json""} 
                    },
                    {
                    ""id"": ""3"",                                                                                               
                    ""method"": ""get"",                                                                                        
                    ""url"": """ + relativeToHostUri + @"""                                                                                   
                    },
                    {
                    ""id"": ""4"",                                                                                               
                    ""method"": ""get"",                                                                                        
                    ""url"": """ + absoluteUri + @""",                                                                                   
                    ""headers"": { ""Accept"": ""application/json""}
                    }
                ]
            }");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Content = content;
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);

            var stream = await response.Content.ReadAsStreamAsync();
            IODataResponseMessage odataResponseMessage = new ODataMessageWrapper(stream, response.Content.Headers);
            int subResponseCount = 0;
            var model = GetEdmModel(new ODataConventionModelBuilder());
            using (var messageReader = new ODataMessageReader(odataResponseMessage, new ODataMessageReaderSettings(), model))
            {
                var batchReader = messageReader.CreateODataBatchReader();
                while (batchReader.Read())
                {
                    switch (batchReader.State)
                    {
                        case ODataBatchReaderState.Operation:
                            var operationMessage = batchReader.CreateOperationResponseMessage();
                            subResponseCount++;
                            Assert.Equal(200, operationMessage.StatusCode);
                            Assert.Contains("application/json", operationMessage.Headers.Single(h => String.Equals(h.Key, "Content-Type", StringComparison.OrdinalIgnoreCase)).Value);
                            using (var innerMessageReader = new ODataMessageReader(operationMessage, new ODataMessageReaderSettings(), model))
                            {
                                var innerReader = innerMessageReader.CreateODataResourceSetReader();
                                while (innerReader.Read()) ;
                            }
                            break;
                    }
                }
            }
            Assert.Equal(3, subResponseCount);
        }

        [Fact]
        public async Task CanHandleAutomicityGroupRequestsAndUngroupedRequest_JsonBatch()
        {
            // Arrange
            var requestUri = string.Format("{0}/DefaultBatch/$batch", this.BaseAddress);
            string absoluteUri = this.BaseAddress + "/DefaultBatch/DefaultBatchCustomer";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpContent content = new StringContent(@"
{
    ""requests"": [{
            ""id"": ""0"",
            ""atomicityGroup"": ""f7de7314-2f3d-4422-b840-ada6d6de0f18"",
            ""method"": ""PATCH"",
            ""url"": """ + absoluteUri + "(6)" + @""",
            ""headers"": {
                ""OData-Version"": ""4.0"",
                ""Content-Type"": ""application/json;odata.metadata=minimal"",
                ""Accept"": ""application/json;odata.metadata=minimal""
            },
            ""body"": {
                ""Name"":""PatchedByJsonBatch_0""
            }
        }, {
            ""id"": ""1"",
            ""atomicityGroup"": ""f7de7314-2f3d-4422-b840-ada6d6de0f18"",
            ""method"": ""POST"",
            ""url"": """ + absoluteUri + @""",
            ""headers"": {
                ""OData-Version"": ""4.0"",
                ""Content-Type"": ""application/json;odata.metadata=minimal"",
                ""Accept"": ""application/json;odata.metadata=minimal""
            },
            ""body"": {
                ""Id"":11,
                ""Name"":""CreatedByJsonBatch_11""
            }
        }, {
            ""id"": ""2"",
            ""atomicityGroup"": ""f7de7314-2f3d-4422-b840-ada6d6de0f18"",
            ""method"": ""POST"",
            ""url"": """ + absoluteUri + @""",
            ""headers"": {
                ""OData-Version"": ""4.0"",
                ""Content-Type"": ""application/json;odata.metadata=minimal"",
                ""Accept"": ""application/json;odata.metadata=minimal""
            },
            ""body"": {
                ""Id"":12,
                ""Name"":""CreatedByJsonBatch_12""
            }
        }, {
            ""id"": ""3"",
            ""method"": ""POST"",
            ""url"": """ + absoluteUri + @""",
            ""headers"": {
                ""OData-Version"": ""4.0"",
                ""Content-Type"": ""application/json;odata.metadata=minimal"",
                ""Accept"": ""application/json;odata.metadata=minimal""
            },
            ""body"": {
                ""Id"":13,
                ""Name"":""CreatedByJsonBatch_3""
            }
        }
    ]
}");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Content = content;
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var stream = await response.Content.ReadAsStreamAsync();
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType.ToString());
            IODataResponseMessage odataResponseMessage = new ODataMessageWrapper(stream, response.Content.Headers);
            int subResponseCount = 0;
            using (var messageReader = new ODataMessageReader(odataResponseMessage, new ODataMessageReaderSettings(), GetEdmModel(new ODataConventionModelBuilder())))
            {
                var batchReader = messageReader.CreateODataBatchReader();

                while (batchReader.Read())
                {
                    switch (batchReader.State)
                    {
                        case ODataBatchReaderState.Operation:
                            var operationMessage = batchReader.CreateOperationResponseMessage();
                            subResponseCount++;

                            if (operationMessage.ContentId.Equals("0"))
                            {
                                // No-Content response for PATCH
                                Assert.Equal(204, operationMessage.StatusCode);
                            }
                            else
                            {
                                Assert.Equal(201, operationMessage.StatusCode);
                            }

                            break;
                    }
                }
            }
            Assert.Equal(4, subResponseCount);
        }

        [Fact]
        public async Task CanHandleSingleDeleteInBatch()
        {
            // Arrange
            var requestUri = string.Format("{0}/DefaultBatch/$batch", this.BaseAddress);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpContent content = new StringContent(@"
            {
                ""requests"": [
                    {
                        ""Id"": ""1"",
                        ""method"": ""DELETE"",
                        ""headers"": {
                            ""odata-version"": ""4.01"",
                            ""content-type"": ""application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false""
                        },
                        ""url"": ""DefaultBatchCustomer(5)"",
                        ""body"": """"
                    }
                ]
            }");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Content = content;
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var stream = await response.Content.ReadAsStreamAsync();
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType.ToString());
            IODataResponseMessage odataResponseMessage = new ODataMessageWrapper(stream, response.Content.Headers);
            int subResponseCount = 0;
            using (var messageReader = new ODataMessageReader(odataResponseMessage, new ODataMessageReaderSettings(), GetEdmModel(new ODataConventionModelBuilder())))
            {
                var batchReader = messageReader.CreateODataBatchReader();

                while (batchReader.Read())
                {
                    switch (batchReader.State)
                    {
                        case ODataBatchReaderState.Operation:
                            var operationMessage = batchReader.CreateOperationResponseMessage();
                            subResponseCount++;
                            break;
                    }
                }
            }
            Assert.Equal(1, subResponseCount);
        }
    }

    public class DefaultBatchHandlerQueryBatchTests : WebHostTestBase
    {
        public DefaultBatchHandlerQueryBatchTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.MapODataServiceRoute(
                "batch",
                "DefaultBatch",
                GetEdmModel(configuration),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
                configuration.CreateDefaultODataBatchHandler());
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<DefaultBatchCustomer>("DefaultBatchCustomer")
                   .EntityType
                   .Collection
                   .Action("OddCustomers")
                   .ReturnsCollectionFromEntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");

            builder.EntitySet<DefaultBatchOrder>("DefaultBatchOrder");
            builder.MaxDataServiceVersion = builder.DataServiceVersion;
            return builder.GetEdmModel();
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
                    // Previous test could modify the total count to be anywhere from, 10 to 14.
                    Assert.InRange(response.Cast<DefaultBatchProxy.DefaultBatchCustomer>().Count(), 10, 14);
                    continue;
                }
                if (response.Query.RequestUri == singleCustomerRequestUri)
                {
                    Assert.Single(response.Cast<DefaultBatchProxy.DefaultBatchCustomer>());
                    continue;
                }
            }
        }
    }


    public class DefaultBatchHandlerErrorsBatchTests : WebHostTestBase
    {
        public DefaultBatchHandlerErrorsBatchTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.MapODataServiceRoute(
                "batch",
                "DefaultBatch",
                GetEdmModel(configuration),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
                configuration.CreateDefaultODataBatchHandler());
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();

            builder.EntitySet<DefaultBatchCustomer>("DefaultBatchCustomer")
                   .EntityType
                   .Collection
                   .Action("OddCustomers")
                   .ReturnsCollectionFromEntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");

            builder.EntitySet<DefaultBatchOrder>("DefaultBatchOrder");

            builder.MaxDataServiceVersion = builder.DataServiceVersion;
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task SendsIndividualErrorWhenOneOfTheRequestsFails()
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
            var exception = await Assert.ThrowsAsync<DataServiceRequestException>(async () =>
            {
                DataServiceResponse response = await client.SaveChangesAsync(SaveChangesOptions.BatchWithSingleChangeset);
            });

            Assert.NotNull(exception);
            Assert.Equal(200, exception.Response.BatchStatusCode);
            Assert.Single(exception.Response);
        }
    }


    public class DefaultBatchHandlerLinksBatchTests : WebHostTestBase
    {
        public DefaultBatchHandlerLinksBatchTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.MapODataServiceRoute(
                "batch",
                "DefaultBatch",
                GetEdmModel(configuration),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
                configuration.CreateDefaultODataBatchHandler());
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();

            builder.EntitySet<DefaultBatchCustomer>("DefaultBatchCustomer")
                   .EntityType
                   .Collection.Action("OddCustomers")
                   .ReturnsCollectionFromEntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");

            builder.EntitySet<DefaultBatchOrder>("DefaultBatchOrder");

            builder.MaxDataServiceVersion = builder.DataServiceVersion;

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task CanSetLinksInABatchWithDataServicesClient()
        {
            Uri serviceUrl = new Uri(BaseAddress + "/DefaultBatch");
            DefaultBatchProxy.Container client = new DefaultBatchProxy.Container(serviceUrl);
            client.Format.UseJson();

            DefaultBatchProxy.DefaultBatchCustomer customer = (await client.DefaultBatchCustomer.ExecuteAsync()).First();
            DefaultBatchProxy.DefaultBatchOrder order = new DefaultBatchProxy.DefaultBatchOrder() { Id = 0, PurchaseDate = DateTime.Now };

            client.AddToDefaultBatchOrder(order);

            client.AddLink(customer, "Orders", order);

            var response = await client.SaveChangesAsync(SaveChangesOptions.BatchWithSingleChangeset);
            Assert.Equal(200, response.BatchStatusCode);
        }
    }

    public class DefaultBatchHandlerContinueOnErrorBatchTests : WebHostTestBase
    {
        public DefaultBatchHandlerContinueOnErrorBatchTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            ODataModelBuilder builder = configuration.CreateConventionModelBuilder();
            configuration.MapODataServiceRoute(
                "batch",
                "DefaultBatch",
                GetEdmModel(builder),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
                configuration.CreateDefaultODataBatchHandler());

            configuration.EnableContinueOnErrorHeader();
        }

        protected static IEdmModel GetEdmModel(ODataModelBuilder builder)
        {
            EntitySetConfiguration<DefaultBatchCustomer> customers = builder.EntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");
            EntitySetConfiguration<DefaultBatchOrder> orders = builder.EntitySet<DefaultBatchOrder>("DefaultBatchOrder");
            customers.EntityType.Collection.Action("OddCustomers").ReturnsCollectionFromEntitySet<DefaultBatchCustomer>("DefaultBatchCustomer");
            builder.MaxDataServiceVersion = builder.DataServiceVersion;
            builder.Namespace = typeof(DefaultBatchCustomer).Namespace;
            return builder.GetEdmModel();
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
            var stream = await response.Content.ReadAsStreamAsync();
            IODataResponseMessage odataResponseMessage = new ODataMessageWrapper(stream, response.Content.Headers);
            int subResponseCount = 0;

            // Assert
            using (var messageReader = new ODataMessageReader(odataResponseMessage, new ODataMessageReaderSettings(), GetEdmModel(new ODataConventionModelBuilder())))
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
            var stream = await response.Content.ReadAsStreamAsync();
            IODataResponseMessage odataResponseMessage = new ODataMessageWrapper(stream, response.Content.Headers);
            int subResponseCount = 0;

            // Assert
            using (var messageReader = new ODataMessageReader(odataResponseMessage, new ODataMessageReaderSettings(), GetEdmModel(new ODataConventionModelBuilder())))
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
