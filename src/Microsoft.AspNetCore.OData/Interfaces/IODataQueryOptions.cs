using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Validators;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Microsoft.AspNet.OData.Interfaces
{
    /// <summary>
    /// This defines a composite OData query options that can be used to perform query composition.
    /// Currently this only supports $filter, $orderby, $top, $skip, and $count.
    /// </summary>
    public interface IODataQueryOptions
    {
        /// <summary>
        ///  Gets the given <see cref="ODataQueryContext"/>
        /// </summary>
        ODataQueryContext Context { get; }
        /// <summary>
        /// Gets the raw string of all the OData query options
        /// </summary>
        ODataRawQueryOptions RawValues { get; }
        /// <summary>
        /// Gets the <see cref="SelectExpandQueryOption"/>.
        /// </summary>
        SelectExpandQueryOption SelectExpand { get; }
        /// <summary>
        /// Gets the <see cref="ApplyQueryOption"/>.
        /// </summary>
        ApplyQueryOption Apply { get; }
        /// <summary>
        /// Gets the <see cref="FilterQueryOption"/>.
        /// </summary>
        FilterQueryOption Filter { get; }
        /// <summary>
        /// Gets the <see cref="OrderByQueryOption"/>.
        /// </summary>
        OrderByQueryOption OrderBy { get; }
        /// <summary>
        /// Gets the <see cref="SkipQueryOption"/>.
        /// </summary>
         SkipQueryOption Skip { get; }

        /// <summary>
        /// Gets the <see cref="SkipTokenQueryOption"/>.
        /// </summary>
         SkipTokenQueryOption SkipToken { get; }

        /// <summary>
        /// Gets the <see cref="TopQueryOption"/>.
        /// </summary>
         TopQueryOption Top { get; }

        /// <summary>
        /// Gets the <see cref="CountQueryOption"/>.
        /// </summary>
         CountQueryOption Count { get; }

        /// <summary>
        /// Gets or sets the query validator.
        /// </summary>
         ODataQueryValidator Validator { get; set; }

        /// <summary>
        /// Gets the <see cref="ETag"/> from IfMatch header.
        /// </summary>
        ETag IfMatch { get; }

        /// <summary>
        /// Gets the <see cref="ETag"/> from IfNoneMatch header.
        /// </summary>
        ETag IfNoneMatch { get; }

        /// <summary>
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <returns>The new <see cref="IQueryable"/> after the query has been applied to.</returns>
        IQueryable ApplyTo(IQueryable query);

        /// <summary>
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="ignoreQueryOptions">The query parameters that are already applied in queries.</param>
        /// <returns>The new <see cref="IQueryable"/> after the query has been applied to.</returns>
        IQueryable ApplyTo(IQueryable query, AllowedQueryOptions ignoreQueryOptions);

        /// <summary>
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The settings to use in query composition.</param>
        /// <param name="ignoreQueryOptions">The query parameters that are already applied in queries.</param>
        /// <returns>The new <see cref="IQueryable"/> after the query has been applied to.</returns>
        IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings, AllowedQueryOptions ignoreQueryOptions);

        /// <summary>
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The settings to use in query composition.</param>
        /// <returns>The new <see cref="IQueryable"/> after the query has been applied to.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These are simple conversion function and cannot be split up.")]
        IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings);

        /// <summary>
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <param name="entity">The original entity.</param>
        /// <param name="querySettings">The <see cref="ODataQuerySettings"/> that contains all the query application related settings.</param>
        /// <param name="ignoreQueryOptions">The query parameters that are already applied in queries.</param>
        /// <returns>The new entity after the $select and $expand query has been applied to.</returns>
        /// <remarks>Only $select and $expand query options can be applied on single entities. This method throws if the query contains any other
        /// query options.</remarks>
        object ApplyTo(object entity, ODataQuerySettings querySettings, AllowedQueryOptions ignoreQueryOptions);

        /// <summary>
        /// Applies the query to the given entity using the given <see cref="ODataQuerySettings"/>.
        /// </summary>
        /// <param name="entity">The original entity.</param>
        /// <param name="querySettings">The <see cref="ODataQuerySettings"/> that contains all the query application related settings.</param>
        /// <returns>The new entity after the $select and $expand query has been applied to.</returns>
        /// <remarks>Only $select and $expand query options can be applied on single entities. This method throws if the query contains any other
        /// query options.</remarks>
        object ApplyTo(object entity, ODataQuerySettings querySettings);

        /// <summary>
        /// Generates the Stable OrderBy query option based on the existing OrderBy and other query options. 
        /// </summary>
        /// <returns>An order by query option that ensures stable ordering of the results.</returns>
        OrderByQueryOption GenerateStableOrder();

        /// <summary>
        /// Check if the given query option is the supported query option.
        /// </summary>
        /// <param name="queryOptionName">The name of the query option.</param>
        /// <returns>Returns <c>true</c> if the query option is the supported query option.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Need lower case string here.")]
        bool IsSupportedQueryOption(string queryOptionName);

        /// <summary>
        /// Validate all OData queries, including $skip, $top, $orderby and $filter, based on the given <paramref name="validationSettings"/>.
        /// It throws an ODataException if validation failed.
        /// </summary>
        /// <param name="validationSettings">The <see cref="ODataValidationSettings"/> instance which contains all the validation settings.</param>
        void Validate(ODataValidationSettings validationSettings);
    }
}
