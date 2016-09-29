// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpContext"/>.
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Extension method to return the <see cref="IODataFeature"/> from the <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="httpContext">The Http context.</param>
        /// <returns>The <see cref="IODataFeature"/>.</returns>
        public static IODataFeature ODataFeature(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull("httpContext");
            }

            IODataFeature odataFeature = httpContext.Features.Get<IODataFeature>();
            if (odataFeature == null)
            {
                odataFeature = new ODataFeature();
                httpContext.Features.Set<IODataFeature>(odataFeature);
            }

            return odataFeature;
        }

        public static IUrlHelper UrlHelper(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull("httpContext");
            }
            var actionContext = new ActionContext {
                HttpContext = httpContext
            };
            return httpContext.RequestServices.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(actionContext);
        }

        public static IETagHandler ETagHandler(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull("httpContext");
            }

            return httpContext.RequestServices.GetRequiredService<IETagHandler>();
        }

        public static IODataPathHandler ODataPathHandler(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull("httpContext");
            }

            return httpContext.RequestServices.GetRequiredService<IODataPathHandler>();
        }

        public static IAssemblyProvider AssemblyProvider(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull("httpContext");
            }

            return httpContext.RequestServices.GetRequiredService<IAssemblyProvider>();
        }
    }
}