//-----------------------------------------------------------------------------
// <copyright file="ODataEdmAPIHandlerFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Factory class for ODataAPIHandlers for typeless entities
    /// </summary>
    internal abstract class ODataEdmAPIHandlerFactory
    {
        /// <summary>
        /// Get the handler depending on navigationpath
        /// </summary>
        /// <param name="navigationPath">Navigation path corresponding to an odataid</param>
        /// <returns></returns>
        public abstract EdmODataAPIHandler GetHandler(NavigationPath navigationPath);
    }
}
