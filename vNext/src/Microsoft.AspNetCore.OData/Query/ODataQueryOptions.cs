using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Query
{
	// TODO: Replace with full version in the future.
	public class ODataQueryOptions
	{
		private ODataQueryOptionParser _queryOptionParser;
		private readonly string _assemblyName;
		private AllowedQueryOptions _ignoreQueryOptions = AllowedQueryOptions.None;

		/// <summary>
		/// Initializes a new instance of the <see cref="ODataQueryOptions"/> class based on the incoming request and some metadata information from
		/// the <see cref="ODataQueryContext"/>.
		/// </summary>
		/// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information.</param>
		/// <param name="request">The incoming request message.</param>
		/// <param name="assemblyName"></param>
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
		/// Gets the <see cref="SelectExpandQueryOption"/>.
		/// </summary>
		public SelectExpandQueryOption SelectExpand { get; private set; }

		/// <summary>
		/// Gets the <see cref="FilterQueryOption"/>.
		/// </summary>
		public FilterQueryOption Filter { get; private set; }

		/// <summary>
		/// Gets the <see cref="OrderByQueryOption"/>.
		/// </summary>
		public OrderByQueryOption OrderBy { get; private set; }

		/// <summary>
		/// Gets the <see cref="CountQueryOption"/>.
		/// </summary>
		public CountQueryOption Count { get; private set; }

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
			if (IsAvailableODataQueryOption(Filter, AllowedQueryOptions.Filter))
			{
				query = Filter.ApplyTo(query, querySettings, _assemblyName);
			}

			if (IsAvailableODataQueryOption(Count, AllowedQueryOptions.Count))
			{
				if (Request.ODataProperties().TotalCountFunc == null)
				{
					Func<long> countFunc = Count.GetEntityCountFunc(query);
					if (countFunc != null)
					{
						Request.ODataProperties().TotalCountFunc = countFunc;
					}
				}

				if (ODataCountMediaTypeMapping.IsCountRequest(Request))
				{
					return query;
				}
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

			AddAutoExpandProperties(querySettings);

			if (SelectExpand != null)
			{
				var tempResult = ApplySelectExpand(query, querySettings);
				if (tempResult != default(IQueryable))
				{
					query = tempResult;
				}
			}

			if (querySettings.PageSize.HasValue)
			{
				bool resultsLimited = true;
				query = LimitResults(query, querySettings.PageSize.Value, out resultsLimited);
				var uriString = Request.GetDisplayUrl();
				if (!string.IsNullOrWhiteSpace(uriString))
				{
					var uri = new Uri(uriString);
					if (resultsLimited && uri != null && uri.IsAbsoluteUri && Request.ODataProperties().NextLink == null)
					{
						Uri nextPageLink = Request.GetNextPageLink(querySettings.PageSize.Value);
						Request.ODataProperties().NextLink = nextPageLink;
					}
				}
			}

			return query;
		}

		internal void AddAutoExpandProperties(ODataQuerySettings querySettings)
		{
			var autoExpandRawValue = GetAutoExpandRawValue(querySettings.SearchDerivedTypeWhenAutoExpand);
			if (autoExpandRawValue != null && !autoExpandRawValue.Equals(RawValues.Expand))
			{
				var queryParameters = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
				queryParameters["$expand"] = autoExpandRawValue;
				_queryOptionParser = new ODataQueryOptionParser(
					Context.Model,
					Context.ElementType,
					Context.NavigationSource,
					queryParameters);
				var originalSelectExpand = SelectExpand;
				SelectExpand = new SelectExpandQueryOption(RawValues.Select, autoExpandRawValue, Context,
					_queryOptionParser);
				if (originalSelectExpand != null && originalSelectExpand.LevelsMaxLiteralExpansionDepth > 0)
				{
					SelectExpand.LevelsMaxLiteralExpansionDepth = originalSelectExpand.LevelsMaxLiteralExpansionDepth;
				}
			}
		}

		private string GetAutoExpandRawValue(bool discoverDerivedTypeWhenAutoExpand)
		{
			var expandRawValue = RawValues.Expand;
			IEdmEntityType baseEntityType = Context.ElementType as IEdmEntityType;
			var autoExpandRawValue = String.Empty;
			var autoExpandNavigationProperties = EdmLibHelpers.GetAutoExpandNavigationProperties(baseEntityType,
				Context.Model, discoverDerivedTypeWhenAutoExpand);

			foreach (var property in autoExpandNavigationProperties)
			{
				if (!String.IsNullOrEmpty(autoExpandRawValue))
				{
					autoExpandRawValue += ",";
				}

				if (property.DeclaringEntityType() != baseEntityType)
				{
					autoExpandRawValue += String.Format(CultureInfo.InvariantCulture, "{0}/",
						property.DeclaringEntityType().FullTypeName());
				}

				autoExpandRawValue += property.Name;
			}

			if (!String.IsNullOrEmpty(autoExpandRawValue))
			{
				if (!String.IsNullOrEmpty(expandRawValue))
				{
					expandRawValue = String.Format(CultureInfo.InvariantCulture, "{0},{1}",
						autoExpandRawValue, expandRawValue);
				}
				else
				{
					expandRawValue = autoExpandRawValue;
				}
			}
			return expandRawValue;
		}

		/// <summary>
		/// Limits the query results to a maximum number of results.
		/// </summary>
		/// <typeparam name="T">The entity CLR type</typeparam>
		/// <param name="queryable">The queryable to limit.</param>
		/// <param name="limit">The query result limit.</param>
		/// <param name="resultsLimited"><c>true</c> if the query results were limited; <c>false</c> otherwise</param>
		/// <returns>The limited query results.</returns>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Not intended for public use, only public to enable invokation without security issues.")]
		public static IQueryable<T> LimitResults<T>(IQueryable<T> queryable, int limit, out bool resultsLimited)
		{
			TruncatedCollection<T> truncatedCollection = new TruncatedCollection<T>(queryable, limit);
			resultsLimited = truncatedCollection.IsTruncated;
			return truncatedCollection.AsQueryable();
		}

		private static readonly MethodInfo _limitResultsGenericMethod = typeof(ODataQueryOptions).GetMethod("LimitResults");
		internal static IQueryable LimitResults(IQueryable queryable, int limit, out bool resultsLimited)
		{
			MethodInfo genericMethod = _limitResultsGenericMethod.MakeGenericMethod(queryable.ElementType);
			object[] args = new object[] { queryable, limit, null };
			IQueryable results = genericMethod.Invoke(null, args) as IQueryable;
			resultsLimited = (bool)args[2];
			return results;
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
						Count = new CountQueryOption(kvp.Value, Context, _queryOptionParser);
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

				if (RawValues.Select != null || RawValues.Expand != null)
				{
					SelectExpand = new SelectExpandQueryOption(RawValues.Select, RawValues.Expand, Context);
				}
			}

			if (ODataCountMediaTypeMapping.IsCountRequest(Request))
			{
				Count = new CountQueryOption(
					"true",
					Context,
					new ODataQueryOptionParser(
						Context.Model,
						Context.ElementType,
						Context.NavigationSource,
						new Dictionary<string, string> { { "$count", "true" } }));
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

		private T ApplySelectExpand<T>(T entity, ODataQuerySettings querySettings)
		{
			var result = default(T);
			bool selectAvailable = IsAvailableODataQueryOption(SelectExpand.RawSelect, AllowedQueryOptions.Select);
			bool expandAvailable = IsAvailableODataQueryOption(SelectExpand.RawExpand, AllowedQueryOptions.Expand);
			if (selectAvailable || expandAvailable)
			{
				if ((!selectAvailable && SelectExpand.RawSelect != null) ||
					(!expandAvailable && SelectExpand.RawExpand != null))
				{
					SelectExpand = new SelectExpandQueryOption(
						selectAvailable ? RawValues.Select : null,
						expandAvailable ? RawValues.Expand : null,
						SelectExpand.Context);
				}
				SelectExpand.SearchDerivedTypeWhenAutoExpand = querySettings.SearchDerivedTypeWhenAutoExpand;
				SelectExpandClause processedClause = SelectExpand.ProcessLevels();
				SelectExpandQueryOption newSelectExpand = new SelectExpandQueryOption(
					SelectExpand.RawSelect,
					SelectExpand.RawExpand,
					SelectExpand.Context,
					processedClause);

				Request.ODataProperties().SelectExpandClause = processedClause;

				var type = typeof(T);
				if (type == typeof(IQueryable))
				{
					result = (T)newSelectExpand.ApplyTo((IQueryable)entity, querySettings, _assemblyName);
				}
				else if (type == typeof(object))
				{
					result = (T)newSelectExpand.ApplyTo(entity, querySettings, _assemblyName);
				}
			}
			return result;
		}
	}
}