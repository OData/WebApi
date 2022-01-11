//-----------------------------------------------------------------------------
// <copyright file="ODataEdmAPIHandlerFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Factory class for ODataAPIHandlers for typeless entities
    /// </summary>
    internal abstract class ODataEdmAPIHandlerFactory
    {
        protected ODataEdmAPIHandlerFactory(IEdmModel model)
        {
            Model = model;
        }

        /// <summary>
        /// Get the handler depending on navigationpath
        /// </summary>
        /// <param name="navigationPath">Navigation path corresponding to an odataid</param>
        /// <returns></returns>
        public abstract EdmODataAPIHandler GetHandler(NavigationPath navigationPath);

        /// <summary>
        /// Get the handler based on the odataPath
        /// </summary>
        /// <param name="odataPath"></param>
        /// <returns>ODataAPIHandler for the specified odata path</returns>
        public EdmODataAPIHandler GetHandler(string odataPath)
        {
            return this.GetHandler(NavigationPath.GetNavigationPath(odataPath, this.Model));
        }

        /// <summary>
        /// The IEdmModel for the Factory
        /// </summary>
        public IEdmModel Model { get; private set; }
    }
}
