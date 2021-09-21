//-----------------------------------------------------------------------------
// <copyright file="DeltaOfTStructuralType.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Builder.Conventions.Attributes;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// A class the tracks changes (i.e. the Delta) for a particular <typeparamref name="TStructuralType"/>.
    /// </summary>
    /// <typeparam name="TStructuralType">TStructuralType is the type of the instance this delta tracks changes for.</typeparam>
    [NonValidatingParameterBinding]
    public class Delta<TStructuralType> : TypedDelta, IDelta, IDeltaSetItem where TStructuralType : class
    {
        // cache property accessors for this type and all its derived types.
        private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyAccessor<TStructuralType>>> _propertyCache
            = new ConcurrentDictionary<Type, Dictionary<string, PropertyAccessor<TStructuralType>>>();

        private Dictionary<string, PropertyAccessor<TStructuralType>> _allProperties;
        private List<string> _updatableProperties;

        private HashSet<string> _changedProperties;

        // Nested resources or structures changed at this level.
        private IDictionary<string, object> _deltaNestedResources;

        private TStructuralType _instance;
        private Type _structuredType;

        private readonly PropertyInfo _dynamicDictionaryPropertyinfo;
        private PropertyInfo _instanceAnnotationsPropertyInfo;
        private HashSet<string> _changedDynamicProperties;
        private IDictionary<string, object> _dynamicDictionaryCache;
        private NavigationPath _navigationPath;
        
        /// <summary>
        /// Initializes a new instance of <see cref="Delta{TStructuralType}"/>.
        /// </summary>
        public Delta()
            : this(typeof(TStructuralType))
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Delta{TStructuralType}"/>.
        /// </summary>
        /// <param name="structuralType">The derived entity type or complex type for which the changes would be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="TStructuralType"/>.
        /// </param>
        public Delta(Type structuralType)
            : this(structuralType, updatableProperties: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Delta{TStructuralType}"/>.
        /// </summary>
        /// <param name="structuralType">The derived entity type or complex type for which the changes would be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="TStructuralType"/>.
        /// </param>
        /// <param name="updatableProperties">The set of properties that can be updated or reset. Unknown property
        /// names, including those of dynamic properties, are ignored.</param>
        public Delta(Type structuralType, IEnumerable<string> updatableProperties)
            : this(structuralType, updatableProperties: updatableProperties, dynamicDictionaryPropertyInfo: null, instanceAnnotationsPropertyInfo:null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Delta{TStructuralType}"/>.
        /// </summary>
        /// <param name="structuralType">The derived entity type or complex type for which the changes would be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="TStructuralType"/>.
        /// </param>
        /// <param name="updatableProperties">The set of properties that can be updated or reset. Unknown property
        /// names, including those of dynamic properties, are ignored.</param>
        /// <param name="dynamicDictionaryPropertyInfo">The property info that is used as dictionary of dynamic
        /// properties. <c>null</c> means this entity type is not open.</param>                
        public Delta(Type structuralType, IEnumerable<string> updatableProperties, PropertyInfo dynamicDictionaryPropertyInfo)
            : this(structuralType, updatableProperties: updatableProperties, dynamicDictionaryPropertyInfo, instanceAnnotationsPropertyInfo: null)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Delta{TStructuralType}"/>.
        /// </summary>
        /// <param name="structuralType">The derived entity type or complex type for which the changes would be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="TStructuralType"/>.
        /// </param>
        /// <param name="updatableProperties">The set of properties that can be updated or reset. Unknown property
        /// names, including those of dynamic properties, are ignored.</param>
        /// <param name="dynamicDictionaryPropertyInfo">The property info that is used as dictionary of dynamic
        /// properties. <c>null</c> means this entity type is not open.</param>        
        /// <param name="instanceAnnotationsPropertyInfo">The property info that is used as container for Instance Annotations</param>
        public Delta(Type structuralType, IEnumerable<string> updatableProperties,
            PropertyInfo dynamicDictionaryPropertyInfo, PropertyInfo instanceAnnotationsPropertyInfo)
        {
            _dynamicDictionaryPropertyinfo = dynamicDictionaryPropertyInfo;
            Reset(structuralType);
            InitializeProperties(updatableProperties);            
            TransientInstanceAnnotationContainer = new ODataInstanceAnnotationContainer();            
            _instanceAnnotationsPropertyInfo = instanceAnnotationsPropertyInfo;
            _navigationPath = new NavigationPath(structuralType.Name, null);
            DeltaKind = EdmDeltaEntityKind.Entry;
        }

        /// <inheritdoc/>
        public override Type StructuredType
            => _structuredType;

        internal IDictionary<string, object> DeltaNestedResources
        {
            get { return _deltaNestedResources; }
        }

        /// <inheritdoc/>
        public override Type ExpectedClrType
            => typeof(TStructuralType);

        /// <summary>
        /// The list of property names that can be updated.
        /// </summary>
        /// <remarks>When the list is modified, any modified properties that were removed from the list are no longer
        /// considered to be changed.</remarks>
        public IList<string> UpdatableProperties
            => _updatableProperties;

        /// <summary>
        /// Gets the enum type of <see cref="EdmDeltaEntityKind"/>.
        /// </summary>
        public EdmDeltaEntityKind DeltaKind { get; protected set; }

        /// <inheritdoc />
        public IODataInstanceAnnotationContainer TransientInstanceAnnotationContainer { get; set; }

        /// <inheritdoc />
        public IODataIdContainer ODataIdContainer { get; set; }

        /// <inheritdoc />
        internal PropertyInfo InstanceAnnotationsPropertyInfo { get { return _instanceAnnotationsPropertyInfo; } }

        /// <inheritdoc/>
        public override void Clear()
        {
            Reset(_structuredType);
        }

        /// <inheritdoc/>
        public override bool TrySetPropertyValue(string name, object value)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw Error.ArgumentNull("name");
            }

            if (_instanceAnnotationsPropertyInfo != null && name == _instanceAnnotationsPropertyInfo.Name)
            {                
                IODataInstanceAnnotationContainer annotationValue = value as IODataInstanceAnnotationContainer;
                if (value != null && annotationValue == null)
                {
                    return false;
                }

                _instanceAnnotationsPropertyInfo.SetValue(_instance, annotationValue);

                return true;                
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
                            GetDynamicPropertyDictionary(_dynamicDictionaryPropertyinfo, _instance, create: true);
                    }

                    _dynamicDictionaryCache[name] = value;
                    _changedDynamicProperties.Add(name);
                    return true;
                }
            }

            if (value is IDelta || value is IDeltaSet)
            {
                return TrySetNestedResourceInternal(name, value);
            }
            else
            {
                return TrySetPropertyValueInternal(name, value);
            }
        }

        /// <inheritdoc/>
        public override bool TryGetPropertyValue(string name, out object value)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }

            if (_instanceAnnotationsPropertyInfo != null && name == _instanceAnnotationsPropertyInfo.Name)
            {                
                object propertyValue = _instanceAnnotationsPropertyInfo.GetValue(_instance);
                if (propertyValue != null)
                {
                    value =  (IODataInstanceAnnotationContainer)propertyValue;
                    return true;
                }

                value = null;
                return false;                
            }

            if (_dynamicDictionaryPropertyinfo != null)
            {
                if (_dynamicDictionaryCache == null)
                {
                    _dynamicDictionaryCache =
                        GetDynamicPropertyDictionary(_dynamicDictionaryPropertyinfo, _instance, create: false);
                }

                if (_dynamicDictionaryCache != null && _dynamicDictionaryCache.TryGetValue(name, out value))
                {
                    return true;
                }
            }

            if (_deltaNestedResources.ContainsKey(name))
            {
                // If this is a nested resource, get the value from the dictionary of nested resources.
                object deltaNestedResource = _deltaNestedResources[name];

                Contract.Assert(deltaNestedResource != null, "deltaNestedResource != null");

                //If DeltaSet collection, we are handling delta collections so the value will be that itself and no need to get instance value
                if (deltaNestedResource is IDeltaSet)
                {
                    value = deltaNestedResource;
                    return true;
                }
                                
                Contract.Assert(IsDeltaOfT(deltaNestedResource.GetType()));

                // Get the Delta<{NestedResourceType}>._instance using Reflection.
                FieldInfo field = deltaNestedResource.GetType().GetField("_instance", BindingFlags.NonPublic | BindingFlags.Instance);
                Contract.Assert(field != null, "field != null");
                value = field.GetValue(deltaNestedResource);
                return true;
            }
            else
            {
                // try to retrieve the value of property.
                PropertyAccessor<TStructuralType> cacheHit;
                if (_allProperties.TryGetValue(name, out cacheHit))
                {
                    value = cacheHit.GetValue(_instance);
                    return true;
                }
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
                        GetDynamicPropertyDictionary(_dynamicDictionaryPropertyinfo, _instance, create: false);
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

            PropertyAccessor<TStructuralType> value;
            if (_allProperties.TryGetValue(name, out value))
            {
                type = value.Property.PropertyType;
                return true;
            }

            type = null;
            return false;
        }

        /// <summary>
        /// Returns the instance that holds all the changes (and original values) being tracked by this Delta.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not appropriate to be a property")]
        public TStructuralType GetInstance()
        {
            return _instance;
        }

        /// <summary>
        /// Returns the known properties that have been modified through this <see cref="Delta"/> as an
        /// <see cref="IEnumerable{T}" /> of property Names.
        /// Includes the structural properties at current level.
        /// Does not include the names of the changed dynamic properties.
        /// </summary>
        public override IEnumerable<string> GetChangedPropertyNames()
        {
            return _changedProperties.Intersect(_updatableProperties).Concat(_deltaNestedResources.Keys);
        }

        /// <summary>
        /// Returns the known properties that have not been modified through this <see cref="Delta"/> as an
        /// <see cref="IEnumerable{T}" /> of property Names. Does not include the names of the changed dynamic
        /// properties.
        /// </summary>
        public override IEnumerable<string> GetUnchangedPropertyNames()
        {
            // UpdatableProperties could include arbitrary strings, filter by _allProperties
            return _updatableProperties.Intersect(_allProperties.Keys).Except(GetChangedPropertyNames());
        }

        /// <summary>
        /// Copies the changed property values from the underlying entity (accessible via <see cref="GetInstance()" />)
        /// to the <paramref name="original"/> entity recursively.
        /// </summary>
        /// <param name="original">The entity to be updated.</param>
        public void CopyChangedValues(TStructuralType original)
        {
            CopyChangedValues(original, null);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        internal void CopyChangedValues(TStructuralType original, ODataAPIHandler<TStructuralType> apiHandler = null, ODataAPIHandlerFactory apiHandlerFactory = null)
        {
            if (original == null)
            {
                throw Error.ArgumentNull("original");
            }

            // Delta parameter type cannot be derived type of original
            // to prevent unrecognizable information from being applied to original resource.
            if (!_structuredType.IsAssignableFrom(original.GetType()))
            {
                throw Error.Argument("original", SRResources.DeltaTypeMismatch, _structuredType, original.GetType());
            }

            //To apply ODataId if its present
            if (apiHandlerFactory != null && ODataIdContainer?.ODataIdNavigationPath != null)
            {
                ApplyODataId(original, apiHandlerFactory);
            }

            RuntimeHelpers.EnsureSufficientExecutionStack();

            // For regular non-structural properties at current level.
            PropertyAccessor<TStructuralType>[] propertiesToCopy =
                _changedProperties.Intersect(_updatableProperties).Select(s => _allProperties[s]).ToArray();
            foreach (PropertyAccessor<TStructuralType> propertyToCopy in propertiesToCopy)
            {
                propertyToCopy.Copy(_instance, original);
            }

            CopyChangedDynamicValues(original);

            // For nested resources.
            foreach (string nestedResourceName in _deltaNestedResources.Keys)
            {
                // Patch for each nested resource changed under this TStructuralType.
                dynamic deltaNestedResource = _deltaNestedResources[nestedResourceName];
                dynamic originalNestedResource = null;

                if(deltaNestedResource is IDeltaSet)
                {
                    IODataAPIHandler apiHandlerNested = apiHandler.GetNestedHandler(original, nestedResourceName);

                    if (apiHandlerNested != null)
                    {
                        deltaNestedResource.CopyChangedValues(apiHandlerNested, apiHandlerFactory);
                    }
                }
                else
                {
                    if (!TryGetPropertyRef(original, nestedResourceName, out originalNestedResource))
                    {
                        throw Error.Argument(nestedResourceName, SRResources.DeltaNestedResourceNameNotFound,
                            nestedResourceName, original.GetType());
                    }

                    if (originalNestedResource == null)
                    {
                        // When patching original target of null value, directly set nested resource.
                        dynamic deltaObject = _deltaNestedResources[nestedResourceName];
                        dynamic instance = deltaObject.GetInstance();

                        // Recursively patch up the instance with the nested resources.
                        deltaObject.CopyChangedValues(instance);

                        _allProperties[nestedResourceName].SetValue(original, instance);
                    }
                    else
                    {
                        //Recursively patch the subtree.
                        bool isDeltaType = TypedDelta.IsDeltaOfT(deltaNestedResource.GetType());
                        Contract.Assert(isDeltaType, nestedResourceName + "'s corresponding value should be Delta<T> type but is not.");

                        deltaNestedResource.CopyChangedValues(originalNestedResource);
                    }
                }
              
            }
        }

        /// <summary>
        /// Copies the unchanged property values from the underlying entity (accessible via <see cref="GetInstance()" />)
        /// to the <paramref name="original"/> entity.
        /// </summary>
        /// <param name="original">The entity to be updated.</param>
        public void CopyUnchangedValues(TStructuralType original)
        {
            if (original == null)
            {
                throw Error.ArgumentNull("original");
            }

            if (!_structuredType.IsInstanceOfType(original))
            {
                throw Error.Argument("original", SRResources.DeltaTypeMismatch, _structuredType, original.GetType());
            }

            IEnumerable<PropertyAccessor<TStructuralType>> propertiesToCopy = GetUnchangedPropertyNames().Select(s => _allProperties[s]);
            foreach (PropertyAccessor<TStructuralType> propertyToCopy in propertiesToCopy)
            {
                propertyToCopy.Copy(_instance, original);
            }

            CopyUnchangedDynamicValues(original);
        }

        /// <summary>
        /// Overwrites the <paramref name="original"/> entity with the changes tracked by this Delta.
        /// <remarks>The semantics of this operation are equivalent to a HTTP PATCH operation, hence the name.</remarks>
        /// </summary>
        /// <param name="original">The entity to be updated.</param>
        public void Patch(TStructuralType original)
        {
            CopyChangedValues(original);
        }

        /// <summary>
        /// Overwrites the <paramref name="original"/> entity with the changes tracked by this Delta.
        /// <remarks>The semantics of this operation are equivalent to a HTTP PATCH operation, hence the name.</remarks>
        /// </summary>
        /// <param name="original">The entity to be updated.</param>        
        /// <param name="apiHandlerFactory">API Handler Factory</param>
        public void Patch(TStructuralType original, ODataAPIHandlerFactory apiHandlerFactory)
        {
            IODataAPIHandler apiHandler = apiHandlerFactory.GetHandler(_navigationPath);

            Debug.Assert(apiHandler != null);

            CopyChangedValues(original, apiHandler as ODataAPIHandler<TStructuralType>, apiHandlerFactory);            
        }

        /// <summary>
        /// This is basically Patch on ODataId. This applies ODataId parsed Navigation paths, get the value identified by that and copy it on original object
        /// </summary>    
        private void ApplyODataId(TStructuralType original, ODataAPIHandlerFactory apiHandlerFactory)
        {
            IODataAPIHandler refapiHandler = apiHandlerFactory.GetHandler(ODataIdContainer.ODataIdNavigationPath);

            if (refapiHandler != null)
            {
                ODataAPIHandler<TStructuralType> refapiHandlerOfT = refapiHandler as ODataAPIHandler<TStructuralType>;

                Debug.Assert(refapiHandlerOfT != null);

                TStructuralType referencedObj;
                string error;

                //Checking to get the referenced entity, get the properties and apply it on original object
                if (refapiHandlerOfT.TryGet(ODataIdContainer.ODataIdNavigationPath.GetNavigationPathItems().Last().KeyProperties, out referencedObj, out error) == ODataAPIResponseStatus.Success)
                {
                    foreach (string property in _updatableProperties)
                    {
                        PropertyInfo propertyInfo = _structuredType.GetProperty(property);

                        object value = propertyInfo.GetValue(referencedObj);
                        propertyInfo.SetValue(original, value);
                    }
                }
            }
        }

        /// <summary>
        /// Overwrites the <paramref name="original"/> entity with the values stored in this Delta.
        /// <remarks>The semantics of this operation are equivalent to a HTTP PUT operation, hence the name.</remarks>
        /// </summary>
        /// <param name="original">The entity to be updated.</param>
        public void Put(TStructuralType original)
        {
            CopyChangedValues(original);
            CopyUnchangedValues(original);
        }

        private static void CopyDynamicPropertyDictionary(IDictionary<string, object> source,
            IDictionary<string, object> dest, PropertyInfo dynamicPropertyInfo, TStructuralType targetEntity)
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
            TStructuralType entity, bool create)
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

        /// <summary>
        /// Attempts to get the property by the specified name.
        /// </summary>
        /// <param name="structural">The structural object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyRef">Output for property value.</param>
        /// <returns>true if the property is found; false otherwise.</returns>
        private static bool TryGetPropertyRef(TStructuralType structural, string propertyName,
            out dynamic propertyRef)
        {
            propertyRef = null;
            PropertyInfo propertyInfo = structural.GetType().GetProperty(propertyName);
            if (propertyInfo != null)
            {
                propertyRef = propertyInfo.GetValue(structural, null);
                return true;
            }

            return false;
        }

        private void Reset(Type structuralType)
        {
            if (structuralType == null)
            {
                throw Error.ArgumentNull("structuralType");
            }

            if (!typeof(TStructuralType).IsAssignableFrom(structuralType))
            {
                throw Error.InvalidOperation(SRResources.DeltaEntityTypeNotAssignable, structuralType, typeof(TStructuralType));
            }

            _instance = Activator.CreateInstance(structuralType) as TStructuralType;
            _changedProperties = new HashSet<string>();
            _deltaNestedResources = new Dictionary<string, object>();
            _structuredType = structuralType;

            _changedDynamicProperties = new HashSet<string>();
            _dynamicDictionaryCache = null;
        }

        private void InitializeProperties(IEnumerable<string> updatableProperties)
        {
            _allProperties = _propertyCache.GetOrAdd(
                _structuredType,
                (backingType) => backingType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => (p.GetSetMethod() != null || TypeHelper.IsCollection(p.PropertyType)) && p.GetGetMethod() != null)
                    .Select<PropertyInfo, PropertyAccessor<TStructuralType>>(p => new FastPropertyAccessor<TStructuralType>(p))
                    .ToDictionary(p => p.Property.Name));
     
            if (updatableProperties != null)
            {
                _updatableProperties = updatableProperties.Intersect(_allProperties.Keys).ToList();
            }
            else
            {
                _updatableProperties = new List<string>(_allProperties.Keys);
            }

            if (_dynamicDictionaryPropertyinfo != null)
            {
                _updatableProperties.Remove(_dynamicDictionaryPropertyinfo.Name);
            }
        }

        // Copy changed dynamic properties and leave the unchanged dynamic properties
        private void CopyChangedDynamicValues(TStructuralType targetEntity)
        {
            if (_dynamicDictionaryPropertyinfo == null)
            {
                return;
            }

            if (_dynamicDictionaryCache == null)
            {
                _dynamicDictionaryCache =
                    GetDynamicPropertyDictionary(_dynamicDictionaryPropertyinfo, _instance, create: false);
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

                // a dynamic property value equal to null, it means to remove this dynamic property
                if (dynamicPropertyValue == null)
                {
                    tempDictionary.Remove(dynamicPropertyName);
                }
                else
                {
                    if (dynamicPropertyValue is IDelta)
                    {
                        dynamic deltaObject = dynamicPropertyValue;
                        dynamic instance = deltaObject.GetInstance();

                        deltaObject.CopyChangedValues(instance);
                        tempDictionary[dynamicPropertyName] = instance;
                    }
                    else
                    {
                        tempDictionary[dynamicPropertyName] = dynamicPropertyValue;
                    }
                }
            }

            CopyDynamicPropertyDictionary(tempDictionary, toDictionary, _dynamicDictionaryPropertyinfo,
                targetEntity);
        }

        // Missing dynamic structural properties MUST be removed or set to null in *Put*
        private void CopyUnchangedDynamicValues(TStructuralType targetEntity)
        {
            if (_dynamicDictionaryPropertyinfo == null)
            {
                return;
            }

            if (_dynamicDictionaryCache == null)
            {
                _dynamicDictionaryCache =
                    GetDynamicPropertyDictionary(_dynamicDictionaryPropertyinfo, _instance, create: false);
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

        private bool TrySetPropertyValueInternal(string name, object value)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }

            if (!(_allProperties.ContainsKey(name) && _updatableProperties.Contains(name)))
            {
                return false;
            }

            PropertyAccessor<TStructuralType> cacheHit = _allProperties[name];

            if (value == null && !EdmLibHelpers.IsNullable(cacheHit.Property.PropertyType))
            {
                return false;
            }

            Type propertyType = cacheHit.Property.PropertyType;
            if (value != null && !TypeHelper.IsCollection(propertyType) && !propertyType.IsAssignableFrom(value.GetType()))
            {
                return false;
            }

            cacheHit.SetValue(_instance, value);
            _changedProperties.Add(name);
            return true;
        }

        private bool TrySetNestedResourceInternal(string name, object deltaNestedResource)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }

            if (!(_allProperties.ContainsKey(name) && _updatableProperties.Contains(name)))
            {
                return false;
            }

            if (_deltaNestedResources.ContainsKey(name))
            {
                // Ignore duplicated nested resource.
                return false;
            }

            //If Edmchangedobject collection, we are handling delta collections so the instance value need not be set,
            //as we consider the value as collection of Delta itself and not instance value of the field
            if (!(deltaNestedResource is IDeltaSet))
            {
                PropertyAccessor<TStructuralType> cacheHit = _allProperties[name];
                // Get the Delta<{NestedResourceType}>._instance using Reflection.
                FieldInfo field = deltaNestedResource.GetType().GetField("_instance", BindingFlags.NonPublic | BindingFlags.Instance);
                Contract.Assert(field != null, "field != null");
                cacheHit.SetValue(_instance, field.GetValue(deltaNestedResource));
            }

            // Add the nested resource in the hierarchy.
            // Note: We shouldn't add the structural properties to the <code>_changedProperties</code>, which
            // is used for keeping track of changed non-structural properties at current level.
            _deltaNestedResources[name] = deltaNestedResource;

            return true;
        }
    }
}