﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// The kind of the EDM property.
    /// </summary>
    public enum PropertyKind
    {
        /// <summary>
        /// Represents an EDM primitive property.
        /// </summary>
        Primitive = 0,

        /// <summary>
        /// Represents an EDM complex property.
        /// </summary>
        Complex = 1,

        /// <summary>
        /// Represents an EDM collection property.
        /// </summary>
        Collection = 2,

        /// <summary>
        /// Represents an EDM navigation property.
        /// </summary>
        Navigation = 3,

        /// <summary>
        /// Represents an EDM enum property.
        /// </summary>
        Enum = 4,

        /// <summary>
        /// Represents a dynamic property dictionary for an open type.
        /// </summary>
        Dynamic = 5,

        /// <summary>
        /// Represents a instance annotation for a CLR type.
        /// </summary>
        InstanceAnnotations = 6
    }
}

