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
using TRouteDictionary = System.Collections.Generic.IDictionary<string, object>;
using TRouteDictionaryConcrete = System.Web.Http.Routing.HttpRouteValueDictionary;
using TRouteHandler = System.Net.Http.HttpMessageHandler;
#else
using TActionDescriptor = System.Web.Mvc.ActionDescriptor;
using TRoute = System.Web.Routing.Route;
using TRouteDictionary = System.Web.Routing.RouteValueDictionary;
using TRouteDictionaryConcrete = System.Web.Routing.RouteValueDictionary;
using TRouteHandler = System.Web.Routing.IRouteHandler;
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
        public void CreateRoute_IfBuilderDefaultsIsNull_UsesDefaultsPropertyWhenBuilding()
        {
            // Arrange
            TRouteDictionary expectedDefaults = new TRouteDictionaryConcrete();
            Mock<RouteProviderAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.Defaults).Returns(expectedDefaults);
            IDirectRouteProvider product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            TRouteDictionary defaults = null;
            DirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                defaults = builder.Defaults;
                return null;
            });
            Assert.Null(builder.Defaults); // Guard
            DirectRouteProviderContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(expectedDefaults, defaults);
        }

        [Fact]
        public void CreateRoute_IfBuilderDefaultsIsNotNull_UpdatesDefaultsFromPropertyWhenBuilding()
        {
            // Arrange
            TRouteDictionary existingDefaults = new TRouteDictionaryConcrete();
            string existingDefaultKey = "ExistingDefaultKey";
            object existingDefaultValue = "ExistingDefault";
            existingDefaults.Add(existingDefaultKey, existingDefaultValue);
            string conflictingDefaultKey = "ConflictingDefaultKey";
            object oldConflictingDefaultValue = "OldConflictingDefault";
            existingDefaults.Add(conflictingDefaultKey, oldConflictingDefaultValue);

            TRouteDictionary additionalDefaults = new TRouteDictionaryConcrete();
            string additionalDefaultKey = "NewDefaultKey";
            string additionalDefaultValue = "NewDefault";
            additionalDefaults.Add(additionalDefaultKey, additionalDefaultValue);
            string newConflictingDefaultValue = "NewConflictingDefault";
            additionalDefaults.Add(conflictingDefaultKey, newConflictingDefaultValue);

            Mock<RouteProviderAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.Defaults).Returns(additionalDefaults);
            IDirectRouteProvider product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            TRouteDictionary defaults = null;
            DirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                defaults = builder.Defaults;
                return null;
            });

            builder.Defaults = existingDefaults;

            DirectRouteProviderContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(existingDefaults, defaults);
            Assert.Equal(3, defaults.Count);
            Assert.True(defaults.ContainsKey(existingDefaultKey));
            Assert.Same(existingDefaultValue, defaults[existingDefaultKey]);
            Assert.True(defaults.ContainsKey(conflictingDefaultKey));
            Assert.Same(newConflictingDefaultValue, defaults[conflictingDefaultKey]);
            Assert.True(defaults.ContainsKey(additionalDefaultKey));
            Assert.Same(additionalDefaultValue, defaults[additionalDefaultKey]);
        }

        [Fact]
        public void CreateRoute_IfBuilderConstraintsIsNotNullAndDefaultsPropertyIsNull_UsesBuilderDefaults()
        {
            // Arrange
            TRouteDictionary existingDefaults = new TRouteDictionaryConcrete();

            Mock<RouteProviderAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.Defaults).Returns((TRouteDictionary)null);
            IDirectRouteProvider product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            TRouteDictionary defaults = null;
            DirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                defaults = builder.Defaults;
                return null;
            });

            builder.Defaults = existingDefaults;

            DirectRouteProviderContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(existingDefaults, defaults);
        }

        [Fact]
        public void CreateRoute_IfBuilderConstraintsIsNull_UsesConstraintsPropertyWhenBuilding()
        {
            // Arrange
            TRouteDictionary expectedConstraints = new TRouteDictionaryConcrete();
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
        public void CreateRoute_IfBuilderConstraintsIsNotNull_UpdatesConstraintsFromPropertyWhenBuilding()
        {
            // Arrange
            TRouteDictionary existingConstraints = new TRouteDictionaryConcrete();
            string existingConstraintKey = "ExistingConstraintKey";
            object existingConstraintValue = "ExistingConstraint";
            existingConstraints.Add(existingConstraintKey, existingConstraintValue);
            string conflictingConstraintKey = "ConflictingConstraintKey";
            object oldConflictingConstraintValue = "OldConflictingConstraint";
            existingConstraints.Add(conflictingConstraintKey, oldConflictingConstraintValue);

            TRouteDictionary additionalConstraints = new TRouteDictionaryConcrete();
            string additionalConstraintKey = "NewConstraintKey";
            string additionalConstraintValue = "NewConstraint";
            additionalConstraints.Add(additionalConstraintKey, additionalConstraintValue);
            string newConflictingConstraintValue = "NewConflictingConstraint";
            additionalConstraints.Add(conflictingConstraintKey, newConflictingConstraintValue);

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
            Assert.Equal(3, constraints.Count);
            Assert.True(constraints.ContainsKey(existingConstraintKey));
            Assert.Same(existingConstraintValue, constraints[existingConstraintKey]);
            Assert.True(constraints.ContainsKey(conflictingConstraintKey));
            Assert.Same(newConflictingConstraintValue, constraints[conflictingConstraintKey]);
            Assert.True(constraints.ContainsKey(additionalConstraintKey));
            Assert.Same(additionalConstraintValue, constraints[additionalConstraintKey]);
        }

        [Fact]
        public void CreateRoute_IfBuilderConstraintsIsNotNullAndConstraintsPropertyIsNull_UsesBuilderConstraints()
        {
            // Arrange
            TRouteDictionary existingConstraints = new TRouteDictionaryConcrete();

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
        
        [Fact]
        public void CreateRoute_IfBuilderDataTokensIsNull_UsesDataTokensPropertyWhenBuilding()
        {
            // Arrange
            TRouteDictionary expectedDataTokens = new TRouteDictionaryConcrete();
            Mock<RouteProviderAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.DataTokens).Returns(expectedDataTokens);
            IDirectRouteProvider product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            TRouteDictionary dataTokens = null;
            DirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                dataTokens = builder.DataTokens;
                return null;
            });
            Assert.Null(builder.DataTokens); // Guard
            DirectRouteProviderContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(expectedDataTokens, dataTokens);
        }

        [Fact]
        public void CreateRoute_IfBuilderDataTokensIsNotNull_UpdatesDataTokensFromPropertyWhenBuilding()
        {
            // Arrange
            TRouteDictionary existingDataTokens = new TRouteDictionaryConcrete();
            string existingDataTokenKey = "ExistingDataTokenKey";
            object existingDataTokenValue = "ExistingDataToken";
            existingDataTokens.Add(existingDataTokenKey, existingDataTokenValue);
            string conflictingDataTokenKey = "ConflictingDataTokenKey";
            object oldConflictingDataTokenValue = "OldConflictingDataToken";
            existingDataTokens.Add(conflictingDataTokenKey, oldConflictingDataTokenValue);

            TRouteDictionary additionalDataTokens = new TRouteDictionaryConcrete();
            string additionalDataTokenKey = "NewDataTokenKey";
            string additionalDataTokenValue = "NewDataToken";
            additionalDataTokens.Add(additionalDataTokenKey, additionalDataTokenValue);
            string newConflictingDataTokenValue = "NewConflictingDataToken";
            additionalDataTokens.Add(conflictingDataTokenKey, newConflictingDataTokenValue);

            Mock<RouteProviderAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.DataTokens).Returns(additionalDataTokens);
            IDirectRouteProvider product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            TRouteDictionary dataTokens = null;
            DirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                dataTokens = builder.DataTokens;
                return null;
            });

            builder.DataTokens = existingDataTokens;

            DirectRouteProviderContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(existingDataTokens, dataTokens);
            Assert.Equal(3, dataTokens.Count);
            Assert.True(dataTokens.ContainsKey(existingDataTokenKey));
            Assert.Same(existingDataTokenValue, dataTokens[existingDataTokenKey]);
            Assert.True(dataTokens.ContainsKey(conflictingDataTokenKey));
            Assert.Same(newConflictingDataTokenValue, dataTokens[conflictingDataTokenKey]);
            Assert.True(dataTokens.ContainsKey(additionalDataTokenKey));
            Assert.Same(additionalDataTokenValue, dataTokens[additionalDataTokenKey]);
        }

        [Fact]
        public void CreateRoute_IfBuilderDataTokensIsNotNullAndDataTokensPropertyIsNull_UsesBuilderDataTokens()
        {
            // Arrange
            TRouteDictionary existingDataTokens = new TRouteDictionaryConcrete();

            Mock<RouteProviderAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.DataTokens).Returns((TRouteDictionary)null);
            IDirectRouteProvider product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            TRouteDictionary dataTokens = null;
            DirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                dataTokens = builder.DataTokens;
                return null;
            });

            builder.DataTokens = existingDataTokens;

            DirectRouteProviderContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(existingDataTokens, dataTokens);
        }

        [Fact]
        public void CreateRoute_UsesHandler()
        {
            // Arrange
            TRouteHandler expectedHandler = new Mock<TRouteHandler>(MockBehavior.Strict).Object;

            Mock<RouteProviderAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.Handler).Returns(expectedHandler);
            IDirectRouteProvider product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            TRouteHandler handler = null;
            DirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                handler = builder.Handler;
                return null;
            });

            DirectRouteProviderContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(handler, expectedHandler);
        }

        [Fact]
        public void AttributeUsage_IsAsSpecified()
        {
            // Act
            AttributeUsageAttribute usage = (AttributeUsageAttribute)Attribute.GetCustomAttribute(
                typeof(RouteProviderAttribute), typeof(AttributeUsageAttribute));

            // Assert
            Assert.NotNull(usage);
            Assert.Equal(AttributeTargets.Class | AttributeTargets.Method, usage.ValidOn);
            Assert.Equal(false, usage.Inherited);
            Assert.Equal(true, usage.AllowMultiple);
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

            internal override DirectRouteBuilder CreateBuilderInternal(string template)
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
