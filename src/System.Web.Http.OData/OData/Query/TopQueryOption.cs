// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;
using System.Linq;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Query
{
    public class TopQueryOption
    {
        /// <summary>
        /// Initialize a new instance of <see cref="OrderByQueryOption"/> based on the raw $top value and 
        /// an EdmModel from <see cref="ODataQueryContext"/>.
        /// </summary>
        /// <param name="rawValue">The raw value for $top query. It can be null or empty.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        public TopQueryOption(string rawValue, ODataQueryContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (string.IsNullOrEmpty(rawValue))
            {
                throw Error.ArgumentNullOrEmpty("rawValue");
            }

            Context = context;
            RawValue = rawValue;     
        }

        /// <summary>
        ///  Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>        
        public ODataQueryContext Context { get; private set; }

        /// <summary>
        ///  Gets the raw $top value.
        /// </summary>
        public string RawValue { get; private set; }

        /// <summary>
        /// Apply the $top query to the given IQueryable.
        /// </summary>
        /// <param name="query">The IQueryable that we are applying top query against.</param>
        /// <returns>The query that the top query has been applied to.</returns>
        public IOrderedQueryable<T> ApplyTo<T>(IQueryable<T> query)
        {
            return ApplyToCore(query) as IOrderedQueryable<T>;
        }

        /// <summary>
        /// Apply the $top query to the given IQueryable.
        /// </summary>
        /// <param name="query">The IQueryable that we are applying top query against.</param>
        /// <returns>The query that the top query has been applied to.</returns>
        public IQueryable ApplyTo(IQueryable query)
        {
            return ApplyToCore(query);
        }

        private IQueryable ApplyToCore(IQueryable query)
        {
            try
            {
                return ExpressionHelpers.Take(query, Convert.ToInt32(RawValue, CultureInfo.InvariantCulture), Context.EntityClrType);
            }
            catch (FormatException exception)
            {
                throw new ODataException(Error.Format(SRResources.CanNotParseInteger, RawValue), exception);
            }
        }
    }
}
