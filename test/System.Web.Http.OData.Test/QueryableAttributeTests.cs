// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Query.Controllers;
using System.Web.Http.OData.TestCommon.Models;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData
{
    public class QueryableAttributeTests
    {
        public static List<Customer> CustomerList = new List<Customer>()
        {
            new Customer(){ Name = "B" },
            new Customer(){ Name = "C" },
            new Customer(){ Name = "A" },
        };

        public static TheoryDataSet<string, object, bool> DifferentReturnTypeWorksTestData
        {
            get
            {
                return new TheoryDataSet<string, object, bool>
                {
                    { "GetObject", new List<Customer>(CustomerList), false },
                    { "GetObject", new Collection<Customer>(CustomerList), false },
                    { "GetObject", new CustomerCollection(), false }
                };
            }
        }

        // Move items to this list from UnsupportedQueryNames as they become supported
        public static TheoryDataSet<string> SupportedQueryNames
        {
            get { return ODataQueryOptionTest.SupportedQueryNames; }
        }

        // Move items from this list to SupportedQueryNames as they become supported
        public static TheoryDataSet<string> UnsupportedQueryNames
        {
            get { return ODataQueryOptionTest.UnsupportedQueryNames; }
        }

        [Fact]
        public void Ctor_Initializes_Properties()
        {
            // Arrange
            QueryableAttribute attribute = new QueryableAttribute();

            // Act & Assert
            Assert.Equal(HandleNullPropagationOption.Default, attribute.HandleNullPropagation);
            Assert.True(attribute.EnsureStableOrdering);
        }

        [Fact]
        public void EnsureStableOrdering_Property_RoundTrips()
        {
            Assert.Reflection.BooleanProperty<QueryableAttribute>(
                new QueryableAttribute(),
                o => o.EnsureStableOrdering,
                true);
        }

        [Fact]
        public void HandleNullPropagation_Property_RoundTrips()
        {
            Assert.Reflection.EnumProperty<QueryableAttribute, HandleNullPropagationOption>(
                new QueryableAttribute(),
                o => o.HandleNullPropagation,
                HandleNullPropagationOption.Default,
                HandleNullPropagationOption.Default - 1,
                HandleNullPropagationOption.True);
        }

        [Fact]
        public void OnActionExecuted_Throws_Null_Context()
        {
            Assert.ThrowsArgumentNull(() => new QueryableAttribute().OnActionExecuted(null), "actionExecutedContext");
        }

        [Fact]
        public void OnActionExecuted_Throws_Null_Request()
        {
            Assert.ThrowsArgument(
                () => new QueryableAttribute().OnActionExecuted(new HttpActionExecutedContext()),
                "actionExecutedContext",
                String.Format("The HttpExecutedActionContext.Request is null.{0}Parameter name: actionExecutedContext", Environment.NewLine));
        }

        [Fact]
        public void OnActionExecuted_Throws_Null_Configuration()
        {
            // Arrange
            QueryableAttribute attribute = new QueryableAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer/?$orderby=Name");
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerContext controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerHighLevel", typeof(CustomerHighLevelController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(CustomerHighLevelController).GetMethod("Get"));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);

            Assert.ThrowsArgument(
                () => new QueryableAttribute().OnActionExecuted(context),
                "actionExecutedContext",
                String.Format("Request message does not contain an HttpConfiguration object.{0}Parameter name: actionExecutedContext", Environment.NewLine));
        }

        [Theory]
        [PropertyData("DifferentReturnTypeWorksTestData")]
        public void DifferentReturnTypeWorks(string methodName, object responseObject, bool isNoOp)
        {
            // Arrange
            QueryableAttribute attribute = new QueryableAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer?$orderby=Name");
            HttpConfiguration config = new HttpConfiguration();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            HttpControllerContext controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerHighLevel", typeof(CustomerHighLevelController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(CustomerHighLevelController).GetMethod(methodName));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);
            context.Response = new HttpResponseMessage(HttpStatusCode.OK);
            context.Response.Content = new ObjectContent(typeof(IEnumerable<Customer>), responseObject, new JsonMediaTypeFormatter());

            // Act and Assert
            attribute.OnActionExecuted(context);

            Assert.Equal(HttpStatusCode.OK, context.Response.StatusCode);
            Assert.True(context.Response.Content is ObjectContent);
            Assert.Equal(isNoOp, ((ObjectContent)context.Response.Content).Value == responseObject);
        }

        [Fact]
        public void UnknownQueryNotStartingWithDollarSignWorks()
        {
            // Arrange
            QueryableAttribute attribute = new QueryableAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer/?select");
            HttpConfiguration config = new HttpConfiguration();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            HttpControllerContext controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerHighLevel", typeof(CustomerHighLevelController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(CustomerHighLevelController).GetMethod("Get"));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);
            context.Response = new HttpResponseMessage(HttpStatusCode.OK);
            context.Response.Content = new ObjectContent(typeof(IEnumerable<Customer>), new List<Customer>(), new JsonMediaTypeFormatter());

            // Act and Assert
            attribute.OnActionExecuted(context);

            Assert.Equal(HttpStatusCode.OK, context.Response.StatusCode);
        }

        [Fact]
        public void UnknownQueryStartingWithDollarSignThrows()
        {
            // Arrange
            QueryableAttribute attribute = new QueryableAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer/?$select");
            HttpConfiguration config = new HttpConfiguration();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            HttpControllerContext controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerHighLevel", typeof(CustomerHighLevelController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(CustomerHighLevelController).GetMethod("Get"));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);
            context.Response = new HttpResponseMessage(HttpStatusCode.OK);
            context.Response.Content = new ObjectContent(typeof(IEnumerable<Customer>), new List<Customer>(), new JsonMediaTypeFormatter());

            // Act and Assert
            HttpResponseException errorResponse = Assert.Throws<HttpResponseException>(() =>
                attribute.OnActionExecuted(context));

            Assert.Equal(HttpStatusCode.BadRequest, errorResponse.Response.StatusCode);
        }

        [Fact]
        public void NonGenericEnumerableReturnTypeThrows()
        {
            // Arrange
            QueryableAttribute attribute = new QueryableAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer/?$skip=1");
            HttpConfiguration config = new HttpConfiguration();
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            HttpControllerContext controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerHighLevel", typeof(CustomerHighLevelController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(CustomerHighLevelController).GetMethod("GetNonGenericEnumerable"));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);
            context.Response = new HttpResponseMessage(HttpStatusCode.OK);
            context.Response.Content = new ObjectContent(typeof(IEnumerable), new NonGenericEnumerable(), new JsonMediaTypeFormatter());

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => attribute.OnActionExecuted(context),
                "Cannot create an EDM model as the action 'QueryableAttribute' on controller 'GetNonGenericEnumerable' has a return type 'CustomerHighLevel' that does not implement IEnumerable<T>.");
        }

        [Fact]
        public void NonObjectContentResponse_ThrowsInvalidOperationException()
        {
            // Arrange
            QueryableAttribute attribute = new QueryableAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer?$skip=1");
            HttpConfiguration config = new HttpConfiguration();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            HttpControllerContext controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerHighLevel", typeof(CustomerHighLevelController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(CustomerHighLevelController).GetMethod("GetIEnumerableOfCustomer"));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);
            context.Response = new HttpResponseMessage(HttpStatusCode.OK);
            context.Response.Content = new StreamContent(new MemoryStream());

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => attribute.OnActionExecuted(context),
                "Queries can not be applied to a response content of type 'System.Net.Http.StreamContent'. The response content must be an ObjectContent.");
        }

        [Theory]
        [InlineData("GetObject")]
        [InlineData("GetCollectionOfCustomer")]
        [InlineData("GetListOfCustomer")]
        [InlineData("GetStronglyTypedCustomer")]
        [InlineData("GetArrayOfCustomers")]
        [InlineData("GetNonGenericEnumerable")]
        public void InvalidActionReturnType_Throws(string actionName)
        {
            // Arrange
            QueryableAttribute attribute = new QueryableAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer?$skip=1");
            HttpConfiguration config = new HttpConfiguration();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            HttpControllerContext controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerHighLevel", typeof(CustomerHighLevelController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(CustomerHighLevelController).GetMethod(actionName));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);
            context.Response = new HttpResponseMessage(HttpStatusCode.OK);
            Type returnType = actionDescriptor.ReturnType;
            object instance = returnType.IsArray ? Array.CreateInstance(returnType.GetElementType(), 5) : Activator.CreateInstance(returnType);
            context.Response.Content = new ObjectContent(returnType, instance, new JsonMediaTypeFormatter());

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => attribute.OnActionExecuted(context),
                String.Format(
                    "The action '{0}' on controller '{1}' with return type '{2}' cannot support querying. Ensure the type of the returned content is IEnumerable, IQueryable, or a generic form of either interface.",
                    actionName,
                    controllerDescriptor.ControllerName,
                    actionDescriptor.ReturnType.FullName));
        }

        [Theory]
        [InlineData("$top=1")]
        [InlineData("$skip=1")]
        public void Primitives_Can_Be_Used_For_Top_And_Skip(string filter)
        {
            // Arrange
            QueryableAttribute attribute = new QueryableAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Primitive/?" + filter);
            HttpConfiguration config = new HttpConfiguration();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            HttpControllerContext controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "Primitive", typeof(PrimitiveController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(PrimitiveController).GetMethod("GetIEnumerableOfInt"));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);
            context.Response = new HttpResponseMessage(HttpStatusCode.OK);
            HttpContent expectedResponse = new ObjectContent(typeof(IEnumerable<int>), new List<int>(), new JsonMediaTypeFormatter());
            context.Response.Content = expectedResponse;

            // Act and Assert
            attribute.OnActionExecuted(context);
            HttpResponseMessage response = context.Response;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedResponse, response.Content);
        }

        [Theory]
        [InlineData("$filter=2 eq 1")]
        [InlineData("$orderby=1")]
        public void Primitives_Cannot_Be_Used_By_Filter_And_OrderBy(string filter)
        {
            // Arrange
            QueryableAttribute attribute = new QueryableAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Primitive/?" + filter);
            HttpConfiguration config = new HttpConfiguration();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            HttpControllerContext controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "Primitive", typeof(PrimitiveController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(PrimitiveController).GetMethod("GetIEnumerableOfInt"));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);
            context.Response = new HttpResponseMessage(HttpStatusCode.OK);
            context.Response.Content = new ObjectContent(typeof(IEnumerable<int>), new List<int>(), new JsonMediaTypeFormatter());

            // Act and Assert
            HttpResponseException responseException = Assert.Throws<HttpResponseException>(() => attribute.OnActionExecuted(context));
            HttpResponseMessage response = responseException.Response;

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsAssignableFrom(typeof(ObjectContent), response.Content);
            Assert.IsType(typeof(HttpError), ((ObjectContent)response.Content).Value);
            Assert.Equal("Only $skip and $top OData query options are supported for this type.",
                         ((HttpError)((ObjectContent)response.Content).Value).Message);
        }

        [Fact]
        public void ValidateQuery_Throws_With_Null_Request()
        {
            // Arrange
            QueryableAttribute attribute = new QueryableAttribute();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => attribute.ValidateQuery(null), "request");
        }

        [Theory]
        [PropertyData("SupportedQueryNames")]
        public void ValidateQuery_Accepts_All_Supported_QueryNames(string queryName)
        {
            // Arrange
            QueryableAttribute attribute = new QueryableAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?" + queryName);

            // Act & Assert
            attribute.ValidateQuery(request);
        }

        [Theory]
        [PropertyData("UnsupportedQueryNames")]
        public void ValidateQuery_Sends_BadRequest_For_Unsupported_QueryNames(string queryName)
        {
            // Arrange
            QueryableAttribute attribute = new QueryableAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?" + queryName);

            // Act & Assert
            HttpResponseException responseException = Assert.Throws<HttpResponseException>(
                                                                () => attribute.ValidateQuery(request));

            Assert.Equal(HttpStatusCode.BadRequest, responseException.Response.StatusCode);
        }

        [Fact]
        public void ValidateQuery_Sends_BadRequest_For_Unrecognized_QueryNames()
        {
            // Arrange
            QueryableAttribute attribute = new QueryableAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?$xxx");

            // Act & Assert
            HttpResponseException responseException = Assert.Throws<HttpResponseException>(
                                                                () => attribute.ValidateQuery(request));

            Assert.Equal(HttpStatusCode.BadRequest, responseException.Response.StatusCode);
        }

        [Fact]
        public void ValidateQuery_Can_Override_Base()
        {
            // Arrange
            Mock<QueryableAttribute> mockAttribute = new Mock<QueryableAttribute>();
            mockAttribute.Setup(m => m.ValidateQuery(It.IsAny<HttpRequestMessage>())).Callback(() => { }).Verifiable();

            QueryableAttribute attribute = new QueryableAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?$xxx");

            // Act & Assert
            mockAttribute.Object.ValidateQuery(null);
            mockAttribute.Verify();
        }


        [Theory]
        [InlineData(typeof(IEnumerable), true)]
        [InlineData(typeof(IQueryable), true)]
        [InlineData(typeof(IEnumerable<Customer>), true)]
        [InlineData(typeof(IQueryable<Customer>), true)]
        [InlineData(typeof(object), false)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(List<Customer>), false)]
        [InlineData(typeof(Customer[]), false)]
        public void IsSupportedReturnType_ReturnsWhetherReturnTypeIsIEnumerableOrIQueryable(Type returnType, bool isSupported)
        {
            Assert.Equal(isSupported, QueryableAttribute.IsSupportedReturnType(returnType));
        }
    }
}
