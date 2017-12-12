// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// An interface that defines a property to access a collection of <see cref="MediaTypeMapping"/> objects.
    /// </summary>
    /// <remarks>
    /// MediaTypeMapping is part of the platform in AspNet but defined here for AspNetCore to allow for reusing
    /// the classes derive form it for managing media type mapping.
    /// </remarks>
    interface IMediaTypeMappingCollection
    {
        /// <summary>
        /// Gets a collection of <see cref="MediaTypeMapping"/> objects.
        /// </summary>
        ICollection<MediaTypeMapping> MediaTypeMappings { get; }
    }
}
