// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Marks properties that have <see cref="DatabaseGeneratedAttribute"/> as non-optional on their EDM type.
    /// </summary>
    internal class DatabaseGeneratedAttributeEdmPropertyConvention :
        AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public DatabaseGeneratedAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(DatabaseGeneratedAttribute), allowMultiple: false)
        {
        }

        /// <summary>
        /// Marks the property with StoreGeneratedPattern on the EDM type.
        /// </summary>
        /// <param name="edmProperty">The EDM property.</param>
        /// <param name="structuralTypeConfiguration">The EDM type being configured.</param>
        /// <param name="attribute">The <see cref="Attribute"/> found.</param>
        public override void Apply(PropertyConfiguration edmProperty,
            StructuralTypeConfiguration structuralTypeConfiguration, Attribute attribute)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            EntityTypeConfiguration entityType = structuralTypeConfiguration as EntityTypeConfiguration;
            PrimitivePropertyConfiguration primitiveProperty = edmProperty as PrimitivePropertyConfiguration;
            DatabaseGeneratedAttribute databaseGeneratedAttribute = attribute as DatabaseGeneratedAttribute;
            if (entityType != null && primitiveProperty != null && databaseGeneratedAttribute != null)
            {
                if (!edmProperty.AddedExplicitly)
                {
                    primitiveProperty.StoreGeneratedPattern = databaseGeneratedAttribute.DatabaseGeneratedOption;
                }
            }
        }
    }
}