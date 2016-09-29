﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData
{
    using Microsoft.AspNetCore.OData.Formatter;

    /// <summary>
    /// Represents an <see cref="IEdmStructuredObject"/> with no backing CLR <see cref="Type"/>.
    /// </summary>
    [NonValidatingParameterBinding]
    public abstract class EdmStructuredObject : Delta, IEdmStructuredObject
    {
        private Dictionary<string, object> _container = new Dictionary<string, object>();
        private HashSet<string> _setProperties = new HashSet<string>();

        private IEdmStructuredType _expectedEdmType;
        private IEdmStructuredType _actualEdmType;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmStructuredObject"/> class.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmStructuredType"/> of this object.</param>
        protected EdmStructuredObject(IEdmStructuredType edmType)
            : this(edmType, isNullable: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmStructuredObject"/> class.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmStructuredTypeReference"/> of this object.</param>
        protected EdmStructuredObject(IEdmStructuredTypeReference edmType)
            : this(edmType.StructuredDefinition(), edmType.IsNullable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmStructuredObject"/> class.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmStructuredTypeReference"/> of this object.</param>
        /// <param name="isNullable">true if this object can be nullable; otherwise, false.</param>
        protected EdmStructuredObject(IEdmStructuredType edmType, bool isNullable)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            _expectedEdmType = edmType;
            _actualEdmType = edmType;
            IsNullable = isNullable;
        }

        /// <summary>
        /// Gets or sets the expected <see cref="IEdmStructuredType"/> of the entity or complex type of this object.
        /// </summary>
        public IEdmStructuredType ExpectedEdmType
        {
            get { return _expectedEdmType; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                if (!_actualEdmType.IsOrInheritsFrom(value))
                {
                    throw Error.InvalidOperation(SRResources.DeltaEntityTypeNotAssignable,
                        _actualEdmType.ToTraceString(), value.ToTraceString());
                }

                _expectedEdmType = value;
            }
        }

        /// <summary>
        /// Gets or sets the actual <see cref="IEdmStructuredType" /> of the entity or complex type of this object.
        /// </summary>
        public IEdmStructuredType ActualEdmType
        {
            get { return _actualEdmType; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                if (!value.IsOrInheritsFrom(_expectedEdmType))
                {
                    throw Error.InvalidOperation(SRResources.DeltaEntityTypeNotAssignable,
                        value.ToTraceString(), _expectedEdmType.ToTraceString());
                }

                _actualEdmType = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the EDM object is nullable or not.
        /// </summary>
        public bool IsNullable { get; set; }

        /// <inheritdoc/>
        public override void Clear()
        {
            _container.Clear();
            _setProperties.Clear();
        }

        /// <inheritdoc/>
        public override bool TrySetPropertyValue(string name, object value)
        {
            IEdmProperty property = _actualEdmType.FindProperty(name);
            if (property != null || _actualEdmType.IsOpen)
            {
                _setProperties.Add(name);
                _container[name] = value;
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public override bool TryGetPropertyValue(string name, out object value)
        {
            IEdmProperty property = _actualEdmType.FindProperty(name);
            if (property != null || _actualEdmType.IsOpen)
            {
                if (_container.ContainsKey(name))
                {
                    value = _container[name];
                    return true;
                }
                else
                {
                    value = GetDefaultValue(property.Type);
                    // store the default value (but don't update the list of 'set properties').
                    _container[name] = value;
                    return true;
                }
            }
            else
            {
                value = null;
                return false;
            }
        }

        /// <inheritdoc/>
        public override bool TryGetPropertyType(string name, out Type type)
        {
            IEdmProperty property = _actualEdmType.FindProperty(name);
            if (property != null)
            {
                type = GetClrTypeForUntypedDelta(property.Type);
                return true;
            }
            else if (_actualEdmType.IsOpen && _container.ContainsKey(name))
            {
                type = _container[name].GetType();
                return true;
            }
            else
            {
                type = null;
                return false;
            }
        }

        /// <summary>
        /// Get all dynamic properties
        /// </summary>
        public Dictionary<string, object> TryGetDynamicProperties()
        {
            if (!_actualEdmType.IsOpen)
            {
                return new Dictionary<string, object>();
            }
            else
            {
                return _container.Where(p => _actualEdmType.FindProperty(p.Key) == null).ToDictionary(property => property.Key, property => property.Value);
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<string> GetChangedPropertyNames()
        {
            return _setProperties;
        }

        /// <inheritdoc/>
        public override IEnumerable<string> GetUnchangedPropertyNames()
        {
            return _actualEdmType.Properties().Select(p => p.Name).Except(GetChangedPropertyNames());
        }

        /// <inheritdoc/>
        public IEdmTypeReference GetEdmType()
        {
            return EdmLibHelpers.ToEdmTypeReference(_actualEdmType, IsNullable);
        }

        internal static object GetDefaultValue(IEdmTypeReference propertyType)
        {
            Contract.Assert(propertyType != null);

            bool isCollection = propertyType.IsCollection();
            if (!propertyType.IsNullable || isCollection)
            {
                Type clrType = GetClrTypeForUntypedDelta(propertyType);

                if (propertyType.IsPrimitive() ||
                    (isCollection && propertyType.AsCollection().ElementType().IsPrimitive()))
                {
                    // primitive or primitive collection
                    return Activator.CreateInstance(clrType);
                }
                else
                {
                    // IEdmObject
                    return Activator.CreateInstance(clrType, propertyType);
                }
            }

            return null;
        }

        internal static Type GetClrTypeForUntypedDelta(IEdmTypeReference edmType)
        {
            Contract.Assert(edmType != null);

            switch (edmType.TypeKind())
            {
                case EdmTypeKind.Primitive:
                    return EdmLibHelpers.GetClrType(edmType.AsPrimitive(), EdmCoreModel.Instance);

                case EdmTypeKind.Complex:
                    return typeof(EdmComplexObject);

                case EdmTypeKind.Entity:
                    return typeof(EdmEntityObject);

                case EdmTypeKind.Enum:
                    return typeof(EdmEnumObject);

                case EdmTypeKind.Collection:
                    IEdmTypeReference elementType = edmType.AsCollection().ElementType();
                    if (elementType.IsPrimitive())
                    {
                        Type elementClrType = GetClrTypeForUntypedDelta(elementType);
                        return typeof(List<>).MakeGenericType(elementClrType);
                    }
                    else if (elementType.IsComplex())
                    {
                        return typeof(EdmComplexObjectCollection);
                    }
                    else if (elementType.IsEntity())
                    {
                        return typeof(EdmEntityObjectCollection);
                    }
                    else if (elementType.IsEnum())
                    {
                        return typeof(EdmEnumObjectCollection);
                    }
                    break;
            }

            throw Error.InvalidOperation(SRResources.UnsupportedEdmType, edmType.ToTraceString(), edmType.TypeKind());
        }
    }
}
