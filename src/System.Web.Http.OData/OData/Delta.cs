// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace System.Web.Http.OData
{
    /// <summary>
    /// A class the tracks changes (i.e. the Delta) for a particular TEntityType
    /// </summary>
    /// <typeparam name="TEntityType">TEntityType is the type of entity this delta tracks changes for.</typeparam>
    /// <remarks>The <typeparamref name="TEntityType"/> must have a public no argument constructor so the Delta can 
    /// construct a new instance when required.</remarks>
    public class Delta<TEntityType> : DynamicObject, IDelta<TEntityType> where TEntityType : class, new()
    {
        private static Dictionary<string, PropertyAccessor<TEntityType>> _propertiesThatExist = InitializePropertiesThatExist();

        private HashSet<string> _changedProperties = new HashSet<string>();
        private TEntityType _entity = new TEntityType();

        /// <summary>
        /// Clears the Delta and resets the underlying Entity.
        /// </summary>
        public void Clear()
        {
            _entity = new TEntityType();
            _changedProperties.Clear();
        }

        /// <summary>
        /// Attempts to set the Property called <paramref name="name"/> to the <paramref name="value"/> specified.
        /// <remarks>
        /// Only properties that exist on <typeparamref name="TEntityType"/> can be set.
        /// If there is a type mismatch the request will fail.
        /// </remarks>
        /// </summary>
        /// <param name="name">The name of the Property</param>
        /// <param name="value">The new value of the Property</param>
        /// <returns>True if successful</returns>
        public bool TrySetPropertyValue(string name, object value)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }

            if (!_propertiesThatExist.ContainsKey(name))
            {
                return false;
            }

            PropertyAccessor<TEntityType> cacheHit = _propertiesThatExist[name];
            Type valueType = value != null ? value.GetType() : null;
            if (cacheHit.Property.PropertyType != valueType)
            {
                if (!(cacheHit.Property.PropertyType.IsClass && value == null))
                {
                    return false;
                }
            }

            //.Setter.Invoke(_entity, new object[] { value });
            cacheHit.SetValue(_entity, value);
            _changedProperties.Add(name);
            return true;
        }

        /// <summary>
        /// Attempts to get the value of the Property called <paramref name="name"/> from the underlying Entity.
        /// <remarks>
        /// Only properties that exist on <typeparamref name="TEntityType"/> can be retrieved.
        /// Both modified and unmodified properties can be retrieved.
        /// </remarks>
        /// </summary>
        /// <param name="name">The name of the Property</param>
        /// <param name="value">The value of the Property</param>
        /// <returns>True if the Property was found</returns>
        public bool TryGetPropertyValue(string name, out object value)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }

            if (_propertiesThatExist.ContainsKey(name))
            {
                PropertyAccessor<TEntityType> cacheHit = _propertiesThatExist[name];
                value = cacheHit.GetValue(_entity);
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to get the <see cref="Type"/> of the Property called <paramref name="name"/> from the underlying Entity.
        /// <remarks>
        /// Only properties that exist on <typeparamref name="TEntityType"/> can be retrieved.
        /// Both modified and unmodified properties can be retrieved.
        /// </remarks>
        /// </summary>
        /// <param name="name">The name of the Property</param>
        /// <param name="type">The type of the Property</param>
        /// <returns>Returns <c>true</c> if the Property was found and <c>false</c> if not.</returns>
        public bool TryGetPropertyType(string name, out Type type)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }

            PropertyAccessor<TEntityType> value;
            if (_propertiesThatExist.TryGetValue(name, out value))
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
        /// Overrides the DynamicObject TrySetMember method, so that only the properties
        /// of <typeparamref name="TEntityType"/> can be set.
        /// </summary>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder == null)
            {
                throw Error.ArgumentNull("binder");
            }

            return TrySetPropertyValue(binder.Name, value);
        }

        /// <summary>
        /// Overrides the DynamicObject TryGetMember method, so that only the properties
        /// of <typeparamref name="TEntityType"/> can be got.
        /// </summary>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder == null)
            {
                throw Error.ArgumentNull("binder");
            }

            return TryGetPropertyValue(binder.Name, out result);
        }

        /// <summary>
        /// Returns the <typeparamref name="TEntityType"/> instance
        /// that holds all the changes (and original values) being tracked by this Delta.
        /// </summary>
        public TEntityType GetEntity()
        {
            return _entity;
        }

        /// <summary>
        /// Returns the Properties that have been modified through this Delta as an 
        /// enumeration of Property Names 
        /// </summary>
        public IEnumerable<string> GetChangedPropertyNames()
        {
            return _changedProperties;
        }

        /// <summary>
        /// Returns the Properties that have not been modified through this Delta as an 
        /// enumeration of Property Names 
        /// </summary>
        public IEnumerable<string> GetUnchangedPropertyNames()
        {
            return _propertiesThatExist.Keys.Except(GetChangedPropertyNames());
        }

        /// <summary>
        /// Copies the changed property values from the underlying entity (accessible via GetEntity()) 
        /// to the <paramref name="original"/> entity.
        /// </summary>
        /// <param name="original">The entity to be updated.</param>
        public void CopyChangedValues(TEntityType original)
        {
            if (original == null)
            {
                throw Error.ArgumentNull("original");
            }
            PropertyAccessor<TEntityType>[] propertiesToCopy = GetChangedPropertyNames().Select(s => _propertiesThatExist[s]).ToArray();
            foreach (PropertyAccessor<TEntityType> propertyToCopy in propertiesToCopy)
            {
                propertyToCopy.Copy(_entity, original);
            }
        }

        /// <summary>
        /// Copies the unchanged property values from the underlying entity (accessible via GetEntity()) 
        /// to the <paramref name="original"/> entity.
        /// </summary>
        /// <param name="original">The entity to be updated.</param>
        public void CopyUnchangedValues(TEntityType original)
        {
            if (original == null)
            {
                throw Error.ArgumentNull("original");
            }

            PropertyAccessor<TEntityType>[] propertiesToCopy = GetUnchangedPropertyNames().Select(s => _propertiesThatExist[s]).ToArray();
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

        private static Dictionary<string, PropertyAccessor<TEntityType>> InitializePropertiesThatExist()
        {
            Type backingType = typeof(TEntityType);
            return backingType.GetProperties()
                                .Where(p => p.GetSetMethod() != null && p.GetGetMethod() != null)
                                .Select<PropertyInfo, PropertyAccessor<TEntityType>>(p => new CompiledPropertyAccessor<TEntityType>(p))
                                .ToDictionary(p => p.Property.Name);
        }
    }
}
