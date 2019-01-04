// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// This defines a $skiptoken OData query option for querying.
    /// </summary>
    public class SkipTokenQueryOption
    {
        private string _value;
        private ODataQueryOptionParser _queryOptionParser;

        /// <summary>
        /// Initialize a new instance of <see cref="SkipQueryOption"/> based on the raw $skip value and
        /// an EdmModel from <see cref="ODataQueryContext"/>.
        /// </summary>
        /// <param name="rawValue">The raw value for $skiptoken query. It can be null or empty.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        /// <param name="queryOptionParser">The <see cref="ODataQueryOptionParser"/> which is used to parse the query option.</param>
        public SkipTokenQueryOption(string rawValue, ODataQueryContext context, ODataQueryOptionParser queryOptionParser)
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

            RawValue = rawValue;
            Validator = SkipTokenQueryValidator.GetSkipTokenQueryValidator(context);
            SkipTokenHandler = GetSkipTokenHandler(context);
            Context = context;
            _queryOptionParser = queryOptionParser;
        }

        // This constructor is intended for unit testing only.
        internal SkipTokenQueryOption(string rawValue, ODataQueryContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (String.IsNullOrEmpty(rawValue))
            {
                throw Error.ArgumentNullOrEmpty("rawValue");
            }

            RawValue = rawValue;
            Validator = SkipTokenQueryValidator.GetSkipTokenQueryValidator(context);
            _queryOptionParser = new ODataQueryOptionParser(
                context.Model,
                context.ElementType,
                context.NavigationSource,
                new Dictionary<string, string> { { "$skiptoken", rawValue } },
                context.RequestContainer);
        }

        /// <summary>
        /// Gets the raw $skiptoken value.
        /// </summary>
        public string RawValue { get; private set; }

        /// <summary>
        /// Gets and sets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; set; }

        /// <summary>
        /// Abstracts the implementation of SkipToken.
        /// </summary>
        public SkipTokenHandler SkipTokenHandler { get; set; }

        /// <summary>
        /// Gets the value of the $skiptoken.
        /// </summary>
        public string Value
        {
            get
            {
                if (_value == null)
                {
                    string skipValue = _queryOptionParser.ParseSkipToken();
                    _value = skipValue;
                }
                return _value;
            }
        }

        /// <summary>
        /// Gets or sets the SkipToken Query Validator.
        /// </summary>
        public SkipTokenQueryValidator Validator { get; set; }

        /// <summary>
        /// Gets or sets the query setting 
        /// </summary>
        public ODataQuerySettings QuerySettings { get; private set; }

        /// <summary>
        /// Gets or sets the list of OrderByNodes for the original query
        /// </summary>
        public IList<OrderByNode> OrderByNodes { get; private set; }

        /// <summary>
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The query settings to use while applying this query option.</param>
        /// <param name="orderByNodes">Information about the orderby query option.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skiptoken query has been applied to.</returns>
        public IQueryable<T> ApplyTo<T>(IQueryable<T> query, ODataQuerySettings querySettings, IList<OrderByNode> orderByNodes)
        {
            QuerySettings = querySettings;
            OrderByNodes = orderByNodes;
            return SkipTokenHandler.ApplyTo<T>(query, this) as IOrderedQueryable<T>;
        }

        /// <summary>
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The query settings to use while applying this query option.</param>
        /// <param name="orderByNodes">Information about the orderby query option.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skiptoken query has been applied to.</returns>
        public IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings, IList<OrderByNode> orderByNodes)
        {
            QuerySettings = querySettings;
            OrderByNodes = orderByNodes;
            return SkipTokenHandler.ApplyTo(query, this);
        }

        /// <summary>
        /// Validate the skiptoken query based on the given <paramref name="validationSettings"/>. It throws an ODataException if validation failed.
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

        /// <summary>
        /// Returns the handler for skip token
        /// </summary>
        /// <param name="context">The <see cref="ODataQueryContext"/> instance of the query.</param>
        /// <returns></returns>
        public static SkipTokenHandler GetSkipTokenHandler(ODataQueryContext context)
        {
            if (context == null || context.RequestContainer == null)
            {
                return new DefaultSkipTokenHandler();
            }
            return context.RequestContainer.GetRequiredService<SkipTokenHandler>();
        }
    }
}