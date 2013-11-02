// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
#if ASPNETWEBAPI
using System.Web.Http.Controllers;
#endif
using Microsoft.TestCommon;
using Moq;

#if ASPNETWEBAPI
using TActionDescriptor = System.Web.Http.Controllers.HttpActionDescriptor;
using TRoute = System.Web.Http.Routing.IHttpRoute;
using TRouteDictionary = System.Web.Http.Routing.HttpRouteValueDictionary;
#else
using TActionDescriptor = System.Web.Mvc.ActionDescriptor;
using TRoute = System.Web.Routing.Route;
using TRouteDictionary = System.Web.Routing.RouteValueDictionary;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    public class RouteProviderAttributeTests
    {
        [Fact]
        public void TemplateGet_ReturnsSpecifiedInstance()
        {
            // Arrange
            string expectedTemplate = "RouteTemplate";
            RouteProviderAttribute product = CreateProductUnderTest(expectedTemplate);

            // Act
            string template = product.Template;

            // Assert
            Assert.Same(expectedTemplate, template);
        }

        [Fact]
        public void NameGet_ReturnsNull()
        {
            // Arrange
            RouteProviderAttribute product = CreateProductUnderTest();

            // Act
            string name = product.Name;

            // Assert
            Assert.Null(name);
        }

        [Fact]
        public void OrderGet_ReturnsZero()
        {
            // Arrange
            RouteProviderAttribute product = CreateProductUnderTest();

            // Act
            int order = product.Order;

            // Assert
            Assert.Equal(0, order);
        }

        [Fact]
        public void ConstraintsGet_ReturnsNull()
        {
            // Arrange
            RouteProviderAttribute product = CreateProductUnderTest();

            // Act
            TRouteDictionary constraints = product.Constraints;

            // Assert
            Assert.Null(constraints);
        }

        [Fact]
        public void CreateRoute_DelegatesToContextCreateBuilderBuild()
        {
            // Arrange
            string expectedTemplate = "RouteTemplate";
            IDirectRouteProvider product = CreateProductUnderTest(expectedTemplate);

            RouteEntry expectedEntry = CreateEntry();

            DirectRouteBuilder builder = CreateBuilder(() => expectedEntry);
            DirectRouteProviderContext context = CreateContext((template) => template == expectedTemplate ? builder :
#if ASPNETWEBAPI
                new DirectRouteBuilder(new TActionDescriptor[0]));
#else
                new DirectRouteBuilder(new TActionDescriptor[0], targetIsAction: true));
#endif

            // Act
            RouteEntry entry = product.CreateRoute(context);

            // Assert
            Assert.Same(expectedEntry, entry);
        }

        [Fact]
        public void CreateRoute_UsesNamePropertyWhenBuilding()
        {
            // Arrange
            string expectedName = "RouteName";
            RouteProviderAttribute product = CreateProductUnderTest();
            product.Name = expectedName;

            string name = null;
            DirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                name = builder.Name;
                return null;
            });
            DirectRouteProviderContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(expectedName, name);
        }

        [Fact]
        public void CreateRoute_UsesOrderPropertyWhenBuilding()
        {
            // Arrange
            int expectedOrder = 123;
            RouteProviderAttribute product = CreateProductUnderTest();
            product.Order = expectedOrder;

            int order = 0;
            DirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                order = builder.Order;
                return null;
            });
            DirectRouteProviderContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Equal(expectedOrder, order);
        }

        [Fact]
        public void CreateRoute_IfBuilderContraintsIsNull_UsesConstraintsPropertyWhenBuilding()
        {
            // Arrange
            TRouteDictionary expectedConstraints = new TRouteDictionary();
            Mock<RouteProviderAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.Constraints).Returns(expectedConstraints);
            IDirectRouteProvider product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            TRouteDictionary constraints = null;
            DirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                constraints = builder.Constraints;
                return null;
            });
            Assert.Null(builder.Constraints); // Guard
            DirectRouteProviderContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(expectedConstraints, constraints);
        }

        [Fact]
        public void CreateRoute_IfBuilderContraintsIsNotNull_AddsConstraintsFromPropertyWhenBuilding()
        {
            // Arrange
            TRouteDictionary existingConstraints = new TRouteDictionary();
            string existingConstraintKey = "ExistingContraintKey";
            object existingConstraintValue = "ExistingContraint";
            existingConstraints.Add(existingConstraintKey, existingConstraintValue);

            TRouteDictionary additionalConstraints = new TRouteDictionary();
            string additionalConstraintKey = "NewConstraintKey";
            string additionalConstraintValue = "NewConstraint";
            additionalConstraints.Add(additionalConstraintKey, additionalConstraintValue);

            Mock<RouteProviderAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.Constraints).Returns(additionalConstraints);
            IDirectRouteProvider product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            TRouteDictionary constraints = null;
            DirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                constraints = builder.Constraints;
                return null;
            });

            builder.Constraints = existingConstraints;

            DirectRouteProviderContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(existingConstraints, constraints);
            Assert.Equal(2, constraints.Count);
            Assert.True(constraints.ContainsKey(existingConstraintKey));
            Assert.Same(existingConstraintValue, constraints[existingConstraintKey]);
            Assert.True(constraints.ContainsKey(additionalConstraintKey));
            Assert.Same(additionalConstraintValue, constraints[additionalConstraintKey]);
        }

        [Fact]
        public void CreateRoute_IfBuilderContraintsIsNotNullAndConstraintsPropertyIsNull_UsesBuilderConstraints()
        {
            // Arrange
            TRouteDictionary existingConstraints = new TRouteDictionary();

            Mock<RouteProviderAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.Constraints).Returns((TRouteDictionary)null);
            IDirectRouteProvider product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            TRouteDictionary constraints = null;
            DirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                constraints = builder.Constraints;
                return null;
            });

            builder.Constraints = existingConstraints;

            DirectRouteProviderContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(existingConstraints, constraints);
        }

        private static DirectRouteBuilder CreateBuilder(Func<RouteEntry> build)
        {
            return new LambdaDirectRouteBuilder(build);
        }

        private static DirectRouteProviderContext CreateContext(Func<string, DirectRouteBuilder> createBuilder)
        {
            return new LambdaDirectRouteProviderContext(createBuilder);
        }

        private static RouteEntry CreateEntry()
        {
#if ASPNETWEBAPI
            TRoute route = new Mock<TRoute>(MockBehavior.Strict).Object;
#else
            TRoute route = new Mock<TRoute>(MockBehavior.Strict, null, null).Object;
#endif
            return new RouteEntry("IgnoreEntry", route);
        }

        private static RouteProviderAttribute CreateProductUnderTest()
        {
            return CreateProductUnderTest("IgnoreTemplate");
        }

        private static RouteProviderAttribute CreateProductUnderTest(string template)
        {
            return CreateProductUnderTestMock(template).Object;
        }

        private static Mock<RouteProviderAttribute> CreateProductUnderTestMock()
        {
            return CreateProductUnderTestMock("IgnoreTemplate");
        }

        private static Mock<RouteProviderAttribute> CreateProductUnderTestMock(string template)
        {
            Mock<RouteProviderAttribute> mock = new Mock<RouteProviderAttribute>(template);
            mock.CallBase = true;
            return mock;
        }

        private class LambdaDirectRouteProviderContext : DirectRouteProviderContext
        {
            private readonly Func<string, DirectRouteBuilder> _createBuilder;

            public LambdaDirectRouteProviderContext(Func<string, DirectRouteBuilder> createBuilder)
#if ASPNETWEBAPI
                : base(null, new TActionDescriptor[] { new Mock<TActionDescriptor>().Object },
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object)
#else
                : base(null, null, new TActionDescriptor[] { CreateStubActionDescriptor() },
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object, targetIsAction: true)
#endif
            {
                Contract.Assert(createBuilder != null);
                _createBuilder = createBuilder;
            }

            public override DirectRouteBuilder CreateBuilder(string template)
            {
                return _createBuilder.Invoke(template);
            }

#if !ASPNETWEBAPI
            private static ActionDescriptor CreateStubActionDescriptor()
            {
                Mock<ActionDescriptor> mock = new Mock<TActionDescriptor>();
                mock.Setup(m => m.ControllerDescriptor).Returns(new Mock<ControllerDescriptor>().Object);
                return mock.Object;
            }
#endif
        }

        private class LambdaDirectRouteBuilder : DirectRouteBuilder
        {
            private readonly Func<RouteEntry> _build;

            public LambdaDirectRouteBuilder(Func<RouteEntry> build)
#if ASPNETWEBAPI
                : base(new TActionDescriptor[0])
#else
                : base(new TActionDescriptor[0], targetIsAction: true)
#endif
            {
                Contract.Assert(build != null);
                _build = build;
            }

            public override RouteEntry Build()
            {
                return _build.Invoke();
            }
        }
    }
}
