// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Query.Controllers;
using System.Web.Http.OData.Query.Validators;
using System.Web.Http.OData.TestCommon.Models;
using System.Web.Http.Routing;
using System.Web.Http.TestCommon;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Query
{
    public class EnableQueryAttributeTest
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

        public static TheoryDataSet<string> SystemQueryOptionNames
        {
            get { return ODataQueryOptionTest.SystemQueryOptionNames; }
        }

        [Fact]
        public void Ctor_Initializes_Properties()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();

            // Act & Assert
            Assert.Equal(HandleNullPropagationOption.Default, attribute.HandleNullPropagation);
            Assert.True(attribute.EnsureStableOrdering);
        }

        [Fact]
        public void EnsureStableOrdering_Property_RoundTrips()
        {
            Assert.Reflection.BooleanProperty<EnableQueryAttribute>(
                new EnableQueryAttribute(),
                o => o.EnsureStableOrdering,
                true);
        }

        [Fact]
        public void HandleNullPropagation_Property_RoundTrips()
        {
            Assert.Reflection.EnumProperty<EnableQueryAttribute, HandleNullPropagationOption>(
                new EnableQueryAttribute(),
                o => o.HandleNullPropagation,
                HandleNullPropagationOption.Default,
                HandleNullPropagationOption.Default - 1,
                HandleNullPropagationOption.True);
        }

        [Fact]
        public void AllowedArithmeticOperators_Property_RoundTrips()
        {
            Assert.Reflection.EnumProperty<EnableQueryAttribute, AllowedArithmeticOperators>(
                new EnableQueryAttribute(),
                o => o.AllowedArithmeticOperators,
                AllowedArithmeticOperators.All,
                AllowedArithmeticOperators.None - 1,
                AllowedArithmeticOperators.Multiply);
        }

        [Fact]
        public void AllowedFunctions_Property_RoundTrips()
        {
            Assert.Reflection.EnumProperty<EnableQueryAttribute, AllowedFunctions>(
                new EnableQueryAttribute(),
                o => o.AllowedFunctions,
                AllowedFunctions.AllFunctions,
                AllowedFunctions.None - 1,
                AllowedFunctions.All);
        }

        [Fact]
        public void AllowedLogicalOperators_Property_RoundTrips()
        {
            Assert.Reflection.EnumProperty<EnableQueryAttribute, AllowedLogicalOperators>(
                new EnableQueryAttribute(),
                o => o.AllowedLogicalOperators,
                AllowedLogicalOperators.All,
                AllowedLogicalOperators.None - 1,
                AllowedLogicalOperators.GreaterThanOrEqual);
        }

        [Fact]
        public void EnableConstantParameterization_Property_RoundTrips()
        {
            Assert.Reflection.BooleanProperty(
                new EnableQueryAttribute(),
                o => o.EnableConstantParameterization,
                expectedDefaultValue: true);
        }

        [Fact]
        public void AllowedQueryOptions_Property_RoundTrips()
        {
            Assert.Reflection.EnumProperty<EnableQueryAttribute, AllowedQueryOptions>(
                new EnableQueryAttribute(),
                o => o.AllowedQueryOptions,
                AllowedQueryOptions.Supported,
                AllowedQueryOptions.None - 1,
                AllowedQueryOptions.All);
        }

        [Fact]
        public void AllowedOrderByProperties_Property_RoundTrips()
        {
            Assert.Reflection.StringProperty<EnableQueryAttribute>(
                new EnableQueryAttribute(),
                o => o.AllowedOrderByProperties,
                expectedDefaultValue: null,
                allowNullAndEmpty: true,
                treatNullAsEmpty: false);
        }

        [Fact]
        public void MaxAnyAllExpressionDepth_Property_RoundTrips()
        {
            Assert.Reflection.IntegerProperty<EnableQueryAttribute, int>(
                new EnableQueryAttribute(),
                o => o.MaxAnyAllExpressionDepth,
                expectedDefaultValue: 1,
                minLegalValue: 1,
                illegalLowerValue: -1,
                illegalUpperValue: null,
                maxLegalValue: int.MaxValue,
                roundTripTestValue: 2);
        }

        [Fact]
        public void MaxNodeCount_Property_RoundTrips()
        {
            Assert.Reflection.IntegerProperty<EnableQueryAttribute, int>(
                new EnableQueryAttribute(),
                o => o.MaxNodeCount,
                expectedDefaultValue: 100,
                minLegalValue: 1,
                maxLegalValue: int.MaxValue,
                illegalLowerValue: 0,
                illegalUpperValue: null,
                roundTripTestValue: 2);
        }

        [Fact]
        public void PageSize_Property_RoundTrips()
        {
            Assert.Reflection.IntegerProperty<EnableQueryAttribute, int>(
                new EnableQueryAttribute(),
                o => o.PageSize,
                expectedDefaultValue: 0,
                minLegalValue: 1,
                illegalLowerValue: 0,
                illegalUpperValue: null,
                maxLegalValue: int.MaxValue,
                roundTripTestValue: 2);
        }

        [Fact]
        public void MaxExpansionDepth_Property_RoundTrips()
        {
            Assert.Reflection.IntegerProperty(
                new EnableQueryAttribute(),
                o => o.MaxExpansionDepth,
                expectedDefaultValue: 2,
                minLegalValue: 0,
                illegalLowerValue: -1,
                illegalUpperValue: null,
                maxLegalValue: int.MaxValue,
                roundTripTestValue: 100);
        }

        [Fact]
        public void MaxOrderByNodeCount_Property_RoundTrips()
        {
            Assert.Reflection.IntegerProperty(
                new EnableQueryAttribute(),
                o => o.MaxOrderByNodeCount,
                expectedDefaultValue: 5,
                minLegalValue: 1,
                illegalLowerValue: -1,
                illegalUpperValue: null,
                maxLegalValue: int.MaxValue,
                roundTripTestValue: 100);
        }

        [Fact]
        public void OnActionExecuted_Throws_Null_Context()
        {
            Assert.ThrowsArgumentNull(() => new EnableQueryAttribute().OnActionExecuted(null), "actionExecutedContext");
        }

        [Fact]
        public void OnActionExecuted_Throws_Null_Request()
        {
            Assert.ThrowsArgument(
                () => new EnableQueryAttribute().OnActionExecuted(new HttpActionExecutedContext()),
                "actionExecutedContext",
                String.Format("The HttpExecutedActionContext.Request is null.{0}Parameter name: actionExecutedContext", Environment.NewLine));
        }

        [Fact]
        public void OnActionExecuted_Throws_Null_Configuration()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer/?$orderby=Name");
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerContext controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerHighLevel", typeof(CustomerHighLevelController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(CustomerHighLevelController).GetMethod("Get"));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);

            Assert.ThrowsArgument(
                () => new EnableQueryAttribute().OnActionExecuted(context),
                "actionExecutedContext",
                String.Format("Request message does not contain an HttpConfiguration object.{0}Parameter name: actionExecutedContext", Environment.NewLine));
        }

        [Theory]
        [PropertyData("DifferentReturnTypeWorksTestData")]
        public void DifferentReturnTypeWorks(string methodName, object responseObject, bool isNoOp)
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer?$orderby=Name");
            HttpConfiguration config = new HttpConfiguration();
            request.SetConfiguration(config);
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
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer/?select");
            HttpConfiguration config = new HttpConfiguration();
            request.SetConfiguration(config);
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
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer/?$custom");
            HttpConfiguration config = new HttpConfiguration();
            request.SetConfiguration(config);
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
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer/?$skip=1");
            HttpConfiguration config = new HttpConfiguration();
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            request.SetConfiguration(config);
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
                "Cannot create an EDM model as the action 'EnableQueryAttribute' on controller 'GetNonGenericEnumerable' has a return type 'CustomerHighLevel' that does not implement IEnumerable<T>.");
        }

        [Fact]
        public void NonObjectContentResponse_ThrowsArgumentException()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer?$skip=1");
            HttpConfiguration config = new HttpConfiguration();
            request.SetConfiguration(config);
            HttpControllerContext controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerHighLevel", typeof(CustomerHighLevelController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(CustomerHighLevelController).GetMethod("GetIEnumerableOfCustomer"));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);
            context.Response = new HttpResponseMessage(HttpStatusCode.OK);
            context.Response.Content = new StreamContent(new MemoryStream());

            // Act & Assert
            Assert.ThrowsArgument(
                () => attribute.OnActionExecuted(context),
                "actionExecutedContext",
                "Queries can not be applied to a response content of type 'System.Net.Http.StreamContent'. The response content must be an ObjectContent.");
        }

        [Fact]
        public void NullContentResponse_DoesNotThrow()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer?$skip=1");
            HttpConfiguration config = new HttpConfiguration();
            request.SetConfiguration(config);
            HttpControllerContext controllerContext = new HttpControllerContext(
                config,
                new HttpRouteData(new HttpRoute()),
                request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(
                new HttpConfiguration(),
                "CustomerHighLevel",
                typeof(CustomerHighLevelController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(
                controllerDescriptor,
                typeof(CustomerHighLevelController).GetMethod("GetIEnumerableOfCustomer"));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null)
            {
                Response = new HttpResponseMessage(HttpStatusCode.OK) { Content = null }
            };

            // Act & Assert
            Assert.DoesNotThrow(() => attribute.OnActionExecuted(context));
        }

        [Theory]
        [InlineData("$top=1")]
        [InlineData("$skip=1")]
        public void Primitives_Can_Be_Used_For_Top_And_Skip(string filter)
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Primitive/?" + filter);
            HttpConfiguration config = new HttpConfiguration();
            request.SetConfiguration(config);
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

        [Fact]
        public void ValidateQuery_Throws_With_Null_Request()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(System.Web.Http.OData.Builder.TestModels.Customer)), new HttpRequestMessage());

            // Act & Assert
            Assert.ThrowsArgumentNull(() => attribute.ValidateQuery(null, options), "request");
        }

        [Fact]
        public void ValidateQuery_Throws_WithNullQueryOptions()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => attribute.ValidateQuery(new HttpRequestMessage(), null), "queryOptions");
        }

        [Theory]
        [InlineData("$filter=Name eq 'abc'")]
        [InlineData("$orderby=Name")]
        [InlineData("$skip=3")]
        [InlineData("$top=2")]
        public void ValidateQuery_Accepts_All_Supported_QueryNames(string query)
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?" + query);
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(System.Web.Http.OData.Builder.TestModels.Customer)), request);

            // Act & Assert
            Assert.DoesNotThrow(() => attribute.ValidateQuery(request, options));
        }

        [Fact]
        public void ValidateQuery_Sends_BadRequest_For_Unrecognized_QueryNames()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?$xxx");
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(System.Web.Http.OData.Builder.TestModels.Customer)), request);

            // Act & Assert
            HttpResponseException responseException = Assert.Throws<HttpResponseException>(
                                                                () => attribute.ValidateQuery(request, options));

            Assert.Equal(HttpStatusCode.BadRequest, responseException.Response.StatusCode);
        }

        [Fact]
        public void ValidateQuery_Can_Override_Base()
        {
            // Arrange
            Mock<EnableQueryAttribute> mockAttribute = new Mock<EnableQueryAttribute>();
            mockAttribute.Setup(m => m.ValidateQuery(It.IsAny<HttpRequestMessage>(), It.IsAny<ODataQueryOptions>())).Callback(() => { }).Verifiable();

            // Act & Assert
            mockAttribute.Object.ValidateQuery(null, null);
            mockAttribute.Verify();
        }

        [Fact]
        public void ApplyQuery_Throws_With_Null_Queryable()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(System.Web.Http.OData.Builder.TestModels.Customer)), new HttpRequestMessage());

            // Act & Assert
            Assert.ThrowsArgumentNull(() => attribute.ApplyQuery(null, options), "queryable");
        }

        [Fact]
        public void ApplyQuery_Throws_WithNullQueryOptions()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => attribute.ApplyQuery(CustomerList.AsQueryable(), null), "queryOptions");
        }

        [Theory]
        [InlineData("$filter=Name eq 'abc'")]
        [InlineData("$orderby=Name")]
        [InlineData("$skip=3")]
        [InlineData("$top=2")]
        public void ApplyQuery_Accepts_All_Supported_QueryNames(string query)
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?" + query);
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(System.Web.Http.OData.Builder.TestModels.Customer)), request);

            // Act & Assert
            Assert.DoesNotThrow(() => attribute.ApplyQuery(new List<System.Web.Http.OData.Builder.TestModels.Customer>().AsQueryable(), options));
        }

        [Fact]
        public void ApplyQuery_Can_Override_Base()
        {
            // Arrange
            Mock<EnableQueryAttribute> mockAttribute = new Mock<EnableQueryAttribute>();
            IQueryable result = CustomerList.AsQueryable();
            mockAttribute.Setup(m => m.ApplyQuery(It.IsAny<IQueryable>(), It.IsAny<ODataQueryOptions>()))
                         .Returns(result);
            mockAttribute.CallBase = false;

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?$top=2");
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(System.Web.Http.OData.Builder.TestModels.Customer)), request);

            // Act & Assert
            Assert.Same(result, mockAttribute.Object.ApplyQuery(result, options));
        }

        [Theory]
        [InlineData("Id,Address")]
        [InlineData("   Id,Address  ")]
        [InlineData(" Id , Address ")]
        [InlineData("Id, Address")]
        public void OrderByDisllowedPropertiesWithSpaces(string allowedProperties)
        {
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            attribute.AllowedOrderByProperties = allowedProperties;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customers/?$orderby=Id,Name");
            ODataQueryOptions queryOptions = new ODataQueryOptions(ValidationTestHelper.CreateCustomerContext(), request);

            Assert.Throws<ODataException>(() => attribute.ValidateQuery(request, queryOptions),
                "Order by 'Name' is not allowed. To allow it, set the 'AllowedOrderByProperties' property on EnableQueryAttribute or QueryValidationSettings.");
        }

        [Theory]
        [InlineData("Id,Name")]
        [InlineData("   Id,Name  ")]
        [InlineData(" Id , Name ")]
        [InlineData("Id, Name")]
        [InlineData("")]
        [InlineData(null)]
        public void OrderByAllowedPropertiesWithSpaces(string allowedProperties)
        {
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            attribute.AllowedOrderByProperties = allowedProperties;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customers/?$orderby=Id,Name");
            ODataQueryOptions queryOptions = new ODataQueryOptions(ValidationTestHelper.CreateCustomerContext(), request);

            Assert.DoesNotThrow(() => attribute.ValidateQuery(request, queryOptions));
        }

        [Fact]
        public void GetModel_ReturnsModel_ForNoModelOnRequest()
        {
            var entityClrType = typeof(QueryCompositionCustomer);
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            var descriptor = new ReflectedHttpActionDescriptor();
            descriptor.Configuration = config;

            var queryModel = new EnableQueryAttribute().GetModel(entityClrType, request, descriptor);

            Assert.NotNull(queryModel);
            Assert.Same(descriptor.Properties["MS_EdmModelSystem.Web.Http.OData.Query.QueryCompositionCustomer"], queryModel);
        }

        [Fact]
        public void CreateQueryContext_ReturnsQueryContext_ForNonMatchingModelOnRequest()
        {
            var builder = new ODataConventionModelBuilder();
            var model = builder.GetEdmModel();
            var entityClrType = typeof(QueryCompositionCustomer);
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            request.ODataProperties().Model = model;
            var descriptor = new ReflectedHttpActionDescriptor();
            descriptor.Configuration = config;

            var queryModel = new EnableQueryAttribute().GetModel(entityClrType, request, descriptor);

            Assert.NotNull(queryModel);
            Assert.Same(descriptor.Properties["MS_EdmModelSystem.Web.Http.OData.Query.QueryCompositionCustomer"], queryModel);
        }


        [Fact]
        public void CreateQueryContext_ReturnsQueryContext_ForMatchingModelOnRequest()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<QueryCompositionCustomer>("customers");
            var model = builder.GetEdmModel();
            var entityClrType = typeof(QueryCompositionCustomer);
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            request.ODataProperties().Model = model;
            var descriptor = new ReflectedHttpActionDescriptor();
            descriptor.Configuration = config;

            var queryModel = new EnableQueryAttribute().GetModel(entityClrType, request, descriptor);

            Assert.NotNull(queryModel);
            Assert.Same(model, queryModel);
            Assert.False(descriptor.Properties.ContainsKey("MS_EdmModelSystem.Web.Http.OData.Query.QueryCompositionCustomer"));
        }

        [Fact]
        public void QueryableOnActionUnknownOperatorIsAllowed()
        {
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext(
                "http://localhost:8080/?$orderby=$it desc&unknown=12",
                Enumerable.Range(0, 5).AsQueryable());

            // unsupported operator - ignored
            attribute.OnActionExecuted(actionExecutedContext);

            List<int> result = actionExecutedContext.Response.Content.ReadAsAsync<List<int>>().Result;
            Assert.Equal(new[] { 4, 3, 2, 1, 0 }, result);
        }

        [Fact]
        public void QueryableOnActionUnknownOperatorStartingDollarSignThrows()
        {
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext(
                "http://localhost:8080/QueryCompositionCustomer?$orderby=Name desc&$unknown=12",
                QueryCompositionCustomerController.CustomerList.AsQueryable());

            var exception = Assert.Throws<HttpResponseException>(() => attribute.OnActionExecuted(actionExecutedContext));

            // EnableQueryAttribute will validate and throws
            Assert.Equal(HttpStatusCode.BadRequest, exception.Response.StatusCode);
        }

        [Fact]
        public virtual void QueryableUsesConfiguredAssembliesResolver_For_MappingDerivedTypes()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext(
                "http://localhost:8080/QueryCompositionCustomer/?$filter=Id eq 2",
                QueryCompositionCustomerController.CustomerList.AsQueryable());

            ODataModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<QueryCompositionCustomer>(typeof(QueryCompositionCustomer).Name);
            IEdmModel model = modelBuilder.GetEdmModel();
            model.SetAnnotationValue<ClrTypeAnnotation>(model.FindType("System.Web.Http.OData.Query.QueryCompositionCustomer"), null);

            bool called = false;
            Mock<IAssembliesResolver> assembliesResolver = new Mock<IAssembliesResolver>();
            assembliesResolver
                .Setup(r => r.GetAssemblies())
                .Returns(new DefaultAssembliesResolver().GetAssemblies())
                .Callback(() => { called = true; })
                .Verifiable();
            actionExecutedContext.Request.GetConfiguration().Services.Replace(typeof(IAssembliesResolver), assembliesResolver.Object);

            // Act
            attribute.OnActionExecuted(actionExecutedContext);

            // Assert
            Assert.True(called);
        }

        [Fact]
        public void ApplyQuery_SingleEntity_ThrowsArgumentNull_Entity()
        {
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            ODataQueryOptions options = new ODataQueryOptions(new ODataQueryContext(EdmCoreModel.Instance, typeof(int)), new HttpRequestMessage());

            Assert.ThrowsArgumentNull(
                () => attribute.ApplyQuery(entity: null, queryOptions: options),
                "entity");
        }

        [Fact]
        public void ApplyQuery_SingleEntity_ThrowsArgumentNull_QueryOptions()
        {
            EnableQueryAttribute attribute = new EnableQueryAttribute();

            Assert.ThrowsArgumentNull(
                () => attribute.ApplyQuery(entity: 42, queryOptions: null),
                "queryOptions");
        }

        [Fact]
        public void ApplyQuery_CallsApplyOnODataQueryOptions()
        {
            object entity = new object();
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            HttpRequestMessage request = new HttpRequestMessage();
            Mock<ODataQueryOptions> queryOptions = new Mock<ODataQueryOptions>(context, request);

            attribute.ApplyQuery(entity, queryOptions.Object);

            queryOptions.Verify(q => q.ApplyTo(entity, It.IsAny<ODataQuerySettings>()), Times.Once());
        }

        public static TheoryDataSet<object, Type> GetElementTypeTestData
        {
            get
            {
                return new TheoryDataSet<object, Type>
                {
                    { Enumerable.Empty<int>(), typeof(int) },
                    { new List<int>(), typeof(int) },
                    { new int[0], typeof(int) },
                    { Enumerable.Empty<string>().AsQueryable(), typeof(string) },
                    { new SingleResult<string>(Enumerable.Empty<string>().AsQueryable()), typeof(string) },
                    { new Customer(), typeof(Customer) }
                };
            }
        }

        [Theory]
        [PropertyData("GetElementTypeTestData")]
        public void GetElementType_Returns_ExpectedElementType(object response, Type expectedElementType)
        {
            HttpActionDescriptor actionDescriptor = new Mock<HttpActionDescriptor>().Object;
            Assert.Equal(expectedElementType, EnableQueryAttribute.GetElementType(response, actionDescriptor));
        }

        [Fact]
        public void SingleOrDefault_IQueryableOfT_OneElementInSequence_ReturnsElement()
        {
            Customer customer = new Customer();
            IQueryable<Customer> queryable = new[] { customer }.AsQueryable();
            HttpActionDescriptor actionDescriptor = new Mock<HttpActionDescriptor>().Object;

            var result = EnableQueryAttribute.SingleOrDefault(queryable, actionDescriptor);

            Assert.Same(customer, result);
        }

        [Fact]
        public void SingleOrDefault_IQueryableOfT_ZeroElementsInSequence_ReturnsNull()
        {
            IQueryable<Customer> queryable = Enumerable.Empty<Customer>().AsQueryable();
            HttpActionDescriptor actionDescriptor = new Mock<HttpActionDescriptor>().Object;

            var result = EnableQueryAttribute.SingleOrDefault(queryable, actionDescriptor);

            Assert.Null(result);
        }

        [Fact]
        public void SingleOrDefault_IQueryableOfT_MoreThaneOneElementInSequence_Throws()
        {
            IQueryable<Customer> queryable = new[] { new Customer(), new Customer() }.AsQueryable();
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor
            {
                Configuration = new HttpConfiguration(),
                MethodInfo = GetType().GetMethod("SomeAction", BindingFlags.Instance | BindingFlags.NonPublic),
                ControllerDescriptor = new HttpControllerDescriptor { ControllerName = "SomeName" }
            };

            Assert.Throws<InvalidOperationException>(
                () => EnableQueryAttribute.SingleOrDefault(queryable, actionDescriptor),
                "The action 'SomeAction' on controller 'SomeName' returned a SingleResult containing more than one element. " +
                "SingleResult must have zero or one elements.");
        }

        [Fact]
        public void SingleOrDefault_DisposeCalled_EmptySequence()
        {
            // Arrange
            var enumerator = new Mock<IEnumerator>(MockBehavior.Strict);
            enumerator.Setup(mock => mock.MoveNext()).Returns(false);

            var disposable = enumerator.As<IDisposable>();
            disposable.Setup(mock => mock.Dispose()).Verifiable();

            var queryable = new Mock<IQueryable>(MockBehavior.Strict);
            queryable.Setup(mock => mock.GetEnumerator()).Returns(enumerator.Object);

            var actionDescriptor = new ReflectedHttpActionDescriptor
            {
                Configuration = new HttpConfiguration(),
                MethodInfo = GetType().GetMethod("SomeAction", BindingFlags.Instance | BindingFlags.NonPublic),
                ControllerDescriptor = new HttpControllerDescriptor { ControllerName = "SomeName" }
            };

            // Act
            EnableQueryAttribute.SingleOrDefault(queryable.Object, actionDescriptor);

            // Assert
            disposable.Verify();
        }

        [Fact]
        public void SingleOrDefault_DisposeCalled_OneElementInSequence()
        {
            // Arrange
            var enumerator = new Mock<IEnumerator>(MockBehavior.Strict);
            enumerator.SetupSequence(mock => mock.MoveNext()).Returns(true).Returns(false);
            enumerator.SetupGet(mock => mock.Current).Returns(new Customer());

            var disposable = enumerator.As<IDisposable>();
            disposable.Setup(mock => mock.Dispose()).Verifiable();

            var queryable = new Mock<IQueryable>(MockBehavior.Strict);
            queryable.Setup(mock => mock.GetEnumerator()).Returns(enumerator.Object);

            var actionDescriptor = new ReflectedHttpActionDescriptor
            {
                Configuration = new HttpConfiguration(),
                MethodInfo = GetType().GetMethod("SomeAction", BindingFlags.Instance | BindingFlags.NonPublic),
                ControllerDescriptor = new HttpControllerDescriptor { ControllerName = "SomeName" }
            };

            // Act
            EnableQueryAttribute.SingleOrDefault(queryable.Object, actionDescriptor);

            // Assert
            disposable.Verify();
        }

        public void SingleOrDefault_DisposeCalled_MultipleElementsInSequence()
        {
            // Arrange
            var enumerator = new Mock<IEnumerator>(MockBehavior.Strict);
            enumerator.Setup(mock => mock.MoveNext()).Returns(true);
            enumerator.SetupGet(mock => mock.Current).Returns(new Customer());

            var disposable = enumerator.As<IDisposable>();
            disposable.Setup(mock => mock.Dispose()).Verifiable();

            var queryable = new Mock<IQueryable>(MockBehavior.Strict);
            queryable.Setup(mock => mock.GetEnumerator()).Returns(enumerator.Object);

            var actionDescriptor = new ReflectedHttpActionDescriptor
            {
                Configuration = new HttpConfiguration(),
                MethodInfo = GetType().GetMethod("SomeAction", BindingFlags.Instance | BindingFlags.NonPublic),
                ControllerDescriptor = new HttpControllerDescriptor { ControllerName = "SomeName" }
            };

            // Act (will throw)
            try
            {
                EnableQueryAttribute.SingleOrDefault(queryable.Object, actionDescriptor);
            }
            catch
            {
                // Other tests confirm the Exception.
            }

            // Assert
            disposable.Verify();
        }

        [Fact]
        public void OnActionExecuted_SingleResult_ReturnsSingleItemEvenIfThereIsNoSelectExpand()
        {
            Customer customer = new Customer();
            SingleResult singleResult = new SingleResult<Customer>(new Customer[] { customer }.AsQueryable());
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext("http://localhost/", singleResult);
            EnableQueryAttribute attribute = new EnableQueryAttribute();

            attribute.OnActionExecuted(actionExecutedContext);

            Assert.Equal(HttpStatusCode.OK, actionExecutedContext.Response.StatusCode);
            Assert.Equal(customer, (actionExecutedContext.Response.Content as ObjectContent).Value);
        }

        [Fact]
        public void OnActionExecuted_SingleResult_Returns400_IfQueryContainsNonSelectExpand()
        {
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext("http://localhost/?$top=10", new Customer());
            EnableQueryAttribute attribute = new EnableQueryAttribute();

            attribute.OnActionExecuted(actionExecutedContext);

            Assert.Equal(HttpStatusCode.BadRequest, actionExecutedContext.Response.StatusCode);
        }

        [Fact]
        public void OnActionExecuted_SingleResult_WithEmptyQueryResult_SetsNotFoundResponse()
        {
            // Arrange
            var customers = Enumerable.Empty<Customer>().AsQueryable();
            SingleResult result = SingleResult.Create(customers);
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext("http://localhost/", result);
            EnableQueryAttribute attribute = new EnableQueryAttribute();

            // Act
            attribute.OnActionExecuted(actionExecutedContext);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, actionExecutedContext.Response.StatusCode);
        }

        [Fact]
        public void OnActionExecuted_SingleResult_WithMoreThanASingleQueryResult_ThrowsInvalidOperationException()
        {
            // Arrange
            var customers = CustomerList.AsQueryable();
            SingleResult result = SingleResult.Create(customers);
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext("http://localhost/", result);
            EnableQueryAttribute attribute = new EnableQueryAttribute();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => attribute.OnActionExecuted(actionExecutedContext));
        }

        [Theory]
        [InlineData("$filter=ID eq 1")]
        [InlineData("$orderby=ID")]
        [InlineData("$inlinecount=allpages")]
        [InlineData("$skip=1")]
        [InlineData("$top=0")]
        public void ValidateSelectExpandOnly_ThrowsODataException_IfODataQueryOptionsHasNonSelectExpand(string parameter)
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost?" + parameter);
            ODataQueryContext context = new ODataQueryContext(model.Model, typeof(Customer));
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

            Assert.Throws<ODataException>(
                () => EnableQueryAttribute.ValidateSelectExpandOnly(queryOptions),
                "The requested resource is not a collection. Query options $filter, $orderby, $inlinecount, $skip, and $top can be applied only on collections.");
        }

        private void SomeAction()
        {
        }

        private static HttpActionExecutedContext GetActionExecutedContext<TResponse>(string uri, TResponse result)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var actionContext = ContextUtil.CreateActionContext(ContextUtil.CreateControllerContext(request: request));
            var response = request.CreateResponse<TResponse>(HttpStatusCode.OK, result);
            var actionExecutedContext = new HttpActionExecutedContext { ActionContext = actionContext, Response = response };
            actionContext.ActionDescriptor.Configuration = request.GetConfiguration();
            return actionExecutedContext;
        }
    }
}
