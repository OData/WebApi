// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Query
{
    /// <summary>
    /// The result of a $select and $expand projection is represented as an <see cref="ISelectExpandWrapper"/>
    /// instance. That instance can be projected into an <see cref="IDictionary{TKey,TValue}"/> instance by calling
    /// <see cref="ISelectExpandWrapper.ToDictionary(Func{IEdmModel,IEdmStructuredType,IPropertyMapper})"/>. 
    /// That method will use the function to construct an <see cref="IPropertyMapper"/> that will map the property 
    /// names in that projection to the keys in the returned <see cref="IDictionary{TKey,TValue}"/>.
    /// The main purpose of converting an <see cref="ISelectExpandWrapper"/> instance into an 
    /// <see cref="IDictionary{TKey,TValue}"/> (using the method mentioned above) is to allow changing the names of the 
    /// properties in the <see cref="IEdmStructuredType"/> that will be used during the serialization of the $select 
    /// and $expand projection by a given formatter. For example, to support custom serialization attributes of a
    /// particular formatter.
    /// </summary>
    public interface IPropertyMapper
    {
        /// <summary>
        /// Defines a mapping between the name of an <see cref="IEdmProperty"/> of an <see cref="IEdmStructuredType"/>
        /// and the name that should be used in other contexts, for example, when projecting an instance of an 
        /// <see cref="ISelectExpandWrapper"/> into an instance of an <see cref="IDictionary{TKey,TValue}"/>
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property in the <see cref="IEdmStructuredType" /> represented
        /// by this instance of <see cref="ISelectExpandWrapper"/>.
        /// </param>
        /// <returns>
        /// The value that will be used as the key for this property in the <see cref="IDictionary{TKey,TValue}" />
        /// resulting from calling ToDictionary on an <see cref="ISelectExpandWrapper"/> instance.
        /// </returns>
        string MapProperty(string propertyName);
    }
}