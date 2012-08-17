// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.Http.OData.Builder.Conventions
{
    /// <summary>
    /// <see cref="IEdmPropertyConvention{TPropertyConfiguration}"/> to process attributes of type TAttribute found on properties of <see cref="IStructuralTypeConfiguration"/>.
    /// </summary>
    /// <typeparam name="TPropertyConfiguration">The property type.</typeparam>
    /// <typeparam name="TAttribute">The attribute type.</typeparam>
    public abstract class AttributeEdmPropertyConvention<TPropertyConfiguration, TAttribute> : IEdmPropertyConvention<TPropertyConfiguration>
        where TPropertyConfiguration : PropertyConfiguration
        where TAttribute : Attribute
    {
        protected AttributeEdmPropertyConvention()
        {
        }

        public void Apply(TPropertyConfiguration edmProperty, IStructuralTypeConfiguration structuralTypeConfiguration)
        {
            PropertyInfo clrProperty = edmProperty.PropertyInfo;
            foreach (TAttribute attribute in clrProperty.GetCustomAttributes(typeof(TAttribute), inherit: false))
            {
                Apply(edmProperty, structuralTypeConfiguration, attribute);
            }
        }

        /// <summary>
        /// Applies the convention.
        /// </summary>
        /// <param name="edmProperty">The property the convention is applied on.</param>
        /// <param name="structuralTypeConfiguration">The <see cref="IStructuralTypeConfiguration"/> the edmProperty belongs to.</param>
        /// <param name="attribute">The attribute instance.</param>
        public abstract void Apply(TPropertyConfiguration edmProperty, IStructuralTypeConfiguration structuralTypeConfiguration, TAttribute attribute);
    }
}
