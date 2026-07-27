//-----------------------------------------------------------------------------
// <copyright file="EnableQueryAttributeTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Models;
using Microsoft.AspNet.OData.Test.Query.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;
#else
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Models;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.AspNet.OData.Test.Query.Controllers;
using Microsoft.AspNet.OData.Test.Query.Validators;
using Microsoft.AspNet.OData.Test.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#endif


namespace Microsoft.AspNet.OData.Test.Query
{
    public class EnableQueryAttributeTest
    {
        public static List<Customer> CustomerList = new List<Customer>()
        {
            new Customer(){ Name = "B" },
            new Customer(){ Name = "C" },
            new Customer(){ Name = "A" },
        };

        public static TheoryDataSet<string, object, bool> DifferentReturnTypeWorksTestData
        {
            get
            {
                return new TheoryDataSet<string, object, bool>
                {
                    { "GetObject", new List<Customer>(CustomerList), false },
                    { "GetObject", new Collection<Customer>(CustomerList), false },
                    { "GetObject", new CustomerCollection(), false }
                };
            }
        }

        public static TheoryDataSet<string> SystemQueryOptionNames
        {
            get { return ODataQueryOptionTest.SystemQueryOptionNames; }
        }

        [Fact]
        public void Ctor_Initializes_Properties()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();

            // Act & Assert
            Assert.Equal(HandleNullPropagationOption.Default, attribute.HandleNullPropagation);
            Assert.True(attribute.EnsureStableOrdering);
        }

        [Fact]
        public void EnsureStableOrdering_Property_RoundTrips()
        {
            ReflectionAssert.BooleanProperty<EnableQueryAttribute>(
                new EnableQueryAttribute(),
                o => o.EnsureStableOrdering,
                true);
        }

        [Fact]
        public void HandleNullPropagation_Property_RoundTrips()
        {
            ReflectionAssert.EnumProperty<EnableQueryAttribute, HandleNullPropagationOption>(
                new EnableQueryAttribute(),
                o => o.HandleNullPropagation,
                HandleNullPropagationOption.Default,
                HandleNullPropagationOption.Default - 1,
                HandleNullPropagationOption.True);
        }

        [Fact]
        public void AllowedArithmeticOperators_Property_RoundTrips()
        {
            ReflectionAssert.EnumProperty<EnableQueryAttribute, AllowedArithmeticOperators>(
                new EnableQueryAttribute(),
                o => o.AllowedArithmeticOperators,
                AllowedArithmeticOperators.All,
                AllowedArithmeticOperators.None - 1,
                AllowedArithmeticOperators.Multiply);
        }

        [Fact]
        public void AllowedFunctions_Property_RoundTrips()
        {
            ReflectionAssert.EnumProperty<EnableQueryAttribute, AllowedFunctions>(
                new EnableQueryAttribute(),
                o => o.AllowedFunctions,
                AllowedFunctions.AllFunctions,
                AllowedFunctions.None - 1,
                AllowedFunctions.All);
        }

        [Fact]
        public void AllowedLogicalOperators_Property_RoundTrips()
        {
            ReflectionAssert.EnumProperty<EnableQueryAttribute, AllowedLogicalOperators>(
                new EnableQueryAttribute(),
                o => o.AllowedLogicalOperators,
                AllowedLogicalOperators.All,
                AllowedLogicalOperators.None - 1,
                AllowedLogicalOperators.GreaterThanOrEqual);
        }

        [Fact]
        public void EnableConstantParameterization_Property_RoundTrips()
        {
            ReflectionAssert.BooleanProperty(
                new EnableQueryAttribute(),
                o => o.EnableConstantParameterization,
                expectedDefaultValue: true);
        }

        [Fact]
        public void EnableCorrelatedSubqueryBuffering_Property_RoundTrips()
        {
            ReflectionAssert.BooleanProperty(
                new EnableQueryAttribute(),
                o => o.EnableCorrelatedSubqueryBuffering,
                expectedDefaultValue: false);
        }

        [Fact]
        public void AllowedQueryOptions_Property_RoundTrips()
        {
            ReflectionAssert.EnumProperty<EnableQueryAttribute, AllowedQueryOptions>(
                new EnableQueryAttribute(),
                o => o.AllowedQueryOptions,
                AllowedQueryOptions.Supported,
                AllowedQueryOptions.None - 1,
                AllowedQueryOptions.All);
        }

        [Fact]
        public void AllowedOrderByProperties_Property_RoundTrips()
        {
            ReflectionAssert.StringProperty<EnableQueryAttribute>(
                new EnableQueryAttribute(),
                o => o.AllowedOrderByProperties,
                expectedDefaultValue: null,
                allowNullAndEmpty: true,
                treatNullAsEmpty: false);
        }

        [Fact]
        public void MaxAnyAllExpressionDepth_Property_RoundTrips()
        {
            ReflectionAssert.IntegerProperty<EnableQueryAttribute, int>(
                new EnableQueryAttribute(),
                o => o.MaxAnyAllExpressionDepth,
                expectedDefaultValue: 1,
                minLegalValue: 1,
                illegalLowerValue: -1,
                illegalUpperValue: null,
                maxLegalValue: int.MaxValue,
                roundTripTestValue: 2);
        }

        [Fact]
        public void MaxNodeCount_Property_RoundTrips()
        {
            ReflectionAssert.IntegerProperty<EnableQueryAttribute, int>(
                new EnableQueryAttribute(),
                o => o.MaxNodeCount,
                expectedDefaultValue: 100,
                minLegalValue: 1,
                maxLegalValue: int.MaxValue,
                illegalLowerValue: 0,
                illegalUpperValue: null,
                roundTripTestValue: 2);
        }

        [Fact]
        public void PageSize_Property_RoundTrips()
        {
            ReflectionAssert.IntegerProperty<EnableQueryAttribute, int>(
                new EnableQueryAttribute(),
                o => o.PageSize,
                expectedDefaultValue: 0,
                minLegalValue: 1,
                illegalLowerValue: 0,
                illegalUpperValue: null,
                maxLegalValue: int.MaxValue,
                roundTripTestValue: 2);
        }

        [Fact]
        public void MaxExpansionDepth_Property_RoundTrips()
        {
            ReflectionAssert.IntegerProperty(
                new EnableQueryAttribute(),
                o => o.MaxExpansionDepth,
                expectedDefaultValue: 2,
                minLegalValue: 0,
                illegalLowerValue: -1,
                illegalUpperValue: null,
                maxLegalValue: int.MaxValue,
                roundTripTestValue: 100);
        }

        [Fact]
        public void MaxOrderByNodeCount_Property_RoundTrips()
        {
            ReflectionAssert.IntegerProperty(
                new EnableQueryAttribute(),
                o => o.MaxOrderByNodeCount,
                expectedDefaultValue: 5,
                minLegalValue: 1,
                illegalLowerValue: -1,
                illegalUpperValue: null,
                maxLegalValue: int.MaxValue,
                roundTripTestValue: 100);
        }

        [Fact]
        public void OnActionExecuted_Throws_Null_Context()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new EnableQueryAttribute().OnActionExecuted(null), "actionExecutedContext");
        }
