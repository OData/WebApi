// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    public class RoutePrecedenceTests
    {
        [Theory]
        [InlineData("Employees/{id}", "Employees/{id}")]
        [InlineData("abc", "def")]
        [InlineData("{x:alpha}", "{x:int}")]
        public void Compute_IsEqual(string xTemplate, string yTemplate)
        {
            // Arrange & Act
            var xPrededence = Compute(xTemplate);
            var yPrededence = Compute(yTemplate);

            // Assert
            Assert.Equal(xPrededence, yPrededence);
        }

        [Theory]
        [InlineData("abc", "a{x}")]
        [InlineData("abc", "{x}c")]
        [InlineData("abc", "{x:int}")]
        [InlineData("abc", "{x}")]
        [InlineData("abc", "{*x}")]
        [InlineData("{x:int}", "{x}")]
        [InlineData("{x:int}", "{*x}")]
        [InlineData("a{x}", "{x}")]
        [InlineData("{x}c", "{x}")]
        [InlineData("a{x}", "{*x}")]
        [InlineData("{x}c", "{*x}")]
        [InlineData("{x}", "{*x}")]
        [InlineData("{*x:maxlength(10)}", "{*x}")]
        [InlineData("abc/def", "abc/{x:int}")]
        [InlineData("abc/def", "abc/{x}")]
        [InlineData("abc/def", "abc/{*x}")]
        [InlineData("abc/{x:int}", "abc/{x}")]
        [InlineData("abc/{x:int}", "abc/{*x}")]
        [InlineData("abc/{x}", "abc/{*x}")]
        [InlineData("{x}/{y:int}", "{x}/{y}")]
        public void Compute_IsLessThan(string xTemplate, string yTemplate)
        {
            // Arrange & Act
            var xPrededence = Compute(xTemplate);
            var yPrededence = Compute(yTemplate);

            // Assert
            Assert.True(xPrededence < yPrededence);
        }

#if ASPNETWEBAPI
        private static decimal Compute(string template)
        {
            DefaultInlineConstraintResolver resolver = new DefaultInlineConstraintResolver();
            HttpRouteValueDictionary defaults = new HttpRouteValueDictionary();
            HttpRouteValueDictionary constraints = new HttpRouteValueDictionary();
            string standardRouteTemplate = InlineRouteTemplateParser.ParseRouteTemplate(template,
                defaults, constraints, new DefaultInlineConstraintResolver());
            HttpParsedRoute parsedRoute = HttpRouteParser.Parse(standardRouteTemplate);
            return RoutePrecedence.Compute(parsedRoute, constraints);
        }
#else
        private static decimal Compute(string template)
        {
            var entry = CreateRouteEntry(template);
            return entry.Route.GetPrecedence();
        }

        private static RouteEntry CreateRouteEntry(string routeTemplate)
        {
            var methodInfo = typeof(TestController).GetMethod("Action");
            var controllerDescriptor = new ReflectedControllerDescriptor(typeof(TestController));
            var actionDescriptor = new ReflectedActionDescriptor(
                methodInfo,
                "Action",
                controllerDescriptor);

            var route = new RouteBuilder2().BuildDirectRoute(
                routeTemplate,
                new RouteAttribute(),
                controllerDescriptor,
                new ActionDescriptor[] { actionDescriptor },
                routeIsForAction: true);

            var entry = new RouteEntry()
            {
                Route = route,
                Template = routeTemplate,
            };

            return entry;
        }

        private class TestController : Controller
        {
            public void Action()
            {
            }
        }
#endif
    }
}