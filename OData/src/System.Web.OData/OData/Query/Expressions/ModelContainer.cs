// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using Microsoft.OData.Edm;

namespace System.Web.OData.Query.Expressions
{
    /// <summary>
    /// EntityFramework does not let you inject non primitive constant values (like IEdmModel) in Select queries. Primitives like strings and guids can be
    /// injected as they can be translated into a SQL query. This container associates a unique string with each EDM model, so that, given the string the model
    /// can be retrieved later.
    /// </summary>
    public static class ModelContainer
    {
        private static ConcurrentDictionary<IEdmModel, string> _map = new ConcurrentDictionary<IEdmModel, string>();
        private static ConcurrentDictionary<string, IEdmModel> _reverseMap = new ConcurrentDictionary<string, IEdmModel>();

        /// <summary>
        /// Gets model id by model.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string GetModelID(IEdmModel model)
        {
            string index = _map.GetOrAdd(model, m => Guid.NewGuid().ToString());
            _reverseMap.TryAdd(index, model);
            return index;
        }

        /// <summary>
        /// Gets model by model id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IEdmModel GetModel(string id)
        {
            return _reverseMap[id];
        }

        /// <summary>
        /// Remove modle from container.
        /// </summary>
        /// <param name="model"></param>
        public static void RemoveModel(IEdmModel model)
        {
            string id;
            if (_map.TryRemove(model, out id))
            {
                IEdmModel oldModel;
                _reverseMap.TryRemove(id, out oldModel);
            }
        }

        /// <summary>
        /// Clears models cache.
        /// </summary>
        public static void ClearCache()
        {
            _map.Clear();
            _reverseMap.Clear();
        }
    }
}
