// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Routing
{
    public class HttpParsedRouteTest
    {
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
            return parsedRoute.GetPrecedence(constraints);
        }
    }
}
