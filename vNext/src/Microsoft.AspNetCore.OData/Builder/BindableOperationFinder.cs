// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Builder
{
    /// <summary>
    /// This class builds a cache that allows for efficient look up of bindable operation by EntityType. 
    /// </summary>
    public class BindableOperationFinder
    {
        private Dictionary<IEdmEntityType, List<IEdmOperation>> _map = new Dictionary<IEdmEntityType, List<IEdmOperation>>();

        private Dictionary<IEdmEntityType, List<IEdmOperation>> _collectionMap = new Dictionary<IEdmEntityType, List<IEdmOperation>>();

        /// <summary>
        /// Constructs a concurrent cache for looking up bindable operations for any EntityType in the provided model.
        /// </summary>
        public BindableOperationFinder(IEdmModel model)
        {
            var operationGroups =
                from op in model.SchemaElements.OfType<IEdmOperation>()
                where op.IsBound && (op.Parameters.First().Type.TypeKind() == EdmTypeKind.Entity || op.Parameters.First().Type.TypeKind() == EdmTypeKind.Collection)
                group op by op.Parameters.First().Type.Definition;

            foreach (var operationGroup in operationGroups)
            {
                var entityType = operationGroup.Key as IEdmEntityType;
                if (entityType != null)
                {
                    _map[entityType] = operationGroup.ToList();
                }

                var collectionType = operationGroup.Key as IEdmCollectionType;
                if (collectionType != null)
                {
                    var elementType = collectionType.ElementType.Definition as IEdmEntityType;
                    if (elementType != null)
                    {
                        // because collection type is temp instance.
                        List<IEdmOperation> value;
                        if (_collectionMap.TryGetValue(elementType, out value))
                        {
                            value.AddRange(operationGroup);
                        }
                        else
                        {
                            _collectionMap[elementType] = operationGroup.ToList();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds operations that can be invoked on the given entity type. This would include all the operations that are bound
        /// to the given type and its base types.
        /// </summary>
        /// <param name="entityType">The EDM entity type.</param>
        /// <returns>A collection of operations bound to the entity type.</returns>
        public virtual IEnumerable<IEdmOperation> FindOperations(IEdmEntityType entityType)
        {
            return GetTypeHierarchy(entityType).SelectMany(FindDeclaredOperations);
        }

        /// <summary>
        /// Finds operations that can be invoked on the feed. This would include all the operations that are bound to the given
        /// type and its base types.
        /// </summary>
        /// <param name="entityType">The EDM entity type.</param>
        /// <returns>A collection of operations bound to the feed.</returns>
        public virtual IEnumerable<IEdmOperation> FindOperationsBoundToCollection(IEdmEntityType entityType)
        {
            return GetTypeHierarchy(entityType).SelectMany(FindDeclaredOperationsBoundToCollection);
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

        private IEnumerable<IEdmOperation> FindDeclaredOperations(IEdmEntityType entityType)
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

        private IEnumerable<IEdmOperation> FindDeclaredOperationsBoundToCollection(IEdmEntityType entityType)
        {
            List<IEdmOperation> results;

            if (_collectionMap.TryGetValue(entityType, out results))
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
