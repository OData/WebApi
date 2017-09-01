// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData
{
    internal static partial class ODataQueryContextExtensions
    {
        public static IWebApiAssembliesResolver GetAssembliesResolver(this ODataQueryContext context)
        {
            IAssembliesResolver resolver = context.RequestContainer.GetRequiredService<IAssembliesResolver>();
            return new WebApiAssembliesResolver(resolver);
        }
    }
}
