// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Sealed class to hold ODataID in parsed format, it will be used by POCO objects as well as Delta{TStructuralType}
    /// </summary>
    public sealed class ODataIdContainer
    {
        /// <summary>
        /// The Navigation path corresponding to the ODataId
        /// </summary>
        public NavigationPath ODataIdNavigationPath { set; get; }
    }
}
