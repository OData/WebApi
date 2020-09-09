// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles navigation properties.
    /// </summary>
    public partial class NavigationRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <inheritdoc/>
        internal static string SelectActionImpl(ODataPath odataPath, IWebApiControllerContext controllerContext, IWebApiActionMap actionMap)
        {
            ODataRequestMethod method = controllerContext.Request.GetRequestMethodOrPreflightMethod();
            string actionNamePrefix = GetActionMethodPrefix(method);
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
                    ODataRequestMethod.Post == method)
                {
                    return null;
                }

                // It is not valid to *Put/Patch" to any collection-valued navigation property.
                if (navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many &&
                    ODataRequestMethod.Put == method)
                {
                    return null;
                }

                // *Get* is the only supported method for $count request.
                if (odataPath.Segments.Last() is CountSegment && ODataRequestMethod.Get != method)
                {
                    return null;
                }

                if (declaringType != null)
                {
                    // e.g. Try GetNavigationPropertyFromDeclaringType first, then fallback on GetNavigationProperty action name
                    string actionName = actionMap.FindMatchingAction(
                        actionNamePrefix + navigationProperty.Name + "From" + declaringType.Name,
                        actionNamePrefix + navigationProperty.Name);

                    if (actionName != null)
                    {
                        if (odataPath.PathTemplate.StartsWith("~/entityset/key", StringComparison.Ordinal))
                        {
                            KeySegment keyValueSegment = (KeySegment)odataPath.Segments[1];
                            controllerContext.AddKeyValueToRouteData(keyValueSegment);
                        }

                        return actionName;
                    }
                }
            }

            return null;
        }

        private static string GetActionMethodPrefix(ODataRequestMethod method)
        {
            switch (method)
            {
                case ODataRequestMethod.Get:
                    return "Get";
                case ODataRequestMethod.Post:
                    return "PostTo";
                case ODataRequestMethod.Put:
                    return "PutTo";
                case ODataRequestMethod.Patch:
                    return "PatchTo";
                default:
                    return null;
            }
        }
    }
}
