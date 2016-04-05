using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Query
{
	// TODO: Replace with full version in the future.
	public class ODataQueryOptions
	{
		private readonly ODataQueryOptionParser _queryOptionParser;
		private string _assemblyName;
		private AllowedQueryOptions _ignoreQueryOptions = AllowedQueryOptions.None;

		/// <summary>
		/// Initializes a new instance of the <see cref="ODataQueryOptions"/> class based on the incoming request and some metadata information from
		/// the <see cref="ODataQueryContext"/>.
		/// </summary>
		/// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information.</param>
		/// <param name="request">The incoming request message.</param>
		public ODataQueryOptions(ODataQueryContext context, HttpRequest request, string assemblyName)
		{
			if (context == null)
			{
				throw Error.ArgumentNull("context");
			}

			if (request == null)
			{
				throw Error.ArgumentNull("request");
			}

			_assemblyName = assemblyName;

			Context = context;
			Request = request;
			RawValues = new ODataRawQueryOptions();

			var queryOptionDict = request.Query.ToDictionary(p => p.Key, p => p.Value.FirstOrDefault());
			_queryOptionParser = new ODataQueryOptionParser(
				context.Model,
				context.ElementType,
				context.NavigationSource,
				queryOptionDict);

			BuildQueryOptions(queryOptionDict);
		}

		/// <summary>
		///  Gets the given <see cref="ODataQueryContext"/>
		/// </summary>
		public ODataQueryContext Context { get; }

		/// <summary>
		/// Gets the request message associated with this instance.
		/// </summary>
		public HttpRequest Request { get; }

		/// <summary>
		/// Gets the raw string of all the OData query options
		/// </summary>
		public ODataRawQueryOptions RawValues { get; }

		/// <summary>
		/// Gets the <see cref="FilterQueryOption"/>.
		/// </summary>
		public FilterQueryOption Filter { get; private set; }

		/// <summary>
		/// Apply the individual query to the given IQueryable in the right order.
		/// </summary>
		/// <param name="query">The original <see cref="IQueryable"/>.</param>
		/// <param name="querySettings">The settings to use in query composition.</param>
		/// <param name="ignoreQueryOptions">The query parameters that are already applied in queries.</param>
		/// <returns>The new <see cref="IQueryable"/> after the query has been applied to.</returns>
		public virtual IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings, AllowedQueryOptions ignoreQueryOptions)
		{
			_ignoreQueryOptions = ignoreQueryOptions;
			if (query == null)
			{
				throw Error.ArgumentNull("query");
			}

			// Construct the actual query and apply them in the following order: filter
			if (Filter != null)
			{
				query = Filter.ApplyTo(query, querySettings, _assemblyName);
			}
			if (IsAvailableODataQueryOption(Top, AllowedQueryOptions.Top))
			{
				query = Top.ApplyTo(query, querySettings);
			}

			return query;
		}
		private bool IsAvailableODataQueryOption(object queryOption, AllowedQueryOptions queryOptionFlag)
		{
			return ((queryOption != null) && ((_ignoreQueryOptions & queryOptionFlag) == AllowedQueryOptions.None));
		}

		private void BuildQueryOptions(IDictionary<string, string> queryParameters)
		{
			foreach (var kvp in queryParameters)
			{
				switch (kvp.Key.ToLowerInvariant())
				{
					case "$filter":
						ThrowIfEmpty(kvp.Value, "$filter");
						RawValues.Filter = kvp.Value;
						Filter = new FilterQueryOption(kvp.Value, Context, _queryOptionParser);
						break;
					case "$orderby":
						ThrowIfEmpty(kvp.Value, "$orderby");
						RawValues.OrderBy = kvp.Value;
						break;
					case "$top":
						ThrowIfEmpty(kvp.Value, "$top");
						RawValues.Top = kvp.Value;
						Top = new TopQueryOption(kvp.Value, Context, _queryOptionParser);
						break;
					case "$skip":
						ThrowIfEmpty(kvp.Value, "$skip");
						RawValues.Skip = kvp.Value;
						break;
					case "$select":
						RawValues.Select = kvp.Value;
						break;
					case "$count":
						ThrowIfEmpty(kvp.Value, "$count");
						RawValues.Count = kvp.Value;
						break;
					case "$expand":
						RawValues.Expand = kvp.Value;
						break;
					case "$format":
						RawValues.Format = kvp.Value;
						break;
					case "$skiptoken":
						RawValues.SkipToken = kvp.Value;
						break;
				}
			}
		}

		public TopQueryOption Top { get; set; }

		private static void ThrowIfEmpty(string queryValue, string queryName)
		{
			if (String.IsNullOrWhiteSpace(queryValue))
			{
				throw new ODataException(Error.Format("Query '{0}' cannot be empty", queryName));
			}
		}
	}
}