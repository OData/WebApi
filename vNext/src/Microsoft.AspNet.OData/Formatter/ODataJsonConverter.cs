using Microsoft.OData.Edm;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.OData.Formatter
{
    public class ODataJsonConverter : JsonConverter
    {
        private Uri _serviceRoot;

        public ODataJsonConverter(Uri serviceRoot)
        {
            _serviceRoot = serviceRoot;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // value should be an entity or a collection of entities.
            var singleEntity = !(value is IEnumerable);
            writer.WriteStartObject();
            writer.WritePropertyName("@odata.context");
            writer.WriteValue(GenerateContextUrlString(value, singleEntity));
            if (!singleEntity)
            {
                writer.WritePropertyName("value");
                writer.WriteStartArray();
            }
            if (singleEntity)
            {
                WriteEntity(writer, value);
            }
            else
            {
                foreach (var o in (IEnumerable)value)
                {
                    writer.WriteStartObject();
                    WriteEntity(writer, o);
                    writer.WriteEndObject();
                }
            }
            if (!singleEntity)
            {
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }

        private void WriteEntity(JsonWriter writer, object value)
        {
            writer.WritePropertyName("@odata.id");
            writer.WriteValue(GenerateIdLinkString(value));
            foreach (var property in GetPublicProperties(value.GetType()))
            {
                if (IsValidStructuralPropertyType(property.PropertyType))
                {
                    writer.WritePropertyName(property.Name);
                    writer.WriteValue(property.GetValue(value));
                }
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return !IsValidStructuralPropertyType(objectType);
        }

        private static bool IsValidStructuralPropertyType(Type propertyType)
        {
            return propertyType.Namespace == "System" && !propertyType.GetTypeInfo().IsGenericType;
        }

        private string GenerateContextUrlString(object value, bool singleEntity)
        {
            return "$metadata#";
            //var valueType = value.GetType();
            //var entityClrType = singleEntity ? valueType : TypeHelper.GetImplementedIEnumerableType(valueType);
            //var entitySet = GetEntitySet(entityClrType);
            //return string.Format("{0}$metadata#{1}{2}", _serviceRoot, entitySet.Name, singleEntity ? "/$entity" : string.Empty);
        }

        private string GenerateIdLinkString(object value)
        {
            return "someid";
            //var valueType = value.GetType();
            //var entitySet = GetEntitySet(valueType);
            //var keyName = entitySet?.EntityType().Key().SingleOrDefault()?.Name;
            //var keyProperty = keyName!=null? GetPublicProperty(valueType, keyName):null;
            //var keyValue = keyProperty?.GetValue(value);
            //return string.Format("{0}{1}({2})", _serviceRoot, entitySet.Name, keyValue);
        }

        private IEdmEntitySet GetEntitySet(Type clrType)
        {
            //var edmTypeName = clrType.EdmFullName();
            //return _model.EntityContainer.EntitySets().Single(e => e.EntityType().FullTypeName() == edmTypeName);
            return null;
        }

        private PropertyInfo[] GetPublicProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        private PropertyInfo GetPublicProperty(Type type, string name)
        {
            return GetPublicProperties(type).Single(p => p.Name == name);
        }
    }
}