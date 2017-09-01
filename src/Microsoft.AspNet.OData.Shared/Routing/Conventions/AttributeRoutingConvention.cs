// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Routing.Template;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// Represents a routing convention that looks for <see cref="ODataRouteAttribute"/>s to match an <see cref="ODataPath"/>
    /// to a controller and an action.
    /// </summary>
    public partial class AttributeRoutingConvention : IODataRoutingConvention
    {
        /// <inheritdoc />
        internal static SelectControllerResult SelectControllerImpl(ODataPath odataPath, IWebApiRequestMessage request,
            IDictionary<ODataPathTemplate, IWebApiActionDescriptor> attributeMappings)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();

            foreach (KeyValuePair<ODataPathTemplate, IWebApiActionDescriptor> attributeMapping in attributeMappings)
            {
                ODataPathTemplate template = attributeMapping.Key;
                IWebApiActionDescriptor action = attributeMapping.Value;

                if (action.IsHttpMethodSupported(request.Method) && template.TryMatch(odataPath, values))
                {
                    values["action"] = action.ActionName;
                    SelectControllerResult result = new SelectControllerResult(action.ControllerName, values);

                    return result;
                }
            }

            return null;
        }

        /// <inheritdoc />
        internal static string SelectActionImpl(ODataPath odataPath, IWebApiControllerContext controllerContext,
            IWebApiActionMap actionMap)
        {
            var routeData = controllerContext.Request.RouteData;
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
    }
}
