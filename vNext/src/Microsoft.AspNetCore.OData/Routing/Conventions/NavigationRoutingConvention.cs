// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles navigation properties.
    /// </summary>
    public class NavigationRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <inheritdoc/>
        public override ActionDescriptor SelectAction(RouteContext routeContext, IEnumerable<ControllerActionDescriptor> actionDescriptors)
        {
            if (routeContext == null)
            {
                throw Error.ArgumentNull("routeContext");
            }

            ODataPath odataPath = routeContext.HttpContext.Request.ODataFeature().Path;
            HttpRequest request = routeContext.HttpContext.Request;
            string httpMethod = request.Method.ToUpperInvariant();

            string actionNamePrefix = GetActionMethodPrefix(httpMethod);
            if (actionNamePrefix == null)
            {
                return null;
            }

            if (odataPath.PathTemplate == "~/entityset/key/navigation" ||
                odataPath.PathTemplate == "~/entityset/key/navigation/$count" ||
                odataPath.PathTemplate == "~/entityset/key/cast/navigation" ||
                odataPath.PathTemplate == "~/entityset/key/cast/navigation/$count" ||
                odataPath.PathTemplate == "~/singleton/navigation" ||
                odataPath.PathTemplate == "~/singleton/navigation/$count" ||
                odataPath.PathTemplate == "~/singleton/cast/navigation" ||
                odataPath.PathTemplate == "~/singleton/cast/navigation/$count")
            {
                NavigationPropertySegment navigationSegment =
                    (odataPath.Segments.Last() as NavigationPropertySegment) ??
                    odataPath.Segments[odataPath.Segments.Count - 2] as NavigationPropertySegment;
                IEdmNavigationProperty navigationProperty = navigationSegment.NavigationProperty;
                IEdmEntityType declaringType = navigationProperty.DeclaringType as IEdmEntityType;

                // It is not valid to *Post* to any non-collection valued navigation property.
                if (navigationProperty.TargetMultiplicity() != EdmMultiplicity.Many &&
                    httpMethod == ODataRouteConstants.HttpPost)
                {
                    return null;
                }

                // It is not valid to *Put/Patch" to any collection-valued navigation property.
                if (navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many &&
                    (httpMethod == ODataRouteConstants.HttpPut || httpMethod == ODataRouteConstants.HttpPatch))
                {
                    return null;
                }

                // *Get* is the only supported method for $count request.
                if (odataPath.Segments.Last() is CountSegment && httpMethod != ODataRouteConstants.HttpGet)
                {
                    return null;
                }

                if (declaringType != null)
                {
                    // e.g. Try GetNavigationPropertyFromDeclaringType first, then fallback on GetNavigationProperty action name
                    ControllerActionDescriptor actionDescriptor = actionDescriptors.FindMatchingAction(
                        actionNamePrefix + navigationProperty.Name + "From" + declaringType.Name,
                        actionNamePrefix + navigationProperty.Name);

                    if (actionDescriptor != null)
                    {
                        if (odataPath.PathTemplate.StartsWith("~/entityset/key", StringComparison.Ordinal))
                        {
                            KeySegment keyValueSegment = (KeySegment)odataPath.Segments[1];
                            routeContext.AddKeyValueToRouteData(keyValueSegment);
                        }

                        return actionDescriptor;
                    }
                }
            }

            return null;
        }

        private static string GetActionMethodPrefix(string method)
        {
            switch (method)
            {
                case ODataRouteConstants.HttpGet:
                    return "Get";

                case ODataRouteConstants.HttpPost:
                    return "PostTo";

                case ODataRouteConstants.HttpPut:
                    return "PutTo";

                case ODataRouteConstants.HttpPatch:
                    return "PatchTo";

                default:
                    return null;
            }
        }
    }
}
