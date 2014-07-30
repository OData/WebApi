// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Marks properties that have <see cref="RequiredAttribute"/> as non-optional on their edm type.
    /// </summary>
    internal class RequiredAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public RequiredAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(RequiredAttribute), allowMultiple: false)
        {
        }

        /// <summary>
        /// Marks the property non-optional on the edm type.
        /// </summary>
        /// <param name="edmProperty">The edm property.</param>
        /// <param name="structuralTypeConfiguration">The edm type being configured.</param>
        /// <param name="attribute">The <see cref="Attribute"/> found.</param>
        public override void Apply(PropertyConfiguration edmProperty, StructuralTypeConfiguration structuralTypeConfiguration, Attribute attribute)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            if (!edmProperty.AddedExplicitly)
            {
                StructuralPropertyConfiguration structuralProperty = edmProperty as StructuralPropertyConfiguration;
                if (structuralProperty != null)
                {
                    structuralProperty.OptionalProperty = false;
            }

                NavigationPropertyConfiguration navigationProperty = edmProperty as NavigationPropertyConfiguration;
                if (navigationProperty != null && navigationProperty.Multiplicity != EdmMultiplicity.Many)
                {
                    navigationProperty.Required();
        }
    }
}
    }
}
