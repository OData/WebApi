// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Interfaces;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// Provides helper methods for querying an action map.
    /// </summary>
    internal static class ActionMapExtensions
    {
        public static string FindMatchingAction(this IWebApiActionMap actionMap, params string[] targetActionNames)
        {
            foreach (string targetActionName in targetActionNames)
            {
                if (actionMap.Contains(targetActionName))
                {
                    return targetActionName;
                }
            }

            return null;
        }
    }
}