#if NETCORE // Following functionality is only supported in NetCore.
        [Fact]
        public void OnActionExecuted_HandlesStatusCodesCorrectly()
        {
            // Arrange
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "Get";
            ActionDescriptor actionDescriptor = new ActionDescriptor();
            ActionContext actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

            ActionExecutedContext context = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), "someController");
            context.Result = new ObjectResult(new { Error = "Error", Message = "Message" }) { StatusCode = 500 };

            EnableQueryAttribute attribute = new EnableQueryAttribute();

            // Act and Assert
            ExceptionAssert.DoesNotThrow(() => attribute.OnActionExecuted(context));
        }

        [Fact]
        public void OnActionExecuted_HandlesRequestsNormally()
        {
            // Arrange
            var routeName = "odata";
            IEdmModel model = new CustomersModelWithInheritance().Model;
            var configuration = RoutingConfigurationFactory.Create();

            configuration.Filter();

            var request = RequestFactory.CreateFromModel(model, "http://localhost/odata/Customers?$filter=Id eq 1", routeName, new ODataPath());

            IServiceProvider serviceProvider = GetServiceProvider(configuration, model, routeName);
            request.ODataFeature().RequestContainer = serviceProvider;
            HttpContext httpContext = request.HttpContext;
            httpContext.RequestServices = serviceProvider;

            ActionDescriptor actionDescriptor = ControllerDescriptorFactory
                                                .Create(configuration, "CustomersController", typeof(CustomersController))
                                                .First(descriptor => descriptor.ActionName.StartsWith("Get", StringComparison.OrdinalIgnoreCase));
            ActionContext actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

            ActionExecutedContext context = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new CustomersController());
            context.Result = new ObjectResult(new List<Customer>()) { StatusCode = 200 };

            EnableQueryAttribute attribute = new EnableQueryAttribute();

            // Act and Assert
            ExceptionAssert.DoesNotThrow(() => attribute.OnActionExecuted(context));

            Assert.NotNull(context.Result as ObjectResult);
        }

        private IServiceProvider GetServiceProvider(IRouteBuilder builder, IEdmModel model, string routeName)
        {
            IPerRouteContainer perRouteContainer = builder.ServiceProvider.GetRequiredService<IPerRouteContainer>();

            // Create an service provider for this route. Add the default services to the custom configuration actions.
            Action<IContainerBuilder> builderAction = ODataRouteBuilderExtensions.ConfigureDefaultServices(builder, b =>
            {
                b.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => model);
            });
            return perRouteContainer.CreateODataRootContainer(routeName, builderAction);
        }

        [Fact]
        public void OnActionExecuting_Throws_Null_Context()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new EnableQueryAttribute().OnActionExecuting(null), "context");
        }

        #region Query validation error logging (opt-in diagnostics)

        [Fact]
        public void EnableQueryValidationErrorLogging_DefaultsToFalse()
        {
            // Arrange & Act
            var attribute = new EnableQueryAttribute();

            // Assert
            Assert.False(attribute.EnableQueryValidationErrorLogging);
        }

        [Fact]
        public void OnActionExecuting_LoggingEnabled_UnknownSelect_WritesDiagnostic()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var context = CreateQueryValidationActionExecutingContext("?$select=NoSuchProperty", BuildLoggerServices(loggerProvider));

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            var entry = Assert.Single(loggerProvider.Entries);
            Assert.Equal(LogLevel.Warning, entry.Level);
            Assert.Equal(typeof(EnableQueryAttribute).FullName, entry.Category);
            Assert.Contains("Customer", entry.GetFieldValue("QueryType"));
            Assert.Equal("$select=NoSuchProperty", entry.GetFieldValue("QueryOptions"));
            Assert.Contains("NoSuchProperty", entry.GetFieldValue("Reason"));
            Assert.NotNull(entry.Exception);
            Assert.Contains("NoSuchProperty", entry.Exception.Message);
        }

        [Fact]
        public void OnActionExecuting_LoggingEnabled_UnknownExpand_WritesDiagnostic()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var context = CreateQueryValidationActionExecutingContext("?$expand=NoSuchNavigation", BuildLoggerServices(loggerProvider));

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            var entry = Assert.Single(loggerProvider.Entries);
            Assert.Equal(LogLevel.Warning, entry.Level);
            Assert.Equal("$expand=NoSuchNavigation", entry.GetFieldValue("QueryOptions"));
            Assert.Contains("NoSuchNavigation", entry.GetFieldValue("Reason"));
            Assert.NotNull(entry.Exception);
        }

        [Fact]
        public void OnActionExecuting_LoggingEnabled_ReportsFullRequestedSelectSet()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var context = CreateQueryValidationActionExecutingContext("?$select=Name,NoSuchProperty", BuildLoggerServices(loggerProvider));

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            var entry = Assert.Single(loggerProvider.Entries);
            Assert.Equal("$select=Name,NoSuchProperty", entry.GetFieldValue("QueryOptions"));
        }

        [Fact]
        public void OnActionExecuting_LoggingEnabled_UnknownExpandWithSelect_ReportsCombinedQueryOptions()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var context = CreateQueryValidationActionExecutingContext("?$select=Name&$expand=NoSuchNavigation", BuildLoggerServices(loggerProvider));

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            var entry = Assert.Single(loggerProvider.Entries);
            Assert.Equal("$select=Name&$expand=NoSuchNavigation", entry.GetFieldValue("QueryOptions"));
            Assert.Contains("NoSuchNavigation", entry.GetFieldValue("Reason"));
        }

        [Fact]
        public void OnActionExecuting_LoggingEnabled_NestedExpandSelect_UnknownProperty_WritesDiagnostic()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var context = CreateQueryValidationActionExecutingContext(
                "?$expand=Orders($select=NoSuchOrderProperty)",
                BuildLoggerServices(loggerProvider),
                collectionEndpoint: true,
                model: GetLoggingCustomerOrdersModel());

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            var entry = Assert.Single(loggerProvider.Entries);
            Assert.Equal(LogLevel.Warning, entry.Level);
            Assert.Equal("odata/Customers", entry.GetFieldValue("Endpoint"));
            Assert.Equal("$expand=Orders($select=NoSuchOrderProperty)", entry.GetFieldValue("QueryOptions"));
            Assert.Contains("NoSuchOrderProperty", entry.GetFieldValue("Reason"));
            Assert.NotNull(entry.Exception);
        }

        [Fact]
        public void OnActionExecuting_LoggingEnabled_NestedExpandFilter_UnknownProperty_WritesDiagnostic()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var context = CreateQueryValidationActionExecutingContext(
                "?$expand=Orders($filter=NoSuchOrderProperty eq 1)",
                BuildLoggerServices(loggerProvider),
                collectionEndpoint: true,
                model: GetLoggingCustomerOrdersModel());

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            var entry = Assert.Single(loggerProvider.Entries);
            Assert.Equal("odata/Customers", entry.GetFieldValue("Endpoint"));
            Assert.Equal("$expand=Orders($filter=NoSuchOrderProperty eq 1)", entry.GetFieldValue("QueryOptions"));
            Assert.Contains("NoSuchOrderProperty", entry.GetFieldValue("Reason"));
            Assert.NotNull(entry.Exception);
        }

        [Fact]
        public void OnActionExecuting_LoggingEnabled_AnyLambdaFilter_UnknownProperty_WritesDiagnostic()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var context = CreateQueryValidationActionExecutingContext(
                "?$filter=Orders/any(o: o/NoSuchOrderProperty eq 1)",
                BuildLoggerServices(loggerProvider),
                collectionEndpoint: true,
                model: GetLoggingCustomerOrdersModel());

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            var entry = Assert.Single(loggerProvider.Entries);
            Assert.Equal("odata/Customers", entry.GetFieldValue("Endpoint"));
            Assert.Contains("NoSuchOrderProperty", entry.GetFieldValue("Reason"));
            Assert.NotNull(entry.Exception);
        }

        [Fact]
        public void OnActionExecuting_LoggingEnabled_AllLambdaFilter_UnknownProperty_WritesDiagnostic()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var context = CreateQueryValidationActionExecutingContext(
                "?$filter=Orders/all(o: o/NoSuchOrderProperty eq 1)",
                BuildLoggerServices(loggerProvider),
                collectionEndpoint: true,
                model: GetLoggingCustomerOrdersModel());

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            var entry = Assert.Single(loggerProvider.Entries);
            Assert.Equal("odata/Customers", entry.GetFieldValue("Endpoint"));
            Assert.Contains("NoSuchOrderProperty", entry.GetFieldValue("Reason"));
            Assert.NotNull(entry.Exception);
        }

#if !NETCOREAPP2_1
        [Fact]
        public void OnActionExecuting_LoggingEnabled_RoutedEndpoint_ReportsRouteTemplate()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var endpoint = new RouteEndpoint(
                ctx => System.Threading.Tasks.Task.CompletedTask,
                Microsoft.AspNetCore.Routing.Patterns.RoutePatternFactory.Parse("odata/Customers({key})"),
                order: 0,
                metadata: EndpointMetadataCollection.Empty,
                displayName: "Customers");
            var context = CreateQueryValidationActionExecutingContext("?$select=NoSuchProperty", BuildLoggerServices(loggerProvider), endpoint);

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            var entry = Assert.Single(loggerProvider.Entries);
            Assert.Equal("odata/Customers({key})", entry.GetFieldValue("Endpoint"));
        }
#endif

#if NETCOREAPP2_1
        [Fact]
        public void OnActionExecuting_LoggingEnabled_RoutedEndpoint_ReportsRouteTemplate()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var context = CreateQueryValidationActionExecutingContext("?$select=NoSuchProperty", BuildLoggerServices(loggerProvider), routedEndpoint: true);

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            var entry = Assert.Single(loggerProvider.Entries);
            Assert.Equal("odata/Customers({key})", entry.GetFieldValue("Endpoint"));
        }
#endif

#if !NETCOREAPP2_1
        [Fact]
        public void OnActionExecuting_LoggingEnabled_NavigationAfterKey_ReportsRouteTemplate()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var endpoint = new RouteEndpoint(
                ctx => System.Threading.Tasks.Task.CompletedTask,
                Microsoft.AspNetCore.Routing.Patterns.RoutePatternFactory.Parse("odata/Customers({key})/Orders"),
                order: 0,
                metadata: EndpointMetadataCollection.Empty,
                displayName: "CustomerOrders");
            var context = CreateQueryValidationActionExecutingContext("?$select=NoSuchProperty", BuildLoggerServices(loggerProvider), endpoint);

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            var entry = Assert.Single(loggerProvider.Entries);
            Assert.Equal("odata/Customers({key})/Orders", entry.GetFieldValue("Endpoint"));
        }
