// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Query
{
    [Flags]
    public enum AllowedQueryOptions
    {
        None = 0x0,
        Filter = 0x1,
        Top = 0x2,
        Skip = 0x4,
        OrderBy = 0x8,
        All = Filter | Top | Skip | OrderBy
    }
}
