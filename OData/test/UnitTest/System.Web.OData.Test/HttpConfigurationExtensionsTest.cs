// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Services;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData
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

        [Fact]
        public void AddQuerySupport_ActionFilters_TakePrecedence()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.AddODataQueryFilter();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(config, "FilterProviderTest", typeof(FilterProviderTestController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(FilterProviderTestController).GetMethod("GetQueryableWithFilterAttribute"));

            Collection<FilterInfo> filters = actionDescriptor.GetFilterPipeline();

            Assert.Equal(1, filters.Count);
            Assert.Equal(100, ((EnableQueryAttribute)filters[0].Instance).PageSize);
        }

        [Fact]
        public void GetETagHandler_ReturnDefaultODataETagHandler_IfNotSet()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            IETagHandler etagHandler = config.GetETagHandler();

            // Assert
            Assert.IsType<DefaultODataETagHandler>(etagHandler);
        }

        [Fact]
        public void SetETagHandler_ReturnsHandlerSet_UsingSetETagHandler()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            IETagHandler etagHandler = new Mock<IETagHandler>().Object;

            // Act
            config.SetETagHandler(etagHandler);

            // Assert
            Assert.Same(etagHandler, config.GetETagHandler());
        }

        [Fact]
        public void GetTimeZoneInfo_ReturnsLocalTimeZone_IfNotSet()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            TimeZoneInfo timeZoneInfo = config.GetTimeZoneInfo();

            // Assert
            Assert.Same(TimeZoneInfo.Local, timeZoneInfo);
        }

        [Fact]
        public void SetTimeZoneInfo_ReturnsTimeZoneInfo_UsingSetTimeZoneInfo()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Utc;

            // Act
            config.SetTimeZoneInfo(timeZoneInfo);

            // Assert
            Assert.Same(timeZoneInfo, config.GetTimeZoneInfo());
        }

        [Fact]
        public void GetResolverSettings_ReturnDefaultResolverSettings_IfNotSet()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            ODataUriResolverSetttings resolverSetttings = config.GetResolverSettings();

            // Assert
            Assert.False(resolverSetttings.CaseInsensitive);
            Assert.False(resolverSetttings.UnqualifiedNameCall);
            Assert.False(resolverSetttings.EnumPrefixFree);
        }

        [Fact]
        public void EnableCaseInsensitive_Sets_KeyWordAndMetadataFlag()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.EnableCaseInsensitive(caseInsensitive: true);
            ODataUriResolverSetttings resolverSetttings = config.GetResolverSettings();

            // Assert
            Assert.True(resolverSetttings.CaseInsensitive);
        }

        [Fact]
        public void EnableUnqualifedCall_Sets_UnqualifedCallFlag()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.EnableUnqualifiedNameCall(unqualifiedNameCall: true);
            ODataUriResolverSetttings resolverSetttings = config.GetResolverSettings();

            // Assert
            Assert.True(resolverSetttings.UnqualifiedNameCall);
        }

        [Fact]
        public void EnableEnumPrefixFree_Sets_EnumPrefixFreeFlag()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.EnableEnumPrefixFree(enumPrefixFree: true);
            ODataUriResolverSetttings resolverSetttings = config.GetResolverSettings();

            // Assert
            Assert.True(resolverSetttings.EnumPrefixFree);
        }

        [Fact]
        public void EnableAlternateKeys_Sets_AlternateKeyFlag()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.EnableAlternateKeys(true);
            ODataUriResolverSetttings resolverSetttings = config.GetResolverSettings();

            // Assert
            Assert.True(resolverSetttings.AlternateKeys);
        }

        [Fact]
        public void EnableContinueOnError_Sets_ContinueOnErrorKeyFlag()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.EnableContinueOnErrorHeader();

            // Assert
            Assert.True(config.HasEnabledContinueOnErrorHeader());
        }

        [Fact]
        public void SetUrlConvension_Sets_UrlConvension()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.SetUrlConventions(ODataUrlConventions.ODataSimplified);
            ODataRoute route = config.MapODataServiceRoute("odata", "odata", new EdmModel());
            var pathHandler = route.PathRouteConstraint.PathHandler as DefaultODataPathHandler;

            // Assert
            Assert.NotNull(pathHandler);
            Assert.Equal(pathHandler.ResolverSetttings.UrlConventions, ODataUrlConventions.ODataSimplified);
        }

        [Fact]
        public void SetUrlConvension_Sets_DefaultValue()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            ODataRoute route = config.MapODataServiceRoute("odata", "odata", new EdmModel());
            var pathHandler = route.PathRouteConstraint.PathHandler as DefaultODataPathHandler;

            // Assert
            Assert.NotNull(pathHandler);
            Assert.Equal(pathHandler.ResolverSetttings.UrlConventions, ODataUrlConventions.Default);
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
