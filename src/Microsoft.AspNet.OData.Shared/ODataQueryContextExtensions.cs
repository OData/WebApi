//-----------------------------------------------------------------------------
// <copyright file="ODataQueryContextExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData
{
    internal static class ODataQueryContextExtensions
    {
        public static ODataQuerySettings UpdateQuerySettings(this ODataQueryContext context, ODataQuerySettings querySettings, IQueryable query)
        {
            ODataQuerySettings updatedSettings = (context == null || context.RequestContainer == null)
                ? new ODataQuerySettings()
                : context.RequestContainer.GetRequiredService<ODataQuerySettings>();

            updatedSettings.CopyFrom(querySettings);

            if (updatedSettings.HandleNullPropagation == HandleNullPropagationOption.Default)
            {
                updatedSettings.HandleNullPropagation = query != null
                    ? HandleNullPropagationOptionHelper.GetDefaultHandleNullPropagationOption(query)
                    : HandleNullPropagationOption.True;
            }

            return updatedSettings;
        }

        public static SkipTokenHandler GetSkipTokenHandler(this ODataQueryContext context)
        {
            if (context == null || context.RequestContainer == null)
            {
                return DefaultSkipTokenHandler.Instance;
            }

            return context.RequestContainer.GetRequiredService<SkipTokenHandler>();
        }

        public static SkipTokenQueryValidator GetSkipTokenQueryValidator(this ODataQueryContext context)
        {
            if (context == null || context.RequestContainer == null)
            {
                return new SkipTokenQueryValidator();
            }

            return context.RequestContainer.GetRequiredService<SkipTokenQueryValidator>();
        }
    }
}
