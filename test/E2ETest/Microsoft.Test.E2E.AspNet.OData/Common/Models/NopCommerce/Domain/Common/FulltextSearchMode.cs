//-----------------------------------------------------------------------------
// <copyright file="FulltextSearchMode.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Nop.Core.Domain.Common
{
    /// <summary>
    /// Represents a Full-Text search mode 
    /// </summary>
    public enum FulltextSearchMode
    {
        /// <summary>
        /// Exact match (using CONTAINS with prefix_term)
        /// </summary>
        ExactMatch = 0,
        /// <summary>
        /// Using CONTAINS and OR with prefix_term
        /// </summary>
        Or = 5,
        /// <summary>
        /// Using CONTAINS and AND with prefix_term
        /// </summary>
        And = 10
    }
}
