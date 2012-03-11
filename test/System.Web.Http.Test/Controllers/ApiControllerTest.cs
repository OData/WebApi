using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;
using System.Web.Http.Routing;
using System.Web.Http.Services;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

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
            controllerDescriptor.HttpActionInvoker = mockInvoker.Object;

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
            controllerDescriptor.HttpActionSelector = mockSelector.Object;

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
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, routeData: route, request: new HttpRequestMessage() { Method = HttpMethod.Get });
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
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, routeData: route, request: new HttpRequestMessage() { Method = HttpMethod.Get });
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
                    Method = HttpMethod.Get,
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

            // Act & Assert
            Assert.Throws<HttpResponseException>(() =>
            {
                HttpResponseMessage message = api.ExecuteAsync(controllerContext, CancellationToken.None).Result;
            },
            String.Format(SRResources.ApiControllerActionSelector_ActionNameNotFound, controllerType.Name, actionName));
        }

        [Fact]
        public void ExecuteAsync_InvokesAuthorizationFilters_ThenInvokesModelBinding_ThenInvokesActionFilters_ThenInvokesAction()
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
            var authFilterMock = CreateAuthorizationFilterMock((ac, ct, cont) =>
            {
                log.Add("auth filters");
                return cont();
            });
            var selectorMock = new Mock<IHttpActionSelector>();
            selectorMock.Setup(s => s.SelectAction(controllerContext).GetFilterPipeline())
                                .Returns(new Collection<FilterInfo>(new List<FilterInfo>() { new FilterInfo(actionFilterMock.Object, FilterScope.Action), new FilterInfo(authFilterMock.Object, FilterScope.Action) }));
            ApiController controller = controllerMock.Object;
            var invokerMock = new Mock<IHttpActionInvoker>();
            invokerMock.Setup(i => i.InvokeActionAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>()))
                       .Returns(() => Task.Factory.StartNew(() =>
                       {
                           log.Add("action");
                           return new HttpResponseMessage();
                       }));
            controllerContext.ControllerDescriptor.HttpActionInvoker = invokerMock.Object;
            controllerContext.ControllerDescriptor.HttpActionSelector = selectorMock.Object;
            controllerContext.ControllerDescriptor.ActionValueBinder = binderMock.Object;

            var task = controller.ExecuteAsync(controllerContext, CancellationToken.None);

            Assert.NotNull(task);
            task.WaitUntilCompleted();
            Assert.Equal(new string[] { "auth filters", "model binding", "action filters", "action" }, log.ToArray());
        }

        [Fact]
        public void GetFilters_QueriesFilterProvidersFromServiceResolver()
        {
            // Arrange
            Mock<IDependencyResolver> resolverMock = new Mock<IDependencyResolver>();
            Mock<IFilterProvider> filterProviderMock = new Mock<IFilterProvider>();
            resolverMock.Setup(r => r.GetServices(typeof(IFilterProvider))).Returns(new object[] { filterProviderMock.Object }).Verifiable();
            _configurationInstance.ServiceResolver.SetResolver(resolverMock.Object);

            HttpActionDescriptor actionDescriptorMock = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            actionDescriptorMock.Configuration = _configurationInstance;

            // Act
            actionDescriptorMock.GetFilterPipeline();

            // Assert
            resolverMock.Verify();
        }

        [Fact]
        public void GetFilters_UsesFilterProvidersToGetFilters()
        {
            // Arrange
            Mock<IDependencyResolver> resolverMock = new Mock<IDependencyResolver>();
            Mock<IFilterProvider> filterProviderMock = new Mock<IFilterProvider>();
            resolverMock.Setup(r => r.GetServices(typeof(IFilterProvider))).Returns(new[] { filterProviderMock.Object });
            _configurationInstance.ServiceResolver.SetResolver(resolverMock.Object);

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
            Mock<IDependencyResolver> resolverMock = BuildFilterProvidingDependencyResolver(_configurationInstance, actionDescriptorMock, globalFilter, actionFilter, controllerFilter);
            _configurationInstance.ServiceResolver.SetResolver(resolverMock.Object);

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
            Mock<IDependencyResolver> resolverMock = BuildFilterProvidingDependencyResolver(
                _configurationInstance, actionDescriptorMock,
                multiActionFilter, multiGlobalFilter, uniqueControllerFilter, uniqueActionFilter);
            _configurationInstance.ServiceResolver.SetResolver(resolverMock.Object);

            // Act
            var result = actionDescriptorMock.GetFilterPipeline().ToArray();

            // Assert
            Assert.Equal(new[] { multiGlobalFilter, multiActionFilter, uniqueActionFilter }, result);
        }

        [Fact]
        public void InvokeActionWithActionFilters_ChainsFiltersInOrderFollowedByInnerActionContinuation()
        {
            // Arrange
            List<string> log = new List<string>();
            Mock<IActionFilter> globalFilterMock = CreateActionFilterMock((ctx, ct, continuation) =>
            {
                log.Add("globalFilter");
                return continuation();
            });
            Mock<IActionFilter> actionFilterMock = CreateActionFilterMock((ctx, ct, continuation) =>
            {
                log.Add("actionFilter");
                return continuation();
            });
            Func<Task<HttpResponseMessage>> innerAction = () => Task<HttpResponseMessage>.Factory.StartNew(() =>
            {
                log.Add("innerAction");
                return null;
            });
            List<IActionFilter> filters = new List<IActionFilter>() {
                globalFilterMock.Object,
                actionFilterMock.Object,
            };

            // Act
            var result = ApiController.InvokeActionWithActionFilters(_actionContextInstance, CancellationToken.None, filters, innerAction);

            // Assert
            Assert.NotNull(result);
            var resultTask = result();
            Assert.NotNull(resultTask);
            resultTask.WaitUntilCompleted();
            Assert.Equal(new[] { "globalFilter", "actionFilter", "innerAction" }, log.ToArray());
            globalFilterMock.Verify();
            actionFilterMock.Verify();
        }

        [Fact]
        public void InvokeActionWithAuthorizationFilters_ChainsFiltersInOrderFollowedByInnerActionContinuation()
        {
            // Arrange
            List<string> log = new List<string>();
            Mock<IAuthorizationFilter> globalFilterMock = CreateAuthorizationFilterMock((ctx, ct, continuation) =>
            {
                log.Add("globalFilter");
                return continuation();
            });
            Mock<IAuthorizationFilter> actionFilterMock = CreateAuthorizationFilterMock((ctx, ct, continuation) =>
            {
                log.Add("actionFilter");
                return continuation();
            });
            Func<Task<HttpResponseMessage>> innerAction = () => Task<HttpResponseMessage>.Factory.StartNew(() =>
            {
                log.Add("innerAction");
                return null;
            });
            List<IAuthorizationFilter> filters = new List<IAuthorizationFilter>() {
                globalFilterMock.Object,
                actionFilterMock.Object,
            };

            // Act
            var result = ApiController.InvokeActionWithAuthorizationFilters(_actionContextInstance, CancellationToken.None, filters, innerAction);

            // Assert
            Assert.NotNull(result);
            var resultTask = result();
            Assert.NotNull(resultTask);
            resultTask.WaitUntilCompleted();
            Assert.Equal(new[] { "globalFilter", "actionFilter", "innerAction" }, log.ToArray());
            globalFilterMock.Verify();
            actionFilterMock.Verify();
        }

        [Fact]
        public void InvokeActionWithExceptionFilters_IfActionTaskIsSuccessful_ReturnsSuccessTask()
        {
            // Arrange
            List<string> log = new List<string>();
            var response = new HttpResponseMessage();
            var actionTask = TaskHelpers.FromResult(response);
            var exceptionFilterMock = CreateExceptionFilterMock((ec, ct) =>
            {
                log.Add("exceptionFilter");
                return Task.Factory.StartNew(() => { });
            });
            var filters = new[] { exceptionFilterMock.Object };

            // Act
            var result = ApiController.InvokeActionWithExceptionFilters(actionTask, _actionContextInstance, CancellationToken.None, filters);

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
            var actionTask = TaskHelpers.Canceled<HttpResponseMessage>();
            var exceptionFilterMock = CreateExceptionFilterMock((ec, ct) =>
            {
                log.Add("exceptionFilter");
                return Task.Factory.StartNew(() => { });
            });
            var filters = new[] { exceptionFilterMock.Object };

            // Act
            var result = ApiController.InvokeActionWithExceptionFilters(actionTask, _actionContextInstance, CancellationToken.None, filters);

            // Assert
            Assert.NotNull(result);
            result.WaitUntilCompleted();
            Assert.Equal(TaskStatus.Canceled, result.Status);
            Assert.Equal(new string[] { }, log.ToArray());
        }

        [Fact]
        public void InvokeActionWithExceptionFilters_IfActionTaskIsFaulted_ExecutesFiltersAndReturnsFaultedTaskIfNotHandled()
        {
            // Arrange
            List<string> log = new List<string>();
            var exception = new Exception();
            var actionTask = TaskHelpers.FromError<HttpResponseMessage>(exception);
            Exception exceptionSeenByFilter = null;
            var exceptionFilterMock = CreateExceptionFilterMock((ec, ct) =>
            {
                exceptionSeenByFilter = ec.Exception;
                log.Add("exceptionFilter");
                return Task.Factory.StartNew(() => { });
            });
            var filters = new[] { exceptionFilterMock.Object };

            // Act
            var result = ApiController.InvokeActionWithExceptionFilters(actionTask, _actionContextInstance, CancellationToken.None, filters);

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
            var actionTask = TaskHelpers.FromError<HttpResponseMessage>(exception);
            HttpResponseMessage globalFilterResponse = new HttpResponseMessage();
            HttpResponseMessage actionFilterResponse = new HttpResponseMessage();
            HttpResponseMessage resultSeenByGlobalFilter = null;
            var globalFilterMock = CreateExceptionFilterMock((ec, ct) =>
            {
                log.Add("globalFilter");
                resultSeenByGlobalFilter = ec.Result;
                ec.Result = globalFilterResponse;
                return Task.Factory.StartNew(() => { });
            });
            var actionFilterMock = CreateExceptionFilterMock((ec, ct) =>
            {
                log.Add("actionFilter");
                ec.Result = actionFilterResponse;
                return Task.Factory.StartNew(() => { });
            });
            var filters = new[] { globalFilterMock.Object, actionFilterMock.Object };

            // Act
            var result = ApiController.InvokeActionWithExceptionFilters(actionTask, _actionContextInstance, CancellationToken.None, filters);

            // Assert
            Assert.NotNull(result);
            result.WaitUntilCompleted();
            Assert.Equal(TaskStatus.RanToCompletion, result.Status);
            Assert.Same(globalFilterResponse, result.Result);
            Assert.Same(actionFilterResponse, resultSeenByGlobalFilter);
            Assert.Equal(new string[] { "actionFilter", "globalFilter" }, log.ToArray());
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

        private static Mock<IDependencyResolver> BuildFilterProvidingDependencyResolver(HttpConfiguration configuration, HttpActionDescriptor action, params FilterInfo[] filters)
        {
            Mock<IDependencyResolver> resolverMock = new Mock<IDependencyResolver>();
            Mock<IFilterProvider> filterProviderMock = new Mock<IFilterProvider>();
            resolverMock.Setup(r => r.GetServices(typeof(IFilterProvider))).Returns(new[] { filterProviderMock.Object });
            filterProviderMock.Setup(fp => fp.GetFilters(configuration, action)).Returns(filters);
            return resolverMock;
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
