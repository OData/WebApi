// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Nop.Core.Domain.Tax
{
    /// <summary>
    /// Represents the VAT number status enumeration
    /// </summary>
    public enum VatNumberStatus
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Empty
        /// </summary>
        Empty = 10,
        /// <summary>
        /// Valid
        /// </summary>
        Valid = 20,
        /// <summary>
        /// Invalid
        /// </summary>
        Invalid = 30
    }
}
