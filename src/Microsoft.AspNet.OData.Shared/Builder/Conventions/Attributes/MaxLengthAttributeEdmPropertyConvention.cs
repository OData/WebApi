// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Web.Http;

namespace System.Web.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Configures string or binary properties that have the <see cref="MaxLengthAttribute"/>.
    /// </summary>
    internal class MaxLengthAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<StructuralPropertyConfiguration>
    {
        public MaxLengthAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(MaxLengthAttribute), allowMultiple: false)
        {
        }

        /// <summary>
        /// Configures string or binary propertie's maxLength.
        /// </summary>
        /// <param name="edmProperty">The key property.</param>
        /// <param name="structuralTypeConfiguration">The edm type being configured.</param>
        /// <param name="attribute">The <see cref="Attribute"/> found on the property.</param>
        /// <param name="model">The ODataConventionModelBuilder used to build the model.</param>
        public override void Apply(StructuralPropertyConfiguration edmProperty,
            StructuralTypeConfiguration structuralTypeConfiguration,
            Attribute attribute,
            ODataConventionModelBuilder model)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            MaxLengthAttribute maxLengthAttribute = attribute as MaxLengthAttribute;
            LengthPropertyConfiguration lengthProperty = edmProperty as LengthPropertyConfiguration;
            if (lengthProperty != null && maxLengthAttribute != null)
            {
                lengthProperty.MaxLength = maxLengthAttribute.Length;
            }
        }
    }
}
