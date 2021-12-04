//-----------------------------------------------------------------------------
// <copyright file="IDeltaSetItem.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Builder;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Basic Interface for representing a delta item like delta, deletedentity etc
    /// </summary>
    public interface IDeltaSetItem
    {
        /// <summary>
        /// Entry or Deleted Entry for Delta Set Item
        /// </summary>
        EdmDeltaEntityKind DeltaKind { get; }

        /// <summary>
        /// Annotation container to hold Transient Instance Annotations
        /// </summary>
        IODataInstanceAnnotationContainer TransientInstanceAnnotationContainer { get; set; }

        /// <summary>
        /// Container to hold ODataId
        /// </summary>
        IODataIdContainer ODataIdContainer { get; set; }
    }
}
