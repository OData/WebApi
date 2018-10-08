// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Exposes the ability to change the behavior on how CLR types and instances are mapped to their corresponding <see cref="IEdmType"/>. 
    /// </summary>
    public interface IEdmModelClrTypeMappingHandler
    {
        /// <summary>
        /// Maps a given CLR type to its corresponding <see cref="IEdmType"/>
        /// </summary>
        /// <param name="edmModel">The EDM model for which the conversion should be performed.</param>
        /// <param name="clrType">The CLR type that should be mapped.</param>
        /// <returns>The <see cref="IEdmType"/> that corresponds for the given CLR type or <code>null</code> if default mapping should be performed.</returns>
        IEdmType MapClrTypeToEdmType(IEdmModel edmModel, Type clrType);

        /// <summary>
        /// Maps a given CLR collection to its corresponding <see cref="IEdmCollectionType"/>
        /// </summary>
        /// <param name="edmModel">The EDM model for which the conversion should be performed.</param>
        /// <param name="clrType">The CLR type of the collection that should be mapped. Typically a type assignable to <see cref="IEnumerable{T}"/>.</param>
        /// <param name="elementClrType">The CLR type of the items contained in the collection.</param>
        /// <remarks>
        /// The element type contained in the <paramref name="clrType"/> parameter might consist of OData specific wrapper objects. The <paramref name="elementClrType"/>
        /// parameter represents the unwrapped CLR type. 
        /// </remarks>
        /// <returns>The <see cref="IEdmCollectionType"/> that corresponds to the given CLR type or <code>null</code> if default mapping should be performed.</returns>
        IEdmCollectionType MapClrEnumerableToEdmCollection(IEdmModel edmModel, Type clrType, Type elementClrType);

        /// <summary>
        /// Maps a given CLR object to its corresponding <see cref="IEdmType"/>.
        /// </summary>
        /// <param name="edmModel">The EDM model for which the conversion should be performed.</param>
        /// <param name="clrInstance">The CLR object for which the mapping should be performed.</param>
        /// <returns>The <see cref="IEdmType"/> that corresponds for the given CLR instance or <code>null</code> if default mapping should be performed.</returns>
        IEdmType MapClrInstanceToEdmType(IEdmModel edmModel, object clrInstance);

        /// <summary>
        /// Maps a given CLR object to its corresponding <see cref="IEdmTypeReference"/>.
        /// </summary>
        /// <param name="edmModel">The EDM model for which the conversion should be performed.</param>
        /// <param name="clrInstance">The CLR object for which the mapping should be performed.</param>
        /// <returns>The <see cref="IEdmTypeReference"/> that corresponds for the given CLR instance or <code>null</code> if default mapping should be performed.</returns>
        IEdmTypeReference MapClrInstanceToEdmTypeReference(IEdmModel edmModel, object clrInstance);
    }
}
