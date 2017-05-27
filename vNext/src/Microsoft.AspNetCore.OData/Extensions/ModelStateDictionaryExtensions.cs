using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
				Details = modelState.SelectMany(kvp => ToODataErrorDetails(kvp.Key, kvp.Value)).ToList(),
//				InnerError = ToODataInnerError(httpError)
			};
		}

		private static IEnumerable<ODataErrorDetail> ToODataErrorDetails(string key, ModelStateEntry modelStateEntry)
		{
			foreach (var error in modelStateEntry.Errors)
			{
				var detail = new ODataErrorDetail();
				detail.Target = key;
				detail.Message = error.ErrorMessage;
				yield return detail;
			}
		}
	}
}