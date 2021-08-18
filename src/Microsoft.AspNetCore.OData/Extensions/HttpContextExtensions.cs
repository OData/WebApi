//-----------------------------------------------------------------------------
// <copyright file="HttpContextExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Extensions
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

        /// <summary>
        /// Extension method to return the <see cref="IODataBatchFeature"/> from the <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="httpContext">The Http context.</param>
        /// <returns>The <see cref="IODataBatchFeature"/>.</returns>
        public static IODataBatchFeature ODataBatchFeature(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull("httpContext");
            }

            IODataBatchFeature odataBatchFeature = httpContext.Features.Get<IODataBatchFeature>();
            if (odataBatchFeature == null)
            {
                odataBatchFeature = new ODataBatchFeature();
                httpContext.Features.Set<IODataBatchFeature>(odataBatchFeature);
            }

            return odataBatchFeature;
        }

        /// <summary>
        /// Extension method to return the <see cref="IUrlHelper"/> from the <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="httpContext">The Http context.</param>
        /// <returns>The <see cref="IUrlHelper"/>.</returns>
        public static IUrlHelper GetUrlHelper(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull("httpContext");
            }

            // Get an IUrlHelper from the global service provider.
            ActionContext actionContext = httpContext.RequestServices.GetRequiredService<IActionContextAccessor>().ActionContext;
            return httpContext.RequestServices.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(actionContext);
        }

        /// <summary>
        /// Extension method to return the <see cref="IETagHandler"/> from the <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="httpContext">The Http context.</param>
        /// <returns>The <see cref="IETagHandler"/>.</returns>
        public static IETagHandler GetETagHandler(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull("httpContext");
            }

            // Get an IETagHandler from the global service provider.
            return httpContext.RequestServices.GetRequiredService<IETagHandler>();
        }
   }
}
