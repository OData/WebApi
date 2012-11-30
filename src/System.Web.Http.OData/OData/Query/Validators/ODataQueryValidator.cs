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
                if ((validationSettings.AllowedQueryOptions & AllowedQueryOptions.Skip) == AllowedQueryOptions.None)
                {
                    throw new ODataException(Error.Format(SRResources.NotAllowedQueryOption, AllowedQueryOptions.Skip, "AllowedQueryOptions"));
                }

                options.Skip.Validate(validationSettings);
            }

            if (options.Top != null)
            {
                if ((validationSettings.AllowedQueryOptions & AllowedQueryOptions.Top) == AllowedQueryOptions.None)
                {
                    throw new ODataException(Error.Format(SRResources.NotAllowedQueryOption, AllowedQueryOptions.Top, "AllowedQueryOptions"));
                }

                options.Top.Validate(validationSettings);
            }

            if (options.OrderBy != null)
            {
                if ((validationSettings.AllowedQueryOptions & AllowedQueryOptions.OrderBy) == AllowedQueryOptions.None)
                {
                    throw new ODataException(Error.Format(SRResources.NotAllowedQueryOption, AllowedQueryOptions.OrderBy, "AllowedQueryOptions"));
                }

                options.OrderBy.Validate(validationSettings);
            }

            if (options.Filter != null)
            {
                if ((validationSettings.AllowedQueryOptions & AllowedQueryOptions.Filter) == AllowedQueryOptions.None)
                {
                    throw new ODataException(Error.Format(SRResources.NotAllowedQueryOption, AllowedQueryOptions.Filter, "AllowedQueryOptions"));
                }

                options.Filter.Validate(validationSettings);
            }
        }
    }
}
