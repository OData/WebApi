//-----------------------------------------------------------------------------
// <copyright file="ODataAPIHandlerFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Factory class for OData API handlers for entities mapped to CLR types.
    /// </summary>
    public abstract class ODataAPIHandlerFactory
    {
        /// <summary>
        /// Creates an instance of an ODataAPIHandlerFactory with the given model.
        /// </summary>
        /// <param name="model">The IEdmModel for the API handler factory.</param>
        protected ODataAPIHandlerFactory(IEdmModel model)
        {
            Model = model;
        }

        /// <summary>
        /// The IEdmModel for the factory.
        /// </summary>
        public IEdmModel Model { get; }

        /// <summary>
        /// Get the handler depending on OData path.
        /// </summary>
        /// <param name="odataPath">OData path corresponding to an @odata.id.</param>
        /// <returns>ODataAPIHandler for the specified OData path.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "1#")]
        public abstract IODataAPIHandler GetHandler(ODataPath odataPath);

        /// <summary>
        /// Get the handler based on the OData path uri string.
        /// </summary>
        /// <param name="path">OData path uri string.</param>
        /// <returns>ODataAPIHandler for the specified odata path uri string.</returns>
        internal IODataAPIHandler GetHandler(string path)
        {
            ODataUriParser parser = new ODataUriParser(this.Model, new Uri(path, UriKind.Relative));
            ODataPath odataPath = parser.ParsePath();

            return this.GetHandler(odataPath);
        }
    }
}
