// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http.Dispatcher;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web.OData
{
    internal static class QueryContextHelpers
    {
        public static IAssembliesResolver GetAssembliesResolver(ODataQueryContext context)
        {
            if (context.RequestContainer != null)
            {
                return context.RequestContainer.GetRequiredService<IAssembliesResolver>();
            }

            // Some test cases may not set the request container.
            return new DefaultAssembliesResolver();
        }
    }
}
