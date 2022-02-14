//-----------------------------------------------------------------------------
// <copyright file="HttpConfigurationExtensionsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Services;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;

namespace Microsoft.AspNet.OData.Test
{
    [Collection("TimeZoneTests")] // TimeZoneInfo is not thread-safe. Tests in this collection will be executed sequentially 
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
            Assert.Single(queryFilterProviders);
            var queryAttribute = Assert.IsType<EnableQueryAttribute>(queryFilterProviders.First().QueryFilter);
        }

        [Fact]
        public void AddQuerySupport_AddsFilterProviderForQueryFilter()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            Mock<IActionFilter> myQueryFilter = new Mock<IActionFilter>();

            configuration.AddODataQueryFilter(myQueryFilter.Object);

            var queryFilterProviders = configuration.Services.GetFilterProviders().OfType<QueryFilterProvider>();
            Assert.Single(queryFilterProviders);
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

            Assert.Single(filters);
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
        public void SetUrlKeyDelimiter_Sets_UrlKeyDelimiter()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.SetUrlKeyDelimiter(ODataUrlKeyDelimiter.Slash);
            config.MapODataServiceRoute("odata", "odata", new EdmModel());
            var pathResolver = GetPathHandler(config);

            // Assert
            Assert.NotNull(pathResolver);
            Assert.Equal(pathResolver.UrlKeyDelimiter, ODataUrlKeyDelimiter.Slash);
        }

        [Fact]
        public void SetUrlKeyDelimiter_Sets_DefaultValue()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.MapODataServiceRoute("odata", "odata", new EdmModel());
            var pathResolver = GetPathHandler(config);

            // Assert
            Assert.NotNull(pathResolver);
            Assert.Null(pathResolver.UrlKeyDelimiter);
        }

        [Fact]
        public void ConfigureServices_ImplicitlySets_RootContainer()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.MapODataServiceRoute("odata", "odata", builder =>
                builder.AddService<IEdmModel, EdmModel>(ServiceLifetime.Singleton)
                       .AddService<ITestService, TestService2>(ServiceLifetime.Singleton));
            IServiceProvider rootContainer = config.GetODataRootContainer("odata");
            ITestService o1 = rootContainer.GetRequiredService<ITestService>();
            ITestService o2 = rootContainer.GetRequiredService<ITestService>();

            // Assert
            Assert.Equal(o1, o2);
        }

        [Fact]
        public void ConfigureServices_Using_MapHttpRoute()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.EnableDependencyInjection(builder =>
                builder.AddService<IEdmModel, EdmModel>(ServiceLifetime.Singleton)
                       .AddService<ITestService, TestService2>(ServiceLifetime.Singleton));
            config.Routes.MapHttpRoute("odata", "odata");
            IServiceProvider rootContainer = config.GetNonODataRootContainer();
            ITestService o1 = rootContainer.GetRequiredService<ITestService>();
            ITestService o2 = rootContainer.GetRequiredService<ITestService>();

            // Assert
            Assert.Equal(o1, o2);
        }

        [Fact]
        public void ConfigureServices_Throws_WhenNoODataRoute()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            Action action = () => config.GetODataRootContainer("odata");

            // Assert
            ExceptionAssert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void ConfigureServices_Throws_WhenDependencyInjectionNotEnabled()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            Action action = () => config.GetNonODataRootContainer();

            // Assert
            ExceptionAssert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void ConfigureServices_CanMap_TwoDifferentODataRoutes()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.MapODataServiceRoute("odata1", "odata1", builder =>
                builder.AddService<IEdmModel, EdmModel>(ServiceLifetime.Singleton)
                       .AddService<ITestService, TestService2>(ServiceLifetime.Singleton));
            config.MapODataServiceRoute("odata2", "odata2", builder =>
                builder.AddService<IEdmModel, EdmModel>(ServiceLifetime.Singleton)
                       .AddService<ITestService, TestService2>(ServiceLifetime.Singleton));
            IServiceProvider rootContainer1 = config.GetODataRootContainer("odata1");
            IServiceProvider rootContainer2 = config.GetODataRootContainer("odata2");
            ITestService o1 = rootContainer1.GetRequiredService<ITestService>();
            ITestService o2 = rootContainer2.GetRequiredService<ITestService>();

            // Assert
            Assert.NotEqual(o1, o2);
        }

        [Fact]
        public void ConfigureServices_CanMap_OneODataRoute_And_OneHttpRoute()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.MapODataServiceRoute("odata", "odata", builder =>
                builder.AddService<IEdmModel, EdmModel>(ServiceLifetime.Singleton)
                       .AddService<ITestService, TestService2>(ServiceLifetime.Singleton));
            config.EnableDependencyInjection(builder =>
                builder.AddService<IEdmModel, EdmModel>(ServiceLifetime.Singleton)
                       .AddService<ITestService, TestService2>(ServiceLifetime.Singleton));
            config.Routes.MapHttpRoute("odata2", "odata2");
            IServiceProvider rootContainer = config.GetODataRootContainer("odata");
            IServiceProvider nonODataRootContainer = config.GetNonODataRootContainer();
            ITestService o1 = rootContainer.GetRequiredService<ITestService>();
            ITestService o2 = nonODataRootContainer.GetRequiredService<ITestService>();

            // Assert
            Assert.NotEqual(o1, o2);
        }

        [Fact]
        public void ConfigureServices_CanMap_TwoDifferentHttpRoutes()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.EnableDependencyInjection(builder =>
                builder.AddService<IEdmModel, EdmModel>(ServiceLifetime.Singleton)
                       .AddService<ITestService, TestService2>(ServiceLifetime.Singleton));
            config.Routes.MapHttpRoute("odata1", "odata1");
            config.Routes.MapHttpRoute("odata2", "odata2");
            IServiceProvider nonODataRootContainer = config.GetNonODataRootContainer();
            ITestService o1 = nonODataRootContainer.GetRequiredService<ITestService>();
            ITestService o2 = nonODataRootContainer.GetRequiredService<ITestService>();

            // Assert
            Assert.Equal(o1, o2);
        }

        [Fact]
        public void ConfigureServices_CanSet_CustomContainer()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.UseCustomContainerBuilder(() => new DerivedContainerBuilder());
            config.MapODataServiceRoute("odata", "odata", builder =>
                builder.AddService<IEdmModel, EdmModel>(ServiceLifetime.Singleton)
                       .AddService<ITestService, TestService>(ServiceLifetime.Singleton));
            IServiceProvider rootContainer = config.GetODataRootContainer("odata");
            ITestService testService = rootContainer.GetRequiredService<ITestService>();

            // Assert
            Assert.Equal(typeof(TestService2), testService.GetType());
        }

        [Fact]
        public void ConfigureServices_CanSet_QueryConfiguration()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.Filter().Count(QueryOptionSetting.Disabled).Expand().OrderBy().MaxTop(10);
            DefaultQuerySettings defaultQuerySettings = config.GetDefaultQuerySettings();

            // Assert
            Assert.True(defaultQuerySettings.EnableFilter);
            Assert.False(defaultQuerySettings.EnableCount);
            Assert.True(defaultQuerySettings.EnableExpand);
            Assert.True(defaultQuerySettings.EnableOrderBy);
            Assert.Equal(10, defaultQuerySettings.MaxTop);
        }

        private static IODataPathHandler GetPathHandler(HttpConfiguration config)
        {
            return config.GetODataRootContainer("odata").GetRequiredService<IODataPathHandler>();
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

        private class DerivedContainerBuilder : DefaultContainerBuilder
        {
            public override IContainerBuilder AddService(
                ServiceLifetime lifetime,
                Type serviceType,
                Type implementationType)
            {
                if (serviceType == typeof(ITestService))
                {
                    return base.AddService(lifetime, serviceType, typeof(TestService2));
                }

                return base.AddService(lifetime, serviceType, implementationType);
            }
        }

        private interface ITestService { }

        private class TestService : ITestService { }

        private class TestService2 : ITestService { }
    }
}
#endif
