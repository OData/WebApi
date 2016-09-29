// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Represents an instance of an <see cref="IEdmChangedObject"/>.
    /// Holds the properties necessary to create the ODataDeltaDeletedEntry.
    /// </summary>
    public interface IEdmDeltaDeletedEntityObject : IEdmChangedObject
    {
        /// <summary>
        /// The id of the deleted entity (same as the odata.id returned or computed when calling GET on resource), which may be absolute or relative.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Optional. Either deleted, if the entity was deleted (destroyed), or changed if the entity was removed from membership in the result (i.e., due to a data change).
        /// </summary>
        DeltaDeletedEntryReason Reason { get; set; }
    }
}
