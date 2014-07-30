// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Query.Validators
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
