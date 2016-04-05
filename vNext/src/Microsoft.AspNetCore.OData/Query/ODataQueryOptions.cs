using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Query
{
	// TODO: Replace with full version in the future.
	public class ODataQueryOptions
	{
		private readonly ODataQueryOptionParser _queryOptionParser;
		private readonly string _assemblyName;
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
		/// Gets the <see cref="OrderByQueryOption"/>.
		/// </summary>
		public OrderByQueryOption OrderBy { get; private set; }

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

			OrderByQueryOption orderBy = OrderBy;

			// $skip or $top require a stable sort for predictable results.
			// Result limits require a stable sort to be able to generate a next page link.
			// If either is present in the query and we have permission,
			// generate an $orderby that will produce a stable sort.
			if (querySettings.EnsureStableOrdering &&
				(IsAvailableODataQueryOption(Skip, AllowedQueryOptions.Skip) ||
				 IsAvailableODataQueryOption(Top, AllowedQueryOptions.Top) ||
				 querySettings.PageSize.HasValue))
			{
				// If there is no OrderBy present, we manufacture a default.
				// If an OrderBy is already present, we add any missing
				// properties necessary to make a stable sort.
				// Instead of failing early here if we cannot generate the OrderBy,
				// let the IQueryable backend fail (if it has to).
				orderBy = orderBy == null
							? GenerateDefaultOrderBy(Context)
							: EnsureStableSortOrderBy(orderBy, Context);
			}

			if (IsAvailableODataQueryOption(orderBy, AllowedQueryOptions.OrderBy))
			{
				query = orderBy.ApplyTo(query, querySettings);
			}
			if (IsAvailableODataQueryOption(Skip, AllowedQueryOptions.Skip))
			{
				query = Skip.ApplyTo(query, querySettings);
			}
			if (IsAvailableODataQueryOption(Top, AllowedQueryOptions.Top))
			{
				query = Top.ApplyTo(query, querySettings);
			}


			return query;
		}

		/// <summary>
		/// Ensures the given <see cref="OrderByQueryOption"/> will produce a stable sort.
		/// If it will, the input <paramref name="orderBy"/> will be returned
		/// unmodified.  If the given <see cref="OrderByQueryOption"/> will not produce a
		/// stable sort, a new <see cref="OrderByQueryOption"/> instance will be created
		/// and returned.
		/// </summary>
		/// <param name="orderBy">The <see cref="OrderByQueryOption"/> to evaluate.</param>
		/// <param name="context">The <see cref="ODataQueryContext"/>.</param>
		/// <returns>An <see cref="OrderByQueryOption"/> that will produce a stable sort.</returns>
		private static OrderByQueryOption EnsureStableSortOrderBy(OrderByQueryOption orderBy, ODataQueryContext context)
		{
			Contract.Assert(orderBy != null);
			Contract.Assert(context != null);

			// Strategy: create a hash of all properties already used in the given OrderBy
			// and remove them from the list of properties we need to add to make the sort stable.
			HashSet<string> usedPropertyNames =
				new HashSet<string>(orderBy.OrderByNodes.OfType<OrderByPropertyNode>().Select(node => node.Property.Name));

			IEnumerable<IEdmStructuralProperty> propertiesToAdd = GetAvailableOrderByProperties(context).Where(prop => !usedPropertyNames.Contains(prop.Name));

			if (propertiesToAdd.Any())
			{
				// The existing query options has too few properties to create a stable sort.
				// Clone the given one and add the remaining properties to end, thereby making
				// the sort stable but preserving the user's original intent for the major
				// sort order.
				orderBy = new OrderByQueryOption(orderBy);

				foreach (IEdmStructuralProperty property in propertiesToAdd)
				{
					orderBy.OrderByNodes.Add(new OrderByPropertyNode(property, OrderByDirection.Ascending));
				}
			}

			return orderBy;
		}
		// Generates the OrderByQueryOption to use by default for $skip or $top
		// when no other $orderby is available.  It will produce a stable sort.
		// This may return a null if there are no available properties.
		private static OrderByQueryOption GenerateDefaultOrderBy(ODataQueryContext context)
		{
			string orderByRaw = String.Empty;
			if (EdmLibHelpers.IsDynamicTypeWrapper(context.ElementClrType))
			{
				orderByRaw = String.Join(",",
					context.ElementClrType.GetTypeInfo().GetProperties()
						.Where(property => EdmLibHelpers.GetEdmPrimitiveTypeOrNull(property.PropertyType) != null)
						.Select(property => property.Name));
			}
			else
			{
				orderByRaw = String.Join(",",
					GetAvailableOrderByProperties(context)
						.Select(property => property.Name));
			}

			return String.IsNullOrEmpty(orderByRaw)
					? null
					: new OrderByQueryOption(orderByRaw, context);
		}


		// Returns a sorted list of all properties that may legally appear
		// in an OrderBy.  If the entity type has keys, all are returned.
		// Otherwise, when no keys are present, all primitive properties are returned.
		private static IEnumerable<IEdmStructuralProperty> GetAvailableOrderByProperties(ODataQueryContext context)
		{
			Contract.Assert(context != null);

			IEdmEntityType entityType = context.ElementType as IEdmEntityType;
			if (entityType != null)
			{
				IEnumerable<IEdmStructuralProperty> properties =
					entityType.Key().Any()
						? entityType.Key()
						: entityType
							.StructuralProperties()
							.Where(property => property.Type.IsPrimitive());

				// Sort properties alphabetically for stable sort
				return properties.OrderBy(property => property.Name);
			}
			else
			{
				return Enumerable.Empty<IEdmStructuralProperty>();
			}
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
						OrderBy = new OrderByQueryOption(kvp.Value, Context, _queryOptionParser);
						break;
					case "$top":
						ThrowIfEmpty(kvp.Value, "$top");
						RawValues.Top = kvp.Value;
						Top = new TopQueryOption(kvp.Value, Context, _queryOptionParser);
						break;
					case "$skip":
						ThrowIfEmpty(kvp.Value, "$skip");
						RawValues.Skip = kvp.Value;
						Skip = new SkipQueryOption(kvp.Value, Context, _queryOptionParser);
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
		public SkipQueryOption Skip { get; set; }

		private static void ThrowIfEmpty(string queryValue, string queryName)
		{
			if (String.IsNullOrWhiteSpace(queryValue))
			{
				throw new ODataException(Error.Format("Query '{0}' cannot be empty", queryName));
			}
		}
	}
}