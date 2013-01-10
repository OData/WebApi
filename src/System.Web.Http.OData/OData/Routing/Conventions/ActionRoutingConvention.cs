// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles action invocations.
    /// </summary>
    public class ActionRoutingConvention : EntitySetRoutingConvention
    {
        /// <summary>
        /// Selects the action for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionMap">The action map.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected action
        /// </returns>
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

            if (controllerContext.Request.Method == HttpMethod.Post)
            {
                if (odataPath.PathTemplate == "~/entityset/key/action" ||
                    odataPath.PathTemplate == "~/entityset/key/cast/action")
                {
                    ActionPathSegment actionSegment = odataPath.Segments.Last() as ActionPathSegment;
                    IEdmFunctionImport action = actionSegment.Action;

                    // The binding parameter is the first parameter by convention
                    IEdmFunctionParameter bindingParameter = action.Parameters.FirstOrDefault();
                    if (action.IsBindable && bindingParameter != null)
                    {
                        IEdmEntityType bindingParameterType = bindingParameter.Type.Definition as IEdmEntityType;
                        if (bindingParameterType != null)
                        {
                            // e.g. Try ActionOnBindingParameterType first, then fallback on Action action name
                            string actionName = actionMap.FindMatchingAction(
                                action.Name + "On" + bindingParameterType.Name,
                                action.Name);

                            if (actionName != null)
                            {
                                KeyValuePathSegment keyValueSegment = odataPath.Segments[1] as KeyValuePathSegment;
                                controllerContext.RouteData.Values[ODataRouteConstants.Key] = keyValueSegment.Value;
                                return actionName;
                            }
                        }
                    }
                }
                else if (odataPath.PathTemplate == "~/entityset/action" ||
                         odataPath.PathTemplate == "~/entityset/cast/action")
                {
                    ActionPathSegment actionSegment = odataPath.Segments.Last() as ActionPathSegment;
                    IEdmFunctionImport action = actionSegment.Action;

                    // The binding parameter is the first parameter by convention
                    IEdmFunctionParameter bindingParameter = action.Parameters.FirstOrDefault();
                    if (action.IsBindable && bindingParameter != null)
                    {
                        IEdmCollectionType bindingParameterType = bindingParameter.Type.Definition as IEdmCollectionType;
                        if (bindingParameterType != null)
                        {
                            // e.g. Try ActionOnBindingParameterType first, then fallback on Action action name
                            IEdmEntityType elementType = bindingParameterType.ElementType.Definition as IEdmEntityType;
                            return actionMap.FindMatchingAction(
                                action.Name + "OnCollectionOf" + elementType.Name,
                                action.Name);
                        }
                    }
                }
            }

            return null;
        }
    }
}