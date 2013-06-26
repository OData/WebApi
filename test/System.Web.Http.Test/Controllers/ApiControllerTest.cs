// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using System.Web.Http.Routing;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class ApiControllerTest
    {
        private readonly HttpActionContext _actionContextInstance = ContextUtil.CreateActionContext();
        private readonly HttpConfiguration _configurationInstance = new HttpConfiguration();
        private readonly HttpActionDescriptor _actionDescriptorInstance = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

        [Fact]
        public void Setting_CustomActionInvoker()
        {
            // Arrange
            ApiController api = new UsersController();
            string responseText = "Hello World";
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext();

            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersController));
            controllerContext.ControllerDescriptor = controllerDescriptor;

            Mock<IHttpActionInvoker> mockInvoker = new Mock<IHttpActionInvoker>();
            mockInvoker
                .Setup(invoker => invoker.InvokeActionAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
                    tcs.TrySetResult(new HttpResponseMessage() { Content = new StringContent(responseText) });
                    return tcs.Task;
                });
            controllerDescriptor.Configuration.Services.Replace(typeof(IHttpActionInvoker), mockInvoker.Object);

            // Act
            HttpResponseMessage message = api.ExecuteAsync(
                controllerContext,
                CancellationToken.None).Result;

            // Assert
            Assert.Equal(responseText, message.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Setting_CustomActionSelector()
        {
            // Arrange
            ApiController api = new UsersController();
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext();

            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersController));
            controllerContext.ControllerDescriptor = controllerDescriptor;

            Mock<IHttpActionSelector> mockSelector = new Mock<IHttpActionSelector>();
            mockSelector
                .Setup(invoker => invoker.SelectAction(It.IsAny<HttpControllerContext>()))
                .Returns(() =>
                {
                    Func<HttpResponseMessage> testDelegate =
                        () => new HttpResponseMessage { Content = new StringContent("This is a test") };
                    return new ReflectedHttpActionDescriptor
                    {
                        Configuration = controllerContext.Configuration,
                        ControllerDescriptor = controllerDescriptor,
                        MethodInfo = testDelegate.Method
                    };
                });
            controllerDescriptor.Configuration.Services.Replace(typeof(IHttpActionSelector), mockSelector.Object);

            // Act
            HttpResponseMessage message = api.ExecuteAsync(
                controllerContext,
                CancellationToken.None).Result;

            // Assert
            Assert.Equal("This is a test", message.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Default_Get()
        {
            // Arrange
            ApiController api = new UsersController();
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, request: new HttpRequestMessage() { Method = HttpMethod.Get });
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersController));

            // Act
            HttpResponseMessage message = api.ExecuteAsync(
                controllerContext,
                CancellationToken.None).Result;

            // Assert
            Assert.Equal("Default User", message.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Default_Post()
        {
            // Arrange
            ApiController api = new UsersController();
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, request: new HttpRequestMessage() { Method = HttpMethod.Post });
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersController));

            // Act
            HttpResponseMessage message = api.ExecuteAsync(
                controllerContext,
                CancellationToken.None).Result;

            // Assert
            Assert.Equal("User Posted", message.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Default_Put()
        {
            // Arrange
            ApiController api = new UsersController();
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, request: new HttpRequestMessage() { Method = HttpMethod.Put });
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersController));

            // Act
            HttpResponseMessage message = api.ExecuteAsync(
                controllerContext,
                CancellationToken.None).Result;

            // Assert
            Assert.Equal("User Updated", message.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Default_Delete()
        {
            // Arrange
            ApiController api = new UsersController();
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, request: new HttpRequestMessage() { Method = HttpMethod.Delete });
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersController));

            // Act
            HttpResponseMessage message = api.ExecuteAsync(
                controllerContext,
                CancellationToken.None).Result;

            // Assert
            Assert.Equal("User Deleted", message.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Route_ActionName()
        {
            // Arrange
            ApiController api = new UsersRpcController();
            HttpRouteData route = new HttpRouteData(new HttpRoute());
            route.Values.Add("action", "Admin");
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, routeData: route, request: new HttpRequestMessage() { Method = HttpMethod.Post });
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersRpcController));

            // Act
            HttpResponseMessage message = api.ExecuteAsync(controllerContext, CancellationToken.None).Result;
            User user = message.Content.ReadAsAsync<User>().Result;

            // Assert
            Assert.Equal("Yao", user.FirstName);
            Assert.Equal("Huang", user.LastName);
        }

        [Fact]
        public void Route_Get_Action_With_Route_Parameters()
        {
            // Arrange
            ApiController api = new UsersRpcController();
            HttpRouteData route = new HttpRouteData(new HttpRoute());
            route.Values.Add("action", "EchoUser");
            route.Values.Add("firstName", "RouteFirstName");
            route.Values.Add("lastName", "RouteLastName");
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, routeData: route, request: new HttpRequestMessage() { Method = HttpMethod.Post });
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersRpcController));

            // Act
            HttpResponseMessage message = api.ExecuteAsync(controllerContext, CancellationToken.None).Result;
            User user = message.Content.ReadAsAsync<User>().Result;

            // Assert
            Assert.Equal("RouteFirstName", user.FirstName);
            Assert.Equal("RouteLastName", user.LastName);
        }

        [Fact]
        public void Route_Get_Action_With_Query_Parameters()
        {
            // Arrange
            ApiController api = new UsersRpcController();
            HttpRouteData route = new HttpRouteData(new HttpRoute());
            route.Values.Add("action", "EchoUser");

            Uri requestUri = new Uri("http://localhost/?firstName=QueryFirstName&lastName=QueryLastName");
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, routeData: route, request: new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = requestUri
                });
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersRpcController));

            // Act
            HttpResponseMessage message = api.ExecuteAsync(controllerContext, CancellationToken.None).Result;
            User user = message.Content.ReadAsAsync<User>().Result;

            // Assert
            Assert.Equal("QueryFirstName", user.FirstName);
            Assert.Equal("QueryLastName", user.LastName);
        }

        [Fact]
        public void Route_Post_Action_With_Content_Parameter()
        {
            // Arrange
            ApiController api = new UsersRpcController();
            HttpRouteData route = new HttpRouteData(new HttpRoute());
            route.Values.Add("action", "EchoUserObject");
            User postedUser = new User()
            {
                FirstName = "SampleFirstName",
                LastName = "SampleLastName"
            };

            HttpRequestMessage request = new HttpRequestMessage() { Method = HttpMethod.Post };

            // Create a serialized request because this test directly calls the controller
            // which would have normally been working with a serialized request content.
            string serializedUserAsString = null;
            using (HttpRequestMessage tempRequest = new HttpRequestMessage() { Content = new ObjectContent<User>(postedUser, new XmlMediaTypeFormatter()) })
            {
                serializedUserAsString = tempRequest.Content.ReadAsStringAsync().Result;
            }

            StringContent stringContent = new StringContent(serializedUserAsString);
            stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            request.Content = stringContent;

            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, routeData: route, request: request);
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersRpcController));

            // Act
            HttpResponseMessage message = api.ExecuteAsync(
                controllerContext,
                CancellationToken.None).Result;
            User user = message.Content.ReadAsAsync<User>().Result;

            // Assert
            Assert.Equal(postedUser.FirstName, user.FirstName);
            Assert.Equal(postedUser.LastName, user.LastName);
        }

        [Fact]
        public void Invalid_Action_In_Route()
        {
            // Arrange
            ApiController api = new UsersController();
            HttpRouteData route = new HttpRouteData(new HttpRoute());
            string actionName = "invalidOp";
            route.Values.Add("action", actionName);
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, routeData: route, request: new HttpRequestMessage() { Method = HttpMethod.Get });
            Type controllerType = typeof(UsersController);
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, controllerType.Name, controllerType);
            controllerContext.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            // Act & Assert
            var exception = Assert.Throws<HttpResponseException>(() =>
             {
                 HttpResponseMessage message = api.ExecuteAsync(controllerContext, CancellationToken.None).Result;
             });

            Assert.Equal(HttpStatusCode.NotFound, exception.Response.StatusCode);
            var content = Assert.IsType<ObjectContent<HttpError>>(exception.Response.Content);
            Assert.Equal("No action was found on the controller 'UsersController' that matches the name 'invalidOp'.",
                ((HttpError)content.Value)["MessageDetail"]);
        }

        [Fact]
        public void ExecuteAsync_InvokesAuthenticationFilters_ThenInvokesAuthorizationFilters_ThenInvokesModelBinding_ThenInvokesActionFilters_ThenInvokesAction()
        {
            List<string> log = new List<string>();
            Mock<ApiController> controllerMock = new Mock<ApiController>() { CallBase = true };
            var controllerContextMock = new Mock<HttpControllerContext>();

            Mock<IActionValueBinder> binderMock = new Mock<IActionValueBinder>();
            Mock<HttpActionBinding> actionBindingMock = new Mock<HttpActionBinding>();
            actionBindingMock.Setup(b => b.ExecuteBindingAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>())).Returns(() => Task.Factory.StartNew(() => { log.Add("model binding"); }));
            binderMock.Setup(b => b.GetBinding(It.IsAny<HttpActionDescriptor>())).Returns(actionBindingMock.Object);
            HttpConfiguration configuration = new HttpConfiguration();

            HttpControllerContext controllerContext = controllerContextMock.Object;
            controllerContext.Configuration = configuration;
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(configuration, "test", typeof(object));
            var actionFilterMock = CreateActionFilterMock((ac, ct, cont) =>
            {
                log.Add("action filters");
                return cont();
            });
            var authorizationFilterMock = CreateAuthorizationFilterMock((ac, ct, cont) =>
            {
                log.Add("authZ filters");
                return cont();
            });
            Mock<IAuthenticationFilter> authenticationFilterMock = new Mock<IAuthenticationFilter>();
            authenticationFilterMock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(),
                It.IsAny<CancellationToken>())).Callback(() =>
                    {
                        log.Add("authN filters authenticate");
                    }).Returns(() => Task.FromResult<object>(null));
            IHttpActionResult innerResult = null;
            Mock<IHttpActionResult> challengeResultMock = new Mock<IHttpActionResult>();
            challengeResultMock.Setup(r => r.ExecuteAsync(It.IsAny<CancellationToken>())).Returns(async () =>
            {
                HttpResponseMessage response = await innerResult.ExecuteAsync(CancellationToken.None);
                log.Add("authN filters challenge");
                return response;
            });
            authenticationFilterMock.Setup(f => f.ChallengeAsync(It.IsAny<HttpAuthenticationChallengeContext>(),
                It.IsAny<CancellationToken>()))
                .Callback<HttpAuthenticationChallengeContext, CancellationToken>((c, t) => {
                    innerResult = c.Result;
                    c.Result = challengeResultMock.Object;})
                .Returns(() => Task.FromResult<object>(null));

            var selectorMock = new Mock<IHttpActionSelector>();

            Mock<HttpActionDescriptor> actionDescriptorMock = new Mock<HttpActionDescriptor>();
            actionDescriptorMock.Setup(ad => ad.ActionBinding).Returns(actionBindingMock.Object);
            actionDescriptorMock.Setup(ad => ad.GetFilterPipeline())
                .Returns(new Collection<FilterInfo>(new List<FilterInfo>() { new FilterInfo(actionFilterMock.Object, FilterScope.Action), new FilterInfo(authorizationFilterMock.Object, FilterScope.Action), new FilterInfo(authenticationFilterMock.Object, FilterScope.Action) }));

            selectorMock.Setup(s => s.SelectAction(controllerContext)).Returns(actionDescriptorMock.Object);

            ApiController controller = controllerMock.Object;
            var invokerMock = new Mock<IHttpActionInvoker>();
            invokerMock.Setup(i => i.InvokeActionAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>()))
                       .Returns(() => Task.Factory.StartNew(() =>
                       {
                           log.Add("action");
                           return new HttpResponseMessage();
                       }));
            controllerContext.Configuration.Services.Replace(typeof(IHttpActionInvoker), invokerMock.Object);
            controllerContext.Configuration.Services.Replace(typeof(IHttpActionSelector), selectorMock.Object);
            controllerContext.Configuration.Services.Replace(typeof(IActionValueBinder), binderMock.Object);

            var task = controller.ExecuteAsync(controllerContext, CancellationToken.None);

            Assert.NotNull(task);
            task.WaitUntilCompleted();
            Assert.Equal(new string[] { "authN filters authenticate", "authZ filters", "model binding", "action filters", "action", "authN filters challenge" }, log.ToArray());
        }

        [Fact]
        public void GetFilters_QueriesFilterProvidersFromServices()
        {
            // Arrange
            Mock<DefaultServices> servicesMock = new Mock<DefaultServices> { CallBase = true };
            Mock<IFilterProvider> filterProviderMock = new Mock<IFilterProvider>();
            servicesMock.Setup(r => r.GetServices(typeof(IFilterProvider))).Returns(new object[] { filterProviderMock.Object }).Verifiable();
            _configurationInstance.Services = servicesMock.Object;

            HttpActionDescriptor actionDescriptorMock = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            actionDescriptorMock.Configuration = _configurationInstance;

            // Act
            actionDescriptorMock.GetFilterPipeline();

            // Assert
            servicesMock.Verify();
        }

        [Fact]
        public void GetFilters_UsesFilterProvidersToGetFilters()
        {
            // Arrange
            Mock<DefaultServices> servicesMock = new Mock<DefaultServices> { CallBase = true };
            Mock<IFilterProvider> filterProviderMock = new Mock<IFilterProvider>();
            servicesMock.Setup(r => r.GetServices(typeof(IFilterProvider))).Returns(new[] { filterProviderMock.Object });
            _configurationInstance.Services = servicesMock.Object;

            HttpActionDescriptor actionDescriptorMock = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            actionDescriptorMock.Configuration = _configurationInstance;

            // Act
            actionDescriptorMock.GetFilterPipeline().ToList();

            // Assert
            filterProviderMock.Verify(fp => fp.GetFilters(_configurationInstance, actionDescriptorMock));
        }

        [Fact]
        public void RequestPropertyGetterSetterWorks()
        {
            Assert.Reflection.Property(new Mock<ApiController>().Object,
                c => c.Request, expectedDefaultValue: null, allowNull: false,
                roundTripTestValue: new HttpRequestMessage());
        }

        [Fact]
        public void ConfigurationPropertyGetterSetterWorks()
        {
            Assert.Reflection.Property(new Mock<ApiController>().Object,
                c => c.Configuration, expectedDefaultValue: null, allowNull: false,
                roundTripTestValue: new HttpConfiguration());
        }

        [Fact]
        public void ModelStatePropertyGetterWorks()
        {
            // Arrange
            ApiController controller = new Mock<ApiController>().Object;

            // Act
            ModelStateDictionary expected = new ModelStateDictionary();
            expected.Add("a", new ModelState() { Value = new ValueProviders.ValueProviderResult("result", "attempted", CultureInfo.InvariantCulture) });

            controller.ModelState.Add("a", new ModelState() { Value = new ValueProviders.ValueProviderResult("result", "attempted", CultureInfo.InvariantCulture) });

            // Assert
            Assert.Equal(expected.Count, controller.ModelState.Count);
        }

        // TODO: Move these tests to ActionDescriptorTest
        [Fact]
        public void GetFilters_OrdersFilters()
        {
            // Arrange
            HttpActionDescriptor actionDescriptorMock = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            actionDescriptorMock.Configuration = _configurationInstance;

            var globalFilter = new FilterInfo(new TestMultiFilter(), FilterScope.Global);
            var actionFilter = new FilterInfo(new TestMultiFilter(), FilterScope.Action);
            var controllerFilter = new FilterInfo(new TestMultiFilter(), FilterScope.Controller);
            Mock<DefaultServices> servicesMock = BuildFilterProvidingServicesMock(_configurationInstance, actionDescriptorMock, globalFilter, actionFilter, controllerFilter);
            _configurationInstance.Services = servicesMock.Object;

            // Act
            var result = actionDescriptorMock.GetFilterPipeline().ToArray();

            // Assert
            Assert.Equal(new[] { globalFilter, controllerFilter, actionFilter }, result);
        }

        [Fact]
        public void GetFilters_RemovesDuplicateUniqueFiltersKeepingMostSpecificScope()
        {
            // Arrange
            HttpActionDescriptor actionDescriptorMock = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            actionDescriptorMock.Configuration = _configurationInstance;

            var multiActionFilter = new FilterInfo(new TestMultiFilter(), FilterScope.Action);
            var multiGlobalFilter = new FilterInfo(new TestMultiFilter(), FilterScope.Global);
            var uniqueControllerFilter = new FilterInfo(new TestUniqueFilter(), FilterScope.Controller);
            var uniqueActionFilter = new FilterInfo(new TestUniqueFilter(), FilterScope.Action);
            Mock<DefaultServices> servicesMock = BuildFilterProvidingServicesMock(
                _configurationInstance, actionDescriptorMock,
                multiActionFilter, multiGlobalFilter, uniqueControllerFilter, uniqueActionFilter);
            _configurationInstance.Services = servicesMock.Object;

            // Act
            var result = actionDescriptorMock.GetFilterPipeline().ToArray();

            // Assert
            Assert.Equal(new[] { multiGlobalFilter, multiActionFilter, uniqueActionFilter }, result);
        }

        [Fact]
        public void InvokeActionWithExceptionFilters_IfActionTaskIsSuccessful_ReturnsSuccessTask()
        {
            // Arrange
            List<string> log = new List<string>();
            var response = new HttpResponseMessage();
            var actionResult = CreateStubActionResult(TaskHelpers.FromResult(response));
            var exceptionFilterMock = CreateExceptionFilterMock((ec, ct) =>
            {
                log.Add("exceptionFilter");
                return Task.Factory.StartNew(() => { });
            });
            var filters = new IExceptionFilter[] { exceptionFilterMock.Object };

            // Act
            var result = ApiController.InvokeActionWithExceptionFilters(actionResult, _actionContextInstance, CancellationToken.None, filters);

            // Assert
            Assert.NotNull(result);
            result.WaitUntilCompleted();
            Assert.Equal(TaskStatus.RanToCompletion, result.Status);
            Assert.Same(response, result.Result);
            Assert.Equal(new string[] { }, log.ToArray());
        }

        [Fact]
        public void InvokeActionWithExceptionFilters_IfActionTaskIsCanceled_ReturnsCanceledTask()
        {
            // Arrange
            List<string> log = new List<string>();
            var actionResult = CreateStubActionResult(TaskHelpers.Canceled<HttpResponseMessage>());
            var exceptionFilterMock = CreateExceptionFilterMock((ec, ct) =>
            {
                log.Add("exceptionFilter");
                return Task.Factory.StartNew(() => { });
            });
            var filters = new IExceptionFilter[] { exceptionFilterMock.Object };

            // Act
            var result = ApiController.InvokeActionWithExceptionFilters(actionResult, _actionContextInstance, CancellationToken.None, filters);

            // Assert
            Assert.NotNull(result);
            result.WaitUntilCompleted();
            Assert.Equal(TaskStatus.Canceled, result.Status);
            Assert.Equal(new string[] { "exceptionFilter" }, log.ToArray());
        }

        [Fact]
        public void InvokeActionWithExceptionFilters_IfActionTaskIsFaulted_ExecutesFiltersAndReturnsFaultedTaskIfNotHandled()
        {
            // Arrange
            List<string> log = new List<string>();
            var exception = new Exception();
            var actionResult = CreateStubActionResult(TaskHelpers.FromError<HttpResponseMessage>(exception));
            Exception exceptionSeenByFilter = null;
            var exceptionFilterMock = CreateExceptionFilterMock((ec, ct) =>
            {
                exceptionSeenByFilter = ec.Exception;
                log.Add("exceptionFilter");
                return Task.Factory.StartNew(() => { });
            });
            var filters = new IExceptionFilter[] { exceptionFilterMock.Object };

            // Act
            var result = ApiController.InvokeActionWithExceptionFilters(actionResult, _actionContextInstance, CancellationToken.None, filters);

            // Assert
            Assert.NotNull(result);
            result.WaitUntilCompleted();
            Assert.Equal(TaskStatus.Faulted, result.Status);
            Assert.Same(exception, result.Exception.InnerException);
            Assert.Same(exception, exceptionSeenByFilter);
            Assert.Equal(new string[] { "exceptionFilter" }, log.ToArray());
        }

        [Fact]
        public void InvokeActionWithExceptionFilters_IfActionTaskIsFaulted_ExecutesFiltersAndReturnsResultIfHandled()
        {
            // Arrange
            List<string> log = new List<string>();
            var exception = new Exception();
            var actionResult = CreateStubActionResult(TaskHelpers.FromError<HttpResponseMessage>(exception));
            HttpResponseMessage globalFilterResponse = new HttpResponseMessage();
            HttpResponseMessage actionFilterResponse = new HttpResponseMessage();
            HttpResponseMessage resultSeenByGlobalFilter = null;
            var globalFilterMock = CreateExceptionFilterMock((ec, ct) =>
            {
                log.Add("globalFilter");
                resultSeenByGlobalFilter = ec.Response;
                ec.Response = globalFilterResponse;
                return Task.Factory.StartNew(() => { });
            });
            var actionFilterMock = CreateExceptionFilterMock((ec, ct) =>
            {
                log.Add("actionFilter");
                ec.Response = actionFilterResponse;
                return Task.Factory.StartNew(() => { });
            });
            var filters = new IExceptionFilter[] { globalFilterMock.Object, actionFilterMock.Object };

            // Act
            var result = ApiController.InvokeActionWithExceptionFilters(actionResult, _actionContextInstance, CancellationToken.None, filters);

            // Assert
            Assert.NotNull(result);
            result.WaitUntilCompleted();
            Assert.Equal(TaskStatus.RanToCompletion, result.Status);
            Assert.Same(globalFilterResponse, result.Result);
            Assert.Same(actionFilterResponse, resultSeenByGlobalFilter);
            Assert.Equal(new string[] { "actionFilter", "globalFilter" }, log.ToArray());
        }

        [Fact]
        public void ExecuteAsync_RunsExceptionFilter_WhenActionThrowsException()
        {
            // Arrange
            Exception expectedException = new NotImplementedException();
            ApiController controller = new ExceptionController(expectedException);

            // Act & Assert
            TestExceptionFilter(controller, expectedException, configure: null);
        }

        [Fact]
        public void ExecuteAsync_RunsExceptionFilter_WhenActionFilterThrowsException()
        {
            // Arrange
            Exception expectedException = new NotImplementedException();
            ApiController controller = new ExceptionlessController();
            Mock<IActionFilter> filterMock = new Mock<IActionFilter>();
            filterMock.Setup(f => f.ExecuteActionFilterAsync(It.IsAny<HttpActionContext>(),
                It.IsAny<CancellationToken>(), It.IsAny<Func<Task<HttpResponseMessage>>>())).Callback(() =>
            {
                throw expectedException;
            });
            IActionFilter filter = filterMock.Object;

            // Act & Assert
            TestExceptionFilter(controller, expectedException, (configuration) =>
                { configuration.Filters.Add(filter); });
        }

        [Fact]
        public void ExecuteAsync_RunsExceptionFilter_WhenAuthorizationFilterThrowsException()
        {
            // Arrange
            Exception expectedException = new NotImplementedException();
            ApiController controller = new ExceptionlessController();
            Mock<IAuthorizationFilter> filterMock = new Mock<IAuthorizationFilter>();
            filterMock.Setup(f => f.ExecuteAuthorizationFilterAsync(It.IsAny<HttpActionContext>(),
                It.IsAny<CancellationToken>(), It.IsAny<Func<Task<HttpResponseMessage>>>())).Callback(() =>
            {
                throw expectedException;
            });
            IAuthorizationFilter filter = filterMock.Object;

            // Act & Assert
            TestExceptionFilter(controller, expectedException, (configuration) =>
                { configuration.Filters.Add(filter); });
        }

        [Fact]
        public void ExecuteAsync_RunsExceptionFilter_WhenAuthenticationFilterAuthenticateThrowsException()
        {
            // Arrange
            Exception expectedException = new NotImplementedException();
            ApiController controller = new ExceptionlessController();
            Mock<IAuthenticationFilter> filterMock = new Mock<IAuthenticationFilter>();
            filterMock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(),
                It.IsAny<CancellationToken>())).Callback(() =>
                {
                    throw expectedException;
                });
            IAuthenticationFilter filter = filterMock.Object;

            // Act & Assert
            TestExceptionFilter(controller, expectedException, (configuration) =>
            { configuration.Filters.Add(filter); });
        }

        [Fact]
        public void ExecuteAsync_RunsExceptionFilter_WhenAuthenticationFilterChallengeThrowsException()
        {
            // Arrange
            Exception expectedException = new NotImplementedException();
            ApiController controller = new ExceptionlessController();
            Mock<IAuthenticationFilter> filterMock = new Mock<IAuthenticationFilter>();
            filterMock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(),
                It.IsAny<CancellationToken>())).Returns(() => Task.FromResult<object>(null));
            filterMock.Setup(f => f.ChallengeAsync(It.IsAny<HttpAuthenticationChallengeContext>(),
                It.IsAny<CancellationToken>())).Callback(() =>
                {
                    throw expectedException;
                });
            IAuthenticationFilter filter = filterMock.Object;

            // Act & Assert
            TestExceptionFilter(controller, expectedException, (configuration) =>
            { configuration.Filters.Add(filter); });
        }

        private static void TestExceptionFilter(ApiController controller, Exception expectedException,
            Action<HttpConfiguration> configure)
        {
            // Arrange
            Exception actualException = null;
            IHttpRouteData routeData = new HttpRouteData(new HttpRoute());

            using (HttpRequestMessage request = new HttpRequestMessage())
            using (HttpConfiguration configuration = new HttpConfiguration())
            using (HttpResponseMessage response = new HttpResponseMessage())
            {
                HttpControllerContext context = new HttpControllerContext(configuration, routeData, request);
                HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(configuration,
                    "Ignored", controller.GetType());
                context.Controller = controller;
                context.ControllerDescriptor = controllerDescriptor;

                if (configure != null)
                {
                    configure.Invoke(configuration);
                }

                Mock<IExceptionFilter> spy = new Mock<IExceptionFilter>();
                spy.Setup(f => f.ExecuteExceptionFilterAsync(It.IsAny<HttpActionExecutedContext>(),
                    It.IsAny<CancellationToken>())).Returns<HttpActionExecutedContext, CancellationToken>(
                    (c, i) =>
                    {
                        actualException = c.Exception;
                        c.Response = response;
                        return Task.FromResult<object>(null);
                    });

                configuration.Filters.Add(spy.Object);

                // Act
                HttpResponseMessage ignore = controller.ExecuteAsync(context, CancellationToken.None).Result;
            }

            // Assert
            Assert.Same(expectedException, actualException);
        }

        [Fact, RestoreThreadPrincipal]
        public void User_ReturnsThreadPrincipal()
        {
            // Arrange
            ApiController controller = new Mock<ApiController>().Object;
            IPrincipal principal = new GenericPrincipal(new GenericIdentity("joe"), new string[0]);
            Thread.CurrentPrincipal = principal;

            // Act
            IPrincipal result = controller.User;

            // Assert
            Assert.Same(result, principal);
        }

        [Fact]
        public void ApiControllerCannotBeReused()
        {
            // Arrange
            var config = new HttpConfiguration() { IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always };
            var singletonController = new Mock<ApiController> { CallBase = true }.Object;
            var mockDescriptor = new Mock<HttpControllerDescriptor>(config, "MyMock", singletonController.GetType()) { CallBase = true };
            mockDescriptor.Setup(d => d.CreateController(It.IsAny<HttpRequestMessage>())).Returns(singletonController);
            var mockSelector = new Mock<DefaultHttpControllerSelector>(config) { CallBase = true };
            mockSelector.Setup(s => s.SelectController(It.IsAny<HttpRequestMessage>())).Returns(mockDescriptor.Object);
            config.Routes.MapHttpRoute("default", "", new { controller = "MyMock" });
            config.Services.Replace(typeof(IHttpControllerSelector), mockSelector.Object);
            var server = new HttpServer(config);
            var invoker = new HttpMessageInvoker(server);

            // Act
            HttpResponseMessage response1 = invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost/"), CancellationToken.None).Result;
            HttpResponseMessage response2 = invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost/"), CancellationToken.None).Result;

            // Assert
            Assert.NotEqual(HttpStatusCode.InternalServerError, response1.StatusCode);
            Assert.Equal(HttpStatusCode.InternalServerError, response2.StatusCode);
            Assert.Contains("Cannot reuse an 'ApiController' instance. 'ApiController' has to be constructed per incoming message.", response2.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void ApiControllerPutsSelfInRequestResourcesToBeDisposed()
        {
            // Arrange
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute("default", "", new { controller = "SpyDispose" });
            var server = new HttpServer(config);
            var invoker = new HttpMessageInvoker(server);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            invoker.SendAsync(request, CancellationToken.None).WaitUntilCompleted();

            // Act
            request.DisposeRequestResources();

            // Assert
            Assert.True(SpyDisposeController.DisposeWasCalled);
        }

        private Mock<IAuthorizationFilter> CreateAuthorizationFilterMock(Func<HttpActionContext, CancellationToken, Func<Task<HttpResponseMessage>>, Task<HttpResponseMessage>> implementation)
        {
            Mock<IAuthorizationFilter> filterMock = new Mock<IAuthorizationFilter>();
            filterMock.Setup(f => f.ExecuteAuthorizationFilterAsync(It.IsAny<HttpActionContext>(),
                                                                    CancellationToken.None,
                                                                    It.IsAny<Func<Task<HttpResponseMessage>>>()))
                      .Returns(implementation)
                      .Verifiable();
            return filterMock;
        }

        private Mock<IActionFilter> CreateActionFilterMock(Func<HttpActionContext, CancellationToken, Func<Task<HttpResponseMessage>>, Task<HttpResponseMessage>> implementation)
        {
            Mock<IActionFilter> filterMock = new Mock<IActionFilter>();
            filterMock.Setup(f => f.ExecuteActionFilterAsync(It.IsAny<HttpActionContext>(),
                                                             CancellationToken.None,
                                                             It.IsAny<Func<Task<HttpResponseMessage>>>()))
                      .Returns(implementation)
                      .Verifiable();
            return filterMock;
        }

        private Mock<IExceptionFilter> CreateExceptionFilterMock(Func<HttpActionExecutedContext, CancellationToken, Task> implementation)
        {
            Mock<IExceptionFilter> filterMock = new Mock<IExceptionFilter>();
            filterMock.Setup(f => f.ExecuteExceptionFilterAsync(It.IsAny<HttpActionExecutedContext>(),
                                                                CancellationToken.None))
                      .Returns(implementation)
                      .Verifiable();
            return filterMock;
        }

        private IHttpActionResult CreateStubActionResult(Task<HttpResponseMessage> task)
        {
            Mock<IHttpActionResult> actionResultMock = new Mock<IHttpActionResult>(MockBehavior.Strict);
            actionResultMock.Setup(r => r.ExecuteAsync(It.IsAny<CancellationToken>())).Returns(() =>
            {
                return task;
            });
            return actionResultMock.Object;
        }

        private static Mock<DefaultServices> BuildFilterProvidingServicesMock(HttpConfiguration configuration, HttpActionDescriptor action, params FilterInfo[] filters)
        {
            var servicesMock = new Mock<DefaultServices> { CallBase = true };
            var filterProviderMock = new Mock<IFilterProvider>();
            servicesMock.Setup(r => r.GetServices(typeof(IFilterProvider))).Returns(new[] { filterProviderMock.Object });
            filterProviderMock.Setup(fp => fp.GetFilters(configuration, action)).Returns(filters);
            return servicesMock;
        }

        public class ExceptionController : ApiController
        {
            private readonly Exception _exception;

            public ExceptionController(Exception exception)
            {
                _exception = exception;
            }

            public void Get()
            {
                throw _exception;
            }
        }

        public class ExceptionlessController : ApiController
        {
            public void Get()
            {
            }
        }

        public class SpyDisposeController : ApiController
        {
            public static bool DisposeWasCalled = false;

            public SpyDisposeController()
            {
            }

            public void Get() { }

            protected override void Dispose(bool disposing)
            {
                DisposeWasCalled = true;
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Simple IFilter implementation with AllowMultiple = true
        /// </summary>
        public class TestMultiFilter : IFilter
        {
            public bool AllowMultiple
            {
                get { return true; }
            }
        }

        /// <summary>
        /// Simple IFilter implementation with AllowMultiple = false
        /// </summary>
        public class TestUniqueFilter : IFilter
        {
            public bool AllowMultiple
            {
                get { return false; }
            }
        }
    }
}
