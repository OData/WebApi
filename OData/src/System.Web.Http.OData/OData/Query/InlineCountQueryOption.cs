// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Query
{
    /// <summary>
    /// Represents the value of the $inlinecount query option and exposes a way to retrieve the number of entities that satisfy a query.
    /// </summary>
    public class InlineCountQueryOption
    {
        private InlineCountValue? _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineCountQueryOption" /> class.
        /// </summary>
        /// <param name="rawValue">The raw value for the $inlinecount query option.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        public InlineCountQueryOption(string rawValue, ODataQueryContext context)
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
        /// Gets the raw $inlinecount value.
        /// </summary>
        public string RawValue { get; private set; }

        /// <summary>
        /// Gets the value of the $inlinecount in a parsed form.
        /// </summary>
        public InlineCountValue Value
        {
            get
            {
                if (_value == null)
                {
                    if (RawValue.Equals("none", StringComparison.OrdinalIgnoreCase))
                    {
                        _value = InlineCountValue.None;
                    }
                    else if (RawValue.Equals("allpages", StringComparison.OrdinalIgnoreCase))
                    {
                        _value = InlineCountValue.AllPages;
                    }
                    else
                    {
                        throw new ODataException(Error.Format(SRResources.InvalidInlineCount, RawValue));
                    }
                }
                Contract.Assert(_value.HasValue);
                return _value.Value;
            }
        }

        /// <summary>
        /// Gets the number of entities that satify the given query if the response should include an inline count, or <c>null</c> otherwise.
        /// </summary>
        /// <param name="query">The query to compute the count for.</param>
        /// <returns>The number of entities that satisfy the specified query if the response should include an inline count, or <c>null</c> otherwise.</returns>
        public long? GetEntityCount(IQueryable query)
        {
            if (Context.ElementClrType == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "GetEntityCount");
            }

            if (Value == InlineCountValue.AllPages)
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
