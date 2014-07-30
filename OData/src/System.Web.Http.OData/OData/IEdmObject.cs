// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData
{
    /// <summary>
    /// Represents an instance of an <see cref="IEdmType"/>.
    /// </summary>
    public interface IEdmObject
    {
        /// <summary>
        /// Gets the <see cref="IEdmTypeReference"/> of this instance.
        /// </summary>
        /// <returns>The <see cref="IEdmTypeReference"/> of this instance.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "This should not be serialized. Having it as a method is more appropriate.")]
        IEdmTypeReference GetEdmType();
    }
}
