// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Query
{
    /// <summary>
    /// Logical operators to allow for querying using $filter.
    /// </summary>
    [Flags]
    public enum AllowedLogicalOperators
    {
        /// <summary>
        /// A value that corresponds to allowing no logical operators in $filter.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// A value that corresponds to allowing 'Or' logical operator in $filter.
        /// </summary>
        Or = 0x1,

        /// <summary>
        /// A value that corresponds to allowing 'And' logical operator in $filter.
        /// </summary>
        And = 0x2,

        /// <summary>
        /// A value that corresponds to allowing 'Equal' logical operator in $filter.
        /// </summary>
        Equal = 0x4,

        /// <summary>
        /// A value that corresponds to allowing 'NotEqual' logical operator in $filter.
        /// </summary>
        NotEqual = 0x8,

        /// <summary>
        /// A value that corresponds to allowing 'GreaterThan' logical operator in $filter.
        /// </summary>   
        GreaterThan = 0x10,

        /// <summary>
        /// A value that corresponds to allowing 'GreaterThanOrEqual' logical operator in $filter.
        /// </summary>
        GreaterThanOrEqual = 0x20,

        /// <summary>
        /// A value that corresponds to allowing 'LessThan' logical operator in $filter.
        /// </summary>  
        LessThan = 0x40,

        /// <summary>
        /// A value that corresponds to allowing 'LessThanOrEqual' logical operator in $filter.
        /// </summary>
        LessThanOrEqual = 0x80,

        /// <summary>
        /// A value that corresponds to allowing 'Not' logical operator in $filter.
        /// </summary>  
        Not = 0x100,

        /// <summary>
        /// A value that corresponds to allowing all logical operators in $filter.
        /// </summary>
        All = Or | And | Equal | NotEqual | GreaterThan | GreaterThanOrEqual | LessThan | LessThanOrEqual | Not
    }
}
