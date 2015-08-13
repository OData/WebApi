// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="Attribute"/> that can be placed on a method to specify
    /// that the method represents a OData Action. It's used in DefaultODataModelProvider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ODataActionAttribute : Attribute
    {
    }
}
