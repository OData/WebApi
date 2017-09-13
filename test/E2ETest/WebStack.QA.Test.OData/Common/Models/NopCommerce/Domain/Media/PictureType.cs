// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Nop.Core.Domain.Media
{
    /// <summary>
    /// Represents a picture item type
    /// </summary>
    public enum PictureType : int
    {
        /// <summary>
        /// Entities (products, categories, manufacturers)
        /// </summary>
        Entity = 1,
        /// <summary>
        /// Avatar
        /// </summary>
        Avatar = 10,
    }
}
