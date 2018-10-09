// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.AspNet.OData.Query.Validators;
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
        /// <param name="rawValue">The raw value for $skip query. It can be null or empty.</param>
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

            Context = context;
            RawValue = rawValue;
            ParseKeyValuePairs(rawValue);
            Validator = SkipTokenQueryValidator.GetSkipTokenQueryValidator(context);
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

            Context = context;
            RawValue = rawValue;
            Validator = SkipTokenQueryValidator.GetSkipTokenQueryValidator(context);
            _queryOptionParser = new ODataQueryOptionParser(
                context.Model,
                context.ElementType,
                context.NavigationSource,
                new Dictionary<string, string> { { "$skiptoken", rawValue } });
        }

        private void ParseKeyValuePairs(string rawValue)
        {
            KeyValuePairs = new Dictionary<string, object>();
            string[] keyValues = rawValue.Split(',');
            foreach(string keyAndValue in keyValues)
            {
                string[] pieces = keyAndValue.Split('=');
                object value = ODataUriUtils.ConvertFromUriLiteral(pieces[1], ODataVersion.V401);
                KeyValuePairs.Add(pieces[0], value);
            }

        }

        /// <summary>
        /// Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; private set; }

        /// <summary>
        /// Stores the key value pairs for the skiptoken query option
        /// </summary>
        public IDictionary<string, object> KeyValuePairs;

        /// <summary>
        /// Gets the raw $skiptoken value.
        /// </summary>
        public string RawValue { get; private set; }

        /// <summary>
        /// Gets the value of the $skiptoken as a parsed integer.
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
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The query settings to use while applying this query option.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skip query has been applied to.</returns>
        public IQueryable<T> ApplyTo<T>(IQueryable<T> query, ODataQuerySettings querySettings)
        {
            return ApplyToCore(query, querySettings) as IOrderedQueryable<T>;
        }

        /// <summary>
        /// Apply the $skip query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The query settings to use while applying this query option.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skip query has been applied to.</returns>
        public IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings)
        {
            return ApplyToCore(query, querySettings);
        }

        /// <summary>
        /// Validate the skip query based on the given <paramref name="validationSettings"/>. It throws an ODataException if validation failed.
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

            bool parameterizeConstant = querySettings.EnableConstantParameterization;
            ParameterExpression param = Expression.Parameter(Context.ElementClrType);
            Expression where = null;
            int count = 0;

            foreach (KeyValuePair<string,object> item in KeyValuePairs)
            {
                MemberExpression property = Expression.Property(param, item.Key);
                object value = item.Value;
                Expression constant = parameterizeConstant ? LinqParameterContainer.Parameterize(value.GetType(), value) : Expression.Constant(value);
                BinaryExpression compare = (count == KeyValuePairs.Keys.Count - 1) ? BinaryExpression.GreaterThan(property, constant) : BinaryExpression.GreaterThanOrEqual(property, constant);
                where = where == null ? compare : Expression.AndAlso(where, compare);

                count++;
            }

            Expression whereLambda = Expression.Lambda(where, param);

            return ExpressionHelpers.Where(query, whereLambda, query.ElementType);
            //return ExpressionHelpers.SkipWhile<string>(query, v, query.ElementType, querySettings.EnableConstantParameterization);
        }
    }
}