#endif

#if NETCOREAPP2_1
        [Fact]
        public void OnActionExecuting_LoggingEnabled_NavigationAfterKey_ReportsRouteTemplate()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var model = GetLoggingCustomerOrdersModel();
            var context = CreateQueryValidationActionExecutingContext("?$select=NoSuchProperty", BuildLoggerServices(loggerProvider), model: model);
            var odataFeature = context.HttpContext.ODataFeature();
            odataFeature.Path = BuildCustomersOrdersPath(model, includeOrderKey: false);
            odataFeature.RoutePrefix = "odata";

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            var entry = Assert.Single(loggerProvider.Entries);
            Assert.Equal("odata/Customers({key})/Orders", entry.GetFieldValue("Endpoint"));
        }

        [Fact]
        public void OnActionExecuting_LoggingEnabled_KeyOnNavigation_ReportsRouteTemplate()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var model = GetLoggingCustomerOrdersModel();
            var context = CreateQueryValidationActionExecutingContext("?$select=NoSuchProperty", BuildLoggerServices(loggerProvider), model: model);
            var odataFeature = context.HttpContext.ODataFeature();
            odataFeature.Path = BuildCustomersOrdersPath(model, includeOrderKey: true);
            odataFeature.RoutePrefix = "odata";

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            var entry = Assert.Single(loggerProvider.Entries);
            Assert.Equal("odata/Customers({key})/Orders({key})", entry.GetFieldValue("Endpoint"));
        }

        [Fact]
        public void OnActionExecuting_LoggingEnabled_NoRoutePrefix_ReportsRouteTemplate()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var model = GetLoggingCustomerOrdersModel();
            var context = CreateQueryValidationActionExecutingContext("?$select=NoSuchProperty", BuildLoggerServices(loggerProvider), model: model);
            var odataFeature = context.HttpContext.ODataFeature();
            odataFeature.Path = BuildCustomersOrdersPath(model, includeOrderKey: false);
            odataFeature.RoutePrefix = null;

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            var entry = Assert.Single(loggerProvider.Entries);
            Assert.Equal("Customers({key})/Orders", entry.GetFieldValue("Endpoint"));
        }
#endif

        [Fact]
        public void OnActionExecuting_DefaultConfiguration_WritesNothing()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute();
            var context = CreateQueryValidationActionExecutingContext("?$select=NoSuchProperty", BuildLoggerServices(loggerProvider));

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            Assert.Empty(loggerProvider.Entries);
        }

        [Fact]
        public void OnActionExecuting_GlobalOptionEnabled_WritesDiagnostic()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute();
            var context = CreateQueryValidationActionExecutingContext(
                "?$select=NoSuchProperty",
                BuildLoggerServicesWithGlobalOption(loggerProvider, globalEnable: true));

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            var entry = Assert.Single(loggerProvider.Entries);
            Assert.Equal(LogLevel.Warning, entry.Level);
            Assert.Contains("NoSuchProperty", entry.GetFieldValue("Reason"));
        }

        [Fact]
        public void OnActionExecuting_GlobalEnabled_AttributeOptOut_WritesNothing()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = false };
            var context = CreateQueryValidationActionExecutingContext(
                "?$select=NoSuchProperty",
                BuildLoggerServicesWithGlobalOption(loggerProvider, globalEnable: true));

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            Assert.Empty(loggerProvider.Entries);
        }

        [Fact]
        public void OnActionExecuting_GlobalDisabled_AttributeEnabled_WritesDiagnostic()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var context = CreateQueryValidationActionExecutingContext(
                "?$select=NoSuchProperty",
                BuildLoggerServicesWithGlobalOption(loggerProvider, globalEnable: false));

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            Assert.Single(loggerProvider.Entries);
        }

        [Fact]
        public void OnActionExecuting_AttributeDisabled_WritesNothing()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = false };
            var context = CreateQueryValidationActionExecutingContext("?$select=NoSuchProperty", BuildLoggerServices(loggerProvider));

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            Assert.Empty(loggerProvider.Entries);
        }

        [Fact]
        public void OnActionExecuting_LoggingEnabled_ValidSelect_WritesNothing()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var context = CreateQueryValidationActionExecutingContext("?$select=Name", BuildLoggerServices(loggerProvider));

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.Null(context.Result);
            Assert.Empty(loggerProvider.Entries);
        }

        [Fact]
        public void OnActionExecuting_LoggingEnabled_NoQueryOptions_WritesNothing()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var context = CreateQueryValidationActionExecutingContext(string.Empty, BuildLoggerServices(loggerProvider));

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.Null(context.Result);
            Assert.Empty(loggerProvider.Entries);
        }

        [Fact]
        public void OnActionExecuting_LoggingEnabled_NoLoggerRegistered_DoesNotThrow_AndReturnsBadRequest()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton(new ODataOptions());
            IServiceProvider requestServices = services.BuildServiceProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var context = CreateQueryValidationActionExecutingContext("?$select=NoSuchProperty", requestServices);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => attribute.OnActionExecuting(context));
            Assert.IsType<BadRequestObjectResult>(context.Result);
        }

        [Fact]
        public void OnActionExecuting_LoggingEnabled_WarningLevelDisabled_WritesNothing()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var context = CreateQueryValidationActionExecutingContext(
                "?$select=NoSuchProperty",
                BuildLoggerServices(loggerProvider, minimumLevel: LogLevel.Error));

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            Assert.Empty(loggerProvider.Entries);
        }

        [Fact]
        public void OnActionExecuting_LoggingEnabled_CustomLogLevel_WritesAtConfiguredLevel()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var context = CreateQueryValidationActionExecutingContext(
                "?$select=NoSuchProperty",
                BuildLoggerServicesWithGlobalOption(loggerProvider, globalEnable: false, globalLevel: LogLevel.Error));

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            var entry = Assert.Single(loggerProvider.Entries);
            Assert.Equal(LogLevel.Error, entry.Level);
        }

        [Fact]
        public void OnActionExecuting_LoggingEnabled_InformationLevel_WritesAtInformation()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var context = CreateQueryValidationActionExecutingContext(
                "?$select=NoSuchProperty",
                BuildLoggerServicesWithGlobalOption(loggerProvider, globalEnable: false, globalLevel: LogLevel.Information));

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            var entry = Assert.Single(loggerProvider.Entries);
            Assert.Equal(LogLevel.Information, entry.Level);
        }

        [Fact]
        public void OnActionExecuting_GlobalEnabled_UsesGlobalLogLevel()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute();
            var context = CreateQueryValidationActionExecutingContext(
                "?$select=NoSuchProperty",
                BuildLoggerServicesWithGlobalOption(loggerProvider, globalEnable: true, globalLevel: LogLevel.Debug));

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            var entry = Assert.Single(loggerProvider.Entries);
            Assert.Equal(LogLevel.Debug, entry.Level);
        }

        [Fact]
        public void OnActionExecuting_LoggingEnabled_CustomLevelDisabled_WritesNothing()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var context = CreateQueryValidationActionExecutingContext(
                "?$select=NoSuchProperty",
                BuildLoggerServicesWithGlobalOption(loggerProvider, globalEnable: false, globalLevel: LogLevel.Information, minimumLevel: LogLevel.Warning));

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
            Assert.Empty(loggerProvider.Entries);
        }

        [Fact]
        public void OnActionExecuting_LoggingEnabled_LoggerThrows_ResponseUnchanged()
        {
            // Arrange
            var throwingProvider = new ThrowingLoggerProvider(typeof(EnableQueryAttribute).FullName);
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };
            var context = CreateQueryValidationActionExecutingContext(
                "?$select=NoSuchProperty",
                BuildThrowingLoggerServices(throwingProvider));

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => attribute.OnActionExecuting(context));
            Assert.IsType<BadRequestObjectResult>(context.Result);
        }

        [Fact]
        public void OnActionExecuted_LoggingEnabled_PostActionValidationFailure_WritesDiagnostic()
        {
            // Arrange
            var loggerProvider = new CapturingLoggerProvider();
            var attribute = new EnableQueryAttribute { EnableQueryValidationErrorLogging = true };

            // Seed the per-request state, but clear the OData path so the pre-action validation is skipped and the
            // query is instead validated after the action runs - the path taken for IActionResult/SingleResult
            // actions whose element type is only known once the result is produced.
            var executingContext = CreateQueryValidationActionExecutingContext("?$select=NoSuchProperty", BuildLoggerServices(loggerProvider));
            executingContext.HttpContext.Request.ODataFeature().Path = null;
            attribute.OnActionExecuting(executingContext);
            Assert.Null(executingContext.Result);
            Assert.Empty(loggerProvider.Entries);

            // The controller result whose element type (Customer) becomes known only now drives post-action
            // validation. The element type must match the model registered on the request (the logging model is
            // built from the TestModels Customer), so the query is validated against that type instead of falling
            // back to a model built from the action descriptor.
            HttpContext httpContext = executingContext.HttpContext;
            ActionDescriptor actionDescriptor = ControllerDescriptorFactory
                .Create(RoutingConfigurationFactory.Create(), "CustomersController", typeof(CustomersController))
                .First(descriptor => descriptor.ActionName.StartsWith("Get", StringComparison.OrdinalIgnoreCase));
            var customers = new List<Microsoft.AspNet.OData.Test.Builder.TestModels.Customer>
            {
                new Microsoft.AspNet.OData.Test.Builder.TestModels.Customer { Name = "Anne" }
            }.AsQueryable();
            var executedContext = new ActionExecutedContext(
                new ActionContext(httpContext, new RouteData(), actionDescriptor),
                new List<IFilterMetadata>(),
                new CustomersController())
            {
                Result = new ObjectResult(customers) { StatusCode = 200 }
            };

            // Act
            attribute.OnActionExecuted(executedContext);

            // Assert
            Assert.IsType<BadRequestObjectResult>(executedContext.Result);
            var entry = Assert.Single(loggerProvider.Entries);
            Assert.Equal(LogLevel.Warning, entry.Level);
            Assert.Equal(typeof(EnableQueryAttribute).FullName, entry.Category);
            Assert.Contains("Customer", entry.GetFieldValue("QueryType"));
            Assert.Equal("$select=NoSuchProperty", entry.GetFieldValue("QueryOptions"));
            Assert.Contains("NoSuchProperty", entry.GetFieldValue("Reason"));
            Assert.NotNull(entry.Exception);
        }

        private static IEdmModel GetLoggingCustomerModel()
        {
            return new ODataModelBuilder().Add_Customers_EntitySet().GetEdmModel();
        }

        private static IEdmModel GetLoggingCustomerOrdersModel()
        {
            // Customer.Orders is a bound navigation to the Orders set, so the outer clause binds and only the nested
            // clause (a $select/$filter on Order, or an any/all lambda over Orders) fails validation.
            return new ODataModelBuilder()
                .Add_Order_EntityType()
                .Add_Customers_EntitySet()
                .Add_Orders_EntitySet()
                .Add_CustomerOrders_Relationship()
                .Add_CustomerOrders_Binding()
                .GetEdmModel();
        }

