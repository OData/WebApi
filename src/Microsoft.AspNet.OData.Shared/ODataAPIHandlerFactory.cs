//-----------------------------------------------------------------------------
// <copyright file="ODataAPIHandlerFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Factory class for ODataAPIHandlers
    /// </summary>
    internal abstract class ODataAPIHandlerFactory
    {
        /// <summary>
        /// Creates an instance of an ODataAPIHandlerFactory with the given moel
        /// </summary>
        /// <param name="model">The IEdmModel for the API Handler Factory</param>
        protected ODataAPIHandlerFactory(IEdmModel model)
        {
            Model = model;
        }

        /// <summary>
        /// Get the handler depending on navigationpath
        /// </summary>
        /// <param name="navigationPath">Navigation path corresponding to an odataid</param>
        /// <returns>ODataAPIHandler for the specified navigation path</returns>
        public abstract IODataAPIHandler GetHandler(NavigationPath navigationPath);

        /// <summary>
        /// Get the handler based on the odataPath
        /// </summary>
        /// <param name="odataPath"></param>
        /// <returns>ODataAPIHandler for the specified odata path</returns>
        public IODataAPIHandler GetHandler(string odataPath)
        {
            return this.GetHandler(new NavigationPath(odataPath, this.Model));
        }

        /// <summary>
        /// The IEdmModel for the Factory
        /// </summary>
        public IEdmModel Model { get; private set; }
    }
}
