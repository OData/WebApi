// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// Provides helper methods for querying an action map.
    /// </summary>
    public static class ControllerActionDescriptorExtensions
    {
        /// <summary>
        /// Find the matching action descriptor.
        /// </summary>
        /// <param name="controllerActionDescriptors">The list of action descriptor.</param>
        /// <param name="targetActionNames">The target action name.</param>
        /// <returns></returns>
        public static ControllerActionDescriptor FindMatchingAction(
            this IEnumerable<ControllerActionDescriptor> controllerActionDescriptors, params string[] targetActionNames)
        {
            return targetActionNames.Select(
                targetActionName => controllerActionDescriptors.FirstOrDefault(
                    c => String.Equals(c.ActionName, targetActionName, StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault(controllerActionDescriptor => controllerActionDescriptor != null);
        }
    }
}
