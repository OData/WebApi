// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Data.Edm;
using Newtonsoft.Json;

namespace System.Web.Http.OData.Query.Expressions
{
    /// <summary>
    /// Represents a custom <see cref="JsonConverter"/> to serialize <see cref="SelectExpandWrapper{TElement}"/> instances to JSON.
    /// </summary>
    internal class SelectExpandWrapperConverter : JsonConverter
    {
        internal static readonly Func<IEdmModel, IEdmStructuredType, IPropertyMapper> MapperProvider =
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
                serializer.Serialize(writer, selectExpandWrapper.ToDictionary(MapperProvider));
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
                Type clrType = GetClrType();
                PropertyInfo info = clrType.GetProperty(propertyName);
                JsonPropertyAttribute jsonProperty = GetJsonProperty(info);
                if (jsonProperty != null && !String.IsNullOrWhiteSpace(jsonProperty.PropertyName))
                {
                    return jsonProperty.PropertyName;
                }
                else
                {
                    DataMemberAttribute dataMember = GetDataMember(clrType, info);
                    if (dataMember != null && !String.IsNullOrWhiteSpace(dataMember.Name))
                    {
                        return dataMember.Name;
                    }
                }
                return propertyName;
            }

            private Type GetClrType()
            {
                ClrTypeAnnotation clrTypeAnnotation = _model.GetAnnotationValue<ClrTypeAnnotation>(_type);
                Contract.Assert(clrTypeAnnotation != null);
                Contract.Assert(clrTypeAnnotation.ClrType != null);
                return clrTypeAnnotation.ClrType;
            }

            private static DataMemberAttribute GetDataMember(Type clrType, PropertyInfo info)
            {
                // Find if a type in the type hierarchy has been annotated with the DataContractAttribute,
                // This is the behavior that JSON.NET exposes.
                Type currentType = clrType;
                while (currentType.GetCustomAttribute<DataContractAttribute>(inherit: true) == null &&
                    currentType.BaseType != null)
                {
                    currentType = currentType.BaseType;
                }

                if (currentType != null)
                {
                    return info.GetCustomAttribute<DataMemberAttribute>(inherit: false);
                }
                return null;
            }

            private static JsonPropertyAttribute GetJsonProperty(PropertyInfo property)
            {
                return property.GetCustomAttribute<JsonPropertyAttribute>(inherit: false);
            }
        }
    }
}
