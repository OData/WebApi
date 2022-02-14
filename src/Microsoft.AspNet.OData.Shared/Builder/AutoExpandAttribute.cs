//-----------------------------------------------------------------------------
// <copyright file="AutoExpandAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="Attribute"/> that can be placed on a navigation property to specify it
    /// is auto expanded, or placed on a class to specify all navigation properties are auto expanded.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class AutoExpandAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets whether the automatic expand will be disabled if there is a $select specify by client.
        /// </summary>
        public bool DisableWhenSelectPresent { get; set; }
    }
}
