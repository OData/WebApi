// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace System.Web.OData.Query.Validators
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
            if (EdmLibHelpers.IsTopLimitExceeded(
                null,
                topQueryOption.Context.ElementType as IEdmStructuredType,
                topQueryOption.Context.Model,
                topQueryOption.Value, topQueryOption.Context.DefaultQuerySettings, 
                out maxTop))
            {
                throw new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, maxTop,
                    AllowedQueryOptions.Top, topQueryOption.Value));
            }
        }
    }
}
