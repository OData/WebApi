//-----------------------------------------------------------------------------
// <copyright file="IEdmEnumObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an instance of an enum value.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Justification = "Marker interface acceptable here for derivation")]
    public interface IEdmEnumObject : IEdmObject
    {
    }
}
