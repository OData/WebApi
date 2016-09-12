// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using Microsoft.OData.Edm;
using System.Diagnostics;
using System.Web.OData.Query.Expressions;

namespace System.Web.OData.Formatter
{
    internal class ClrTypeCache
    {
        private ConcurrentDictionary<Type, IEdmTypeReference> _cache =
            new ConcurrentDictionary<Type, IEdmTypeReference>();

        public IEdmTypeReference GetEdmType(Type clrType, IEdmModel model)
        {
            // Dynamicly generated types don't have corresponding IEdmType
            if (clrType.IsGenericType && typeof(DynamicTypeWrapper).IsAssignableFrom(clrType.GenericTypeArguments[0]))
            {
                return null;
            }

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
