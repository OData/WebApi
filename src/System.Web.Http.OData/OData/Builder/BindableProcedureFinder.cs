// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// This class builds a cache that allows for efficient look up of bindable procedure by EntityType. 
    /// </summary>
    public class BindableProcedureFinder
    {
        private Dictionary<IEdmEntityType, List<IEdmFunctionImport>> _map = new Dictionary<IEdmEntityType, List<IEdmFunctionImport>>();

        /// <summary>
        /// Constructs a concurrent cache for looking up bindable procedures for any EntityType in the provided model.
        /// </summary>
        public BindableProcedureFinder(IEdmModel model)
        {
            var query =
                from ec in model.EntityContainers()
                from fi in ec.FunctionImports()
                where fi.IsBindable && fi.Parameters.First().Type.TypeKind() == EdmTypeKind.Entity
                group fi by fi.Parameters.First().Type.Definition into fgroup
                select new { EntityType = fgroup.Key as IEdmEntityType, BindableFunctions = fgroup.ToList() };

            foreach (var match in query)
            {
                _map[match.EntityType] = match.BindableFunctions;
            }
        }

        public virtual IEnumerable<IEdmFunctionImport> FindProcedures(IEdmEntityType entityType)
        {
            return GetTypeHierarchy(entityType).SelectMany(e => FindDeclaredProcedures(e));
        }

        private IEnumerable<IEdmEntityType> GetTypeHierarchy(IEdmEntityType entityType)
        {
            IEdmEntityType current = entityType;
            while (current != null)
            {
                yield return current;
                current = current.BaseEntityType();
            }
        }

        private IEnumerable<IEdmFunctionImport> FindDeclaredProcedures(IEdmEntityType entityType)
        {
            List<IEdmFunctionImport> results = null;

            if (_map.TryGetValue(entityType, out results))
            {
                return results;
            }
            else
            {
                return Enumerable.Empty<IEdmFunctionImport>();
            }
        }
    }
}
