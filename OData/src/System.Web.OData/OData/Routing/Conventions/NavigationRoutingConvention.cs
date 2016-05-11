﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles navigation properties.
    /// </summary>
    public class NavigationRoutingConvention : NavigationSourceRoutingConvention
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

            HttpMethod method = controllerContext.Request.Method;
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
                NavigationPathSegment navigationSegment =
                    (odataPath.Segments.Last() as NavigationPathSegment) ??
                    odataPath.Segments[odataPath.Segments.Count - 2] as NavigationPathSegment;
                IEdmNavigationProperty navigationProperty = navigationSegment.NavigationProperty;
                IEdmEntityType declaringType = navigationProperty.DeclaringType as IEdmEntityType;

                // It is not valid to *Post* to any non-collection valued navigation property.
                if (navigationProperty.TargetMultiplicity() != EdmMultiplicity.Many &&
                    method == HttpMethod.Post)
                {
                    return null;
                }

                // It is not valid to *Put/Patch" to any collection-valued navigation property.
                if (navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many &&
                    (method == HttpMethod.Put || "PATCH" == method.Method.ToUpperInvariant()))
                {
                    return null;
                }

                // *Get* is the only supported method for $count request.
                if (odataPath.Segments.Last() is CountPathSegment && method != HttpMethod.Get)
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
                            EntitySetPathSegment entitySetPathSegment = (EntitySetPathSegment)odataPath.Segments.First();
                            IEdmEntityType edmEntityType = entitySetPathSegment.EntitySetBase.EntityType();
                            KeyValuePathSegment keyValueSegment = (KeyValuePathSegment)odataPath.Segments[1];

                            controllerContext.AddKeyValueToRouteData(keyValueSegment, edmEntityType, ODataRouteConstants.Key);
                        }

                        return actionName;
                    }
                }
            }

            return null;
        }

        private static string GetActionMethodPrefix(HttpMethod method)
        {
            switch (method.Method.ToUpperInvariant())
            {
                case "GET":
                    return "Get";
                case "POST":
                    return "PostTo";
                case "PUT":
                    return "PutTo";
                case "PATCH":
                    return "PatchTo";
                default:
                    return null;
            }
        }
    }
}
