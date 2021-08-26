// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Factory class for ODataAPIHandlers for typeless entities
    /// </summary>
    public abstract class ODataEdmAPIHandlerFactory
    {
        /// <summary>
        /// Get the handler depending on navigationpath
        /// </summary>
        /// <param name="navigationPath">Navigation path corresponding to an odataid</param>
        /// <returns></returns>
        public abstract EdmODataAPIHandler GetHandler(NavigationPath navigationPath);
    }
}
