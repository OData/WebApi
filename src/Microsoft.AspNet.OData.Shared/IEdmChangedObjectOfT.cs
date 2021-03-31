// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Base interface to be implemented by any Delta object required to be part of the DeltaFeed Payload.
    /// </summary>
    /// <typeparam name="TStructuralType">Generic Type for changed object</typeparam>
    public interface IEdmChangedObject<TStructuralType> : IEdmChangedObject
    {        

    }
}