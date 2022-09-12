//-----------------------------------------------------------------------------
// <copyright file="IDeltaSetItem.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Basic interface for representing a delta item like delta, deleted entity, etc.
    /// </summary>
    internal interface IDeltaSetItem
    {
        /// <summary>
        /// Gets the kind of object within the delta payload.
        /// </summary>
        EdmDeltaEntityKind DeltaKind { get; }

        /// <summary>
        /// Gets or sets the annotation container to hold transient instance annotations.
        /// </summary>
        IODataInstanceAnnotationContainer TransientInstanceAnnotationContainer { get; set; }

        /// <summary>
        /// Gets or sets the container to hold ODataId
        /// </summary>
        ODataIdContainer ODataIdContainer { get; set; }

        /// <summary>
        /// Gets or sets the OData path for the item.
        /// </summary>
        ODataPath ODataPath { get; set; }
    }
}
