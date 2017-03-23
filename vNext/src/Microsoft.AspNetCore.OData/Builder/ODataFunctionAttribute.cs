// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="Attribute"/> that can be placed on a method to specify
    /// that the method represents a OData Function. It's used in DefaultODataModelProvider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ODataFunctionAttribute : Attribute
    {
        public bool IsBound { get; set; }
        public string BindingName { get; set; }
    }
}