#if NETCOREAPP2_1
        // Builds a multi-segment OData path (Customers(1)/Orders[(5)]) for endpoint-template reconstruction tests.
        private static ODataPath BuildCustomersOrdersPath(IEdmModel model, bool includeOrderKey)
        {
            var customers = model.EntityContainer.FindEntitySet("Customers");
            var orders = model.EntityContainer.FindEntitySet("Orders");
            var ordersProperty = customers.EntityType().FindProperty("Orders") as IEdmNavigationProperty;

            var entitySetSegment = new Microsoft.OData.UriParser.EntitySetSegment(customers);
            var customerKeySegment = new Microsoft.OData.UriParser.KeySegment(
                new[] { new KeyValuePair<string, object>("CustomerId", 1) },
                customers.EntityType(),
                customers);
            var ordersSegment = new Microsoft.OData.UriParser.NavigationPropertySegment(ordersProperty, orders);

            if (includeOrderKey)
            {
                var orderKeySegment = new Microsoft.OData.UriParser.KeySegment(
                    new[] { new KeyValuePair<string, object>("OrderId", 5) },
                    orders.EntityType(),
                    orders);
                return new ODataPath(entitySetSegment, customerKeySegment, ordersSegment, orderKeySegment);
            }

            return new ODataPath(entitySetSegment, customerKeySegment, ordersSegment);
        }
#endif

        private static IServiceProvider BuildLoggerServices(CapturingLoggerProvider loggerProvider)
        {
            return BuildLoggerServices(loggerProvider, LogLevel.Trace, null);
        }

        private static IServiceProvider BuildLoggerServices(CapturingLoggerProvider loggerProvider, LogLevel minimumLevel)
        {
            return BuildLoggerServices(loggerProvider, minimumLevel, null);
        }

        private static IServiceProvider BuildLoggerServices(CapturingLoggerProvider loggerProvider, LogLevel minimumLevel, ODataOptions odataOptions)
        {
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(minimumLevel);
                builder.AddProvider(loggerProvider);
            });
            services.AddSingleton(odataOptions ?? new ODataOptions());
            return services.BuildServiceProvider();
        }

        private static IServiceProvider BuildLoggerServicesWithGlobalOption(CapturingLoggerProvider loggerProvider, bool globalEnable, LogLevel? globalLevel = null, LogLevel minimumLevel = LogLevel.Trace)
        {
            var odataOptions = new ODataOptions
            {
                EnableQueryValidationErrorLogging = globalEnable
            };
            if (globalLevel.HasValue)
            {
                odataOptions.QueryValidationErrorLogLevel = globalLevel.Value;
            }

            return BuildLoggerServices(loggerProvider, minimumLevel, odataOptions);
        }

        private static IServiceProvider BuildThrowingLoggerServices(ThrowingLoggerProvider loggerProvider)
        {
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddProvider(loggerProvider);
            });
            services.AddSingleton(new ODataOptions());
            return services.BuildServiceProvider();
        }

#if NETCOREAPP2_1
        private ActionExecutingContext CreateQueryValidationActionExecutingContext(string queryString, IServiceProvider requestServices, bool routedEndpoint = false, bool collectionEndpoint = false, IEdmModel model = null)
#else
        private ActionExecutingContext CreateQueryValidationActionExecutingContext(string queryString, IServiceProvider requestServices, Endpoint routeEndpoint = null, bool collectionEndpoint = false, IEdmModel model = null)
#endif
        {
            var routeName = "querylogging";
            model = model ?? GetLoggingCustomerModel();

            var customers = model.EntityContainer.FindEntitySet("Customers");
            var path = new ODataPath(new Microsoft.OData.UriParser.EntitySetSegment(customers));

            var request = RequestFactory.CreateFromModel(model, "http://localhost/odata/Customers" + queryString, routeName, path);

            // Enable $select and $expand so the requested clause is bound during validation (which then reports the
            // unknown property) instead of being rejected outright as a disallowed option.
            var configuration = RoutingConfigurationFactory.Create();
            configuration.Select().Expand().Filter().OrderBy().Count();
            IServiceProvider odataContainer = GetServiceProvider(configuration, model, routeName);
            request.ODataFeature().RequestContainer = odataContainer;

            HttpContext httpContext = request.HttpContext;
            httpContext.RequestServices = requestServices;

#if !NETCOREAPP2_1
            if (routeEndpoint != null)
            {
                httpContext.SetEndpoint(routeEndpoint);
            }
            else if (collectionEndpoint)
            {
                // Collection endpoint (no key). Attach a routed endpoint whose route pattern is the collection
                // template so the diagnostic reports "odata/Customers" for a query over the Customers collection.
                httpContext.SetEndpoint(new RouteEndpoint(
                    ctx => System.Threading.Tasks.Task.CompletedTask,
                    Microsoft.AspNetCore.Routing.Patterns.RoutePatternFactory.Parse("odata/Customers"),
                    order: 0,
                    metadata: EndpointMetadataCollection.Empty,
                    displayName: "Customers"));
            }
#else
            if (routedEndpoint)
            {
                // Targets that predate endpoint routing expose the matched OData endpoint through the request's
                // ODataFeature rather than an Endpoint object. Populate the parsed path (entity set + key) and the
                // route prefix the way the classic OData router does, so the diagnostic reconstructs the same
                // "odata/Customers({key})" template produced from the endpoint route pattern on later targets.
                var keySegment = new Microsoft.OData.UriParser.KeySegment(
                    new[] { new KeyValuePair<string, object>("CustomerId", 1) },
                    customers.EntityType(),
                    customers);
                request.ODataFeature().Path = new ODataPath(new Microsoft.OData.UriParser.EntitySetSegment(customers), keySegment);
                request.ODataFeature().RoutePrefix = "odata";
            }
            else if (collectionEndpoint)
            {
                // Collection endpoint (no key). Keep the entity-set (keyless) path and just set the route prefix, so
                // the diagnostic reconstructs the collection template "odata/Customers" for the same request.
                request.ODataFeature().RoutePrefix = "odata";
            }
#endif

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), controller: new object());
        }

        #endregion
