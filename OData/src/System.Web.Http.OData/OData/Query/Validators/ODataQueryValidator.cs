// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Query.Validators
{
    /// <summary>
    /// Represents a validator used to validate OData queries based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class ODataQueryValidator
    {
        /// <summary>
        /// Validates the OData query.
        /// </summary>
        /// <param name="options">The OData query to validate.</param>
        /// <param name="validationSettings">The validation settings.</param>
        public virtual void Validate(ODataQueryOptions options, ODataValidationSettings validationSettings)
        {
            if (options == null)
            {
                throw Error.ArgumentNull("options");
            }

            if (validationSettings == null)
            {
                throw Error.ArgumentNull("validationSettings");
            }

            // Validate each query options
            if (options.Skip != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.Skip, validationSettings.AllowedQueryOptions);
                options.Skip.Validate(validationSettings);
            }

            if (options.Top != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.Top, validationSettings.AllowedQueryOptions);
                options.Top.Validate(validationSettings);
            }

            if (options.OrderBy != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.OrderBy, validationSettings.AllowedQueryOptions);
                options.OrderBy.Validate(validationSettings);
            }

            if (options.Filter != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.Filter, validationSettings.AllowedQueryOptions);
                options.Filter.Validate(validationSettings);
            }

            if (options.InlineCount != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.InlineCount, validationSettings.AllowedQueryOptions);
            }

            if (options.RawValues.Expand != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.Expand, validationSettings.AllowedQueryOptions);
            }

            if (options.RawValues.Select != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.Select, validationSettings.AllowedQueryOptions);
            }

            if (options.SelectExpand != null)
            {
                options.SelectExpand.Validate(validationSettings);
            }

            if (options.RawValues.Format != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.Format, validationSettings.AllowedQueryOptions);
            }

            if (options.RawValues.SkipToken != null)
            {
                ValidateQueryOptionAllowed(AllowedQueryOptions.SkipToken, validationSettings.AllowedQueryOptions);
            }
        }

        private static void ValidateQueryOptionAllowed(AllowedQueryOptions queryOption, AllowedQueryOptions allowed)
        {
            if ((queryOption & allowed) == AllowedQueryOptions.None)
            {
                throw new ODataException(Error.Format(SRResources.NotAllowedQueryOption, queryOption, "AllowedQueryOptions"));
            }
        }
    }
}
