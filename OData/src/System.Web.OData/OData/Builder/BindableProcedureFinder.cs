// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// This class builds a cache that allows for efficient look up of bindable procedure by EntityType. 
    /// </summary>
    internal class BindableProcedureFinder
    {
        private Dictionary<IEdmEntityType, List<IEdmOperation>> _map = new Dictionary<IEdmEntityType, List<IEdmOperation>>();

        /// <summary>
        /// Constructs a concurrent cache for looking up bindable procedures for any EntityType in the provided model.
        /// </summary>
        public BindableProcedureFinder(IEdmModel model)
        {
            var operationGroups =
                from op in model.SchemaElements.OfType<IEdmOperation>()
                where op.IsBound && op.Parameters.First().Type.TypeKind() == EdmTypeKind.Entity
                group op by op.Parameters.First().Type.Definition;
            
            foreach (var operationGroup in operationGroups)
            {
                var entityType = operationGroup.Key as IEdmEntityType;
                if (entityType != null)
                {
                    _map[entityType] = operationGroup.ToList();
                }
            }
        }

        /// <summary>
        /// Finds procedures that can be invoked on the given entity type. This would include all the procedures that are bound
        /// to the given type and its base types.
        /// </summary>
        /// <param name="entityType">The EDM entity type.</param>
        /// <returns>A collection of procedures bound to the entity type.</returns>
        public virtual IEnumerable<IEdmOperation> FindProcedures(IEdmEntityType entityType)
        {
            return GetTypeHierarchy(entityType).SelectMany(FindDeclaredProcedures);
        }

        private static IEnumerable<IEdmEntityType> GetTypeHierarchy(IEdmEntityType entityType)
        {
            IEdmEntityType current = entityType;
            while (current != null)
            {
                yield return current;
                current = current.BaseEntityType();
            }
        }

        private IEnumerable<IEdmOperation> FindDeclaredProcedures(IEdmEntityType entityType)
        {
            List<IEdmOperation> results;

            if (_map.TryGetValue(entityType, out results))
            {
                return results;
            }
            else
            {
                return Enumerable.Empty<IEdmFunction>();
            }
        }
    }
}
