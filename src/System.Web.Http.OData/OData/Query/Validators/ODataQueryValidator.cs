// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Query.Validators
{
    public class ODataQueryValidator
    {
        /// <summary>
        /// Validate if the given ODataQueryOption follows what is in the AllowedQueryOptions. By default, 
        /// we allow all four operators.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="validationSettings"></param>
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

            // Filter and OrderBy require entity sets.  Top and Skip may accept primitives.
            if (options.Context.IsPrimitiveClrType && (options.Filter != null || options.OrderBy != null))
            {
                // An attempt to use a query option not allowed for primitive types
                // generates a BadRequest with a general message that avoids information disclosure.
                throw new ODataException(SRResources.OnlySkipAndTopSupported);
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
