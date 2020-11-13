// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an instance of an <see cref="IEdmDeltaDeletedEntityObject"/>.
    /// Holds the properties necessary to create the ODataDeltaDeletedEntry.
    /// </summary>
    public interface IEdmDeltaDeletedEntityObject<TStructuralType> : IEdmChangedObject<TStructuralType>, IEdmDeltaDeletedEntityObject
    {

    }
}
