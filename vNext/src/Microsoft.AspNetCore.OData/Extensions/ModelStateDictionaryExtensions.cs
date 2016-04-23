using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData.Builder;
using Microsoft.OData.Core;

namespace Microsoft.AspNetCore.OData.Extensions
{
	/// <summary>
	/// Provides extension methods for the <see cref="HttpError"/> class.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class ModelStateDictionaryExtensions
	{
		public static ODataError ToODataError(this ModelStateDictionary modelState, string errorCode = "", string errorMessage = "")
		{
			return new ODataError
			{
				Message = errorMessage,
				ErrorCode = errorCode,
				Details = modelState.Select(kvp => ToODataErrorDetail(kvp.Key, kvp.Value)).ToList(),
//				InnerError = ToODataInnerError(httpError)
			};
		}

		private static ODataErrorDetail ToODataErrorDetail(string key, ModelStateEntry modelStateEntry)
		{
			var detail = new ODataErrorDetail();
			var messages = modelStateEntry.Errors.Select(error => error.ErrorMessage).ToList();
			detail.Target = key;
			detail.Message = string.Join(", ", messages);
			return detail;
		}
	}
}