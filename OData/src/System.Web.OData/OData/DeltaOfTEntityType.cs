// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;

namespace System.Web.OData
{
    /// <summary>
    /// A class the tracks changes (i.e. the Delta) for a particular <typeparamref name="TEntityType"/>.
    /// </summary>
    /// <typeparam name="TEntityType">TEntityType is the base type of entity this delta tracks changes for.</typeparam>
    [NonValidatingParameterBinding]
    public class Delta<TEntityType> : TypedDelta, IDelta where TEntityType : class
    {
        // cache property accessors for this type and all its derived types.
        private static ConcurrentDictionary<Type, Dictionary<string, PropertyAccessor<TEntityType>>> _propertyCache
            = new ConcurrentDictionary<Type, Dictionary<string, PropertyAccessor<TEntityType>>>();

        private Dictionary<string, PropertyAccessor<TEntityType>> _allProperties;
        private HashSet<string> _updatableProperties;

        private HashSet<string> _changedProperties;
        private TEntityType _entity;
        private Type _entityType;

        private PropertyInfo _dynamicDictionaryPropertyinfo;
        private HashSet<string> _changedDynamicProperties;
        private IDictionary<string, object> _dynamicDictionaryCache;
 
        /// <summary>
        /// Initializes a new instance of <see cref="Delta{TEntityType}"/>.
        /// </summary>
        public Delta()
            : this(typeof(TEntityType))
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Delta{TEntityType}"/>.
        /// </summary>
        /// <param name="entityType">The derived entity type for which the changes would be tracked.
        /// <paramref name="entityType"/> should be assignable to instances of <typeparamref name="TEntityType"/>.
        /// </param>
        public Delta(Type entityType)
            : this(entityType, updatableProperties: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Delta{TEntityType}"/>.
        /// </summary>
        /// <param name="entityType">The derived entity type for which the changes would be tracked.
        /// <paramref name="entityType"/> should be assignable to instances of <typeparamref name="TEntityType"/>.
        /// </param>
        /// <param name="updatableProperties">The set of properties that can be updated or reset. Unknown property
        /// names, including those of dynamic properties, are ignored.</param>
        public Delta(Type entityType, IEnumerable<string> updatableProperties)
            : this(entityType, updatableProperties: updatableProperties, dynamicDictionaryPropertyInfo: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Delta{TEntityType}"/>.
        /// </summary>
        /// <param name="entityType">The derived entity type for which the changes would be tracked.
        /// <paramref name="entityType"/> should be assignable to instances of <typeparamref name="TEntityType"/>.
        /// </param>
        /// <param name="updatableProperties">The set of properties that can be updated or reset. Unknown property
        /// names, including those of dynamic properties, are ignored.</param>
        /// <param name="dynamicDictionaryPropertyInfo">The property info that is used as dictionary of dynamic
        /// properties. <c>null</c> means this entity type is not open.</param>
        public Delta(Type entityType, IEnumerable<string> updatableProperties,
            PropertyInfo dynamicDictionaryPropertyInfo)
        {
            _dynamicDictionaryPropertyinfo = dynamicDictionaryPropertyInfo;
            Reset(entityType);
            InitializeProperties(updatableProperties);
        }

        /// <inheritdoc/>
        public override Type EntityType
        {
            get
            {
                return _entityType;
            }
        }

        /// <inheritdoc/>
        public override Type ExpectedClrType
        {
            get { return typeof(TEntityType); }
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            Reset(_entityType);
        }

        /// <inheritdoc/>
        public override bool TrySetPropertyValue(string name, object value)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }

            if (_dynamicDictionaryPropertyinfo != null)
            {
                // Dynamic property can have the same name as the dynamic property dictionary.
                if (name == _dynamicDictionaryPropertyinfo.Name ||
                    !_allProperties.ContainsKey(name))
                {
                    if (_dynamicDictionaryCache == null)
                    {
                        _dynamicDictionaryCache =
                            GetDynamicPropertyDictionary(_dynamicDictionaryPropertyinfo, _entity, create: true);
                    }

                    _dynamicDictionaryCache[name] = value;
                    _changedDynamicProperties.Add(name);
                    return true;
                }
            }

            if (!_updatableProperties.Contains(name))
            {
                return false;
            }

            PropertyAccessor<TEntityType> cacheHit = _allProperties[name];

            if (value == null && !EdmLibHelpers.IsNullable(cacheHit.Property.PropertyType))
            {
                return false;
            }

            Type propertyType = cacheHit.Property.PropertyType;
            if (value != null && !propertyType.IsCollection() && !propertyType.IsAssignableFrom(value.GetType()))
            {
                return false;
            }

            //.Setter.Invoke(_entity, new object[] { value });
            cacheHit.SetValue(_entity, value);
            _changedProperties.Add(name);
            return true;
        }

