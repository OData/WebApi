// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using Microsoft.Data.OData.Query;

namespace System.Web.Http.OData.Builder.Conventions
{
    internal static class ConventionsHelpers
    {
        public static bool IsEntityType(Type type)
        {
            return GetKeyProperty(type) != null;
        }

        public static PropertyInfo GetKeyProperty(Type entityType, bool throwOnError = false)
        {
            var keys = entityType.GetProperties()
                .Where(p => (p.Name.Equals(entityType.Name + "Id", StringComparison.OrdinalIgnoreCase) || p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                && EdmLibHelpers.GetEdmPrimitiveTypeOrNull(p.PropertyType) != null);

            if (keys.Count() == 0)
            {
                if (throwOnError)
                {
                    throw Error.InvalidOperation(SRResources.NoKeyFound, entityType.FullName);
                }
            }
            else if (keys.Count() > 1)
            {
                if (throwOnError)
                {
                    throw Error.InvalidOperation(SRResources.MultipleKeysFound, entityType.FullName);
                }
            }
            else
            {
                return keys.Single();
            }

            return null;
        }

        public static string GetEntityKeyValue(EntityInstanceContext entityContext, IEntityTypeConfiguration entityTypeConfiguration)
        {
            // TODO: BUG 453795: reflection cleanup
            if (entityTypeConfiguration.Keys.Count() == 1)
            {
                return GetUriRepresentationForKeyValue(entityTypeConfiguration.Keys.First().PropertyInfo, entityContext.EntityInstance);
            }
            else
            {
                return String.Join(
                    ",",
                    entityTypeConfiguration
                        .Keys
                        .Select(
                            key => String.Format(CultureInfo.InvariantCulture, "{0}={1}", key.Name, GetUriRepresentationForKeyValue(key.PropertyInfo, entityContext.EntityInstance))));
            }
        }

        public static PropertyInfo[] GetProperties(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            return type
                .GetProperties()
                .Where(p => p.IsValidStructuralProperty())
                .ToArray();
        }

        public static bool IsValidStructuralProperty(this PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            if (propertyInfo.CanRead && (propertyInfo.CanWrite || propertyInfo.PropertyType.IsCollection()))
            {
                // non-public getters are not valid properties
                MethodInfo publicGetter = propertyInfo.GetGetMethod();
                if (publicGetter != null && !publicGetter.IsAbstract && propertyInfo.PropertyType.IsValidStructuralPropertyType())
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsValidStructuralPropertyType(this Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            return !(type.IsGenericTypeDefinition
                     || type.IsNested
                     || type.IsPointer
                     || type == typeof(object));
        }

        public static bool IsCollection(this Type type)
        {
            return type.IsCollection(out type);
        }

        public static bool IsCollection(this Type type, out Type elementType)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            elementType = type;

            var collectionInterface
                = type.GetInterfaces()
                    .Union(new[] { type })
                    .FirstOrDefault(
                        t => t.IsGenericType
                             && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (collectionInterface != null)
            {
                elementType = collectionInterface.GetGenericArguments().Single();

                return true;
            }

            return false;
        }

        private static string GetUriRepresentationForKeyValue(PropertyInfo key, object entityInstance)
        {
            Contract.Assert(key != null);
            Contract.Assert(entityInstance != null);

            return ODataUriBuilder.GetUriRepresentation(key.GetValue(entityInstance, null));
        }
    }
}
