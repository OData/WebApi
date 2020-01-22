// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Xunit;
using Newtonsoft.Json;
#if !NETCORE
using System.Web.Http;
using System.Web.Http.Routing;
#else
using Microsoft.AspNetCore.Mvc;
#endif

namespace Microsoft.AspNet.OData.Test.Batch
{
    public class DefaultODataBatchHandlerTest
    {
#if !NETCORE // TODO #939: Enable these test on AspNetCore.
        [Fact]
        public void Parameter_Constructor()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());

            Assert.NotNull(batchHandler.Invoker);
            Assert.NotNull(batchHandler.MessageQuotas);
            Assert.Null(batchHandler.ODataRouteName);
        }

        [Fact]
        public void Constructor_Throws_IfHttpServerIsNull()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new DefaultODataBatchHandler(null),
                "httpServer");
        }

        [Fact]
        public async Task CreateResponseMessageAsync_Throws_IfResponsesAreNull()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            HttpRequestMessage request = new HttpRequestMessage();
            request.EnableHttpDependencyInjectionSupport();
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => batchHandler.CreateResponseMessageAsync(null, request, CancellationToken.None),
                "responses");
        }

        [Fact]
        public async Task CreateResponseMessageAsync_Throws_IfRequestIsNull()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => batchHandler.CreateResponseMessageAsync(new ODataBatchResponseItem[0], null, CancellationToken.None),
                "request");
        }

        [Fact]
        public async Task CreateResponseMessageAsync_ReturnsODataBatchContent()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            ODataBatchResponseItem[] responses = new ODataBatchResponseItem[]
            {
                new OperationResponseItem(new HttpResponseMessage(HttpStatusCode.OK))
            };
            HttpRequestMessage request = new HttpRequestMessage();
            request.EnableHttpDependencyInjectionSupport();

            HttpResponseMessage response = await batchHandler.CreateResponseMessageAsync(responses, request, CancellationToken.None);

            var batchContent = Assert.IsType<ODataBatchContent>(response.Content);
            Assert.Single(batchContent.Responses);
        }

        [Fact]
        public async Task ProcessBatchAsync_Throws_IfRequestIsNull()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => batchHandler.ProcessBatchAsync(null, CancellationToken.None),
                "request");
        }

        [Fact]
        public async Task ProcessBatchAsync_CallsRegisterForDispose()
        {
            // Arrange
            List<IDisposable> expectedResourcesForDisposal = new List<IDisposable>();
            MockHttpServer server = new MockHttpServer(request =>
            {
                var tmpContent = new StringContent(String.Empty);
                request.RegisterForDispose(tmpContent);
                expectedResourcesForDisposal.Add(tmpContent);
                return new HttpResponseMessage { Content = new StringContent(request.RequestUri.AbsoluteUri) };
            });
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(server);
            HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://example.com/$batch")
            {
                Content = new MultipartContent("mixed")
                {
                    ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Get, "http://example.com/"))
                }
            };
            batchRequest.EnableHttpDependencyInjectionSupport();

            // Act
            var response = await batchHandler.ProcessBatchAsync(batchRequest, CancellationToken.None);
            var resourcesForDisposal = batchRequest.GetResourcesForDisposal();

            // Assert
            foreach (var expectedResource in expectedResourcesForDisposal)
            {
                Assert.Contains(expectedResource, resourcesForDisposal);
            }
        }

        [Theory]
        [InlineData(true, "", 2)]
        [InlineData(true, "odata.continue-on-error", 3)]
        [InlineData(true, "odata.continue-on-error=true", 3)]
        [InlineData(true, "odata.continue-on-error=false", 2)]
        [InlineData(true, "continue-on-error", 3)]
        [InlineData(true, "continue-on-error=true", 3)]
        [InlineData(true, "continue-on-error=false", 2)]
        [InlineData(false, "", 3)]
        [InlineData(false, "odata.continue-on-error", 3)]
        [InlineData(false, "odata.continue-on-error=true", 3)]
        [InlineData(false, "odata.continue-on-error=false", 3)]
        [InlineData(false, "continue-on-error", 3)]
        [InlineData(false, "continue-on-error=true", 3)]
        [InlineData(false, "continue-on-error=false", 3)]
        public async Task ProcessBatchAsync_ContinueOnError(bool enableContinueOnError, string preferenceHeader, int expectedResponses)
        {
            // Arrange
            MockHttpServer server = new MockHttpServer(async request =>
            {
                string responseContent = request.RequestUri.AbsoluteUri;
                string content = "";
                if (request.Content != null)
                {
                    content = await request.Content.ReadAsStringAsync();
                    if (!String.IsNullOrEmpty(content))
                    {
                        responseContent += "," + content;
                    }
                }
                HttpResponseMessage responseMessage = new HttpResponseMessage { Content = new StringContent(responseContent) };
                if (content.Equals("foo"))
                {
                    responseMessage.StatusCode = HttpStatusCode.BadRequest;
                }
                return responseMessage;
            });
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(server);
            HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://example.com/$batch")
            {
                Content = new MultipartContent("mixed")
                {
                    ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Get, "http://example.com/")),
                    new MultipartContent("mixed") // ChangeSet
                    {
                        ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Post, "http://example.com/values")
                        {
                            Content = new StringContent("foo")
                        })
                    },
                    ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Post, "http://example.com/values")
                    {
                        Content = new StringContent("bar")
                    }),
                }
            };
            var enableContinueOnErrorconfig = new HttpConfiguration();
            enableContinueOnErrorconfig.EnableODataDependencyInjectionSupport();
            enableContinueOnErrorconfig.EnableContinueOnErrorHeader();
            if(enableContinueOnError)
                batchRequest.SetConfiguration(enableContinueOnErrorconfig);
            if (!string.IsNullOrEmpty(preferenceHeader))
                batchRequest.Headers.Add("prefer", preferenceHeader);
            batchRequest.EnableHttpDependencyInjectionSupport();

            // Act
            var response = await batchHandler.ProcessBatchAsync(batchRequest, CancellationToken.None);
            var batchContent = Assert.IsType<ODataBatchContent>(response.Content);
            var batchResponses = batchContent.Responses.ToArray();

            // Assert
            Assert.Equal(expectedResponses, batchResponses.Length);
        }

        [Fact]
        public async Task ExecuteRequestMessagesAsync_CallsInvokerForEachRequest()
        {
            MockHttpServer server = new MockHttpServer(async request =>
            {
                string responseContent = request.RequestUri.AbsoluteUri;
                if (request.Content != null)
                {
                    string content = await request.Content.ReadAsStringAsync();
                    if (!String.IsNullOrEmpty(content))
                    {
                        responseContent += "," + content;
                    }
                }
                return new HttpResponseMessage { Content = new StringContent(responseContent) };
            });
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(server);
            ODataBatchRequestItem[] requests = new ODataBatchRequestItem[]
            {
                new OperationRequestItem(new HttpRequestMessage(HttpMethod.Get, "http://example.com/")),
                new ChangeSetRequestItem(new HttpRequestMessage[]
                {
                    new HttpRequestMessage(HttpMethod.Post, "http://example.com/values")
                    {
                        Content = new StringContent("foo")
                    }
                })
            };

            var responses = await batchHandler.ExecuteRequestMessagesAsync(requests, CancellationToken.None);
            var response0 = (OperationResponseItem)responses[0];
            var response1 = (ChangeSetResponseItem)responses[1];

            Assert.Equal(2, responses.Count);
            Assert.Equal("http://example.com/", await response0.Response.Content.ReadAsStringAsync());
            Assert.Equal("http://example.com/values,foo", await response1.Responses.First().Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task ExecuteRequestMessagesAsync_DisposesResponseInCaseOfException()
        {
            List<MockHttpResponseMessage> responses = new List<MockHttpResponseMessage>();
            MockHttpServer server = new MockHttpServer(request =>
            {
                if (request.Method == HttpMethod.Put)
                {
                    throw new InvalidOperationException();
                }
                var response = new MockHttpResponseMessage();
                responses.Add(response);
                return response;
            });
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(server);
            ODataBatchRequestItem[] requests = new ODataBatchRequestItem[]
            {
                new OperationRequestItem(new HttpRequestMessage(HttpMethod.Get, "http://example.com/")),
                new OperationRequestItem(new HttpRequestMessage(HttpMethod.Post, "http://example.com/")),
                new OperationRequestItem(new HttpRequestMessage(HttpMethod.Put, "http://example.com/")),
            };

            await ExceptionAssert.ThrowsAsync<InvalidOperationException>(
                () => batchHandler.ExecuteRequestMessagesAsync(requests, CancellationToken.None));

            Assert.Equal(2, responses.Count);
            foreach (var response in responses)
            {
                Assert.True(response.IsDisposed);
            }
        }

        [Fact]
        public async Task ExecuteRequestMessagesAsync_Throws_IfRequestsIsNull()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => batchHandler.ExecuteRequestMessagesAsync(null, CancellationToken.None),
                "requests");
        }

        [Fact]
        public async Task ParseBatchRequestsAsync_Throws_IfRequestIsNull()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => batchHandler.ParseBatchRequestsAsync(null, CancellationToken.None),
                "request");
        }

        [Fact]
        public async Task ParseBatchRequestsAsync_Returns_RequestsFromMultipartContent()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://example.com/$batch")
            {
                Content = new MultipartContent("mixed")
                {
                    ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Get, "http://example.com/")),
                    new MultipartContent("mixed") // ChangeSet
                    {
                        ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Post, "http://example.com/values"))
                    }
                }
            };
            batchRequest.EnableHttpDependencyInjectionSupport();

            IList<ODataBatchRequestItem> requests = await batchHandler.ParseBatchRequestsAsync(batchRequest, CancellationToken.None);

            Assert.Equal(2, requests.Count);

            var operationRequest = ((OperationRequestItem)requests[0]).Request;
            Assert.Equal(HttpMethod.Get, operationRequest.Method);
            Assert.Equal("http://example.com/", operationRequest.RequestUri.AbsoluteUri);

            var changeSetRequest = ((ChangeSetRequestItem)requests[1]).Requests.First();
            Assert.Equal(HttpMethod.Post, changeSetRequest.Method);
            Assert.Equal("http://example.com/values", changeSetRequest.RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task ParseBatchRequestsAsync_CopiesPropertiesFromRequest_WithoutExcludedProperties()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://example.com/$batch")
            {
                Content = new MultipartContent("mixed")
                {
                    ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Get, "http://example.com/")),
                    new MultipartContent("mixed") // ChangeSet
                    {
                        ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Post, "http://example.com/values"))
                    }
                }
            };
            batchRequest.Properties.Add("foo", "bar");
            batchRequest.SetRouteData(new HttpRouteData(new HttpRoute()));
            batchRequest.RegisterForDispose(new StringContent(String.Empty));
            batchRequest.EnableHttpDependencyInjectionSupport();

            IList<ODataBatchRequestItem> requests = await batchHandler.ParseBatchRequestsAsync(batchRequest, CancellationToken.None);

            Assert.Equal(2, requests.Count);

            var operationRequest = ((OperationRequestItem)requests[0]).Request;
            Assert.Equal(HttpMethod.Get, operationRequest.Method);
            Assert.Equal("http://example.com/", operationRequest.RequestUri.AbsoluteUri);
            Assert.Equal("bar", operationRequest.Properties["foo"]);
            Assert.Null(operationRequest.GetRouteData());
            Assert.Same(operationRequest, operationRequest.GetUrlHelper().Request);
            Assert.Empty(operationRequest.GetResourcesForDisposal());

            var changeSetRequest = ((ChangeSetRequestItem)requests[1]).Requests.First();
            Assert.Equal(HttpMethod.Post, changeSetRequest.Method);
            Assert.Equal("http://example.com/values", changeSetRequest.RequestUri.AbsoluteUri);
            Assert.Equal("bar", changeSetRequest.Properties["foo"]);
            Assert.Null(changeSetRequest.GetRouteData());
            Assert.Same(operationRequest, operationRequest.GetUrlHelper().Request);
            Assert.Empty(changeSetRequest.GetResourcesForDisposal());
        }

        [Fact]
        public void ValidateRequest_Throws_IfResponsesIsNull()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            ExceptionAssert.ThrowsArgumentNull(
                () => batchHandler.ValidateRequest(null),
                "request");
        }

        [Fact]
        public async Task ValidateRequest_Throws_IfRequestContentIsNull()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            HttpRequestMessage request = new HttpRequestMessage();

            HttpResponseException errorResponse = ExceptionAssert.Throws<HttpResponseException>(
                () => batchHandler.ValidateRequest(request));
            Assert.Equal(HttpStatusCode.BadRequest, errorResponse.Response.StatusCode);
            Assert.Equal("The 'Content' property on the batch request cannot be null.",
                (await errorResponse.Response.Content.ReadAsAsync<HttpError>()).Message);
        }

        [Fact]
        public async Task ValidateRequest_Throws_IfRequestContentTypeIsNull()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent(String.Empty);
            request.Content.Headers.ContentType = null;

            HttpResponseException errorResponse = ExceptionAssert.Throws<HttpResponseException>(
                () => batchHandler.ValidateRequest(request));
            Assert.Equal(HttpStatusCode.BadRequest, errorResponse.Response.StatusCode);
            Assert.Equal("The batch request must have a \"Content-Type\" header.",
                (await errorResponse.Response.Content.ReadAsAsync<HttpError>()).Message);
        }

        [Fact]
        public async Task ValidateRequest_Throws_IfRequestMediaTypeIsWrong()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent(String.Empty);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/json");

            HttpResponseException errorResponse = ExceptionAssert.Throws<HttpResponseException>(
                () => batchHandler.ValidateRequest(request));
            Assert.Equal(HttpStatusCode.BadRequest, errorResponse.Response.StatusCode);
            Assert.Equal("The batch request must have 'multipart/mixed' or 'application/json' as the media type.",
                (await errorResponse.Response.Content.ReadAsAsync<HttpError>()).Message);
        }

        [Fact]
        public async Task ValidateRequest_Throws_IfRequestContentTypeDoesNotHaveBoundary()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent(String.Empty);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("multipart/mixed");

            HttpResponseException errorResponse = ExceptionAssert.Throws<HttpResponseException>(
                () => batchHandler.ValidateRequest(request));
            Assert.Equal(HttpStatusCode.BadRequest, errorResponse.Response.StatusCode);
            Assert.Equal("The batch request must have a boundary specification in the \"Content-Type\" header.",
                (await errorResponse.Response.Content.ReadAsAsync<HttpError>()).Message);
        }
