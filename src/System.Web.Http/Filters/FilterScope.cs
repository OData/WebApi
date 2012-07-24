// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.Filters
{
    public enum FilterScope
    {
        Global = 0,
        Controller = 10,
        Action = 20,
    }
}
