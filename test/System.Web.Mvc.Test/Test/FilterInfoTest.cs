// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;

namespace System.Web.Mvc.Test
{
    public class FilterInfoTest
    {
        [Fact]
        public void Constructor_Default()
        {
            // Arrange + Act
            FilterInfo filterInfo = new FilterInfo();

            // Assert
            Assert.Empty(filterInfo.ActionFilters);
            Assert.Empty(filterInfo.AuthorizationFilters);
            Assert.Empty(filterInfo.ExceptionFilters);
            Assert.Empty(filterInfo.ResultFilters);
        }

        [Fact]
        public void Constructor_PopulatesFilterCollections()
        {
            // Arrange
            Mock<IActionFilter> actionFilterMock = new Mock<IActionFilter>();
            Mock<IAuthorizationFilter> authorizationFilterMock = new Mock<IAuthorizationFilter>();
            Mock<IExceptionFilter> exceptionFilterMock = new Mock<IExceptionFilter>();
            Mock<IResultFilter> resultFilterMock = new Mock<IResultFilter>();

            List<Filter> filters = new List<Filter>()
            {
                CreateFilter(actionFilterMock),
                CreateFilter(authorizationFilterMock),
                CreateFilter(exceptionFilterMock),
                CreateFilter(resultFilterMock),
            };

            // Act
            FilterInfo filterInfo = new FilterInfo(filters);

            // Assert
            Assert.Equal(actionFilterMock.Object, filterInfo.ActionFilters.SingleOrDefault());
            Assert.Equal(authorizationFilterMock.Object, filterInfo.AuthorizationFilters.SingleOrDefault());
            Assert.Equal(exceptionFilterMock.Object, filterInfo.ExceptionFilters.SingleOrDefault());
            Assert.Equal(resultFilterMock.Object, filterInfo.ResultFilters.SingleOrDefault());
        }

        [Fact]
        public void Constructor_IteratesOverFiltersOnlyOnce()
        {
            // Arrange
            var filtersMock = new Mock<IEnumerable<Filter>>();
            filtersMock.Setup(f => f.GetEnumerator()).Returns(new List<Filter>().GetEnumerator());

            // Act
            FilterInfo filterInfo = new FilterInfo(filtersMock.Object);

            // Assert
            filtersMock.Verify(f => f.GetEnumerator(), Times.Once());
        }

        private static Filter CreateFilter(Mock instanceMock)
        {
            return new Filter(instanceMock.Object, FilterScope.Global, null);
        }
    }
}
