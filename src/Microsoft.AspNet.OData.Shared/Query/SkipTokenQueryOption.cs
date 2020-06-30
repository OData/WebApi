// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// This defines a $skiptoken OData query option for querying.
    /// </summary>
    public class SkipTokenQueryOption
    {
        /// <summary>
        /// Generates the nextlink value and consumes the skiptoken value.
        /// </summary>
        private SkipTokenHandler skipTokenHandler;

        /// <summary>
        /// Initialize a new instance of <see cref="SkipQueryOption"/> based on the raw $skiptoken value and
        /// an EdmModel from <see cref="ODataQueryContext"/>.
        /// </summary>
        /// <param name="rawValue">The raw value for $skiptoken query.</param>
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
            Validator = context.GetSkipTokenQueryValidator();
            skipTokenHandler = context.GetSkipTokenHandler();
            Context = context;
        }

        /// <summary>
        /// Gets the raw $skiptoken value.
        /// </summary>
        public string RawValue { get; private set; }

        /// <summary>
        /// Gets and sets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; private set; }

        /// <summary>
        /// Gets or sets the SkipToken Query Validator.
        /// </summary>
        public SkipTokenQueryValidator Validator { get; }

        /// <summary>
        /// Gets or sets the query setting 
        /// </summary>
        public ODataQuerySettings QuerySettings { get; private set; }

        /// <summary>
        /// Gets or sets the QueryOptions
        /// </summary>
        public IODataQueryOptions QueryOptions { get; private set; }

        /// <summary>
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The query settings to use while applying this query option.</param>
        /// <param name="queryOptions">Information about the other query options.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skiptoken query has been applied to.</returns>
        public virtual IQueryable<T> ApplyTo<T>(IQueryable<T> query, ODataQuerySettings querySettings, IODataQueryOptions queryOptions)
        {
            QuerySettings = querySettings;
            QueryOptions = queryOptions;
            return skipTokenHandler.ApplyTo<T>(query, this) as IOrderedQueryable<T>;
        }

        /// <summary>
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The query settings to use while applying this query option.</param>
        /// <param name="queryOptions">Information about the other query options.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skiptoken query has been applied to.</returns>
        public virtual IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings, IODataQueryOptions queryOptions)
        {
            QuerySettings = querySettings;
            QueryOptions = queryOptions;
            return skipTokenHandler.ApplyTo(query, this);
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
    }
}