#else
 
        [Fact]
        public async Task SendAsync_Works_ForBatchRequestWithInsertedEntityReferencedInAnotherRequest()
        {
            const string acceptJsonFullMetadata = "application/json;odata.metadata=minimal";
            const string acceptJson = "application/json";

            Type[] controllers = new[] { typeof(BatchTestCustomersController), typeof(BatchTestOrdersController), };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                var builder = ODataConventionModelBuilderFactory.Create(config);
                builder.EntitySet<BatchTestCustomer>("BatchTestCustomers");
                builder.EntitySet<BatchTestOrder>("BatchTestOrders");

                config.MapODataServiceRoute("odata", null, builder.GetEdmModel(), new DefaultODataBatchHandler());
                config.Expand();
                config.EnableDependencyInjection();
            });

            var client = TestServerFactory.CreateClient(server);

            var endpoint = "http://localhost";

            var batchRef = $"batch_{Guid.NewGuid()}";
            var changesetRef = $"changeset_{Guid.NewGuid()}";

            var orderId = 2;
            var createOrderPayload = $@"{{""@odata.type"":""Microsoft.AspNet.OData.Test.Batch.BatchTestOrder"",""Id"":{orderId},""Amount"":50}}";
            var createRefPayload = @"{""@odata.id"":""$3""}";
            
            var batchRequest = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/$batch");
            batchRequest.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("multipart/mixed"));
            HttpContent httpContent = new StringContent($@"
--{batchRef}
Content-Type: multipart/mixed; boundary={changesetRef}

--{changesetRef}
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 3

POST {endpoint}/BatchTestOrders HTTP/1.1
OData-Version: 4.0;NetFx
OData-MaxVersion: 4.0;NetFx
Content-Type: {acceptJsonFullMetadata}
Accept: {acceptJsonFullMetadata}
Accept-Charset: UTF-8

{createOrderPayload}
--{changesetRef}
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 4

POST {endpoint}/BatchTestCustomers(2)/Orders/$ref HTTP/1.1
OData-Version: 4.0;NetFx
OData-MaxVersion: 4.0;NetFx
Content-Type: {acceptJsonFullMetadata}
Accept: {acceptJsonFullMetadata}
Accept-Charset: UTF-8

{createRefPayload}
--{changesetRef}--
--{batchRef}--
");

            httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/mixed; boundary={batchRef}");
            batchRequest.Content = httpContent;
            
            var response = await client.SendAsync(batchRequest);

            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());

            HttpRequestMessage customerRequest = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}/BatchTestCustomers(2)?$expand=Orders");
            customerRequest.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptJson));

            var customerResponse = client.SendAsync(customerRequest).Result;
            var objAsJsonString = await customerResponse.Content.ReadAsStringAsync();
            var customer = JsonConvert.DeserializeObject<BatchTestCustomer>(objAsJsonString);

            Assert.NotNull(customer.Orders?.SingleOrDefault(d => d.Id.Equals(orderId)));
        }
