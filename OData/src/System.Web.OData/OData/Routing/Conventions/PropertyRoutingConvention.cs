// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles reading structural properties.
    /// </summary>
    public class PropertyRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <inheritdoc/>
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            if (odataPath == null)
            {
                throw Error.ArgumentNull("odataPath");
            }

            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            if (actionMap == null)
            {
                throw Error.ArgumentNull("actionMap");
            }

            IEdmProperty property = GetProperty(odataPath, controllerContext.Request.Method);
            IEdmEntityType declaringType = property == null ? null : property.DeclaringType as IEdmEntityType;

            if (declaringType != null)
            {
                // e.g. Try GetPropertyFromDeclaringType first, then fallback on GetProperty action name
                string actionName = actionMap.FindMatchingAction(
                    "Get" + property.Name + "From" + declaringType.Name,
                    "Get" + property.Name);

                if (actionName != null)
                {
                    if (odataPath.PathTemplate.StartsWith("~/entityset/key", StringComparison.Ordinal))
                    {
                        KeyValuePathSegment keyValueSegment = odataPath.Segments[1] as KeyValuePathSegment;
                        controllerContext.RouteData.Values[ODataRouteConstants.Key] = keyValueSegment.Value;
                    }

                    return actionName;
                }
            }

            return null;
        }

        private static IEdmProperty GetProperty(ODataPath odataPath, HttpMethod method)
        {
            PropertyAccessPathSegment segment = null;
            if (method == HttpMethod.Get)
            {
                if (odataPath.PathTemplate == "~/entityset/key/property" ||
                    odataPath.PathTemplate == "~/entityset/key/cast/property" ||
                    odataPath.PathTemplate == "~/singleton/property" ||
                    odataPath.PathTemplate == "~/singleton/cast/property")
                {
                    segment = odataPath.Segments[odataPath.Segments.Count - 1] as PropertyAccessPathSegment;
                }
                else if (odataPath.PathTemplate == "~/entityset/key/property/$value" ||
                    odataPath.PathTemplate == "~/entityset/key/cast/property/$value" ||
                    odataPath.PathTemplate == "~/singleton/property/$value" ||
                    odataPath.PathTemplate == "~/singleton/cast/property/$value" ||
                    odataPath.PathTemplate == "~/entityset/key/property/$count" ||
                    odataPath.PathTemplate == "~/entityset/key/cast/property/$count" ||
                    odataPath.PathTemplate == "~/singleton/property/$count" ||
                    odataPath.PathTemplate == "~/singleton/cast/property/$count")
                {
                    segment = odataPath.Segments[odataPath.Segments.Count - 2] as PropertyAccessPathSegment;
                }
            }

            return segment == null ? null : segment.Property;
        }
    }
}
