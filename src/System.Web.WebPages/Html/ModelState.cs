// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.WebPages.Html
{
    public class ModelState
    {
        private List<string> _errors = new List<string>();

        public IList<string> Errors
        {
            get { return _errors; }
        }

        public object Value { get; set; }
    }
}
