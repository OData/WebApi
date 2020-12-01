// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see cref="IEdmObject"/> that is a collection of <see cref="IEdmChangedObject"/>s.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmChangedObjectCollection<TStructuralType> : EdmChangedObjectCollection, ICollection<IEdmChangedObject<TStructuralType>>, IEdmObject
    {
        private ICollection<IEdmChangedObject<TStructuralType>> _items;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmChangedObjectCollection"/> class.
        /// </summary>
        /// <param name="entityType">The Edm entity type of the collection.</param>
        public EdmChangedObjectCollection(IEdmEntityType entityType)
            : base(entityType)
        {
            _items = new Collection<IEdmChangedObject<TStructuralType>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmChangedObjectCollection"/> class.
        /// </summary>
        /// <param name="entityType">The Edm type of the collection.</param>
        /// <param name="changedObjectList">The list that is wrapped by the new collection.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public EdmChangedObjectCollection(IEdmEntityType entityType, IList<IEdmChangedObject<TStructuralType>> changedObjectList)
            : base(entityType, changedObjectList as IList<IEdmChangedObject>)
        {            
            _items = changedObjectList;
        }

        /// <inheritdoc/>
        public void Add(IEdmChangedObject<TStructuralType> item)
        {
            _items.Add(item);
        }

       
        /// <inheritdoc/>
        public bool Contains(IEdmChangedObject<TStructuralType> item)
        {
            return _items.Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(IEdmChangedObject<TStructuralType>[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public bool Remove(IEdmChangedObject<TStructuralType> item)
        {
            return _items.Remove(item);
        }

        /// <inheritdoc/>
        IEnumerator<IEdmChangedObject<TStructuralType>> IEnumerable<IEdmChangedObject<TStructuralType>>.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Copy changed values is an implementation of Patch
        /// </summary>
        /// <param name="original"></param>
        /// <param name="changedObjCollection"></param>        
        public void CopyChangedValues(ICollection<TStructuralType> original, EdmChangedObjectCollection changedObjCollection)
        {
            foreach(dynamic changedObj in changedObjCollection)
            {
                object Id;
                IEdmDeltaDeletedEntityObject deletedObj = changedObj as IEdmDeltaDeletedEntityObject;

                if(deletedObj != null)
                {
                    TStructuralType originalObj = original.FirstOrDefault(x => x.GetType().GetProperty("Id").GetValue(x).ToString() == deletedObj.Id);

                    if (originalObj != null)
                    {
                        original.Remove(originalObj);
                    }
                }
                else
                {
                    string key = GetKeyProperty(changedObj.ExpectedClrType.GetProperties(), changedObj.ExpectedClrType.Name);
                    changedObj.TryGetPropertyValue(key, out Id);
                    TStructuralType originalObj = original.FirstOrDefault(x => x.GetType().GetProperty("Id").GetValue(x).ToString() == Id.ToString());

                    if (originalObj == null)
                    {
                        originalObj = Activator.CreateInstance(changedObj.ExpectedClrType);
                        original.Add(originalObj);
                    }

                    changedObj.Patch(originalObj);
                }               
            }
        }

        /// <summary>
        /// Patch for EdmChangedobjectCollection, a collection for Delta<typeparamref name="TStructuralType"/>
        /// </summary>
        /// <param name="original"></param>        
        public void Patch(ICollection<TStructuralType> original)
        {
            CopyChangedValues(original, this);
        }

        private static string GetKeyProperty(PropertyInfo[] allProperties, string entityName)
        {
            var keys =
               allProperties
               .Where(p => (p.Name.Equals(entityName + "Id", StringComparison.OrdinalIgnoreCase) || p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
               && (EdmLibHelpers.GetEdmPrimitiveTypeOrNull(p.PropertyType) != null || TypeHelper.IsEnum(p.PropertyType)));

            if (keys.Count() == 1)
            {
                return keys.Single().Name;
            }

            return null;
        }
    }
}