// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace System.Web.OData.Builder
{
    internal class OperationTitleAnnotation
    {
        public OperationTitleAnnotation(string title)
        {
            Title = title;
        }

        public string Title { get; private set; }
    }
}
