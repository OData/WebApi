﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an instance of an <see cref="IEdmChangedObject"/>.
    /// Holds the properties necessary to create the ODataDeltaDeletedLink.
    /// </summary>
    public interface IEdmDeltaDeletedLink : IEdmDeltaLinkBase, IEdmChangedObject
    {
    }
}
