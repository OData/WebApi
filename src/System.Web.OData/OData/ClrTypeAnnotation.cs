// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Edm;

namespace System.Web.OData
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
        /// The backing CLR type for the EDM type.
        /// </summary>
        public Type ClrType { get; set; }
    }
}
