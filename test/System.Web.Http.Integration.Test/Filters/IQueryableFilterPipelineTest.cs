using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Moq;
using Xunit;

namespace System.Web.Http.Filters
{
    public class IQueryableFilterPipelineTest
    {
        [Fact]
        public void IQueryableRelatedFiltersAreOrderedCorrectly()
        {
            // Arrange
            Mock<HttpControllerDescriptor> controllerDescriptorMock = new Mock<HttpControllerDescriptor>() { CallBase = true };
            controllerDescriptorMock.Object.Configuration = new HttpConfiguration();
            controllerDescriptorMock.Object.ControllerType = typeof(IQueryableFilterPipelineTest);
            Mock<HttpActionDescriptor> actionDescriptorMock = new Mock<HttpActionDescriptor>(controllerDescriptorMock.Object) { CallBase = true };
            actionDescriptorMock.Setup(ad => ad.GetFilters()).Returns(new Collection<IFilter>(new IFilter[] { new ResultLimitAttribute(42) }));
            actionDescriptorMock.Setup(ad => ad.ReturnType).Returns(typeof(IQueryable<string>));
            HttpActionDescriptor actionDescriptor = actionDescriptorMock.Object;

            // Act
            var filters = actionDescriptor.GetFilterPipeline();

            // Assert
            Assert.Equal(3, filters.Count);
            Assert.IsType<EnumerableEvaluatorFilter>(filters[0].Instance);
            Assert.IsType<ResultLimitAttribute>(filters[1].Instance);
            Assert.IsType<QueryCompositionFilterAttribute>(filters[2].Instance);
        }

        [Fact]
        public void EnumerableEvaluatorFilterExecutesQuerySoThatExceptionsCanBeCaughtByExceptionFilters()
        {
            // Arrange
            Mock<ApiController> controllerMock = new Mock<ApiController> { CallBase = true };
            Mock<HttpActionDescriptor> actionDescriptorMock = new Mock<HttpActionDescriptor>();
            actionDescriptorMock.Setup(ad => ad.ReturnType).Returns(typeof(IEnumerable<string>));
            Mock<IExceptionFilter> exceptionFilterMock = new Mock<IExceptionFilter>();
            actionDescriptorMock.Setup(ad => ad.GetFilterPipeline())
                                .Returns(new Collection<FilterInfo>(new IFilter[] { EnumerableEvaluatorFilter.Instance, exceptionFilterMock.As<IFilter>().Object }.Select(f => new FilterInfo(f, FilterScope.Action)).ToList()));
            Mock<IHttpActionSelector> actionSelectorMock = new Mock<IHttpActionSelector>();
            actionSelectorMock.Setup(actionSelector => actionSelector.SelectAction(It.IsAny<HttpControllerContext>())).Returns(actionDescriptorMock.Object);
            Mock<IHttpActionInvoker> actionInvokerMock = new Mock<IHttpActionInvoker>();
            InvalidOperationException exception = new InvalidOperationException("Bad enumeration");
            IEnumerable<string> actionResult = Enumerable.Range(0, 1).Select<int, string>(i =>
                {
                    throw exception;
                });
            var invocationTask = Task.Factory.StartNew<HttpResponseMessage>(() => new HttpResponseMessage
            {
                Content = new ObjectContent<IEnumerable<string>>(actionResult, new JsonMediaTypeFormatter())
            });
            actionInvokerMock.Setup(ai => ai.InvokeActionAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>()))
                             .Returns(invocationTask);
            Mock<HttpControllerDescriptor> controllerDescriptorMock = new Mock<HttpControllerDescriptor>();
            controllerDescriptorMock.Object.HttpActionSelector = actionSelectorMock.Object;
            controllerDescriptorMock.Object.HttpActionInvoker = actionInvokerMock.Object;            
                        
            Mock<IActionValueBinder> binderMock = new Mock<IActionValueBinder>();
            Mock<HttpActionBinding> actionBindingMock = new Mock<HttpActionBinding>();
            actionBindingMock.Setup(b => b.ExecuteBindingAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>())).Returns(Task.Factory.StartNew(() => { }));
            binderMock.Setup(b => b.GetBinding(It.IsAny<HttpActionDescriptor>())).Returns(actionBindingMock.Object);
            controllerDescriptorMock.Object.ActionValueBinder = binderMock.Object;

            HttpConfiguration config = new HttpConfiguration();

            var controllerContext = new HttpControllerContext { ControllerDescriptor = controllerDescriptorMock.Object, Configuration = config };

            // Act
            var responseTask = controllerMock.Object.ExecuteAsync(controllerContext, CancellationToken.None);

            // Assert
            responseTask.WaitUntilCompleted();
            exceptionFilterMock.Verify(ef => ef.ExecuteExceptionFilterAsync(It.Is<HttpActionExecutedContext>(aec => aec.Exception == exception), It.IsAny<CancellationToken>()));
        }

        public class TestController : ApiController
        {
            private Exception _ex;
            public TestController(Exception ex)
            {
                _ex = ex;
            }
            public IEnumerable<string> Get()
            {
                yield return "cat";
                throw _ex;
            }
        }
    }
}
