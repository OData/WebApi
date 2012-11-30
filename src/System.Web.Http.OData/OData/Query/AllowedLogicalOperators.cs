// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Query
{
    [Flags]
    public enum AllowedLogicalOperators
    {
        None = 0x0,
        Or = 0x1,
        And = 0x2,
        Equal = 0x4,
        NotEqual = 0x8,
        GreaterThan = 0x10,
        GreaterThanOrEqual = 0x20,
        LessThan = 0x40,
        LessThanOrEqual = 0x80,
        Not = 0x100,
        All = 0x1FF
    }
}
