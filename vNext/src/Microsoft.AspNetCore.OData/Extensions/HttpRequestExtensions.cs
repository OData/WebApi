using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Extensions
{
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Gets the <see cref="HttpRequestProperties"/> instance containing OData methods and properties
        /// for given <see cref="HttpRequest"/>.
        /// </summary>
        /// <param name="request">The request of interest.</param>
        /// <returns>
        /// An object through which OData methods and properties for given <paramref name="request"/> are available.
        /// </returns>
        public static ODataProperties ODataProperties(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

	        return request.HttpContext.ODataProperties();
        }

        public static IETagHandler ETagHandler(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.HttpContext.ETagHandler();
        }

        public static bool HasQueryOptions(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request?.Query != null && request.Query.Count > 0;
        }

		/// <summary>
		/// Creates a link for the next page of results; To be used as the value of @odata.nextLink.
		/// </summary>
		/// <param name="request">The request on which to base the next page link.</param>
		/// <param name="pageSize">The number of results allowed per page.</param>
		/// <returns>A next page link.</returns>
		public static Uri GetNextPageLink(this HttpRequest request, int pageSize)
		{
			var requestUriString = request?.GetDisplayUrl();
			if (requestUriString == null)
			{
				throw Error.ArgumentNull("request");
			}

			var requestUri = new Uri(requestUriString);
			if (!requestUri.IsAbsoluteUri)
			{
				throw Error.ArgumentUriNotAbsolute("request", requestUri);
			}

			return GetNextPageLink(requestUri, request.Query, pageSize);
		}

		//internal static Uri GetNextPageLink(Uri requestUri, int pageSize)
		//{
		//    Contract.Assert(requestUri != null);
		//    Contract.Assert(requestUri.IsAbsoluteUri);

		//    return GetNextPageLink(requestUri, new FormDataCollection(requestUri), pageSize);
		//}

		private static Uri GetNextPageLink(Uri requestUri, IQueryCollection queryParameters, int pageSize)
		{
			Contract.Assert(requestUri != null);
			Contract.Assert(queryParameters != null);
			Contract.Assert(requestUri.IsAbsoluteUri);

			var queryBuilder = new StringBuilder();

			var nextPageSkip = pageSize;

			foreach (var kvp in queryParameters)
			{
				var key = kvp.Key;
				string value = kvp.Value;
				switch (key)
				{
					case "$top":
						int top;
						if (Int32.TryParse(value, out top))
						{
							// There is no next page if the $top query option's value is less than or equal to the page size.
							//Contract.Assert(top > pageSize);
							// We decrease top by the pageSize because that's the number of results we're returning in the current page
							value = (top - pageSize).ToString(CultureInfo.InvariantCulture);
						}
						break;
					case "$skip":
						int skip;
						if (Int32.TryParse(value, out skip))
						{
							// We increase skip by the pageSize because that's the number of results we're returning in the current page
							nextPageSkip += skip;
						}
						continue;
					default:
						break;
				}

				if (key.Length > 0 && key[0] == '$')
				{
					// $ is a legal first character in query keys
					key = '$' + Uri.EscapeDataString(key.Substring(1));
				}
				else
				{
					key = Uri.EscapeDataString(key);
				}
				value = Uri.EscapeDataString(value);

				queryBuilder.Append(key);
				queryBuilder.Append('=');
				queryBuilder.Append(value);
				queryBuilder.Append('&');
			}

			queryBuilder.AppendFormat("$skip={0}", nextPageSkip);

			UriBuilder uriBuilder = new UriBuilder(requestUri)
			{
				Query = queryBuilder.ToString()
			};
			return uriBuilder.Uri;
		}
	}
}