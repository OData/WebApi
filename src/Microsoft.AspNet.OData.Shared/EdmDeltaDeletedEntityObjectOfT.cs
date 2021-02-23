// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
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
    /// Represents an <see cref="IEdmDeltaDeletedEntityObject"/> with no backing CLR <see cref="Type"/>.
    /// Used to hold the Deleted Entry object in the Delta Feed Payload.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmDeltaDeletedEntityObject<TStructuralType> : EdmDeltaDeletedEntityObject, IEdmDeltaDeletedEntityObject<TStructuralType> where TStructuralType : class
    {

        private TStructuralType _instance;
        private PropertyInfo _instanceAnnotationsPropertyInfo;
        private IODataInstanceAnnotationContainer _instanceAnnotationCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaDeletedEntityObject"/> class.
        /// </summary>
        /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaDeletedEntityObject.</param>
        public EdmDeltaDeletedEntityObject(IEdmEntityType entityType)
            : this(entityType, isNullable: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaDeletedEntityObject"/> class.
        /// </summary>
        /// <param name="entityTypeReference">The <see cref="IEdmEntityTypeReference"/> of this DeltaDeletedEntityObject.</param>
        public EdmDeltaDeletedEntityObject(IEdmEntityTypeReference entityTypeReference)
            : this(entityTypeReference.EntityDefinition(), entityTypeReference.IsNullable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaDeletedEntityObject"/> class.
        /// </summary>
        /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaDeletedEntityObject.</param>
        /// <param name="isNullable">true if this object can be nullable; otherwise, false.</param>
        public EdmDeltaDeletedEntityObject(IEdmEntityType entityType, bool isNullable)
            : this(entityType, isNullable, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaDeletedEntityObject"/> class.
        /// </summary>
        /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaDeletedEntityObject.</param>        
        /// <param name="instanceAnnotationsPropertyInfo">Propertyinfo denoting Instanceannotaitoncontainer.</param>
        public EdmDeltaDeletedEntityObject(IEdmEntityType entityType, PropertyInfo instanceAnnotationsPropertyInfo)
            : this(entityType, false, instanceAnnotationsPropertyInfo)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaDeletedEntityObject"/> class.
        /// </summary>
        /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaDeletedEntityObject.</param>
        /// <param name="isNullable">true if this object can be nullable; otherwise, false.</param>
        /// <param name="instanceAnnotationsPropertyInfo">Propertyinfo denoting Instanceannotaitoncontainer.</param>
        public EdmDeltaDeletedEntityObject(IEdmEntityType entityType, bool isNullable, PropertyInfo instanceAnnotationsPropertyInfo)
            : base(entityType, isNullable)
        {
            _instanceAnnotationsPropertyInfo = instanceAnnotationsPropertyInfo;
            Reset();
        }

        internal PropertyInfo InstanceAnnotationsPropertyInfo { get { return _instanceAnnotationsPropertyInfo; } }

        private void Reset()
        {
            Type type = typeof(TStructuralType);
            _instance = Activator.CreateInstance(type) as TStructuralType;
        }

        ///<inheritdoc/>
        public override bool TrySetInstanceAnnotations(IODataInstanceAnnotationContainer value)
        {
            if (_instanceAnnotationsPropertyInfo != null)
            {
                if (_instanceAnnotationCache == null)
                {
                    _instanceAnnotationCache =
                        GetInstanceannotationContainer(_instanceAnnotationsPropertyInfo, _instance, value, create: true);
                }

                return true;
            }

            return false;
        }

        ///<inheritdoc/>      
        public override IODataInstanceAnnotationContainer TryGetInstanceAnnotations()
        {
            if (_instanceAnnotationsPropertyInfo != null)
            {
                if (_instanceAnnotationCache == null)
                {
                    _instanceAnnotationCache =
                        GetInstanceannotationContainer(_instanceAnnotationsPropertyInfo, _instance, null, create: true);
                }

                if (_instanceAnnotationCache != null)
                {
                    return _instanceAnnotationCache;
                }
            }

            return null;
        }

        private static IODataInstanceAnnotationContainer GetInstanceannotationContainer(PropertyInfo propertyInfo,
          TStructuralType entity, IODataInstanceAnnotationContainer value, bool create)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }

            object propertyValue = propertyInfo.GetValue(entity);
            if (propertyValue != null)
            {
                return (IODataInstanceAnnotationContainer)propertyValue;
            }

            if (create)
            {
                if (!propertyInfo.CanWrite)
                {
                    throw Error.InvalidOperation(SRResources.CannotSetAnnotationPropertyDictionary, propertyInfo.Name,
                            entity.GetType().FullName);
                }

                propertyInfo.SetValue(entity, value);
                return value;
            }

            return null;
        }
    }
}