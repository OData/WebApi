// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Used to configure a string or binary property length of an entity type or complex type.
    /// This configuration functionality is exposed by the model builder Fluent API, see <see cref="ODataModelBuilder"/>.
    /// </summary>
    public class LengthPropertyConfiguration : PrimitivePropertyConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LengthPropertyConfiguration"/> class.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="declaringType">The declaring EDM type of the property.</param>
        public LengthPropertyConfiguration(PropertyInfo property, StructuralTypeConfiguration declaringType)
            : base(property, declaringType)
        {
        }

        /// <summary>
        /// Gets or sets the maximum length of the value of the property on a type instance.
        /// </summary>
        public int? MaxLength { get; set; }
    }
}