// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Microsoft.AspNet.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi action map to OData WebApi.
    /// </summary>
    internal class WebApiActionMap : IWebApiActionMap
    {
        /// <summary>
        /// The inner map wrapped by this instance.
        /// </summary>
        private IEnumerable<ControllerActionDescriptor> innerMap;

        /// <summary>
        /// Initializes a new instance of the WebApiActionMap class.
        /// </summary>
        /// <param name="actionMap">The inner map.</param>
        public WebApiActionMap(IEnumerable<ControllerActionDescriptor> actionMap)
        {
            if (actionMap == null)
            {
                throw Error.ArgumentNull("actionMap");
            }

            this.innerMap = actionMap;
        }

        /// <summary>
        /// Determines whether a specified action exists.
        /// </summary>
        /// <param name="name">The action name.</param>
        /// <returns>True if the action name exist; false otherwise.</returns>
        public bool Contains(string name)
        {
            return this.innerMap.Any(a => String.Equals(a.ActionName, name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
