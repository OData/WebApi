using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Moq;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Filters
{
    public class EnumerableEvaluatorFilterProviderTest
    {
        private HttpConfiguration _configuration = new HttpConfiguration();
        private Mock<HttpActionDescriptor> _actionDescriptorMock = new Mock<HttpActionDescriptor>();
        private EnumerableEvaluatorFilterProvider _filterProvider = new EnumerableEvaluatorFilterProvider();

        [Fact]
        public void GetFilters_WhenConfigurationParameterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                _filterProvider.GetFilters(configuration: null, actionDescriptor: _actionDescriptorMock.Object);
            }, "configuration");
        }

        [Fact]
        public void GetFilters_WhenActionDescriptorParameterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                _filterProvider.GetFilters(_configuration, actionDescriptor: null);
            }, "actionDescriptor");
        }

        [Theory]
        [InlineData(typeof(IEnumerable<string>))]
        [InlineData(typeof(IEnumerable<object>))]
        [InlineData(typeof(IQueryable<string>))]
        //[InlineData(typeof(HttpResponseMessage))] // static signature problems
        //[InlineData(typeof(Task<HttpResponseMessage>))] // static signature problems
        [InlineData(typeof(ObjectContent<IEnumerable<string>>))]
        [InlineData(typeof(Task<ObjectContent<IEnumerable<string>>>))]
        public void GetFilters_IfActionResultTypeIsSupported_ReturnsFilterInstance(Type actionReturnType)
        {
            // Arrange
            _actionDescriptorMock.Setup(ad => ad.ReturnType).Returns(actionReturnType);

            // Act
            IEnumerable<FilterInfo> result = _filterProvider.GetFilters(_configuration, _actionDescriptorMock.Object);

            // Assert
            FilterInfo filter = result.Single();
            Assert.NotNull(filter);
            Assert.IsType<EnumerableEvaluatorFilter>(filter.Instance);
            Assert.Equal(FilterScope.First, filter.Scope);
        }

        [Theory]
        [InlineData(typeof(Object))]
        [InlineData(typeof(String))]
        [InlineData(typeof(Int32))]
        [InlineData(typeof(object[]))]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(IList<string>))]
        [InlineData(typeof(IEnumerable))]
        [InlineData(typeof(IQueryable))]
        public void GetFilters_IfActionResultTypeIsNotSupported_ReturnsEmptyResult(Type actionReturnType)
        {
            // Arrange
            _actionDescriptorMock.Setup(ad => ad.ReturnType).Returns(actionReturnType);

            // Act
            IEnumerable<FilterInfo> result = _filterProvider.GetFilters(_configuration, _actionDescriptorMock.Object);

            // Assert
            Assert.Empty(result);
        }
    }
}
