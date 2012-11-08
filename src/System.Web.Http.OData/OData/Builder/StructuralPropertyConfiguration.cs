// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.Http.OData.Formatter;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// Base class for all structural property configurations.
    /// </summary>
    public abstract class StructuralPropertyConfiguration : PropertyConfiguration
    {
        private bool _optionalProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuralPropertyConfiguration"/> class.
        /// </summary>
        /// <param name="property">The property of the configuration.</param>
        /// <param name="declaringType">The declaring type of the property.</param>
        protected StructuralPropertyConfiguration(PropertyInfo property, StructuralTypeConfiguration declaringType)
            : base(property, declaringType)
        {
            _optionalProperty = EdmLibHelpers.IsNullable(property.PropertyType);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this property is optional or not.
        /// </summary>
        public bool OptionalProperty
        {
            get
            {
                return _optionalProperty;
            }

            set
            {
                _optionalProperty = value;
            }
        }
    }
}
