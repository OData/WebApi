// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Serialization;
using System.Web.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder.Conventions
{
    internal static class ConventionsHelpers
    {
        public static string GetEntityKeyValue(EntityInstanceContext entityContext)
        {
            Contract.Assert(entityContext != null);
            Contract.Assert(entityContext.EntityType != null);
            Contract.Assert(entityContext.EdmObject != null);

            IEnumerable<IEdmProperty> keys = entityContext.EntityType.Key();

            // TODO: BUG 453795: reflection cleanup
            if (keys.Count() == 1)
            {
                return GetUriRepresentationForKeyValue(keys.First(), entityContext);
            }
            else
            {
                IEnumerable<string> keyValues =
                    keys.Select(key => String.Format(
                        CultureInfo.InvariantCulture, "{0}={1}", key.Name, GetUriRepresentationForKeyValue(key, entityContext)));
                return String.Join(",", keyValues);
            }
        }

        // Get properties of this entity type that are not already declared in the base entity type and are not already ignored.
        public static IEnumerable<PropertyInfo> GetProperties(EntityTypeConfiguration entity, bool includeReadOnly)
        {
            IEnumerable<PropertyInfo> allProperties = GetAllProperties(entity as StructuralTypeConfiguration, includeReadOnly);
            if (entity.BaseType != null)
            {
                IEnumerable<PropertyInfo> baseTypeProperties = GetAllProperties(entity.BaseType as StructuralTypeConfiguration, includeReadOnly);
                return allProperties.Except(baseTypeProperties, PropertyEqualityComparer.Instance);
            }
            else
            {
                return allProperties;
            }
        }

        // Get all properties of this type (that are not already ignored).
        public static IEnumerable<PropertyInfo> GetAllProperties(StructuralTypeConfiguration type, bool includeReadOnly)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            return type
                .ClrType
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.IsValidStructuralProperty() && !type.IgnoredProperties().Any(p1 => p1.Name == p.Name)
                    && (includeReadOnly || p.GetSetMethod() != null || p.PropertyType.IsCollection()));
        }

        public static bool IsValidStructuralProperty(this PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            // ignore any indexer properties.
            if (propertyInfo.GetIndexParameters().Any())
            {
                return false;
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

        // Gets the ignored properties from this type and the base types.
        public static IEnumerable<PropertyInfo> IgnoredProperties(this StructuralTypeConfiguration structuralType)
        {
            if (structuralType == null)
            {
                return Enumerable.Empty<PropertyInfo>();
            }

            EntityTypeConfiguration entityType = structuralType as EntityTypeConfiguration;
            if (entityType != null)
            {
                return entityType.IgnoredProperties.Concat(entityType.BaseType.IgnoredProperties());
            }
            else
            {
                return structuralType.IgnoredProperties;
            }
        }

        public static bool IsValidStructuralPropertyType(this Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            Type elementType;

            return !(type.IsGenericTypeDefinition
                     || type.IsPointer
                     || type == typeof(object)
                     || (type.IsCollection(out elementType) && elementType == typeof(object)));
        }

        // gets the primitive odata uri representation.
        public static string GetUriRepresentationForValue(object value)
        {
            Contract.Assert(value != null);

            Type type = value.GetType();
            if (type.IsEnum)
            {
                value = new ODataEnumValue(value.ToString(), type.EdmFullName());
            }
            else
            {
                Contract.Assert(EdmLibHelpers.GetEdmPrimitiveTypeOrNull(type) != null);
                value = ODataPrimitiveSerializer.ConvertUnsupportedPrimitives(value);
            }

            return ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V4);
        }

        private static string GetUriRepresentationForKeyValue(IEdmProperty key, EntityInstanceContext entityInstanceContext)
        {
            Contract.Assert(key != null);
            Contract.Assert(entityInstanceContext != null);

            object value = entityInstanceContext.GetPropertyValue(key.Name);
            if (value == null)
            {
                IEdmTypeReference edmType = entityInstanceContext.EdmObject.GetEdmType();
                throw Error.InvalidOperation(SRResources.KeyValueCannotBeNull, key.Name, edmType.Definition);
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
