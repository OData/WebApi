// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Web.Http.Internal;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="IEdmStructuredObject"/> backed by a CLR object with a one-to-one mapping.
    /// </summary>
    internal abstract class TypedEdmStructuredObject : IEdmStructuredObject
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<string, Func<object, object>>> _propertyGetterCache =
            new ConcurrentDictionary<Type, Dictionary<string, Func<object, object>>>();

        private IEdmStructuredTypeReference _edmType;
        private Type _type;
        private Dictionary<string, Func<object, object>> _typePropertyGetterCache = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedEdmStructuredObject"/> class.
        /// </summary>
        /// <param name="instance">The backing CLR instance.</param>
        /// <param name="edmType">The <see cref="IEdmStructuredType"/> of this object.</param>
        /// <param name="edmModel">The <see cref="IEdmModel"/>.</param>
        protected TypedEdmStructuredObject(object instance, IEdmStructuredTypeReference edmType, IEdmModel edmModel)
        {
            Contract.Assert(edmType != null);

            Instance = instance;
            _edmType = edmType;
            _type = instance == null ? null : instance.GetType();
            Model = edmModel;
        }

        /// <summary>
        /// Gets the backing CLR object.
        /// </summary>
        public object Instance { get; private set; }

        /// <summary>
        /// Gets the EDM model.
        /// </summary>
        public IEdmModel Model { get; private set; }

        /// <inheritdoc/>
        public IEdmTypeReference GetEdmType()
        {
            return _edmType;
        }

        /// <inheritdoc/>
        public bool TryGetPropertyValue(string propertyName, out object value)
        {
            if (Instance == null)
            {
                value = null;
                return false;
            }

            Contract.Assert(_type != null);

            Func<object, object> getter = GetOrCreatePropertyGetter(_type, propertyName, _edmType, Model, ref _typePropertyGetterCache);
            if (getter == null)
            {
                value = null;
                return false;
            }
            else
            {
                value = getter(Instance);
                return true;
            }
        }

        internal static Func<object, object> GetOrCreatePropertyGetter(
            Type type,
            string propertyName,
            IEdmStructuredTypeReference edmType,
            IEdmModel model,
            ref Dictionary<string, Func<object, object>> propertyGetterCache)
        {
            EnsurePropertyGettingCachePopulated(type, edmType, model, ref propertyGetterCache);

            return propertyGetterCache[propertyName];
        }

        private static void EnsurePropertyGettingCachePopulated(Type type, IEdmStructuredTypeReference edmType, IEdmModel model, ref Dictionary<string, Func<object, object>> propertyGetterCache)
        {
            if (propertyGetterCache == null)
            {
                propertyGetterCache = _propertyGetterCache.GetOrAdd(type, t =>
                {
                    // Creating all property getters on first access to the type
                    // It will allows us to avoid growing dictionary from 0 to number of properties that means copy data over and over as soon as capacity reached
                    // Also we don't need ConcurrentDictionary for nested getters cache and use Dictionary that adds less overhead

                    // First get all properties
                    var properties = edmType.StructuredDefinition().Properties().ToList();
                    // Create dictionary with right capacity
                    var result = new Dictionary<string, Func<object, object>>(properties.Count);

                    // Fill dictionary with getters for each property
                    foreach (IEdmProperty property in properties)
                    {
                        var name = EdmLibHelpers.GetClrPropertyName(property, model) ?? property.Name;
                        result.Add(property.Name, CreatePropertyGetter(type, name));
                    }
                    return result;
                });
            }
        }

        internal static Func<object, object> GetOrCreatePropertyGetter(
                          Type type,
                          string propertyName,
                          IEdmStructuredTypeReference edmType,
                          IEdmModel model)
        {
            Dictionary<string, Func<object, object>> propertyGetterCache = null;
            return GetOrCreatePropertyGetter(type, propertyName, edmType, model, ref propertyGetterCache);
        }

        private static Func<object, object> CreatePropertyGetter(Type type, string propertyName)
        {
            PropertyInfo property = type.GetProperty(propertyName);

            if (property == null)
            {
                return null;
            }

            var helper = new PropertyHelper(property);

            return helper.GetValue;
        }
    }
}
