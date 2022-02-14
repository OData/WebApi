//-----------------------------------------------------------------------------
// <copyright file="OperationKind.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// The Kind of OData Operation.
    /// One of Action, Function or ServiceOperation.
    /// </summary>
    public enum OperationKind
    {
        /// <summary>
        /// An action
        /// </summary>
        Action = 0,

        /// <summary>
        /// A function
        /// </summary>
        Function = 1,

        /// <summary>
        /// A service operation
        /// </summary>
        ServiceOperation = 2
    }
}
