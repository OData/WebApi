// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Builder
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
