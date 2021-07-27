// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Base Interface for PatchMethodHandler. 
    /// This is being implemented by PatchMethodHandler{TStructuralType} which has a method returning nested patchhandler.
    /// A generic empty interface is needed since the nestedpatch handler will be of different type.
    /// </summary>
    public interface IPatchMethodHandler
    {

    }
}
