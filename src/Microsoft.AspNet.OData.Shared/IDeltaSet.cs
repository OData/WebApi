// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Basic interface to reperesent a deltaset which is a collection of Deltas.
    /// This is being implemented by Deltaset{TStructuralType}. Since its being implementd by a gemeric type and
    /// since we need to check in a few places(like deserializer) where the object is a DeltaSet and the {TStructuralType} is not available,
    /// we need a marker interface which can be used in these checks.
    /// </summary>
    public interface IDeltaSet
    {
        
    }
}
