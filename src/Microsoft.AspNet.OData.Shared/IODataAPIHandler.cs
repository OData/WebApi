// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Base Interface for ODataAPIHandler. 
    /// This is being implemented by ODataAPIHandler{TStructuralType} which has a method returning nested ODataApiHandler.
    /// A generic empty interface is needed since the nestedpatch handler will be of different type.
    /// </summary>
    internal interface IODataAPIHandler
    {

    }
}
