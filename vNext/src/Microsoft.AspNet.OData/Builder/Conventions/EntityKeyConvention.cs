// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using System.Web.OData.Formatter;
using Microsoft.AspNet.OData;

namespace System.Web.OData.Builder.Conventions
{
    /// <summary>
    /// <see cref="EntityTypeConvention"/> for figuring out the entity keys.
    /// <remarks>This convention configures properties that are named 'ID' (case-insensitive) or {EntityName}+ID (case-insensitive) as the key.</remarks>
    /// </summary>
    internal class EntityKeyConvention : EntityTypeConvention
    {
        /// <summary>
        /// Figures out the key properties and marks them as Keys in the EDM model.
        /// </summary>
        /// <param name="entity">The entity type being configured.</param>
        /// <param name="model">The <see cref="ODataModelBuilder"/>.</param>
        public override void Apply(EntityTypeConfiguration entity, ODataConventionModelBuilder model)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }

            // Suppress the EntityKeyConvention if there is any key in EntityTypeConfiguration.
            if (entity.Keys.Any())
            {
                return;
            }

            // Try to figure out keys only if there is no base type.
            if (entity.BaseType == null)
            {
                PropertyConfiguration key = GetKeyProperty(entity);
                if (key != null)
                {
                    entity.HasKey(key.PropertyInfo);
                }
            }
        }

        private static PropertyConfiguration GetKeyProperty(EntityTypeConfiguration entityType)
        {
            IEnumerable<PropertyConfiguration> keys =
                entityType.Properties
                .Where(p => (p.Name.Equals(entityType.Name + "Id", StringComparison.OrdinalIgnoreCase) || p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                && EdmLibHelpers.GetEdmPrimitiveTypeOrNull(p.PropertyInfo.PropertyType) != null);

            if (keys.Count() == 1)
            {
                return keys.Single();
            }

            return null;
        }
    }
}
