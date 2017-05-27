// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData.Core;

namespace Microsoft.AspNetCore.OData.Query.Validators
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
                throw new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, validationSettings.MaxTop, AllowedQueryOptions.Top, topQueryOption.Value));
            }
        }
    }
}
