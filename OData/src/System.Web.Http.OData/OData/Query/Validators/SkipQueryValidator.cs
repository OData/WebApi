// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Query.Validators
{
    /// <summary>
    /// Represents a validator used to validate a <see cref="SkipQueryOption"/> based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class SkipQueryValidator
    {
        /// <summary>
        /// Validates a <see cref="SkipQueryOption" />.
        /// </summary>
        /// <param name="skipQueryOption">The $skip query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        public virtual void Validate(SkipQueryOption skipQueryOption, ODataValidationSettings validationSettings)
        {
            if (skipQueryOption == null)
            {
                throw Error.ArgumentNull("skipQueryOption");
            }

            if (validationSettings == null)
            {
                throw Error.ArgumentNull("validationSettings");
            }

            if (skipQueryOption.Value > validationSettings.MaxSkip)
            {
                throw new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, validationSettings.MaxSkip, AllowedQueryOptions.Skip, skipQueryOption.Value));
            }
        }
    }
}
