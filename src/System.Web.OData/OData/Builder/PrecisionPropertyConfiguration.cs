// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Used to configure a  datetime-with-offset, decimal, duration, or time-of-day property precision of an entity type or complex type.
    /// This configuration functionality is exposed by the model builder Fluent API, see <see cref="ODataModelBuilder"/>.
    /// </summary>
    public class PrecisionPropertyConfiguration : PrimitivePropertyConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrecisionPropertyConfiguration"/> class.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="declaringType">The declaring EDM type of the property.</param>
        public PrecisionPropertyConfiguration(PropertyInfo property, StructuralTypeConfiguration declaringType)
            : base(property, declaringType)
        {
        }

        /// <summary>
        /// Get or set the maximum number of digits allowed in the property’s value for decimal property.
        /// Get or set the number of decimal places allowed in the seconds portion of the property’s value for temporal property.
        /// </summary>
        public int? Precision { get; set; }
    }
}