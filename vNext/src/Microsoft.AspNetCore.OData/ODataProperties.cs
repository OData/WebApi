using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using ODataPath = Microsoft.AspNetCore.OData.Routing.ODataPath;

namespace Microsoft.AspNetCore.OData
{
	public class ODataProperties
	{
		private const string TotalCountFuncKey = "System.Web.OData.TotalCountFunc";
		private const string TotalCountKey = "System.Web.OData.TotalCount";

		internal const string ODataServiceVersionHeader = "OData-Version";

		internal const ODataVersion DefaultODataVersion = ODataVersion.V4;

		private HttpRequestMessage _request;

		public IEdmModel Model { get; set; }

		// TODO: Consider remove this.
		public ODataPath Path { get; set; }

		public Microsoft.OData.Core.UriParser.Semantic.ODataPath NewPath { get; set; }

		/// <summary>
		///     Gets or sets the total count for the OData response.
		/// </summary>
		/// <value><c>null</c> if no count should be sent back to the client.</value>
		public long? TotalCount
		{
			get
			{
				object totalCount;
				if (_request.Properties.TryGetValue(TotalCountKey, out totalCount))
				{
					// Fairly big problem if following cast fails. Indicates something else is writing properties with
					// names we've chosen. Do not silently return null because that will hide the problem.
					return (long) totalCount;
				}

				if (TotalCountFunc != null)
				{
					var count = TotalCountFunc();
					_request.Properties[TotalCountKey] = count;
					return count;
				}

				return null;
			}
			set
			{
				if (!value.HasValue)
				{
					throw Error.ArgumentNull("value");
				}

				_request.Properties[TotalCountKey] = value;
			}
		}

		public Uri NextLink { get; set; }

		public bool IsValidODataRequest { get; set; }

		public SelectExpandClause SelectExpandClause { get; set; }

		internal Func<long> TotalCountFunc
		{
			get
			{
				object totalCountFunc;
				if (_request.Properties.TryGetValue(TotalCountFuncKey, out totalCountFunc))
				{
					return (Func<long>) totalCountFunc;
				}

				return null;
			}
			set { _request.Properties[TotalCountFuncKey] = value; }
		}

		internal void Configure(HttpContext httpContext)
		{
			_request = httpContext.GetHttpRequestMessage();
			//Contract.Assert(request != null);
			//_request = request;
		}
	}
}