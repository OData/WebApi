// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;

namespace System.Web.Http.Routing
{
    internal class DirectRouteProviderContext
    {
        private readonly string _actionName;
        private readonly string _prefix;
        private readonly IEnumerable<HttpActionDescriptor> _actions;
        private readonly IInlineConstraintResolver _inlineConstraintResolver;

        public DirectRouteProviderContext(string prefix, IEnumerable<HttpActionDescriptor> actions,
            IInlineConstraintResolver inlineConstraintResolver)
        {
            if (actions == null)
            {
                throw new ArgumentNullException("actions");
            }

            if (inlineConstraintResolver == null)
            {
                throw new ArgumentNullException("inlineConstraintResolver");
            }

            _prefix = prefix;
            _actions = actions;
            _inlineConstraintResolver = inlineConstraintResolver;

            HttpActionDescriptor firstDescriptor = actions.FirstOrDefault();
            Contract.Assert(firstDescriptor != null);
            _actionName = firstDescriptor.ActionName;
        }

        public string Prefix
        {
            get { return _prefix; }
        }

        public IEnumerable<HttpActionDescriptor> Actions
        {
            get { return _actions; }
        }

        public IInlineConstraintResolver InlineConstraintResolver
        {
            get { return _inlineConstraintResolver; }
        }

        public virtual DirectRouteBuilder CreateBuilder(string template)
        {
            return CreateBuilder(template, _inlineConstraintResolver);
        }

        internal virtual DirectRouteBuilder CreateBuilder(string template,
            IInlineConstraintResolver constraintResolver)
        {
            DirectRouteBuilder builder = new DirectRouteBuilder(_actions);

            string prefixedTemplate = BuildRouteTemplate(_prefix, template);

            Contract.Assert(prefixedTemplate != null);

            if (prefixedTemplate.StartsWith("/", StringComparison.Ordinal))
            {
                throw Error.InvalidOperation(SRResources.AttributeRoutes_InvalidTemplate, template, _actionName);
            }

            if (constraintResolver != null)
            {
                builder.Defaults = new HttpRouteValueDictionary();
                builder.Constraints = new HttpRouteValueDictionary();
                builder.Template = InlineRouteTemplateParser.ParseRouteTemplate(prefixedTemplate, builder.Defaults,
                    builder.Constraints, constraintResolver);
                builder.ParsedRoute = HttpRouteParser.Parse(builder.Template);
                builder.Precedence = RoutePrecedence.Compute(builder.ParsedRoute, builder.Constraints);
            }
            else
            {
                builder.Template = prefixedTemplate;
            }

            return builder;
        }

        private static string BuildRouteTemplate(string routePrefix, string routeTemplate)
        {
            if (String.IsNullOrEmpty(routeTemplate))
            {
                return routePrefix ?? String.Empty;
            }

            // If the provider's template starts with '~/', ignore the route prefix
            if (routeTemplate.StartsWith("~/", StringComparison.Ordinal))
            {
                return routeTemplate.Substring(2);
            }
            else if (String.IsNullOrEmpty(routePrefix))
            {
                return routeTemplate;
            }
            else
            {
                // template and prefix both not null - combine them
                return routePrefix + '/' + routeTemplate;
            }
        }
    }
}
