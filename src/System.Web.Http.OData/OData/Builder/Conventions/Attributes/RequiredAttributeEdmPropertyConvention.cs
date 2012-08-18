// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Marks properties that have <see cref="RequiredAttribute"/> as non-optional on their edm type.
    /// </summary>
    public class RequiredAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<StructuralPropertyConfiguration, RequiredAttribute>
    {
        public RequiredAttributeEdmPropertyConvention()
            : base(allowMultiple: false)
        {
        }

        /// <summary>
        /// Marks the property non-optional on the edm type.
        /// </summary>
        /// <param name="edmProperty">The edm property.</param>
        /// <param name="structuralTypeConfiguration">The edm type being configured.</param>
        /// <param name="attribute">The <see cref="RequiredAttribute"/> found.</param>
        public override void Apply(StructuralPropertyConfiguration edmProperty, IStructuralTypeConfiguration structuralTypeConfiguration, RequiredAttribute attribute)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            edmProperty.OptionalProperty = false;
        }
    }
}
