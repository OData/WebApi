// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.Web.WebPages
{
    internal static class AnonymousObjectReflectionHelper
    {
        public static PropertyHelper[] GetProperties<T>(object instance,
                                                          ConcurrentDictionary<Type, PropertyHelper[]> cache)
            where T : PropertyHelper, new()
        {
            // Using an array rather than IEnumerable, as this will be called on the hot path numerous times.
            PropertyHelper[] helpers;

            Type type = instance.GetType();

            if (!cache.TryGetValue(type, out helpers))
            {
                // We avoid loading indexed properties using the where statement.
                // Indexed properties are not useful (or valid) for grabbing properties off an anonymous object.
                IEnumerable<PropertyInfo> properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                                                           .Where(prop => prop.GetIndexParameters().Length == 0 &&
                                                                          prop.GetMethod != null);

                var newHelpers = new List<PropertyHelper>();

                foreach (PropertyInfo property in properties)
                {
                    var propertyHelper = new T();
                    propertyHelper.Initialize(property);

                    newHelpers.Add(propertyHelper);
                }

                helpers = newHelpers.ToArray();
                cache.TryAdd(type, helpers);
            }

            return helpers;
        }
    }
}
