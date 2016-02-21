// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData
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