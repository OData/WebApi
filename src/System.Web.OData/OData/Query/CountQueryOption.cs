// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Core;

namespace System.Web.OData.Query
{
    /// <summary>
    /// Represents the value of the $count query option and exposes a way to retrieve the number of entities that satisfy a query.
    /// </summary>
    public class CountQueryOption
    {
        private bool? _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="CountQueryOption" /> class.
        /// </summary>
        /// <param name="rawValue">The raw value for the $count query option.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the query context.</param>
        public CountQueryOption(string rawValue, ODataQueryContext context)
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
                    // $count value is case-insensitive.
                    bool result;
                    if (Boolean.TryParse(RawValue, out result))
                    {
                        _value = result;
                    }
                    else
                    {
                        throw new ODataException(Error.Format(SRResources.InvalidCountOption, RawValue));
                    }
                }

                Contract.Assert(_value != null);
                return _value.Value;
            }
        }

        /// <summary>
        /// Gets the number of entities that satify the given query if the response should include a count query option, or <c>null</c> otherwise.
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
                return ExpressionHelpers.Count(query, Context.ElementClrType);
            }
            else
            {
                return null;
            }
        }
    }
}
