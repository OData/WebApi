// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.AspNet.OData.Common;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Batch
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
        [InlineData(true)]
        [InlineData(false)]
        public async Task ProcessBatchAsync_ContinueOnError(bool enableContinueOnError)
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
            batchRequest.SetConfiguration(enableContinueOnErrorconfig);
            batchRequest.EnableHttpDependencyInjectionSupport();
            HttpRequestMessage batchRequestWithPrefContinueOnError = new HttpRequestMessage(HttpMethod.Post, "http://example.com/$batch")
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
            batchRequestWithPrefContinueOnError.EnableHttpDependencyInjectionSupport();
            if (enableContinueOnError)
            {
                batchRequestWithPrefContinueOnError.SetConfiguration(enableContinueOnErrorconfig);
                batchRequestWithPrefContinueOnError.Headers.Add("prefer", "odata.continue-on-error");
            }

            // Act
            var response = await batchHandler.ProcessBatchAsync(batchRequest, CancellationToken.None);
            var batchContent = Assert.IsType<ODataBatchContent>(response.Content);
            var batchResponses = batchContent.Responses.ToArray();
            var responseWithPrefContinueOnError = await batchHandler.ProcessBatchAsync(batchRequestWithPrefContinueOnError, CancellationToken.None);
            var batchContentWithPrefContinueOnError = Assert.IsType<ODataBatchContent>(responseWithPrefContinueOnError.Content);
            var batchResponsesWithPrefContinueOnError = batchContentWithPrefContinueOnError.Responses.ToArray();

            // Assert
            Assert.Equal(2, batchResponses.Length);
            Assert.Equal(3, batchResponsesWithPrefContinueOnError.Length);
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
            Assert.Equal("The batch request must have 'multipart/mixed' as the media type.",
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
    }
}
#endif