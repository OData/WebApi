// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Web.Http.Data
{
    /// <summary>
    /// Extension methods for TypeDescriptors
    /// </summary>
    internal static class TypeDescriptorExtensions
    {
        /// <summary>
        /// Extension method to extract only the explicitly specified attributes from a <see cref="PropertyDescriptor"/>.
        /// </summary>
        /// <remarks>
        /// Normal TypeDescriptor semantics are to inherit the attributes of a property's type.  This method
        /// exists to suppress those inherited attributes.
        /// </remarks>
        /// <param name="propertyDescriptor">The property descriptor whose attributes are needed.</param>
        /// <returns>A new <see cref="AttributeCollection"/> stripped of any attributes from the property's type.</returns>
        public static AttributeCollection ExplicitAttributes(this PropertyDescriptor propertyDescriptor)
        {
            List<Attribute> attributes = new List<Attribute>(propertyDescriptor.Attributes.Cast<Attribute>());
            AttributeCollection typeAttributes = TypeDescriptor.GetAttributes(propertyDescriptor.PropertyType);
            bool removedAttribute = false;
            foreach (Attribute attr in typeAttributes)
            {
                for (int i = attributes.Count - 1; i >= 0; --i)
                {
                    // We must use ReferenceEquals since attributes could Match if they are the same.
                    // Only ReferenceEquals will catch actual duplications.
                    if (Object.ReferenceEquals(attr, attributes[i]))
                    {
                        attributes.RemoveAt(i);
                        removedAttribute = true;
                    }
                }
            }
            return removedAttribute ? new AttributeCollection(attributes.ToArray()) : propertyDescriptor.Attributes;
        }

        /// <summary>
        /// Extension method to extract attributes from a type taking into account the inheritance type of attributes
        /// </summary>
        /// <remarks>
        /// Normal TypeDescriptor semantics are to inherit the attributes of a type's base type, regardless of their 
        /// inheritance type.
        /// </remarks>
        /// <param name="type">The type whose attributes are needed.</param>
        /// <returns>A new <see cref="AttributeCollection"/> stripped of any incorrectly inherited attributes from the type.</returns>
        public static AttributeCollection Attributes(this Type type)
        {
            AttributeCollection baseTypeAttributes = TypeDescriptor.GetAttributes(type.BaseType);
            List<Attribute> typeAttributes = new List<Attribute>(TypeDescriptor.GetAttributes(type).Cast<Attribute>());
            foreach (Attribute attr in baseTypeAttributes)
            {
                AttributeUsageAttribute attributeUsageAtt = (AttributeUsageAttribute)TypeDescriptor.GetAttributes(attr)[typeof(AttributeUsageAttribute)];
                if (attributeUsageAtt != null && !attributeUsageAtt.Inherited)
                {
                    for (int i = typeAttributes.Count - 1; i >= 0; --i)
                    {
                        // We must use ReferenceEquals since attributes could Match if they are the same.
                        // Only ReferenceEquals will catch actual duplications.
                        if (Object.ReferenceEquals(attr, typeAttributes[i]))
                        {
                            typeAttributes.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            return new AttributeCollection(typeAttributes.ToArray());
        }

        /// <summary>
        /// Checks to see if an attribute collection contains any attributes of the provided type.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute type to check for</typeparam>
        /// <param name="attributes">The attribute collection to inspect</param>
        /// <returns><c>True</c> if an attribute of the provided type is contained in the attribute collection.</returns>
        public static bool ContainsAttributeType<TAttribute>(this AttributeCollection attributes) where TAttribute : Attribute
        {
            return attributes.Cast<Attribute>().Any(a => a.GetType() == typeof(TAttribute));
        }
    }
}
