// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Helpers
{
    public class WebGridColumn
    {
        public bool CanSort { get; set; }

        public string ColumnName { get; set; }

        public Func<dynamic, object> Format { get; set; }

        public string Header { get; set; }

        public string Style { get; set; }
    }
}
