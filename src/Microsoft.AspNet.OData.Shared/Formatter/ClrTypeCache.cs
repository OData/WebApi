// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter
{
    internal class ClrTypeCache
    {
        private ConcurrentDictionary<Type, IEdmTypeReference> _clrToEdmTypeCache = new ConcurrentDictionary<Type, IEdmTypeReference>();

        private ConcurrentDictionary<EdmTypeCacheItem, Type> _edmToClrTypeCache
            = new ConcurrentDictionary<EdmTypeCacheItem, Type>(new EdmTypeCacheItemComparer());

        // for unit test only
        internal ConcurrentDictionary<Type, IEdmTypeReference> ClrToEdmTypeCache => _clrToEdmTypeCache;
        internal ConcurrentDictionary<EdmTypeCacheItem, Type> EdmToClrTypeCache => _edmToClrTypeCache;

        public IEdmTypeReference GetEdmType(Type clrType, IEdmModel model)
        {
            IEdmTypeReference edmType;
            if (!_clrToEdmTypeCache.TryGetValue(clrType, out edmType))
            {
                edmType = model.GetEdmTypeReference(clrType);
                _clrToEdmTypeCache[clrType] = edmType;
            }

            return edmType;
        }

        public Type GetClrType(IEdmTypeReference edmType, IEdmModel edmModel)
        {
            Type clrType;

            EdmTypeCacheItem item = new EdmTypeCacheItem(edmType.Definition, edmType.IsNullable);
            if (!_edmToClrTypeCache.TryGetValue(item, out clrType))
            {
                clrType = EdmLibHelpers.GetClrType(edmType, edmModel);
                _edmToClrTypeCache[item] = clrType;
            }

            return clrType;
        }

        internal struct EdmTypeCacheItem
        {
            public IEdmType EdmType { get; }

            public bool Nullable { get; }

            public EdmTypeCacheItem(IEdmType edmType, bool nullable)
            {
                EdmType = edmType;
                Nullable = nullable;
            }
        }

        internal class EdmTypeCacheItemComparer : IEqualityComparer<EdmTypeCacheItem>
        {
            public bool Equals(EdmTypeCacheItem x, EdmTypeCacheItem y)
            {
                return (x.EdmType == y.EdmType) && (x.Nullable == y.Nullable);
            }

            public int GetHashCode(EdmTypeCacheItem obj)
            {
                string combined = $"{obj.EdmType.FullTypeName()}~{obj.Nullable}";
                return combined.GetHashCode();
            }
        }
    }
}
