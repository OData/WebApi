﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// Encapsulates an <see cref="ODataDeltaResourceSet"/> and the <see cref="ODataResourceBase"/>'s that are part of it.
    /// </summary>
    public sealed class ODataDeltaResourceSetWrapper : ODataResourceSetWrapperBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataDeltaResourceSetWrapper"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        public ODataDeltaResourceSetWrapper(ODataDeltaResourceSet item)
            : base(item)
        {
            
        }

        internal override ResourceSetType ResourceSetType => ResourceSetType.DeltaResourceSet;
    }
}
