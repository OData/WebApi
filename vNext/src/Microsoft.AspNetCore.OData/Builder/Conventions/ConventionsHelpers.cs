// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Formatter.Serialization;

namespace Microsoft.AspNetCore.OData.Builder.Conventions
{
    using Microsoft.AspNetCore.OData.Formatter;

    using TypeExtensions = Microsoft.AspNetCore.OData.Extensions.TypeExtensions;

    internal static class ConventionsHelpers
    {
        public static IEnumerable<KeyValuePair<string, object>> GetEntityKey(ResourceContext resourceContext)
        {
            Contract.Assert(resourceContext != null);
            Contract.Assert(resourceContext.StructuredType != null);
            Contract.Assert(resourceContext.EdmObject != null);

            IEdmEntityType entityType = resourceContext.StructuredType as IEdmEntityType;
            if (entityType == null)
            {
                return Enumerable.Empty<KeyValuePair<string, object>>();
            }

            IEnumerable<IEdmStructuralProperty> keys = entityType.Key();
            return keys.Select(k => new KeyValuePair<string, object>(k.Name, GetKeyValue(k, resourceContext)));
        }

        private static object GetKeyValue(IEdmProperty key, ResourceContext resourceContext)
        {
            Contract.Assert(key != null);
            Contract.Assert(resourceContext != null);

            object value = resourceContext.GetPropertyValue(key.Name);
            if (value == null)
            {
                IEdmTypeReference edmType = resourceContext.EdmObject.GetEdmType();
                throw Error.InvalidOperation(SRResources.KeyValueCannotBeNull, key.Name, edmType.Definition);
            }

            return ConvertValue(value);
        }

        public static object ConvertValue(object value)
        {
            Contract.Assert(value != null);

            Type type = value.GetType();
            if (type.GetTypeInfo().IsEnum)
            {
                value = new ODataEnumValue(value.ToString(), type.EdmFullName());
            }
            else
            {
                Contract.Assert(EdmLibHelpers.GetEdmPrimitiveTypeOrNull(type) != null);
                value = ODataPrimitiveSerializer.ConvertUnsupportedPrimitives(value);
            }

            return value;
        }

        public static string GetEntityKeyValue(ResourceContext resourceContext)
        {
            Contract.Assert(resourceContext != null);
            Contract.Assert(resourceContext.StructuredType != null);
            Contract.Assert(resourceContext.EdmObject != null);

            IEdmEntityType entityType = resourceContext.StructuredType as IEdmEntityType;
            if (entityType == null)
            {
                return String.Empty;
            }

            IEnumerable<IEdmProperty> keys = entityType.Key();
            if (keys.Count() == 1)
            {
                return GetUriRepresentationForKeyValue(keys.First(), resourceContext);
            }
            else
            {
                IEnumerable<string> keyValues =
                    keys.Select(key => String.Format(
                        CultureInfo.InvariantCulture, "{0}={1}", key.Name, GetUriRepresentationForKeyValue(key, resourceContext)));
                return String.Join(",", keyValues);
            }
        }

        // Get properties of this structural type that are not already declared in the base structural type and are not already ignored.
        public static IEnumerable<PropertyInfo> GetProperties(StructuralTypeConfiguration structural, bool includeReadOnly)
        {
            IEnumerable<PropertyInfo> allProperties = GetAllProperties(structural, includeReadOnly);
            if (structural.BaseTypeInternal != null)
            {
                IEnumerable<PropertyInfo> baseTypeProperties = GetAllProperties(structural.BaseTypeInternal, includeReadOnly);
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

            return !(type.GetTypeInfo().IsGenericTypeDefinition
                     || type.IsPointer
                     || type == typeof(object)
                     || (type.IsCollection(out elementType) && elementType == typeof(object)));
        }

        // gets the primitive odata uri representation.
        public static string GetUriRepresentationForValue(object value)
        {
            Contract.Assert(value != null);

            Type type = value.GetType();
            if (type.GetTypeInfo().IsEnum)
            {
                value = new ODataEnumValue(value.ToString(), TypeExtensions.EdmFullName(type));
            }
            else
            {
                Contract.Assert(EdmLibHelpers.GetEdmPrimitiveTypeOrNull(type) != null);
                value = ODataPrimitiveSerializer.ConvertUnsupportedPrimitives(value);
            }

            return ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V4);
        }

        private static string GetUriRepresentationForKeyValue(IEdmProperty key, ResourceContext resourceContext)
        {
            Contract.Assert(key != null);
            Contract.Assert(resourceContext != null);

            object value = resourceContext.GetPropertyValue(key.Name);
            if (value == null)
            {
                IEdmTypeReference edmType = resourceContext.EdmObject.GetEdmType();
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
