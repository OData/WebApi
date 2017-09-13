// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a download activation type
    /// </summary>
    public enum DownloadActivationType
    {
        /// <summary>
        /// When order is paid
        /// </summary>
        WhenOrderIsPaid = 1,
        /// <summary>
        /// Manually
        /// </summary>
        Manually = 10,
    }
}
