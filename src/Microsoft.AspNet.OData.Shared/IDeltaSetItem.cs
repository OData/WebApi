// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
        ODataIdContainer ODataIdContainer { get; set; }
    }
}
