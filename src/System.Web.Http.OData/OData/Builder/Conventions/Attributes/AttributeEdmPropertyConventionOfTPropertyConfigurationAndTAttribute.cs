// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Base class for all attribute based <see cref="IEdmPropertyConvention"/>'s.
    /// </summary>
    /// <typeparam name="TPropertyConfiguration">The type of the property this configuration applies to.</typeparam>
    /// <typeparam name="TAttribute">The type of the attribute this convention looks for.</typeparam>
    public abstract class AttributeEdmPropertyConvention<TPropertyConfiguration, TAttribute> : AttributeEdmPropertyConvention<TPropertyConfiguration>
        where TPropertyConfiguration : PropertyConfiguration
        where TAttribute : Attribute
    {
        protected AttributeEdmPropertyConvention(bool allowMultiple)
            : base((attribute) => typeof(TAttribute) == attribute.GetType(), allowMultiple)
        {
        }

        /// <summary>
        /// Applies the convention.
        /// </summary>
        /// <param name="edmProperty">The property being configured.</param>
        /// <param name="structuralTypeConfiguration">The type being configured.</param>
        /// <param name="attribute">The attribute to be used during configuration.</param>
        public override void Apply(TPropertyConfiguration edmProperty, IStructuralTypeConfiguration structuralTypeConfiguration, Attribute attribute)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            if (structuralTypeConfiguration == null)
            {
                throw Error.ArgumentNull("structuralTypeConfiguration");
            }

            if (attribute == null)
            {
                throw Error.ArgumentNull("attribute");
            }

            Apply(edmProperty, structuralTypeConfiguration, attribute as TAttribute);
        }

        /// <summary>
        /// Applies the convention.
        /// </summary>
        /// <param name="edmProperty">The property being configured.</param>
        /// <param name="structuralTypeConfiguration">The type being configured.</param>
        /// <param name="attribute">The attribute to be used during configuration.</param>
        public abstract void Apply(TPropertyConfiguration edmProperty, IStructuralTypeConfiguration structuralTypeConfiguration, TAttribute attribute);
    }
}