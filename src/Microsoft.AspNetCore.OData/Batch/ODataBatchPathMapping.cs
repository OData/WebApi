// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// A class for storing batch route names and prefixes used to determine if a route is a
    /// batch route.
    /// </summary>
    public class ODataBatchPathMapping
    {
        private Dictionary<string, string> templateMappings = new Dictionary<string, string>();

        /// <summary>
        /// Add a route name and template for batching.
        /// </summary>
        /// <param name="routeName"></param>
        /// <param name="routeTemplate"></param>
        public void AddRoute(string routeName, string routeTemplate)
        {
            templateMappings[routeTemplate] = routeName;
        }

        /// <summary>
        /// Try and get the batch handler for a given path.
        /// </summary>
        /// <param name="path">The request path to match against the templates.</param>
        /// <param name="routeName">The route name if found or null.</param>
        /// <returns>true if a route name is found, otherwise false.</returns>
        public bool TryGetRouteName(string path, out string routeName)
        {
            return templateMappings.TryGetValue(path, out routeName);
        }
    }
}
