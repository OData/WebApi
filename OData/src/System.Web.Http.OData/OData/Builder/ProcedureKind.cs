// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// The Kind of OData Procedure.
    /// One of Action, Function or ServiceOperation.
    /// </summary>
    public enum ProcedureKind
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
