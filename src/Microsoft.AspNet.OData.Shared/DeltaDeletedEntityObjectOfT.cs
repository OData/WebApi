﻿//-----------------------------------------------------------------------------
// <copyright file="DeltaDeletedEntityObjectOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.OData;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see cref="IDeltaDeletedEntityObject"/> with a backing CLR type.
    /// Used to hold the deleted entry object in the delta feed payload.
    /// </summary>
    [NonValidatingParameterBinding]
    public class DeltaDeletedEntityObject<TStructuralType> : Delta<TStructuralType>, IDeltaDeletedEntityObject where TStructuralType : class
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DeltaDeletedEntityObject{TStructuralType}"/>.
        /// </summary>
        public DeltaDeletedEntityObject()
            : this(typeof(TStructuralType))
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DeltaDeletedEntityObject{TStructuralType}"/>
        /// </summary>
        /// <param name="structuralType">The derived entity type or complex type for which the changes will be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="TStructuralType"/>.
        /// </param>
        public DeltaDeletedEntityObject(Type structuralType)
            : this(structuralType, dynamicDictionaryPropertyInfo: null, instanceAnnotationsPropertyInfo: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DeltaDeletedEntityObject{TStructuralType}"/>.
        /// </summary>
        /// <param name="structuralType">The derived entity type or complex type for which the changes will be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="TStructuralType"/>.
        /// </param>
        /// <param name="updatableProperties">Properties that can be updated.</param>
        public DeltaDeletedEntityObject(Type structuralType, IEnumerable<string> updatableProperties)
            : this(structuralType, updatableProperties, dynamicDictionaryPropertyInfo: null, instanceAnnotationsPropertyInfo: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DeltaDeletedEntityObject{TStructuralType}"/>.
        /// </summary>
        /// <param name="structuralType">The derived entity type or complex type for which the changes will be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="TStructuralType"/>.
        /// </param>         
        /// <param name="instanceAnnotationsPropertyInfo">The property info that is used as container for instance annotations</param>
        public DeltaDeletedEntityObject(Type structuralType, PropertyInfo instanceAnnotationsPropertyInfo)
            : this(structuralType, dynamicDictionaryPropertyInfo: null, instanceAnnotationsPropertyInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DeltaDeletedEntityObject{TStructuralType}"/>.
        /// </summary>
        /// <param name="structuralType">The derived entity type or complex type for which the changes will be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="TStructuralType"/>.
        /// </param>
        /// <param name="dynamicDictionaryPropertyInfo">The property info that is used as dictionary of dynamic
        /// properties. <c>null</c> means this entity type is not open.</param>        
        /// <param name="instanceAnnotationsPropertyInfo">The property info that is used as container for instance annotations</param>
        public DeltaDeletedEntityObject(Type structuralType, PropertyInfo dynamicDictionaryPropertyInfo, PropertyInfo instanceAnnotationsPropertyInfo)
            : this(structuralType, updatableProperties: null, dynamicDictionaryPropertyInfo, instanceAnnotationsPropertyInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DeltaDeletedEntityObject{TStructuralType}"/>.
        /// </summary>
        /// <param name="structuralType">The derived entity type or complex type for which the changes will be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="TStructuralType"/>.
        /// </param>
        /// <param name="updatableProperties">Properties that can be updated.</param>
        /// <param name="dynamicDictionaryPropertyInfo">The property info that is used as dictionary of dynamic
        /// properties. <c>null</c> means this entity type is not open.</param>
        /// <param name="instanceAnnotationsPropertyInfo">The property info that is used as container for instance annotations</param>
        public DeltaDeletedEntityObject(Type structuralType, IEnumerable<string> updatableProperties, PropertyInfo dynamicDictionaryPropertyInfo, PropertyInfo instanceAnnotationsPropertyInfo)
            : this(structuralType, updatableProperties, dynamicDictionaryPropertyInfo, false, instanceAnnotationsPropertyInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DeltaDeletedEntityObject{TStructuralType}"/>.
        /// </summary>
        /// <param name="structuralType">The derived entity type or complex type for which the changes will be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="TStructuralType"/>.
        /// </param>
        /// <param name="updatableProperties">Properties that can be updated.</param>
        /// <param name="dynamicDictionaryPropertyInfo">The property info that is used as dictionary of dynamic
        /// properties. <c>null</c> means this entity type is not open.</param>
        /// <param name="isComplexType">To determine if the entity is a complex type</param>
        /// <param name="instanceAnnotationsPropertyInfo">The property info that is used as container for instance annotations</param>
        public DeltaDeletedEntityObject(Type structuralType, IEnumerable<string> updatableProperties, PropertyInfo dynamicDictionaryPropertyInfo, bool isComplexType, PropertyInfo instanceAnnotationsPropertyInfo)
            : base(structuralType, updatableProperties, dynamicDictionaryPropertyInfo, isComplexType, instanceAnnotationsPropertyInfo)
        {
            DeltaKind = EdmDeltaEntityKind.DeletedEntry;
        }

        /// <inheritdoc />
        public Uri Id { get; set; }

        /// <inheritdoc />
        public DeltaDeletedEntryReason? Reason { get; set; }

        /// <inheritdoc />
        public string NavigationSource { get; set; }
    }
}
