// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Nop.Core.Domain.Seo
{
    /// <summary>
    /// Represents an editor type
    /// </summary>
    public enum PageTitleSeoAdjustment
    {
        /// <summary>
        /// Pagename comes after storename
        /// </summary>
        PagenameAfterStorename = 0,
        /// <summary>
        /// Storename comes after pagename
        /// </summary>
        StorenameAfterPagename = 10
    }
}
