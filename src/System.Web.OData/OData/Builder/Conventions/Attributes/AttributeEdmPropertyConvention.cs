// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Base class for all attribute based <see cref="IEdmPropertyConvention"/>'s.
    /// </summary>
    /// <typeparam name="TPropertyConfiguration">The type of the property this configuration applies to.</typeparam>
    internal abstract class AttributeEdmPropertyConvention<TPropertyConfiguration> : AttributeConvention, IEdmPropertyConvention<TPropertyConfiguration>
        where TPropertyConfiguration : PropertyConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeEdmPropertyConvention{TPropertyConfiguration}"/> class.
        /// </summary>
        /// <param name="attributeFilter">A function to test whether this convention applies to an attribute or not.</param>
        /// <param name="allowMultiple"><see langword="true"/> if the convention allows multiple attributes; otherwise, <see langword="false"/>.</param>
        protected AttributeEdmPropertyConvention(Func<Attribute, bool> attributeFilter, bool allowMultiple)
            : base(attributeFilter, allowMultiple)
        {
        }

        /// <summary>
        /// Applies the convention.
        /// </summary>
        /// <param name="edmProperty">The property being configured.</param>
        /// <param name="structuralTypeConfiguration">The type being configured.</param>
        public void Apply(PropertyConfiguration edmProperty, StructuralTypeConfiguration structuralTypeConfiguration)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            if (structuralTypeConfiguration == null)
            {
                throw Error.ArgumentNull("structuralTypeConfiguration");
            }

            TPropertyConfiguration property = edmProperty as TPropertyConfiguration;
            if (property != null)
            {
                Apply(property, structuralTypeConfiguration);
            }
        }

        /// <summary>
        /// Applies the convention.
        /// </summary>
        /// <param name="edmProperty">The property being configured.</param>
        /// <param name="structuralTypeConfiguration">The type being configured.</param>
        public void Apply(TPropertyConfiguration edmProperty, StructuralTypeConfiguration structuralTypeConfiguration)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            if (structuralTypeConfiguration == null)
            {
                throw Error.ArgumentNull("structuralTypeConfiguration");
            }

            foreach (Attribute attribute in GetAttributes(edmProperty.PropertyInfo))
            {
                Apply(edmProperty, structuralTypeConfiguration, attribute);
            }
        }

        /// <summary>
        /// Applies the convention.
        /// </summary>
        /// <param name="edmProperty">The property being configured.</param>
        /// <param name="structuralTypeConfiguration">The type being configured.</param>
        /// <param name="attribute">The attribute to be used during configuration.</param>
        public abstract void Apply(TPropertyConfiguration edmProperty, StructuralTypeConfiguration structuralTypeConfiguration, Attribute attribute);
    }
}
