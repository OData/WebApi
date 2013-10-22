// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Web.Http.Internal;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="IEdmStructuredObject"/> backed by a CLR object with a one-to-one mapping.
    /// </summary>
    internal abstract class TypedEdmStructuredObject : IEdmStructuredObject
    {
        private static readonly ConcurrentDictionary<Tuple<string, Type>, Func<object, object>> _propertyGetterCache =
            new ConcurrentDictionary<Tuple<string, Type>, Func<object, object>>();

        private IEdmStructuredTypeReference _edmType;
        private Type _type;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedEdmStructuredObject"/> class.
        /// </summary>
        /// <param name="instance">The backing CLR instance.</param>
        /// <param name="edmType">The <see cref="IEdmStructuredType"/> of this object.</param>
        protected TypedEdmStructuredObject(object instance, IEdmStructuredTypeReference edmType)
        {
            Contract.Assert(edmType != null);

            Instance = instance;
            _edmType = edmType;
            _type = instance == null ? null : instance.GetType();
        }

        /// <summary>
        /// Gets the backing CLR object.
        /// </summary>
        public object Instance { get; private set; }

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

            Func<object, object> getter = GetOrCreatePropertyGetter(_type, propertyName);
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

        internal static Func<object, object> GetOrCreatePropertyGetter(Type type, string propertyName)
        {
            Tuple<string, Type> key = Tuple.Create(propertyName, type);
            Func<object, object> getter;

            if (!_propertyGetterCache.TryGetValue(key, out getter))
            {
                getter = CreatePropertyGetter(type, propertyName);
                _propertyGetterCache[key] = getter;
            }

            return getter;
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
