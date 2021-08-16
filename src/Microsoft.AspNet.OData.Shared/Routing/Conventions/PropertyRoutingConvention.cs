//-----------------------------------------------------------------------------
// <copyright file="PropertyRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles reading structural properties.
    /// </summary>
    public partial class PropertyRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <inheritdoc/>
        internal static string SelectActionImpl(ODataPath odataPath, IWebApiControllerContext controllerContext, IWebApiActionMap actionMap)
        {
            string prefix;
            TypeSegment cast;
            IEdmProperty property = GetProperty(odataPath, controllerContext.Request.GetRequestMethodOrPreflightMethod(), out prefix, out cast);
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
                    actionName = actionMap.FindMatchingAction(
                        prefix + property.Name + "Of" + typeCast.Name + "From" + declaringType.Name,
                        prefix + property.Name + "Of" + typeCast.Name);
                }

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

            return null;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity",
            Justification = "These are simple conversion function and cannot be split up.")]
        private static IEdmProperty GetProperty(ODataPath odataPath, ODataRequestMethod method, out string prefix,
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
                    case ODataRequestMethod.Get:
                        prefix = "Get";
                        segment = tempSegment;
                        break;
                    case ODataRequestMethod.Post:
                        //Allow post only to collection properties
                        if (tempSegment.Property.Type.IsCollection())
                        {
                            prefix = "PostTo";
                            segment = tempSegment;
                        }
                        break;
                    case ODataRequestMethod.Put:
                        prefix = "PutTo";
                        segment = tempSegment;
                        break;
                    case ODataRequestMethod.Patch:
                        // OData Spec: PATCH is not supported for collection properties.
                        if (!tempSegment.Property.Type.IsCollection())
                        {
                            prefix = "PatchTo";
                            segment = tempSegment;
                        }
                        break;
                    case ODataRequestMethod.Delete:
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
                    case ODataRequestMethod.Get:
                        prefix = "Get";
                        segment = tempSegment;
                        cast = tempCast;
                        break;
                    case ODataRequestMethod.Post:
                        //Allow post only to collection properties
                        if (tempSegment.Property.Type.IsCollection())
                        {
                            prefix = "PostTo";
                            segment = tempSegment;
                            cast = tempCast;
                        }
                        break;
                    case ODataRequestMethod.Put:
                        prefix = "PutTo";
                        segment = tempSegment;
                        cast = tempCast;
                        break;
                    case ODataRequestMethod.Patch:
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
                    case ODataRequestMethod.Get:
                        prefix = "Get";
                        segment = tempSegment;
                        break;
                }
            }

            return segment == null ? null : segment.Property;
        }
    }
}
