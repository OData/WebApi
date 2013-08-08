// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.OData.Batch;
using System.Web.Http.Routing;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http
{
    public class UnbufferedODataBatchHandlerTest
    {
        [Fact]
        public void Parameter_Constructor()
        {
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(new HttpServer());

            Assert.NotNull(batchHandler.Invoker);
            Assert.NotNull(batchHandler.MessageQuotas);
            Assert.Null(batchHandler.ODataRouteName);
        }

        [Fact]
        public void Constructor_Throws_IfHttpServerIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => new UnbufferedODataBatchHandler(null),
                "httpServer");
        }

        [Fact]
        public void CreateResponseMessageAsync_Throws_IfResponsesAreNull()
        {
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(new HttpServer());
            Assert.ThrowsArgumentNull(
                () => batchHandler.CreateResponseMessageAsync(null, new HttpRequestMessage(), CancellationToken.None).Wait(),
                "responses");
        }

        [Fact]
        public void ProcessBatchAsync_Throws_IfRequestIsNull()
        {
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(new HttpServer());
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
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(server);
            HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://example.com/$batch")
            {
                Content = new MultipartContent("mixed")
                {
                    ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Get, "http://example.com/")),
                    new MultipartContent("mixed") // ChangeSet
                    {
                        ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Post, "http://example.com/"))
                    }
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
        public void ProcessBatchAsync_CallsInvokerForEachRequest()
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
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(server);
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
                    }
                }
            };

            var response = batchHandler.ProcessBatchAsync(batchRequest, CancellationToken.None).Result;

            var batchContent = Assert.IsType<ODataBatchContent>(response.Content);
            var batchResponses = batchContent.Responses.ToArray();
            Assert.Equal(2, batchResponses.Length);
            Assert.Equal("http://example.com/", ((OperationResponseItem)batchResponses[0]).Response.Content.ReadAsStringAsync().Result);
            Assert.Equal("http://example.com/values,foo", ((ChangeSetResponseItem)batchResponses[1]).Responses.First().Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void ProcessBatchAsync_DisposesResponseInCaseOfException()
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
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(server);
            HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://example.com/$batch")
            {
                Content = new MultipartContent("mixed")
                {
                    ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Get, "http://example.com/")),
                    new MultipartContent("mixed") // ChangeSet
                    {
                        ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Post, "http://example.com/values")),
                        ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Put, "http://example.com/values"))
                    }
                }
            };

            Assert.Throws<InvalidOperationException>(
                () => batchHandler.ProcessBatchAsync(batchRequest, CancellationToken.None).Result);

            Assert.Equal(2, responses.Count);
            foreach (var response in responses)
            {
                Assert.True(response.IsDisposed);
            }
        }

        [Fact]
        public void ValidateRequest_Throws_IfResponsesIsNull()
        {
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(new HttpServer());
            Assert.ThrowsArgumentNull(
                () => batchHandler.ValidateRequest(null),
                "request");
        }

        [Fact]
        public void ExecuteChangeSet_Throws_IfReaderIsNull()
        {
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(new HttpServer());
            Assert.ThrowsArgumentNull(
                () => batchHandler.ExecuteChangeSetAsync(null, Guid.NewGuid(), new HttpRequestMessage(), CancellationToken.None).Wait(),
                "batchReader");
        }

        [Fact]
        public void ExecuteChangeSet_Throws_IfRequestIsNull()
        {
            var httpContent = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            var reader = httpContent.GetODataMessageReaderAsync(new ODataMessageReaderSettings(), CancellationToken.None).Result;
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(new HttpServer());

            Assert.ThrowsArgumentNull(
                () => batchHandler.ExecuteChangeSetAsync(reader.CreateODataBatchReader(), Guid.NewGuid(), null, CancellationToken.None).Wait(),
                "originalRequest");
        }

        [Fact]
        public void ExecuteChangeSetAsync_CopiesPropertiesFromRequest_WithoutExcludedProperties()
        {
            MockHttpServer server = new MockHttpServer(request =>
            {
                return new HttpResponseMessage
                {
                    RequestMessage = request
                };
            });
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(server);
            HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://example.com/$batch")
            {
                Content = new MultipartContent("mixed")
                {
                    new MultipartContent("mixed") // ChangeSet
                    {
                        ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Post, "http://example.com/values")
                        {
                            Content = new StringContent("foo")
                        })
                    }
                }
            };
            batchRequest.Properties.Add("foo", "bar");
            batchRequest.SetRouteData(new HttpRouteData(new HttpRoute()));
            batchRequest.RegisterForDispose(new StringContent(String.Empty));
            ODataMessageReader reader = batchRequest.Content
                .GetODataMessageReaderAsync(new ODataMessageReaderSettings { BaseUri = new Uri("http://example.com") }, CancellationToken.None)
                .Result;
            ODataBatchReader batchReader = reader.CreateODataBatchReader();
            List<ODataBatchResponseItem> responses = new List<ODataBatchResponseItem>();
            Guid batchId = Guid.NewGuid();
            batchReader.Read();

            var response = batchHandler.ExecuteChangeSetAsync(batchReader, Guid.NewGuid(), batchRequest, CancellationToken.None).Result;

            var changeSetResponses = ((ChangeSetResponseItem)response).Responses;
            foreach (var changeSetResponse in changeSetResponses)
            {
                var changeSetRequest = changeSetResponse.RequestMessage;
                Assert.Equal("bar", changeSetRequest.Properties["foo"]);
                Assert.Null(changeSetRequest.GetRouteData());
                Assert.Same(changeSetRequest, changeSetRequest.GetUrlHelper().Request);
                Assert.Empty(changeSetRequest.GetResourcesForDisposal());
            }
        }

        [Fact]
        public void ExecuteChangeSetAsync_ReturnsSingleErrorResponse()
        {
            MockHttpServer server = new MockHttpServer(request =>
            {
                if (request.Method == HttpMethod.Post)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest));
                }
                return Task.FromResult(new HttpResponseMessage());
            });
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(server);
            HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://example.com/$batch")
            {
                Content = new MultipartContent("mixed")
                {
                    new MultipartContent("mixed") // ChangeSet
                    {
                        ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Put, "http://example.com/values")),
                        ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Post, "http://example.com/values")),
                        ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Delete, "http://example.com/values")),
                    }
                }
            };
            ODataMessageReader reader = batchRequest.Content
                .GetODataMessageReaderAsync(new ODataMessageReaderSettings { BaseUri = new Uri("http://example.com") }, CancellationToken.None)
                .Result;
            ODataBatchReader batchReader = reader.CreateODataBatchReader();

            var response = batchHandler.ExecuteChangeSetAsync(batchReader, Guid.NewGuid(), batchRequest, CancellationToken.None).Result;

            var changesetResponse = Assert.IsType<ChangeSetResponseItem>(response);
            Assert.Equal(1, changesetResponse.Responses.Count());
            Assert.Equal(HttpStatusCode.BadRequest, changesetResponse.Responses.First().StatusCode);
        }

        [Fact]
        public void ExecuteOperationAsync_CopiesPropertiesFromRequest_WithoutExcludedProperties()
        {
            MockHttpServer server = new MockHttpServer(request =>
            {
                return new HttpResponseMessage
                {
                    RequestMessage = request
                };
            });
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(server);
            HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://example.com/$batch")
            {
                Content = new MultipartContent("mixed")
                {
                    ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Get, "http://example.com/")),
                }
            };
            batchRequest.Properties.Add("foo", "bar");
            batchRequest.SetRouteData(new HttpRouteData(new HttpRoute()));
            batchRequest.RegisterForDispose(new StringContent(String.Empty));
            ODataMessageReader reader = batchRequest.Content
                .GetODataMessageReaderAsync(new ODataMessageReaderSettings { BaseUri = new Uri("http://example.com") }, CancellationToken.None)
                .Result;
            ODataBatchReader batchReader = reader.CreateODataBatchReader();
            List<ODataBatchResponseItem> responses = new List<ODataBatchResponseItem>();
            Guid batchId = Guid.NewGuid();
            batchReader.Read();

            var response = batchHandler.ExecuteOperationAsync(batchReader, Guid.NewGuid(), batchRequest, CancellationToken.None).Result;

            var operationResponse = ((OperationResponseItem)response).Response;
            var operationRequest = operationResponse.RequestMessage;
            Assert.Equal("bar", operationRequest.Properties["foo"]);
            Assert.Null(operationRequest.GetRouteData());
            Assert.Same(operationRequest, operationRequest.GetUrlHelper().Request);
            Assert.Empty(operationRequest.GetResourcesForDisposal());
        }

        [Fact]
        public void ExecuteOperation_Throws_IfReaderIsNull()
        {
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(new HttpServer());
            Assert.ThrowsArgumentNull(
                () => batchHandler.ExecuteOperationAsync(null, Guid.NewGuid(), new HttpRequestMessage(), CancellationToken.None).Wait(),
                "batchReader");
        }

        [Fact]
        public void ExecuteOperation_Throws_IfRequestIsNull()
        {
            var httpContent = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            var reader = httpContent.GetODataMessageReaderAsync(new ODataMessageReaderSettings(), CancellationToken.None).Result;
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(new HttpServer());

            Assert.ThrowsArgumentNull(
                () => batchHandler.ExecuteOperationAsync(reader.CreateODataBatchReader(), Guid.NewGuid(), null, CancellationToken.None).Wait(),
                "originalRequest");
        }
    }
}