#endif

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
        [Fact]
        public void OnActionExecuted_Throws_Null_Request()
        {
            ExceptionAssert.ThrowsArgument(
                () => new EnableQueryAttribute().OnActionExecuted(new HttpActionExecutedContext()),
                "actionExecutedContext",
                String.Format("The HttpExecutedActionContext.Request is null.{0}Parameter name: actionExecutedContext", Environment.NewLine));
        }

        [Fact]
        public void OnActionExecuted_Throws_Null_Configuration()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer/?$orderby=Name");
            var config = RoutingConfigurationFactory.Create();
            HttpControllerContext controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerHighLevel", typeof(CustomerHighLevelController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(CustomerHighLevelController).GetMethod("Get"));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);

            ExceptionAssert.ThrowsArgument(
                () => new EnableQueryAttribute().OnActionExecuted(context),
                "actionExecutedContext",
                String.Format("Request message does not contain an HttpConfiguration object.{0}Parameter name: actionExecutedContext", Environment.NewLine));
        }

        [Theory]
        [MemberData(nameof(DifferentReturnTypeWorksTestData))]
        public void DifferentReturnTypeWorks(string methodName, object responseObject, bool isNoOp)
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer?$orderby=Name");
            request.EnableODataDependencyInjectionSupport();
            request.GetConfiguration().Count().OrderBy().Filter().Expand().MaxTop(null);
            HttpControllerContext controllerContext = new HttpControllerContext(request.GetConfiguration(), new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerHighLevel", typeof(CustomerHighLevelController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(CustomerHighLevelController).GetMethod(methodName));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);
            context.Response = new HttpResponseMessage(HttpStatusCode.OK);
            context.Response.Content = new ObjectContent(typeof(IEnumerable<Customer>), responseObject, new JsonMediaTypeFormatter());

            // Act and Assert
            attribute.OnActionExecuted(context);

            Assert.Equal(HttpStatusCode.OK, context.Response.StatusCode);
            Assert.True(context.Response.Content is ObjectContent);
            Assert.Equal(isNoOp, ((ObjectContent)context.Response.Content).Value == responseObject);
        }

        [Fact]
        public void CountValueReturnsAsContent_CountRequest()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                "http://localhost/DollarCountEntities(5)/StringCollectionProp/$count");
            request.ODataProperties().Path = new ODataPath(CountSegment.Instance);
            request.EnableODataDependencyInjectionSupport();
            HttpControllerContext controllerContext = new HttpControllerContext(
                request.GetConfiguration(),
                new HttpRouteData(new HttpRoute()),
                request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(
                new HttpConfiguration(),
                "DollarCountEntities",
                typeof(ODataCountTest.DollarCountEntitiesController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(
                controllerDescriptor,
                typeof(ODataCountTest.DollarCountEntitiesController).GetMethod("GetStringCollectionProp"));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);
            context.Response = new HttpResponseMessage(HttpStatusCode.OK);
            context.Response.Content = new ObjectContent(
                typeof(IEnumerable<string>),
                new[] { "123", "abc", "A1B2" },
                new JsonMediaTypeFormatter());

            // Act
            attribute.OnActionExecuted(context);

            // Assert
            Assert.Equal(HttpStatusCode.OK, context.Response.StatusCode);
            Assert.True(context.Response.Content is ObjectContent);
            Assert.Equal(3L, ((ObjectContent)context.Response.Content).Value);
        }

        [Fact]
        public void UnknownQueryNotStartingWithDollarSignWorks()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer/?select");

            // Enable DI with default resolver.
            request.EnableODataDependencyInjectionSupport("default",
                b => b.AddService(ServiceLifetime.Singleton, sp => new ODataUriResolver()));

            HttpControllerContext controllerContext = new HttpControllerContext(request.GetConfiguration(), new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerHighLevel", typeof(CustomerHighLevelController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(CustomerHighLevelController).GetMethod("Get"));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);
            context.Response = new HttpResponseMessage(HttpStatusCode.OK);
            context.Response.Content = new ObjectContent(typeof(IEnumerable<Customer>), new List<Customer>(), new JsonMediaTypeFormatter());

            // Act and Assert
            attribute.OnActionExecuted(context);

            Assert.Equal(HttpStatusCode.OK, context.Response.StatusCode);
        }

        [Fact]
        public void UnknownQueryStartingWithDollarSignThrows()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer/?$custom");
            request.EnableODataDependencyInjectionSupport();
            HttpControllerContext controllerContext = new HttpControllerContext(request.GetConfiguration(), new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerHighLevel", typeof(CustomerHighLevelController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(CustomerHighLevelController).GetMethod("Get"));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);
            context.Response = new HttpResponseMessage(HttpStatusCode.OK);
            context.Response.Content = new ObjectContent(typeof(IEnumerable<Customer>), new List<Customer>(), new JsonMediaTypeFormatter());

            // Act and Assert
            HttpResponseException errorResponse = ExceptionAssert.Throws<HttpResponseException>(() =>
                attribute.OnActionExecuted(context));

            Assert.Equal(HttpStatusCode.BadRequest, errorResponse.Response.StatusCode);
        }

        [Fact]
        public async Task NonGenericEnumerableReturnType_ReturnsBadRequest()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer/?$skip=1");
            var config = RoutingConfigurationFactory.Create();
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            request.SetConfiguration(config);
            HttpControllerContext controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerHighLevel", typeof(CustomerHighLevelController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(CustomerHighLevelController).GetMethod("GetNonGenericEnumerable"));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);
            context.Response = new HttpResponseMessage(HttpStatusCode.OK);
            context.Response.Content = new ObjectContent(typeof(IEnumerable), new NonGenericEnumerable(), new JsonMediaTypeFormatter());

            // Act
            attribute.OnActionExecuted(context);
            string responseString = await context.Response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, context.Response.StatusCode);
            Assert.Contains("The query specified in the URI is not valid. Cannot create an EDM model as the action 'EnableQueryAttribute' " +
                "on controller 'GetNonGenericEnumerable' has a return type 'CustomerHighLevel' that does not implement IEnumerable<T>.",
                responseString);
        }

        [Fact]
        public void NonObjectContentResponse_ThrowsArgumentException()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer?$skip=1");
            var config = RoutingConfigurationFactory.Create();
            request.SetConfiguration(config);
            HttpControllerContext controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "CustomerHighLevel", typeof(CustomerHighLevelController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(CustomerHighLevelController).GetMethod("GetIEnumerableOfCustomer"));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);
            context.Response = new HttpResponseMessage(HttpStatusCode.OK);
            context.Response.Content = new StreamContent(new MemoryStream());

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => attribute.OnActionExecuted(context),
                "actionExecutedContext",
                "Queries can not be applied to a response content of type 'System.Net.Http.StreamContent'. The response content must be an ObjectContent.");
        }

        [Fact]
        public void NullContentResponse_DoesNotThrow()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customer?$skip=1");
            var config = RoutingConfigurationFactory.Create();
            request.SetConfiguration(config);
            HttpControllerContext controllerContext = new HttpControllerContext(
                config,
                new HttpRouteData(new HttpRoute()),
                request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(
                new HttpConfiguration(),
                "CustomerHighLevel",
                typeof(CustomerHighLevelController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(
                controllerDescriptor,
                typeof(CustomerHighLevelController).GetMethod("GetIEnumerableOfCustomer"));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null)
            {
                Response = new HttpResponseMessage(HttpStatusCode.OK) { Content = null }
            };

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => attribute.OnActionExecuted(context));
        }

        [Theory]
        [InlineData("$top=1")]
        [InlineData("$skip=1")]
        public void Primitives_Can_Be_Used_For_Top_And_Skip(string filter)
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Primitive/?" + filter);
            request.EnableODataDependencyInjectionSupport();
            HttpControllerContext controllerContext = new HttpControllerContext(request.GetConfiguration(), new HttpRouteData(new HttpRoute()), request);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "Primitive", typeof(PrimitiveController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(PrimitiveController).GetMethod("GetIEnumerableOfInt"));
            HttpActionContext actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            HttpActionExecutedContext context = new HttpActionExecutedContext(actionContext, null);
            context.Response = new HttpResponseMessage(HttpStatusCode.OK);
            HttpContent expectedResponse = new ObjectContent(typeof(IEnumerable<int>), new List<int>(), new JsonMediaTypeFormatter());
            context.Response.Content = expectedResponse;

            // Act and Assert
            attribute.OnActionExecuted(context);
            HttpResponseMessage response = context.Response;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedResponse, response.Content);
        }

        [Fact]
        public void ValidateQuery_Throws_With_Null_Request()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            var request = RequestFactory.Create();
            request.EnableHttpDependencyInjectionSupport();
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Builder.TestModels.Customer)), request);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => attribute.ValidateQuery(null, options), "request");
        }

        [Fact]
        public void ValidateQuery_Throws_WithNullQueryOptions()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => attribute.ValidateQuery(new HttpRequestMessage(), null), "queryOptions");
        }

        [Theory]
        [InlineData("$filter=Name eq 'abc'")]
        [InlineData("$orderby=Name")]
        [InlineData("$skip=3")]
        [InlineData("$top=2")]
        public void ValidateQuery_Accepts_All_Supported_QueryNames(string query)
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?" + query);
            request.EnableHttpDependencyInjectionSupport();
            DefaultQuerySettings defaultQuerySettings = request.GetConfiguration().GetDefaultQuerySettings();
            defaultQuerySettings.EnableFilter = true;
            defaultQuerySettings.EnableOrderBy = true;
            defaultQuerySettings.MaxTop = null;

            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Builder.TestModels.Customer), null);
            var options = new ODataQueryOptions(context, request);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => attribute.ValidateQuery(request, options));
        }

        [Fact]
        public void ValidateQuery_Sends_BadRequest_For_Unrecognized_QueryNames()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?$xxx");
            request.EnableHttpDependencyInjectionSupport();
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Builder.TestModels.Customer)), request);

            // Act & Assert
            HttpResponseException responseException = ExceptionAssert.Throws<HttpResponseException>(
                                                                () => attribute.ValidateQuery(request, options));

            Assert.Equal(HttpStatusCode.BadRequest, responseException.Response.StatusCode);
        }

        [Fact]
        public void ValidateQuery_Can_Override_Base()
        {
            // Arrange
            Mock<EnableQueryAttribute> mockAttribute = new Mock<EnableQueryAttribute>();
            mockAttribute.Setup(m => m.ValidateQuery(It.IsAny<HttpRequestMessage>(), It.IsAny<ODataQueryOptions>())).Callback(() => { }).Verifiable();

            // Act & Assert
            mockAttribute.Object.ValidateQuery(null, null);
            mockAttribute.Verify();
        }

        [Fact]
        public void ApplyQuery_Throws_With_Null_Queryable()
        {
            // Arrange
            var message = RequestFactory.Create();
            message.EnableHttpDependencyInjectionSupport();
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Builder.TestModels.Customer)), message);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => attribute.ApplyQuery(null, options), "queryable");
        }

        [Fact]
        public void ApplyQuery_Throws_WithNullQueryOptions()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => attribute.ApplyQuery(CustomerList.AsQueryable(), null), "queryOptions");
        }

        [Theory]
        [InlineData("$filter=Name eq 'abc'")]
        [InlineData("$orderby=Name")]
        [InlineData("$skip=3")]
        [InlineData("$top=2")]
        public void ApplyQuery_Accepts_All_Supported_QueryNames(string query)
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?" + query);
            request.EnableHttpDependencyInjectionSupport();
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Builder.TestModels.Customer)), request);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => attribute.ApplyQuery(new List<Builder.TestModels.Customer>().AsQueryable(), options));
        }

        [Fact]
        public void ApplyQuery_Can_Override_Base()
        {
            // Arrange
            Mock<EnableQueryAttribute> mockAttribute = new Mock<EnableQueryAttribute>();
            IQueryable result = CustomerList.AsQueryable();
            mockAttribute.Setup(m => m.ApplyQuery(It.IsAny<IQueryable>(), It.IsAny<ODataQueryOptions>()))
                         .Returns(result);
            mockAttribute.CallBase = false;

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?$top=2");
            request.EnableHttpDependencyInjectionSupport();
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Builder.TestModels.Customer)), request);

            // Act & Assert
            Assert.Same(result, mockAttribute.Object.ApplyQuery(result, options));
        }

        [Theory]
        [InlineData("Id,Address")]
        [InlineData("   Id,Address  ")]
        [InlineData(" Id , Address ")]
        [InlineData("Id, Address")]
        public void OrderByDisllowedPropertiesWithSpaces(string allowedProperties)
        {
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            attribute.AllowedOrderByProperties = allowedProperties;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customers/?$orderby=Id,Name");
            request.EnableHttpDependencyInjectionSupport();
            ODataQueryOptions queryOptions = new ODataQueryOptions(ValidationTestHelper.CreateCustomerContext(false), request);

            ExceptionAssert.Throws<ODataException>(() => attribute.ValidateQuery(request, queryOptions),
                "Order by 'Name' is not allowed. To allow it, set the 'AllowedOrderByProperties' property on EnableQueryAttribute or QueryValidationSettings.");
        }

        [Theory]
        [InlineData("Id,Name")]
        [InlineData("   Id,Name  ")]
        [InlineData(" Id , Name ")]
        [InlineData("Id, Name")]
        [InlineData("")]
        [InlineData(null)]
        public void OrderByAllowedPropertiesWithSpaces(string allowedProperties)
        {
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            attribute.AllowedOrderByProperties = allowedProperties;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customers/?$orderby=Id,Name");
            var config = RoutingConfigurationFactory.Create();
            config.Count().OrderBy().Filter().Expand().MaxTop(null);
            request.SetConfiguration(config);
            request.EnableHttpDependencyInjectionSupport();

            ODataQueryContext context = ValidationTestHelper.CreateCustomerContext(false);
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

            ExceptionAssert.DoesNotThrow(() => attribute.ValidateQuery(request, queryOptions));
        }

        [Fact]
        public void GetModel_ReturnsModel_ForNoModelOnRequest()
        {
            var entityClrType = typeof(QueryCompositionCustomer);
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            request.EnableHttpDependencyInjectionSupport();
            var descriptor = new ReflectedHttpActionDescriptor();
            descriptor.Configuration = config;

            var queryModel = new EnableQueryAttribute().GetModel(entityClrType, request, descriptor);

            Assert.NotNull(queryModel);
            Assert.Same(descriptor.Properties["Microsoft.AspNet.OData.Model+Microsoft.AspNet.OData.Test.Query.QueryCompositionCustomer"],
                queryModel);
        }

        [Fact]
        public void CreateQueryContext_ReturnsQueryContext_ForNonMatchingModelOnRequest()
        {
            var builder = ODataConventionModelBuilderFactory.Create();
            var model = builder.GetEdmModel();
            var entityClrType = typeof(QueryCompositionCustomer);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            request.EnableHttpDependencyInjectionSupport(model);
            var descriptor = new ReflectedHttpActionDescriptor();
            descriptor.Configuration = request.GetConfiguration();

            var queryModel = new EnableQueryAttribute().GetModel(entityClrType, request, descriptor);

            Assert.NotNull(queryModel);
            Assert.Same(descriptor.Properties["Microsoft.AspNet.OData.Model+Microsoft.AspNet.OData.Test.Query.QueryCompositionCustomer"],
                queryModel);
        }


        [Fact]
        public void CreateQueryContext_ReturnsQueryContext_ForMatchingModelOnRequest()
        {
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<QueryCompositionCustomer>("customers");
            var model = builder.GetEdmModel();
            var entityClrType = typeof(QueryCompositionCustomer);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            request.EnableHttpDependencyInjectionSupport(model);
            var descriptor = new ReflectedHttpActionDescriptor();
            descriptor.Configuration = request.GetConfiguration();

            var queryModel = new EnableQueryAttribute().GetModel(entityClrType, request, descriptor);

            Assert.NotNull(queryModel);
            Assert.Same(model, queryModel);
            Assert.DoesNotContain("Microsoft.AspNet.OData.Model+Microsoft.AspNet.OData.Test.Query.QueryCompositionCustomer",
                descriptor.Properties.Keys.OfType<string>());
        }

        [Fact]
        public async Task QueryableOnActionUnknownOperatorIsAllowed()
        {
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext(
                "http://localhost:8080/?$orderby=$it desc&unknown=12",
                Enumerable.Range(0, 5).AsQueryable());

            // unsupported operator - ignored
            attribute.OnActionExecuted(actionExecutedContext);

            List<int> result = await actionExecutedContext.Response.Content.ReadAsObject<List<int>>();
            Assert.Equal(new[] { 4, 3, 2, 1, 0 }, result);
        }

        [Fact]
        public void QueryableOnActionUnknownOperatorStartingDollarSignThrows()
        {
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext(
                "http://localhost:8080/QueryCompositionCustomer?$orderby=Name desc&$unknown=12",
                QueryCompositionCustomerController.CustomerList.AsQueryable());

            var exception = ExceptionAssert.Throws<HttpResponseException>(() => attribute.OnActionExecuted(actionExecutedContext));

            // EnableQueryAttribute will validate and throws
            Assert.Equal(HttpStatusCode.BadRequest, exception.Response.StatusCode);
        }

        [Fact]
        public virtual void QueryableUsesConfiguredAssembliesResolver_For_MappingDerivedTypes()
        {
            // Arrange
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext(
                "http://localhost:8080/QueryCompositionCustomer/?$filter=Id eq 2",
                QueryCompositionCustomerController.CustomerList.AsQueryable());

            ODataModelBuilder modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.EntitySet<QueryCompositionCustomer>(typeof(QueryCompositionCustomer).Name);
            IEdmModel model = modelBuilder.GetEdmModel();
            model.SetAnnotationValue<ClrTypeAnnotation>(model.FindType("Microsoft.AspNet.OData.Test.Query.QueryCompositionCustomer"), null);

            bool called = false;
            Mock<IAssembliesResolver> assembliesResolver = new Mock<IAssembliesResolver>();
            assembliesResolver
                .Setup(r => r.GetAssemblies())
                .Returns(new DefaultAssembliesResolver().GetAssemblies())
                .Callback(() => { called = true; })
                .Verifiable();
            actionExecutedContext.Request.GetConfiguration().Services.Replace(typeof(IAssembliesResolver), assembliesResolver.Object);

            // Act
            attribute.OnActionExecuted(actionExecutedContext);

            // Assert
            Assert.True(called);
        }

        [Fact]
        public void ApplyQuery_SingleEntity_ThrowsArgumentNull_Entity()
        {
            var message = RequestFactory.Create();
            message.EnableHttpDependencyInjectionSupport();
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            ODataQueryOptions options = new ODataQueryOptions(new ODataQueryContext(EdmCoreModel.Instance, typeof(int)), message);

            ExceptionAssert.ThrowsArgumentNull(
                () => attribute.ApplyQuery(entity: null, queryOptions: options),
                "entity");
        }

        [Fact]
        public void ApplyQuery_SingleEntity_ThrowsArgumentNull_QueryOptions()
        {
            EnableQueryAttribute attribute = new EnableQueryAttribute();

            ExceptionAssert.ThrowsArgumentNull(
                () => attribute.ApplyQuery(entity: 42, queryOptions: null),
                "queryOptions");
        }

        [Fact]
        public void ApplyQuery_CallsApplyOnODataQueryOptions()
        {
            object entity = new object();
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            var request = RequestFactory.Create();
            request.EnableHttpDependencyInjectionSupport();
            Mock<ODataQueryOptions> queryOptions = new Mock<ODataQueryOptions>(context, request);

            attribute.ApplyQuery(entity, queryOptions.Object);

            queryOptions.Verify(q => q.ApplyTo(entity, It.IsAny<ODataQuerySettings>()), Times.Once());
        }

        public static TheoryDataSet<object, Type> GetElementTypeTestData
        {
            get
            {
                return new TheoryDataSet<object, Type>
                {
                    { Enumerable.Empty<int>(), typeof(int) },
                    { new List<int>(), typeof(int) },
                    { new int[0], typeof(int) },
                    { Enumerable.Empty<string>().AsQueryable(), typeof(string) },
                    { new SingleResult<string>(Enumerable.Empty<string>().AsQueryable()), typeof(string) },
                    { new Customer(), typeof(Customer) }
                };
            }
        }

        [Theory]
        [MemberData(nameof(GetElementTypeTestData))]
        public void GetElementType_Returns_ExpectedElementType(object response, Type expectedElementType)
        {
            HttpActionDescriptor actionDescriptor = new Mock<HttpActionDescriptor>().Object;
            SingleResult singleResult = response as SingleResult;
            IQueryable collection = (singleResult == null) ? null : singleResult.Queryable;
            Assert.Equal(expectedElementType, EnableQueryAttribute.GetElementType(response, collection, new WebApiActionDescriptor(actionDescriptor)));
        }

        [Fact]
        public void SingleOrDefault_IQueryableOfT_OneElementInSequence_ReturnsElement()
        {
            Customer customer = new Customer();
            IQueryable<Customer> queryable = new[] { customer }.AsQueryable();
            HttpActionDescriptor actionDescriptor = new Mock<HttpActionDescriptor>().Object;
            var result = QueryHelpers.SingleOrDefault(queryable, new WebApiActionDescriptor(actionDescriptor));

            Assert.Same(customer, result);
        }

        [Fact]
        public void SingleOrDefault_IQueryableOfT_ZeroElementsInSequence_ReturnsNull()
        {
            IQueryable<Customer> queryable = Enumerable.Empty<Customer>().AsQueryable();
            HttpActionDescriptor actionDescriptor = new Mock<HttpActionDescriptor>().Object;

            var result = QueryHelpers.SingleOrDefault(queryable, new WebApiActionDescriptor(actionDescriptor));

            Assert.Null(result);
        }

        [Fact]
        public void SingleOrDefault_IQueryableOfT_MoreThaneOneElementInSequence_Throws()
        {
            IQueryable<Customer> queryable = new[] { new Customer(), new Customer() }.AsQueryable();
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor
            {
                Configuration = new HttpConfiguration(),
                MethodInfo = GetType().GetMethod("SomeAction", BindingFlags.Instance | BindingFlags.NonPublic),
                ControllerDescriptor = new HttpControllerDescriptor { ControllerName = "SomeName" }
            };

            ExceptionAssert.Throws<InvalidOperationException>(
                () => QueryHelpers.SingleOrDefault(queryable, new WebApiActionDescriptor(actionDescriptor)),
                "The action 'SomeAction' on controller 'SomeName' returned a SingleResult containing more than one element. " +
                "SingleResult must have zero or one elements.");
        }

        [Fact]
        public void SingleOrDefault_DisposeCalled_EmptySequence()
        {
            // Arrange
            var enumerator = new Mock<IEnumerator>(MockBehavior.Strict);
            enumerator.Setup(mock => mock.MoveNext()).Returns(false);

            var disposable = enumerator.As<IDisposable>();
            disposable.Setup(mock => mock.Dispose()).Verifiable();

            var queryable = new Mock<IQueryable>(MockBehavior.Strict);
            queryable.Setup(mock => mock.GetEnumerator()).Returns(enumerator.Object);

            var actionDescriptor = new ReflectedHttpActionDescriptor
            {
                Configuration = new HttpConfiguration(),
                MethodInfo = GetType().GetMethod("SomeAction", BindingFlags.Instance | BindingFlags.NonPublic),
                ControllerDescriptor = new HttpControllerDescriptor { ControllerName = "SomeName" }
            };

            // Act
            QueryHelpers.SingleOrDefault(queryable.Object, new WebApiActionDescriptor(actionDescriptor));

            // Assert
            disposable.Verify();
        }

        [Fact]
        public void SingleOrDefault_DisposeCalled_OneElementInSequence()
        {
            // Arrange
            var enumerator = new Mock<IEnumerator>(MockBehavior.Strict);
            enumerator.SetupSequence(mock => mock.MoveNext()).Returns(true).Returns(false);
            enumerator.SetupGet(mock => mock.Current).Returns(new Customer());

            var disposable = enumerator.As<IDisposable>();
            disposable.Setup(mock => mock.Dispose()).Verifiable();

            var queryable = new Mock<IQueryable>(MockBehavior.Strict);
            queryable.Setup(mock => mock.GetEnumerator()).Returns(enumerator.Object);

            var actionDescriptor = new ReflectedHttpActionDescriptor
            {
                Configuration = new HttpConfiguration(),
                MethodInfo = GetType().GetMethod("SomeAction", BindingFlags.Instance | BindingFlags.NonPublic),
                ControllerDescriptor = new HttpControllerDescriptor { ControllerName = "SomeName" }
            };

            // Act
            QueryHelpers.SingleOrDefault(queryable.Object, new WebApiActionDescriptor(actionDescriptor));

            // Assert
            disposable.Verify();
        }

        [Fact]
        public void SingleOrDefault_DisposeCalled_MultipleElementsInSequence()
        {
            // Arrange
            var enumerator = new Mock<IEnumerator>(MockBehavior.Strict);
            enumerator.Setup(mock => mock.MoveNext()).Returns(true);
            enumerator.SetupGet(mock => mock.Current).Returns(new Customer());

            var disposable = enumerator.As<IDisposable>();
            disposable.Setup(mock => mock.Dispose()).Verifiable();

            var queryable = new Mock<IQueryable>(MockBehavior.Strict);
            queryable.Setup(mock => mock.GetEnumerator()).Returns(enumerator.Object);

            var actionDescriptor = new ReflectedHttpActionDescriptor
            {
                Configuration = new HttpConfiguration(),
                MethodInfo = GetType().GetMethod("SomeAction", BindingFlags.Instance | BindingFlags.NonPublic),
                ControllerDescriptor = new HttpControllerDescriptor { ControllerName = "SomeName" }
            };

            // Act (will throw)
            try
            {
                QueryHelpers.SingleOrDefault(queryable.Object, new WebApiActionDescriptor(actionDescriptor));
            }
            catch
            {
                // Other tests confirm the Exception.
            }

            // Assert
            disposable.Verify();
        }

        [Fact]
        public void OnActionExecuted_SingleResult_ReturnsSingleItemEvenIfThereIsNoSelectExpand()
        {
            BellevueCustomer customer = new BellevueCustomer();
            SingleResult singleResult = new SingleResult<BellevueCustomer>(new BellevueCustomer[] { customer }.AsQueryable());
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext("http://localhost/", singleResult);
            EnableQueryAttribute attribute = new EnableQueryAttribute();

            attribute.OnActionExecuted(actionExecutedContext);

            Assert.Equal(HttpStatusCode.OK, actionExecutedContext.Response.StatusCode);
            Assert.Equal(customer, (actionExecutedContext.Response.Content as ObjectContent).Value);
        }

        [Fact]
        public void OnActionExecuted_SingleResult_Returns400_IfQueryContainsNonSelectExpand()
        {
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext("http://localhost/?$top=10", new Customer());
            EnableQueryAttribute attribute = new EnableQueryAttribute();

            attribute.OnActionExecuted(actionExecutedContext);

            Assert.Equal(HttpStatusCode.BadRequest, actionExecutedContext.Response.StatusCode);
        }

        [Fact]
        public void OnActionExecuted_SingleResult_WithEmptyQueryResult_SetsNotFoundResponse()
        {
            // Arrange
            var customers = Enumerable.Empty<Customer>().AsQueryable();
            SingleResult result = SingleResult.Create(customers);
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext("http://localhost/", result);
            EnableQueryAttribute attribute = new EnableQueryAttribute();

            // Act
            attribute.OnActionExecuted(actionExecutedContext);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, actionExecutedContext.Response.StatusCode);
        }

        [Fact]
        public void OnActionExecuted_SingleResult_WithEmptyQueryResult_SetsNotFound()
        {
            // Arrange
            var customers = Enumerable.Empty<Customer>().AsQueryable();
            SingleResult result = SingleResult.Create(customers);
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext("http://localhost/", result);
            var container = new EdmEntityContainer("NS", "Default");
            var entityType = new EdmEntityType("NS", "entity");
            var entitySet = new EdmEntitySet(container, "entities", entityType);
            actionExecutedContext.Request.ODataProperties().Path = new ODataPath(new EntitySetSegment(entitySet));
            EnableQueryAttribute attribute = new EnableQueryAttribute();

            // Act
            attribute.OnActionExecuted(actionExecutedContext);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, actionExecutedContext.Response.StatusCode);
        }

        [Fact]
        public async Task OnActionExecuted_SingleResult_WithMoreThanASingleQueryResult_ReturnsBadRequest()
        {
            // Arrange
            var customers = CustomerList.AsQueryable();
            SingleResult result = SingleResult.Create(customers);
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext("http://localhost/", result);
            EnableQueryAttribute attribute = new EnableQueryAttribute();

            // Act
            attribute.OnActionExecuted(actionExecutedContext);
            string responseString = await actionExecutedContext.Response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, actionExecutedContext.Response.StatusCode);
            Assert.Contains("The query specified in the URI is not valid. The action 'Bar' on controller 'FooController' " +
                "returned a SingleResult containing more than one element. SingleResult must have zero or one elements.",
                responseString);
        }

