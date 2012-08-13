// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.Http.OData.Builder.Conventions
{
    public abstract class AttributeEdmPropertyConvention<TPropertyConfiguration, TAttribute> : IEdmPropertyConvention<TPropertyConfiguration>
        where TPropertyConfiguration : PropertyConfiguration
        where TAttribute : Attribute
    {
        public void Apply(TPropertyConfiguration edmProperty, IStructuralTypeConfiguration structuralTypeConfiguration)
        {
            PropertyInfo clrProperty = edmProperty.PropertyInfo;
            foreach (TAttribute attribute in clrProperty.GetCustomAttributes(typeof(TAttribute), inherit: false))
            {
                Apply(edmProperty, structuralTypeConfiguration, attribute);
            }
        }

        public abstract void Apply(TPropertyConfiguration edmProperty, IStructuralTypeConfiguration structuralTypeConfiguration, TAttribute attribute);
    }
}
