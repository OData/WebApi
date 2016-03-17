// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Newtonsoft.Json;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    using System;

    /// <summary>
    /// Represents a custom <see cref="JsonConverter"/> to serialize <see cref="SelectExpandWrapper{TElement}"/> instances to JSON.
    /// </summary>
    internal class SelectExpandWrapperConverter : JsonConverter
    {
        private static readonly Func<IEdmModel, IEdmStructuredType, IPropertyMapper> _mapperProvider =
                (IEdmModel model, IEdmStructuredType type) => new JsonPropertyNameMapper(model, type);

        public override bool CanConvert(Type objectType)
        {
            if (objectType == null)
            {
                throw Error.ArgumentNull("objectType");
            }

            return objectType.IsAssignableFrom(typeof(ISelectExpandWrapper));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Contract.Assert(false, "SelectExpandWrapper is internal and should never be deserialized into.");
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            ISelectExpandWrapper selectExpandWrapper = value as ISelectExpandWrapper;
            if (selectExpandWrapper != null)
            {
                serializer.Serialize(writer, selectExpandWrapper.ToDictionary(_mapperProvider));
            }
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
                IEdmProperty property = _type.Properties().Single(s => s.Name == propertyName);
                PropertyInfo info = GetPropertyInfo(property);
                JsonPropertyAttribute jsonProperty = GetJsonProperty(info);
                if (jsonProperty != null && !String.IsNullOrWhiteSpace(jsonProperty.PropertyName))
                {
                    return jsonProperty.PropertyName;
                }
                else
                {
                    return property.Name;
                }
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

                PropertyInfo info = clrTypeAnnotation.ClrType.GetProperty(property.Name);
                Contract.Assert(info != null);

                return info;
            }

            private static JsonPropertyAttribute GetJsonProperty(PropertyInfo property)
            {
                return property.GetCustomAttributes(typeof(JsonPropertyAttribute), inherit: false)
                       .OfType<JsonPropertyAttribute>().SingleOrDefault();
            }
        }
    }
}
