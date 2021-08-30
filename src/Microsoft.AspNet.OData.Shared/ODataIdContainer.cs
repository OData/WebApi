// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Interface to hold ODataID in parsed format, it will be used by POCO objects as well as Delta{TStructuralType}
    /// </summary>
    public interface IODataIdContainer
    {
        /// <summary>
        /// The Navigation path corresponding to the ODataId
        /// </summary>
        NavigationPath ODataIdNavigationPath { set; get; }
    }

    /// <summary>
    /// Default implementation of IOdataIdContainer
    /// </summary>
    public class ODataIdContainer : IODataIdContainer
    {
        ///<inheritdoc/>
        public NavigationPath ODataIdNavigationPath { get; set; }
    }
}
