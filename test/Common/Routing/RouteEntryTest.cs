// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if ASPNETWEBAPI
using System.Net.Http;
#endif
using Microsoft.TestCommon;

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    public class RouteEntryTest
    {
#if !ASPNETWEBAPI

        [Fact]
        public void Precdendence_ForEquivalentRoutes()
        {
            // Arrange
            var x = CreateRouteEntry("Employees/{id}");
            var y = CreateRouteEntry("Employees/{id}");

            // Act
            var xPrededence = x.Route.GetPrecedence();
            var yPrededence = y.Route.GetPrecedence();

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
        public void CompareTo_ComparesCorrectly(string earlier, string later)
        {
            // Arrange
            var x = CreateRouteEntry(earlier);
            var y = CreateRouteEntry(later);

            // Act
            var xPrededence = x.Route.GetPrecedence();
            var yPrededence = y.Route.GetPrecedence();

            // Assert
            Assert.True(xPrededence < yPrededence);
        }

        [Theory]
        [InlineData("abc", "def")]
        [InlineData("{x:alpha}", "{x:int}")]
        public void CompareTo_ComparesCorrectly_Equal(string earlier, string later)
        {
            // Arrange
            var x = CreateRouteEntry(earlier);
            var y = CreateRouteEntry(later);

            // Act
            var xPrededence = x.Route.GetPrecedence();
            var yPrededence = y.Route.GetPrecedence();

            // Assert
            Assert.True(xPrededence == yPrededence);
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
                actionDescriptor);

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
#else
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
        public void GetPrecedence_ValuesInComparison(string earlier, string later)
        {
            decimal earlierPrecedence = GetPrecedence(earlier);
            decimal laterPrecedence = GetPrecedence(later);

            Assert.True(earlierPrecedence < laterPrecedence);
        }

        private static decimal GetPrecedence(string attributeRouteTemplate)
        {
            DefaultInlineConstraintResolver resolver = new DefaultInlineConstraintResolver();
            HttpRouteValueDictionary defaults = new HttpRouteValueDictionary();
            HttpRouteValueDictionary constraints = new HttpRouteValueDictionary();
            string standardRouteTemplate = InlineRouteTemplateParser.ParseRouteTemplate(attributeRouteTemplate,
                defaults, constraints, new DefaultInlineConstraintResolver());
            HttpParsedRoute parsedRoute = HttpRouteParser.Parse(standardRouteTemplate);
            return HttpRouteEntry.GetPrecedence(parsedRoute, constraints);
        }
#endif
    }
}