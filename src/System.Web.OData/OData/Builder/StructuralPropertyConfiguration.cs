// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.OData.Formatter;

namespace System.Web.OData.Builder
{
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
