// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Tracing.Tracers
{
    public class FilterTracerTest
    {
        [Fact]
        public void CreateFilterTracers_IFilter_With_IFilter_Returns_Single_Wrapped_IFilter()
        {
            // Arrange
            Mock<IFilter> mockFilter = new Mock<IFilter>();

            // Act
            IFilter[] wrappedFilters = FilterTracer.CreateFilterTracers(mockFilter.Object, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<FilterTracer>(wrappedFilters[0]);
        }

        [Fact]
        public void CreateFilterTracers_IFilter_With_IActionFilter_Returns_Single_Wrapped_IActionFilter()
        {
            // Arrange
            Mock<IActionFilter> mockFilter = new Mock<IActionFilter>();

            // Act
            IFilter[] wrappedFilters = FilterTracer.CreateFilterTracers(mockFilter.Object, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<ActionFilterTracer>(wrappedFilters[0]);
        }

        [Fact]
        public void CreateFilterTracers_IFilter_With_IExceptionFilter_Returns_Single_Wrapped_IExceptionFilter()
        {
            // Arrange
            Mock<IExceptionFilter> mockFilter = new Mock<IExceptionFilter>();

            // Act
            IFilter[] wrappedFilters = FilterTracer.CreateFilterTracers(mockFilter.Object, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<ExceptionFilterTracer>(wrappedFilters[0]);
        }

        [Fact]
        public void CreateFilterTracers_IFilter_With_IAuthorizationFilter_Returns_Single_Wrapped_IAuthorizationFilter()
        {
            // Arrange
            Mock<IAuthorizationFilter> mockFilter = new Mock<IAuthorizationFilter>();

            // Act
            IFilter[] wrappedFilters = FilterTracer.CreateFilterTracers(mockFilter.Object, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<AuthorizationFilterTracer>(wrappedFilters[0]);
        }

        [Fact]
        public void CreateFilterTracers_IFilter_With_ActionFilterAttribute_Returns_Single_Wrapped_Filter()
        {
            // Arrange
            Mock<ActionFilterAttribute> mockFilter = new Mock<ActionFilterAttribute>();

            // Act
            IFilter[] wrappedFilters = FilterTracer.CreateFilterTracers(mockFilter.Object, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<ActionFilterAttributeTracer>(wrappedFilters[0]);
        }

        [Fact]
        public void CreateFilterTracers_IFilter_With_ExceptionFilterAttribute_Returns_Single_Wrapped_Filter()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> mockFilter = new Mock<ExceptionFilterAttribute>();

            // Act
            IFilter[] wrappedFilters = FilterTracer.CreateFilterTracers(mockFilter.Object, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<ExceptionFilterAttributeTracer>(wrappedFilters[0]);
        }

        [Fact]
        public void CreateFilterTracers_IFilter_With_AuthorizationFilterAttribute_Returns_Single_Wrapped_Filter()
        {
            // Arrange
            Mock<AuthorizationFilterAttribute> mockFilter = new Mock<AuthorizationFilterAttribute>();

            // Act
            IFilter[] wrappedFilters = FilterTracer.CreateFilterTracers(mockFilter.Object, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<AuthorizationFilterAttributeTracer>(wrappedFilters[0]);
        }

        [Fact]
        public void CreateFilterTracers_IFilter_With_All_Filter_Interfaces_Returns_3_Wrapped_Filters()
        {
            // Arrange
            IFilter filter = new TestFilterAllBehaviors();

            // Act
            IFilter[] wrappedFilters = FilterTracer.CreateFilterTracers(filter, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(3, wrappedFilters.Length);
            Assert.Equal(1, wrappedFilters.OfType<ActionFilterTracer>().Count());
            Assert.Equal(1, wrappedFilters.OfType<AuthorizationFilterTracer>().Count());
            Assert.Equal(1, wrappedFilters.OfType<ExceptionFilterTracer>().Count());
        }

        [Fact]
        public void CreateFilterTracers_With_IFilter_Returns_Single_Wrapped_IFilter()
        {
            // Arrange
            Mock<IFilter> mockFilter = new Mock<IFilter>();
            FilterInfo filter = new FilterInfo(mockFilter.Object, FilterScope.Action);

            // Act
            FilterInfo[] wrappedFilters = FilterTracer.CreateFilterTracers(filter, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<FilterTracer>(wrappedFilters[0].Instance);
        }

        [Fact]
        public void CreateFilterTracers_With_IActionFilter_Returns_Single_Wrapped_IActionFilter()
        {
            // Arrange
            Mock<IActionFilter> mockFilter = new Mock<IActionFilter>();
            FilterInfo filter = new FilterInfo(mockFilter.Object, FilterScope.Action);

            // Act
            FilterInfo[] wrappedFilters = FilterTracer.CreateFilterTracers(filter, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<ActionFilterTracer>(wrappedFilters[0].Instance);
        }

        [Fact]
        public void CreateFilterTracers_With_IExceptionFilter_Returns_Single_Wrapped_IExceptionFilter()
        {
            // Arrange
            Mock<IExceptionFilter> mockFilter = new Mock<IExceptionFilter>();
            FilterInfo filter = new FilterInfo(mockFilter.Object, FilterScope.Action);

            // Act
            FilterInfo[] wrappedFilters = FilterTracer.CreateFilterTracers(filter, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<ExceptionFilterTracer>(wrappedFilters[0].Instance);
        }

        [Fact]
        public void CreateFilterTracers_With_IAuthorizationFilter_Returns_Single_Wrapped_IAuthorizationFilter()
        {
            // Arrange
            Mock<IAuthorizationFilter> mockFilter = new Mock<IAuthorizationFilter>();
            FilterInfo filter = new FilterInfo(mockFilter.Object, FilterScope.Action);

            // Act
            FilterInfo[] wrappedFilters = FilterTracer.CreateFilterTracers(filter, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<AuthorizationFilterTracer>(wrappedFilters[0].Instance);
        }

        [Fact]
        public void CreateFilterTracers_With_ActionFilterAttribute_Returns_2_Wrapped_Filters()
        {
            // Arrange
            Mock<ActionFilterAttribute> mockFilter = new Mock<ActionFilterAttribute>();
            FilterInfo filter = new FilterInfo(mockFilter.Object, FilterScope.Action);

            // Act
            FilterInfo[] wrappedFilters = FilterTracer.CreateFilterTracers(filter, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<ActionFilterAttributeTracer>(wrappedFilters[0].Instance);
        }

        [Fact]
        public void CreateFilterTracers_With_ExceptionFilterAttribute_Returns_2_Wrapped_Filters()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> mockFilter = new Mock<ExceptionFilterAttribute>();
            FilterInfo filter = new FilterInfo(mockFilter.Object, FilterScope.Action);

            // Act
            FilterInfo[] wrappedFilters = FilterTracer.CreateFilterTracers(filter, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<ExceptionFilterAttributeTracer>(wrappedFilters[0].Instance);
        }

        [Fact]
        public void CreateFilterTracers_With_AuthorizationFilterAttribute_Returns_2_Wrapped_Filters()
        {
            // Arrange
            Mock<AuthorizationFilterAttribute> mockFilter = new Mock<AuthorizationFilterAttribute>();
            FilterInfo filter = new FilterInfo(mockFilter.Object, FilterScope.Action);

            // Act
            FilterInfo[] wrappedFilters = FilterTracer.CreateFilterTracers(filter, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(1, wrappedFilters.Length);
            Assert.IsType<AuthorizationFilterAttributeTracer>(wrappedFilters[0].Instance); ;
        }

        [Fact]
        public void CreateFilterTracers_With_All_Filter_Interfaces_Returns_3_Wrapped_Filters()
        {
            // Arrange
            FilterInfo filter = new FilterInfo(new TestFilterAllBehaviors(), FilterScope.Action);

            // Act
            FilterInfo[] wrappedFilters = FilterTracer.CreateFilterTracers(filter, new TestTraceWriter()).ToArray();

            // Assert
            Assert.Equal(3, wrappedFilters.Length);
            Assert.Equal(1, wrappedFilters.Where(f => f.Instance.GetType() == typeof(ActionFilterTracer)).Count());
            Assert.Equal(1, wrappedFilters.Where(f => f.Instance.GetType() == typeof(AuthorizationFilterTracer)).Count());
            Assert.Equal(1, wrappedFilters.Where(f => f.Instance.GetType() == typeof(ExceptionFilterTracer)).Count());
        }

        // Test filter class that exposes all filter behaviors will cause separate filters for each
        class TestFilterAllBehaviors : IActionFilter, IExceptionFilter, IAuthorizationFilter
        {
            Task<Net.Http.HttpResponseMessage> IActionFilter.ExecuteActionFilterAsync(Controllers.HttpActionContext actionContext, Threading.CancellationToken cancellationToken, Func<Threading.Tasks.Task<Net.Http.HttpResponseMessage>> continuation)
            {
                throw new NotImplementedException();
            }

            bool IFilter.AllowMultiple
            {
                get { throw new NotImplementedException(); }
            }

            Task IExceptionFilter.ExecuteExceptionFilterAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            Task<HttpResponseMessage> IAuthorizationFilter.ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<Net.Http.HttpResponseMessage>> continuation)
            {
                throw new NotImplementedException();
            }
        }
    }
}
