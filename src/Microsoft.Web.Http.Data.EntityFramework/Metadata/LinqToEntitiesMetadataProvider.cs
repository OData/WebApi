// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Metadata.Edm;
using Microsoft.Web.Http.Data.Metadata;

namespace Microsoft.Web.Http.Data.EntityFramework.Metadata
{
    internal class LinqToEntitiesMetadataProvider : MetadataProvider
    {
        private static ConcurrentDictionary<Type, LinqToEntitiesTypeDescriptionContext> _tdpContextMap = new ConcurrentDictionary<Type, LinqToEntitiesTypeDescriptionContext>();
        private readonly LinqToEntitiesTypeDescriptionContext _typeDescriptionContext;
        private readonly bool _isDbContext;
        private Dictionary<Type, ICustomTypeDescriptor> _descriptors = new Dictionary<Type, ICustomTypeDescriptor>();

        public LinqToEntitiesMetadataProvider(Type contextType, MetadataProvider parent, bool isDbContext)
            : base(parent)
        {
            _isDbContext = isDbContext;

            _typeDescriptionContext = _tdpContextMap.GetOrAdd(contextType, type =>
            {
                // create and cache a context for this provider type
                return new LinqToEntitiesTypeDescriptionContext(contextType, _isDbContext);
            });
        }

        /// <summary>
        /// Returns a custom type descriptor for the specified type (either an entity or complex type).
        /// </summary>
        /// <param name="objectType">Type of object for which we need the descriptor</param>
        /// <param name="parent">The parent type descriptor</param>
        /// <returns>Custom type description for the specified type</returns>
        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, ICustomTypeDescriptor parent)
        {
            // No need to deal with concurrency... Worst case scenario we have multiple 
            // instances of this thing.
            ICustomTypeDescriptor td = null;
            if (!_descriptors.TryGetValue(objectType, out td))
            {
                // call into base so the TDs are chained
                parent = base.GetTypeDescriptor(objectType, parent);

                StructuralType edmType = _typeDescriptionContext.GetEdmType(objectType);
                if (edmType != null &&
                    (edmType.BuiltInTypeKind == BuiltInTypeKind.EntityType || edmType.BuiltInTypeKind == BuiltInTypeKind.ComplexType))
                {
                    // only add an LTE TypeDescriptor if the type is an EF Entity or ComplexType
                    td = new LinqToEntitiesTypeDescriptor(_typeDescriptionContext, edmType, parent);
                }
                else
                {
                    td = parent;
                }

                _descriptors[objectType] = td;
            }

            return td;
        }

        public override bool LookUpIsEntityType(Type type)
        {
            StructuralType edmType = _typeDescriptionContext.GetEdmType(type);
            if (edmType != null && edmType.BuiltInTypeKind == BuiltInTypeKind.EntityType)
            {
                return true;
            }
            return false;
        }
    }
}
