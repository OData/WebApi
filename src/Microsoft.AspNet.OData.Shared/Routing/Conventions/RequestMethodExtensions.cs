//-----------------------------------------------------------------------------
// <copyright file="RequestMethodExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Interfaces;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    internal static class RequestMethodExtensions
    {
        /// <summary>
        /// Returns the request method and in the case of Options request it returns the Access-Control-Request-Method present in the
        /// preflight request for the request method that will be used for the actual request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        internal static ODataRequestMethod GetRequestMethodOrPreflightMethod(this IWebApiRequestMessage request)
        {
            if (request.Method != ODataRequestMethod.Options)
            {
                return request.Method;
            }

            IEnumerable<string> values;
            if (!request.Headers.TryGetValues("Access-Control-Request-Method", out values))
            {
                return ODataRequestMethod.Unknown;
            }

            ODataRequestMethod preflightMethod;
            return Enum.TryParse<ODataRequestMethod>(values.FirstOrDefault(), true, out preflightMethod) ? preflightMethod : ODataRequestMethod.Unknown;
        }
    }
}
