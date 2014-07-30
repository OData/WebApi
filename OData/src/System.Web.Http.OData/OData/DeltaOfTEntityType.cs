// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;

namespace System.Web.Http.OData
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
        /// <paramref name="entityType"/> should be assignable to instances of <typeparamref name="TEntityType"/>.</param>
        public Delta(Type entityType)
            : this(entityType, updatableProperties: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Delta{TEntityType}"/>.
        /// </summary>
        /// <param name="entityType">The derived entity type for which the changes would be tracked.
        /// <param name="updatableProperties">The set of properties that can be updated or reset.</param>
        /// <paramref name="entityType"/> should be assignable to instances of <typeparamref name="TEntityType"/>.</param>
        public Delta(Type entityType, IEnumerable<string> updatableProperties)
        {
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
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return this._allProperties.Keys;
        }

        /// <inheritdoc/>
        public override bool TrySetPropertyValue(string name, object value)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
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

            PropertyAccessor<TEntityType> cacheHit;
            if (_allProperties.TryGetValue(name, out cacheHit))
            {
                value = cacheHit.GetValue(_entity);
                return true;
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
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }

            PropertyAccessor<TEntityType> value;
            if (_allProperties.TryGetValue(name, out value))
            {
                type = value.Property.PropertyType;
                return true;
            }
            else
            {
                type = null;
                return false;
            }
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

        /// <inheritdoc/>
        public override IEnumerable<string> GetChangedPropertyNames()
        {
            return _changedProperties;
        }

        /// <inheritdoc/>
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
        }
    }
}
