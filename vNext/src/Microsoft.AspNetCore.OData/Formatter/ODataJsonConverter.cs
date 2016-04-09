using System;
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData.Edm.Library;

namespace Microsoft.AspNetCore.OData.Formatter
{
	public class ODataJsonConverter : JsonConverter
	{
		private Uri _serviceRoot;
		private readonly ODataProperties _odataProperties;

		public ODataJsonConverter(Uri serviceRoot, ODataProperties odataProperties)
		{
			_serviceRoot = serviceRoot;
			_odataProperties = odataProperties;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			// value should be an entity or a collection of entities.
			var singleEntity = !(value is IEnumerable);
			writer.WriteStartObject();
			writer.WritePropertyName("@odata.context");
			writer.WriteValue(GenerateContextUrlString(value, singleEntity));
			if (_odataProperties.TotalCount.HasValue)
			{
				writer.WritePropertyName("@odata.count");
				writer.WriteValue(_odataProperties.TotalCount.Value);
			}
			if (_odataProperties.NextLink != null)
			{
				writer.WritePropertyName("@odata.nextLink");
				writer.WriteValue(_odataProperties.NextLink);
			}
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
				//var a = db.Products.Include(p => p.Customer);
				foreach (var o in (IQueryable)value)
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
			var edmType = _odataProperties.Model.GetEdmType(value.GetType()) as EdmEntityType;
			if (edmType == null)
			{
				return;
			}
			var properties = GetPublicProperties(value.GetType()).ToDictionary(
				p => p.Name);
			foreach (var property in edmType.DeclaredProperties)
			{
				var propertyInfo = properties[property.Name];
				if (propertyInfo.GetCustomAttribute<NotMappedAttribute>() != null)
				{
					continue;
				}
				WriteProperty(writer, value, propertyInfo, property, edmType);
			}
		}

		/// <summary>
		/// Hack just to get variables to show up in debug in VS
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="property"></param>
		/// <param name="edmProperty"></param>
		/// <param name="type"></param>
		private void WriteProperty(JsonWriter writer, object value, PropertyInfo property, IEdmProperty edmProperty, IEdmType type)
		{
			if (IsValidStructuralPropertyType(property.PropertyType))
			{
				writer.WritePropertyName(property.Name);
				writer.WriteValue(property.GetValue(value));
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
			return propertyType.Namespace == "System" &&
			       (!propertyType.GetTypeInfo().IsGenericType || propertyType.Name == "Nullable`1");
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