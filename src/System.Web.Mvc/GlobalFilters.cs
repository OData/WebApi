// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public static class GlobalFilters
    {
        static GlobalFilters()
        {
            Filters = new GlobalFilterCollection();
        }

        public static GlobalFilterCollection Filters { get; private set; }
    }
}
