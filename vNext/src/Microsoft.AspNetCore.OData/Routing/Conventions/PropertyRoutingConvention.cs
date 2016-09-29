// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles reading structural properties.
    /// </summary>
    public class PropertyRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <inheritdoc/>
        public override ActionDescriptor SelectAction(RouteContext routeContext, IEnumerable<ControllerActionDescriptor> actionDescriptors)
        {
            if (routeContext == null)
            {
                throw Error.ArgumentNull("routeContext");
            }

            ODataPath odataPath = routeContext.HttpContext.ODataFeature().Path;
            HttpRequest request = routeContext.HttpContext.Request;
            string httpMethod = request.Method.ToUpperInvariant();

            string prefix;
            TypeSegment cast;
            IEdmProperty property = GetProperty(odataPath, httpMethod, out prefix, out cast);
            IEdmEntityType declaringType = property == null ? null : property.DeclaringType as IEdmEntityType;

            if (declaringType != null)
            {
                ControllerActionDescriptor actionDescriptor;
                if (cast == null)
                {
                    actionDescriptor = actionDescriptors.FindMatchingAction(
                        prefix + property.Name + "From" + declaringType.Name,
                        prefix + property.Name);
                }
                else
                {
                    IEdmComplexType typeCast;
                    if (cast.EdmType.TypeKind == EdmTypeKind.Collection)
                    {
                        typeCast = ((IEdmCollectionType)cast.EdmType).ElementType.AsComplex().ComplexDefinition();
                    }
                    else
                    {
                        typeCast = (IEdmComplexType)cast.EdmType;
                    }

                    // for example: GetCityOfSubAddressFromVipCustomer or GetCityOfSubAddress
                    actionDescriptor = actionDescriptors.FindMatchingAction(
                        prefix + property.Name + "Of" + typeCast.Name + "From" + declaringType.Name,
                        prefix + property.Name + "Of" + typeCast.Name);
                }

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

            return null;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity",
            Justification = "These are simple conversion function and cannot be split up.")]
        private static IEdmProperty GetProperty(ODataPath odataPath, string method, out string prefix,
            out TypeSegment cast)
        {
            prefix = String.Empty;
            cast = null;
            PropertySegment segment = null;

            if (odataPath.PathTemplate == "~/entityset/key/property" ||
                odataPath.PathTemplate == "~/entityset/key/cast/property" ||
                odataPath.PathTemplate == "~/singleton/property" ||
                odataPath.PathTemplate == "~/singleton/cast/property")
            {
                PropertySegment tempSegment =
                    (PropertySegment)odataPath.Segments[odataPath.Segments.Count - 1];

                switch (method)
                {
                    case ODataRouteConstants.HttpGet:
                        prefix = "Get";
                        segment = tempSegment;
                        break;

                    case ODataRouteConstants.HttpPut:
                        prefix = "PutTo";
                        segment = tempSegment;
                        break;

                    case ODataRouteConstants.HttpPatch:
                        // OData Spec: PATCH is not supported for collection properties.
                        if (!tempSegment.Property.Type.IsCollection())
                        {
                            prefix = "PatchTo";
                            segment = tempSegment;
                        }
                        break;

                    case ODataRouteConstants.HttpDelete:
                        // OData spec: A successful DELETE request to the edit URL for a structural property, ... sets the property to null.
                        // The request body is ignored and should be empty.
                        // DELETE request to a non-nullable value MUST fail and the service respond with 400 Bad Request or other appropriate error.
                        if (tempSegment.Property.Type.IsNullable)
                        {
                            prefix = "DeleteTo";
                            segment = tempSegment;
                        }
                        break;
                }
            }
            else if (odataPath.PathTemplate == "~/entityset/key/property/cast" ||
                     odataPath.PathTemplate == "~/entityset/key/cast/property/cast" ||
                     odataPath.PathTemplate == "~/singleton/property/cast" ||
                     odataPath.PathTemplate == "~/singleton/cast/property/cast")
            {
                PropertySegment tempSegment =
                    (PropertySegment)odataPath.Segments[odataPath.Segments.Count - 2];
                TypeSegment tempCast = (TypeSegment)odataPath.Segments.Last();
                switch (method)
                {
                    case ODataRouteConstants.HttpGet:
                        prefix = "Get";
                        segment = tempSegment;
                        cast = tempCast;
                        break;

                    case ODataRouteConstants.HttpPut:
                        prefix = "PutTo";
                        segment = tempSegment;
                        cast = tempCast;
                        break;

                    case ODataRouteConstants.HttpPatch:
                        // PATCH is not supported for collection properties.
                        if (!tempSegment.Property.Type.IsCollection())
                        {
                            prefix = "PatchTo";
                            segment = tempSegment;
                            cast = tempCast;
                        }
                        break;
                }
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
                PropertySegment tempSegment = (PropertySegment)odataPath.Segments[odataPath.Segments.Count - 2];
                switch (method)
                {
                    case ODataRouteConstants.HttpGet:
                        prefix = "Get";
                        segment = tempSegment;
                        break;
                }
            }

            return segment == null ? null : segment.Property;
        }
    }
}
