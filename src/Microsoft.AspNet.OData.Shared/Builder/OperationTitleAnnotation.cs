//-----------------------------------------------------------------------------
// <copyright file="OperationTitleAnnotation.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData.Builder
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
