// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Represents an instance of an enum value.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Justification = "Marker interface acceptable here for derivation")]
    public interface IEdmEnumObject : IEdmObject
    {
    }
}