#if !NETCORE
        [Fact]
        public void OnActionExecuted_UseCachedODataQueryOptions()
        {
            var model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));

            Customer customer = new Customer();
            SingleResult singleResult = new SingleResult<Customer>(new Customer[] { customer }.AsQueryable());
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext("http://localhost/", singleResult);

            ODataQueryOptions actualQueryOptions = null;
            ODataQueryOptions expectedQueryOptions = new ODataQueryOptions(
                new ODataQueryContext(model.Model, typeof(Customer)),
                actionExecutedContext.Request);

            actionExecutedContext.Request.SetODataQueryOptions(expectedQueryOptions);

            var mockAttribute = new Mock<EnableQueryAttribute>
            {
                CallBase = true,
            };
            mockAttribute
                .Setup(x => x.ValidateQuery(It.IsAny<HttpRequestMessage>(), It.IsAny<ODataQueryOptions>()))
                .Callback<HttpRequestMessage, ODataQueryOptions>((r, o) => { actualQueryOptions = o; });

            mockAttribute.Object.OnActionExecuted(actionExecutedContext);

            Assert.Same(expectedQueryOptions, actualQueryOptions);
        }

        [Fact]
        public void OnActionExecuted_UseCachedODataQueryOptionsDisabled()
        {
            var model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));

            Customer customer = new Customer();
            SingleResult singleResult = new SingleResult<Customer>(new Customer[] { customer }.AsQueryable());
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext(
                "http://localhost/",
                singleResult,
                CompatibilityOptions.DisableODataQueryOptionsReuse);

            ODataQueryOptions actualQueryOptions = null;
            ODataQueryOptions expectedQueryOptions = new ODataQueryOptions(
                new ODataQueryContext(model.Model, typeof(Customer)),
                actionExecutedContext.Request);

            actionExecutedContext.Request.SetODataQueryOptions(expectedQueryOptions);

            var mockAttribute = new Mock<EnableQueryAttribute>
            {
                CallBase = true,
            };
            mockAttribute
                .Setup(x => x.ValidateQuery(It.IsAny<HttpRequestMessage>(), It.IsAny<ODataQueryOptions>()))
                .Callback<HttpRequestMessage, ODataQueryOptions>((r, o) => { actualQueryOptions = o; });

            mockAttribute.Object.OnActionExecuted(actionExecutedContext);

            Assert.NotSame(expectedQueryOptions, actualQueryOptions);
        }
