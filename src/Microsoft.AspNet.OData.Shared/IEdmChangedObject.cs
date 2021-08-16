//-----------------------------------------------------------------------------
// <copyright file="IEdmChangedObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an instance of an <see cref="IEdmChangedObject"/>.
    /// Base interface to be implemented by any Delta object required to be part of the DeltaFeed Payload.
    /// </summary>
    public interface IEdmChangedObject : IEdmStructuredObject
    {
        /// <summary>
        /// DeltaKind for the objects part of the DeltaFeed Payload.
        /// Used to determine which Delta object to create during serialization.
        /// </summary>
        EdmDeltaEntityKind DeltaKind { get; }
    }
}
