//-----------------------------------------------------------------------------
// <copyright file="DecimalPropertyConfiguration.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Reflection;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Used to configure a decimal property of an entity type or complex type.
    /// This configuration functionality is exposed by the model builder Fluent API, see <see cref="ODataModelBuilder"/>.
    /// </summary>
    public class DecimalPropertyConfiguration : PrecisionPropertyConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecimalPropertyConfiguration"/> class.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="declaringType">The declaring EDM type of the property.</param>
        public DecimalPropertyConfiguration(PropertyInfo property, StructuralTypeConfiguration declaringType)
            : base(property, declaringType)
        {
        }

        /// <summary>
        /// Gets or sets the maximum number of digits allowed to the right of the decimal point.
        /// </summary>
        public int? Scale { get; set; }
    }
}
