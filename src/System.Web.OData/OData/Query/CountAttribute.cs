// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace System.Web.OData.Query
{
    /// <summary>
    /// Represents an <see cref="Attribute"/> that can be placed on a class or property
    /// correlate to OData's $count query option settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class CountAttribute : Attribute
    {
        /// <summary>
        /// Represents whether the $count can be applied on the property or the entityset.
        /// </summary>
        public bool Disabled { get; set; }
    }
}
