//-----------------------------------------------------------------------------
// <copyright file="ClrTypeAnnotation.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents a mapping from an <see cref="IEdmType"/> to a CLR type.
    /// </summary>
    public class ClrTypeAnnotation
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ClrTypeAnnotation"/> class.
        /// </summary>
        /// <param name="clrType">The backing CLR type for the EDM type.</param>
        public ClrTypeAnnotation(Type clrType)
        {
            ClrType = clrType;
        }

        /// <summary>
        /// Gets the backing CLR type for the EDM type.
        /// </summary>
        public Type ClrType { get; private set; }
    }
}
