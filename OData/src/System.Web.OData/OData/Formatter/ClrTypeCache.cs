// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter
{
    internal class ClrTypeCache
    {
        private ConcurrentDictionary<Type, IEdmTypeReference> _cache =
            new ConcurrentDictionary<Type, IEdmTypeReference>();

        public IEdmTypeReference GetEdmType(Type clrType, IEdmModel model)
        {
            IEdmTypeReference edmType;
            if (!_cache.TryGetValue(clrType, out edmType))
            {
                edmType = model.GetEdmTypeReference(clrType);
                _cache[clrType] = edmType;
            }

            return edmType;
        }
    }
}
