// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Query
{
    [Flags]
    public enum AllowedArithmeticOperators
    {
        None = 0x0,
        Add = 0x1,
        Subtract = 0x2,
        Multiply = 0x4,
        Divide = 0x8,
        Modulo = 0x10,
        All = Add | Subtract | Multiply | Divide | Modulo
    }
}
