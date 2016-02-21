// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Represents an instance of an <see cref="IEdmChangedObject"/>.
    /// Holds the properties necessary to create the ODataDeltaLink.
    /// </summary>
    public interface IEdmDeltaLink : IEdmDeltaLinkBase, IEdmChangedObject
    {
    }
}
