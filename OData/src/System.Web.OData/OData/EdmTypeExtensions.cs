// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using Microsoft.OData.Edm;

namespace System.Web.OData
{
    /// <summary>
    /// Extension methods for the <see cref="IEdmType"/> interface.
    /// </summary>
    public static class EdmTypeExtensions
    {
        /// <summary>
        /// Method to determine whether the current type is containing a Delta Feed
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
        /// Method to determine whether the current type is containing a Delta Entry
        /// </summary>
        /// <param name="type">IEdmType to be compared</param>
        /// <returns>True or False if type is same as <see cref="EdmDeltaEntityObject"/></returns>
        public static bool IsDeltaObject(this IEdmObject type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            return (type is EdmDeltaEntityObject || type is EdmDeltaComplexObject);
        }

    }
}