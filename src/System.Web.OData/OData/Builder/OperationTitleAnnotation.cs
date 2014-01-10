// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
