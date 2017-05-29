// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;

namespace System.Web.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles dynamic properties for open type.
    /// </summary>
    public class DynamicPropertyRoutingConvention : NavigationSourceRoutingConvention
    {
        private readonly string _actionName = "DynamicProperty";

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Relies on many ODataLib classes.")]
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext,
            ILookup<string, HttpActionDescriptor> actionMap)
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

            string actionName = null;
            DynamicPropertyPathSegment dynamicPropertSegment = null;

            switch (odataPath.PathTemplate)
            {
                case "~/entityset/key/dynamicproperty":
                case "~/entityset/key/cast/dynamicproperty":
                case "~/singleton/dynamicproperty":
                case "~/singleton/cast/dynamicproperty":
                    dynamicPropertSegment = odataPath.Segments[odataPath.Segments.Count - 1] as DynamicPropertyPathSegment;
                    if (dynamicPropertSegment == null)
                    {
                        return null;
                    }

                    if (controllerContext.Request.Method == HttpMethod.Get)
                    {
                        string actionNamePrefix = String.Format(CultureInfo.InvariantCulture, "Get{0}", _actionName);
                        actionName = actionMap.FindMatchingAction(actionNamePrefix);
                    }
                    break;
                case "~/entityset/key/property/dynamicproperty":
                case "~/entityset/key/cast/property/dynamicproperty":
                case "~/singleton/property/dynamicproperty":
                case "~/singleton/cast/property/dynamicproperty":
                    dynamicPropertSegment = odataPath.Segments[odataPath.Segments.Count - 1] as DynamicPropertyPathSegment;
                    if (dynamicPropertSegment == null)
                    {
                        return null;
                    }

                    PropertyAccessPathSegment propertyAccessSegment = odataPath.Segments[odataPath.Segments.Count - 2]
                            as PropertyAccessPathSegment;
                    if (propertyAccessSegment == null)
                    {
                        return null;
                    }

                    EdmComplexType complexType = propertyAccessSegment.Property.Type.Definition as EdmComplexType;
                    if (complexType == null)
                    {
                        return null;
                    }

                    if (controllerContext.Request.Method == HttpMethod.Get)
                    {
                        string actionNamePrefix = String.Format(CultureInfo.InvariantCulture, "Get{0}", _actionName);
                        actionName = actionMap.FindMatchingAction(actionNamePrefix + "From" + propertyAccessSegment.Property.Name);
                    }
                    break;
                default: break;
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

                controllerContext.RouteData.Values[ODataRouteConstants.DynamicProperty] = dynamicPropertSegment.PropertyName;
                var key = ODataParameterValue.ParameterValuePrefix + ODataRouteConstants.DynamicProperty;
                var value = new ODataParameterValue(dynamicPropertSegment.PropertyName, EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string)));
                controllerContext.Request.ODataProperties().RoutingConventionsStore[key] = value;
                return actionName;
            }
            return null;
        }
    }
}