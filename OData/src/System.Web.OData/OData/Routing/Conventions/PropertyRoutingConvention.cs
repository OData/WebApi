// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
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

            string prefix;
            ComplexCastPathSegment cast;
            IEdmProperty property = GetProperty(odataPath, controllerContext.Request.Method, out prefix, out cast);
            IEdmEntityType declaringType = property == null ? null : property.DeclaringType as IEdmEntityType;

            if (declaringType != null)
            {
                string actionName;
                if (cast == null)
                {
                    actionName = actionMap.FindMatchingAction(
                        prefix + property.Name + "From" + declaringType.Name,
                        prefix + property.Name);
                }
                else
                {
                    // for example: GetCityOfSubAddressFromVipCustomer or GetCityOfSubAddress
                    actionName = actionMap.FindMatchingAction(
                        prefix + property.Name + "Of" + cast.CastType.Name + "From" + declaringType.Name,
                        prefix + property.Name + "Of" + cast.CastType.Name);
                }

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

            return null;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity",
            Justification = "These are simple conversion function and cannot be split up.")]
        private static IEdmProperty GetProperty(ODataPath odataPath, HttpMethod method, out string prefix,
            out ComplexCastPathSegment cast)
        {
            prefix = String.Empty;
            cast = null;
            PropertyAccessPathSegment segment = null;

            if (odataPath.PathTemplate == "~/entityset/key/property" ||
                odataPath.PathTemplate == "~/entityset/key/cast/property" ||
                odataPath.PathTemplate == "~/singleton/property" ||
                odataPath.PathTemplate == "~/singleton/cast/property")
            {
                PropertyAccessPathSegment tempSegment =
                    (PropertyAccessPathSegment)odataPath.Segments[odataPath.Segments.Count - 1];

                switch (method.Method.ToUpperInvariant())
                {
                    case "GET":
                        prefix = "Get";
                        segment = tempSegment;
                        break;
                    case "PUT":
                        prefix = "PutTo";
                        segment = tempSegment;
                        break;
                    case "PATCH":
                        // OData Spec: PATCH is not supported for collection properties.
                        if (!tempSegment.Property.Type.IsCollection())
                        {
                            prefix = "PatchTo";
                            segment = tempSegment;
                        }
                        break;
                    case "DELETE":
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
            else if (odataPath.PathTemplate == "~/entityset/key/property/complexcast" ||
                     odataPath.PathTemplate == "~/entityset/key/cast/property/complexcast" ||
                     odataPath.PathTemplate == "~/singleton/property/complexcast" ||
                     odataPath.PathTemplate == "~/singleton/cast/property/complexcast")
            {
                PropertyAccessPathSegment tempSegment =
                    (PropertyAccessPathSegment)odataPath.Segments[odataPath.Segments.Count - 2];
                ComplexCastPathSegment tempCast = (ComplexCastPathSegment)odataPath.Segments.Last();
                switch (method.Method.ToUpperInvariant())
                {
                    case "GET":
                        prefix = "Get";
                        segment = tempSegment;
                        cast = tempCast;
                        break;
                    case "PUT":
                        prefix = "PutTo";
                        segment = tempSegment;
                        cast = tempCast;
                        break;
                    case "PATCH":
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
                PropertyAccessPathSegment tempSegment = (PropertyAccessPathSegment)odataPath.Segments[odataPath.Segments.Count - 2];
                switch (method.Method.ToUpperInvariant())
                {
                    case "GET":
                        prefix = "Get";
                        segment = tempSegment;
                        break;
                }
            }

            return segment == null ? null : segment.Property;
        }
    }
}
