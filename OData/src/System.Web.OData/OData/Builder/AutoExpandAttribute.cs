// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="Attribute"/> that can be placed on a navigation property to specify it
    /// is auto expanded, or placed on a class to specify all navigation properties are auto expanded.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class AutoExpandAttribute : Attribute
    {
    }
}
