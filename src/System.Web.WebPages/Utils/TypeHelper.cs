// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web.Routing;

namespace System.Web.WebPages
{
    internal static class TypeHelper
    {
        /// <summary>
        /// Given an object of anonymous type, add each property as a key and associated with its value to a dictionary.
        /// </summary>
        internal static IDictionary<string, object> ObjectToDictionary(object value)
        {
            return new RouteValueDictionary(value);
        }

        /// <summary>
        /// Given an object of anonymous type, add each property as a key and associated with its value to the given dictionary.
        /// </summary>
        internal static void AddAnonymousObjectToDictionary(IDictionary<string, object> dictionary, object value)
        {
            var values = ObjectToDictionary(value);
            foreach (var item in values)
            {
                dictionary.Add(item);
            }
        }

        /// <remarks>This code is copied from http://www.liensberger.it/web/blog/?p=191 </remarks>
        internal static bool IsAnonymousType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            // TODO: The only way to detect anonymous types right now.
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                   && type.IsGenericType && type.Name.Contains("AnonymousType")
                   && (type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase) || type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase))
                   && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }
    }
}
