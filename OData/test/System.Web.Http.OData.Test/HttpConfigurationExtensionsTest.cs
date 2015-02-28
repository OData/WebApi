// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Query;
using System.Web.Http.Services;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData
{
    public class HttpConfigurationExtensionsTest
    {
        [Fact]
        public void SetODataFormatter_AddsFormatterToTheFormatterCollection()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            ODataMediaTypeFormatter formatter = CreateODataFormatter();

            // Act
            configuration.Formatters.Insert(0, formatter);

            // Assert
            Assert.Contains(formatter, configuration.Formatters);
        }

        [Fact]
        public void IsODataFormatter_ReturnsTrue_ForODataFormatters()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            ODataMediaTypeFormatter formatter1 = CreateODataFormatter();
            ODataMediaTypeFormatter formatter2 = CreateODataFormatter();
            configuration.Formatters.Add(formatter1);
            configuration.Formatters.Add(formatter2);

            // Act
            IEnumerable<MediaTypeFormatter> result = configuration.Formatters.Where(f => f != null && Decorator.GetInner(f) is ODataMediaTypeFormatter);

            // Assert
            IEnumerable<MediaTypeFormatter> expectedFormatters = new MediaTypeFormatter[]
            {
                formatter1, formatter2
            };

            Assert.True(expectedFormatters.SequenceEqual(result));
        }

        [Fact]
        public void IsODataFormatter_ReturnsTrue_For_Derived_ODataFormatters()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            ODataMediaTypeFormatter formatter1 = CreateODataFormatter();
            DerivedODataMediaTypeFormatter formatter2 = new DerivedODataMediaTypeFormatter(new ODataPayloadKind[0]);
            configuration.Formatters.Add(formatter1);
            configuration.Formatters.Add(formatter2);

            // Act
            IEnumerable<MediaTypeFormatter> result = configuration.Formatters.Where(f => f != null && Decorator.GetInner(f) is ODataMediaTypeFormatter);

            // Assert
            IEnumerable<MediaTypeFormatter> expectedFormatters = new MediaTypeFormatter[]
            {
                formatter1, formatter2
            };

            Assert.True(expectedFormatters.SequenceEqual(result));
        }

        [Fact]
        public void AddQuerySupport_AddsQueryableFilterProvider()
        {
            HttpConfiguration configuration = new HttpConfiguration();

            configuration.AddODataQueryFilter();

            var queryFilterProviders = configuration.Services.GetFilterProviders().OfType<QueryFilterProvider>();
            Assert.Equal(1, queryFilterProviders.Count());
            var queryAttribute = Assert.IsType<EnableQueryAttribute>(queryFilterProviders.First().QueryFilter);
        }

        [Fact]
        public void AddQuerySupport_AddsFilterProviderForQueryFilter()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            Mock<IActionFilter> myQueryFilter = new Mock<IActionFilter>();

            configuration.AddODataQueryFilter(myQueryFilter.Object);

            var queryFilterProviders = configuration.Services.GetFilterProviders().OfType<QueryFilterProvider>();
            Assert.Equal(1, queryFilterProviders.Count());
            Assert.Same(myQueryFilter.Object, queryFilterProviders.First().QueryFilter);
        }

        [Theory]
        [InlineData("GetQueryableWithFilterAttribute")]
        [InlineData("GetQueryableWithQueryableFilterAttributeOnBase")]
        public void AddQuerySupport_ActionFilters_TakePrecedence(string actionName)
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.AddODataQueryFilter();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(
                config, 
                "FilterProviderTest", 
                typeof(FilterProviderTestController));

            MethodInfo method = typeof(FilterProviderTestController).GetMethod(actionName);
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, method);

            // Act
            Collection<FilterInfo> filters = actionDescriptor.GetFilterPipeline();

            // Assert
            Assert.Equal(1, filters.Count);
            Assert.Equal(100, ((EnableQueryAttribute)filters[0].Instance).PageSize);
        }

        [Fact]
        public void GetETagHandler_ReturnDefaultODataETagHandler_IfNotSet()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            IETagHandler eTagHandler = config.GetETagHandler();

            // Assert
            Assert.IsType<DefaultODataETagHandler>(eTagHandler);
        }

        [Fact]
        public void SetETagHandler_ReturnsHandlerSet_UsingSetETagHandler()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            IETagHandler eTagHandler = new Mock<IETagHandler>().Object;

            // Act
            config.SetETagHandler(eTagHandler);

            // Assert
            Assert.Equal(eTagHandler, config.GetETagHandler());
        }

        private static ODataMediaTypeFormatter CreateODataFormatter()
        {
            return new ODataMediaTypeFormatter(new ODataPayloadKind[0]);
        }

        private class DerivedODataMediaTypeFormatter: ODataMediaTypeFormatter
        {
            public DerivedODataMediaTypeFormatter(IEnumerable<ODataPayloadKind> payloadKinds)
                : base(payloadKinds)
            {
            }
        }
    }
}
