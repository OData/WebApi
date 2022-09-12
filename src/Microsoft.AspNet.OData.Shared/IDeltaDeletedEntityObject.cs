//-----------------------------------------------------------------------------
// <copyright file="IDeltaDeletedEntityObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.OData;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Base interface to represent a typed deleted entity object
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
        string NavigationSource { get; set; }
    }
}
