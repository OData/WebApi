// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Web.Http.Data.EntityFramework.Metadata
{
    /// <summary>
    /// CustomTypeDescriptor base type shared by LINQ To SQL and LINQ To Entities
    /// </summary>
    internal abstract class TypeDescriptorBase : CustomTypeDescriptor
    {
        private PropertyDescriptorCollection _properties;

        /// <summary>
        /// Main constructor that accepts the parent custom type descriptor
        /// </summary>
        /// <param name="parent">The parent custom type descriptor.</param>
        public TypeDescriptorBase(ICustomTypeDescriptor parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Override of the <see cref="CustomTypeDescriptor.GetProperties()"/> to obtain the list
        /// of properties for this type.
        /// </summary>
        /// <remarks>
        /// This method is overridden so that it can merge this class's parent attributes with those
        /// it infers from the DAL-specific attributes.
        /// </remarks>
        /// <returns>A list of properties for this type</returns>
        public sealed override PropertyDescriptorCollection GetProperties()
        {
            // No need to lock anything... Worst case scenario we create the properties multiple times.
            if (_properties == null)
            {
                // Get properties from our parent
                PropertyDescriptorCollection originalCollection = base.GetProperties();

                bool customDescriptorsCreated = false;
                List<PropertyDescriptor> tempPropertyDescriptors = new List<PropertyDescriptor>();

                // for every property exposed by our parent, see if we have additional metadata to add
                foreach (PropertyDescriptor propDescriptor in originalCollection)
                {
                    Attribute[] newMetadata = GetMemberAttributes(propDescriptor).ToArray();
                    if (newMetadata.Length > 0)
                    {
                        tempPropertyDescriptors.Add(new MetadataPropertyDescriptorWrapper(propDescriptor, newMetadata));
                        customDescriptorsCreated = true;
                    }
                    else
                    {
                        tempPropertyDescriptors.Add(propDescriptor);
                    }
                }

                if (customDescriptorsCreated)
                {
                    _properties = new PropertyDescriptorCollection(tempPropertyDescriptors.ToArray(), true);
                }
                else
                {
                    _properties = originalCollection;
                }
            }

            return _properties;
        }

        /// <summary>
        /// Abstract method specific DAL implementations must override to return the
        /// list of RIA <see cref="Attribute"/>s implied by their DAL-specific attributes
        /// </summary>
        /// <param name="pd">A <see cref="PropertyDescriptor"/> to examine.</param>
        /// <returns>A list of RIA attributes implied by the DAL specific attributes</returns>
        protected abstract IEnumerable<Attribute> GetMemberAttributes(PropertyDescriptor pd);

        /// <summary>
        /// Returns <c>true</c> if the given type is a <see cref="Nullable"/>
        /// </summary>
        /// <param name="type">The type to test</param>
        /// <returns><c>true</c> if the given type is a nullable type</returns>
        public static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
