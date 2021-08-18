//-----------------------------------------------------------------------------
// <copyright file="EdmTypeExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Extension methods for the <see cref="IEdmType"/> interface.
    /// </summary>
    public static class EdmTypeExtensions
    {
        /// <summary>
        /// Method to determine whether the current type is a Delta Feed
        /// </summary>
        /// <param name="type">IEdmType to be compared</param>
        /// <returns>True or False if type is same as <see cref="EdmDeltaCollectionType"/></returns>
        public static bool IsDeltaFeed(this IEdmType type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            return (type.GetType() == typeof(EdmDeltaCollectionType));
        }
        
        /// <summary>
        /// Method to determine whether the current Edm object is a Delta Entry
        /// </summary>
        /// <param name="resource">IEdmObject to be compared</param>
        /// <returns>True or False if type is same as <see cref="EdmDeltaEntityObject"/> or <see cref="EdmDeltaComplexObject"/></returns>
        public static bool IsDeltaResource(this IEdmObject resource)
        {
            if (resource == null)
            {
                throw Error.ArgumentNull("resource");
            }
            return (resource is EdmDeltaEntityObject || resource is EdmDeltaComplexObject);
        }
    }
}
