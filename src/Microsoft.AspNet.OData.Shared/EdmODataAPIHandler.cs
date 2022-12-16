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
    /// Handler class to handle user's methods for get, create, delete and updateRelatedObject.
    /// This is the handler for data modification where there is no CLR type.
    /// </summary>
    public abstract class EdmODataAPIHandler : IODataAPIHandler
    {
        /// <summary>
        /// Create a new object.
        /// </summary>
        /// <param name="keyValues">Key-value pairs for the entity keys.</param>
        /// <param name="createdObject">The created object (Typeless).</param>
        /// <param name="errorMessage">Any error message in case of an exception.</param>
        /// <returns>The status of the TryCreate method, statuses are <see cref="ODataAPIResponseStatus"/>.</returns>
        public abstract ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out IEdmStructuredObject createdObject, out string errorMessage);

        /// <summary>
        ///  Get the original object based on key-value pairs.
        /// </summary>
        /// <param name="keyValues">Key-value pairs for the entity keys.</param>
        /// <param name="originalObject">Object to return.</param>
        /// <param name="errorMessage">Any error message in case of an exception.</param>
        /// <returns>The status of the TryGet method, statuses are <see cref="ODataAPIResponseStatus"/>.</returns>
        public abstract ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out IEdmStructuredObject originalObject, out string errorMessage);

        /// <summary>
        ///  Delete the object based on key-value pairs.
        /// </summary>
        /// <param name="keyValues">Key-value pairs for the entity keys.</param>
        /// <param name="errorMessage">Any error message in case of an exception.</param>
        /// <returns>The status of the TryDelete method, statuses are <see cref="ODataAPIResponseStatus"/>.</returns>
        public abstract ODataAPIResponseStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage);

        /// <summary>
        /// Add related object.
        /// </summary>
        /// <param name="resource">The object to be added.</param>
        /// <param name="errorMessage">Any error message in case of an exception.</param>
        /// <returns>The status of the AddRelatedObject method <see cref="ODataAPIResponseStatus"/>.</returns>
        public abstract ODataAPIResponseStatus TryAddRelatedObject(IEdmStructuredObject resource, out string errorMessage);

        /// <summary>
        /// Get the API handler for the nested type.
        /// </summary>
        /// <param name="parent">Parent instance.</param>
        /// <param name="navigationPropertyName">The name of the navigation property for the handler.</param>
        /// <returns>The EdmODataApiHandler for the navigation property.</returns>
        public abstract EdmODataAPIHandler GetNestedHandler(IEdmStructuredObject parent, string navigationPropertyName);
    }
}
