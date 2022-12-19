//-----------------------------------------------------------------------------
// <copyright file="IDeltaSet.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Basic interface to represent a deltaset which is a collection of deltas.
    /// This is being implemented by DeltaSet<TStructuralType/>. Since it's being implemented by a generic type and
    /// since we need to check in a few places (like deserializer) whether the object is a deltaset and the {TStructuralType} is not available,
    /// we need a marker interface which can be used in these checks.
    /// </summary>
    public interface IDeltaSet
    {
    }
}
