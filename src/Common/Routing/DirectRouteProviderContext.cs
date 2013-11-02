// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

#if ASPNETWEBAPI
using System.Web.Http.Controllers;
using System.Web.Http.Properties;
using TActionDescriptor = System.Web.Http.Controllers.HttpActionDescriptor;
using TParsedRoute = System.Web.Http.Routing.HttpParsedRoute;
using TRouteDictionary = System.Web.Http.Routing.HttpRouteValueDictionary;
#else
using System.Text;
using System.Web.Mvc.Properties;
using System.Web.Routing;
using TActionDescriptor = System.Web.Mvc.ActionDescriptor;
using TParsedRoute = System.Web.Mvc.Routing.ParsedRoute;
using TRouteDictionary = System.Web.Routing.RouteValueDictionary;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    internal class DirectRouteProviderContext
    {
        private readonly string _actionName;

#if !ASPNETWEBAPI
        private readonly string _controllerName;
#endif

#if ASPNETWEBAPI
        private readonly string _prefix;
#else
        private readonly string _areaPrefix;
        private readonly string _controllerPrefix;
#endif

        private readonly IEnumerable<TActionDescriptor> _actions;
        private readonly IInlineConstraintResolver _inlineConstraintResolver;

#if !ASPNETWEBAPI
        private readonly bool _targetIsAction;
#endif

#if ASPNETWEBAPI
        public DirectRouteProviderContext(string prefix, IEnumerable<HttpActionDescriptor> actions,
            IInlineConstraintResolver inlineConstraintResolver)
#else
        public DirectRouteProviderContext(string areaPrefix, string controllerPrefix,
            IEnumerable<ActionDescriptor> actions, IInlineConstraintResolver inlineConstraintResolver,
            bool targetIsAction)
#endif
        {
            if (actions == null)
            {
                throw new ArgumentNullException("actions");
            }

            if (inlineConstraintResolver == null)
            {
                throw new ArgumentNullException("inlineConstraintResolver");
            }

#if ASPNETWEBAPI
            _prefix = prefix;
#else
            _areaPrefix = areaPrefix;
            _controllerPrefix = controllerPrefix;
#endif
            _actions = actions;
            _inlineConstraintResolver = inlineConstraintResolver;

            TActionDescriptor firstDescriptor = actions.FirstOrDefault();
            Contract.Assert(firstDescriptor != null);
            _actionName = firstDescriptor.ActionName;

#if !ASPNETWEBAPI
            ControllerDescriptor controllerDescriptor = firstDescriptor.ControllerDescriptor;
            Contract.Assert(controllerDescriptor != null);
            _controllerName = controllerDescriptor.ControllerName;

            _targetIsAction = targetIsAction;
#endif
        }

#if ASPNETWEBAPI
        public string Prefix
        {
            get { return _prefix; }
        }
#else
        public string AreaPrefix
        {
            get { return _areaPrefix; }
        }

        public string ControllerPrefix
        {
            get { return _controllerPrefix; }
        }
#endif

        public IEnumerable<TActionDescriptor> Actions
        {
            get { return _actions; }
        }

        public IInlineConstraintResolver InlineConstraintResolver
        {
            get { return _inlineConstraintResolver; }
        }

#if !ASPNETWEBAPI
        public bool TargetIsAction
        {
            get { return _targetIsAction; }
        }
#endif

        public virtual DirectRouteBuilder CreateBuilder(string template)
        {
            return CreateBuilder(template, _inlineConstraintResolver);
        }

        internal virtual DirectRouteBuilder CreateBuilder(string template,
            IInlineConstraintResolver constraintResolver)
        {
#if ASPNETWEBAPI
            DirectRouteBuilder builder = new DirectRouteBuilder(_actions);
#else
            DirectRouteBuilder builder = new DirectRouteBuilder(_actions, _targetIsAction);
#endif

#if ASPNETWEBAPI
            string prefixedTemplate = BuildRouteTemplate(_prefix, template);
#else
            string prefixedTemplate = BuildRouteTemplate(_areaPrefix, _controllerPrefix, template ?? String.Empty);
#endif
            Contract.Assert(prefixedTemplate != null);

            ValidateTemplate(prefixedTemplate);

            if (constraintResolver != null)
            {
                TRouteDictionary defaults = new TRouteDictionary();
                TRouteDictionary constraints = new TRouteDictionary();

                string detokenizedTemplate = InlineRouteTemplateParser.ParseRouteTemplate(prefixedTemplate, defaults,
                    constraints, constraintResolver);
                TParsedRoute parsedRoute = RouteParser.Parse(detokenizedTemplate);
                decimal precedence = RoutePrecedence.Compute(parsedRoute, constraints);

                builder.Defaults = defaults;
                builder.Constraints = constraints;
                builder.Template = detokenizedTemplate;
                builder.Precedence = precedence;
#if ASPNETWEBAPI
                builder.ParsedRoute = parsedRoute;
#endif
            }
            else
            {
                builder.Template = prefixedTemplate;
            }

            return builder;
        }

#if ASPNETWEBAPI
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
#else
        internal static string BuildRouteTemplate(string areaPrefix, string prefix, string template)
        {
            Contract.Assert(template != null);

            // If the attribute's template starts with '~/', ignore the area and controller prefixes
            if (template.StartsWith("~/", StringComparison.Ordinal))
            {
                return template.Substring(2);
            }

            if (prefix == null && areaPrefix == null)
            {
                return template;
            }

            StringBuilder templateBuilder = new StringBuilder();

            if (areaPrefix != null)
            {
                templateBuilder.Append(areaPrefix);
            }

            if (!String.IsNullOrEmpty(prefix))
            {
                if (templateBuilder.Length > 0)
                {
                    templateBuilder.Append('/');
                }
                templateBuilder.Append(prefix);
            }

            if (!String.IsNullOrEmpty(template))
            {
                if (templateBuilder.Length > 0)
                {
                    templateBuilder.Append('/');
                }
                templateBuilder.Append(template);
            }

            return templateBuilder.ToString();
        }
#endif

        private void ValidateTemplate(string template)
        {
            if (template.StartsWith("/", StringComparison.Ordinal))
            {
#if ASPNETWEBAPI
                string errorMessage = Error.Format(SRResources.AttributeRoutes_InvalidTemplate, template, _actionName);
#else
                string errorMessage = Error.Format(MvcResources.RouteTemplate_CannotStart_WithForwardSlash, template,
                    _actionName, _controllerName);
#endif
                throw new InvalidOperationException(errorMessage);
            }
        }
    }
}
