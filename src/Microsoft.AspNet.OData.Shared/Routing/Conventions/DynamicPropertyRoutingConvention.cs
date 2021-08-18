//-----------------------------------------------------------------------------
// <copyright file="DynamicPropertyRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles dynamic properties for open type.
    /// </summary>
    public partial class DynamicPropertyRoutingConvention : NavigationSourceRoutingConvention
    {
        private const string ActionName = "DynamicProperty";

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity",
            Justification = "These is simple conversion function based on OData path value and cannot be split up.")]
        internal static string SelectActionImpl(ODataPath odataPath, IWebApiControllerContext controllerContext,
            IWebApiActionMap actionMap)
        {
            string actionName = null;
            DynamicPathSegment dynamicPropertSegment = null;

            switch (odataPath.PathTemplate)
            {
                case "~/entityset/key/dynamicproperty":
                case "~/entityset/key/cast/dynamicproperty":
                case "~/singleton/dynamicproperty":
                case "~/singleton/cast/dynamicproperty":
                    dynamicPropertSegment = odataPath.Segments.Last() as DynamicPathSegment;
                    if (dynamicPropertSegment == null)
                    {
                        return null;
                    }

                    if (ODataRequestMethod.Get == controllerContext.Request.GetRequestMethodOrPreflightMethod())
                    {
                        string actionNamePrefix = String.Format(CultureInfo.InvariantCulture, "Get{0}", ActionName);
                        actionName = actionMap.FindMatchingAction(actionNamePrefix);
                    }
                    break;
                case "~/entityset/key/property/dynamicproperty":
                case "~/entityset/key/cast/property/dynamicproperty":
                case "~/singleton/property/dynamicproperty":
                case "~/singleton/cast/property/dynamicproperty":
                    dynamicPropertSegment = odataPath.Segments.Last() as DynamicPathSegment;
                    if (dynamicPropertSegment == null)
                    {
                        return null;
                    }

                    PropertySegment propertyAccessSegment = odataPath.Segments[odataPath.Segments.Count - 2]
                            as PropertySegment;
                    if (propertyAccessSegment == null)
                    {
                        return null;
                    }

                    EdmComplexType complexType = propertyAccessSegment.Property.Type.Definition as EdmComplexType;
                    if (complexType == null)
                    {
                        return null;
                    }

                    if (ODataRequestMethod.Get == controllerContext.Request.GetRequestMethodOrPreflightMethod())
                    {
                        string actionNamePrefix = String.Format(CultureInfo.InvariantCulture, "Get{0}", ActionName);
                        actionName = actionMap.FindMatchingAction(actionNamePrefix + "From" + propertyAccessSegment.Property.Name);
                    }
                    break;
                default: break;
            }

            if (actionName != null)
            {
                if (odataPath.PathTemplate.StartsWith("~/entityset/key", StringComparison.Ordinal))
                {
                    KeySegment keyValueSegment = (KeySegment)odataPath.Segments[1];
                    controllerContext.AddKeyValueToRouteData(keyValueSegment);
                }

                controllerContext.RouteData.Add(ODataRouteConstants.DynamicProperty, dynamicPropertSegment.Identifier);
                var key = ODataParameterValue.ParameterValuePrefix + ODataRouteConstants.DynamicProperty;
                var value = new ODataParameterValue(dynamicPropertSegment.Identifier, EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string)));
                controllerContext.RouteData.Add(key, value);
                controllerContext.Request.Context.RoutingConventionsStore.Add(key, value);
                return actionName;
            }
            return null;
        }
    }
}
