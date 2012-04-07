// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public enum FilterScope
    {
        First = 0,
        Global = 10,
        Controller = 20,
        Action = 30,
        Last = 100,
    }
}
