// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Formatter.Serialization;
using System.Web.Http.OData.Properties;
using Microsoft.Data.OData.Query;

namespace System.Web.Http.OData.Builder.Conventions
{
    internal static class ConventionsHelpers
    {
        private static HashSet<Type> _ignoredCollectionTypes = new HashSet<Type>(new Type[] { typeof(string) });

        public static string GetEntityKeyValue(EntityInstanceContext entityContext, IEntityTypeConfiguration entityTypeConfiguration)
        {
            // TODO: BUG 453795: reflection cleanup
            if (entityTypeConfiguration.Keys().Count() == 1)
            {
                return GetUriRepresentationForKeyValue(entityTypeConfiguration.Keys().First().PropertyInfo, entityContext.EntityInstance, entityTypeConfiguration);
            }
            else
            {
                return String.Join(
                    ",",
                    entityTypeConfiguration
                        .Keys()
                        .Select(
                            key => String.Format(CultureInfo.InvariantCulture, "{0}={1}", key.Name, GetUriRepresentationForKeyValue(key.PropertyInfo, entityContext.EntityInstance, entityTypeConfiguration))));
            }
        }

        // Get properties of this entity type that are not already declared in the base entity type and are not already ignored.
        public static IEnumerable<PropertyInfo> GetProperties(IEntityTypeConfiguration entity)
        {
            IEnumerable<PropertyInfo> allProperties = GetAllProperties(entity as IStructuralTypeConfiguration);
            if (entity.BaseType != null)
            {
                IEnumerable<PropertyInfo> baseTypeProperties = GetAllProperties(entity.BaseType as IStructuralTypeConfiguration);
                return allProperties.Except(baseTypeProperties, PropertyEqualityComparer.Instance);
            }
            else
            {
                return allProperties;
            }
        }

        // Get all properties of this type (that are not already ignored).
        public static IEnumerable<PropertyInfo> GetAllProperties(IStructuralTypeConfiguration type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            return type
                .ClrType
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.IsValidStructuralProperty() && !type.IgnoredProperties.Contains(p))
                .ToArray();
        }

        public static bool IsValidStructuralProperty(this PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            if (propertyInfo.CanRead)
            {
                // non-public getters are not valid properties
                MethodInfo publicGetter = propertyInfo.GetGetMethod();
                if (publicGetter != null && propertyInfo.PropertyType.IsValidStructuralPropertyType())
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

            // see if this type should be ignored.
            if (_ignoredCollectionTypes.Contains(type))
            {
                return false;
            }

            Type collectionInterface
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

        // gets the primitive odata uri representation.
        public static string GetUriRepresentationForValue(object value)
        {
            Contract.Assert(value != null);
            Contract.Assert(EdmLibHelpers.GetEdmPrimitiveTypeOrNull(value.GetType()) != null);

            value = ODataPrimitiveSerializer.ConvertUnsupportedPrimitives(value);
            return ODataUriBuilder.GetUriRepresentation(value);
        }

        private static string GetUriRepresentationForKeyValue(PropertyInfo key, object entityInstance, IEntityTypeConfiguration entityType)
        {
            Contract.Assert(key != null);
            Contract.Assert(entityInstance != null);
            Contract.Assert(entityType != null);

            object value = key.GetValue(entityInstance, null);

            if (value == null)
            {
                throw Error.InvalidOperation(SRResources.KeyValueCannotBeNull, key.Name, entityType.FullName);
            }

            return GetUriRepresentationForValue(value);
        }

        private class PropertyEqualityComparer : IEqualityComparer<PropertyInfo>
        {
            public static PropertyEqualityComparer Instance = new PropertyEqualityComparer();

            public bool Equals(PropertyInfo x, PropertyInfo y)
            {
                Contract.Assert(x != null);
                Contract.Assert(y != null);

                return x.Name == y.Name;
            }

            public int GetHashCode(PropertyInfo obj)
            {
                Contract.Assert(obj != null);
                return obj.Name.GetHashCode();
            }
        }
    }
}
