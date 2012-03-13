using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Filters
{
    public class EnumerableEvaluatorFilterTest
    {
        private EnumerableEvaluatorFilter _filter = new EnumerableEvaluatorFilter();
        private HttpActionExecutedContext _actionExecutedContext;
        private Mock<HttpActionDescriptor> _actionDescriptorMock;

        public EnumerableEvaluatorFilterTest()
        {
            _actionDescriptorMock = new Mock<HttpActionDescriptor>();
            var actionContext = new HttpActionContext { ActionDescriptor = _actionDescriptorMock.Object };
            _actionExecutedContext = new HttpActionExecutedContext(actionContext, exception: null);
        }

        [Fact]
        public void AllowMultiple_ReturnsFalse()
        {
            Assert.False(_filter.AllowMultiple);
        }

        [Fact]
        public void Instance_IsSingletonProperty()
        {
            var first = EnumerableEvaluatorFilter.Instance;
            Assert.NotNull(first);
            var second = EnumerableEvaluatorFilter.Instance;
            Assert.Same(first, second);
        }

        [Fact]
        public void OnActionExecuted_IfContextParameterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                _filter.OnActionExecuted(actionExecutedContext: null);
            }, "actionExecutedContext");
        }

        [Fact]
        public void OnActionExecuted_IfActionContentTypeIsNotIEnumerable_ButResponseContentIsAnIEnumerable_DoesNothing()
        {
            // Arrange
            _actionDescriptorMock.Setup(ad => ad.ReturnType).Returns(typeof(object));
            _actionExecutedContext.Result = new HttpResponseMessage() { Content = new ObjectContent<IEnumerable<string>>(new List<string>(), new JsonMediaTypeFormatter()) };
            var content = _actionExecutedContext.Result.Content;

            // Act
            _filter.OnActionExecuted(_actionExecutedContext);

            // Assert
            Assert.Same(content, _actionExecutedContext.Result.Content);
        }

        [Fact]
        public void OnActionExecuted_IfActionContentTypeIsIEnumerable_ButResponseContentIsNull_DoesNothing()
        {
            // Arrange
            _actionDescriptorMock.Setup(ad => ad.ReturnType).Returns(typeof(IEnumerable<string>));
            _actionExecutedContext.Result = new HttpResponseMessage() { Content = new ObjectContent<IEnumerable<string>>(null, new JsonMediaTypeFormatter()) };
            var content = _actionExecutedContext.Result.Content;

            // Act
            _filter.OnActionExecuted(_actionExecutedContext);

            // Assert
            Assert.Same(content, _actionExecutedContext.Result.Content);
        }

        [Theory]
        [PropertyData("NotSupportedTypesTestData")]
        public void OnActionExecuted_IfActionContentTypeIsNotIEnumerable_DoesNothing(Type actionReturnType, object input)
        {
            // Arrange
            _actionDescriptorMock.Setup(ad => ad.ReturnType).Returns(actionReturnType);
            _actionExecutedContext.Result = new HttpResponseMessage() { Content = new ObjectContent<object>(input, new JsonMediaTypeFormatter()) };
            var content = _actionExecutedContext.Result.Content;

            // Act
            _filter.OnActionExecuted(_actionExecutedContext);

            // Assert
            var objectContent = Assert.IsAssignableFrom<ObjectContent>(_actionExecutedContext.Result.Content);
            object output = objectContent.Value;
            Assert.Same(input, output);
            Assert.Same(content, objectContent);
        }

        public static TheoryDataSet<Type, object> NotSupportedTypesTestData
        {
            get
            {
                return new TheoryDataSet<Type, object>
                {
                    {typeof(int), 42},
                    {typeof(object), new object()},
                    {typeof(IEnumerable), new ArrayList()},
                    {typeof(IQueryable), new List<string>().AsQueryable()},
                    {typeof(string), "some value"},
                    {typeof(byte[]), new byte[3]},
                    {typeof(string[]), new string[3]},
                    {typeof(List<string>), new List<string>()},
                };
            }
        }

        [Theory]
        [InlineData(typeof(IEnumerable<string>))]
        //[InlineData(typeof(HttpResponseMessage))]  // static signature problems
        // [InlineData(typeof(Task<HttpResponseMessage>))] // static signature problems
        [InlineData(typeof(ObjectContent<IEnumerable<string>>))]
        [InlineData(typeof(Task<ObjectContent<IEnumerable<string>>>))]
        public void OnActionExecuted_IfActionContentTypeIsIEnumerable_AndResponseContentTypeMatches_CopiesContentToNewList(Type actionReturnType)
        {
            // Arrange
            _actionDescriptorMock.Setup(ad => ad.ReturnType).Returns(actionReturnType);
            List<string> input = Enumerable.Range(0, 3).Select(i => "Item " + i).ToList();
            _actionExecutedContext.Result = new HttpResponseMessage() { Content = new ObjectContent<List<string>>(input, new JsonMediaTypeFormatter()) }; 

            // Act
            _filter.OnActionExecuted(_actionExecutedContext);

            // Assert
            var content = Assert.IsAssignableFrom<ObjectContent>(_actionExecutedContext.Result.Content);
            object output = content.Value;
            Assert.NotSame(input, output);
            Assert.IsType<List<string>>(output);
        }

        [Fact]
        public void OnActionExecuted_IfActionContentTypeIsIEnumerable_ButResponseContentTypeIsDifferentIEnumerable_DoesNothing()
        {
            // Arrange
            _actionDescriptorMock.Setup(ad => ad.ReturnType).Returns(typeof(IEnumerable<string>));
            List<int> input = Enumerable.Range(0, 3).ToList();
            _actionExecutedContext.Result = new HttpResponseMessage() { Content = new ObjectContent<List<int>>(input, new JsonMediaTypeFormatter()) };

            // Act
            _filter.OnActionExecuted(_actionExecutedContext);

            // Assert
            var content = Assert.IsAssignableFrom<ObjectContent>(_actionExecutedContext.Result.Content);
            List<int> output = Assert.IsType<List<int>>(content.Value);
            Assert.Same(input, output);
        }

        [Fact]
        public void OnActionExecuted_IfActionContentTypeIsIQueryable_CopiesContentToNewResult()
        {
            // Arrange
            _actionDescriptorMock.Setup(ad => ad.ReturnType).Returns(typeof(IQueryable<string>));
            IQueryable<string> input = Enumerable.Range(0, 3).Select(i => "Item " + i).AsQueryable();
            _actionExecutedContext.Result = new HttpResponseMessage() { Content = new ObjectContent<IQueryable<string>>(input, new JsonMediaTypeFormatter()) };

            // Act
            _filter.OnActionExecuted(_actionExecutedContext);

            // Assert
            var content = Assert.IsAssignableFrom<ObjectContent>(_actionExecutedContext.Result.Content);
            object output = content.Value;
            Assert.NotSame(input, output);
        }

        [Fact]
        public void OnActionExecuted_IfResponseIsNull_DoesNothing()
        {
            // Arrange
            _actionDescriptorMock.Setup(ad => ad.ReturnType).Returns(typeof(IEnumerable<string>));
            _actionExecutedContext.Result = null;

            // Act
            _filter.OnActionExecuted(_actionExecutedContext);

            // Assert
            Assert.Null(_actionExecutedContext.Result);
        }
    }
}
