// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Extensions;

namespace Microsoft.AspNet.OData
{
    internal static partial class ODataQueryContextExtensions
    {
        public static IWebApiAssembliesResolver GetAssembliesResolver(this ODataQueryContext context)
        {
            return context.RequestContainer.GetWebApiAssembliesResolver();
        }
    }
}
