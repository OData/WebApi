//-----------------------------------------------------------------------------
// <copyright file="SelectExpandWrapperJsonConverter.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCOREAPP3_1_OR_GREATER
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Query.Expressions
{
    /// <summary>
    /// Represents a custom <see cref="JsonConverter"/> to serialize <see cref="SelectExpandWrapper{TElement}"/> instances to JSON.
    /// </summary>
    internal class SelectExpandWrapperJsonConverter : JsonConverterFactory
    {
        private static readonly Func<IEdmModel, IEdmStructuredType, IPropertyMapper> _mapperProvider =
            (IEdmModel model, IEdmStructuredType type) => new JsonPropertyNameMapper(model, type);

        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == null)
            {
                throw Error.ArgumentNull(nameof(typeToConvert));
            }

            if (!typeToConvert.IsGenericType)
            {
                return false;
            }

            return typeof(ISelectExpandWrapper).IsAssignableFrom(typeToConvert);
        }

        /// <inheritdoc/>
        public override JsonConverter CreateConverter(
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (typeToConvert == null || !typeToConvert.IsGenericType)
            {
                return null;
            }

            Type genericType = typeToConvert.GetGenericTypeDefinition();
            Type type = typeToConvert.GetGenericArguments()[0];

            if (genericType == typeof(SelectExpandBinder.SelectSome<>))
            {
                return (JsonConverter)Activator.CreateInstance(
                    typeof(SelectSomeOfTJsonConverter<>).MakeGenericType(new Type[] { type }));
            }

            if (genericType == typeof(SelectExpandBinder.SelectSomeAndInheritance<>))
            {
                return (JsonConverter)Activator.CreateInstance(
                    typeof(SelectSomeAndInheritanceOfTJsonConverter<>).MakeGenericType(new Type[] { type }));
            }

            if (genericType == typeof(SelectExpandBinder.SelectAll<>))
            {
                return (JsonConverter)Activator.CreateInstance(
                    typeof(SelectAllOfTJsonConverter<>).MakeGenericType(new Type[] { type }));
            }

            if (genericType == typeof(SelectExpandBinder.SelectAllAndExpand<>))
            {
                return (JsonConverter)Activator.CreateInstance(
                    typeof(SelectAllAndExpandOfTJsonConverter<>).MakeGenericType(new Type[] { type }));
            }

            if (genericType == typeof(SelectExpandWrapper<>))
            {
                return (JsonConverter)Activator.CreateInstance(
                    typeof(SelectExpandWrapperOfTJsonConverter<>).MakeGenericType(new Type[] { type }));
            }

            return null;
        }

        private class JsonPropertyNameMapper : IPropertyMapper
        {
            private IEdmModel _model;
            private IEdmStructuredType _type;

            public JsonPropertyNameMapper(IEdmModel model, IEdmStructuredType type)
            {
                _model = model;
                _type = type;
            }

            public string MapProperty(string propertyName)
            {
                IEdmProperty property = _type.Properties().First(s => s.Name == propertyName);
                PropertyInfo propertyInfo = GetPropertyInfo(property);

                Newtonsoft.Json.JsonPropertyAttribute jsonPropertyAttribute;
                JsonPropertyNameAttribute jsonPropertyNameAttribute;

                // NOTE: Preceding .NET Core 3.1 Newtonsoft was the default serialization library
                // We check the Newtonsoft JsonPropertyAttribute first to prevent breaking changes
                if ((jsonPropertyAttribute = GetNewtonsoftJsonPropertyAttribute(propertyInfo)) != null && !string.IsNullOrWhiteSpace(jsonPropertyAttribute.PropertyName))
                {
                    return jsonPropertyAttribute.PropertyName;
                }

                if ((jsonPropertyNameAttribute = GetSystemTextJsonPropertyNameAttribute(propertyInfo)) != null && !string.IsNullOrWhiteSpace(jsonPropertyNameAttribute.Name))
                {
                    return jsonPropertyNameAttribute.Name;
                }

                return property.Name;
            }

            private PropertyInfo GetPropertyInfo(IEdmProperty property)
            {
                ClrPropertyInfoAnnotation clrPropertyAnnotation = _model.GetAnnotationValue<ClrPropertyInfoAnnotation>(property);
                if (clrPropertyAnnotation != null)
                {
                    return clrPropertyAnnotation.ClrPropertyInfo;
                }

                ClrTypeAnnotation clrTypeAnnotation = _model.GetAnnotationValue<ClrTypeAnnotation>(property.DeclaringType);
                Contract.Assert(clrTypeAnnotation != null);

                PropertyInfo propertyInfo = clrTypeAnnotation.ClrType.GetProperty(property.Name);
                Contract.Assert(propertyInfo != null);

                return propertyInfo;
            }

            private static Newtonsoft.Json.JsonPropertyAttribute GetNewtonsoftJsonPropertyAttribute(PropertyInfo property)
            {
                return property.GetCustomAttributes(typeof(Newtonsoft.Json.JsonPropertyAttribute), inherit: false)
                    .OfType<Newtonsoft.Json.JsonPropertyAttribute>().SingleOrDefault();
            }

            private static JsonPropertyNameAttribute GetSystemTextJsonPropertyNameAttribute(PropertyInfo property)
            {
                return property.GetCustomAttributes(typeof(JsonPropertyNameAttribute), inherit: false)
                    .OfType<JsonPropertyNameAttribute>().SingleOrDefault();
            }
        }

        /// <summary>
        /// Represents a custom <see cref="JsonConverter"/> to serialize <see cref="SelectExpandBinder.SelectSome{TElement}"/> instances to JSON.
        /// </summary>
        private class SelectSomeOfTJsonConverter<TEntity>
            : JsonConverter<SelectExpandBinder.SelectSome<TEntity>>
        {
            /// <inheritdoc/>
            public override SelectExpandBinder.SelectSome<TEntity> Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException(
                    Error.Format(
                        SRResources.JsonConverterDoesNotSupportRead,
                        typeof(SelectExpandBinder.SelectSome<TEntity>).Name));
            }

            /// <inheritdoc/>
            public override void Write(
                Utf8JsonWriter writer,
                SelectExpandBinder.SelectSome<TEntity> value,
                JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(
                    writer,
                    value.ToDictionary(_mapperProvider), typeof(IDictionary<string, object>),
                    options);
            }
        }

        /// <summary>
        /// Represents a custom <see cref="JsonConverter"/> to serialize <see cref="SelectExpandBinder.SelectSomeAndInheritance{TElement}"/> instances to JSON.
        /// </summary>
        private class SelectSomeAndInheritanceOfTJsonConverter<TEntity>
            : JsonConverter<SelectExpandBinder.SelectSomeAndInheritance<TEntity>>
        {
            /// <inheritdoc/>
            public override SelectExpandBinder.SelectSomeAndInheritance<TEntity> Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException(
                    Error.Format(
                        SRResources.JsonConverterDoesNotSupportRead,
                        typeof(SelectExpandBinder.SelectSomeAndInheritance<TEntity>).Name));
            }

            /// <inheritdoc/>
            public override void Write(
                Utf8JsonWriter writer,
                SelectExpandBinder.SelectSomeAndInheritance<TEntity> value,
                JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(
                    writer,
                    value.ToDictionary(_mapperProvider),
                    typeof(IDictionary<string, object>), options);
            }
        }

        /// <summary>
        /// Represents a custom <see cref="JsonConverter"/> to serialize <see cref="SelectExpandBinder.SelectAll{TElement}"/> instances to JSON.
        /// </summary>
        private class SelectAllOfTJsonConverter<TEntity>
            : JsonConverter<SelectExpandBinder.SelectAll<TEntity>>
        {
            /// <inheritdoc/>
            public override SelectExpandBinder.SelectAll<TEntity> Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException(
                    Error.Format(
                        SRResources.JsonConverterDoesNotSupportRead,
                        typeof(SelectExpandBinder.SelectAll<TEntity>).Name));
            }

            /// <inheritdoc/>
            public override void Write(
                Utf8JsonWriter writer,
                SelectExpandBinder.SelectAll<TEntity> value,
                JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(
                    writer,
                    value.ToDictionary(_mapperProvider), typeof(IDictionary<string, object>),
                    options);
            }
        }

        /// <summary>
        /// Represents a custom <see cref="JsonConverter"/> to serialize <see cref="SelectExpandBinder.SelectAllAndExpand{TElement}"/> instances to JSON.
        /// </summary>
        private class SelectAllAndExpandOfTJsonConverter<TEntity>
            : JsonConverter<SelectExpandBinder.SelectAllAndExpand<TEntity>>
        {
            /// <inheritdoc/>
            public override SelectExpandBinder.SelectAllAndExpand<TEntity> Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException(
                    Error.Format(
                        SRResources.JsonConverterDoesNotSupportRead,
                        typeof(SelectExpandBinder.SelectAllAndExpand<TEntity>).Name));
            }

            /// <inheritdoc/>
            public override void Write(
                Utf8JsonWriter writer,
                SelectExpandBinder.SelectAllAndExpand<TEntity> value,
                JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(
                    writer,
                    value.ToDictionary(_mapperProvider),
                    typeof(IDictionary<string, object>), options);
            }
        }

        /// <summary>
        /// Represents a custom <see cref="JsonConverter"/> to serialize <see cref="SelectExpandWrapper{TElement}"/> instances to JSON.
        /// </summary>
        private class SelectExpandWrapperOfTJsonConverter<TEntity>
            : JsonConverter<SelectExpandWrapper<TEntity>>
        {
            /// <inheritdoc/>
            public override SelectExpandWrapper<TEntity> Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException(
                    Error.Format(
                        SRResources.JsonConverterDoesNotSupportRead,
                        typeof(SelectExpandWrapper<TEntity>).Name));
            }

            /// <inheritdoc/>
            public override void Write(
                Utf8JsonWriter writer,
                SelectExpandWrapper<TEntity> value,
                JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(
                    writer,
                    value.ToDictionary(_mapperProvider),
                    typeof(IDictionary<string, object>), options);
            }
        }
    }
}
#endif