#endif
    }

    public class BatchTestCustomer
    {
        private static Lazy<IList<BatchTestCustomer>> _customers =
            new Lazy<IList<BatchTestCustomer>>(() => {
                BatchTestCustomer customer01 = new BatchTestCustomer { Id = 1, Name = "Customer 01" };
                customer01.Orders = new List<BatchTestOrder> { BatchTestOrder.Orders.SingleOrDefault(d => d.Id.Equals(1)) };

                BatchTestCustomer customer02 = new BatchTestCustomer { Id = 2, Name = "Customer 02" };
                
                return new List<BatchTestCustomer> { customer01, customer02 };
            });

        public static IList<BatchTestCustomer> Customers
        {
            get
            {
                return _customers.Value;
            }
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IList<BatchTestOrder> Orders { get; set; }
    }

    public class BatchTestOrder
    {
        private static Lazy<IList<BatchTestOrder>> _orders = 
            new Lazy<IList<BatchTestOrder>>(() => {
                BatchTestOrder order01 = new BatchTestOrder { Id = 1, Amount = 100 };
                
                return new List<BatchTestOrder> { order01 };
            });

        public static IList<BatchTestOrder> Orders
        {
            get
            {
                return _orders.Value;
            }
        }

        public int Id { get; set; }
        public decimal Amount { get; set; }
    }

    public class BatchTestCustomersController : TestODataController
    {
        [EnableQuery]
        public IEnumerable<BatchTestCustomer> Get()
        {
            return BatchTestCustomer.Customers;
        }

        [EnableQuery]
        public SingleResult<BatchTestCustomer> Get([FromODataUri]int key)
        {
            return SingleResult.Create(BatchTestCustomer.Customers.Where(d => d.Id.Equals(key)).AsQueryable());
        }

        public ITestActionResult CreateRef([FromODataUri]int key, [FromODataUri]string navigationProperty, [FromBody]Uri link)
        {
            var customer = BatchTestCustomer.Customers.FirstOrDefault(d => d.Id.Equals(key));
            if (customer == null)
                return NotFound();

            switch (navigationProperty)
            {
                case "Orders":
                    var orderId = GetKeyFromLinkUri<int>(Request, link);
                    var order = BatchTestOrder.Orders.FirstOrDefault(d => d.Id.Equals(orderId));

                    if (order == null)
                        return NotFound();

                    if (customer.Orders == null)
                        customer.Orders = new List<BatchTestOrder>();
                    if (customer.Orders.FirstOrDefault(d => d.Id.Equals(orderId)) == null)
                        customer.Orders.Add(order);
                    break;
                default:
                    return BadRequest();
            }

            return NoContent();
        }
    }

    public class BatchTestOrdersController : TestODataController
    {
        [EnableQuery]
        public IEnumerable<BatchTestOrder> Get()
        {
            return BatchTestOrder.Orders;
        }

        public ITestActionResult Post([FromBody]BatchTestOrder order)
        {
            BatchTestOrder.Orders.Add(order);

            return Created(order);
        }
    }
}
