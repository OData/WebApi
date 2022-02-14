//-----------------------------------------------------------------------------
// <copyright file="AllowedArithmeticOperators.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Arithmetic operators to allow for querying using $filter.
    /// </summary>
    [Flags]
    public enum AllowedArithmeticOperators
    {
        /// <summary>
        /// A value that corresponds to allowing no arithmetic operators in $filter.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// A value that corresponds to allowing 'Add' arithmetic operator in $filter.
        /// </summary>
        Add = 0x1,

        /// <summary>
        /// A value that corresponds to allowing 'Subtract' arithmetic operator in $filter.
        /// </summary>
        Subtract = 0x2,

        /// <summary>
        /// A value that corresponds to allowing 'Multiply' arithmetic operator in $filter.
        /// </summary>
        Multiply = 0x4,

        /// <summary>
        /// A value that corresponds to allowing 'Divide' arithmetic operator in $filter.
        /// </summary>
        Divide = 0x8,

        /// <summary>
        /// A value that corresponds to allowing 'Modulo' arithmetic operator in $filter.
        /// </summary>
        Modulo = 0x10,

        /// <summary>
        /// A value that corresponds to allowing all arithmetic operators in $filter.
        /// </summary>
        All = Add | Subtract | Multiply | Divide | Modulo
    }
}
