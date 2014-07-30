// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Properties;
using System.Web.OData.Query.Validators;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;

namespace System.Web.OData.Query
{
    /// <summary>
    /// This defines a $top OData query option for querying.
    /// </summary>
    public class TopQueryOption
    {
        private int? _value;
        private ODataQueryOptionParser _queryOptionParser;

        /// <summary>
        /// Initialize a new instance of <see cref="TopQueryOption"/> based on the raw $top value and 
        /// an EdmModel from <see cref="ODataQueryContext"/>.
        /// </summary>
        /// <param name="rawValue">The raw value for $top query. It can be null or empty.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        /// <param name="queryOptionParser">The <see cref="ODataQueryOptionParser"/> which is used to parse the query option.</param>
        public TopQueryOption(string rawValue, ODataQueryContext context, ODataQueryOptionParser queryOptionParser)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (String.IsNullOrEmpty(rawValue))
            {
                throw Error.ArgumentNullOrEmpty("rawValue");
            }

            if (queryOptionParser == null)
            {
                throw Error.ArgumentNull("queryOptionParser");
            }

            Context = context;
            RawValue = rawValue;
            Validator = new TopQueryValidator();
            _queryOptionParser = queryOptionParser;
        }

        // This constructor is intended for unit testing only.
        internal TopQueryOption(string rawValue, ODataQueryContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (String.IsNullOrEmpty(rawValue))
            {
                throw Error.ArgumentNullOrEmpty("rawValue");
            }

            Context = context;
            RawValue = rawValue;
            Validator = new TopQueryValidator();
            _queryOptionParser = new ODataQueryOptionParser(
                context.Model,
                context.ElementType,
                context.NavigationSource,
                new Dictionary<string, string> { { "$top", rawValue } });
        }

        /// <summary>
        /// Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; private set; }

        /// <summary>
        /// Gets the raw $top value.
        /// </summary>
        public string RawValue { get; private set; }

        /// <summary>
        /// Gets the value of the $top as a parsed integer.
        /// </summary>
        public int Value
        {
            get
            {
                if (_value == null)
                {
                    long? topValue = _queryOptionParser.ParseTop();

                    if (topValue.HasValue)
                    {
                        Contract.Assert(topValue.Value <= Int32.MaxValue);
                    }

                    _value = (int?)topValue;
                }

                Contract.Assert(_value.HasValue);
                return _value.Value;
            }
        }

        /// <summary>
        /// Gets or sets the Top Query Validator.
        /// </summary>
        public TopQueryValidator Validator { get; set; }

        /// <summary>
        /// Apply the $top query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The query settings to use while applying this query option.</param>
        /// <returns>The new <see cref="IQueryable"/> after the top query has been applied to.</returns>
        public IOrderedQueryable<T> ApplyTo<T>(IQueryable<T> query, ODataQuerySettings querySettings)
        {
            return ApplyToCore(query, querySettings) as IOrderedQueryable<T>;
        }

        /// <summary>
        /// Apply the $top query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The query settings to use while applying this query option.</param>
        /// <returns>The new <see cref="IQueryable"/> after the top query has been applied to.</returns>
        public IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings)
        {
            return ApplyToCore(query, querySettings);
        }

        /// <summary>
        /// Validate the top query based on the given <paramref name="validationSettings"/>. It throws an ODataException if validation failed.
        /// </summary>
        /// <param name="validationSettings">The <see cref="ODataValidationSettings"/> instance which contains all the validation settings.</param>
        public void Validate(ODataValidationSettings validationSettings)
        {
            if (validationSettings == null)
            {
                throw Error.ArgumentNull("validationSettings");
            }

            if (Validator != null)
            {
                Validator.Validate(this, validationSettings);
            }
        }

        private IQueryable ApplyToCore(IQueryable query, ODataQuerySettings querySettings)
        {
            if (Context.ElementClrType == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "ApplyTo");
            }

            return ExpressionHelpers.Take(query, Value, Context.ElementClrType, querySettings.EnableConstantParameterization);
        }
    }
}
