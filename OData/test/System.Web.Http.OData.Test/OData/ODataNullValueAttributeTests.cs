// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData
{
    public class ODataNullValueAttributeTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_ActionExecutedContext()
        {
            ODataNullValueAttribute odataNullValue = new ODataNullValueAttribute();

            Assert.ThrowsArgumentNull(() => { odataNullValue.OnActionExecuted(null); }, "actionExecutedContext");
        }

        [Fact]
        public void OnActionExecuted_Generates404_IfContentIsNull()
        {
            IEdmModel model = new Mock<IEdmModel>().Object;
            ODataPath path = new ODataPath(new EntitySetPathSegment("FakeEntitySet"),
                                           new KeyValuePathSegment("FakeKey"),
                                           new PropertyAccessPathSegment("FakeProperty"),
                                           new ValuePathSegment());
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            request.ODataProperties().Model = model;
            request.ODataProperties().Path = path;
            ODataNullValueAttribute odataNullValue = new ODataNullValueAttribute();
            HttpResponseMessage response = SetUpResponse(HttpStatusCode.OK, null, typeof(object));
            HttpActionExecutedContext context = SetUpContext(request, response);

            odataNullValue.OnActionExecuted(context);

            Assert.NotNull(context.Response);
            Assert.Equal(HttpStatusCode.NotFound, context.Response.StatusCode);
        }

        [Fact]
        public void OnActionExecuted_DoesntChangeTheResponse_IfContentIsntNull()
        {
            IEdmModel model = new Mock<IEdmModel>().Object;
            ODataPath path = new ODataPath(new EntitySetPathSegment("FakeEntitySet"),
                                           new KeyValuePathSegment("FakeKey"),
                                           new PropertyAccessPathSegment("FakeProperty"),
                                           new ValuePathSegment());
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            request.ODataProperties().Model = model;
            request.ODataProperties().Path = path;
            ODataNullValueAttribute odataNullValue = new ODataNullValueAttribute();
            HttpResponseMessage response = SetUpResponse(HttpStatusCode.OK, 5, typeof(int));
            HttpActionExecutedContext context = SetUpContext(request, response);

            odataNullValue.OnActionExecuted(context);

            Assert.Equal(response, context.Response);
        }

        [Fact]
        public void OnActionExecuted_DoesntChangeTheResponse_IfResponseIsntSuccessful()
        {
            IEdmModel model = new Mock<IEdmModel>().Object;
            ODataPath path = new ODataPath(new EntitySetPathSegment("FakeEntitySet"),
                                           new KeyValuePathSegment("FakeKey"),
                                           new PropertyAccessPathSegment("FakeProperty"),
                                           new ValuePathSegment());
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            request.ODataProperties().Model = model;
            request.ODataProperties().Path = path;
            ODataNullValueAttribute odataNullValue = new ODataNullValueAttribute();
            HttpResponseMessage response = SetUpResponse(HttpStatusCode.InternalServerError, null, typeof(object));
            HttpActionExecutedContext context = SetUpContext(request, response);

            odataNullValue.OnActionExecuted(context);

            Assert.Equal(response, context.Response);
        }

        [Fact]
        public void OnActionExecuted_DoesntChangeTheResponse_IfNonODataRequests()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            ODataNullValueAttribute odataNullValue = new ODataNullValueAttribute();
            HttpResponseMessage response = SetUpResponse(HttpStatusCode.OK, null, typeof(object));
            HttpActionExecutedContext context = SetUpContext(request, response);

            odataNullValue.OnActionExecuted(context);

            Assert.Equal(response, context.Response);
        }

        [Fact]
        public void OnActionExecuted_DoesntChangeTheResponse_OfOtherODataRequests()
        {
            IEdmModel model = new Mock<IEdmModel>().Object;
            ODataPath path = new ODataPath(new EntitySetPathSegment("FakeEntitySet"),
                                           new KeyValuePathSegment("FakeKey"),
                                           new PropertyAccessPathSegment("FakeProperty"));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            request.ODataProperties().Model = model;
            request.ODataProperties().Path = path;
            ODataNullValueAttribute odataNullValue = new ODataNullValueAttribute();
            HttpResponseMessage response = SetUpResponse(HttpStatusCode.InternalServerError, null, typeof(object));
            HttpActionExecutedContext context = SetUpContext(request, response);

            odataNullValue.OnActionExecuted(context);

            Assert.Equal(response, context.Response);
        }

        private static HttpResponseMessage SetUpResponse(HttpStatusCode statusCode, object value, Type type)
        {
            Mock<MediaTypeFormatter> formatter = new Mock<MediaTypeFormatter>();
            formatter.Setup(x => x.CanWriteType(It.IsAny<Type>())).Returns(true);
            HttpResponseMessage response = new HttpResponseMessage(statusCode)
            {
                Content = new ObjectContent(type, value, formatter.Object)
            };
            return response;
        }

        private static HttpActionExecutedContext SetUpContext(HttpRequestMessage request, HttpResponseMessage response)
        {
            HttpControllerContext controllerContext = new HttpControllerContext();
            controllerContext.Request = request;
            HttpActionDescriptor descriptor = new ReflectedHttpActionDescriptor();
            HttpActionContext actionContext = new HttpActionContext(controllerContext, descriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);
            context.Response = response;
            return context;
        }
    }
}
