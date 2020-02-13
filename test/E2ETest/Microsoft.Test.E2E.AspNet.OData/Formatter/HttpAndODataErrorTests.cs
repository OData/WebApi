// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE // TODO #939: Enable these tests for AspNetCore
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter
{
    public enum ErrorType
    {
        ThrowExceptionInAction,
        ThrowHttpResponseExceptionInAction,
        ResponseErrorResponseInAction,
        ResponseHttpErrorResponseInAction,
        ReturnODataErrorResponseInAction,
        QueryableThrowException,
        NotSupportGetException,
        ActionNotFound,
        ModelStateError
    }

    public class HttpError_Todo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ErrorType ErrorType { get; set; }
        public IEnumerable<HttpError_Item> Items { get; set; }
    }

    public class HttpError_Item
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class HttpError_TodoController : TestODataController
    {
        [HttpPost]
        public HttpError_Todo Get(int key)
        {
            return null;
        }

        [EnableQuery]
        public HttpResponseMessage Post(HttpError_Todo todo)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            switch (todo.ErrorType)
            {
                case ErrorType.ThrowExceptionInAction:
                    throw new Exception("ThrowExceptionInAction");
                case ErrorType.ThrowHttpResponseExceptionInAction:
                    throw new System.Web.Http.HttpResponseException(
                        this.Request.CreateErrorResponse(
                            HttpStatusCode.NotFound,
                            new Exception("ThrowHttpResponseExceptionInAction")));
                case ErrorType.ResponseErrorResponseInAction:
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.NotFound,
                        new Exception("ResponseErrorResponseInAction"));
                case ErrorType.ReturnODataErrorResponseInAction:
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new ODataError()
                    {
                        Message = "ReturnODataErrorResponseInActionMessage",
                        ErrorCode = "ReturnODataErrorResponseInActionCode",
                        InnerError = new ODataInnerError(new Exception("ReturnODataErrorResponseInActionException"))
                    });
                case ErrorType.ResponseHttpErrorResponseInAction:
                    return
                        this.Request.CreateResponse<System.Web.Http.HttpError>(
                            HttpStatusCode.NotFound,
                            new System.Web.Http.HttpError("ResponseHttpErrorResponseInAction"));
                default:
                    return null;
            }
        }

        [EnableQuery]
        public HttpError_Item GetItems(int key)
        {
            return null;
        }
    }

    public class HttpAndODataErrorAlwaysIncludeDetailsTests : WebHostTestBase<HttpAndODataErrorAlwaysIncludeDetailsTests>
    {
        public HttpAndODataErrorAlwaysIncludeDetailsTests(WebHostTestFixture<HttpAndODataErrorAlwaysIncludeDetailsTests> fixture)
            :base(fixture)
        {
        }

        public static TheoryDataSet<ErrorType, int, string, string> TestData
        {
            get
            {
                var testData = new List<object[]>
                {
                    new object[] { ErrorType.ThrowExceptionInAction, (int)HttpStatusCode.InternalServerError, "ThrowExceptionInAction" },
                    new object[] { ErrorType.ThrowHttpResponseExceptionInAction, (int)HttpStatusCode.NotFound, "ThrowHttpResponseExceptionInAction" },
                    new object[] { ErrorType.ResponseErrorResponseInAction, (int)HttpStatusCode.NotFound, "ResponseErrorResponseInAction" },
                    new object[] { ErrorType.ResponseHttpErrorResponseInAction, (int)HttpStatusCode.NotFound, "ResponseHttpErrorResponseInAction" },
                    new object[] { ErrorType.QueryableThrowException, (int)HttpStatusCode.InternalServerError, "Cannot serialize a null 'Resource'." },
                    new object[] { ErrorType.NotSupportGetException, (int)HttpStatusCode.MethodNotAllowed, "The requested resource does not support http method 'GET'." },
                    new object[] { ErrorType.ActionNotFound, (int)HttpStatusCode.NotFound, "No HTTP resource was found that matches the request URI" },
                    new object[] { ErrorType.ReturnODataErrorResponseInAction, (int)HttpStatusCode.InternalServerError, "ReturnODataErrorResponseInActionException" },
                    new object[] { ErrorType.ModelStateError, (int)HttpStatusCode.BadRequest, "Requested value 'NotExistType' was not found." }
                };
                var acceptHeaders = new List<string> 
                {
                    "application/json;odata.metadata=minimal;odata.streaming=true",
                    "application/json;odata.metadata=minimal;odata.streaming=false",
                    "application/json;odata.metadata=minimal",
                    "application/json;odata.metadata=full;odata.streaming=true",
                    "application/json;odata.metadata=full;odata.streaming=false",
                    "application/json;odata.metadata=full",
                    "application/json;odata.metadata=none;odata.streaming=true",
                    "application/json;odata.metadata=none;odata.streaming=false",
                    "application/json;odata.metadata=none",
                    "application/json;odata.streaming=true",
                    "application/json;odata.streaming=false",
                    "application/json"
                };
                var theory = new TheoryDataSet<ErrorType, int, string, string>();
                foreach (var acceptHeader in acceptHeaders)
                {
                    foreach (dynamic item in testData)
                    {
                        theory.Add(item[0], item[1], item[2], acceptHeader);
                    }
                }
                return theory;
            }
        }

        public static HttpRequestMessage CreateRequestMessage(string baseAddress, ErrorType errorType, string header)
        {
            var request = new HttpRequestMessage();
            switch (errorType)
            {
                case ErrorType.ThrowExceptionInAction:
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(baseAddress + "/HttpError_Todo/");
                    request.Content = new StringContent(@"{ ""ErrorType"": ""ThrowExceptionInAction"" }");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(header);
                    break;
                case ErrorType.ThrowHttpResponseExceptionInAction:
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(baseAddress + "/HttpError_Todo/");
                    request.Content = new StringContent(@"{ ""ErrorType"": ""ThrowHttpResponseExceptionInAction"" }");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(header);
                    break;
                case ErrorType.ResponseErrorResponseInAction:
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(baseAddress + "/HttpError_Todo/");
                    request.Content = new StringContent(@"{ ""ErrorType"": ""ResponseErrorResponseInAction"" }");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(header);
                    break;
                case ErrorType.ResponseHttpErrorResponseInAction:
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(baseAddress + "/HttpError_Todo/");
                    request.Content = new StringContent(@"{ ""ErrorType"": ""ResponseHttpErrorResponseInAction"" }");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(header);
                    break;
                case ErrorType.ReturnODataErrorResponseInAction:
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(baseAddress + "/HttpError_Todo/");
                    request.Content = new StringContent(@"{ ""ErrorType"": ""ReturnODataErrorResponseInAction"" }");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(header);
                    break;
                case ErrorType.QueryableThrowException:
                    request.Method = HttpMethod.Get;
                    request.RequestUri = new Uri(baseAddress + "/HttpError_Todo(1)/Items");
                    request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(header));
                    break;
                case ErrorType.NotSupportGetException:
                    request.Method = HttpMethod.Get;
                    request.RequestUri = new Uri(baseAddress + "/HttpError_Todo(1)");
                    request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(header));
                    break;
                case ErrorType.ActionNotFound:
                    request.Method = HttpMethod.Get;
                    request.RequestUri = new Uri(baseAddress + "/HttpError_Todo(1)/Name");
                    request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(header));
                    break;
                case ErrorType.ModelStateError:
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(baseAddress + "/HttpError_Todo/");
                    request.Content = new StringContent(@"{ ""ErrorType"": ""NotExistType"" }");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(header);
                    break;
                default:
                    break;
            }

            return request;
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel(configuration));
            configuration.AddODataQueryFilter(new EnableQueryAttribute() { PageSize = 100 });
        }

        public static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var mb = configuration.CreateConventionModelBuilder();
            mb.EntitySet<HttpError_Todo>("HttpError_Todo");
            mb.EntitySet<HttpError_Item>("HttpError_Item");
            return mb.GetEdmModel();
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public virtual async Task TestHttpErrorInAction(ErrorType errorType, int code, string message, string header)
        {
            // Arrange
            if (header != string.Empty)
            {
                Client.DefaultRequestHeaders.Accept.Clear();
                Client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(header));
            }

            // Act
            var request = CreateRequestMessage(this.BaseAddress, errorType, header);
            var response = await this.Client.SendAsync(request);
            string responseMessage = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(code, (int)response.StatusCode);
            Assert.Contains(message, responseMessage);
        }
    }

    public class HttpAndODataErrorNeverIncludeDetailsTests : WebHostTestBase<HttpAndODataErrorNeverIncludeDetailsTests>
    {
        public HttpAndODataErrorNeverIncludeDetailsTests(WebHostTestFixture<HttpAndODataErrorNeverIncludeDetailsTests> fixture)
            :base(fixture)
        {
        }

        public static TheoryDataSet<ErrorType, int, string, string> TestData
        {
            get
            {
                var testData = new List<object[]>
                {
                    new object[] { ErrorType.ThrowExceptionInAction, (int)HttpStatusCode.InternalServerError, "ThrowExceptionInAction" },
                    new object[] { ErrorType.ThrowHttpResponseExceptionInAction, (int)HttpStatusCode.NotFound, "ThrowHttpResponseExceptionInAction" },
                    new object[] { ErrorType.ResponseErrorResponseInAction, (int)HttpStatusCode.NotFound, "ResponseErrorResponseInAction" },
                    new object[] { ErrorType.QueryableThrowException, (int)HttpStatusCode.InternalServerError, "Cannot serialize a null 'Resource'." },
                    new object[] { ErrorType.ReturnODataErrorResponseInAction, (int)HttpStatusCode.InternalServerError, "ReturnODataErrorResponseInActionException" },
                    new object[] { ErrorType.ModelStateError, (int)HttpStatusCode.BadRequest, "Requested value 'NotExistType' was not found." }
                };
                var acceptHeaders = new List<string> 
                {
                    "application/json;odata.metadata=minimal;odata.streaming=true",
                    "application/json;odata.metadata=minimal;odata.streaming=false",
                    "application/json;odata.metadata=minimal",
                    "application/json;odata.metadata=full;odata.streaming=true",
                    "application/json;odata.metadata=full;odata.streaming=false",
                    "application/json;odata.metadata=full",
                    "application/json;odata.metadata=none;odata.streaming=true",
                    "application/json;odata.metadata=none;odata.streaming=false",
                    "application/json;odata.metadata=none",
                    "application/json;odata.streaming=true",
                    "application/json;odata.streaming=false",
                    "application/json"
                };
                var theory = new TheoryDataSet<ErrorType, int, string, string>();
                foreach (var acceptHeader in acceptHeaders)
                {
                    foreach (dynamic item in testData)
                    {
                        theory.Add(item[0], item[1], item[2], acceptHeader);
                    }
                }
                return theory;
            }
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.IncludeErrorDetail = false;
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(HttpAndODataErrorAlwaysIncludeDetailsTests.GetEdmModel(configuration));
            configuration.AddODataQueryFilter(new EnableQueryAttribute() { PageSize = 100 });
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task TestHttpErrorInAction(ErrorType errorType, int code, string message, string header)
        {
            // Arrange
            if (header != string.Empty)
            {
                Client.DefaultRequestHeaders.Accept.Clear();
                Client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(header));
            }

            // Act
            var request = HttpAndODataErrorAlwaysIncludeDetailsTests.CreateRequestMessage(this.BaseAddress, errorType, header);
            var response = await this.Client.SendAsync(request);
            string responseMessage = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(code, (int)response.StatusCode);
            Assert.DoesNotContain(message, responseMessage);
        }
    }
}
#endif
