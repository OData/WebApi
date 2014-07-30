// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Query.Validators;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Query
{
    /// <summary>
    /// This defines a $skip OData query option for querying.
    /// </summary>
    public class SkipQueryOption
    {
        private int? _value;

        /// <summary>
        /// Initialize a new instance of <see cref="SkipQueryOption"/> based on the raw $skip value and 
        /// an EdmModel from <see cref="ODataQueryContext"/>.
        /// </summary>
        /// <param name="rawValue">The raw value for $skip query. It can be null or empty.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        public SkipQueryOption(string rawValue, ODataQueryContext context)
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
            Validator = new SkipQueryValidator();
        }

        /// <summary>
        /// Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>        
        public ODataQueryContext Context { get; private set; }

        /// <summary>
        /// Gets the raw $skip value.
        /// </summary>
        public string RawValue { get; private set; }

        /// <summary>
        /// Gets the value of the $skip as a parsed integer.
        /// </summary>
        public int Value
        {
            get
            {
                if (_value == null)
                {
                    try
                    {
                        _value = Convert.ToInt32(RawValue, CultureInfo.InvariantCulture);
                    }
                    catch (FormatException exception)
                    {
                        throw new ODataException(Error.Format(SRResources.CanNotParseInteger, RawValue), exception);
                    }
                }
                Contract.Assert(_value.HasValue);
                return _value.Value;
            }
        }

        /// <summary>
        /// Gets or sets the Skip Query Validator.
        /// </summary>
        public SkipQueryValidator Validator { get; set; }

        /// <summary>
        /// Apply the $skip query to the given IQueryable.
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

            return ExpressionHelpers.Skip(query, Value, Context.ElementClrType, querySettings.EnableConstantParameterization);
        }
    }
}
