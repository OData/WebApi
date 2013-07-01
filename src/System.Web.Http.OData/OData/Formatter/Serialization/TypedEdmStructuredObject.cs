// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Reflection;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="IEdmStructuredObject"/> backed by a CLR object with a one-to-one mapping.
    /// </summary>
    internal abstract class TypedEdmStructuredObject : IEdmStructuredObject
    {
        private IEdmStructuredTypeReference _edmType;

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

            Type type = Instance.GetType();
            PropertyInfo property = type.GetProperty(propertyName);
            if (property == null)
            {
                value = null;
                return false;
            }
            else
            {
                value = property.GetValue(Instance);
                return true;
            }
        }
    }
}
