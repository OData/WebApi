// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http.OData;
using System.Web.Http.OData.Batch;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http
{
    public class DefaultODataBatchHandlerTest
    {
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
            Assert.ThrowsArgumentNull(
                () => new DefaultODataBatchHandler(null),
                "httpServer");
        }

        [Fact]
        public void CreateResponseMessageAsync_Throws_IfResponsesAreNull()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            Assert.ThrowsArgumentNull(
                () => batchHandler.CreateResponseMessageAsync(null, new HttpRequestMessage(), CancellationToken.None).Wait(),
                "responses");
        }

        [Fact]
        public void CreateResponseMessageAsync_Throws_IfRequestIsNull()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            Assert.ThrowsArgumentNull(
                () => batchHandler.CreateResponseMessageAsync(new ODataBatchResponseItem[0], null, CancellationToken.None).Wait(),
                "request");
        }

        [Fact]
        public void CreateResponseMessageAsync_ReturnsODataBatchContent()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            ODataBatchResponseItem[] responses = new ODataBatchResponseItem[]
            {
                new OperationResponseItem(new HttpResponseMessage(HttpStatusCode.OK))
            };

            HttpResponseMessage response = batchHandler.CreateResponseMessageAsync(responses, new HttpRequestMessage(), CancellationToken.None).Result;

            var batchContent = Assert.IsType<ODataBatchContent>(response.Content);
            Assert.Equal(1, batchContent.Responses.Count());
        }

        [Fact]
        public void ProcessBatchAsync_Throws_IfRequestIsNull()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            Assert.ThrowsArgumentNull(
                () => batchHandler.ProcessBatchAsync(null, CancellationToken.None).Wait(),
                "request");
        }

        [Fact]
        public void ProcessBatchAsync_CallsRegisterForDispose()
        {
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

            var response = batchHandler.ProcessBatchAsync(batchRequest, CancellationToken.None).Result;
            var resourcesForDisposal = batchRequest.GetResourcesForDisposal();

            foreach (var expectedResource in expectedResourcesForDisposal)
            {
                Assert.Contains(expectedResource, resourcesForDisposal);
            }
        }

        [Fact]
        public void ExecuteRequestMessagesAsync_CallsInvokerForEachRequest()
        {
            MockHttpServer server = new MockHttpServer(request =>
            {
                string responseContent = request.RequestUri.AbsoluteUri;
                if (request.Content != null)
                {
                    string content = request.Content.ReadAsStringAsync().Result;
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

            var responses = batchHandler.ExecuteRequestMessagesAsync(requests, CancellationToken.None).Result;

            Assert.Equal(2, responses.Count);
            Assert.Equal("http://example.com/", ((OperationResponseItem)responses[0]).Response.Content.ReadAsStringAsync().Result);
            Assert.Equal("http://example.com/values,foo", ((ChangeSetResponseItem)responses[1]).Responses.First().Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void ExecuteRequestMessagesAsync_DisposesResponseInCaseOfException()
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

            Assert.Throws<InvalidOperationException>(
                () => batchHandler.ExecuteRequestMessagesAsync(requests, CancellationToken.None).Result);

            Assert.Equal(2, responses.Count);
            foreach (var response in responses)
            {
                Assert.True(response.IsDisposed);
            }
        }

        [Fact]
        public void ExecuteRequestMessagesAsync_Throws_IfRequestsIsNull()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            Assert.ThrowsArgumentNull(
                () => batchHandler.ExecuteRequestMessagesAsync(null, CancellationToken.None).Wait(),
                "requests");
        }

        [Fact]
        public void ParseBatchRequestsAsync_Throws_IfRequestIsNull()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            Assert.ThrowsArgumentNull(
                () => batchHandler.ParseBatchRequestsAsync(null, CancellationToken.None).Wait(),
                "request");
        }

        [Fact]
        public void ParseBatchRequestsAsync_Returns_RequestsFromMultipartContent()
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

            IList<ODataBatchRequestItem> requests = batchHandler.ParseBatchRequestsAsync(batchRequest, CancellationToken.None).Result;

            Assert.Equal(2, requests.Count);

            var operationRequest = ((OperationRequestItem)requests[0]).Request;
            Assert.Equal(HttpMethod.Get, operationRequest.Method);
            Assert.Equal("http://example.com/", operationRequest.RequestUri.AbsoluteUri);

            var changeSetRequest = ((ChangeSetRequestItem)requests[1]).Requests.First();
            Assert.Equal(HttpMethod.Post, changeSetRequest.Method);
            Assert.Equal("http://example.com/values", changeSetRequest.RequestUri.AbsoluteUri);
        }

        [Fact]
        public void ParseBatchRequestsAsync_CopiesPropertiesFromRequest_WithoutExcludedProperties()
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

            IList<ODataBatchRequestItem> requests = batchHandler.ParseBatchRequestsAsync(batchRequest, CancellationToken.None).Result;

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
            Assert.ThrowsArgumentNull(
                () => batchHandler.ValidateRequest(null),
                "request");
        }

        [Fact]
        public void ValidateRequest_Throws_IfRequestContentIsNull()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            HttpRequestMessage request = new HttpRequestMessage();

            HttpResponseException errorResponse = Assert.Throws<HttpResponseException>(
                () => batchHandler.ValidateRequest(request));
            Assert.Equal(HttpStatusCode.BadRequest, errorResponse.Response.StatusCode);
            Assert.Equal("The 'Content' property on the batch request cannot be null.",
                errorResponse.Response.Content.ReadAsAsync<HttpError>().Result.Message);
        }

        [Fact]
        public void ValidateRequest_Throws_IfRequestContentTypeIsNull()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent(String.Empty);
            request.Content.Headers.ContentType = null;

            HttpResponseException errorResponse = Assert.Throws<HttpResponseException>(
                () => batchHandler.ValidateRequest(request));
            Assert.Equal(HttpStatusCode.BadRequest, errorResponse.Response.StatusCode);
            Assert.Equal("The batch request must have a \"Content-Type\" header.",
                errorResponse.Response.Content.ReadAsAsync<HttpError>().Result.Message);
        }

        [Fact]
        public void ValidateRequest_Throws_IfRequestMediaTypeIsWrong()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent(String.Empty);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/json");

            HttpResponseException errorResponse = Assert.Throws<HttpResponseException>(
                () => batchHandler.ValidateRequest(request));
            Assert.Equal(HttpStatusCode.BadRequest, errorResponse.Response.StatusCode);
            Assert.Equal("The batch request must have 'multipart/mixed' as the media type.",
                errorResponse.Response.Content.ReadAsAsync<HttpError>().Result.Message);
        }

        [Fact]
        public void ValidateRequest_Throws_IfRequestContentTypeDoesNotHaveBoundary()
        {
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler(new HttpServer());
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent(String.Empty);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("multipart/mixed");

            HttpResponseException errorResponse = Assert.Throws<HttpResponseException>(
                () => batchHandler.ValidateRequest(request));
            Assert.Equal(HttpStatusCode.BadRequest, errorResponse.Response.StatusCode);
            Assert.Equal("The batch request must have a boundary specification in the \"Content-Type\" header.",
                errorResponse.Response.Content.ReadAsAsync<HttpError>().Result.Message);
        }

        [Fact]
        public void BatchRequest_Works_AbsoluteAndRelativeUri()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<BatchCustomer>("BatchCustomers");
            IEdmModel model = builder.GetEdmModel();

            HttpConfiguration configuration = new HttpConfiguration();
            HttpServer server = new HttpServer(configuration);
            HttpClient client = new HttpClient(server);
            configuration.Routes.MapODataServiceRoute("odata", "odata", model, new DefaultODataBatchHandler(server));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/odata/$batch");
            request.Content = new StringContent(
@"--batch_36522ad7-fc75-4b56-8c71-56071383e77b
Content-Type: application/http
Content-Transfer-Encoding:binary

GET http://localhost/odata/BatchCustomers(5) HTTP/1.1

--batch_36522ad7-fc75-4b56-8c71-56071383e77b
Content-Type: application/http
Content-Transfer-Encoding:binary

GET /odata/BatchCustomers(6) HTTP/1.1
Host: localhost

--batch_36522ad7-fc75-4b56-8c71-56071383e77b
Content-Type: application/http
Content-Transfer-Encoding:binary

GET BatchCustomers(7) HTTP/1.1

--batch_36522ad7-fc75-4b56-8c71-56071383e77b--
");
            request.Content.Headers.ContentType =
                MediaTypeHeaderValue.Parse("multipart/mixed;boundary=batch_36522ad7-fc75-4b56-8c71-56071383e77b");

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            Assert.Contains("\"odata.metadata\":\"http://localhost/odata/$metadata#BatchCustomers/@Element\",\"ID\":5", responseString);
            Assert.Contains("\"odata.metadata\":\"http://localhost/odata/$metadata#BatchCustomers/@Element\",\"ID\":6", responseString);
            Assert.Contains("\"odata.metadata\":\"http://localhost/odata/$metadata#BatchCustomers/@Element\",\"ID\":7", responseString);
        }

        public class BatchCustomersController : ODataController
        {
            public IHttpActionResult Get(int key)
            {
                return Ok(new BatchCustomer { ID = key });
            }
        }

        public class BatchCustomer
        {
            public int ID { get; set; }
        }
    }
}