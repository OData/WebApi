// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Query.Expressions
{
    /// <summary>
    /// EntityFramework does not let you inject non primitive constant values (like IEdmModel) in Select queries. Primitives like strings and guids can be
    /// injected as they can be translated into a SQL query. This container associates a unique string with each EDM model, so that, given the string the model
    /// can be retrieved later.
    /// </summary>
    internal static class ModelContainer
    {
        private static ConcurrentDictionary<IEdmModel, string> _map = new ConcurrentDictionary<IEdmModel, string>();
        private static ConcurrentDictionary<string, IEdmModel> _reverseMap = new ConcurrentDictionary<string, IEdmModel>();

        public static string GetModelID(IEdmModel model)
        {
            string index = _map.GetOrAdd(model, m => Guid.NewGuid().ToString());
            _reverseMap.TryAdd(index, model);
            return index;
        }

        public static IEdmModel GetModel(string id)
        {
            return _reverseMap[id];
        }
    }
}
