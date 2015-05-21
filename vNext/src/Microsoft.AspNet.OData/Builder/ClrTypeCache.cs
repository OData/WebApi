// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
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
                edmType = model.GetEdmType(clrType).ToEdmTypeReference(false);
                _cache[clrType] = edmType;
            }

            return edmType;
        }
    }
}
