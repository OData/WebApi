// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Helpers
{
    internal interface IWebGridDataSource
    {
        int TotalRowCount { get; }

        IList<WebGridRow> GetRows(SortInfo sortInfo, int pageIndex);
    }
}
