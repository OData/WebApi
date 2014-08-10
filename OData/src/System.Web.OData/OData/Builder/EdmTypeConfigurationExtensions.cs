// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder
{
    internal static class EdmTypeConfigurationExtensions
    {
        // returns all the properties declared in the base types of this type.
        public static IEnumerable<PropertyConfiguration> DerivedProperties(
            this StructuralTypeConfiguration structuralType)
        {
            if (structuralType == null)
            {
                throw Error.ArgumentNull("structuralType");
            }

            if (structuralType.Kind == EdmTypeKind.Entity)
            {
                return DerivedProperties((EntityTypeConfiguration)structuralType);
            }

            if (structuralType.Kind == EdmTypeKind.Complex)
            {
                return DerivedProperties((ComplexTypeConfiguration)structuralType);
            }

            return Enumerable.Empty<PropertyConfiguration>();
        }

        public static IEnumerable<PropertyConfiguration> DerivedProperties(
            this EntityTypeConfiguration entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }

            EntityTypeConfiguration baseType = entity.BaseType;
            while (baseType != null)
            {
                foreach (PropertyConfiguration property in baseType.Properties)
                {
                    yield return property;
                }

                baseType = baseType.BaseType;
            }
        }

        public static IEnumerable<PropertyConfiguration> DerivedProperties(
            this ComplexTypeConfiguration complex)
        {
            if (complex == null)
            {
                throw Error.ArgumentNull("complex");
            }

            ComplexTypeConfiguration baseType = complex.BaseType;
            while (baseType != null)
            {
                foreach (PropertyConfiguration property in baseType.Properties)
                {
                    yield return property;
                }

                baseType = baseType.BaseType;
            }
        }

        // returns the keys declared or inherited for this entity type.
        public static IEnumerable<PropertyConfiguration> Keys(this EntityTypeConfiguration entity)
        {
            Contract.Assert(entity != null);
            return entity.BaseType == null ? entity.Keys : Keys(entity.BaseType);
        }

        // Returns the base types, this type.
        public static IEnumerable<StructuralTypeConfiguration> ThisAndBaseTypes(
            this StructuralTypeConfiguration structuralType)
        {
            Contract.Assert(structuralType != null);
            return structuralType.BaseTypes().Concat(new[] { structuralType });
        }

        // Returns the base types, this type and all the derived types of this type.
        public static IEnumerable<StructuralTypeConfiguration> ThisAndBaseAndDerivedTypes(
            this ODataModelBuilder modelBuilder, StructuralTypeConfiguration structuralType)
        {
            Contract.Assert(modelBuilder != null);
            Contract.Assert(structuralType != null);

            return structuralType.BaseTypes()
                    .Concat(new[] { structuralType })
                    .Concat(modelBuilder.DerivedTypes(structuralType));
        }

        // Returns the base types for this type.
        public static IEnumerable<StructuralTypeConfiguration> BaseTypes(
            this StructuralTypeConfiguration structuralType)
        {
            Contract.Assert(structuralType != null);

            if (structuralType.Kind == EdmTypeKind.Entity)
            {
                EntityTypeConfiguration entity = (EntityTypeConfiguration)structuralType;

                entity = entity.BaseType;
                while (entity != null)
                {
                    yield return entity;
                    entity = entity.BaseType;
                }
            }

            if (structuralType.Kind == EdmTypeKind.Complex)
            {
                ComplexTypeConfiguration complex = (ComplexTypeConfiguration)structuralType;

                complex = complex.BaseType;
                while (complex != null)
                {
                    yield return complex;
                    complex = complex.BaseType;
                }
            }
        }

        // Returns all the derived types of this type.
        public static IEnumerable<StructuralTypeConfiguration> DerivedTypes(this ODataModelBuilder modelBuilder,
            StructuralTypeConfiguration structuralType)
        {
            if (modelBuilder == null)
            {
                throw Error.ArgumentNull("modelBuilder");
            }

            if (structuralType == null)
            {
                throw Error.ArgumentNull("structuralType");
            }

            if (structuralType.Kind == EdmTypeKind.Entity)
            {
                return DerivedTypes(modelBuilder, (EntityTypeConfiguration)structuralType);
            }

            if (structuralType.Kind == EdmTypeKind.Complex)
            {
                return DerivedTypes(modelBuilder, (ComplexTypeConfiguration)structuralType);
            }

            return Enumerable.Empty<StructuralTypeConfiguration>();
        }

        public static IEnumerable<EntityTypeConfiguration> DerivedTypes(this ODataModelBuilder modelBuilder,
            EntityTypeConfiguration entity)
        {
            if (modelBuilder == null)
            {
                throw Error.ArgumentNull("modelBuilder");
            }

            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }

            IEnumerable<EntityTypeConfiguration> derivedEntities = modelBuilder.StructuralTypes
                .OfType<EntityTypeConfiguration>().Where(e => e.BaseType == entity);

            foreach (EntityTypeConfiguration derivedType in derivedEntities)
            {
                yield return derivedType;
                foreach (EntityTypeConfiguration derivedDerivedType in modelBuilder.DerivedTypes(derivedType))
                {
                    yield return derivedDerivedType;
                }
            }
        }

        public static IEnumerable<ComplexTypeConfiguration> DerivedTypes(this ODataModelBuilder modelBuilder,
               ComplexTypeConfiguration complex)
        {
            if (modelBuilder == null)
            {
                throw Error.ArgumentNull("modelBuilder");
            }

            if (complex == null)
            {
                throw Error.ArgumentNull("complex");
            }

            IEnumerable<ComplexTypeConfiguration> derivedComplexs =
                modelBuilder.StructuralTypes.OfType<ComplexTypeConfiguration>().Where(e => e.BaseType == complex);

            foreach (ComplexTypeConfiguration derivedType in derivedComplexs)
            {
                yield return derivedType;
                foreach (ComplexTypeConfiguration derivedDerivedType in modelBuilder.DerivedTypes(derivedType))
                {
                    yield return derivedDerivedType;
                }
            }
        }

        public static bool IsAssignableFrom(this StructuralTypeConfiguration baseStructuralType,
            StructuralTypeConfiguration structuralType)
        {
            if (structuralType.Kind == EdmTypeKind.Entity && baseStructuralType.Kind == EdmTypeKind.Entity)
            {
                EntityTypeConfiguration entity = (EntityTypeConfiguration)structuralType;
                while (entity != null)
                {
                    if (baseStructuralType == entity)
                    {
                        return true;
                    }

                    entity = entity.BaseType;
                }
            }
            else if (structuralType.Kind == EdmTypeKind.Complex && baseStructuralType.Kind == EdmTypeKind.Complex)
            {
                ComplexTypeConfiguration complex = (ComplexTypeConfiguration)structuralType;
                while (complex != null)
                {
                    if (baseStructuralType == complex)
                    {
                        return true;
                    }

                    complex = complex.BaseType;
                }
            }

            return false;
        }
    }
}
