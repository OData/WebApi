//-----------------------------------------------------------------------------
// <copyright file="HttpConfigurationExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System.Web.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Test
{
    internal static class HttpConfigurationExtensions
    {
        public static void MapODataServiceRoute(this HttpConfiguration configuration, IEdmModel model)
        {
            configuration.MapODataServiceRoute("IgnoredRouteName", null, model);
        }
    }
}
#endif
