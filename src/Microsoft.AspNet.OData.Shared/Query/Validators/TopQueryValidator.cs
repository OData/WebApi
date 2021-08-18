//-----------------------------------------------------------------------------
// <copyright file="TopQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Query.Validators
{
    /// <summary>
    /// Represents a validator used to validate a <see cref="TopQueryOption"/> based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class TopQueryValidator
    {
        /// <summary>
        /// Validates a <see cref="TopQueryOption" />.
        /// </summary>
        /// <param name="topQueryOption">The $top query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        public virtual void Validate(TopQueryOption topQueryOption, ODataValidationSettings validationSettings)
        {
            if (topQueryOption == null)
            {
                throw Error.ArgumentNull("topQueryOption");
            }

            if (validationSettings == null)
            {
                throw Error.ArgumentNull("validationSettings");
            }

            if (topQueryOption.Value > validationSettings.MaxTop)
            {
                throw new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, validationSettings.MaxTop,
                    AllowedQueryOptions.Top, topQueryOption.Value));
            }

            int maxTop;
            IEdmProperty property = topQueryOption.Context.TargetProperty;
            IEdmStructuredType structuredType = topQueryOption.Context.TargetStructuredType;

            if (EdmLibHelpers.IsTopLimitExceeded(
                property,
                structuredType,
                topQueryOption.Context.Model,
                topQueryOption.Value, topQueryOption.Context.DefaultQuerySettings, 
                out maxTop))
            {
                throw new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, maxTop,
                    AllowedQueryOptions.Top, topQueryOption.Value));
            }
        }

        internal static TopQueryValidator GetTopQueryValidator(ODataQueryContext context)
        {
            if (context == null || context.RequestContainer == null)
            {
                return new TopQueryValidator();
            }

            return context.RequestContainer.GetRequiredService<TopQueryValidator>();
        }
    }
}
