// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
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
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, Func<object, object>>> _propertyGetterCache =
            new ConcurrentDictionary<Type, ConcurrentDictionary<string, Func<object, object>>>();

        private IEdmStructuredTypeReference _edmType;
        private Type _type;
        private ConcurrentDictionary<string, Func<object, object>> _typePropertyGetterCache = null;

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
            ref ConcurrentDictionary<string, Func<object, object>> propertyGetterCache)
        {
            if (propertyGetterCache == null)
            {
                propertyGetterCache = _propertyGetterCache.GetOrAdd(type, t => new ConcurrentDictionary<string, Func<object, object>>());
            }

            return propertyGetterCache.GetOrAdd(propertyName, name =>
            {
                IEdmProperty property = edmType.FindProperty(name);
                if (property != null && model != null)
                {
                    name = EdmLibHelpers.GetClrPropertyName(property, model) ?? name;
                }

                return CreatePropertyGetter(type, name);
            });
        }

        internal static Func<object, object> GetOrCreatePropertyGetter(
                          Type type,
                          string propertyName,
                          IEdmStructuredTypeReference edmType,
                          IEdmModel model)
        {
            ConcurrentDictionary<string, Func<object, object>> propertyGetterCache = null;
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
