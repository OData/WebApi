// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Web.Http.Data
{
    /// <summary>
    /// Attribute applied to a <see cref="DataController"/> method to indicate that it is an update method.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property,
        AllowMultiple = false, Inherited = true)]
    public sealed class UpdateAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether the method is a custom update operation.
        /// </summary>
        public bool UsingCustomMethod { get; set; }
    }
}
