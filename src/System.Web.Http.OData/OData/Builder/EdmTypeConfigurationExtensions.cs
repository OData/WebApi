// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace System.Web.Http.OData.Builder
{
    internal static class EdmTypeConfigurationExtensions
    {
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
