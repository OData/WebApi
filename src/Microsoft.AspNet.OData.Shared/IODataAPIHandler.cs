//-----------------------------------------------------------------------------
// <copyright file="IODataAPIHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Base Interface for ODataAPIHandler. 
    /// This is being implemented by ODataAPIHandler{TStructuralType} which has a method returning nested ODataApiHandler.
    /// A generic empty interface is needed since the nestedpatch handler will be of different type.
    /// </summary>
    public interface IODataAPIHandler
    {

    }
}
