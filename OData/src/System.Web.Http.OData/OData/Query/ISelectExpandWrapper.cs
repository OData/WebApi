// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.OData.Query;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Query
{
    /// <summary>
    /// Represents the result of a $select and $expand query operation.
    /// </summary>
    public interface ISelectExpandWrapper
    {
        /// <summary>
        /// Projects the result of a $select and $expand query to a <see cref="IDictionary{TKey,TValue}" />.
        /// </summary>
        /// <returns>An <see cref="IDictionary{TKey,TValue}"/> representing the $select and $expand result.</returns>
        IDictionary<string, object> ToDictionary();

        /// <summary>
        /// Projects the result of a $select and/or $expand query to an <see cref="IDictionary{TKey,TValue}" /> using 
        /// the given <paramref name="propertyMapperProvider"/>. The <paramref name="propertyMapperProvider"/> is used 
        /// to obtain an <see cref="IPropertyMapper"/> for the <see cref="IEdmStructuredType"/> that this 
        /// <see cref="ISelectExpandWrapper"/> instance represents. This <see cref="IPropertyMapper"/> will be used to 
        /// map the properties of the <see cref="ISelectExpandWrapper"/> instance to the keys of the 
        /// returned <see cref="IDictionary{TKey,TValue}"/>. This method can be used, for example, to map the property 
        /// names in the <see cref="IEdmStructuredType"/> to the names that should be used to serialize the properties 
        /// that this projection contains.
        /// </summary>
        /// <param name="propertyMapperProvider">
        /// A function that provides a new instance of an <see cref="IPropertyMapper"/> for a given 
        /// <see cref="IEdmStructuredType"/> and a given <see cref="IEdmModel"/>.
        /// </param>
        /// <returns>An <see cref="IDictionary{TKey,TValue}"/> representing the $select and $expand result.</returns>
        IDictionary<string, object> ToDictionary(Func<IEdmModel, IEdmStructuredType, IPropertyMapper> propertyMapperProvider);
    }
}
