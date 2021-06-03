// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Base interface to represented a typed deleted entity object
    /// </summary>
    public interface IDeltaDeletedEntityObject
    {
        /// <summary>
        /// The id of the deleted entity (same as the odata.id returned or computed when calling GET on resource), which may be absolute or relative.
        /// </summary>
        Uri Id { get; set; }

        /// <summary>
        /// Optional. Either deleted, if the entity was deleted (destroyed), or changed if the entity was removed from membership in the result (i.e., due to a data change).
        /// </summary>
        DeltaDeletedEntryReason? Reason { get; set; }

        /// <summary>
        /// The navigation source of the deleted entity. If null, then the deleted entity is from the current feed.
        /// </summary>
        IEdmNavigationSource NavigationSource { get; set; }
        
    }
}
