//-----------------------------------------------------------------------------
// <copyright file="CountQueryOption.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Represents the value of the $count query option and exposes a way to retrieve the number of entities that satisfy a query.
    /// </summary>
    public class CountQueryOption
    {
        private bool? _value;
        private ODataQueryOptionParser _queryOptionParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="CountQueryOption" /> class.
        /// </summary>
        /// <param name="rawValue">The raw value for the $count query option.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the query context.</param>
        /// <param name="queryOptionParser">The <see cref="ODataQueryOptionParser"/> which is used to parse the query option.</param>
        public CountQueryOption(string rawValue, ODataQueryContext context, ODataQueryOptionParser queryOptionParser)
        {
            if (String.IsNullOrEmpty(rawValue))
            {
                throw Error.ArgumentNullOrEmpty("rawValue");
            }

            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (queryOptionParser == null)
            {
                throw Error.ArgumentNull("queryOptionParser");
            }

            Context = context;
            RawValue = rawValue;
            Validator = CountQueryValidator.GetCountQueryValidator(context);
            _queryOptionParser = queryOptionParser;
        }

        // This constructor is intended for unit testing only.
        internal CountQueryOption(string rawValue, ODataQueryContext context)
        {
            if (String.IsNullOrEmpty(rawValue))
            {
                throw Error.ArgumentNullOrEmpty("rawValue");
            }

            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            Context = context;
            RawValue = rawValue;
            Validator = CountQueryValidator.GetCountQueryValidator(context);
            _queryOptionParser = new ODataQueryOptionParser(
                context.Model,
                context.ElementType,
                context.NavigationSource,
                new Dictionary<string, string> { { "$count", rawValue } },
                context.RequestContainer);
        }

        /// <summary>
        /// Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; private set; }

        /// <summary>
        /// Gets the raw $count value.
        /// </summary>
        public string RawValue { get; private set; }

        /// <summary>
        /// Gets the value of the $count in a parsed form.
        /// </summary>
        public bool Value
        {
            get
            {
                if (_value == null)
                {
                    _value = _queryOptionParser.ParseCount();
                }

                Contract.Assert(_value.HasValue);
                return _value.Value;
            }
        }

        /// <summary>
        /// Gets or sets the $count query validator.
        /// </summary>
        public CountQueryValidator Validator { get; set; }

        /// <summary>
        /// Validate the count query based on the given <paramref name="validationSettings"/>.
        /// It throws an ODataException if validation failed.
        /// </summary>
        /// <param name="validationSettings">The <see cref="ODataValidationSettings"/> instance
        /// which contains all the validation settings.</param>
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
        /// Gets the number of entities that satisfy the given query if the response should include a count query option, or <c>null</c> otherwise.
        /// </summary>
        /// <param name="query">The query to compute the count for.</param>
        /// <returns>The number of entities that satisfy the specified query if the response should include a count query option, or <c>null</c> otherwise.</returns>
        public long? GetEntityCount(IQueryable query)
        {
            if (Context.ElementClrType == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "GetEntityCount");
            }

            if (Value)
            {
                return ExpressionHelpers.Count(query, Context.ElementClrType)();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the Func of entities number that satisfy the given query if the response should include a count query option, or <c>null</c> otherwise.
        /// </summary>
        /// <param name="query">The query to compute the count for.</param>
        /// <returns>The the Func of entities number that satisfy the specified query if the response should include a count query option, or <c>null</c> otherwise.</returns>
        internal Func<long> GetEntityCountFunc(IQueryable query)
        {
            if (Context.ElementClrType == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "GetEntityCount");
            }

            if (Value)
            {
                return ExpressionHelpers.Count(query, Context.ElementClrType);
            }
            else
            {
                return null;
            }
        }
    }
}
