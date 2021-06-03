// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// Enum to determine the type of Resource Set
    /// </summary>
    internal enum ResourceSetType
    {
        /// <summary>
        /// A normal ResourceSet
        /// </summary>
        ResourceSet,
		
        /// <summary>
        /// A Delta Resource Set
        /// </summary>
        DeltaResourceSet
    }
}
