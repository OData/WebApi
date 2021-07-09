// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see cref="IDeltaDeletedEntityObject"/> with a backing CLR <see cref="Type"/>.
    /// Used to hold the Deleted Entry object in the Delta Feed Payload.
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
        /// Initializes a new instance of <see cref="DeltaDeletedEntityObject{TStructuralType}"/>.
        /// </summary>
        /// <param name="structuralType">The derived entity type or complex type for which the changes would be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="TStructuralType"/>.
        /// </param>
        public DeltaDeletedEntityObject(Type structuralType)
            : this(structuralType, dynamicDictionaryPropertyInfo: null, instanceAnnotationsPropertyInfo: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DeltaDeletedEntityObject{TStructuralType}"/>.
        /// </summary>
        /// <param name="structuralType">The derived entity type or complex type for which the changes would be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="TStructuralType"/>.
        /// </param>
        /// <param name="updatableProperties">Properties to update</param>
        public DeltaDeletedEntityObject(Type structuralType, IEnumerable<string> updatableProperties)
            : this(structuralType, updatableProperties, dynamicDictionaryPropertyInfo: null, instanceAnnotationsPropertyInfo: null)
        {

        }

        /// <summary>
        /// Initializes a new instance of <see cref="DeltaDeletedEntityObject{TStructuralType}"/>.
        /// </summary>
        /// <param name="structuralType">The derived entity type or complex type for which the changes would be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="TStructuralType"/>.
        /// </param>         
        /// <param name="instanceAnnotationsPropertyInfo">The property info that is used as container for Instance Annotations</param>
        public DeltaDeletedEntityObject(Type structuralType, PropertyInfo instanceAnnotationsPropertyInfo)
            : this(structuralType, dynamicDictionaryPropertyInfo: null, instanceAnnotationsPropertyInfo)
        {

        }

        /// <summary>
        /// Initializes a new instance of <see cref="DeltaDeletedEntityObject{TStructuralType}"/>.
        /// </summary>
        /// <param name="structuralType">The derived entity type or complex type for which the changes would be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="TStructuralType"/>.
        /// </param>
        /// <param name="dynamicDictionaryPropertyInfo">The property info that is used as dictionary of dynamic
        /// properties. <c>null</c> means this entity type is not open.</param>        
        /// <param name="instanceAnnotationsPropertyInfo">The property info that is used as container for Instance Annotations</param>
        public DeltaDeletedEntityObject(Type structuralType, PropertyInfo dynamicDictionaryPropertyInfo, PropertyInfo instanceAnnotationsPropertyInfo)
            : this(structuralType, updatableProperties: null , dynamicDictionaryPropertyInfo, instanceAnnotationsPropertyInfo)
        {
 
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DeltaDeletedEntityObject{TStructuralType}"/>.
        /// </summary>
        /// <param name="structuralType">The derived entity type or complex type for which the changes would be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="TStructuralType"/>.
        /// </param>
        /// <param name="updatableProperties"> Properties that can be updated</param>
        /// <param name="dynamicDictionaryPropertyInfo">The property info that is used as dictionary of dynamic
        /// properties. <c>null</c> means this entity type is not open.</param>
        /// <param name="instanceAnnotationsPropertyInfo">The property info that is used as container for Instance Annotations</param>
        public DeltaDeletedEntityObject(Type structuralType, IEnumerable<string> updatableProperties, PropertyInfo dynamicDictionaryPropertyInfo, PropertyInfo instanceAnnotationsPropertyInfo)
            : base(structuralType, updatableProperties, dynamicDictionaryPropertyInfo, instanceAnnotationsPropertyInfo)
        {
            DeltaKind = EdmDeltaEntityKind.DeletedEntry;
        }
     
        /// <inheritdoc />
        public Uri Id { get; set; }
        
        /// <inheritdoc />
        public DeltaDeletedEntryReason? Reason { get; set; }
        
        /// <inheritdoc />
        public IEdmNavigationSource NavigationSource { get; set; }  
    }
}