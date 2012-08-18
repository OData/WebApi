// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Base class for all <see cref="IEdmTypeConvention"/>'s based on a attribute on the type.
    /// </summary>
    /// <typeparam name="TEdmTypeConfiguration">The kind of Edm type that this convention must be applied to.</typeparam>
    /// <typeparam name="TAttribute">The type of the <see cref="Attribute"/> to look for.</typeparam>
    public abstract class AttributeEdmTypeConvention<TEdmTypeConfiguration, TAttribute> : AttributeEdmTypeConvention<TEdmTypeConfiguration>
        where TAttribute : Attribute
        where TEdmTypeConfiguration : class, IStructuralTypeConfiguration
    {
        protected AttributeEdmTypeConvention(bool allowMultiple)
            : base((attribute) => typeof(TAttribute).IsAssignableFrom(attribute.GetType()), allowMultiple)
        {
        }

        /// <summary>
        /// Applies the convention.
        /// </summary>
        /// <param name="edmTypeConfiguration">The edm type to apply the convention to.</param>
        /// <param name="model">The model that this edm type belongs to.</param>
        /// <param name="attribute">The attribute found on this edm type.</param>
        public override void Apply(TEdmTypeConfiguration edmTypeConfiguration, ODataModelBuilder model, Attribute attribute)
        {
            if (attribute == null)
            {
                throw Error.ArgumentNull("attribute");
            }

            TAttribute typedttribute = (TAttribute)attribute;
            Apply(edmTypeConfiguration, model, typedttribute);
        }

        /// <summary>
        /// Applies the convention.
        /// </summary>
        /// <param name="edmTypeConfiguration">The edm type to apply the convention to.</param>
        /// <param name="model">The model that this edm type belongs to.</param>
        /// <param name="attribute">The attribute found on this edm type.</param>
        public abstract void Apply(TEdmTypeConfiguration edmTypeConfiguration, ODataModelBuilder model, TAttribute attribute);
    }
}
