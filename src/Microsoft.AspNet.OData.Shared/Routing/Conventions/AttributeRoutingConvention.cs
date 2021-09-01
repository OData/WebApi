//-----------------------------------------------------------------------------
// <copyright file="AttributeRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// Represents a routing convention that looks for <see cref="ODataRouteAttribute"/>s to match an <see cref="ODataPath"/>
    /// to a controller and an action.
    /// </summary>
    public partial class AttributeRoutingConvention
    {
        private readonly string _routeName;

        private IDictionary<ODataPathTemplate, IWebApiActionDescriptor> _attributeMappings;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        private AttributeRoutingConvention(string routeName)
        {
            if (routeName == null)
            {
                throw Error.ArgumentNull("routeName");
            }

            _routeName = routeName;
        }

        /// <summary>
        /// Gets the <see cref="IODataPathTemplateHandler"/> to be used for parsing the route templates.
        /// </summary>
        public IODataPathTemplateHandler ODataPathTemplateHandler { get; private set; }

        /// <inheritdoc />
        internal static SelectControllerResult SelectControllerImpl(ODataPath odataPath, IWebApiRequestMessage request,
            IDictionary<ODataPathTemplate, IWebApiActionDescriptor> attributeMappings)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();

            foreach (KeyValuePair<ODataPathTemplate, IWebApiActionDescriptor> attributeMapping in attributeMappings)
            {
                ODataPathTemplate template = attributeMapping.Key;
                IWebApiActionDescriptor action = attributeMapping.Value;

                if (action.IsHttpMethodSupported(request.GetRequestMethodOrPreflightMethod()) && template.TryMatch(odataPath, values))
                {
                    values["action"] = action.ActionName;
                    // store the method info to allow the action selector to distinguish
                    // between this action and other overloads
                    values[ODataRouteConstants.MethodInfo] = action.GetMethodInfo();
                    SelectControllerResult result = new SelectControllerResult(action.ControllerName, values);

                    return result;
                }

                // It's possible that template.TryMatch inserted values in the values dict even if
                // it did not match the current path. So let's clear the dict before trying
                // the next template
                values.Clear();
            }

            return null;
        }

        /// <inheritdoc />
        internal static string SelectActionImpl(IWebApiControllerContext controllerContext)
        {
            var routeData = controllerContext.RouteData;
            var routingConventionsStore = controllerContext.Request.Context.RoutingConventionsStore;

            IDictionary<string, object> attributeRouteData = controllerContext.ControllerResult.Values;
            if (attributeRouteData != null)
            {
                foreach (var item in attributeRouteData)
                {
                    if (item.Key.StartsWith(ODataParameterValue.ParameterValuePrefix, StringComparison.Ordinal) &&
                        item.Value is ODataParameterValue)
                    {
                        routingConventionsStore.Add(item);
                        RoutingConventionHelpers.IncrementKeyCount(routingConventionsStore);
                    }
                    else
                    {
                        routeData.Add(item);
                    }
                }

                return attributeRouteData["action"] as string;
            }

            return null;
        }

        private static IEnumerable<string> GetODataRoutePrefixes(IEnumerable<ODataRoutePrefixAttribute> prefixAttributes, string controllerName)
        {
            Contract.Assert(prefixAttributes != null);

            if (!prefixAttributes.Any())
            {
                yield return null;
            }
            else
            {
                foreach (ODataRoutePrefixAttribute prefixAttribute in prefixAttributes)
                {
                    string prefix = prefixAttribute.Prefix;

                    if (prefix != null && prefix.StartsWith("/", StringComparison.Ordinal))
                    {
                        throw Error.InvalidOperation(SRResources.RoutePrefixStartsWithSlash, prefix, controllerName);
                    }

                    if (prefix != null && prefix.EndsWith("/", StringComparison.Ordinal))
                    {
                        prefix = prefix.TrimEnd('/');
                    }

                    yield return prefix;
                }
            }
        }

        private ODataPathTemplate GetODataPathTemplate(string prefix, string pathTemplate,
            IServiceProvider requestContainer, string controllerName, string actionName)
        {
            if (prefix != null && !pathTemplate.StartsWith("/", StringComparison.Ordinal))
            {
                if (String.IsNullOrEmpty(pathTemplate))
                {
                    pathTemplate = prefix;
                }
                else if (pathTemplate.StartsWith("(", StringComparison.Ordinal))
                {
                    // We don't need '/' when the pathTemplate starts with a key segment.
                    pathTemplate = prefix + pathTemplate;
                }
                else
                {
                    pathTemplate = prefix + "/" + pathTemplate;
                }
            }

            if (pathTemplate.StartsWith("/", StringComparison.Ordinal))
            {
                pathTemplate = pathTemplate.Substring(1);
            }

            ODataPathTemplate odataPathTemplate;

            try
            {
                // We are NOT in a request but establishing the attribute routing convention.
                // So use the root container rather than the request container.
                odataPathTemplate = ODataPathTemplateHandler.ParseTemplate(pathTemplate, requestContainer);
            }
            catch (ODataException e)
            {
                throw Error.InvalidOperation(SRResources.InvalidODataRouteOnAction, pathTemplate, actionName, controllerName, e.Message);
            }

            return odataPathTemplate;
        }
    }
}