        /// <inheritdoc/>
        public override bool TryGetPropertyValue(string name, out object value)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }

            if (_dynamicDictionaryPropertyinfo != null)
            {
                if (_dynamicDictionaryCache == null)
                {
                    _dynamicDictionaryCache = 
                        GetDynamicPropertyDictionary(_dynamicDictionaryPropertyinfo, _entity, create: false);
                }

                if (_dynamicDictionaryCache != null && _dynamicDictionaryCache.TryGetValue(name, out value))
                {
                    return true;
                }
            }

            PropertyAccessor<TEntityType> cacheHit;
            if (_allProperties.TryGetValue(name, out cacheHit))
            {
                value = cacheHit.GetValue(_entity);
                return true;
            }

            value = null;
            return false;
        }

        /// <inheritdoc/>
        public override bool TryGetPropertyType(string name, out Type type)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }

            if (_dynamicDictionaryPropertyinfo != null)
            {
                if (_dynamicDictionaryCache == null)
                {
                    _dynamicDictionaryCache =
                        GetDynamicPropertyDictionary(_dynamicDictionaryPropertyinfo, _entity, create: false);
                }

                object dynamicValue;
                if (_dynamicDictionaryCache != null &&
                    _dynamicDictionaryCache.TryGetValue(name, out dynamicValue))
                {
                    if (dynamicValue == null)
                    {
                        type = null;
                        return false;
                    }

                    type = dynamicValue.GetType();
                    return true;
                }
            }

            PropertyAccessor<TEntityType> value;
            if (_allProperties.TryGetValue(name, out value))
            {
                type = value.Property.PropertyType;
                return true;
            }

            type = null;
            return false;
        }

        /// <summary>
        /// Returns the <see cref="EntityType"/> instance
        /// that holds all the changes (and original values) being tracked by this Delta.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not appropriate to be a property")]
        public TEntityType GetEntity()
        {
            return _entity;
        }

        /// <summary>
        /// Returns the known properties that have been modified through this <see cref="Delta"/> as an
        /// <see cref="IEnumerable{T}" /> of property Names. Does not include the names of the changed
        /// dynamic properties.
        /// </summary>
        public override IEnumerable<string> GetChangedPropertyNames()
        {
            return _changedProperties;
        }

        /// <summary>
        /// Returns the known properties that have not been modified through this <see cref="Delta"/> as an
        /// <see cref="IEnumerable{T}" /> of property Names. Does not include the names of the changed dynamic
        /// properties.
        /// </summary>
        public override IEnumerable<string> GetUnchangedPropertyNames()
        {
            return _updatableProperties.Except(GetChangedPropertyNames());
        }

        /// <summary>
        /// Copies the changed property values from the underlying entity (accessible via <see cref="GetEntity()" />) 
        /// to the <paramref name="original"/> entity.
        /// </summary>
        /// <param name="original">The entity to be updated.</param>
        public void CopyChangedValues(TEntityType original)
        {
            if (original == null)
            {
                throw Error.ArgumentNull("original");
            }

            if (!_entityType.IsAssignableFrom(original.GetType()))
            {
                throw Error.Argument("original", SRResources.DeltaTypeMismatch, _entityType, original.GetType());
            }

            PropertyAccessor<TEntityType>[] propertiesToCopy = GetChangedPropertyNames().Select(s => _allProperties[s]).ToArray();
            foreach (PropertyAccessor<TEntityType> propertyToCopy in propertiesToCopy)
            {
                propertyToCopy.Copy(_entity, original);
            }

            CopyChangedDynamicValues(original);
        }

        // Copy changed dynamic properties and leave the unchanged dynamic properties
        private void CopyChangedDynamicValues(TEntityType targetEntity)
        {
            if (_dynamicDictionaryPropertyinfo == null)
            {
                return;
            }

            if (_dynamicDictionaryCache == null)
            {
                _dynamicDictionaryCache =
                    GetDynamicPropertyDictionary(_dynamicDictionaryPropertyinfo, _entity, create: false);
            }

            IDictionary<string, object> fromDictionary = _dynamicDictionaryCache;
            if (fromDictionary == null)
            {
                return;
            }

            IDictionary<string, object> toDictionary =
                GetDynamicPropertyDictionary(_dynamicDictionaryPropertyinfo, targetEntity, create: false);

            IDictionary<string, object> tempDictionary = toDictionary != null
                ? new Dictionary<string, object>(toDictionary)
                : new Dictionary<string, object>();

            foreach (string dynamicPropertyName in _changedDynamicProperties)
            {
                object dynamicPropertyValue = fromDictionary[dynamicPropertyName];

                // a dynamic propery value equal to null, it means to remove this dynamic property
                if (dynamicPropertyValue == null)
                {
                    tempDictionary.Remove(dynamicPropertyName);
                }
                else
                {
                    tempDictionary[dynamicPropertyName] = dynamicPropertyValue;
                }
            }

            CopyDynamicPropertyDictionary(tempDictionary, toDictionary, _dynamicDictionaryPropertyinfo,
                targetEntity);
        }

        /// <summary>
        /// Copies the unchanged property values from the underlying entity (accessible via <see cref="GetEntity()" />) 
        /// to the <paramref name="original"/> entity.
        /// </summary>
        /// <param name="original">The entity to be updated.</param>
        public void CopyUnchangedValues(TEntityType original)
        {
            if (original == null)
            {
                throw Error.ArgumentNull("original");
            }

            if (!_entityType.IsAssignableFrom(original.GetType()))
            {
                throw Error.Argument("original", SRResources.DeltaTypeMismatch, _entityType, original.GetType());
            }

            IEnumerable<PropertyAccessor<TEntityType>> propertiesToCopy = GetUnchangedPropertyNames().Select(s => _allProperties[s]);
            foreach (PropertyAccessor<TEntityType> propertyToCopy in propertiesToCopy)
            {
                propertyToCopy.Copy(_entity, original);
            }

            CopyUnchangedDynamicValues(original);
        }

        // Missing dynamic structural properties MUST be removed or set to null in *Put*
        private void CopyUnchangedDynamicValues(TEntityType targetEntity)
        {
            if (_dynamicDictionaryPropertyinfo == null)
            {
                return;
            }

            if (_dynamicDictionaryCache == null)
            {
                _dynamicDictionaryCache =
                    GetDynamicPropertyDictionary(_dynamicDictionaryPropertyinfo, _entity, create: false);
            }

            IDictionary<string, object> toDictionary =
                    GetDynamicPropertyDictionary(_dynamicDictionaryPropertyinfo, targetEntity, create: false);

            if (_dynamicDictionaryCache == null)
            {
                if (toDictionary != null)
                {
                    toDictionary.Clear();
                }
            }
            else
            {
                IDictionary<string, object> tempDictionary = toDictionary != null
                    ? new Dictionary<string, object>(toDictionary)
                    : new Dictionary<string, object>();

                List<string> removedSet = tempDictionary.Keys.Except(_changedDynamicProperties).ToList();

                foreach (string name in removedSet)
                {
                    tempDictionary.Remove(name);
                }

                CopyDynamicPropertyDictionary(tempDictionary, toDictionary, _dynamicDictionaryPropertyinfo,
                    targetEntity);
            }
        }

        /// <summary>
        /// Overwrites the <paramref name="original"/> entity with the changes tracked by this Delta.
        /// <remarks>The semantics of this operation are equivalent to a HTTP PATCH operation, hence the name.</remarks>
        /// </summary>
        /// <param name="original">The entity to be updated.</param>
        public void Patch(TEntityType original)
        {
            CopyChangedValues(original);
        }

        /// <summary>
        /// Overwrites the <paramref name="original"/> entity with the values stored in this Delta.
        /// <remarks>The semantics of this operation are equivalent to a HTTP PUT operation, hence the name.</remarks>
        /// </summary>
        /// <param name="original">The entity to be updated.</param>
        public void Put(TEntityType original)
        {
            CopyChangedValues(original);
            CopyUnchangedValues(original);
        }

        private void Reset(Type entityType)
        {
            if (entityType == null)
            {
                throw Error.ArgumentNull("entityType");
            }

            if (!typeof(TEntityType).IsAssignableFrom(entityType))
            {
                throw Error.InvalidOperation(SRResources.DeltaEntityTypeNotAssignable, entityType, typeof(TEntityType));
            }

            _entity = Activator.CreateInstance(entityType) as TEntityType;
            _changedProperties = new HashSet<string>();
            _entityType = entityType;

            _changedDynamicProperties = new HashSet<string>();
            _dynamicDictionaryCache = null;
        }

        private void InitializeProperties(IEnumerable<string> updatableProperties)
        {
            _allProperties = _propertyCache.GetOrAdd(
                _entityType,
                (backingType) => backingType
                    .GetProperties()
                    .Where(p => (p.GetSetMethod() != null || p.PropertyType.IsCollection()) && p.GetGetMethod() != null)
                    .Select<PropertyInfo, PropertyAccessor<TEntityType>>(p => new FastPropertyAccessor<TEntityType>(p))
                    .ToDictionary(p => p.Property.Name));

            if (updatableProperties != null)
            {
                _updatableProperties = new HashSet<string>(updatableProperties);
                _updatableProperties.IntersectWith(_allProperties.Keys);
            }
            else
            {
                _updatableProperties = new HashSet<string>(_allProperties.Keys);
            }

            if (_dynamicDictionaryPropertyinfo != null)
            {
                _updatableProperties.Remove(_dynamicDictionaryPropertyinfo.Name);
            }
        }

        private static void CopyDynamicPropertyDictionary(IDictionary<string, object> source, 
            IDictionary<string, object> dest, PropertyInfo dynamicPropertyInfo, TEntityType targetEntity)
        {
            Contract.Assert(source != null);
            Contract.Assert(dynamicPropertyInfo != null);
            Contract.Assert(targetEntity != null);

            if (source.Count == 0)
            {
                if (dest != null)
                {
                    dest.Clear();
                }
            }
            else
            {
                if (dest == null)
                {
                    dest = GetDynamicPropertyDictionary(dynamicPropertyInfo, targetEntity, create: true);
                }
                else
                {
                    dest.Clear();
                }

                foreach (KeyValuePair<string, object> item in source)
                {
                    dest.Add(item);
                }
            }
        }

        private static IDictionary<string, object> GetDynamicPropertyDictionary(PropertyInfo propertyInfo,
            TEntityType entity, bool create)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }

            object propertyValue = propertyInfo.GetValue(entity);
            if (propertyValue != null)
            {
                return (IDictionary<string, object>)propertyValue;
            }

            if (create)
            {
                if (!propertyInfo.CanWrite)
                {
                    throw Error.InvalidOperation(SRResources.CannotSetDynamicPropertyDictionary, propertyInfo.Name,
                            entity.GetType().FullName);
                }
                IDictionary<string, object> newPropertyValue = new Dictionary<string, object>();

                propertyInfo.SetValue(entity, newPropertyValue);
                return newPropertyValue;
            }

            return null;
        }
    }
}
