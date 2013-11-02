// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Routing
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
            HttpRouteValueDictionary constraints = product.Constraints;

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
            DirectRouteProviderContext context = CreateContext((template) =>
                template == expectedTemplate ? builder : new DirectRouteBuilder(new ReflectedHttpActionDescriptor[0]));

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
            HttpRouteValueDictionary expectedConstraints = new HttpRouteValueDictionary();
            Mock<RouteProviderAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.Constraints).Returns(expectedConstraints);
            IDirectRouteProvider product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            HttpRouteValueDictionary constraints = null;
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
            HttpRouteValueDictionary existingConstraints = new HttpRouteValueDictionary();
            string existingConstraintKey = "ExistingContraintKey";
            object existingConstraintValue = "ExistingContraint";
            existingConstraints.Add(existingConstraintKey, existingConstraintValue);

            HttpRouteValueDictionary additionalConstraints = new HttpRouteValueDictionary();
            string additionalConstraintKey = "NewConstraintKey";
            string additionalConstraintValue = "NewConstraint";
            additionalConstraints.Add(additionalConstraintKey, additionalConstraintValue);

            Mock<RouteProviderAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.Constraints).Returns(additionalConstraints);
            IDirectRouteProvider product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            HttpRouteValueDictionary constraints = null;
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
            HttpRouteValueDictionary existingConstraints = new HttpRouteValueDictionary();

            Mock<RouteProviderAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.Constraints).Returns((HttpRouteValueDictionary)null);
            IDirectRouteProvider product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            HttpRouteValueDictionary constraints = null;
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
            return new RouteEntry("IgnoreEntry", new Mock<IHttpRoute>(MockBehavior.Strict).Object);
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
                : base(null, new ReflectedHttpActionDescriptor[] { new ReflectedHttpActionDescriptor() },
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object)
            {
                Contract.Assert(createBuilder != null);
                _createBuilder = createBuilder;
            }

            public override DirectRouteBuilder CreateBuilder(string template)
            {
                return _createBuilder.Invoke(template);
            }
        }

        private class LambdaDirectRouteBuilder : DirectRouteBuilder
        {
            private readonly Func<RouteEntry> _build;

            public LambdaDirectRouteBuilder(Func<RouteEntry> build)
                : base(new ReflectedHttpActionDescriptor[0])
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
