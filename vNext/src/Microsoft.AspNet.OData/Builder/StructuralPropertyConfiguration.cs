// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Builder
{
    using System.Reflection;

    using Microsoft.AspNetCore.OData.Formatter;

    /// <summary>
    /// Base class for all structural property configurations.
    /// </summary>
    public abstract class StructuralPropertyConfiguration : PropertyConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StructuralPropertyConfiguration"/> class.
        /// </summary>
        /// <param name="property">The property of the configuration.</param>
        /// <param name="declaringType">The declaring type of the property.</param>
        protected StructuralPropertyConfiguration(PropertyInfo property, StructuralTypeConfiguration declaringType)
            : base(property, declaringType)
        {
            OptionalProperty = EdmLibHelpers.IsNullable(property.PropertyType);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this property is optional or not.
        /// </summary>
        public bool OptionalProperty { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this property is a concurrency token or not.
        /// </summary>
        public bool ConcurrencyToken { get; set; }
    }
}
