using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.OData.Extensions
{
	public static class ControllerExtensions
	{
		public static IActionResult ODataModelStateError(this Controller controller,
			string errorCode = "", string errorMessage = "")
		{
			return controller.BadRequest(controller.ModelState.ToODataError(errorCode, errorMessage));
		}

		public static IActionResult ODataModelState(this Controller controller,
			string errorCode = "", string errorMessage = "")
		{
			return controller.Ok(controller.ModelState.ToODataError(errorCode, errorMessage));
		}

		public static T GetODataModel<T>(this Controller controller, JObject obj, bool isPatch)
		{
			if (controller.ModelState.Any())
			{
				controller.ModelState.Clear();
			}
			var type = typeof(T);
			if (isPatch)
			{
				// If we're patching, we only care about the properties
				// we're trying to update
				foreach (var jProperty in obj.PropertyValues())
				{
					ValidateProperty<T>(controller, obj, type.GetProperty(jProperty.Path));
				}
			}
			else
			{
				foreach (var property in type.GetProperties())
				{
					ValidateProperty<T>(controller, obj, property);
				}
			}
			var value = default(T);
			if (controller.ModelState.IsValid)
			{
				var jsonSerializer = JsonSerializer.CreateDefault();
				value = obj.ToObject<T>(jsonSerializer);
			}
			return value;
		}

		private static void ValidateProperty<T>(Controller controller, JObject obj, PropertyInfo property)
		{
			var modelState = controller.ModelState;
			var jToken = obj.GetValue(property.Name);
			var propertyValue = jToken != null && jToken.HasValues ? jToken.Value<string>() : null;
			controller.ValidateField(property, propertyValue, modelState);
		}

		public static async Task<IActionResult> ValidateField<T>(this Controller controller,
			JObject validation)
		{
			return await controller.ValidateField(validation, typeof (T));
		}

		public static async Task<IActionResult> ValidateFieldInService<TService>(this Controller controller, 
			JObject validation)
		{
			var setName = validation.GetValue("SetName").Value<string>();
			var setType = typeof (TService).GetProperty(setName).PropertyType.GetGenericArguments().First();
			var fieldName = validation.GetValue("Name").Value<string>();
			var valueToValidate = validation.GetValue("Value")?.Value<string>();
			controller.ValidateField(setType, fieldName, valueToValidate);
			return controller.ODataModelState();
		}

		public static async Task<IActionResult> ValidateField(this Controller controller, 
			JObject validation, Type type)
		{
			var fieldName = validation.GetValue("Name").Value<string>();
			var valueToValidate = validation.GetValue("Value")?.Value<string>();
			controller.ValidateField(type, fieldName, valueToValidate);
			return controller.ODataModelState();
		}

		public static void ValidateField<T>(this Controller controller, string propertyName, object propertyValue,
			ModelStateDictionary modelState = null)
		{
			controller.ValidateField(
				typeof(T),
				propertyName,
				propertyValue,
				modelState
				);
		}

		public static void ValidateField(this Controller controller, Type type, string propertyName, object propertyValue,
			ModelStateDictionary modelState = null)
		{
			controller.ValidateField(
				type.GetProperty(propertyName),
				propertyValue,
				modelState
				);
		}

		public static void ValidateField(this Controller controller, PropertyInfo property, object propertyValue,
			ModelStateDictionary modelState = null)
		{
			modelState = modelState ?? controller.ModelState;
			var validations = property.GetCustomAttributes<ValidationAttribute>();
			foreach (var validation in validations)
			{
				var validationContext = new ValidationContext(controller) {DisplayName = DisplayName(property)};
				//var valid = validation.IsValid(propertyValue);
				var result = validation.GetValidationResult(propertyValue, validationContext);
				if (result?.ErrorMessage != null)
				{
					modelState.AddModelError(property.Name, result.ErrorMessage);
				}
			}
		}

		private static string DisplayName(PropertyInfo property)
		{
			var attr = property.GetCustomAttribute<DisplayNameAttribute>();
			return attr == null ? property.Name : attr.DisplayName;
		}
	}
}