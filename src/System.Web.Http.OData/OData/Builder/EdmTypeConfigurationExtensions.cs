// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace System.Web.Http.OData.Builder
{
    internal static class EdmTypeConfigurationExtensions
    {
        // returns all the properties declared in the base types of this type.
        public static IEnumerable<PropertyConfiguration> DerivedProperties(this IEntityTypeConfiguration entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }

            IEntityTypeConfiguration baseType = entity.BaseType;

            while (baseType != null)
            {
                foreach (PropertyConfiguration property in baseType.Properties)
                {
                    yield return property;
                }

                baseType = baseType.BaseType;
            }
        }

        // returns the keys declared or inherited for this entity 
        public static IEnumerable<PropertyConfiguration> Keys(this IEntityTypeConfiguration entity)
        {
            Contract.Assert(entity != null);
            return entity.BaseType == null ? entity.Keys : Keys(entity.BaseType);
        }

        // Returns the base types, this type and all the derived types of this type.
        public static IEnumerable<IEntityTypeConfiguration> ThisAndBaseAndDerivedTypes(this ODataModelBuilder modelBuilder, IEntityTypeConfiguration entity)
        {
            Contract.Assert(modelBuilder != null);
            Contract.Assert(entity != null);

            return entity.BaseTypes()
                    .Concat(new[] { entity })
                    .Concat(modelBuilder.DerivedTypes(entity));
        }

        // Returns the base types for this type.
        public static IEnumerable<IEntityTypeConfiguration> BaseTypes(this IEntityTypeConfiguration entity)
        {
            Contract.Assert(entity != null);

            entity = entity.BaseType;
            while (entity != null)
            {
                yield return entity;
                entity = entity.BaseType;
            }
        }

        // Returns all the derived types of this type.
        public static IEnumerable<IEntityTypeConfiguration> DerivedTypes(this ODataModelBuilder modelBuilder, IEntityTypeConfiguration entity)
        {
            if (modelBuilder == null)
            {
                throw Error.ArgumentNull("modelBuilder");
            }

            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }

            IEnumerable<IEntityTypeConfiguration> derivedEntities = modelBuilder.StructuralTypes.OfType<IEntityTypeConfiguration>().Where(e => e.BaseType == entity);

            foreach (IEntityTypeConfiguration derivedEntity in derivedEntities)
            {
                yield return derivedEntity;
                foreach (IEntityTypeConfiguration derivedDerivedEntity in modelBuilder.DerivedTypes(derivedEntity))
                {
                    yield return derivedDerivedEntity;
                }
            }
        }
    }
}
