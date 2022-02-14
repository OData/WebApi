//-----------------------------------------------------------------------------
// <copyright file="TaxDisplayType.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Nop.Core.Domain.Tax
{
    /// <summary>
    /// Represents the tax display type enumeration
    /// </summary>
    public enum TaxDisplayType
    {
        /// <summary>
        /// Including tax
        /// </summary>
        IncludingTax = 0,
        /// <summary>
        /// Excluding tax
        /// </summary>
        ExcludingTax = 10,
    }
}
