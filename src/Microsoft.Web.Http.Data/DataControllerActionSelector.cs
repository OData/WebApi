// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Microsoft.Web.Http.Data
{
    public sealed class DataControllerActionSelector : ApiControllerActionSelector
    {
        private const string ActionRouteKey = "action";
        private const string SubmitActionValue = "Submit";

        public override HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
        {
            // first check to see if this is a call to Submit
            string actionName;
            if (controllerContext.RouteData.Values.TryGetValue(ActionRouteKey, out actionName) && actionName.Equals(SubmitActionValue, StringComparison.Ordinal))
            {
                return new SubmitActionDescriptor(controllerContext.ControllerDescriptor, controllerContext.Controller.GetType());
            }

            // next check to see if this is a direct invocation of a CUD action
            DataControllerDescription description = DataControllerDescription.GetDescription(controllerContext.ControllerDescriptor);
            UpdateActionDescriptor action = description.GetUpdateAction(actionName);
            if (action != null)
            {
                return new SubmitProxyActionDescriptor(action);
            }

            // for all other non-CUD operations, we wrap the descriptor in our
            // customizing descriptor to layer on additional functionality.
            return new CustomizingActionDescriptor(base.SelectAction(controllerContext));
        }
    }
}
