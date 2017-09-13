// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Represents the customer registration type fortatting enumeration
    /// </summary>
    public enum UserRegistrationType : int
    {
        /// <summary>
        /// Standard account creation
        /// </summary>
        Standard = 1,
        /// <summary>
        /// Email validation is required after registration
        /// </summary>
        EmailValidation = 2,
        /// <summary>
        /// A customer should be approved by administrator
        /// </summary>
        AdminApproval = 3,
        /// <summary>
        /// Registration is disabled
        /// </summary>
        Disabled = 4,
    }
}