#endif

        [Theory]
        [InlineData("$filter=ID eq 1")]
        [InlineData("$orderby=ID")]
        [InlineData("$count=true")]
        [InlineData("$skip=1")]
        [InlineData("$top=0")]
        public void ValidateSelectExpandOnly_ThrowsODataException_IfODataQueryOptionsHasNonSelectExpand(string parameter)
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost?" + parameter);
            request.EnableHttpDependencyInjectionSupport();
            ODataQueryContext context = new ODataQueryContext(model.Model, typeof(Customer));
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

            ExceptionAssert.Throws<ODataException>(
                () => EnableQueryAttribute.ValidateSelectExpandOnly(queryOptions),
                "The requested resource is not a collection. Query options $filter, $orderby, $count, $skip, and $top can be applied only on collections.");
        }

        [Fact]
        public void OnActionExecuted_Works_WithPath()
        {
            // Arrange
            SimpleCustomer customer = new SimpleCustomer();
            SingleResult singleResult = new SingleResult<SimpleCustomer>(new[] { customer }.AsQueryable());
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext("http://localhost/", singleResult);
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpRequestMessage request = actionExecutedContext.Request;
            var container = new EdmEntityContainer("NS", "Default");
            var entityType = new EdmEntityType("NS", "entity");
            var entitySet = new EdmEntitySet(container, "entities", entityType);
            request.ODataProperties().Path = new ODataPath(new EntitySetSegment(entitySet));

            // Act
            attribute.OnActionExecuted(actionExecutedContext);

            // Assert
            Assert.Equal(HttpStatusCode.OK, actionExecutedContext.Response.StatusCode);
            Assert.Equal(customer, ((ObjectContent)actionExecutedContext.Response.Content).Value);
        }

        private class SimpleCustomer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public void OnActionExecuted_StringValue()
        {
            // Arrange
            string stringResult = "foo";
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext("http://localhost/Suppliers(1)/CompanyName?customqueryoption=bar", stringResult);

            // Act
            attribute.OnActionExecuted(actionExecutedContext);

            // Assert
            Assert.Equal(HttpStatusCode.OK, actionExecutedContext.Response.StatusCode);
            Assert.Equal(stringResult, ((ObjectContent)actionExecutedContext.Response.Content).Value);
        }

        [Fact]
        public void OnActionExecuted_ByteArrayValue()
        {
            // Arrange
            byte[] bytesResult = BitConverter.GetBytes(42);
            EnableQueryAttribute attribute = new EnableQueryAttribute();
            HttpActionExecutedContext actionExecutedContext = GetActionExecutedContext("http://localhost/Suppliers(1)/Version?customqueryoption=bar", bytesResult);

            // Act
            attribute.OnActionExecuted(actionExecutedContext);

            // Assert
            Assert.Equal(HttpStatusCode.OK, actionExecutedContext.Response.StatusCode);
            Assert.Equal(bytesResult, ((ObjectContent)actionExecutedContext.Response.Content).Value);
        }

        private void SomeAction()
        {
        }

        private static HttpActionExecutedContext GetActionExecutedContext<TResponse>(
            string uri,
            TResponse result,
            CompatibilityOptions? compatibilityOptions = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.EnableODataDependencyInjectionSupport();
            var actionContext = ContextUtil.CreateActionContext(ContextUtil.CreateControllerContext(request: request));
            var response = request.CreateResponse<TResponse>(HttpStatusCode.OK, result);
            var actionExecutedContext = new HttpActionExecutedContext { ActionContext = actionContext, Response = response };
            actionContext.ActionDescriptor.Configuration = request.GetConfiguration();

            if (compatibilityOptions.HasValue)
            {
                actionContext.ActionDescriptor.Configuration.SetCompatibilityOptions(compatibilityOptions.GetValueOrDefault());
            }

            return actionExecutedContext;
        }
#endif
    }
}
