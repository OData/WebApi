//-----------------------------------------------------------------------------
// <copyright file="EdmODataAPIHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNet.OData
{

    /// <summary>
    /// Handler Class to handle users methods for create, delete and update.
    /// This is the handler for data modification where there is no CLR type.
    /// </summary>
    public abstract class EdmODataAPIHandler
    {
        /// <summary>
        /// TryCreate method to create a new object.
        /// </summary>
        /// <param name="changedObject">Changed object which can be applied on created object, optional.</param>
        /// <param name="createdObject">The created object (Typeless).</param>
        /// <param name="errorMessage">Any error message in case of an exception.</param>
        /// <returns>The status of the TryCreate method, statuses are <see cref="ODataAPIResponseStatus"/>.</returns>
        public abstract ODataAPIResponseStatus TryCreate(IEdmChangedObject changedObject, out IEdmStructuredObject createdObject, out string errorMessage);

        /// <summary>
        ///  TryGet method which tries to get the original object based on a key-values.
        /// </summary>
        /// <param name="keyValues">Key-value pair for the entity keys.</param>
        /// <param name="originalObject">Object to return.</param>
        /// <param name="errorMessage">Any error message in case of an exception.</param>
        /// <returns>The status of the TryGet method, statuses are <see cref="ODataAPIResponseStatus"/>.</returns>
        public abstract ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out IEdmStructuredObject originalObject, out string errorMessage);

        /// <summary>
        ///  TryDelete Method which will delete the object based on key-value pairs.
        /// </summary>
        /// <param name="keyValues">Key-value pair for the entity keys.</param>
        /// <param name="errorMessage">Any error message in case of an exception.</param>
        /// <returns>The status of the TryDelete method, statuses are <see cref="ODataAPIResponseStatus"/>.</returns>
        public abstract ODataAPIResponseStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage);

        /// <summary>
        /// Get the API handler for the nested type.
        /// </summary>
        /// <param name="parent">Parent instance.</param>
        /// <param name="navigationPropertyName">The name of the navigation property for the handler.</param>
        /// <returns>The EdmODataApiHandler for the navigation property.</returns>
        public abstract EdmODataAPIHandler GetNestedHandler(IEdmStructuredObject parent, string navigationPropertyName);
    }
}
